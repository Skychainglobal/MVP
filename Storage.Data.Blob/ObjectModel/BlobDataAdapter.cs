using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Storage.Engine;
using Storage.Lib;

namespace Storage.Data.Blob
{
    /// <summary>
    /// Адаптер данных. Хранит данные в блобе(ах).
    /// </summary>
    internal class BlobDataAdapter : IDataAdapter
    {
        public BlobDataAdapter() { }

        public BlobDataAdapter(XmlNode node)
        {
            this.Node = node;
        }

        /// <summary>
        /// Узел настроек.
        /// </summary>
        private XmlNode Node { get; set; }

        private bool __init_MaxBlobSize;
        private long _MaxBlobSize;
        /// <summary>
        /// Максимальный размер блоба.
        /// </summary>
        public long MaxBlobSize
        {
            get
            {
                if (!__init_MaxBlobSize)
                {
                    long value = BlobConsts.Blobs.DefaultMaxBlobSize;
                    if (this.Node != null)
                    {
                        XmlNode sizeParamNode = this.Node.SelectSingleNode("MaxBlobSize");
                        if (sizeParamNode != null && !string.IsNullOrEmpty(sizeParamNode.InnerText))
                            value = long.Parse(sizeParamNode.InnerText);
                    }

                    _MaxBlobSize = value;

                    __init_MaxBlobSize = true;
                }
                return _MaxBlobSize;
            }
        }

        private bool __init_ActiveBlobsCount;
        private int _ActiveBlobsCount;
        /// <summary>
        /// Максимальный размер блоба.
        /// </summary>
        public int ActiveBlobsCount
        {
            get
            {
                if (!__init_ActiveBlobsCount)
                {
                    int value = BlobConsts.Blobs.DefaultActiveBlobsCount;
                    if (this.Node != null)
                    {
                        XmlNode sizeParamNode = this.Node.SelectSingleNode("ActiveBlobsCount");
                        if (sizeParamNode != null && !string.IsNullOrEmpty(sizeParamNode.InnerText))
                        {
                            value = int.Parse(sizeParamNode.InnerText);
                            if (value < 1)
                                throw new Exception(string.Format("Количество активных блобов контейнера должно быть больше 0"));
                        }
                    }


                    _ActiveBlobsCount = value;

                    __init_ActiveBlobsCount = true;
                }
                return _ActiveBlobsCount;
            }
        }

        private bool __init_AllowedStreamingLocksLimit;
        private double _AllowedStreamingLocksLimit;
        /// <summary>
        /// Максимальный размер блоба.
        /// </summary>
        public double AllowedStreamingLocksLimit
        {
            get
            {
                if (!__init_AllowedStreamingLocksLimit)
                {
                    int value = BlobConsts.Blobs.AllowedStreamingLocksLimit;
                    if (this.Node != null)
                    {
                        XmlNode streamingLocksLimitParamNode = this.Node.SelectSingleNode("AllowedStreamingLocksLimit");
                        if (streamingLocksLimitParamNode != null && !string.IsNullOrEmpty(streamingLocksLimitParamNode.InnerText))
                        {
                            value = int.Parse(streamingLocksLimitParamNode.InnerText);
                            if (value < 1 || value > 100)
                                throw new Exception(string.Format("Значение % от активных блобов, которое может быть заблокировано операциями потоковой передачи данных должно быть целым числом и входить в диапазон от 0 до 100."));
                        }
                    }

                    _AllowedStreamingLocksLimit = (double)value / 100;

                    __init_AllowedStreamingLocksLimit = true;
                }
                return _AllowedStreamingLocksLimit;
            }
        }

        private bool __init_MetadataAdapter;
        private IMetadataAdapter _MetadataAdapter;
        /// <summary>
        /// Адаптер метаданных.
        /// </summary>
        public IMetadataAdapter MetadataAdapter
        {
            get
            {
                if (!__init_MetadataAdapter)
                {
                    _MetadataAdapter = ConfigFactory.Instance.Create<IMetadataAdapter>();
                    __init_MetadataAdapter = true;
                }
                return _MetadataAdapter;
            }
        }

