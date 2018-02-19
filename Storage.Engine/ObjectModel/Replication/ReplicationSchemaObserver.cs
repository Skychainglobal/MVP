using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Storage.Lib;

namespace Storage.Engine
{
    internal class ReplicationSchemaObserver
    {
        static object _commonLocker = new object();
        static object _addStorageLocker = new object();
        static object _checkReplicationFolderLocker = new object();
        static object _ensureReplicationFolderLocker = new object();
        static object _addFilesThreadLocker = new object();
        static object _updateFolderLocker = new object();
        static object _updateStorageLocker = new object();

        static Dictionary<Guid, StorageNode> _storageNodesByID = new Dictionary<Guid, StorageNode>();

        static Dictionary<Guid, Dictionary<string, ReplicationFolder>> _replicationFolders = new Dictionary<Guid, Dictionary<string, ReplicationFolder>>();

        static Dictionary<string, Thread> _observerThread = new Dictionary<string, Thread>();

        static Dictionary<string, Thread> _filesThread = new Dictionary<string, Thread>();

        static Dictionary<Guid, Thread> _relationThread = new Dictionary<Guid, Thread>();

        public ReplicationSchemaObserver(ReplicationAdapter replicationAdapter, StorageEngine engine)
        {
            if (replicationAdapter == null)
                throw new ArgumentNullException("replicationAdapter");

            if (engine == null)
                throw new ArgumentNullException("engine");

            this.ReplicationAdapter = replicationAdapter;
            this.Engine = engine;
        }

        public ReplicationAdapter ReplicationAdapter { get; private set; }

        public StorageEngine Engine { get; private set; }

        private bool __init_ReplicationConfiguration;
        private ReplicationConfiguration _ReplicationConfiguration;
        /// <summary>
        /// Настройка из конфига.
        /// </summary>
        private ReplicationConfiguration ReplicationConfiguration
        {
            get
            {
                if (!__init_ReplicationConfiguration)
                {
                    _ReplicationConfiguration = new ReplicationConfiguration();
                    __init_ReplicationConfiguration = true;
                }
                return _ReplicationConfiguration;
            }
        }

        private ReplicationFolder EnsureReplicationFolder(StorageNode sourceNode, string folderUrl, bool isCurrentSettings, out bool justCreated)
        {
            if (sourceNode == null)
                throw new ArgumentNullException("sourceNode");

            if (string.IsNullOrEmpty(folderUrl))
                throw new ArgumentNullException("folderUrl");

            justCreated = false;

            ReplicationFolder replicationFolder = this.GetReplicationFolder(sourceNode.UniqueID, folderUrl);
            if (replicationFolder == null)
            {
                lock (_ensureReplicationFolderLocker)
                {
                    replicationFolder = this.GetReplicationFolder(sourceNode.UniqueID, folderUrl);
                    if (replicationFolder == null)
                    {
                        Folder folder = this.Engine.EnsureFolderInternal(folderUrl);
                        IReplicationFolderMetadata replicationFolderMetadata = this.Engine.MetadataAdapter.CreateReplicationFolder(folder.Metadata, sourceNode.Metadata);
                        replicationFolderMetadata.IsCurrentNodeSettings = isCurrentSettings;
                        this.Engine.MetadataAdapter.SaveReplicationFolder(replicationFolderMetadata);
                        replicationFolder = new ReplicationFolder(this.Engine, folder, sourceNode, replicationFolderMetadata);

                        this.AddReplicationFolder(replicationFolder);
                    }
                }
            }

            return replicationFolder;
        }

