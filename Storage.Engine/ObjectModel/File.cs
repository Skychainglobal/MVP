using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Storage.Lib;

namespace Storage.Engine
{
    /// <summary>
    /// Файл хранилища.
    /// </summary>
    internal class File : IFile
    {
        public File(StorageEngine storage, IFolder folder, IFileMetadata metadata)
        {
            if (storage == null)
                throw new ArgumentNullException("storage");

            if (folder == null)
                throw new ArgumentNullException("folder");

            if (metadata == null)
                throw new ArgumentNullException("metadata");

            this.TypedStorage = storage;
            this.Folder = folder;
            this.Metadata = metadata;
        }

        public File(StorageEngine storage, IFolder folder, IFileMetadata metadata, byte[] content)
            : this(storage, folder, metadata)
        {
            this.Content = content;
        }

        /// <summary>
        /// Типизированное хранилище файлов.
        /// </summary>
        public StorageEngine TypedStorage { get; private set; }

        /// <summary>
        /// Папка файла.
        /// </summary>
        public IFolder Folder { get; private set; }

        /// <summary>
        /// Метаданные файла.
        /// </summary>
        internal IFileMetadata Metadata { get; set; }

        private bool __init_Content;
        private byte[] _Content;
        /// <summary>
        /// Содержимое файла.
        /// </summary>
        public byte[] Content
        {
            get
            {
                if (!__init_Content)
                {
                    _Content = this.TypedStorage.DataAdapter.ReadFileContent(this.Metadata);
                    __init_Content = true;
                }
                return _Content;
            }
            private set
            {
                _Content = value;
                __init_Content = true;
            }
        }

        private bool __init_Name;
        private string _Name;
        /// <summary>
        /// Имя файла.
        /// </summary>
        public string Name
        {
            get
            {
                if (!__init_Name)
                {
                    _Name = this.Metadata.Name;
                    __init_Name = true;
                }
                return _Name;
            }
        }

        private bool __init_UniqueID;
        private Guid _UniqueID;
        /// <summary>
        /// Уникальный идентификатор файла.
        /// </summary>
        public Guid UniqueID
        {
            get
            {
                if (!__init_UniqueID)
                {
                    _UniqueID = this.Metadata.UniqueID;
                    __init_UniqueID = true;
                }
                return _UniqueID;
            }
        }

        private bool __init_VersionUniqueID;
        private Guid _VersionUniqueID;
        /// <summary>
        /// Уникальный идентификатор текущей версии файла.
        /// </summary>
        public Guid VersionUniqueID
        {
            get
            {
                if (!__init_VersionUniqueID)
                {
                    _VersionUniqueID = this.Metadata.VersionUniqueID;
                    __init_VersionUniqueID = true;
                }
                return _VersionUniqueID;
            }
        }

        /// <summary>
        /// Удаляет файл.
        /// </summary>
        public void Delete()
        {
            this.Storage.DeleteFile(this);
        }

        private bool __init_Extension;
        private string _Extension;
        /// <summary>
        /// Расширение файла.
        /// </summary>
        public string Extension
        {
            get
            {
                if (!__init_Extension)
                {
                    _Extension = Path.GetExtension(this.Name);
                    __init_Extension = true;
                }
                return _Extension;
            }
        }

        private bool __init_CurrentVersion;
        private IFileVersion _CurrentVersion;
        /// <summary>
        /// Объект текущей версии файла.
        /// </summary>
        internal IFileVersion CurrentVersion
        {
            get
            {
                if (!__init_CurrentVersion)
                {
                    _CurrentVersion = this.GetVersion(this.VersionUniqueID);
                    __init_CurrentVersion = true;
                }
                return _CurrentVersion;
            }
        }

        /// <summary>
        /// Возвращает версию файла.
        /// </summary>
        /// <param name="versionUniqueID">Уникальный идентификатор версии.</param>
        /// <param name="throwIfNotexists">Выбросить исключение, если версии не существует?</param>
        /// <returns></returns>
        public IFileVersion GetVersion(Guid versionUniqueID, bool throwIfNotexists = true)
        {
            if (versionUniqueID == Guid.Empty)
                throw new ArgumentNullException("versionUniqueID");

            IFileVersion version = null;
            if (this.VersionsByID.ContainsKey(versionUniqueID))
                version = this.VersionsByID[versionUniqueID];

            if (throwIfNotexists && version == null)
                throw new Exception(string.Format("Не удалось найти версию с идентификатором {0}",
                    versionUniqueID));

            return version;
        }