        private bool __init_BlobMetadataAdapter;
        private IBlobMetadataAdapter _BlobMetadataAdapter;
        /// <summary>
        /// Адаптер метаданных для блоба.
        /// </summary>
        internal IBlobMetadataAdapter BlobMetadataAdapter
        {
            get
            {
                if (!__init_BlobMetadataAdapter)
                {
                    _BlobMetadataAdapter = ConfigFactory.Instance.Create<IBlobMetadataAdapter>(this.MetadataAdapter);
                    __init_BlobMetadataAdapter = true;
                }
                return _BlobMetadataAdapter;
            }
        }

        private bool __init_Logger;
        private ILogProvider _Logger;
        /// <summary>
        /// Логгер.
        /// </summary>
        internal ILogProvider Logger
        {
            get
            {
                if (!__init_Logger)
                {
                    _Logger = ConfigFactory.Instance.Create<ILogProvider>(BlobConsts.Logs.Scopes.DataBlobAdapter);
                    __init_Logger = true;
                }
                return _Logger;
            }
        }




        private IFileVersionMetadata WriteInternal(IBlobFileMetadata blobFileMetadata, Stream stream, DateTime? remoteTimeCreated = null)
        {
            if (blobFileMetadata == null)
                throw new ArgumentNullException("param");

            if (stream == null)
                throw new ArgumentNullException("stream");

            this.Logger.WriteFormatMessage("WriteFile:Начало записи файла, folderMetadata.Name: {0}, fileName: {1}", blobFileMetadata.FolderMetadata.Name, blobFileMetadata.Name);

            //получение контейнера блобов.
            BlobContainer container = this.GetBlobContainer(blobFileMetadata.FolderMetadata);

            //запись файла в контейнер.
            BlobFileInfo blobFileInfo = container.Write(blobFileMetadata, stream, remoteTimeCreated);

            //установка свойств файла, хранящегося в блобе
            blobFileMetadata.BlobID = blobFileInfo.BlobID;
            blobFileMetadata.BlobStartPosition = blobFileInfo.BlobStartPosition;
            blobFileMetadata.BlobEndPosition = blobFileInfo.BlobEndPosition;

            this.Logger.WriteMessage("WriteFile:Начало сохранения метаданных файла");
            IBlobFileVersionMetadata savedVersion = this.BlobMetadataAdapter.SaveFile(blobFileMetadata, blobFileInfo.TimeCreated);
            this.Logger.WriteMessage("WriteFile:Окончание сохранения метаданных файла");

            this.Logger.WriteFormatMessage("WriteFile:Начало записи файла, folderMetadata.Name: {0}, fileName: {1}, fileUniqueID: {2}, fileVersionUniqueID: {3}",
                blobFileMetadata.FolderMetadata.Name,
                blobFileMetadata.Name,
                savedVersion.FileMetadata.UniqueID,
                savedVersion.UniqueID);

            return savedVersion;
        }

        /// <summary>
        /// Записывает файл в хранилище.
        /// </summary>
        /// <param name="folderMetadata">Метаданные папки.</param>
        /// <param name="fileName">Имя файла.</param>
        /// <param name="stream">Содержимое файла.</param>
        /// <returns></returns>
        public IFileVersionMetadata WriteFile(IFolderMetadata folderMetadata, string fileName, Stream stream)
        {
            if (folderMetadata == null)
                throw new ArgumentNullException("folderMetadata");

            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException("fileName");

            if (stream == null)
                throw new ArgumentNullException("stream");

            IBlobFileMetadata blobFileMetadata = this.BlobMetadataAdapter.CreateFile(folderMetadata, fileName);
            blobFileMetadata.Name = fileName;

            //запись файла
            IFileVersionMetadata savedVersion = this.WriteInternal(blobFileMetadata, stream);

            return savedVersion;
        }

        /// <summary>
        /// Записывает версию файла в хранилище.
        /// </summary>
        /// <param name="fileMetadata">Метаданные существующего файла.</param>
        /// <param name="stream">Содержимое файла.</param>
        /// <param name="fileName">Имя файла.</param>
        /// <returns></returns>
        public IFileVersionMetadata WriteFileVersion(IFileMetadata fileMetadata, Stream stream, string fileName = null)
        {
            if (fileMetadata == null)
                throw new ArgumentNullException("fileMetadata");

            if (fileMetadata.FolderMetadata == null)
                throw new ArgumentNullException("fileMetadata.FolderMetadata");

            if (stream == null)
                throw new ArgumentNullException("stream");

            this.Logger.WriteFormatMessage("WriteFileVersion:Начало записи новой версии файла, fileMetadata.Name: {0}, fileMetadata.UniqueID: {1}", fileMetadata.Name, fileMetadata.UniqueID);

            IBlobFileMetadata blobFileMetadata = (IBlobFileMetadata)fileMetadata;
            if (!string.IsNullOrEmpty(fileName))
                blobFileMetadata.Name = fileName;

            //резервация параметров сохранения
            //например идентификатора версии, его нужно знать заранее.
            blobFileMetadata.EnsureSaveProperties();

            //запись версии файла
            IFileVersionMetadata savedVersion = this.WriteInternal(blobFileMetadata, stream);

            return savedVersion;
        }

