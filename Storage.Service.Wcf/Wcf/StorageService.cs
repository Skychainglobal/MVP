using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Security.Principal;
using System.ServiceModel;
using System.ServiceModel.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Storage.Engine;
using Storage.Lib;

namespace Storage.Service.Wcf
{
    /// <summary>
    /// Интерфейс службы файлового хранилища.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    internal class StorageService : IStorageService, IStorageServiceStreamed, IDisposable
    {
        private bool __init_Engine;
        private StorageEngine _Engine;
        private StorageEngine Engine
        {
            get
            {
                if (!__init_Engine)
                {
                    _Engine = new StorageEngine();
                    __init_Engine = true;
                }
                return _Engine;
            }
        }

        private bool __init_Storage;
        private IStorage _Storage;
        /// <summary>
        /// Файловое хранилище.
        /// </summary>
        private IStorage Storage
        {
            get
            {
                if (!__init_Storage)
                {
                    _Storage = this.Engine;
                    __init_Storage = true;
                }
                return _Storage;
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
                    _Logger = ConfigFactory.Instance.Create<ILogProvider>(ServiceConsts.Logs.Scopes.WcfService);
                    __init_Logger = true;
                }
                return _Logger;
            }
        }

        private bool __init_PermissionAdapter;
        private PermissionAdapter _PermissionAdapter;
        private PermissionAdapter PermissionAdapter
        {
            get
            {
                if (!__init_PermissionAdapter)
                {
                    _PermissionAdapter = new PermissionAdapter();
                    __init_PermissionAdapter = true;
                }
                return _PermissionAdapter;
            }
        }

        private bool __init_Configuration;
        private ServiceConfiguration _Configuration;
        /// <summary>
        /// Параметры веб-сервиса.
        /// </summary>
        private ServiceConfiguration Configuration
        {
            get
            {
                if (!__init_Configuration)
                {
                    _Configuration = new ServiceConfiguration();
                    __init_Configuration = true;
                }
                return _Configuration;
            }
        }

        /// <summary>
        /// Возвращает информацию о файловом хранилище.
        /// </summary>
        /// <returns></returns>
        public WcfStorageInfo GetStorageInfo()
        {
            WcfStorageInfo storageInfo = null;
            try
            {
                this.Authorize();

                this.Logger.WriteMessage("GetStorageInfo:Начало получения информации о хранилище");
                storageInfo = new WcfStorageInfo(this.Engine.CurrentNode);
                this.Logger.WriteMessage("GetStorageInfo:Окончание получения информации о хранилище");
            }
            catch (Exception ex)
            {
                this.Logger.WriteFormatMessage("GetStorageInfo:Ошибка выполенения операции, текст ошибки: {0}", ex);
                throw ex;
            }
            return storageInfo;
        }

        #region Folders
        /// <summary>
        /// Возвращает папку по-умолчанию для загрузки файлов.
        /// </summary>
        /// <returns></returns>
        public WcfFolderInfo GetDefaultFolder()
        {
            WcfFolderInfo wcfFolder = null;
            try
            {
                this.Authorize();

                this.Logger.WriteMessage("GetDefaultFolder:Начало получения папки по-умолчанию");
                IFolder folder = this.Storage.DefaultFolder;
                if (folder == null)
                    throw new Exception(string.Format("Не удалось получить папку по-умолчанию"));

                wcfFolder = WcfFolderInfo.FromFolder(folder);
                this.Logger.WriteMessage("GetDefaultFolder:Окончание получения папки по-умолчанию");
            }
            catch (Exception ex)
            {
                this.Logger.WriteFormatMessage("GetDefaultFolder:Ошибка выполенения операции, текст ошибки: {0}", ex);
                throw ex;
            }

            return wcfFolder;
        }

        /// <summary>
        /// Возвращает коллекцию папок.
        /// </summary>
        /// <param name="parentFolderUrl">Адрес родительской папки.</param>
        /// <returns></returns>
        public WcfFolderInfo[] GetFolders(string parentFolderUrl = null)
        {
            List<WcfFolderInfo> resultList = new List<WcfFolderInfo>();

            try
            {
                this.Authorize();

                this.Logger.WriteMessage("GetFolders:Начало получения дочерних папок");
                IFolder[] folders = null;
                if (!string.IsNullOrEmpty(parentFolderUrl))
                {
                    IFolder parentFolder = this.Storage.GetFolder(parentFolderUrl);
                    folders = parentFolder.Folders.ToArray();
                }
                else
                    folders = this.Storage.Folders.ToArray();

                if (folders != null)
                {
                    foreach (IFolder folder in folders)
                    {
                        WcfFolderInfo wcfFolder = WcfFolderInfo.FromFolder(folder);
                        resultList.Add(wcfFolder);
                    }
                }

                this.Logger.WriteMessage("GetFolders:Окончание получения дочерних папок");
            }
            catch (Exception ex)
            {
                this.Logger.WriteFormatMessage("GetFolders:Ошибка выполенения операции, текст ошибки: {0}", ex);

                throw ex;
            }

            return resultList.ToArray();
        }