        internal void Init()
        {
            //Debugger.Launch();

            //1 - поднять настройку из бд
            this.BuildSettings();

            //2 - добавляем настройку из конфига в метаданные
            if (this.ReplicationConfiguration.ConfigFolders.Count > 0)
            {
                foreach (ConfigReplicationFolder configFolder in this.ReplicationConfiguration.ConfigFolders)
                {
                    //проверка существования настройки из конфига
                    ReplicationFolder replicationFolder = this.GetReplicationFolder(configFolder);
                    if (replicationFolder == null)
                    {
                        //папки репликации нет в метаданных, но есть в конфиге, нужно ее создать
                        StorageNode sourceStorage = null;
                        foreach (StorageNode node in _storageNodesByID.Values)
                        {
                            if (node.Host.ToLower() == configFolder.SourceNode.ToLower())
                            {
                                sourceStorage = node;
                                break;
                            }
                        }

                        if (sourceStorage == null)
                        {
                            //1 - нет самого узла источника данных
                            this.StartObserverThread(configFolder.SourceNode);
                        }
                        else
                        {
                            //2 - узел есть, создаем папку репликации
                            bool justCreated;
                            replicationFolder = this.EnsureReplicationFolder(sourceStorage, configFolder.Url, true, out justCreated);
                        }
                    }
                    else
                    {
                        bool updateRequired = false;
                        if (!replicationFolder.IsCurrentNodeSettings)
                        {
                            //если вдруг по каким-то причинам флаг сняли
                            replicationFolder.IsCurrentNodeSettings = true;
                            updateRequired = true;
                        }

                        if (replicationFolder.Deleted)
                        {
                            //восстановление настройки
                            replicationFolder.Deleted = false;
                            updateRequired = true;
                        }

                        if (updateRequired)
                            replicationFolder.Update();
                    }
                }
            }

            //3 - проверяем настройку из бд, которая была текущей по конфигу
            //т.к. ее могли удалить
            foreach (Dictionary<string, ReplicationFolder> storageReplicationFolders in _replicationFolders.Values)
            {
                foreach (ReplicationFolder replicationFolder in storageReplicationFolders.Values)
                {
                    //проверяем только текущую настройку, остальная может быть актуальной на других узлах
                    if (replicationFolder.IsCurrentNodeSettings)
                    {
                        if (!this.ReplicationConfiguration.ContainsFolder(replicationFolder.Folder.Url))
                        {
                            //в конфиге нет настройки из бд
                            //помечаем ее в бд, как удаленную
                            replicationFolder.Deleted = true;
                            replicationFolder.Update();
                        }
                    }
                }
            }
        }

        private void BuildSettings()
        {
            //построение словаря узлов файловых хранилищ.
            ICollection<IStorageMetadata> storageNodes = this.Engine.MetadataAdapter.GetStorages();
            foreach (IStorageMetadata nodeMetadata in storageNodes)
            {
                StorageNode typedNode = new StorageNode(this.Engine, nodeMetadata);
                this.AddStorageNode(typedNode);
            }

            this.BuildReplicationFolders();
        }

        private void BuildReplicationFolders()
        {
            //построение словаря папок репликации
            ICollection<IReplicationFolderMetadata> replicationFolders = this.Engine.MetadataAdapter.GetReplicationFolders();
            foreach (IReplicationFolderMetadata replicationFolderMetadata in replicationFolders)
            {
                StorageNode storageNode = new StorageNode(this.Engine, replicationFolderMetadata.SourceStorage);
                Folder folder = this.Engine.EnsureFolderInternal(replicationFolderMetadata.Folder.Url);
                ReplicationFolder replicationFolder = new ReplicationFolder(this.Engine, folder, storageNode, replicationFolderMetadata);
                this.AddReplicationFolder(replicationFolder);
            }
        }

        private void RebuildStorageReplicationFolders(StorageNode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            //посмотреть конфиг и попытаться создать то, что еще не создано в бд
            foreach (ConfigReplicationFolder configFolder in this.ReplicationConfiguration.ConfigFolders)
            {
                //только для данного узла
                if (node.Host.ToLower() == configFolder.SourceNode.ToLower())
                {
                    //создаем папку репликации, если ее еще нет.
                    bool justCreated;
                    this.EnsureReplicationFolder(node, configFolder.Url, true, out justCreated);
                }
            }
        }

        private void AddStorageNode(StorageNode storageNode)
        {
            if (storageNode == null)
                throw new ArgumentNullException("storageNode");

            if (!_storageNodesByID.ContainsKey(storageNode.UniqueID))
            {
                lock (_addStorageLocker)
                {
                    if (!_storageNodesByID.ContainsKey(storageNode.UniqueID))
                    {
                        _storageNodesByID.Add(storageNode.UniqueID, storageNode);

                        //после добавления узла, запускаем по нему поток с запросом 
                        //если это не текущий узел
                        if (!storageNode.IsCurrent)
                            this.StartRelationThread(storageNode);
                    }
                }
            }
        }