        /// <summary>
        /// Возвращает контейнер блобов для папки.
        /// </summary>
        /// <param name="folderMetadata">Метаданные папки.</param>
        /// <returns></returns>
        private BlobContainer GetBlobContainer(IFolderMetadata folderMetadata)
        {
            if (folderMetadata == null)
                throw new ArgumentNullException("folderMetadata");

            this.Logger.WriteFormatMessage("GetBlobContainer:Начало получения контейнера, folder.Url: {0}", folderMetadata.Url);

            ICollection<IBlobContainerMetadata> blobContainers = this.BlobMetadataAdapter.GetBlobContainers(folderMetadata.ID);
            if (blobContainers == null || blobContainers.Count == 0)
                blobContainers = this.BlobMetadataAdapter.DefaultContainers;

            if (blobContainers == null || blobContainers.Count == 0)
                throw new Exception(string.Format("Не удалось найти ни одного контейнера блобов для папки с именем {0}",
                    folderMetadata.Name));

            IBlobContainerMetadata metadata = null;
            if (blobContainers.Count > 1)
            {
                //выбираем рандомный контейнер для записи.
                int index = Math.Abs(Guid.NewGuid().GetHashCode() % blobContainers.Count);
                metadata = blobContainers.ToList()[index];
            }
            else
                metadata = blobContainers.First();

            BlobContainer container = new BlobContainer(this, metadata);
            this.Logger.WriteFormatMessage("GetBlobContainer:Окончание получения контейнера, folder.Url: {0}", folderMetadata.Url);

            return container;
        }

        /// <summary>
        /// Возвращает метаданные файла хранилища.
        /// </summary>
        /// <param name="folderMetadata">Метаданные папки.</param>
        /// <param name="fileUniqueID">Уникальный идентификатор файла.</param>
        /// <returns></returns>
        public IFileMetadata ReadFileMetadata(IFolderMetadata folderMetadata, Guid fileUniqueID)
        {
            if (folderMetadata == null)
                throw new ArgumentNullException("folderMetadata");

            if (fileUniqueID == Guid.Empty)
                throw new ArgumentNullException("fileUniqueID");

            this.Logger.WriteFormatMessage("ReadFileMetadata:Начало получения метаданных файла, folder.Url: {0}, fileUniqueID: {1}", folderMetadata.Url, fileUniqueID);
            IBlobFileMetadata fileMetadata = this.BlobMetadataAdapter.GetFile(folderMetadata, fileUniqueID);
            this.Logger.WriteFormatMessage("ReadFileMetadata:Окончание получения метаданных файла, folder.Url: {0}, fileUniqueID: {1}", folderMetadata.Url, fileUniqueID);

            return fileMetadata;
        }

        /// <summary>
        /// Возвращает содержимое файла хранилища.
        /// </summary>
        /// <param name="fileMetadata">Метаданные файла.</param>
        /// <returns></returns>
        public byte[] ReadFileContent(IFileMetadata fileMetadata)
        {
            if (fileMetadata == null)
                throw new ArgumentNullException("fileMetadata");

            this.Logger.WriteFormatMessage("ReadFileContent:Начало чтения содержимого файла, folder.Url: {0}, fileMetadata.UniqueID: {1}", fileMetadata.FolderMetadata.Url, fileMetadata.UniqueID);

            IBlobFileMetadata typedFileMetadata = (IBlobFileMetadata)fileMetadata;
            //получаем контейнер блобов
            Blob blob = this.GetBlob(typedFileMetadata.BlobID);
            byte[] content = blob.Read(typedFileMetadata);

            this.Logger.WriteFormatMessage("ReadFileContent:Окончание чтения содержимого файла, folder.Url: {0}, fileMetadata.UniqueID: {1}", fileMetadata.FolderMetadata.Url, fileMetadata.UniqueID);

            return content;
        }