        /// <summary>
        /// Возвращает папку по адресу. Если папки не существует, то она будет создана.
        /// </summary>
        /// <param name="url">Адрес папки.</param>
        /// <returns></returns>
        public WcfFolderInfo EnsureFolder(string url)
        {
            WcfFolderInfo wcfFolder = null;
            try
            {
                this.Authorize();

                if (string.IsNullOrEmpty(url))
                    throw new ArgumentNullException("url");

                this.Logger.WriteFormatMessage("EnsureFolder:Начало получения папки, url: {0}", url);
                IFolder folder = this.Storage.EnsureFolder(url);
                wcfFolder = WcfFolderInfo.FromFolder(folder);

                this.Logger.WriteFormatMessage("EnsureFolder:Окончание получения папки, url: {0}", url);
            }
            catch (Exception ex)
            {
                this.Logger.WriteFormatMessage("EnsureFolder:Ошибка выполенения операции, текст ошибки: {0}", ex);

                throw ex;
            }

            return wcfFolder;
        }

        /// <summary>
        /// Возвращает папку по адресу.
        /// </summary>
        /// <param name="url">Адрес папки.</param>
        /// <returns></returns>
        public WcfFolderInfo GetFolder(string url)
        {
            WcfFolderInfo wcfFolder = null;

            try
            {
                this.Authorize();

                if (string.IsNullOrEmpty(url))
                    throw new ArgumentNullException("url");

                this.Logger.WriteFormatMessage("GetFolder:Начало получения папки, url: {0}", url);
                IFolder folder = null;
                if (url.ToLower() == StorageConsts.Folders.DefaultFolderName.ToLower())
                    folder = this.Storage.DefaultFolder;
                else
                    folder = this.Storage.GetFolder(url);
                if (folder != null)
                    wcfFolder = WcfFolderInfo.FromFolder(folder);

                this.Logger.WriteFormatMessage("GetFolder:Окончание получения папки, url: {0}", url);
            }
            catch (Exception ex)
            {
                this.Logger.WriteFormatMessage("GetFolder:Ошибка выполенения операции, текст ошибки: {0}", ex);
                throw ex;
            }

            return wcfFolder;
        }
        #endregion

        #region Files
        /// <summary>
        /// Загружает файл в хранилище.
        /// </summary>
        /// <param name="folderUrl">Адрес папки.</param>
        /// <param name="fileName">Имя файла.</param>
        /// <param name="content">Содержимое файла.</param>
        /// <returns></returns>
        public WcfFileInfo UploadFile(string folderUrl, string fileName, byte[] content)
        {
            WcfFileInfo wcfFile = null;

            try
            {
                this.Authorize();

                if (string.IsNullOrEmpty(folderUrl))
                    throw new ArgumentNullException("folderUrl");

                if (string.IsNullOrEmpty(fileName))
                    throw new ArgumentNullException("fileName");

                if (content == null)
                    throw new ArgumentNullException("content");

                this.Logger.WriteFormatMessage("UploadFile:Начало загрузки файла, folderUrl: {0}, fileName: {1}, contentLength: {2}", folderUrl, fileName, content.LongLength);

                using (MemoryStream ms = new MemoryStream(content))
                {
                    using (IFile file = this.Storage.UploadFile(folderUrl, fileName, ms))
                    {
                        wcfFile = WcfFileInfo.FromFile(file);
                    }
                }

                this.Logger.WriteFormatMessage("UploadFile:Окончание загрузки файла, folderUrl: {0}, fileName: {1}, contentLength: {2}", folderUrl, fileName, content.LongLength);
            }
            catch (Exception ex)
            {
                this.Logger.WriteFormatMessage("UploadFile:Ошибка выполенения операции, текст ошибки: {0}", ex);
                throw ex;
            }

            return wcfFile;
        }