        internal StorageNode GetStorage(Guid uniqueID)
        {
            if (uniqueID == Guid.Empty)
                throw new ArgumentNullException("uniqueID");

            StorageNode node = null;
            if (_storageNodesByID.ContainsKey(uniqueID))
                node = _storageNodesByID[uniqueID];

            return node;
        }

        private void AddReplicationFolder(ReplicationFolder replicationFolder)
        {
            if (replicationFolder == null)
                throw new ArgumentNullException("replicationFolder");

            Dictionary<string, ReplicationFolder> nodeReplicationFolders = null;
            bool added = false;
            if (!_replicationFolders.ContainsKey(replicationFolder.SourceStorage.UniqueID))
            {
                lock (_checkReplicationFolderLocker)
                {
                    if (!_replicationFolders.ContainsKey(replicationFolder.SourceStorage.UniqueID))
                    {
                        nodeReplicationFolders = new Dictionary<string, ReplicationFolder>();
                        FolderUri uri = new FolderUri(replicationFolder.TypedFolder.Url);
                        nodeReplicationFolders.Add(uri.UrlLower, replicationFolder);
                        _replicationFolders.Add(replicationFolder.SourceStorage.UniqueID, nodeReplicationFolders);

                        //запускаем поток по репликации папки
                        this.StartFilesThread(replicationFolder);

                        added = true;
                    }
                }
            }

            if (!added)
            {
                //внутренний словарь должен был либо добавиться выше, либо уже существовать
                if (!_replicationFolders.ContainsKey(replicationFolder.SourceStorage.UniqueID))
                    throw new Exception(string.Format("Не удалось добавить папку репликации, неизвестная ошибка."));

                nodeReplicationFolders = _replicationFolders[replicationFolder.SourceStorage.UniqueID];
                FolderUri uri = new FolderUri(replicationFolder.TypedFolder.Url);
                if (!nodeReplicationFolders.ContainsKey(uri.UrlLower))
                {
                    lock (_checkReplicationFolderLocker)
                    {
                        if (!nodeReplicationFolders.ContainsKey(uri.UrlLower))
                        {
                            nodeReplicationFolders.Add(uri.UrlLower, replicationFolder);
                            this.StartFilesThread(replicationFolder);
                        }
                    }
                }
            }
        }

        private ReplicationFolder GetReplicationFolder(ConfigReplicationFolder configReplicationFolder)
        {
            if (configReplicationFolder == null)
                throw new ArgumentNullException("configReplicationFolder");

            ReplicationFolder replicationFolder = null;

            IStorageMetadata replicationNodeMetadata = this.Engine.MetadataAdapter.GetStorage(configReplicationFolder.SourceNode);
            if (replicationNodeMetadata != null)
            {
                StorageNode replicationNode = this.GetStorage(replicationNodeMetadata.UniqueID);
                if (replicationNode == null)
                    throw new Exception(string.Format("Невозможно найти узел репликации {0} для папки {1}",
                        configReplicationFolder.SourceNode,
                        configReplicationFolder.Url));

                replicationFolder = this.GetReplicationFolder(replicationNode.UniqueID, configReplicationFolder.Url);
            }
            return replicationFolder;
        }

        internal ReplicationFolder GetReplicationFolder(Guid storageID, string folderUrl)
        {
            if (storageID == Guid.Empty)
                throw new ArgumentNullException("storageID");

            if (string.IsNullOrEmpty(folderUrl))
                throw new ArgumentNullException("folderUrl");

            ReplicationFolder replicationFolder = null;
            if (_replicationFolders.ContainsKey(storageID))
            {
                Dictionary<string, ReplicationFolder> storageFolders = _replicationFolders[storageID];
                FolderUri uri = new FolderUri(folderUrl);
                if (storageFolders.ContainsKey(uri.UrlLower))
                    replicationFolder = storageFolders[uri.UrlLower];
            }

            return replicationFolder;
        }

        private void StartObserverThread(string host)
        {
            if (string.IsNullOrEmpty(host))
                throw new ArgumentNullException("host");

            string hostLower = host.ToLower();
            if (!_observerThread.ContainsKey(hostLower))
            {
                lock (_commonLocker)
                {
                    if (!_observerThread.ContainsKey(hostLower))
                    {
                        Thread hostThread = new Thread(new ParameterizedThreadStart(this.ObserveWorker));
                        hostThread.IsBackground = true;
                        _observerThread.Add(hostLower, hostThread);
                        hostThread.Start(hostLower);
                    }
                }
            }
        }

