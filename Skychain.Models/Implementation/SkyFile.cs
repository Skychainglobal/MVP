using Storage.Lib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skychain.Models.Implementation
{
    /// <summary>
    /// Представляет файле в хранилище.
    /// Содержит методы чтения, создания, обновления, удаления файла и проверки наличия файла.
    /// </summary>
    public class SkyFile : ISkyFile
    {
        internal SkyFile(Guid fileID, string folderName, bool readOnly, SkyContext context)
        {
            if (string.IsNullOrEmpty(folderName))
                throw new ArgumentNullException("folderName");
            if (context == null)
                throw new ArgumentNullException("context");

            this.ID = fileID;
            this.FolderName = folderName;
            this.ReadOnly = readOnly;
            this.Context = context;
        }

        /// <summary>
        /// Идентификатор файла.
        /// </summary>
        public Guid ID { get; private set; }

        /// <summary>
        /// Возвращает true, если файл является новым и не имеет идентификатора.
        /// </summary>
        public bool IsNew => this.ID == Guid.Empty;

        /// <summary>
        /// Название папки хранилища, в которой содержится файл.
        /// </summary>
        internal string FolderName { get; private set; }

        /// <summary>
        /// Файл только для чтения.
        /// При установленном значении true загрузка новой версии файла запрещена.
        /// </summary>
        public bool ReadOnly { get; private set; }

        /// <summary>
        /// Контекст системы.
        /// </summary>
        public SkyContext Context { get; private set; }

        /// <summary>
        /// Имя файла.
        /// </summary>
        public string Name => this.File.Name;

        /// <summary>
        /// Размер файла.
        /// </summary>
        public long Size => this.File.Size;

        /// <summary>
        /// Дата создания файла.
        /// </summary>
        public DateTime TimeCreated => this.File.TimeCreated;

        /// <summary>
        /// Дата последнего изменения файла.
        /// </summary>
        public DateTime TimeModified => this.File.TimeModified;


        private bool __init_Folder = false;
        private IFolder _Folder;
        /// <summary>
        /// Папка хранилища, в которой содержится файл.
        /// </summary>
        private IFolder Folder
        {
            get
            {
                if (!__init_Folder)
                {
                    _Folder = this.Context.Storage.EnsureFolder(this.FolderName);
                    if (_Folder == null)
                        throw new Exception(string.Format("Failed to get the [{0}] folder.", this.FolderName));
                    __init_Folder = true;
                }
                return _Folder;
            }
        }


        private bool __init_FileInternal = false;
        private IFile _FileInternal;
        /// <summary>
        /// Файл в хранилище.
        /// Генерирует исключение в случае отсутствия файла.
        /// </summary>
        private IFile FileInternal
        {
            get
            {
                if (!__init_FileInternal)
                {
                    _FileInternal = null;
                    if (!this.IsNew)
                        _FileInternal = this.Folder.GetFile(this.ID, null, false);
                    __init_FileInternal = true;
                }
                return _FileInternal;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                _FileInternal = value;
                __init_FileInternal = true;
            }
        }


        /// <summary>
        /// Файл в хранилище.
        /// Генерирует исключение в случае отсутствия файла.
        /// </summary>
        private IFile File
        {
            get
            {
                this.CheckExists();
                return this.FileInternal;
            }
        }


        /// <summary>
        /// Возвращает true, если файл существует в хранилище.
        /// </summary>
        public bool Exists => this.FileInternal != null;


        /// <summary>
        /// Генерирует исключение в случае отсутствия файла.
        /// </summary>
        public void CheckExists()
        {
            if (!this.Exists)
            {
                if (this.IsNew)
                    throw new Exception("Failed to get file by because file is not exists.");
                else
                    throw new Exception(string.Format("Failed to get file by ID={0} from folder [{1}].", this.ID, this.Folder.Url));
            }
        }

		/// <summary>
		/// Загружает новый файл в хранилище, если файл не существует.
		/// Генерирует исключение в случае, если файл существует.
		/// </summary>
		/// <param name="fileName">Название загружаемого файла.</param>
		/// <param name="content">Содержимое загружаемого файла.</param>
		public void Upload(string fileName, byte[] content)
		{
			using (MemoryStream stream = new MemoryStream(content))
			{
				stream.Position = 0;
				this.Upload(fileName, stream);
			}
		}

        /// <summary>
        /// Загружает новый файл в хранилище, если файл не существует.
        /// Генерирует исключение в случае, если файл существует.
        /// </summary>
        /// <param name="fileName">Название загружаемого файла.</param>
        /// <param name="fileStream">Поток данных загружаемого файла.</param>
        public void Upload(string fileName, Stream fileStream)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException("fileName");
            if (fileStream == null)
                throw new ArgumentNullException("fileStream");

            //проверяем возможность загрузки файла только для чтения.
            if (!this.IsNew && this.ReadOnly)
            {
                if (this.Exists)
                    throw new Exception(string.Format("Cannot upload file version because file with ID={0} is readonly.", this.ID));
                else
                    throw new Exception(string.Format("Cannot upload unexisting file because file has already mapped on ID={0}.", this.ID));
            }

            //загружаем новый файл.
            if (this.IsNew)
            {
                this.FileInternal = this.Folder.UploadFile(fileName, fileStream);
                this.ID = this.File.UniqueID;
            }
            //обновляем версию файла.
            else
                this.File.Update(fileStream, fileName);
        }


        /// <summary>
        /// Открывает поток чтения данных файла.
        /// </summary>
        public Stream Open()
        {
            return this.File.Open();
        }

		private bool __init_Content;
		private byte[] _Content;
		/// <summary>
		/// Контент файла.
		/// </summary>
		public byte[] Content
		{
			get
			{
				if (!__init_Content)
				{
					using (MemoryStream stream = new MemoryStream())
					using (Stream st = this.Open())
					{
						byte[] buffer = new byte[4096];
						int read;
						while (st.CanRead && (read = st.Read(buffer, 0, buffer.Length)) > 0)
						{
							stream.Write(buffer, 0, read);
						}
						_Content = stream.ToArray();
					}
					__init_Content = true;
				}
				return _Content;
			}
		}

        /// <summary>
        /// Удаляет файл из хранилища.
        /// Генерирует исключение в случае, если файл является файлом только для чтения.
        /// </summary>
        public void Delete()
        {
            //проверяем, что файл существует.
            this.CheckExists();

            //проверяем возможность удаления файла.
            if (this.ReadOnly && !this.Context.IsDeletingContext())
                throw new Exception(string.Format("Cannot delete file because file with ID={0} is readonly.", this.ID));

            //удаляем файл.
            this.File.Delete();
        }

        /// <summary>
        /// Возвращает сессионную ссылку на файл.
        /// </summary>
        /// <returns></returns>
        public string GetSessionLink()
        {
            this.CheckExists();

            string sessionLink = this.Context.Storage.GetSessionFileLink(this.FileInternal.Url, this.FileInternal.VersionUniqueID);
            return sessionLink;
        }

        ISkyContext ISkyFile.Context => this.Context;

        /// <summary>
        /// Текстовое представление экземпляра класса.
        /// </summary>
        public override string ToString()
        {
            if (this.Exists)
                return this.File.Name;
            return base.ToString();
        }
    }
}
