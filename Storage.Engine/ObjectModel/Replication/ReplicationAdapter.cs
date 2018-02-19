using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Storage.Lib;

namespace Storage.Engine
{
    /// <summary>
    /// 
    /// </summary>
    internal class ReplicationAdapter : IReplicationAdapter
    {
        private static object _lockByFolderLocker = new object();
        private static Dictionary<int, object> _lockByFolder = new Dictionary<int, object>();

        /// <summary>
        /// 
        /// </summary>
        internal StorageEngine Engine { get; private set; }

        public ReplicationAdapter(StorageEngine engine)
        {
            if (engine == null)
                throw new ArgumentNullException("engine");

            this.Engine = engine;
        }


        /// <summary>
        /// 
        /// </summary>
        public IStorageNode CurrentNode
        {
            get { return this.Engine.CurrentNode; }
        }

        private bool __init_ReplicationObserver;
        private ReplicationSchemaObserver _ReplicationObserver;
        /// <summary>
        /// 
        /// </summary>
        private ReplicationSchemaObserver ReplicationObserver
        {
            get
            {
                if (!__init_ReplicationObserver)
                {
                    _ReplicationObserver = new ReplicationSchemaObserver(this, this.Engine);
                    __init_ReplicationObserver = true;
                }
                return _ReplicationObserver;
            }
        }

        /// <summary>
        /// Файловое хранилище.
        /// </summary>
        private IStorage Storage { get { return this.Engine; } }

        /// <summary>
        /// Логгер.
        /// </summary>
        private ILogProvider Logger { get { return this.Engine.Logger; } }

        private bool __init_Transport;
        private IReplicationTransport _Transport;
        /// <summary>
        /// Транспорт.
        /// </summary>
        public IReplicationTransport Transport
        {
            get
            {
                if (!__init_Transport)
                {
                    _Transport = ConfigFactory.Instance.Create<IReplicationTransport>(this);
                    __init_Transport = true;
                }
                return _Transport;
            }
        }

        #region IReplicationAdapter Members

        /// <summary>
        /// Обновляет знания о существующей схеме репликации.
        /// </summary>
        /// <param name="remoteSchema">Схема репликации узла, с которого пришел запрос.</param>
        public IReplicationSchema UpdateReplicationSchema(IReplicationSchema remoteSchema)
        {
            //обновление локальной схемы
            this.ReplicationObserver.UpdateSchema(remoteSchema);

            //возврат текущей схемы
            IReplicationSchema currentSchema = this.ReplicationObserver.GetCurrentSchema();
            return currentSchema;
        }

        /// <summary>
        /// Возвращает идентификаторы версий файлов для репликации.
        /// </summary>
        /// <param name="requestStorageID">Идентификатор узла, запрашивающего файлы для репликации.</param>
        /// <param name="folderUrl">Адрес папки.</param>
        /// <param name="from">Дата, с которой необходимо забрать файлы с узла источника.</param>
        /// <returns></returns>
        public Tuple<Guid, Guid>[] GetReplicationFiles(Guid requestStorageID, string folderUrl, DateTime @from)
        {
            if (requestStorageID == Guid.Empty)
                throw new ArgumentNullException("requestStorageID");

            if (string.IsNullOrEmpty(folderUrl))
                throw new ArgumentNullException("folderUrl");

            StorageNode requestStorage = this.ReplicationObserver.GetStorage(requestStorageID);
            if (requestStorage == null)
                throw new Exception(string.Format("Не удалось найти узел хранилища с идентификатором {0}",
                    requestStorageID));

            ReplicationFolder replicationFolder = this.ReplicationObserver.GetReplicationFolder(requestStorageID, folderUrl);
            if (replicationFolder == null)
                throw new Exception(string.Format("Не удалось найти папку репликации с адресом {0} для узла {1}",
                    folderUrl,
                    requestStorage.Host));

            Folder typedFolder = (Folder)replicationFolder.Folder;

            Tuple<Guid, Guid>[] files = this.Engine.MetadataAdapter.GetReplicationFiles(
                requestStorage.Metadata,
                typedFolder.Metadata,
                from);

            return files;

        }

        internal void ProcessRemoteFiles(ReplicationFolder replicationFolder, Tuple<Guid, Guid>[] remoteFiles)
        {
            if (replicationFolder == null)
                throw new ArgumentNullException("replicationFolder");

            //файлов может не быть
            if (remoteFiles == null || remoteFiles.Length == 0)
                return;

            var sourceStorage = replicationFolder.SourceStorage;
            if (sourceStorage.IsCurrent)
                throw new Exception(string.Format("Невозможно реплицировать файлы с узла источника, который является текущим."));

            //блокировка потоков обработки по папке репликации
            //в данный метод могут поступить ответы с 2х удаленных узлов
            //с пересекающейся коллекций файлов, поэтому лок по папке
            lock (this.GetFolderLock(replicationFolder.TypedFolder.ID))
            {
                this.ProcessRemoteFilesInternal(replicationFolder, remoteFiles);
            }
        }
        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            this.Transport.Dispose();
        }

        #endregion

        internal void Init()
        {
            this.ReplicationObserver.Init();
        }