        /// <summary>
        /// Обновляет содержимое файла в хранилище.
        /// </summary>
        /// <param name="folderUrl">Адрес папки.</param>
        /// <param name="fileUniqueID">Уникальный идентификатор файла.</param>
        /// <param name="content">Содержимое файла.</param>
        /// <param name="fileName">Имя файла.</param>
        /// <returns></returns>
        public WcfFileInfo UpdateFile(string folderUrl, Guid fileUniqueID, byte[] content, string fileName = null)
        {
            WcfFileInfo wcfFile = null;

            try
            {
                this.Authorize();

                if (string.IsNullOrEmpty(folderUrl))
                    throw new ArgumentNullException("folderUrl");

                if (fileUniqueID == Guid.Empty)
                    throw new ArgumentNullException("fileUniqueID");

                if (content == null || content.Length == 0)
                    throw new ArgumentNullException("content");

                this.Logger.WriteFormatMessage("UpdateFile:Начало обновления содержимого файла, folderUrl: {0}, fileUniqueID: {1}, contentLength: {2}", folderUrl, fileUniqueID, content.LongLength);
                using (MemoryStream ms = new MemoryStream(content))
                {
                    using (IFile file = this.Storage.GetFile(folderUrl, fileUniqueID))
                    {
                        file.Update(ms, fileName);
                        wcfFile = WcfFileInfo.FromFile(file);
                    }
                }

                this.Logger.WriteFormatMessage("UpdateFile:Окончание обновления содержимого файла, folderUrl: {0}, fileUniqueID: {1}, contentLength: {2}", folderUrl, fileUniqueID, content.LongLength);
            }
            catch (Exception ex)
            {
                this.Logger.WriteFormatMessage("UpdateFile:Ошибка выполенения операции, текст ошибки: {0}", ex);
                throw ex;
            }

            return wcfFile;
        }

        /// <summary>
        /// Обновляет содержимое файла в хранилище.
        /// </summary>
        /// <param name="folderUrl">Адрес файла.</param>
        /// <param name="content">Содержимое файла.</param>
        /// <param name="fileName">Имя файла.</param>
        /// <returns></returns>
        public WcfFileInfo UpdateFile(string fileUrl, byte[] content, string fileName = null)
        {
            WcfFileInfo wcfFile = null;

            try
            {
                this.Authorize();

                if (string.IsNullOrEmpty(fileUrl))
                    throw new ArgumentNullException("fileUrl");

                if (content == null || content.Length == 0)
                    throw new ArgumentNullException("content");

                this.Logger.WriteFormatMessage("UpdateFile:Начало обновления содержимого файла, fileUrl: {0}, contentLength: {1}", fileUrl, content.LongLength);

                using (MemoryStream ms = new MemoryStream(content))
                {
                    using (IFile file = this.Storage.GetFile(fileUrl))
                    {
                        file.Update(ms, fileName);
                        wcfFile = WcfFileInfo.FromFile(file);
                    }
                }
                this.Logger.WriteFormatMessage("UpdateFile:Окончание обновления содержимого файла, fileUrl: {0}, contentLength: {1}", fileUrl, content.LongLength);
            }
            catch (Exception ex)
            {
                this.Logger.WriteFormatMessage("UpdateFile:Ошибка выполенения операции, текст ошибки: {0}", ex);
                throw ex;
            }

            return wcfFile;
        }

        /// <summary>
        /// Возвращает файл.
        /// </summary>
        /// <param name="folderUrl">Адрес папки.</param>
        /// <param name="fileUniqueID">Уникальный идентификатор файла.</param>
        /// <param name="loadOptions">Опции загрузки.</param>
        /// <returns></returns>
        public WcfFileInfo GetFile(string folderUrl, Guid fileUniqueID, GetFileOptions loadOptions = null)
        {
            WcfFileInfo wcfFile = null;

            try
            {
                this.Authorize();

                if (string.IsNullOrEmpty(folderUrl))
                    throw new ArgumentNullException("folderUrl");

                if (fileUniqueID == Guid.Empty)
                    throw new ArgumentNullException("fileUniqueID");

                this.Logger.WriteFormatMessage("GetFile:Начало получения файла, folderUrl: {0}, fileUniqueID: {1}", folderUrl, fileUniqueID);

                //файл выбираем всегда без содержимого, чтобы не превысить размер буферной передачи,
                //которая известна только на уровне транспорта
                using (IFile file = this.Storage.GetFile(folderUrl, fileUniqueID))
                {
                    if (loadOptions != null && loadOptions.LoadContent)
                        this.VerifyBufferSize(file.Size);

                    wcfFile = WcfFileInfo.FromFile(file, loadOptions);
                }
                this.Logger.WriteFormatMessage("GetFile:Окончание получения файла, folderUrl: {0}, fileUniqueID: {1}", folderUrl, fileUniqueID);
            }
            catch (Exception ex)
            {
                this.Logger.WriteFormatMessage("GetFile:Ошибка выполенения операции, текст ошибки: {0}", ex);
                throw ex;
            }

            return wcfFile;
        }

