using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Storage.Metadata.MSSQL;
using Storage.Data.Blob;
using Storage.Engine;

namespace Storage.Metadata.Blob.MSSQL
{
    /// <summary>
    /// Представляет адаптер для работы с метаданными blob-файлов, документов, хранящихся в blob а также сопоставленных им объектов метаданных.
    /// </summary>
    public class BlobMetadataAdapter : IBlobMetadataAdapter
    {
        /// <summary>
        /// К-тор.
        /// </summary>
        /// <param name="metadataAdapter">Адаптер работы с метаданными системы.</param>
        public BlobMetadataAdapter(MetadataAdapter metadataAdapter)
        {
            if (metadataAdapter == null)
                throw new ArgumentNullException("metadataAdapter");

            this.TypedMetadataAdapter = metadataAdapter;
        }

        public IMetadataAdapter MetadataAdapter
        {
            get { return this.MetadataAdapter; }
        }

        private MetadataAdapter _TypedMetadataAdapter;
        /// <summary>
        /// Адаптер работы с метаданными системы.
        /// </summary>
        public MetadataAdapter TypedMetadataAdapter
        {
            get { return _TypedMetadataAdapter; }
            set { _TypedMetadataAdapter = value; }
        }

        private bool __init_ContainerAdapter = false;
        private BlobContainerAdapter _ContainerAdapter;
        /// <summary>
        /// Адаптер работы с метаданными контейнеров blob.
        /// </summary>
        private BlobContainerAdapter ContainerAdapter
        {
            get
            {
                if (!__init_ContainerAdapter)
                {
                    _ContainerAdapter = new BlobContainerAdapter(this.TypedMetadataAdapter);
                    __init_ContainerAdapter = true;
                }
                return _ContainerAdapter;
            }
        }

        private bool __init_BlobAdapter = false;
        private BlobAdapter _BlobAdapter;
        /// <summary>
        /// Адаптер работы с метаданными blob.
        /// </summary>
        private BlobAdapter BlobAdapter
        {
            get
            {
                if (!__init_BlobAdapter)
                {
                    _BlobAdapter = new BlobAdapter(this.TypedMetadataAdapter);

                    __init_BlobAdapter = true;
                }
                return _BlobAdapter;
            }
        }

        private bool __init_FileAdapter = false;
        private FileAdapter _FileAdapter;
        /// <summary>
        /// Адаптер работы с метаданными файлов.
        /// </summary>
        public FileAdapter FileAdapter
        {
            get
            {
                if (!__init_FileAdapter)
                {
                    _FileAdapter = new FileAdapter(this.TypedMetadataAdapter);
                    __init_FileAdapter = true;
                }
                return _FileAdapter;
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
                    _Logger = ConfigFactory.Instance.Create<ILogProvider>(BlobMetadataConsts.Scopes.BlobMetadataAdapter);
                    __init_Logger = true;
                }
                return _Logger;
            }
        }


        #region BlobContainers

        private bool __init_DefaultContainers = false;
        private ICollection<IBlobContainerMetadata> _DefaultContainers;
        /// <summary>
        /// 
        /// </summary>
        public ICollection<IBlobContainerMetadata> DefaultContainers
        {
            get
            {
                if (!__init_DefaultContainers)
                {
                    this.Logger.WriteMessage("DefaultContainers_get: Начало.");

                    List<BlobContainerMetadata> containers = this.ContainerAdapter.GetBlobContainers("[FolderID] = 0 AND ([Closed] = 0 OR [Closed] IS NULL)");
                    _DefaultContainers = containers.Select(x => (IBlobContainerMetadata)x).ToList();

                    this.Logger.WriteMessage("DefaultContainers_get: Конец.");

                    __init_DefaultContainers = true;
                }
                return _DefaultContainers;
            }
        }

        public IBlobContainerMetadata CreateBlobContainer(string name, string path, int folderID)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException("path");

