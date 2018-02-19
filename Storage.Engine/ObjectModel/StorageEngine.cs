using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Storage.Lib;

namespace Storage.Engine
{
    /// <summary>
    /// Файловое хранилище.
    /// </summary>
    public class StorageEngine : IStorage
    {
        private bool __init_MetadataAdapter;
        private IMetadataAdapter _MetadataAdapter;
        /// <summary>
        /// Адаптер метаданных хранилища.
        /// </summary>
        public IMetadataAdapter MetadataAdapter
        {
            get
            {
                if (!__init_MetadataAdapter)
                {
                    _MetadataAdapter = this.DataAdapter.MetadataAdapter;
                    __init_MetadataAdapter = true;
                }
                return _MetadataAdapter;
            }
        }

        private bool __init_DataAdapter;
        private IDataAdapter _DataAdapter;
        /// <summary>
        /// Адаптер данных хранилища.
        /// </summary>
        public IDataAdapter DataAdapter
        {
            get
            {
                if (!__init_DataAdapter)
                {
                    _DataAdapter = ConfigFactory.Instance.Create<IDataAdapter>();
                    __init_DataAdapter = true;
                }
                return _DataAdapter;
            }
        }

        private bool __init_TypedReplicationAdapter;
        private ReplicationAdapter _TypedReplicationAdapter;
        private ReplicationAdapter TypedReplicationAdapter
        {
            get
            {
                if (!__init_TypedReplicationAdapter)
                {
                    _TypedReplicationAdapter = new ReplicationAdapter(this);
                    __init_TypedReplicationAdapter = true;
                }
                return _TypedReplicationAdapter;
            }
        }


        private bool __init_ReplicationAdapter;
        private IReplicationAdapter _ReplicationAdapter;
        /// <summary>
        /// Адаптер репликации.
        /// </summary>
        private IReplicationAdapter ReplicationAdapter
        {
            get
            {
                if (!__init_ReplicationAdapter)
                {
                    _ReplicationAdapter = this.TypedReplicationAdapter;
                    __init_ReplicationAdapter = true;
                }
                return _ReplicationAdapter;
            }
        }

        private bool __init_FileTokenAdapter;
        private IFileTokenAdapter _FileTokenAdapter;
        /// <summary>
        /// Адаптер файловых токенов.
        /// </summary>
        private IFileTokenAdapter FileTokenAdapter
        {
            get
            {
                if (!__init_FileTokenAdapter)
                {
                    _FileTokenAdapter = ConfigFactory.Instance.Create<IFileTokenAdapter>(this.MetadataAdapter);
                    __init_FileTokenAdapter = true;
                }
                return _FileTokenAdapter;
            }
        }

        private bool __init_SessionLinkResolver;
        private ISessionResolver _SessionLinkResolver;
        /// <summary>
        /// Адаптер сессионных ссылок.
        /// </summary>
        private ISessionResolver SessionLinkResolver
        {
            get
            {
                if (!__init_SessionLinkResolver)
                {
                    _SessionLinkResolver = ConfigFactory.Instance.Create<ISessionResolver>();
                    __init_SessionLinkResolver = true;
                }
                return _SessionLinkResolver;
            }
        }

        private bool __init_SecurityAdapter = false;
        private ISecurityAdapter _SecurityAdapter;
        /// <summary>
        /// 
        /// </summary>
        private ISecurityAdapter SecurityAdapter
        {
            get
            {
                if (!__init_SecurityAdapter)
                {
                    _SecurityAdapter = ConfigFactory.Instance.Create<ISecurityAdapter>();
                    __init_SecurityAdapter = true;
                }
                return _SecurityAdapter;
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
                    _Logger = ConfigFactory.Instance.Create<ILogProvider>(EngineConsts.Logs.Scopes.Engine);
                    __init_Logger = true;
                }
                return _Logger;
            }
        }

        private bool __init_Cache;
        private ICacheProvider _Cache;
        private ICacheProvider Cache
        {
            get
            {
                if (!__init_Cache)
                {
                    _Cache = MemoryCache.Current;
                    __init_Cache = true;
                }
                return _Cache;
            }
        }

        #region StorageInfo
        private bool __init_StorageInfo;
        private IStorageMetadata _StorageInfo;
        /// <summary>
        /// Метаданные хранилища.
        /// </summary>
        private IStorageMetadata StorageInfo
        {
            get
            {
                if (!__init_StorageInfo)
                {
                    this.Logger.WriteMessage("StorageInfo:Начало получения информации о хранилище");

                    _StorageInfo = this.MetadataAdapter.CurrentStorage;
                    if (_StorageInfo == null)
                        throw new Exception(string.Format("Не удалось найти текущий экземпляр файлового хранилища"));

                    this.Logger.WriteMessage("StorageInfo:Окончание получения информации о хранилище");

                    __init_StorageInfo = true;
                }
                return _StorageInfo;
            }
        }

        private bool __init_CurrentNode;
        private IStorageNode _CurrentNode;
        public IStorageNode CurrentNode
        {
            get
            {
                if (!__init_CurrentNode)
                {
                    _CurrentNode = new StorageNode(this, this.StorageInfo);
                    __init_CurrentNode = true;
                }
                return _CurrentNode;
            }
        }
        #endregion