        /// <summary>
        /// Возвращает файл.
        /// </summary>
        /// <param name="fileUrl">Адрес файла.</param>
        /// <param name="loadOptions">Опции загрузки.</param>
        /// <returns></returns>
        public WcfFileInfo GetFile(string fileUrl, GetFileOptions loadOptions = null)
        {
            WcfFileInfo wcfFile = null;

            try
            {
                this.Authorize();

                if (string.IsNullOrEmpty(fileUrl))
                    throw new ArgumentNullException("fileUrl");

                this.Logger.WriteFormatMessage("GetFile:Начало получения файла, fileUrl: {0}", fileUrl);

                //файл выбираем всегда без содержимого, чтобы не превысить размер буферной передачи,
                //которая известна только на уровне транспорта
                using (IFile file = this.Storage.GetFile(fileUrl, loadOptions))
                {
                    if (loadOptions != null && loadOptions.LoadContent)
                        this.VerifyBufferSize(file.Size);

                    wcfFile = WcfFileInfo.FromFile(file, loadOptions);
                }
                this.Logger.WriteFormatMessage("GetFile:Окончание получения файла, fileUrl: {0}", fileUrl);
            }
            catch (Exception ex)
            {
                this.Logger.WriteFormatMessage("GetFile:Ошибка выполенения операции, текст ошибки: {0}", ex);
                throw ex;
            }

            return wcfFile;
        }

        /// <summary>
        /// Возвращает содержимое файла.
        /// </summary>
        /// <param name="folderUrl">Адрес папки.</param>
        /// <param name="fileUniqueID">Уникальный идентификатор файла.</param>
        /// <returns></returns>
        public byte[] GetFileContent(string folderUrl, Guid fileUniqueID)
        {
            byte[] content = null;

            try
            {
                this.Authorize();

                if (string.IsNullOrEmpty(folderUrl))
                    throw new ArgumentNullException("folderUrl");

                if (fileUniqueID == Guid.Empty)
                    throw new ArgumentNullException("fileUniqueID");

                this.Logger.WriteFormatMessage("GetFile:Начало получения содержимого файла, folderUrl: {0}, fileUniqueID: {1}", folderUrl, fileUniqueID);

                //файл выбираем всегда без содержимого, чтобы не превысить размер буферной передачи,
                //которая известна только на уровне транспорта
                using (IFile file = this.Storage.GetFile(folderUrl, fileUniqueID))
                {
                    this.VerifyBufferSize(file.Size);
                    content = file.Content;
                }
                this.Logger.WriteFormatMessage("GetFile:Окончание получения содержимого файла, folderUrl: {0}, fileUniqueID: {1}", folderUrl, fileUniqueID);
            }
            catch (Exception ex)
            {
                this.Logger.WriteFormatMessage("GetFile:Ошибка выполенения операции, текст ошибки: {0}", ex);
                throw ex;
            }

            return content;
        }

        /// <summary>
        /// Возвращает содержимое версии файла.
        /// </summary>
        /// <param name="folderUrl">Адрес папки.</param>
        /// <param name="fileUniqueID">Уникальный идентификатор файла.</param>
        /// <param name="versionUniqueID">Уникальный идентификатор версии файла.</param>
        /// <returns></returns>
        public byte[] GetFileVersionContent(string folderUrl, Guid fileUniqueID, Guid versionUniqueID)
        {
            byte[] content = null;

            try
            {
                this.Authorize();

                if (string.IsNullOrEmpty(folderUrl))
                    throw new ArgumentNullException("folderUrl");

                if (fileUniqueID == Guid.Empty)
                    throw new ArgumentNullException("fileUniqueID");

                if (versionUniqueID == Guid.Empty)
                    throw new ArgumentNullException("versionUniqueID");

                this.Logger.WriteFormatMessage("GetFileVersionContent:Начало получения содержимого файла, folderUrl: {0}, fileUniqueID: {1}, versionUniqueID: {2}", folderUrl, fileUniqueID, versionUniqueID);

                using (IFile file = this.Storage.GetFile(folderUrl, fileUniqueID))
                {
                    using (IFileVersion version = file.GetVersion(versionUniqueID))
                    {
                        this.VerifyBufferSize(version.Size);
                        content = version.Content;
                    }
                }
                this.Logger.WriteFormatMessage("GetFileVersionContent:Начало получения содержимого файла, folderUrl: {0}, fileUniqueID: {1}, versionUniqueID: {2}", folderUrl, fileUniqueID, versionUniqueID);
            }
            catch (Exception ex)
            {
                this.Logger.WriteFormatMessage("GetFileVersionContent:Ошибка выполенения операции, текст ошибки: {0}", ex);
                throw ex;
            }

            return content;
        }

        /// <summary>
        /// Проверяет размер файла при возврате содержимого файла.
        /// </summary>
        /// <param name="fileSize"></param>
        private void VerifyBufferSize(long fileSize)
        {
            if (fileSize < 1)
                throw new ArgumentNullException("fileSize");

            var tmp = OperationContext.Current;
            bool overflow = false;
            if (fileSize > this.Configuration.MaxBufferRequestSize)
                overflow = true;

            if (overflow)
            {
                int mb = 1024 * 1024;
                double fileSizeInMB = (double)fileSize / mb;
                double maxRequestSizeInMB = (double)this.Configuration.MaxBufferRequestSize / mb;

                throw new Exception(string.Format("Невозможно вернуть содержимое файла, превышен лимит буферной передачи. Размер запрашиваемого файла: {0} ({1:0.00} MB), в то время как максимально допустимый размер файла в буферном режиме: {2} ({3:0.00} MB).",
                    fileSize,
                    fileSizeInMB,
                    this.Configuration.MaxBufferRequestSize,
                    maxRequestSizeInMB));
            }
        }