        private void StartRelationThread(StorageNode storageNode)
        {
            if (storageNode == null)
                throw new ArgumentNullException("storageNode");

            //если это не текущий узел
            if (storageNode.IsCurrent)
                return;

            if (!_relationThread.ContainsKey(storageNode.UniqueID))
            {
                lock (_commonLocker)
                {
                    if (!_relationThread.ContainsKey(storageNode.UniqueID))
                    {
                        Thread relationThread = new Thread(new ParameterizedThreadStart(this.GetNeighborsWorker));
                        relationThread.IsBackground = true;
                        _relationThread.Add(storageNode.UniqueID, relationThread);
                        relationThread.Start(storageNode);
                    }
                }
            }
        }

        private void StartFilesThread(ReplicationFolder replicationFolder)
        {
            if (replicationFolder == null || replicationFolder.SourceStorage == null)
                throw new ArgumentNullException("storageNode");

            //если это не текущий узел
            if (replicationFolder.SourceStorage.IsCurrent)
                return;

            string key = string.Format("{0}_{1}",
                replicationFolder.SourceStorage.UniqueID,
                replicationFolder.TypedFolder.ID);

            if (!_filesThread.ContainsKey(key))
            {
                lock (_addFilesThreadLocker)
                {
                    if (!_filesThread.ContainsKey(key))
                    {
                        Thread filesThread = new Thread(new ParameterizedThreadStart(this.FilesWorker));
                        filesThread.IsBackground = true;
                        _filesThread.Add(key, filesThread);
                        filesThread.Start(replicationFolder);
                    }
                }
            }
        }

        private void ObserveWorker(object state)
        {
            if (state == null)
                throw new ArgumentNullException("state");

            string host = state.ToString();
            bool nodeAdded = false;

            while (!nodeAdded)
            {
                try
                {
                    IStorageNode node = this.ReplicationAdapter.Transport.GetStorageInfo(host);
                    if (node != null)
                    {
                        bool justCreated;
                        this.EnsureNode(node.UniqueID, node.Host, out justCreated);
                        //узел добавлен, можно отпустит поток
                        nodeAdded = true;
                    }
                }
                catch (Exception ex)
                {
                    this.Engine.Logger.WriteMessage(ex.ToString());
                }
                finally
                {
                    Thread.Sleep(Timeouts.Observe);
                }
            }
        }

        private void GetNeighborsWorker(object state)
        {
            if (state == null)
                throw new ArgumentNullException("state");

            StorageNode node = (StorageNode)state;

            while (true)
            {
                try
                {
                    //периодически запрашиваем информацию о сети
                    //передаем текущие папки, которые реплицируются на данной ноде
                    //чтобы возвращать только пересекающиеся
                    IReplicationSchema currentSchema = this.GetCurrentSchema();
                    IReplicationSchema remoteSchema = this.ReplicationAdapter.Transport.ExchangeSchema(node, currentSchema);

                    //обновляем схему по схеме соседнего узла по следующей логике
                    //что пришло со строгой ссылкой, то всегда создаем (значит эта настройка соседнего узла)
                    //со слабой ссылкой создаем только папки, по которым мы пересекаемся
                    if (remoteSchema != null)
                        this.UpdateSchema(remoteSchema);
                }
                catch (Exception ex)
                {
                    this.Engine.Logger.WriteMessage(ex.ToString());
                }
                finally
                {
                    Thread.Sleep(Timeouts.Schema);
                }
            }
        }

        private void FilesWorker(object state)
        {
            if (state == null)
                throw new ArgumentNullException("state");

            ReplicationFolder replicationFolder = (ReplicationFolder)state;
            while (true)
            {
                try
                {
                    //проверка необходимости дальнейшего продолжения реплицирования файлов
                    bool replicationExists = !replicationFolder.Deleted;
                    if (replicationExists)
                    {
                        Tuple<Guid, Guid>[] remoteFiles = this.ReplicationAdapter.Transport.GetReplicationFiles(
                             replicationFolder.SourceStorage,
                             this.Engine.CurrentNode.UniqueID,
                             replicationFolder.Folder.Url,
                             replicationFolder.LastSyncTime);

                        this.ReplicationAdapter.ProcessRemoteFiles(replicationFolder, remoteFiles);
                    }
                }
                catch (Exception ex)
                {
                    this.Engine.Logger.WriteMessage(ex.ToString());
                }
                finally
                {
                    Thread.Sleep(Timeouts.Files);
                }
            }
        }