        #region Folders
        private bool __init_DefaultFolder;
        private IFolder _DefaultFolder;
        /// <summary>
        /// Папка по-умолчанию.
        /// </summary>
        public IFolder DefaultFolder
        {
            get
            {
                if (!__init_DefaultFolder)
                {
                    this.Logger.WriteMessage("DefaultFolder:Начало получения папки по-умолчанию");

                    _DefaultFolder = this.EnsureFolder(StorageConsts.Folders.DefaultFolderName);

                    this.Logger.WriteMessage("DefaultFolder:Окончание получения папки по-умолчанию");
                    __init_DefaultFolder = true;
                }
                return _DefaultFolder;
            }
        }

        private bool __init_Folders;
        private IReadOnlyCollection<IFolder> _Folders;
        /// <summary>
        /// Папки файлового хранилища.
        /// </summary>
        public IReadOnlyCollection<IFolder> Folders
        {
            get
            {
                if (!__init_Folders)
                {
                    this.Logger.WriteMessage("Folders:Начало получения получения корневых папок хранилища");

                    List<IFolder> folders = new List<IFolder>();

                    ICollection<IFolderMetadata> foldersMetadata = this.MetadataAdapter.GetFolders();
                    foreach (IFolderMetadata folderMetadata in foldersMetadata)
                    {
                        IFolder rootFolder = new Folder(this, folderMetadata, null);
                        folders.Add(rootFolder);
                    }

                    _Folders = folders.AsReadOnly();

                    this.Logger.WriteMessage("Folders:Окончание получения получения корневых папок хранилища");
                    __init_Folders = true;
                }
                return _Folders;
            }
        }


        /// <summary>
        /// Возвращает папку по адресу.
        /// </summary>
        /// <param name="url">Адрес папки.</param>
        /// <param name="throwIfNotExists">Выбросить исключение, если папка не существует.</param>
        /// <returns></returns>
        public IFolder GetFolder(string url, bool throwIfNotExists = true)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException("url");

            this.Logger.WriteFormatMessage("GetFolder:Начало получения папки, url: {0}", url);
            Folder folder = this.GetFolderInternal(url, throwIfNotExists);
            this.Logger.WriteFormatMessage("GetFolder:Окончание получения папки, url: {0}", url);

