using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Storage.Lib;
using System.IO;

namespace Storage.Service.Wcf
{
    /// <summary>
    /// Интерфейс службы файлового хранилища.
    /// </summary>
    [ServiceContract(Namespace = "Storage.Service.IStorageService")]
    internal interface IStorageService
    {
        /// <summary>
        /// Возвращает информацию о файловом хранилище.
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        WcfStorageInfo GetStorageInfo();

        #region Folders
        /// <summary>
        /// Возвращает папку по-умолчанию для загрузки файлов.
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        WcfFolderInfo GetDefaultFolder();

        /// <summary>
        /// Возвращает коллекцию папок.
        /// </summary>
        /// <param name="parentFolderUrl">Адрес родительской папки.</param>
        /// <returns></returns>
        [OperationContract]
        WcfFolderInfo[] GetFolders(string parentFolderUrl = null);

        /// <summary>
        /// Возвращает папку по адресу. Если папки не существует, то она будет создана.
        /// </summary>
        /// <param name="url">Адрес папки.</param>
        /// <returns></returns>
        [OperationContract]
        WcfFolderInfo EnsureFolder(string url);

        /// <summary>
        /// Возвращает папку по адресу.
        /// </summary>
        /// <param name="url">Адрес папки.</param>
        /// <returns></returns>
        [OperationContract]
        WcfFolderInfo GetFolder(string url);
        #endregion

        #region Files
        /// <summary>
        /// Загружает файл в хранилище.
        /// </summary>
        /// <param name="folderUrl">Адрес папки.</param>
        /// <param name="fileName">Имя файла.</param>
        /// <param name="content">Содержимое файла.</param>
        /// <returns></returns>
        [OperationContract]
        WcfFileInfo UploadFile(string folderUrl, string fileName, byte[] content);

        /// <summary>
        /// Обновляет содержимое файла в хранилище.
        /// </summary>
        /// <param name="folderUrl">Адрес папки.</param>
        /// <param name="fileUniqueID">Уникальный идентификатор файла.</param>
        /// <param name="content">Содержимое файла.</param>
        /// <param name="fileName">Имя файла.</param>
        /// <returns></returns>
        [OperationContract]
        WcfFileInfo UpdateFile(string folderUrl, Guid fileUniqueID, byte[] content, string fileName = null);

        /// <summary>
        /// Обновляет содержимое файла в хранилище.
        /// </summary>
        /// <param name="fileUrl">Адрес файла.</param>
        /// <param name="content">Содержимое файла.</param>
        /// <param name="fileName">Имя файла.</param>
        /// <returns></returns>
        [OperationContract(Name = "UpdateFileByUrl")]
        WcfFileInfo UpdateFile(string fileUrl, byte[] content, string fileName = null);

        /// <summary>
        /// Удаляет файл.
        /// </summary>
        /// <param name="folderUrl">Адрес папки.</param>
        /// <param name="fileUniqueID">Уникальный идентификатор файла.</param>
        /// <returns></returns>
        [OperationContract]
        bool DeleteFile(string folderUrl, Guid fileUniqueID);

        /// <summary>
        /// Возвращает файл.
        /// </summary>
        /// <param name="folderUrl">Адрес папки.</param>
        /// <param name="fileUniqueID">Уникальный идентификатор файла.</param>
        /// <param name="loadOptions">Опции загрузки.</param>
        /// <returns></returns>
        [OperationContract]
        WcfFileInfo GetFile(string folderUrl, Guid fileUniqueID, GetFileOptions loadOptions = null);

        /// <summary>
        /// Возвращает файл.
        /// </summary>
        /// <param name="fileUrl">Адрес файла.</param>
        /// <param name="loadOptions">Опции загрузки.</param>
        /// <returns></returns>
        [OperationContract(Name = "GetFileByUrl")]
        WcfFileInfo GetFile(string fileUrl, GetFileOptions loadOptions = null);

        /// <summary>
        /// Возвращает содержимое файла.
        /// </summary>
        /// <param name="folderUrl">Адрес папки.</param>
        /// <param name="fileUniqueID">Уникальный идентификатор файла.</param>
        /// <returns></returns>
        [OperationContract]
        byte[] GetFileContent(string folderUrl, Guid fileUniqueID);

        /// <summary>
        /// Возвращает содержимое версии файла.
        /// </summary>
        /// <param name="folderUrl">Адрес папки.</param>
        /// <param name="fileUniqueID">Уникальный идентификатор файла.</param>
        /// <param name="versionUniqueID">Уникальный идентификатор версии файла.</param>
        /// <returns></returns>
        [OperationContract]
        byte[] GetFileVersionContent(string folderUrl, Guid fileUniqueID, Guid versionUniqueID);

        /// <summary>
        /// Возвращает коллекию версий файлов.
        /// </summary>
        /// <param name="folderUrl">Адрес папки.</param>
        /// <param name="fileUniqueID">Уникальный идентификатор файла.</param>
        /// <returns></returns>
        [OperationContract]
        WcfFileVersionInfo[] GetFileVersions(string folderUrl, Guid fileUniqueID);
        #endregion

        #region Session Links
        /// <summary>
        /// Возвращает сессионную ссылку на версию файла.
        /// </summary>
        /// <param name="fileUrl">Адрес файла.</param>
        /// <param name="versionUniqueID">Уникальный идентификатор версии файла.</param>
        /// <returns></returns>
        [OperationContract(Name = "GetSessionFileLinkByUrl")]
        string GetSessionFileLink(string fileUrl, Guid versionUniqueID);

        /// <summary>
        /// Возвращает сессионную ссылку на версию файла.
        /// </summary>
        /// <param name="fileUrl">Адрес файла.</param>
        /// <param name="versionUniqueID">Уникальный идентификатор версии файла.</param>
        /// <returns></returns>
        [OperationContract(Name = "GetUserSessionFileLinkByUrl")]
        string GetUserSessionFileLink(string fileUrl, Guid versionUniqueID, string userLogin);

        /// <summary>
        /// Возвращает сессионную ссылку на версию файла.
        /// </summary>
        /// <param name="folderUrl">Адрес папки.</param>
        /// <param name="fileUrl">Адрес файла.</param>
        /// <param name="versionUniqueID">Уникальный идентификатор версии файла.</param>
        /// <returns></returns>
        [OperationContract]
        string GetSessionFileLink(string folderUrl, Guid fileUniqueID, Guid versionUniqueID);

        /// <summary>
        /// Возвращает сессионную ссылку на версию файла.
        /// </summary>
        /// <param name="folderUrl">Адрес папки.</param>
        /// <param name="fileUrl">Адрес файла.</param>
        /// <param name="versionUniqueID">Уникальный идентификатор версии файла.</param>
        /// <returns></returns>
        [OperationContract]
        string GetUserSessionFileLink(string folderUrl, Guid fileUniqueID, Guid versionUniqueID, string userLogin);
        #endregion

        #region Tokens
        /// <summary>
        /// Вызыет токен доступа на операцию с файловым хранилищем.
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        WcfTokenInfo GetAccessToken();
        #endregion
    }
}