        /// <summary>
        /// Возвращает схему репликации сети, известную текущему узлу.
        /// </summary>
        /// <returns></returns>
        internal IReplicationSchema GetCurrentSchema()
        {
            //выбираем текущий элемент схемы репликации по своим настройкам
            List<ReplicationSchemaItem> currentItems = new List<ReplicationSchemaItem>();
            foreach (Dictionary<string, ReplicationFolder> storageReplicationFolders in _replicationFolders.Values)
            {
                ReplicationSchemaItem currentItem = null;
                foreach (ReplicationFolder replicationFolder in storageReplicationFolders.Values)
                {
                    if (replicationFolder.Deleted)
                        continue;

                    if (replicationFolder.IsCurrentNodeSettings)
                    {
                        if (currentItem == null)
                            currentItem = new ReplicationSchemaItem(replicationFolder);
                        else
                            currentItem.Merge(replicationFolder);
                    }
                }

                if (currentItem != null)
                    currentItems.Add(currentItem);
            }

            //остальная схема репликации сети, которая находится в метаданных
            //она может быть неактуальной, но у нее будет WEAK признак
            List<ReplicationSchemaItem> restSchemaItems = new List<ReplicationSchemaItem>();
            foreach (Dictionary<string, ReplicationFolder> storageReplicationFolders in _replicationFolders.Values)
            {
                ReplicationSchemaItem restSchemaItem = null;
                foreach (ReplicationFolder replicationFolder in storageReplicationFolders.Values)
                {
                    if (replicationFolder.Deleted)
                        continue;

                    if (!replicationFolder.IsCurrentNodeSettings)
                    {
                        if (restSchemaItem == null)
                            restSchemaItem = new ReplicationSchemaItem(replicationFolder);
                        else
                            restSchemaItem.Merge(replicationFolder);
                    }
                }

                if (restSchemaItem != null)
                    restSchemaItems.Add(restSchemaItem);
            }

            ReplicationSchema schema = new ReplicationSchema(this.Engine.CurrentNode, currentItems, restSchemaItems);

            return schema;
        }