        /// <summary>
        /// Возвращает коллекию версий файлов.
        /// </summary>
        /// <param name="folderUrl">Адрес папки.</param>
        /// <param name="fileUniqueID">Уникальный идентификатор файла.</param>
        /// <returns></returns>
        public WcfFileVersionInfo[] GetFileVersions(string folderUrl, Guid fileUniqueID)
        {
            List<WcfFileVersionInfo> versionsInfo = new List<WcfFileVersionInfo>();

            try
            {
                this.Authorize();

                if (string.IsNullOrEmpty(folderUrl))
                    throw new ArgumentNullException("folderUrl");

                if (fileUniqueID == Guid.Empty)
                    throw new ArgumentNullException("fileUniqueID");

                this.Logger.WriteFormatMessage("GetFileVersions:Начало получения версий файла, folderUrl: {0}, fileUniqueID: {1}", folderUrl, fileUniqueID);

                using (IFile file = this.Storage.GetFile(folderUrl, fileUniqueID))
                {
                    if (file.Versions != null)
                    {
                        foreach (IFileVersion version in file.Versions)
                        {
                            using (version)
                            {
                                WcfFileVersionInfo versionInfo = WcfFileVersionInfo.FromFileVersion(version);
                                versionsInfo.Add(versionInfo);
                            }
                        }
                    }
                }

                this.Logger.WriteFormatMessage("GetFileVersions:Начало получения версий файла, folderUrl: {0}, fileUniqueID: {1}", folderUrl, fileUniqueID);
            }
            catch (Exception ex)
            {
                this.Logger.WriteFormatMessage("GetFileVersions:Ошибка выполенения операции, текст ошибки: {0}", ex);
                throw ex;
            }

            return versionsInfo.ToArray();
        }

        /// <summary>
        /// Удаляет файл.
        /// </summary>
        /// <param name="folderUrl">Адрес папки.</param>
        /// <param name="fileUniqueID">Уникальный идентификатор файла.</param>
        /// <returns></returns>
        public bool DeleteFile(string folderUrl, Guid fileUniqueID)
        {
            bool result = false;

            try
            {
                this.Authorize();

                if (string.IsNullOrEmpty(folderUrl))
                    throw new ArgumentNullException("folderUrl");

                if (fileUniqueID == null)
                    throw new ArgumentNullException("fileUniqueID");

                this.Logger.WriteFormatMessage("DeleteFile:Начало удаления файла, folderUrl: {0}, fileUniqueID: {1}", folderUrl, fileUniqueID);

                result = this.Storage.DeleteFile(folderUrl, fileUniqueID);

                this.Logger.WriteFormatMessage("DeleteFile:Окончание удаления файла, folderUrl: {0}, fileUniqueID: {1}", folderUrl, fileUniqueID);
            }
            catch (Exception ex)
            {
                this.Logger.WriteFormatMessage("DeleteFile:Ошибка выполенения операции, текст ошибки: {0}", ex);
                throw ex;
            }

            return result;
        }
        #endregion

        #region Session Links
        /// <summary>
        /// Возвращает сессионную ссылку на версию файла.
        /// </summary>
        /// <param name="fileUrl">Адрес файла.</param>
        /// <param name="versionUniqueID">Уникальный идентификатор версии файла.</param>
        /// <returns></returns>
        public string GetSessionFileLink(string fileUrl, Guid versionUniqueID)
        {
            string sessionLink = null;

            try
            {
                this.Authorize();

                if (string.IsNullOrEmpty(fileUrl))
                    throw new ArgumentNullException("fileUrl");

                if (versionUniqueID == Guid.Empty)
                    throw new ArgumentNullException("versionUniqueID");

                this.Logger.WriteFormatMessage("GetSessionFileLink:Начало получения сессионной ссылки на файл, fileUrl: {0}, versionUniqueID: {1}", fileUrl, versionUniqueID);

                sessionLink = this.Storage.GetSessionFileLink(fileUrl, versionUniqueID);

                this.Logger.WriteFormatMessage("GetSessionFileLink:Окончание получения сессионной ссылки на файл, fileUrl: {0}, versionUniqueID: {1}", fileUrl, versionUniqueID);
            }
            catch (Exception ex)
            {
                this.Logger.WriteFormatMessage("GetSessionFileLink:Ошибка выполенения операции, текст ошибки: {0}", ex);
                throw ex;
            }

            return sessionLink;
        }

