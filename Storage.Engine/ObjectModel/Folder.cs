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
    /// Папка файлового хранилища.
    /// </summary>
    internal class Folder : IFolder, IEngineObjectMetadata
    {
        public Folder(StorageEngine storage, IFolderMetadata metadata, IFolder parentFolder)
        {
            if (storage == null)
                throw new ArgumentNullException("storage");

            if (metadata == null)
                throw new ArgumentNullException("metadata");

            this.TypedStorage = storage;
            this.Metadata = metadata;
            this.ParentFolder = parentFolder;
        }

        /// <summary>
        /// Типизированное файловое хранилище.
        /// </summary>
        internal StorageEngine TypedStorage { get; private set; }

        /// <summary>
        /// Метаданные папки.
        /// </summary>
        internal IFolderMetadata Metadata { get; private set; }

        /// <summary>
        /// Родительская папка.
        /// </summary>
        public IFolder ParentFolder { get; private set; }

        private bool __init_ID;
        private int _ID;
        /// <summary>
        /// Локальный идентификатор папки.
        /// </summary>
        public int ID
        {
            get
            {
                if (!__init_ID)
                {
                    _ID = this.Metadata.ID;
                    __init_ID = true;
                }
                return _ID;
            }
        }

        private bool __init_ParentFolderID;
        private int _ParentFolderID;
        /// <summary>
        /// Локальный идентификатор родительской папки.
        /// </summary>
        internal int ParentFolderID
        {
            get
            {
                if (!__init_ParentFolderID)
                {
                    _ParentFolderID = this.Metadata.ParentID;
                    __init_ParentFolderID = true;
                }
                return _ParentFolderID;
            }
        }

        /// <summary>
        /// Файловое хранилище.
        /// </summary>
        public IStorage Storage
        {
            get { return this.TypedStorage; }
        }

        /// <summary>
        /// Имя папки.
        /// </summary>
        public string Name
        {
            get { return this.Metadata.Name; }
        }

        /// <summary>
        /// Уникальный идентификатор папки.
        /// </summary>
        public Guid UniqueID
        {
            get { return this.Metadata.UniqueID; }
        }

        /// <summary>
        /// Адрес папки.
        /// </summary>
        public string Url
        {
            get { return this.Metadata.Url; }
        }


        private bool __init_FoldersByName;
        private Dictionary<string, IFolder> _FoldersByName;
        /// <summary>
        /// Словарь дочерних папок по имени.
        /// </summary>
        private Dictionary<string, IFolder> FoldersByName
        {
            get
            {
                if (!__init_FoldersByName)
                {
                    _FoldersByName = new Dictionary<string, IFolder>();

                    //дочерние папки
                    List<Folder> folders = this.TypedStorage.GetFolders(this);
                    foreach (Folder folder in folders)
                    {
                        if (!_FoldersByName.ContainsKey(folder.Name))
                            _FoldersByName.Add(folder.Name, folder);
                    }


                    __init_FoldersByName = true;
                }
                return _FoldersByName;
            }
        }

        private bool __init_Folders;
        private IReadOnlyCollection<IFolder> _Folders;
        /// <summary>
        /// Дочерние папки.
        /// </summary>
        public IReadOnlyCollection<IFolder> Folders
        {
            get
            {
                if (!__init_Folders)
                {
                    _Folders = this.FoldersByName.Values.ToList().AsReadOnly();
                    __init_Folders = true;
                }
                return _Folders;
            }
        }

        /// <summary>
        /// Возвращает дочернюю папку по имени. Если папки не существует, то она будет создана.
        /// </summary>
        /// <param name="name">Имя дочерней папки.</param>
        /// <returns></returns>
        public IFolder EnsureFolder(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            IFolder folder = this.TypedStorage.EnsureFolder(name, this);
            this.AddFolderToCollection(folder);

            return folder;
        }

        /// <summary>
        /// Возвращает дочернюю папку по имени.
        /// </summary>
        /// <param name="name">Имя дочерней папки.</param>
        /// <param name="throwIfNotExists">Выбросить исключение, если папка не существует.</param>
        /// <returns></returns>
        public IFolder GetFolder(string name, bool throwIfNotExists = true)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            FolderUri folderUri = new FolderUri(name, this.Url);
            IFolder folder = this.TypedStorage.GetFolderInternal(folderUri.Url, throwIfNotExists);

            return folder;
        }

        /// <summary>
        /// Добавляет дочернюю папку в локальную коллекцию папок.
        /// </summary>
        /// <param name="folder">Дочерняя папка.</param>
        private void AddFolderToCollection(IFolder folder)
        {
            if (folder == null)
                throw new ArgumentNullException("folder");

            if (!this.FoldersByName.ContainsKey(folder.Name))
            {
                this.FoldersByName.Add(folder.Name, folder);
                __init_Folders = false;
            }
        }

        /// <summary>
        /// Возвращает файл.
        /// </summary>
        /// <param name="fileUniqueID">Уникальный идентификатор файла.</param>
        /// <param name="loadOptions">Опции загрузки файла.</param>
        /// <param name="throwIfNotExists">Выдать ислючение если файл не найден.</param>
        /// <returns></returns>
        public IFile GetFile(Guid fileUniqueID, GetFileOptions loadOptions = null, bool throwIfNotExists = true)
        {
            FileUri fileUri = new FileUri(this.Url, fileUniqueID);
            File file = this.TypedStorage.GetFileInternal(fileUri, loadOptions, throwIfNotExists);

            return file;
        }

        /// <summary>
        /// Загружает файл в хранилище.
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

            File file = this.TypedStorage.UploadFileInternal(this, fileName, stream);
            return file;
        }


        /// <summary>
        /// Освобождает ресурсы папки.
        /// </summary>
        public void Dispose()
        {
            this.FoldersByName.Clear();
        }
    }
}