        /// <summary>
        /// Обновляет текущую схему репликацию сети, по схеме репликации другого узла сети.
        /// </summary>
        /// <param name="remoteSchema">Схема репликации другого узла сети.</param>
        internal void UpdateSchema(IReplicationSchema remoteSchema)
        {
            if (remoteSchema == null)
                throw new ArgumentNullException("remoteSchema");

            //remote схема содержит ссылки на текущий узел
            bool remoteSchemaContainsCurrentStorage = false;

            //обновляем схему по схеме соседнего узла по следующей логике
            //что пришло со строгой ссылкой, то всегда создаем (значит эта настройка соседнего узла)
            //со слабой ссылкой создаем только папки, по которым мы пересекаемся
            if (remoteSchema.StrongItems != null)
            {
                foreach (IReplicationSchemaItem schemaItem in remoteSchema.StrongItems)
                {
                    bool forCurrentStorage = schemaItem.StorageID == this.Engine.CurrentNode.UniqueID;
                    if (!remoteSchemaContainsCurrentStorage)
                        remoteSchemaContainsCurrentStorage = forCurrentStorage;

                    this.UpdateSchemaItem(remoteSchema, schemaItem);
                }
            }

            if (remoteSchema.WeakItems != null)
            {
                foreach (IReplicationSchemaItem schemaItem in remoteSchema.WeakItems)
                {
                    bool forCurrentStorage = schemaItem.StorageID == this.Engine.CurrentNode.UniqueID;
                    if (!remoteSchemaContainsCurrentStorage)
                        remoteSchemaContainsCurrentStorage = forCurrentStorage;

                    this.UpdateSchemaItem(remoteSchema, schemaItem);
                }
            }

            if (remoteSchemaContainsCurrentStorage)
            {
                //схема remote узла содержит текущий узел
                bool justCreated;
                StorageNode remoteNode = this.EnsureNode(remoteSchema.StorageID, remoteSchema.Host, out justCreated);
                remoteNode.LastAccessTime = DateTime.Now;
                remoteNode.Update();
            }

            //удаление текущий настроек по remote схеме
            //для этого нужно пройтись по всем настройкам схемы текущего узла для remote узла
            if (_replicationFolders.ContainsKey(remoteSchema.StorageID))
            {
                #region Удаление из текущей схемы
                Dictionary<string, ReplicationFolder> replicationFoldersForRemoteSchema = _replicationFolders[remoteSchema.StorageID];
                foreach (ReplicationFolder replicationFolder in replicationFoldersForRemoteSchema.Values)
                {
                    if (replicationFolder.IsCurrentNodeSettings)
                    {
                        //в схеме текущего узла репликация с remote узлом задана в настройке (STRONG)
                        //такие настройки удалять не нужно
                        continue;
                    }

                    bool containsInRemoteSchema = false;
                    FolderUri localUri = new FolderUri(replicationFolder.Folder.Url);

                    //содержаться может и в STRONG и в WEAK виде
                    if (remoteSchema.StrongItems != null)
                    {
                        foreach (IReplicationSchemaItem schemaItem in remoteSchema.StrongItems)
                        {
                            if (schemaItem.Folders != null)
                            {
                                foreach (string folderUrl in schemaItem.Folders)
                                {
                                    FolderUri remoteUri = new FolderUri(folderUrl);
                                    if (localUri.UrlLower == remoteUri.UrlLower)
                                    {
                                        containsInRemoteSchema = true;
                                        break;
                                    }
                                }
                            }

                            if (containsInRemoteSchema)
                                break;
                        }
                    }

                    if (!containsInRemoteSchema && remoteSchema.WeakItems != null)
                    {
                        foreach (IReplicationSchemaItem schemaItem in remoteSchema.WeakItems)
                        {
                            if (schemaItem.Folders != null)
                            {
                                foreach (string folderUrl in schemaItem.Folders)
                                {
                                    FolderUri remoteUri = new FolderUri(folderUrl);
                                    if (localUri.UrlLower == remoteUri.UrlLower)
                                    {
                                        containsInRemoteSchema = true;
                                        break;
                                    }
                                }
                            }

                            if (containsInRemoteSchema)
                                break;
                        }
                    }

                    //в remote схеме нет репликации с папками из схемы текущего узла
                    if (!containsInRemoteSchema)
                    {
                        replicationFolder.Deleted = true;
                        replicationFolder.Update();
                    }
                }
                #endregion
            }
        }

        private void UpdateSchemaItem(IReplicationSchema remoteSchema, IReplicationSchemaItem remoteSchemaItem)
        {
            if (remoteSchema == null)
                throw new ArgumentNullException("remoteSchema");

            if (remoteSchemaItem == null)
                throw new ArgumentNullException("remoteSchemaItem");

            bool forCurrentStorage = remoteSchemaItem.StorageID == this.Engine.CurrentNode.UniqueID;
            bool isStrongRelation = remoteSchemaItem.RelationType == ReplicationRelation.Strong;
            //хост хранилища может поменяться
            //ищем по UniqueID и если не совпадают хосты, то меняем хост
            //доверяем хосту, только от которого пришел ответ
            if (forCurrentStorage)
            {
                if (isStrongRelation)
                {
                    //есть строгая ссылка на текущий узел, для узла, с которого пришел запрос.
                    #region Обновление информации об узле
                    bool justCreated;
                    StorageNode remoteNode = this.EnsureNode(remoteSchema.StorageID, remoteSchema.Host, out justCreated);

                    if (!justCreated)
                    {
                        //если узел уже был, то проверяем нужно ли обновить хост существующего узла
                        if (remoteNode.Host.ToLower() != remoteSchema.Host.ToLower())
                        {
                            lock (_updateStorageLocker)
                            {
                                if (remoteNode.Host.ToLower() != remoteSchema.Host.ToLower())
                                {
                                    remoteNode.Host = remoteSchema.Host.ToLower();
                                    remoteNode.Update();
                                }
                            }
                        }
                    }
                    #endregion
                }
            }

            //если это STRONG признак, то создаем, если настройки нет
            foreach (string folderUrl in remoteSchemaItem.Folders)
            {
                if (isStrongRelation)
                {
                    #region strong relation
                    //настройка пришла непосредственно с узла схемы и на этом узле есть
                    //если она предназначена для текущего узла, то создаем/снимаем признак удаленности
                    if (forCurrentStorage)
                    {
                        //узел remote схемы
                        bool justCreated;
                        StorageNode remoteNode = this.EnsureNode(remoteSchema.StorageID, remoteSchema.Host, out justCreated);

                        //получаем папку для репликации с remote узлом
                        bool replicationFolderJustCreated;
                        ReplicationFolder replicationFolder = this.EnsureReplicationFolder(remoteNode, folderUrl, false, out replicationFolderJustCreated);
                        if (!replicationFolderJustCreated)
                        {
                            //если настройка существовала и была удалена, то восстанавливаем ее
                            if (replicationFolder.Deleted)
                            {
                                replicationFolder.Deleted = false;
                                replicationFolder.Update();
                            }
                        }
                    }
                    else
                    {
                        //Strong ссылка на репликацию с 3-им (отличным от текущего) узлом, 
                        //смотрим если на текущем узле есть пересекающиеся папки, то добавляем weak настройку
                        this.AddWeakRelation(remoteSchemaItem.StorageID, remoteSchemaItem.Name, folderUrl);
                    }
                    #endregion
                }
                else
                {
                    if (forCurrentStorage)
                    {
                        //слабые ссылки на самого себя не надо восстанавливать
                        continue;
                    }

                    //если это WEAK признак, то создаем, только если есть пересекающиеся настройки
                    this.AddWeakRelation(remoteSchemaItem.StorageID, remoteSchemaItem.Name, folderUrl);

                    //проверку на признак удаления делать не нужно, т.к. ссылка слабая
                }
            }
        }