        /// <summary>
        /// Возвращает сессионную ссылку на версию файла.
        /// </summary>
        /// <param name="folderUrl">Адрес папки.</param>
        /// <param name="fileUrl">Адрес файла.</param>
        /// <param name="versionUniqueID">Уникальный идентификатор версии файла.</param>
        /// <returns></returns>
        public string GetSessionFileLink(string folderUrl, Guid fileUniqueID, Guid versionUniqueID)
        {
            string sessionLink = null;

            try
            {
                this.Authorize();

                if (string.IsNullOrEmpty(folderUrl))
                    throw new ArgumentNullException("folderUrl");

                if (fileUniqueID == Guid.Empty)
                    throw new ArgumentNullException("fileUniqueID");

                if (versionUniqueID == Guid.Empty)
                    throw new ArgumentNullException("versionUniqueID");

                this.Logger.WriteFormatMessage("GetSessionFileLink:Начало получения сессионной ссылки на файл, folderUrl: {0}, fileUniqueID: {1}, versionUniqueID: {2}", folderUrl, fileUniqueID, versionUniqueID);

                sessionLink = this.Storage.GetSessionFileLink(folderUrl, fileUniqueID, versionUniqueID);

                this.Logger.WriteFormatMessage("GetSessionFileLink:Окончание получения сессионной ссылки на файл, folderUrl: {0}, fileUniqueID: {1}, versionUniqueID: {2}", folderUrl, fileUniqueID, versionUniqueID);
            }
            catch (Exception ex)
            {
                this.Logger.WriteFormatMessage("GetSessionFileLink:Ошибка выполенения операции, текст ошибки: {0}", ex);
                throw ex;
            }

            return sessionLink;
        }

        /// <summary>
        /// Возвращает персонализированную сессионную ссылку на версию файла.
        /// </summary>
        /// <param name="fileUrl">Адрес файла.</param>
        /// <param name="versionUniqueID">Уникальный идентификатор версии файла.</param>
        /// <param name="userLogin">Логин пользователя.</param>
        /// <returns></returns>
        public string GetUserSessionFileLink(string fileUrl, Guid versionUniqueID, string userLogin)
        {
            string sessionLink = null;

            try
            {
                this.Authorize();

                if (string.IsNullOrEmpty(fileUrl))
                    throw new ArgumentNullException("fileUrl");

                if (versionUniqueID == Guid.Empty)
                    throw new ArgumentNullException("versionUniqueID");

                if (String.IsNullOrEmpty(userLogin))
                    throw new ArgumentNullException("userLogin");

                this.Logger.WriteFormatMessage("GetSessionFileLink:Начало получения сессионной ссылки на файл, fileUrl: {0}, versionUniqueID: {1}", fileUrl, versionUniqueID);

                sessionLink = this.Storage.GetSessionFileLink(fileUrl, versionUniqueID, userLogin);

                this.Logger.WriteFormatMessage("GetSessionFileLink:Окончание получения сессионной ссылки на файл, fileUrl: {0}, versionUniqueID: {1}", fileUrl, versionUniqueID);
            }
            catch (Exception ex)
            {
                this.Logger.WriteFormatMessage("GetSessionFileLink:Ошибка выполенения операции, текст ошибки: {0}", ex);
                throw ex;
            }

            return sessionLink;
        }

        /// <summary>
        /// Возвращает персонализированную сессионную ссылку на версию файла.
        /// </summary>
        /// <param name="folderUrl">Адрес папки.</param>
        /// <param name="fileUrl">Адрес файла.</param>
        /// <param name="versionUniqueID">Уникальный идентификатор версии файла.</param>
        /// <param name="userLogin">Логин пользователя.</param>
        /// <returns></returns>
        public string GetUserSessionFileLink(string folderUrl, Guid fileUniqueID, Guid versionUniqueID, string userLogin)
        {
            string sessionLink = null;

            try
            {
                this.Authorize();

                if (string.IsNullOrEmpty(folderUrl))
                    throw new ArgumentNullException("folderUrl");

                if (fileUniqueID == Guid.Empty)
                    throw new ArgumentNullException("fileUniqueID");

                if (versionUniqueID == Guid.Empty)
                    throw new ArgumentNullException("versionUniqueID");

                if (string.IsNullOrEmpty(userLogin))
                    throw new ArgumentNullException("userLogin");

                this.Logger.WriteFormatMessage("GetUserSessionFileLink:Начало получения сессионной ссылки на файл, folderUrl: {0}, fileUniqueID: {1}, versionUniqueID: {2}", folderUrl, fileUniqueID, versionUniqueID);

                sessionLink = this.Storage.GetSessionFileLink(folderUrl, fileUniqueID, versionUniqueID, userLogin);

                this.Logger.WriteFormatMessage("GetUserSessionFileLink:Окончание получения сессионной ссылки на файл, folderUrl: {0}, fileUniqueID: {1}, versionUniqueID: {2}", folderUrl, fileUniqueID, versionUniqueID);
            }
            catch (Exception ex)
            {
                this.Logger.WriteFormatMessage("GetUserSessionFileLink:Ошибка выполенения операции, текст ошибки: {0}", ex);
                throw ex;
            }

            return sessionLink;
        }
        #endregion