        /// <summary>
        /// Размер файла.
        /// </summary>
        public long Size
        {
            get { return this.Metadata.Size; }
        }

        /// <summary>
        /// Хранилище.
        /// </summary>
        public IStorage Storage
        {
            get { return this.Folder.Storage; }
        }

        /// <summary>
        /// Дата создания файла.
        /// </summary>
        public DateTime TimeCreated
        {
            get { return this.Metadata.TimeCreated; }
        }

        /// <summary>
        /// Дата изменения файла.
        /// </summary>
        public DateTime TimeModified
        {
            get { return this.Metadata.TimeModified; }
        }

        /// <summary>
        /// Обновляет содержимое файла.
        /// </summary>
        /// <param name="stream">Новое содержимое файла.</param>
        /// <param name="fileName">Имя файла.</param>
        public void Update(Stream stream, string fileName = null)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            this.TypedStorage.UpdateFileInternal(this, stream, fileName);
        }

        private bool __init_FolderUrl;
        private string _FolderUrl;
        /// <summary>
        /// Адрес папки файла.
        /// </summary>
        public string FolderUrl
        {
            get
            {
                if (!__init_FolderUrl)
                {
                    _FolderUrl = this.Folder.Url;
                    __init_FolderUrl = true;
                }
                return _FolderUrl;
            }
        }

        private bool __init_Url;
        private string _Url;
        /// <summary>
        /// Адрес файла.
        /// </summary>
        public string Url
        {
            get
            {
                if (!__init_Url)
                {
                    _Url = string.Format("{0}/{1}",
                        this.Folder.Url,
                        this.UniqueID);

                    __init_Url = true;
                }
                return _Url;
            }
        }

        private bool __init_VersionsByID;
        private Dictionary<Guid, IFileVersion> _VersionsByID;
        /// <summary>
        /// Словарь версий по идентификатору.
        /// </summary>
        private Dictionary<Guid, IFileVersion> VersionsByID
        {
            get
            {
                if (!__init_VersionsByID)
                {
                    _VersionsByID = new Dictionary<Guid, IFileVersion>();

                    this.TypedStorage.Logger.WriteFormatMessage("Начало построения словаря версий файла {0} ({1})", this.Name, this.UniqueID);

                    ICollection<IFileVersionMetadata> versionsMetadata = this.TypedStorage.DataAdapter.ReadFileVersionsMetadata(this.Metadata);
                    if (versionsMetadata != null)
                    {
                        foreach (IFileVersionMetadata versionMetadata in versionsMetadata)
                        {
                            IFileVersion fileVersion = new FileVersion(this.TypedStorage, this, versionMetadata);
                            if (!_VersionsByID.ContainsKey(fileVersion.UniqueID))
                                _VersionsByID.Add(fileVersion.UniqueID, fileVersion);
                        }
                    }

                    this.TypedStorage.Logger.WriteFormatMessage("Окончание построения словаря версий файла {0} ({1})", this.Name, this.UniqueID);

                    __init_VersionsByID = true;
                }
                return _VersionsByID;
            }
        }

        /// <summary>
        /// Коллекция версий файла.
        /// </summary>
        public IReadOnlyCollection<IFileVersion> Versions
        {
            get { return this.VersionsByID.Values.ToList().AsReadOnly(); }
        }

        /// <summary>
        /// Освобождает ресурсы файла.
        /// </summary>
        public void Dispose()
        {
            if (_Content != null)
                this.Content = null;

            if (_VersionsByID != null)
            {
                foreach (IFileVersion version in this.VersionsByID.Values)
                {
                    version.Dispose();
                }
            }
        }

        /// <summary>
        /// Открывает поток чтения данных файла.
        /// </summary>
        /// <returns></returns>
        public Stream Open()
        {
            return this.TypedStorage.OpenFile(this);
        }
    }
}