            return folder;
        }

        /// <summary>
        /// Возвращает папку по адресу.
        /// </summary>
        /// <param name="url">Адрес папки.</param>
        /// <param name="throwIfNotExists">Выбросить исключение, если папка не существует.</param>
        /// <returns></returns>
        internal Folder GetFolderInternal(string url, bool throwIfNotExists = true)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException("url");

            this.Logger.WriteFormatMessage("GetFolderInternal:Начало получения папки, url: {0}", url);

            //построение и обход дерева сегментов url
            FolderUri folderUri = new FolderUri(url);
            List<FolderUri> segments = new List<FolderUri>();

            FolderUri tmpFolderUri = folderUri;
            while (tmpFolderUri != null)
            {
                segments.Add(tmpFolderUri);
                tmpFolderUri = tmpFolderUri.ParentUri;
            }

            //сортировка для обхода с корневой папки
            IOrderedEnumerable<FolderUri> sortedSegments = segments.OrderBy(s => s.Url.Length);
            ICollection<IFolderMetadata> foldersMetadata = this.MetadataAdapter.GetFolders(sortedSegments.Select(s => s.Url).ToArray());
            Dictionary<string, IFolderMetadata> foldersMetadataByUrl = new Dictionary<string, IFolderMetadata>();
            if (foldersMetadata != null)
            {
                foreach (IFolderMetadata folderMetadata in foldersMetadata)
                {
                    if (!foldersMetadataByUrl.ContainsKey(folderMetadata.Url))
                        foldersMetadataByUrl.Add(folderMetadata.Url.ToLower(), folderMetadata);
                }
            }

            Folder resultFolder = null;
            Folder lastSegmentFolder = null;
            foreach (FolderUri segment in sortedSegments)
            {
                IFolderMetadata folderMetadata = null;
                string urlLower = segment.Url.ToLower();
                if (foldersMetadataByUrl.ContainsKey(urlLower))
                    folderMetadata = foldersMetadataByUrl[urlLower];

                //цепочка оборвалась
                if (folderMetadata == null)
                {
                    if (throwIfNotExists)
                        throw new Exception(string.Format("Не удалось найти папку по адресу {0}", segment.Url));
                    else
                    {
                        lastSegmentFolder = null;
                        break;
                    }
                }
                else
                    lastSegmentFolder = new Folder(this, folderMetadata, lastSegmentFolder);
            }
            resultFolder = lastSegmentFolder;

            this.Logger.WriteFormatMessage("GetFolderInternal:Окончание получения папки, url: {0}", url);

            return resultFolder;
        }


        /// <summary>
        /// Возвращает папку по адресу. Если папки не существует, то она будет создана.
        /// </summary>
        /// <param name="url">Адрес папки.</param>
        /// <returns></returns>
        public IFolder EnsureFolder(string url)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException("url");

            this.Logger.WriteFormatMessage("EnsureFolder:Начало получение папки, url: {0}", url);
            Folder folder = this.EnsureFolderInternal(url);
            this.Logger.WriteFormatMessage("EnsureFolder:Окончание получение папки, url: {0}", url);

            return folder;
        }

        /// <summary>
        /// Возвращает папку по адресу. Если папки не существует, то она будет создана.
        /// </summary>
        /// <param name="url">Адрес папки.</param>
        /// <returns></returns>
        internal Folder EnsureFolderInternal(string url)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException("url");

            this.Logger.WriteFormatMessage("EnsureFolderInternal:Начало получение папки, url: {0}", url);
            Folder folder = null;
            FolderUri folderUri = new FolderUri(url);
            Folder rootFolder = this.EnsureFolder(folderUri.RootUri.Url, null);

            if (folderUri.IsRoot)
                folder = rootFolder;
            else
            {
                bool allChainCreated = false;
                FolderUri tmpFolderUri = folderUri;
                Folder lastCreatedFolder = rootFolder;

                while (!allChainCreated)
                {
                    while (tmpFolderUri.ParentUri != null && tmpFolderUri.ParentUri.Url != lastCreatedFolder.Url)
                    {
                        tmpFolderUri = tmpFolderUri.ParentUri;
                    }
                    FolderUri firstNotCreatedUrl = tmpFolderUri;
                    lastCreatedFolder = this.EnsureFolder(firstNotCreatedUrl.FolderName, lastCreatedFolder);

                    allChainCreated = lastCreatedFolder.Url == url;
                    tmpFolderUri = folderUri;
                }

                folder = lastCreatedFolder;
            }

            this.Logger.WriteFormatMessage("EnsureFolderInternal:Окончание получение папки, url: {0}", url);

            return folder;
        }

        /// <summary>
        /// Получает коллекцию папок.
        /// </summary>
        /// <param name="parentFolder">Родительская папка.</param>
        /// <returns></returns>
        internal List<Folder> GetFolders(Folder parentFolder = null)
        {
            this.Logger.WriteMessage("GetFolders:Начало получения дочерних папок");

            List<Folder> folders = new List<Folder>();
            int parentFolderID = 0;
            if (parentFolder != null)
                parentFolderID = parentFolder.ID;

            this.Logger.WriteFormatMessage("GetFolders:Идентификатор родительской папки: {0}", parentFolderID.ToString());

            ICollection<IFolderMetadata> foldersMetadata = this.MetadataAdapter.GetFolders(parentFolderID);
            if (foldersMetadata != null)
            {
                foreach (IFolderMetadata folderMetadata in foldersMetadata)
                {
                    Folder folder = new Folder(this, folderMetadata, parentFolder);
                    folders.Add(folder);
                }
            }

            this.Logger.WriteMessage("GetFolders:Окончание получения дочерних папок");

            return folders;
        }

        /// <summary>
        /// Возвращает папку по имени.
        /// </summary>
        /// <param name="name">Имя папки.</param>
        /// <param name="parentFolder">Дочерняя папка.</param>
        /// <returns></returns>
        internal Folder EnsureFolder(string name, Folder parentFolder = null)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            this.Logger.WriteFormatMessage("EnsureFolder:Начало получения папки, name: {0}", name);
            FolderUri folderUri;
            if (parentFolder != null)
                folderUri = new FolderUri(name, parentFolder.Url);
            else
                folderUri = new FolderUri(name, null);

            string key = string.Format("EnsureFolder_{0}", folderUri.Url);
            Folder folder = this.Cache.GetObject<Folder>(key);
            if (folder == null)
            {
                IFolderMetadata folderMetadata = this.MetadataAdapter.EnsureFolder(folderUri.Url);
                folder = new Folder(this, folderMetadata, parentFolder);
                this.Cache.AddObject(key, folder, EngineConsts.Lifetimes.Folder);
            }

            this.Logger.WriteFormatMessage("EnsureFolder:Окончание получения папки, name: {0}", name);

            return folder;
        }
        #endregion

        #region Files
        /// <summary>
        /// Загружает файл хранилище.
        /// </summary>
        /// <param name="folderUrl">Адрес папки, в которую необходимо загрузить файл.</param>
        /// <param name="fileName">Имя файла.</param>
        /// <param name="stream">Содержимое файла.</param>
        /// <returns></returns>
        public IFile UploadFile(string folderUrl, string fileName, Stream stream)
        {
            if (string.IsNullOrEmpty(folderUrl))
                throw new ArgumentNullException("folderUrl");

            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException("fileName");

            if (stream == null)
                throw new ArgumentNullException("stream");

            this.Logger.WriteFormatMessage("UploadFile:Начало загрузки файла, folderUrl: {0}, fileName: {1}", folderUrl, fileName);
            //получаем папку
            Folder folder = this.EnsureFolderInternal(folderUrl);
            File file = this.UploadFileInternal(folder, fileName, stream);

            this.Logger.WriteFormatMessage("UploadFile:Окончание загрузки файла, folderUrl: {0}, fileName: {1}", folderUrl, fileName);

            return file;
        }

        internal File UploadFileInternal(Folder folder, string fileName, Stream stream)
        {
            if (folder == null)
                throw new ArgumentNullException("folder");

            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException("fileName");

            if (stream == null)
                throw new ArgumentNullException("stream");

            this.Logger.WriteFormatMessage("UploadFile:Начало загрузки файла, folder.Url: {0}, fileName: {1}", folder.Url, fileName);
            this.Logger.WriteMessage("UploadFile:Начало записи файла");
            IFileVersionMetadata versionMetadata = this.DataAdapter.WriteFile(folder.Metadata, fileName, stream);
            File file = new File(this, folder, versionMetadata.FileMetadata);
            FileVersion version = new FileVersion(this, file, versionMetadata);
            this.Logger.WriteMessage("UploadFile:Окончание записи файла");

            this.Logger.WriteFormatMessage("UploadFile:Окончание загрузки файла, folder.Url: {0}, fileName: {1}", folder.Url, fileName);

            return file;
        }

        /// <summary>
        /// Обновляет содержимое файла.
        /// </summary>
        /// <param name="file">Файл.</param>
        /// <param name="stream">Содержимое файла.</param>
        /// <param name="fileName">Новое имя файла.</param>
        internal void UpdateFileInternal(File file, Stream stream, string fileName = null)
        {
            if (file == null)
                throw new ArgumentNullException("file");

            if (stream == null)
                throw new ArgumentNullException("stream");

            this.Logger.WriteFormatMessage("UpdateFileInternal:Начало обновления содержимого файла {0} ({1}). ", file.Name, file.UniqueID);
            IFileVersionMetadata versionMetadata = this.DataAdapter.WriteFileVersion(file.Metadata, stream, fileName);
            FileVersion version = new FileVersion(this, file, versionMetadata);
            this.Logger.WriteFormatMessage("UpdateFileInternal:Окончание обновления содержимого файла {0} ({1}).", file.Name, file.UniqueID);
        }

        /// <summary>
        /// Загружает файла в хранилище.
        /// </summary>
        /// <param name="fileName">Имя файла.</param>
        /// <param name="stream">Содержимое файла.</param>
        /// <returns></returns>
        public IFile UploadFile(string fileName, Stream stream)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException("fileName");

            if (stream == null)
                throw new ArgumentNullException("stream");

            this.Logger.WriteFormatMessage("UploadFile:Начало загрузки файла {0}", fileName);
            IFile file = this.UploadFile(StorageConsts.Folders.DefaultFolderName, fileName, stream);
            this.Logger.WriteFormatMessage("UploadFile:Окончание загрузки файла {0}, file.UniqueID: {1}", fileName, file.UniqueID);

            return file;
        }

        /// <summary>
        /// Возвращает файл из папки по умолчанию по уникальному идентификатору.
        /// </summary>
        /// <param name="fileUniqueID">Уникальный идентификатор файла.</param>
        /// <param name="loadOptions">Опции загрузки файла.</param>
        /// <param name="throwIfNotExists">Выдать ислючение если файл не найден.</param>
        /// <returns></returns>
        public IFile GetFile(Guid fileUniqueID, GetFileOptions loadOptions = null, bool throwIfNotExists = true)
        {
            if (fileUniqueID == null)
                throw new ArgumentNullException("fileUniqueID");

            this.Logger.WriteFormatMessage("GetFile:Начало получения файла, fileUniqueID: {0}", fileUniqueID);
            File file = this.GetFileInternal(fileUniqueID, loadOptions, throwIfNotExists);
            this.Logger.WriteFormatMessage("GetFile:Окончание получения файла, fileUniqueID: {0}", fileUniqueID);

            return file;
        }

        /// <summary>
        /// Возвращает файл из папки по умолчанию по уникальному идентификатору.
        /// </summary>
        /// <param name="fileUniqueID">Уникальный идентификатор файла.</param>
        /// <param name="loadOptions">Опции загрузки файла.</param>
        /// <param name="throwIfNotExists">Выдать ислючение если файл не найден.</param>
        /// <returns></returns>
        internal File GetFileInternal(Guid fileUniqueID, GetFileOptions loadOptions = null, bool throwIfNotExists = true)
        {
            if (fileUniqueID == null)
                throw new ArgumentNullException("fileUniqueID");

            this.Logger.WriteFormatMessage("GetFileInternal:Начало получения файла, fileUniqueID: {0}", fileUniqueID);
            File file = this.GetFileInternal(this.DefaultFolder.Url, fileUniqueID, loadOptions, throwIfNotExists);
            this.Logger.WriteFormatMessage("GetFileInternal:Окончание получения файла, fileUniqueID: {0}", fileUniqueID);

            return file;
        }

        /// <summary>
        /// Возвращает файл по адресу.
        /// </summary>
        /// <param name="fileUrl">Адрес файла.</param>
        /// <param name="loadOptions">Опции загрузки файла.</param>
        /// <param name="throwIfNotExists">Выдать ислючение если файл не найден.</param>
        /// <returns></returns>
        public IFile GetFile(string fileUrl, GetFileOptions loadOptions = null, bool throwIfNotExists = true)
        {
            if (string.IsNullOrEmpty(fileUrl))
                throw new ArgumentNullException("fileUrl");

            this.Logger.WriteFormatMessage("GetFile:Начало получения файла, fileUrl: {0}", fileUrl);
            FileUri fileUri = new FileUri(fileUrl);
            File file = this.GetFileInternal(fileUri, loadOptions, throwIfNotExists);
            this.Logger.WriteFormatMessage("GetFile:Окончание получения файла, fileUrl: {0}", fileUrl);

            return file;
        }

        /// <summary>
        /// Возвращает файл по адресу.
        /// </summary>
        /// <param name="fileUrl">Адрес файла.</param>
        /// <param name="loadOptions">Опции загрузки файла.</param>
        /// <param name="throwIfNotExists">Выдать ислючение если файл не найден.</param>
        /// <returns></returns>
        internal File GetFileInternal(string fileUrl, GetFileOptions loadOptions = null, bool throwIfNotExists = true)
        {
            if (string.IsNullOrEmpty(fileUrl))
                throw new ArgumentNullException("fileUrl");

            this.Logger.WriteFormatMessage("GetFileInternal:Начало получения файла, fileUrl: {0}", fileUrl);
            FileUri fileUri = new FileUri(fileUrl);
            File file = this.GetFileInternal(fileUri, loadOptions, throwIfNotExists);
            this.Logger.WriteFormatMessage("GetFileInternal:Окончание получения файла, fileUrl: {0}", fileUrl);

            return file;
        }

        /// <summary>
        /// Возвращает файл.
        /// </summary>
        /// <param name="folderUrl">Адрес папки с файлом.</param>
        /// <param name="fileUniqueID">Уникальный идентификатор файла.</param>
        /// <param name="loadOptions">Опции загрузки файла.</param>
        /// <param name="throwIfNotExists">Выдать ислючение если файл не найден.</param>
        /// <returns></returns>
        public IFile GetFile(string folderUrl, Guid fileUniqueID, GetFileOptions loadOptions = null, bool throwIfNotExists = true)
        {
            if (string.IsNullOrEmpty(folderUrl))
                throw new ArgumentNullException("folderUrl");

            if (fileUniqueID == Guid.Empty)
                throw new ArgumentNullException("fileUniqueID");

            this.Logger.WriteFormatMessage("GetFile:Начало получения файла, folderUrl: {0}, fileUniqueID: {1}", folderUrl, fileUniqueID);
            FileUri fileUri = new FileUri(folderUrl, fileUniqueID);
            File file = this.GetFileInternal(fileUri, loadOptions, throwIfNotExists);
            this.Logger.WriteFormatMessage("GetFile:Окончание получения файла, folderUrl: {0}, fileUniqueID: {1}", folderUrl, fileUniqueID);

            return file;
        }

        /// <summary>
        /// Возвращает файл.
        /// </summary>
        /// <param name="folderUrl">Адрес папки с файлом.</param>
        /// <param name="fileUniqueID">Уникальный идентификатор файла.</param>
        /// <param name="loadOptions">Опции загрузки файла.</param>
        /// <param name="throwIfNotExists">Выдать ислючение если файл не найден.</param>
        /// <returns></returns>
        internal File GetFileInternal(string folderUrl, Guid fileUniqueID, GetFileOptions loadOptions = null, bool throwIfNotExists = true)
        {
            if (string.IsNullOrEmpty(folderUrl))
                throw new ArgumentNullException("folderUrl");

            if (fileUniqueID == Guid.Empty)
                throw new ArgumentNullException("fileUniqueID");

            this.Logger.WriteFormatMessage("GetFileInternal:Начало получения файла, folderUrl: {0}, fileUniqueID: {1}", folderUrl, fileUniqueID);
            FileUri fileUri = new FileUri(folderUrl, fileUniqueID);
            File file = this.GetFileInternal(fileUri, loadOptions, throwIfNotExists);
            this.Logger.WriteFormatMessage("GetFileInternal:Окончание получения файла, folderUrl: {0}, fileUniqueID: {1}", folderUrl, fileUniqueID);

            return file;
        }

        /// <summary>
        /// Возвращает файл.
        /// </summary>
        /// <param name="fileUri">Адрес файла.</param>
        /// <param name="loadOptions">Опции загрузки файла.</param>
        /// <param name="throwIfNotExists">Выдать ислючение если файл не найден.</param>
        /// <returns></returns>
        internal File GetFileInternal(FileUri fileUri, GetFileOptions loadOptions = null, bool throwIfNotExists = true)
        {
            if (fileUri == null)
                throw new ArgumentNullException("fileUri");

            this.Logger.WriteFormatMessage("GetFileInternal:Начало получения файла, fileUri.Url: {0}", fileUri.Url);
            Folder folder = this.GetFolderInternal(fileUri.FolderUrl);
            IFileMetadata fileMetadata = this.DataAdapter.ReadFileMetadata(folder.Metadata, fileUri.FileUniqueID);

            File file = null;
            if (fileMetadata != null)
            {
                //чтение содержимого файла.
                byte[] content = null;
                if (loadOptions != null && loadOptions.LoadContent)
                {
                    this.Logger.WriteFormatMessage("GetFileInternal:Начало чтение содержимого файла, fileUri.Url: {0}", fileUri.Url);
                    content = this.DataAdapter.ReadFileContent(fileMetadata);
                    this.Logger.WriteFormatMessage("GetFileInternal:Окончание чтение содержимого файла, fileUri.Url: {0}", fileUri.Url);
                }

                file = new File(this, folder, fileMetadata, content);
            }

            if (throwIfNotExists && file == null)
                throw new Exception(string.Format("Не удалось найти файл по адресу {0}.", fileUri.Url));

            this.Logger.WriteFormatMessage("GetFileInternal:Окончание получения файла, fileUri.Url: {0}", fileUri.Url);

            return file;
        }

        /// <summary>
        /// Удаляет файл.
        /// </summary>
        /// <param name="folderUrl">Адрес папки.</param>
        /// <param name="fileUniqueID">Уникальный идентификатор файла.</param>
        /// <returns></returns>
        public bool DeleteFile(string folderUrl, Guid fileUniqueID)
        {
            if (string.IsNullOrEmpty(folderUrl))
                throw new ArgumentNullException("folderUrl");

            if (fileUniqueID == Guid.Empty)
                throw new ArgumentNullException("fileUniqueID");

            this.Logger.WriteFormatMessage("DeleteFile:Начало удаления файла, folderUrl: {0}, fileUniqueID: {1}", folderUrl, fileUniqueID);
            Folder folder = this.GetFolderInternal(folderUrl);
            bool result = this.DataAdapter.DeleteFile(folder.Metadata, fileUniqueID);
            this.Logger.WriteFormatMessage("DeleteFile:Окончание удаления файла, folderUrl: {0}, fileUniqueID: {1}", folderUrl, fileUniqueID);

            return result;
        }

        /// <summary>
        /// Удаляет файл.
        /// </summary>
        /// <param name="file">Файл.</param>
        /// <returns></returns>
        public void DeleteFile(IFile file)
        {
            if (file == null)
                throw new ArgumentNullException("file");

            this.Logger.WriteFormatMessage("DeleteFile:Начало удаления файла, file.Name: {0}, file.UniqueID: {1}", file.Name, file.UniqueID);
            File typedFile = (File)file;
            this.Logger.WriteFormatMessage("DeleteFile:Начало удаления файла из адаптера данных, file.Name: {0}, file.UniqueID: {1}", file.Name, file.UniqueID);
            this.DataAdapter.DeleteFile(typedFile.Metadata);
            this.Logger.WriteFormatMessage("DeleteFile:Окончание удаления файла из адаптера данных, file.Name: {0}, file.UniqueID: {1}", file.Name, file.UniqueID);

            this.Logger.WriteFormatMessage("DeleteFile:Окончание удаления файла, file.Name: {0}, file.UniqueID: {1}", file.Name, file.UniqueID);
        }
        #endregion

        #region SessionLinks

        /// <summary>
        /// Возвращает сессионную ссылку на версию файла.
        /// </summary>
        /// <param name="fileUrl">Адрес файла.</param>
        /// <param name="versionUniqueID">Уникальный идентификатор версии.</param>
        /// <returns></returns>
        public string GetSessionFileLink(string fileUrl, Guid versionUniqueID)
        {
            return this.GetSessionFileLink(fileUrl, versionUniqueID, null);
        }

        /// <summary>
        /// Возвращает сессионную ссылку на версию файла.
        /// </summary>
        /// <param name="folderUrl">Адрес папки.</param>
        /// <param name="fileUniqueID">Уникальный идентификатор файла.</param>
        /// <param name="versionUniqueID">Уникальный идентификатор версии.</param>
        /// <returns></returns>
        public string GetSessionFileLink(string folderUrl, Guid fileUniqueID, Guid versionUniqueID)
        {
            return this.GetSessionFileLink(folderUrl, fileUniqueID, versionUniqueID, null);
        }

        /// <summary>
        /// Возвращает сессионную ссылку на версию файла.
        /// </summary>
        /// <param name="fileUrl">Адрес файла.</param>
        /// <param name="versionUniqueID">Уникальный идентификатор версии.</param>
        /// <param name="userIdentity">Строка, идентифицирующая пользователя.</param>
        /// <returns></returns>
        public string GetSessionFileLink(string fileUrl, Guid versionUniqueID, string userIdentity)
        {
            if (string.IsNullOrEmpty(fileUrl))
                throw new ArgumentNullException("fileUrl");

            if (versionUniqueID == Guid.Empty)
                throw new ArgumentNullException("versionUniqueID");

            this.Logger.WriteFormatMessage("GetSessionFileLink:Начало генерации сессионной ссылки, fileUrl: {0}, versionUniqueID: {1}", fileUrl, versionUniqueID);

            File file = this.GetFileInternal(fileUrl);
            IFileVersion version = file.GetVersion(versionUniqueID);
            string sessionLink = this.GetSessionFileLinkInternal(version, userIdentity);

            this.Logger.WriteFormatMessage("GetSessionFileLink:Окончание генерации сессионной ссылки, fileUrl: {0}, versionUniqueID: {1}", fileUrl, versionUniqueID);

            return sessionLink;
        }

        /// <summary>
        /// Возвращает сессионную ссылку на версию файла.
        /// </summary>
        /// <param name="folderUrl">Адрес папки.</param>
        /// <param name="fileUniqueID">Уникальный идентификатор файла.</param>
        /// <param name="versionUniqueID">Уникальный идентификатор версии.</param>
        /// <param name="userIdentity">Строка, идентифицирующая пользователя.</param>
        /// <returns></returns>
        public string GetSessionFileLink(string folderUrl, Guid fileUniqueID, Guid versionUniqueID, string userIdentity)
        {
            if (string.IsNullOrEmpty(folderUrl))
                throw new ArgumentNullException("folderUrl");

            if (fileUniqueID == Guid.Empty)
                throw new ArgumentNullException("fileUniqueID");

            if (versionUniqueID == Guid.Empty)
                throw new ArgumentNullException("versionUniqueID");

            this.Logger.WriteFormatMessage("GetSessionFileLink:Начало генерации сессионной ссылки, folderUrl: {0}, fileUniqueID: {1}, versionUniqueID: {2}", folderUrl, fileUniqueID, versionUniqueID);

            File file = this.GetFileInternal(folderUrl, fileUniqueID);
            IFileVersion version = file.GetVersion(versionUniqueID);
            string sessionLink = this.GetSessionFileLinkInternal(version, userIdentity);

            this.Logger.WriteFormatMessage("GetSessionFileLink:Окончание генерации сессионной ссылки, folderUrl: {0}, fileUniqueID: {1}, versionUniqueID: {2}", folderUrl, fileUniqueID, versionUniqueID);

            return sessionLink;
        }

        private string GetSessionFileLinkInternal(IFileVersion version)
        {
            return this.GetSessionFileLinkInternal(version, null);
        }

        private string GetSessionFileLinkInternal(IFileVersion version, string userIdentity)
        {
            if (version == null)
                throw new ArgumentNullException("version");

            this.Logger.WriteFormatMessage("GetSessionFileLinkInternal:Начало генерации сессионной ссылки");

            FileVersion typedVersion = (FileVersion)version;

            string securityIdentifier = null;
            if (!String.IsNullOrEmpty(userIdentity))
            {
                this.Logger.WriteFormatMessage("SecurityAdapter.GetSecurityIdentifier:Начало генерации securityIdentifier, userIdentity: {0}", userIdentity);
                securityIdentifier = this.SecurityAdapter.GetSecurityIdentifier(userIdentity);
                this.Logger.WriteFormatMessage("SecurityAdapter.GetSecurityIdentifier:Окончание генерации securityIdentifier,userIdentity: {0}, securityIdentifier: {1}", userIdentity, securityIdentifier ?? String.Empty);
            }

            this.Logger.WriteFormatMessage("FileTokenAdapter.GenerateFileToken:Начало генерации токена для файла, version.Name: {0}, version.UniqueID: {1}", version.Name, version.UniqueID);
            IFileToken token = this.FileTokenAdapter.GenerateFileToken(typedVersion.Metadata, securityIdentifier);
            this.Logger.WriteFormatMessage("FileTokenAdapter.GenerateFileToken:Окончание генерации токена для файла, version.Name: {0}, version.UniqueID: {1}, token.ID: {2}", version.Name, version.UniqueID, token.UniqueID);

            this.Logger.WriteFormatMessage("SessionLinkResolver.GetSessionLink:Начало генерации сессионной ссылки на файл, version.Name: {0}, version.UniqueID: {1}", version.Name, version.UniqueID);
            string sessionLink = this.SessionLinkResolver.GetSessionLink(token);
            this.Logger.WriteFormatMessage("SessionLinkResolver.GetSessionLink:Окончание генерации сессионной ссылки на файл, version.Name: {0}, version.UniqueID: {1}, sessionLink: {2}", version.Name, version.UniqueID, sessionLink);

            this.Logger.WriteFormatMessage("GetSessionFileLinkInternal:Окончание генерации сессионной ссылки");

            return sessionLink;
        }

        /// <summary>
        /// Возвращает версию по сессионной ссылке.
        /// </summary>
        /// <param name="sessionLink">Сессионная ссылка.</param>
        /// <param name="identity">Идентификатор пользователя.</param>
        /// <returns></returns>
        public IFileVersion ResolveSessionLink(string sessionLink, IIdentity identity)
        {
            if (string.IsNullOrEmpty(sessionLink))
                throw new ArgumentNullException("sessionLink");

            if (identity == null)
                throw new ArgumentNullException("identity");

            this.Logger.WriteFormatMessage("ResolveSessionLink:Начало разрешения сессионной ссылки, sessionLink: {0}", sessionLink);

            IFileToken sessionToken = this.SessionLinkResolver.Resolve(sessionLink);
            if (string.IsNullOrEmpty(sessionToken.FolderUrl))
                throw new Exception(string.Format("sessionToken.FolderUrl"));

            if (sessionToken.UniqueID == Guid.Empty)
                throw new ArgumentNullException("sessionToken.UniqueID");

            if (sessionToken.FileUniqueID == Guid.Empty)
                throw new ArgumentNullException("sessionToken.FileUniqueID");

            if (sessionToken.VersionUniqueID == Guid.Empty)
                throw new ArgumentNullException("sessionToken.VersionUniqueID");

            this.Logger.WriteFormatMessage("ResolveSessionLink:Результат разрешения ссылки: sessionLink:{0}, sessionToken.UniqueID: {1}, sessionToken.FileUniqueID: {2}, sessionToken.VersionUniqueID: {3}, sessionToken.FolderUrl: {4}",
                sessionLink,
                sessionToken.UniqueID,
                sessionToken.FileUniqueID,
                sessionToken.VersionUniqueID,
                sessionToken.FolderUrl);

            //сначала нужно поднять версию файла
            File file = this.GetFileInternal(sessionToken.FolderUrl, sessionToken.FileUniqueID);
            FileVersion version = (FileVersion)file.GetVersion(sessionToken.VersionUniqueID);

            this.Logger.WriteFormatMessage("ResolveSessionLink:Разрешение сессионной ссылки, sessionLink: {0}, version.Name: {1}, version.UniqueID: {2}", sessionLink, version.Name, version.UniqueID);

            bool tokenValid = false;

            IFileToken fileToken = this.FileTokenAdapter.GetToken(version.Metadata, sessionToken.UniqueID);
            if (fileToken != null)
                tokenValid = this.SecurityAdapter.ValidateToken(fileToken, identity);

            this.Logger.WriteFormatMessage("ResolveSessionLink:Валидация сессионной ссылки, sessionLink: {0}, valid: {1}, identity.Name: {2}", sessionLink, tokenValid, identity.Name);

            if (!tokenValid)
                throw new Exception(string.Format("Токен не валиден"));

            this.Logger.WriteFormatMessage("ResolveSessionLink:Окончание разрешения сессионной ссылки, sessionLink: {0}", sessionLink);

            return version;
        }

        #endregion

        /// <summary>
        /// Осовобдает ресурсы хранилища.
        /// </summary>
        public void Dispose()
        {
            this.Logger.WriteMessage("Dispose:Начало освобождения ресурсов хранилища");

            if (_MetadataAdapter != null)
                this.MetadataAdapter.Dispose();

            if (_DataAdapter != null)
                this.DataAdapter.Dispose();

            if (_ReplicationAdapter != null)
                this.ReplicationAdapter.Dispose();

            this.Logger.WriteMessage("Dispose:Окончание освобождения ресурсов хранилища");
        }

        /// <summary>
        /// Инициализирует схему и настройки файлового хранилища.
        /// </summary>
        public void Init()
        {
            this.InitSchema();

            //вызов адаптера данных для инициализации
            //независимо от необходимости восстановления целостности
            IDataAdapter dataAdapter = this.DataAdapter;

            bool allowRestoring = ConfigReader.GetBooleanValue(EngineConsts.CongfigParams.AllowMetadataRestoring, false);
            if (allowRestoring)
            {
                //восстановление метаданных.
                dataAdapter.RestoreMetadata();
            }

            this.TypedReplicationAdapter.Init();
        }

        private void InitSchema()
        {
            object initConfig = ConfigurationManager.GetSection("StorageInitConfiguration");
            if (initConfig == null)
                throw new Exception(string.Format("Не задана секция инициализации конфигурации хранилища"));
            XmlNode initNode = (XmlNode)initConfig;

            string currentStorageHost = null;
            XmlAttribute currentStorageAttr = initNode.Attributes["CurrentStorage"];
            if (currentStorageAttr != null)
                currentStorageHost = currentStorageAttr.Value;

            if (string.IsNullOrEmpty(currentStorageHost))
                throw new Exception(string.Format("В настройках не задан адрес текущего узла хранилища."));

            bool requiredUpdate = true;
            IStorageMetadata currentStorage = this.MetadataAdapter.CurrentStorage;
            if (currentStorage == null)
                currentStorage = this.MetadataAdapter.GetStorage(currentStorageHost);
            else
            {
                //текущая нода есть в метаданных, проверяем соответствие хоста
                string metadataHost = currentStorage.Host;
                if (currentStorage.IsCurrent
                    && !string.IsNullOrEmpty(metadataHost)
                    && metadataHost.ToLower() == currentStorageHost.ToLower())
                    requiredUpdate = false;
            }

            if (requiredUpdate)
            {
                //нет ни текущей ноды, ни ноды с адресом из настройки
                if (currentStorage == null)
                {
                    //=> создаем текущую ноду
                    currentStorage = this.MetadataAdapter.CreateCurrentStorageNode(currentStorageHost);
                }
                else
                {
                    currentStorage.Host = currentStorageHost.ToLower();
                    currentStorage.IsCurrent = true;
                }

                this.MetadataAdapter.SaveStorage(currentStorage);
            }
        }

        #region Streaming
        /// <summary>
        /// Запускает потоковое чтение данных файла.
        /// </summary>
        /// <param name="folderUrl">Адрес папки.</param>
        /// <param name="fileUniqueID">Уникальный идентификатор файла.</param>
        /// <returns></returns>
        public Stream OpenFile(string folderUrl, Guid fileUniqueID)
        {
            if (string.IsNullOrEmpty(folderUrl))
                throw new ArgumentNullException("folderUrl");

            if (fileUniqueID == Guid.Empty)
                throw new ArgumentNullException("fileUniqueID");

            File file = this.GetFileInternal(this.DefaultFolder.Url, fileUniqueID);
            return this.DataAdapter.ReadFileStream(file.Metadata);
        }

        /// <summary>
        /// Запускает потоковое чтение данных версии файла.
        /// </summary>
        /// <param name="folderUrl">Адрес папки.</param>
        /// <param name="fileUniqueID">Уникальный идентификатор файла.</param>
        /// <param name="versionUniqueID">Уникальный идентификатор версии файла.</param>
        /// <returns></returns>
        public Stream OpenFile(string folderUrl, Guid fileUniqueID, Guid versionUniqueID)
        {
            if (string.IsNullOrEmpty(folderUrl))
                throw new ArgumentNullException("folderUrl");

            if (fileUniqueID == Guid.Empty)
                throw new ArgumentNullException("fileUniqueID");

            if (versionUniqueID == Guid.Empty)
                throw new ArgumentNullException("versionUniqueID");

            File file = this.GetFileInternal(this.DefaultFolder.Url, fileUniqueID);
            FileVersion version = (FileVersion)file.GetVersion(versionUniqueID);
            return this.DataAdapter.ReadFileVersionStream(version.Metadata);
        }

        /// <summary>
        /// Запускает потоковое чтение данных файла.
        /// </summary>
        /// <param name="file">Файл.</param>
        /// <returns></returns>
        internal Stream OpenFile(File file)
        {
            if (file == null)
                throw new ArgumentNullException("file");

            return this.DataAdapter.ReadFileStream(file.Metadata);
        }

        /// <summary>
        /// Запускает потоковое чтение данных версии файла.
        /// </summary>
        /// <param name="version">Версия файла.</param>
        /// <returns></returns>
        internal Stream OpenFileVersion(FileVersion version)
        {
            if (version == null)
                throw new ArgumentNullException("version");

            return this.DataAdapter.ReadFileVersionStream(version.Metadata);
        }
        #endregion
    }
}