        #region Streaming
        /// <summary>
        /// Запускает потоковую передачу данных файла.
        /// </summary>
        /// <param name="uploadMessage">Сообщение с параметрами загрузки файла.</param>
        /// <returns></returns>
        public WcfFileInfoMessage UploadFile(UploadMessage uploadMessage)
        {
            WcfFileInfoMessage fileInfo = null;

            try
            {
                if (uploadMessage == null)
                    throw new ArgumentNullException("uploadMessage");

                if (string.IsNullOrEmpty(uploadMessage.FolderUrl))
                    throw new ArgumentNullException("uploadMessage.FolderUrl");

                if (string.IsNullOrEmpty(uploadMessage.FileName))
                    throw new ArgumentNullException("uploadMessage.FileName");

                if (uploadMessage.FileStream == null)
                    throw new ArgumentNullException("uploadMessage.FileStream");

                if (uploadMessage.AccessToken == Guid.Empty)
                    throw new ArgumentNullException("uploadMessage.AccessToken");

                //проверка токена
                IToken token = this.Engine.MetadataAdapter.GetToken(uploadMessage.AccessToken);
                if (token == null || !token.IsValid())
                    throw new SecurityAccessDeniedException(string.Format("Access is denied."));

                this.Logger.WriteFormatMessage("UploadFileStream:Начало потоковой передачи данных файла");

                //загружаем поток данных
                using (IFile file = this.Storage.UploadFile(uploadMessage.FolderUrl, uploadMessage.FileName, uploadMessage.FileStream))
                {
                    fileInfo = WcfFileInfoMessage.FromFile(file);
                }

                //удаляем токен, после его использования
                this.Engine.MetadataAdapter.RemoveToken(uploadMessage.AccessToken);

                this.Logger.WriteFormatMessage("UploadFileStream:Окончание потоковой передачи данных файла");
            }
            catch (Exception ex)
            {
                this.Logger.WriteFormatMessage("UploadFileStream:Ошибка выполенения операции, текст ошибки: {0}", ex);
                throw ex;
            }

            return fileInfo;
        }

        public WcfFileInfoMessage UpdateFile(UpdateMessage updateMessage)
        {
            WcfFileInfoMessage fileInfo = null;

            try
            {
                if (updateMessage == null)
                    throw new ArgumentNullException("updateMessage");

                if (string.IsNullOrEmpty(updateMessage.FolderUrl))
                    throw new ArgumentNullException("updateMessage.FolderUrl");

                if (updateMessage.FileUniqueID == null)
                    throw new ArgumentNullException("updateMessage.FileUniqueID");

                if (updateMessage.FileStream == null)
                    throw new ArgumentNullException("updateMessage.FileStream");

                if (updateMessage.AccessToken == Guid.Empty)
                    throw new ArgumentNullException("updateMessage.AccessToken");

                //проверка токена
                IToken token = this.Engine.MetadataAdapter.GetToken(updateMessage.AccessToken);
                if (token == null || !token.IsValid())
                    throw new SecurityAccessDeniedException(string.Format("Access is denied."));

                this.Logger.WriteFormatMessage("UploadFileStream:Начало потоковой передачи данных файла");

                //загружаем поток данных
                using (IFile file = this.Storage.GetFile(updateMessage.FolderUrl, updateMessage.FileUniqueID))
                {
                    file.Update(updateMessage.FileStream, updateMessage.FileName);
                    fileInfo = WcfFileInfoMessage.FromFile(file);
                }

                //удаляем токен, после его использования
                this.Engine.MetadataAdapter.RemoveToken(updateMessage.AccessToken);

                this.Logger.WriteFormatMessage("UploadFileStream:Окончание потоковой передачи данных файла");
            }
            catch (Exception ex)
            {
                this.Logger.WriteFormatMessage("UploadFileStream:Ошибка выполенения операции, текст ошибки: {0}", ex);
                throw ex;
            }

            return fileInfo;
        }