        /// <summary>
        /// Возвращает содержимое версии файла.
        /// </summary>
        /// <param name="versionMetadata">Метаданные версии файла.</param>
        /// <returns></returns>
        public byte[] ReadFileVersionContent(IFileVersionMetadata versionMetadata)
        {
            if (versionMetadata == null)
                throw new ArgumentNullException("versionMetadata");

            this.Logger.WriteFormatMessage("ReadFileVersionContent:Начало чтения содержимого версии файла, folder.Url: {0}, version.UniqueID: {1}", versionMetadata.FileMetadata.FolderMetadata.Url, versionMetadata.UniqueID);

            IBlobFileVersionMetadata typedVersionMetadata = (IBlobFileVersionMetadata)versionMetadata;
            //получаем контейнер блобов
            Blob blob = this.GetBlob(typedVersionMetadata.BlobID);
            byte[] content = blob.Read(typedVersionMetadata);

            this.Logger.WriteFormatMessage("ReadFileVersionContent:Окончание чтения содержимого версии файла, folder.Url: {0}, version.UniqueID: {1}", versionMetadata.FileMetadata.FolderMetadata.Url, versionMetadata.UniqueID);

            return content;
        }

        /// <summary>
        /// Возвращает коллекцию метаданных версий файла.
        /// </summary>
        /// <param name="fileMetadata">Метаданные файла.</param>
        /// <returns></returns>
        public ICollection<IFileVersionMetadata> ReadFileVersionsMetadata(IFileMetadata fileMetadata)
        {
            if (fileMetadata == null)
                throw new ArgumentNullException("fileMetadata");

            this.Logger.WriteFormatMessage("ReadFileVersionsMetadata:Начало получения версий файла, folder.Url: {0}, file.UniqueID: {1}", fileMetadata.FolderMetadata.Url, fileMetadata.UniqueID);

            IBlobFileMetadata typedFileMetadata = (IBlobFileMetadata)fileMetadata;
            ICollection<IBlobFileVersionMetadata> fileVersionsMetadata = this.BlobMetadataAdapter.GetVersions(typedFileMetadata);
            List<IFileVersionMetadata> result = new List<IFileVersionMetadata>();
            if (fileVersionsMetadata != null)
                result.AddRange(fileVersionsMetadata);

            this.Logger.WriteFormatMessage("ReadFileVersionsMetadata:Окончание получения версий файла, folder.Url: {0}, file.UniqueID: {1}", fileMetadata.FolderMetadata.Url, fileMetadata.UniqueID);

            return result;
        }


        /// <summary>
        /// Удаляет файл.
        /// </summary>
        /// <param name="folderUrl">Адрес папки.</param>
        /// <param name="fileUniqueID">Уникальный идентификатор файла.</param>
        /// <returns></returns>
        public bool DeleteFile(IFolderMetadata folderMetadata, Guid fileUniqueID)
        {
            if (folderMetadata == null)
                throw new ArgumentNullException("folderMetadata");

            if (fileUniqueID == Guid.Empty)
                throw new ArgumentNullException("fileUniqueID");

            this.Logger.WriteFormatMessage("DeleteFile:Начало удаления файла, folder.Url: {0}, file.UniqueID: {1}", folderMetadata.Url, fileUniqueID);

            bool result = false;
            //физического удаления для файла в блобе не предусмотрено
            //удаление заключается в установке Deleted для метаданных файла.
            IBlobFileMetadata fileMetadata = this.BlobMetadataAdapter.GetFile(folderMetadata, fileUniqueID);
            if (fileMetadata != null)
            {
                this.BlobMetadataAdapter.DeleteFile(fileMetadata);
                result = fileMetadata.Deleted;
            }
            this.Logger.WriteFormatMessage("DeleteFile:Окончание удаления файла, folder.Url: {0}, file.UniqueID: {1}", folderMetadata.Url, fileUniqueID);


            return result;
        }

