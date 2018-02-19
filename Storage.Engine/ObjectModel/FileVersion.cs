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
    /// Версия файла.
    /// </summary>
    internal class FileVersion : IFileVersion
    {
        public FileVersion(StorageEngine storage, File file, IFileVersionMetadata metadata)
        {
            if (storage == null)
                throw new ArgumentNullException("storage");

            if (file == null)
                throw new ArgumentNullException("file");

            if (metadata == null)
                throw new ArgumentNullException("metadata");

            this.TypedStorage = storage;
            this.TypedFile = file;
            this.Metadata = metadata;
        }

        /// <summary>
        /// Хранилище.
        /// </summary>
        public StorageEngine TypedStorage { get; private set; }

        /// <summary>
        /// Файл.
        /// </summary>
        public File TypedFile { get; private set; }

        /// <summary>
        /// Метаданные версии
        /// </summary>
        public IFileVersionMetadata Metadata { get; private set; }

        private bool __init_Content;
        private byte[] _Content;
        /// <summary>
        /// Содержимое версии.
        /// </summary>
        public byte[] Content
        {
            get
            {
                if (!__init_Content)
                {
                    _Content = this.TypedStorage.DataAdapter.ReadFileVersionContent(this.Metadata);
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

        /// <summary>
        /// Файл.
        /// </summary>
        public IFile File
        {
            get { return this.TypedFile; }
        }

        /// <summary>
        /// Папка.
        /// </summary>
        public IFolder Folder
        {
            get { return this.File.Folder; }
        }

        /// <summary>
        /// Имя файла версии.
        /// </summary>
        public string Name
        {
            get { return this.Metadata.Name; }
        }

        /// <summary>
        /// Размер версии.
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
            get { return this.TypedStorage; }
        }

        /// <summary>
        /// Дата создания.
        /// </summary>
        public DateTime TimeCreated
        {
            get { return this.Metadata.TimeCreated; }
        }

        /// <summary>
        /// Уникальный идентификатор.
        /// </summary>
        public Guid UniqueID
        {
            get { return this.Metadata.UniqueID; }
        }

        private bool __init_IsCurrent;
        private bool _IsCurrent;
        /// <summary>
        /// Является текущей?
        /// </summary>
        public bool IsCurrent
        {
            get
            {
                if (!__init_IsCurrent)
                {
                    _IsCurrent = this.UniqueID == this.File.VersionUniqueID;
                    __init_IsCurrent = true;
                }
                return _IsCurrent;
            }
        }

        private bool __init_CreatedStorageID;
        private Guid _CreatedStorageID;
        /// <summary>
        /// Идентификатор хранилища, на котором была создана версия.
        /// </summary>
        public Guid CreatedStorageID
        {
            get
            {
                if (!__init_CreatedStorageID)
                {
                    _CreatedStorageID = this.Metadata.CreatedStorageUniqueID;
                    __init_CreatedStorageID = true;
                }
                return _CreatedStorageID;
            }
        }

        /// <summary>
        /// Освобождает ресурсы, занятые объектом версии файла.
        /// </summary>
        public void Dispose()
        {
            if (_Content != null)
                this.Content = null;
        }

        /// <summary>
        /// Открывает поток чтения данных версии файла.
        /// </summary>
        /// <returns></returns>
        public Stream Open()
        {
            return this.TypedStorage.OpenFileVersion(this);
        }
    }
}