        private void ProcessRemoteFilesInternal(ReplicationFolder replicationFolder, Tuple<Guid, Guid>[] remoteFiles)
        {
            if (replicationFolder == null)
                throw new ArgumentNullException("replicationFolder");

            if (remoteFiles == null || remoteFiles.Length == 0)
                throw new ArgumentNullException("remoteFiles");

            //словарь существующих файлов.
            Dictionary<Guid, File> existsFiles = new Dictionary<Guid, File>();

            Dictionary<string, byte> uniqueFileVersions = new Dictionary<string, byte>();

            //формируем словарь существующих файлов и списков файлов,
            //которых не существует и которые необходимо реплицировать.
            List<RemoteFileInfo> filesToDownload = new List<RemoteFileInfo>();
            foreach (Tuple<Guid, Guid> fileVersionInfo in remoteFiles)
            {
                Guid fileID = fileVersionInfo.Item1;
                Guid fileVersionID = fileVersionInfo.Item2;
                bool fileVersionExists = false;

                string key = string.Format("{0}_{1}",
                    fileID,
                    fileVersionID);

                if (uniqueFileVersions.ContainsKey(key))
                    continue;

                uniqueFileVersions.Add(key, 0);

                File file = null;
                try
                {
                    file = this.Engine.GetFileInternal(replicationFolder.Folder.Url, fileID, null, false);
                }
                catch (Exception fileEx)
                {
                    //таблицы с файлов может не быть.
                    this.Logger.WriteMessage(fileEx.ToString(), LogLevel.Error);
                }

                if (file != null)
                {
                    fileVersionExists = file.VersionUniqueID == fileVersionID || file.GetVersion(fileVersionID, false) != null;

                    if (!existsFiles.ContainsKey(file.UniqueID))
                        existsFiles.Add(file.UniqueID, file);
                }

                if (!fileVersionExists)
                    filesToDownload.Add(new RemoteFileInfo(fileVersionInfo, replicationFolder.Folder));
            }

            if (filesToDownload.Count == 0)
                return;

            DateTime? lastSync = null;
            try
            {
                foreach (RemoteFileInfo remoteFileInfo in filesToDownload)
                {
                    IRemoteFile remoteFile = null;

                    try
                    {
                        remoteFile = this.Transport.GetReplicationFile(
                            replicationFolder.SourceStorage,
                            remoteFileInfo.Folder.Url,
                            remoteFileInfo.UniqueID,
                            remoteFileInfo.VersionID);

                        if (remoteFile == null)
                            throw new Exception(string.Format("Не удалось получить файла с идентификатором {0} с узла {1}",
                                remoteFileInfo.UniqueID,
                                replicationFolder.SourceStorage.Host));

                        StorageNode typedNode = (StorageNode)replicationFolder.SourceStorage;
                        remoteFile.CreatedStorageNode = typedNode.Metadata;

                        var stream = remoteFile.Stream;
                        if (stream == null)
                            throw new Exception(string.Format("Не удалось получить поток файла с идентификатором {0} с узла {1}",
                                remoteFileInfo.UniqueID,
                                replicationFolder.SourceStorage.Host));

                        File localFile = null;
                        if (existsFiles.ContainsKey(remoteFile.UniqueID))
                            localFile = existsFiles[remoteFile.UniqueID];

                        if (localFile == null)
                        {
                            //загружаем новый файл
                            IFileVersionMetadata fileVersion = this.Engine.DataAdapter.WriteRemoteFile(replicationFolder.TypedFolder.Metadata, remoteFile);
                            localFile = new File(this.Engine, replicationFolder.Folder, fileVersion.FileMetadata);

                            //добавляем в коллекцию существующих на текущем узле
                            existsFiles.Add(localFile.UniqueID, localFile);
                        }
                        else
                        {
                            //файл уже существует, добавляем только новую версию файла
                            this.Engine.DataAdapter.WriteRemoteFileVersion(localFile.Metadata, remoteFile);
                        }

                        //обновляем дату синхронизации по дате создания версии
                        lastSync = remoteFile.TimeCreated;
                    }
                    catch (Exception remoteFileEx)
                    {
                        throw new Exception(string.Format("Ошибка при репликации файла {0} с узла {1}",
                            remoteFileInfo.UniqueID,
                            replicationFolder.SourceStorage.Host), remoteFileEx);
                    }
                    finally
                    {
                        try
                        {
                            if (remoteFile != null && remoteFile.Stream != null)
                                remoteFile.Stream.Dispose();
                        }
                        catch (Exception innerEx)
                        {
                            this.Logger.WriteMessage(innerEx.ToString(), LogLevel.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.Logger.WriteMessage(ex.ToString(), LogLevel.Error);
            }

            if (lastSync.HasValue)
            {
                replicationFolder.Metadata.LastSyncTime = lastSync.Value;
                replicationFolder.Update();
            }
        }

        private object GetFolderLock(int folderID)
        {
            object locker;
            if (!_lockByFolder.TryGetValue(folderID, out locker))
            {
                lock (_lockByFolderLocker)
                {
                    if (!_lockByFolder.TryGetValue(folderID, out locker))
                    {
                        _lockByFolder.Add(folderID, locker = new object());
                    }
                }
            }
            return locker;
        }
    }
}