        /// <summary>
        /// Удаляет файл.
        /// </summary>
        /// <param name="fileMetadata">Метаданные файла.</param>
        /// <returns></returns>
        public bool DeleteFile(IFileMetadata fileMetadata)
        {
            if (fileMetadata == null)
                throw new ArgumentNullException("fileMetadata");

            if (fileMetadata.FolderMetadata == null)
                throw new ArgumentNullException("fileMetadata.FolderMetadata");

            this.Logger.WriteFormatMessage("DeleteFile:Начало удаления файла, folder.Url: {0}, file.UniqueID: {1}", fileMetadata.FolderMetadata.Url, fileMetadata.UniqueID);

            IBlobFileMetadata typedFileMetadata = (IBlobFileMetadata)fileMetadata;
            this.BlobMetadataAdapter.DeleteFile(typedFileMetadata);

            this.Logger.WriteFormatMessage("DeleteFile:Окончание удаления файла, folder.Url: {0}, file.UniqueID: {1}", fileMetadata.FolderMetadata.Url, fileMetadata.UniqueID);

            return fileMetadata.Deleted;
        }


        /// <summary>
        /// Возвращает блоб.
        /// </summary>
        /// <param name="id">Идентификатор блоба.</param>
        /// <param name="throwIfNotExists">Выбросить исключение, если блоб не существует?</param>
        /// <returns></returns>
        private Blob GetBlob(int id, bool throwIfNotExists = true)
        {
            if (id < 1)
                throw new ArgumentNullException("id");

            this.Logger.WriteFormatMessage("GetBlob:Начало получения блоба для файла, id: {0}", id);

            //метаданные блоба
            IBlobMetadata metadata = this.BlobMetadataAdapter.GetBlob(id);
            if (throwIfNotExists && metadata == null)
                throw new Exception(string.Format("Не удалось найти блоб с идентификатором {0}", id));

            //метаданные контейнера
            IBlobContainerMetadata containerMetadata = this.BlobMetadataAdapter.GetBlobContainer(metadata.ContainerID);
            if (throwIfNotExists && containerMetadata == null)
                throw new Exception(string.Format("Не удалось найти контейнер блобов с идентификатором {0}", metadata.ContainerID));

            BlobContainer container = new BlobContainer(this, containerMetadata);
            Blob blob = new Blob(container, metadata);

            this.Logger.WriteFormatMessage("GetBlob:Окончание получения блоба для файла, id: {0}", id);

            return blob;
        }

        /// <summary>
        /// Освобождает ресурсы, занятые объектом
        /// </summary>
        public void Dispose()
        {
            if (_MetadataAdapter != null)
                this.MetadataAdapter.Dispose();

            if (_BlobMetadataAdapter != null)
                this.BlobMetadataAdapter.Dispose();
        }


        static BlobDataAdapter()
        {
            //TODO DM DATA.BLOB --- move to storage installer
            //обновление схемы адаптера метаданных по конфигурации
            BlobDataAdapter.InitSchema();
        }

        private static void InitSchema()
        {
            BlobDataAdapterConfiguration configuration = new BlobDataAdapterConfiguration();
            configuration.UpdateMetadataSchema();
        }

        /// <summary>
        /// Восстанавливает метаданные по существующим данным.
        /// </summary>
        public void RestoreMetadata()
        {
            this.Logger.WriteMessage("Начало процедуры восстановления метаданных");

            //background операция восстановления целостности метаданных
            Thread restoreThread = new Thread(new ParameterizedThreadStart(BlobDataAdapter.RestoreMetadataWorker));
            restoreThread.IsBackground = true;
            restoreThread.Start(this);

            this.Logger.WriteMessage("Запущен поток процедуры восстановления метаданных.");
        }

        private static void RestoreMetadataWorker(object dataAdapterObj)
        {
            if (dataAdapterObj == null)
                throw new ArgumentNullException("dataAdapterObj");

            ILogProvider logger = null;

            try
            {
                BlobDataAdapter dataAdapter = (BlobDataAdapter)dataAdapterObj;
                logger = dataAdapter.Logger;

                RestoreBlobMetadataAdapter restoreMetadataAdapter = new RestoreBlobMetadataAdapter(dataAdapter);
                restoreMetadataAdapter.Restore();
            }
            catch (Exception ex)
            {
                if (logger != null)
                    logger.WriteFormatMessage("Не удалось выполнить процедуру восстановления метаданных. Текст ошибки: {0}", ex);
            }
        }

