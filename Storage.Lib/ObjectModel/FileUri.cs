using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storage.Lib
{
    /// <summary>
    /// Uri файла.
    /// </summary>
    public class FileUri
    {
        /// <summary>
        /// Создает Uri файла по адресу.
        /// </summary>
        /// <param name="fileUrl">Адрес файла.</param>
        public FileUri(string fileUrl)
        {
            if (string.IsNullOrEmpty(fileUrl))
                throw new ArgumentNullException("uri");

            this.Url = fileUrl;
        }

        /// <summary>
        /// Создает Uri файла.
        /// </summary>
        /// <param name="folderUrl">Адрес папки.</param>
        /// <param name="fileUniqueID">Уникальный идентификатор файла.</param>
        public FileUri(string folderUrl, Guid fileUniqueID)
        {
            if (string.IsNullOrEmpty(folderUrl))
                throw new ArgumentNullException("folderUrl");

            if (fileUniqueID == Guid.Empty)
                throw new ArgumentNullException("fileUniqueID");

            this.FolderUrl = folderUrl;
            this.FileUniqueID = fileUniqueID;
        }

        /// <summary>
        /// Создает Uri файла.
        /// </summary>
        /// <param name="folder">Папка.</param>
        /// <param name="fileUniqueID">Уникальный идентификатор папки.</param>
        public FileUri(IFolder folder, Guid fileUniqueID)
        {
            if (folder == null)
                throw new ArgumentNullException("folder");

            if (fileUniqueID == Guid.Empty)
                throw new ArgumentNullException("fileUniqueID");

            this.FolderUrl = folder.Url;
            this.FileUniqueID = fileUniqueID;
        }

        /// <summary>
        /// Создает Uri файла.
        /// </summary>
        /// <param name="file">Файл.</param>
        public FileUri(IFile file)
        {
            if (file == null)
                throw new ArgumentNullException("file");

            this.FolderUrl = file.Folder.Url;
            this.FileUniqueID = file.UniqueID;
            this.Url = file.Url;
        }

        private bool __init_Url;
        private string _Url;
        /// <summary>
        /// Полный адрес файла.
        /// </summary>
        public string Url
        {
            get
            {
                if (!__init_Url)
                {
                    _Url = string.Format("{0}/{1}",
                        this.FolderUrl,
                        this.FileUniqueID);

                    __init_Url = true;
                }
                return _Url;
            }
            private set
            {
                _Url = value;
                __init_Url = true;
            }
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
                    int index = this.Url.LastIndexOf('/');
                    if (index == -1)
                        throw new Exception(string.Format("Некорректный формат адреса файла: {0}", this.Url));

                    _FolderUrl = this.Url.Substring(0, index);

                    __init_FolderUrl = true;
                }
                return _FolderUrl;
            }
            private set
            {
                _FolderUrl = value;
                __init_FolderUrl = true;
            }
        }

        private bool __init_FileUniqueIDString;
        private string _FileUniqueIDString;
        /// <summary>
        /// Строка уникального идентификатора файла.
        /// </summary>
        private string FileUniqueIDString
        {
            get
            {
                if (!__init_FileUniqueIDString)
                {
                    int index = this.Url.LastIndexOf('/');
                    if (index == -1)
                        throw new Exception(string.Format("Некорректный формат адреса файла: {0}", this.Url));

                    _FileUniqueIDString = this.Url.Substring(index + 1);
                    __init_FileUniqueIDString = true;
                }
                return _FileUniqueIDString;
            }
        }

        private bool __init_FileUniqueID;
        private Guid _FileUniqueID;
        /// <summary>
        /// Уникальный идентификатор файла.
        /// </summary>
        public Guid FileUniqueID
        {
            get
            {
                if (!__init_FileUniqueID)
                {
                    _FileUniqueID = Guid.Parse(this.FileUniqueIDString);
                    __init_FileUniqueID = true;
                }
                return _FileUniqueID;
            }
            private set
            {
                _FileUniqueID = value;
                __init_FileUniqueID = true;
            }
        }
    }
}