        /// <summary>
        /// Запускает потоковое чтение данных файла.
        /// </summary>
        /// <param name="folderUrl">Адрес папки.</param>
        /// <param name="fileUniqueID">Уникальный идентификатор файла.</param>
        /// <returns></returns>
        public Stream OpenFile(string folderUrl, Guid fileUniqueID, Guid accessToken)
        {
            Stream stream = null;

            try
            {
                if (string.IsNullOrEmpty(folderUrl))
                    throw new ArgumentNullException("folderUrl");

                if (fileUniqueID == Guid.Empty)
                    throw new ArgumentNullException("fileUniqueID");

                if (accessToken == Guid.Empty)
                    throw new ArgumentNullException("accessToken");

                //проверка токена
                IToken token = this.Engine.MetadataAdapter.GetToken(accessToken);
                if (token == null || !token.IsValid())
                    throw new SecurityAccessDeniedException(string.Format("Access is denied."));

                this.Logger.WriteFormatMessage("GetFileStream:Начало потоковой передачи данных файла");

                //получение потока от хранилища
                stream = this.Storage.OpenFile(folderUrl, fileUniqueID);

                //удаляем токен, после его использования
                this.Engine.MetadataAdapter.RemoveToken(accessToken);

                this.Logger.WriteFormatMessage("GetFileStream:Окончание потоковой передачи данных файла");
            }
            catch (Exception ex)
            {
                this.Logger.WriteFormatMessage("GetFileStream:Ошибка выполенения операции, текст ошибки: {0}", ex);
                throw ex;
            }

            return stream;
        }

        /// <summary>
        /// Запускает потоковое чтение данных версии файла.
        /// </summary>
        /// <param name="folderUrl">Адрес папки.</param>
        /// <param name="fileUniqueID">Уникальный идентификатор файла.</param>
        /// <param name="versionUniqueID">Уникальный идентификатор версии файла.</param>
        /// <returns></returns>
        public Stream OpenFileVersion(string folderUrl, Guid fileUniqueID, Guid versionUniqueID, Guid accessToken)
        {
            Stream stream = null;

            try
            {
                if (string.IsNullOrEmpty(folderUrl))
                    throw new ArgumentNullException("folderUrl");

                if (fileUniqueID == Guid.Empty)
                    throw new ArgumentNullException("fileUniqueID");

                if (versionUniqueID == Guid.Empty)
                    throw new ArgumentNullException("versionUniqueID");

                if (accessToken == Guid.Empty)
                    throw new ArgumentNullException("accessToken");

                //проверка токена
                IToken token = this.Engine.MetadataAdapter.GetToken(accessToken);
                if (token == null || !token.IsValid())
                    throw new SecurityAccessDeniedException(string.Format("Access is denied."));

                this.Logger.WriteFormatMessage("GetFileVersionStream:Начало потоковой передачи данных версии файла");

                //получение потока от хранилища
                stream = this.Storage.OpenFile(folderUrl, fileUniqueID, versionUniqueID);

                //удаляем токен, после его использования
                this.Engine.MetadataAdapter.RemoveToken(accessToken);

                this.Logger.WriteFormatMessage("GetFileVersionStream:Окончание потоковой передачи данных версии файла");
            }
            catch (Exception ex)
            {
                this.Logger.WriteFormatMessage("GetFileVersionStream:Ошибка выполенения операции, текст ошибки: {0}", ex);
                throw ex;
            }

            return stream;
        }
        #endregion

        #region Tokens
        /// <summary>
        /// Вызыет токен доступа на операцию с файловым хранилищем.
        /// </summary>
        /// <returns></returns>
        public WcfTokenInfo GetAccessToken()
        {
            WcfTokenInfo wcfToken = null;
            try
            {
                this.Authorize();

                this.Logger.WriteMessage("GetAccessToken:Начало выдачи токена доступа");
                IToken token = this.Engine.MetadataAdapter.GenerateToken();
                if (token == null || !token.IsValid())
                    throw new Exception(string.Format("Не удалось выдать валидный токен доступа."));

                wcfToken = new WcfTokenInfo()
                {
                    UniqueID = token.UniqueID
                };
                this.Logger.WriteMessage("GetAccessToken:Окончание выдачи токена доступа");
            }
            catch (Exception ex)
            {
                this.Logger.WriteFormatMessage("GetAccessToken:Ошибка выполенения операции, текст ошибки: {0}", ex);
                throw ex;
            }

            return wcfToken;
        }
        #endregion

        /// <summary>
        /// Авторизует пользователя (проверка прав доступа).
        /// </summary>
        private void Authorize()
        {
            this.Logger.WriteMessage("Authorize:Начало авторизации пользователя");

            string resultUserName = null;
            if (ServiceSecurityContext.Current != null && ServiceSecurityContext.Current.WindowsIdentity != null)
                resultUserName = ServiceSecurityContext.Current.WindowsIdentity.Name;

            bool inGroup = false;
            foreach (string groupName in this.Configuration.WcfCallsPermissionGroups)
            {
                inGroup = this.PermissionAdapter.IsUserMemberOf(groupName, resultUserName);
                if (inGroup)
                    break;
            }

            //вызываем Access denied
            if (!inGroup)
                throw new SecurityAccessDeniedException(string.Format("Access is denied."));

            this.Logger.WriteMessage("Authorize:Пользователь успешно авторизован");
        }

        /// <summary>
        /// Освобождает ресурсы занятые объектом сервиса.
        /// Вызывается WCF службой.
        /// </summary>
        public void Dispose()
        {
            if (_Storage != null)
                this.Storage.Dispose();
        }
    }
}