        #region Streaming
        /// <summary>
        /// Считывает поток данных файла.
        /// </summary>
        /// <param name="fileMetadata">Метаданные файла.</param>
        /// <returns></returns>
        public Stream ReadFileStream(IFileMetadata fileMetadata)
        {
            if (fileMetadata == null)
                throw new ArgumentNullException("fileMetadata");

            this.Logger.WriteFormatMessage("ReadFileStream:Начало чтения содержимого файла, folder.Url: {0}, fileMetadata.UniqueID: {1}", fileMetadata.FolderMetadata.Url, fileMetadata.UniqueID);
            IBlobFileMetadata typedFileMetadata = (IBlobFileMetadata)fileMetadata;
            //получаем контейнер блобов
            Blob blob = this.GetBlob(typedFileMetadata.BlobID);
            Stream stream = blob.ReadStream(typedFileMetadata);

            this.Logger.WriteFormatMessage("ReadFileStream:Окончание чтения содержимого файла, folder.Url: {0}, fileMetadata.UniqueID: {1}", fileMetadata.FolderMetadata.Url, fileMetadata.UniqueID);

            return stream;
        }

        /// <summary>
        /// Считывает поток данных версии файла.
        /// </summary>
        /// <param name="versionMetadata">Метаданные версии файла.</param>
        /// <returns></returns>
        public Stream ReadFileVersionStream(IFileVersionMetadata versionMetadata)
        {
            if (versionMetadata == null)
                throw new ArgumentNullException("versionMetadata");

            this.Logger.WriteFormatMessage("ReadFileVersionStream:Начало чтения содержимого версии файла, folder.Url: {0}, version.UniqueID: {1}", versionMetadata.FileMetadata.FolderMetadata.Url, versionMetadata.UniqueID);

            IBlobFileVersionMetadata typedVersionMetadata = (IBlobFileVersionMetadata)versionMetadata;
            //получаем контейнер блобов
            Blob blob = this.GetBlob(typedVersionMetadata.BlobID);
            Stream stream = blob.ReadStream(typedVersionMetadata);

            this.Logger.WriteFormatMessage("ReadFileVersionStream:Окончание чтения содержимого версии файла, folder.Url: {0}, version.UniqueID: {1}", versionMetadata.FileMetadata.FolderMetadata.Url, versionMetadata.UniqueID);

            return stream;
        }
        #endregion

        #region Remote Files (replication)

        public IFileVersionMetadata WriteRemoteFile(IFolderMetadata folderMetadata, IRemoteFile remoteFile)
        {
            if (folderMetadata == null)
                throw new ArgumentNullException("folderMetadata");

            if (remoteFile == null)
                throw new ArgumentNullException("remoteFile");

            if (string.IsNullOrEmpty(remoteFile.Name))
                throw new ArgumentNullException("remoteFile.Name");

            if (remoteFile.Stream == null)
                throw new ArgumentNullException("remoteFile.Stream");

            IBlobFileMetadata blobFileMetadata = this.BlobMetadataAdapter.CreateFile(folderMetadata, remoteFile.Name);
            blobFileMetadata.Name = remoteFile.Name;

            //резервация параметров сохранения файла с 
            blobFileMetadata.EnsureRemoteSaveProperties(remoteFile);

            //запись файла
            IFileVersionMetadata savedVersion = this.WriteInternal(blobFileMetadata, remoteFile.Stream, remoteFile.TimeCreated);

            return savedVersion;
        }

        public IFileVersionMetadata WriteRemoteFileVersion(IFileMetadata fileMetadata, IRemoteFile remoteFile)
        {
            if (fileMetadata == null)
                throw new ArgumentNullException("fileMetadata");

            if (fileMetadata.FolderMetadata == null)
                throw new ArgumentNullException("fileMetadata.FolderMetadata");

            if (remoteFile == null)
                throw new ArgumentNullException("remoteFile");

            if (remoteFile.Stream == null)
                throw new ArgumentNullException("remoteFile.Stream");

            this.Logger.WriteFormatMessage("WriteFileVersion:Начало записи новой версии файла, fileMetadata.Name: {0}, fileMetadata.UniqueID: {1}", fileMetadata.Name, fileMetadata.UniqueID);

            IBlobFileMetadata blobFileMetadata = (IBlobFileMetadata)fileMetadata;
            if (!string.IsNullOrEmpty(remoteFile.Name))
                blobFileMetadata.Name = remoteFile.Name;

            //резервация параметров сохранения
            blobFileMetadata.EnsureRemoteSaveProperties(remoteFile);

            //запись версии файла
            IFileVersionMetadata savedVersion = this.WriteInternal(blobFileMetadata, remoteFile.Stream, remoteFile.TimeCreated);

            return savedVersion;
        }
        #endregion
    }
}