        private StorageNode EnsureNode(Guid uniqueID, string host, out bool justCreated)
        {
            if (uniqueID == Guid.Empty)
                throw new ArgumentNullException("uniqueID");

            justCreated = false;
            StorageNode savedNode = this.GetStorage(uniqueID);
            if (savedNode == null)
            {
                lock (_updateStorageLocker)
                {
                    savedNode = this.GetStorage(uniqueID);
                    if (savedNode == null)
                    {
                        //ноды нет в словаре, создаем
                        IStorageMetadata nodeMetadata = this.Engine.MetadataAdapter.CreateStorageNode(host, uniqueID);
                        nodeMetadata.LastAccessTime = DateTime.Now;
                        savedNode = new StorageNode(this.Engine, nodeMetadata);
                        this.Engine.MetadataAdapter.SaveStorage(nodeMetadata);

                        //добавляем в словарь
                        this.AddStorageNode(savedNode);
                        justCreated = true;

                        //после сохранения узла, может стать доступна настройка
                        //репликации с тим узлом
                        this.RebuildStorageReplicationFolders(savedNode);
                    }
                }
            }

            return savedNode;
        }

        private void AddWeakRelation(Guid storageID, string host, string folderUrl)
        {
            if (string.IsNullOrEmpty(folderUrl))
                throw new ArgumentNullException("folderUrl");

            if (string.IsNullOrEmpty(host))
                throw new ArgumentNullException("host");

            bool exists = false;
            foreach (Dictionary<string, ReplicationFolder> storageReplicationFolders in _replicationFolders.Values)
            {
                FolderUri uri = new FolderUri(folderUrl);
                exists = storageReplicationFolders.ContainsKey(uri.UrlLower);
                if (exists)
                    break;
            }

            //настройка существует, но может существовать для другого узла
            //в таком случае нужно создать
            if (exists)
            {
                //но для текущего элемента схемы ее нет
                ReplicationFolder replicationFolder = this.GetReplicationFolder(storageID, folderUrl);
                lock (_updateFolderLocker)
                {
                    replicationFolder = this.GetReplicationFolder(storageID, folderUrl);
                    if (replicationFolder == null)
                    {
                        //создание узла для элемента схемы
                        bool justCreated;
                        StorageNode schemaItemNode = this.EnsureNode(storageID, host, out justCreated);

                        bool replicationFolderJustCreated;
                        replicationFolder = this.EnsureReplicationFolder(schemaItemNode, folderUrl, false, out replicationFolderJustCreated);
                    }
                }
            }
        }


        private class Timeouts
        {
            public static readonly TimeSpan Observe = TimeSpan.FromSeconds(30);
            public static readonly TimeSpan Files = TimeSpan.FromSeconds(30);
            public static readonly TimeSpan Schema = TimeSpan.FromSeconds(5 * 60);
        }
    }
}