            BlobContainerMetadata metadata = BlobContainerMetadata.Create(name, path, folderID);
            return metadata;
        }

        public ICollection<IBlobContainerMetadata> GetBlobContainers()
        {
            this.Logger.WriteFormatMessage("GetBlobContainers: Начало.");

            List<BlobContainerMetadata> containers = this.ContainerAdapter.GetBlobContainers("([Closed] = 0 OR [Closed] IS NULL)");

            this.Logger.WriteMessage("GetBlobContainers: Конец.");

            return containers.Select(x => (IBlobContainerMetadata)x).ToList();
        }

        public ICollection<IBlobContainerMetadata> GetBlobContainers(int folderID)
        {
            this.Logger.WriteFormatMessage("GetBlobContainers: Начало. folderID='{0}'", folderID);

            List<BlobContainerMetadata> containers = this.ContainerAdapter.GetBlobContainers(String.Format("[FolderID] = {0} AND ([Closed] = 0 OR [Closed] IS NULL)", folderID));

            this.Logger.WriteMessage("GetBlobContainers: Конец.");

            return containers.Select(x => (IBlobContainerMetadata)x).ToList();
        }

        public IBlobContainerMetadata GetBlobContainer(int id)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException("id");

            this.Logger.WriteFormatMessage("GetBlobContainer: Начало. id='{0}'", id);

            BlobContainerMetadata blobContainer = this.ContainerAdapter.GetBlobContainer(String.Format("[ID] = {0}", id));

            this.Logger.WriteMessage("GetBlobContainer: Конец.");

            if (blobContainer != null)
                return (IBlobContainerMetadata)blobContainer;

            return null;
        }

        public void SaveBlobContainer(IBlobContainerMetadata containerMetadata)
        {
            if (containerMetadata == null)
                throw new ArgumentNullException("containerMetadata");

            this.Logger.WriteMessage("SaveBlobContainer: Начало.");

            this.ContainerAdapter.UpdateBlobContainer((BlobContainerMetadata)containerMetadata);

            this.Logger.WriteMessage("SaveBlobContainer: Конец.");
        }

        #endregion


        #region Blobs

        public IBlobMetadata CreateBlob(string name, int containerID)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            if (containerID < 1)
                throw new ArgumentOutOfRangeException("containerID");

            BlobMetadata blob = BlobMetadata.Create(name, containerID);
            return (IBlobMetadata)blob;
        }

        public IBlobMetadata GetBlob(int containerID, string name)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            this.Logger.WriteFormatMessage("GetBlob: Начало. containerID='{0}' name='{1}'", containerID, name);

            BlobMetadata blob = this.BlobAdapter.GetBlob(String.Format("[ContainerID] = {0} AND [Name] = N'{1}'", containerID, name));

            this.Logger.WriteMessage("GetBlob: Конец.");

            if (blob != null)
                return (IBlobMetadata)blob;
            else
                return null;
        }

        public IBlobMetadata GetBlob(int id)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException("id");

            this.Logger.WriteFormatMessage("GetBlob: Начало. id='{0}'", id);

            BlobMetadata blob = this.BlobAdapter.GetBlob(String.Format("[ID] = {0}", id));

            this.Logger.WriteMessage("GetBlob: Конец.");

            if (blob != null)
                return (IBlobMetadata)blob;
            else
                return null;
        }

        public ICollection<IBlobMetadata> GetBlobs(int containerID)
        {
            if (containerID < 1)
                throw new ArgumentNullException("containerID");

            this.Logger.WriteFormatMessage("GetBlobs: Начало. containerID='{0}'", containerID);

            List<BlobMetadata> blobs = this.BlobAdapter.GetBlobs(String.Format("[ContainerID] = {0}", containerID));

            this.Logger.WriteMessage("GetBlobs: Конец.");

            return blobs.Select(x => (IBlobMetadata)x).ToList();
        }

        /// <summary>
        /// Возвращает коллекцию активных блобов контейнера.
        /// </summary>
        /// <param name="containerID">Идентификатор контейнера.</param>
        /// <returns></returns>
        public ICollection<IBlobMetadata> GetActiveBlobs(int containerID)
        {
            if (containerID < 1)
                throw new ArgumentNullException("containerID");

            this.Logger.WriteFormatMessage("GetActiveBlobs: Начало. containerID='{0}'", containerID);

            List<BlobMetadata> blobs = this.BlobAdapter.GetBlobs(string.Format("[ContainerID] = {0} AND ([Closed] = 0 OR [Closed] IS NULL)", containerID));

            this.Logger.WriteMessage("GetActiveBlobs: Конец.");

            return blobs.Select(x => (IBlobMetadata)x).ToList();
        }

        public void SaveBlob(IBlobMetadata blobMetadata)
        {
            if (blobMetadata == null)
                throw new ArgumentNullException("blobMetadata");

            this.Logger.WriteMessage("SaveBlob: Начало.");

            this.BlobAdapter.UpdateBlob((BlobMetadata)blobMetadata);

            this.Logger.WriteMessage("SaveBlob: Конец.");
        }

        #endregion


        #region Files

        public IBlobFileMetadata CreateFile(IFolderMetadata folderMetadata, string fileName)
        {
            return this.FileAdapter.CreateFile(folderMetadata, fileName);
        }

        public IBlobFileMetadata GetFile(Engine.IFolderMetadata folderMetadata, Guid fileUniqueID)
        {
            if (folderMetadata == null)
                throw new ArgumentNullException("folderMetadata");

            if (fileUniqueID == Guid.Empty)
                throw new ArgumentNullException("fileUniqueID");

            this.Logger.WriteFormatMessage("GetFile: Начало. fileUniqueID='{0}' folderUrl='{1}'",
                fileUniqueID, folderMetadata.Url);

            FileMetadata file = this.FileAdapter.GetFile(fileUniqueID, folderMetadata);
            this.Logger.WriteMessage("GetFile: Конец.");

            return file;
        }

        public IBlobFileVersionMetadata SaveFile(IBlobFileMetadata fileMetadata, DateTime versionTimeCreated)
        {
            if (fileMetadata == null)
                throw new ArgumentNullException("fileMetadata");

            if (versionTimeCreated == null)
                throw new ArgumentNullException("versionTimeCreated");

            FileMetadata typedFile = (FileMetadata)fileMetadata;
            if (typedFile.ID != 0 && typedFile.RemoteFile == null)
            {
                if (typedFile.OriginalVersionUniqueID == Guid.Empty || fileMetadata.VersionUniqueID == typedFile.OriginalVersionUniqueID)
                    throw new Exception("Для обновления существующего файла необходимо вызвать резервирование параметров сохранения.");
            }

            this.Logger.WriteMessage("SaveFile: Начало.");

            FileVersionMetadata version = this.FileAdapter.UpdateFile(typedFile, versionTimeCreated);

            this.Logger.WriteMessage("SaveFile: Конец.");

            return version;
        }

        public void DeleteFile(IBlobFileMetadata fileMetadata)
        {
            if (fileMetadata == null)
                throw new ArgumentNullException("fileMetadata");

            this.Logger.WriteMessage("DeleteFile: Начало.");

            this.FileAdapter.DeleteFile((FileMetadata)fileMetadata);

            this.Logger.WriteMessage("DeleteFile: Конец.");
        }

        #endregion


        #region ExistsFiles
        /// <summary>
        /// Создает метаданные существующего файла.
        /// </summary>
        /// <param name="storageMetadata">Метаданные хранилища.</param>
        /// <param name="blobMetadata">Метаданные блоба.</param>
        /// <param name="folderMetadata">Метаданные папки.</param>
        /// <param name="fileHeader">Заголовок файла.</param>
        /// <param name="blobStartPosition">Начальная позиция файла в блобе.</param>
        /// <param name="blobEndPosition">Конечная позиция файла в блобе.</param>
        /// <returns></returns>
        public IBlobFileMetadata AddExistsFileVersion(IStorageMetadata storageMetadata, IBlobMetadata blobMetadata, IFolderMetadata folderMetadata, IFileHeader fileHeader, long blobStartPosition, long blobEndPosition)
        {
            if (storageMetadata == null)
                throw new ArgumentNullException("storageMetadata");

            if (blobMetadata == null)
                throw new ArgumentNullException("blobMetadata");

            if (folderMetadata == null)
                throw new ArgumentNullException("folderMetadata");

            if (fileHeader == null)
                throw new ArgumentNullException("fileHeader");

            if (blobStartPosition < 0)
                throw new ArgumentNullException("blobStartPosition");

            IBlobFileHeader blobHeader = (IBlobFileHeader)((object)fileHeader);
            FileMetadata file = this.FileAdapter.GetFile(fileHeader.UniqueID, folderMetadata);
            bool justCreated = false;

            if (file == null)
            {
                justCreated = true;
                file = new FileMetadata(this.FileAdapter);
                file.UniqueID = fileHeader.UniqueID;
                file.VersionUniqueID = fileHeader.VersionUniqueID;
                file.FolderID = folderMetadata.ID;
                file.FolderMetadata = folderMetadata;
                file.Name = fileHeader.FileName;

                file.BlobID = blobMetadata.ID;
                file.BlobStartPosition = blobStartPosition;
                file.BlobEndPosition = blobEndPosition;
                file.Deleted = false;
                file.Size = blobHeader.ContentLength;
                file.TimeCreated = fileHeader.TimeCreated;
                file.TimeModified = fileHeader.TimeCreated;

                this.FileAdapter.UpdateFileTransparent(file);
            }
            else
            {
                if (file.Versions.Any(x => x.UniqueID == fileHeader.VersionUniqueID))
                    throw new Exception(String.Format("Версия с идентификатором [{0}] уже существует.", fileHeader.VersionUniqueID.ToString()));
            }

            //создание версии файла
            FileVersionMetadata version = new FileVersionMetadata(file);
            version.UniqueID = fileHeader.VersionUniqueID;
            version.BlobID = blobMetadata.ID;
            version.BlobStartPosition = blobStartPosition;
            version.BlobEndPosition = blobEndPosition;
            version.Size = blobHeader.ContentLength;
            version.TimeCreated = fileHeader.TimeCreated;
            version.CreatedStorageID = storageMetadata.ID;
            version.Name = fileHeader.FileName;

            //сохранение версии
            this.FileAdapter.VersionAdapter.InsertVerion(file, version);
            file.ResetVersions();

            //обновление параметров существующего файла
            if (!justCreated)
            {
                bool fileUpdate = false;
                if (version.TimeCreated > file.TimeModified)
                {
                    file.TimeModified = version.TimeCreated;
                    file.VersionUniqueID = version.UniqueID;
                    file.BlobID = version.BlobID;
                    fileUpdate = true;
                }

                if (version.TimeCreated < file.TimeCreated)
                {
                    file.TimeCreated = version.TimeCreated;
                    fileUpdate = true;
                }

                //обновление файла
                if (fileUpdate)
                    this.FileAdapter.UpdateFileTransparent(file);
            }

            return file;
        }

        #endregion

        #region FileFersions

        public ICollection<IBlobFileVersionMetadata> GetVersions(IBlobFileMetadata fileMetadata)
        {
            if (fileMetadata == null)
                throw new ArgumentNullException("fileMetadata");

            this.Logger.WriteFormatMessage("GetVersions: Начало. fileUniqueID='{0}' folderUrl='{1}'",
                fileMetadata.UniqueID, fileMetadata.FolderMetadata.Url);

            FileVersionsCollection collection = ((FileMetadata)fileMetadata).Versions;

            this.Logger.WriteMessage("GetVersions: Конец.");

            return collection;
        }

        #endregion


        public void Dispose()
        {
            //TODO [AO] ???
        }
    }
}
