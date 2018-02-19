using System;
using System.Collections.Generic;
using System.IO;

namespace Storage.Lib
{
    /// <summary>
    /// Хранилище.
    /// </summary>
    public interface IStorage : IDisposable
    {
        /// <summary>
        /// Коллекция корневых папок хранилища.
        /// </summary>
        IReadOnlyCollection<IFolder> Folders { get; }

        /// <summary>
        /// Папка по-умолчанию.
        /// </summary>
        IFolder DefaultFolder { get; }

        /// <summary>
        /// Возврает папку по адресу.
        /// </summary>
        /// <param name="url">Адрес папки.</param>
        /// <param name="throwIfNotExists">Выбросить исключение, если папка не будет найдена?</param>
        /// <returns></returns>
        IFolder GetFolder(string url, bool throwIfNotExists = true);

        /// <summary>
        /// Возвращает папку по адресу и создает ее, если папка не существует.
        /// </summary>
        /// <param name="url">Адрес папки.</param>
        /// <returns></returns>
        IFolder EnsureFolder(string url);

        /// <summary>
        /// Возвращает файл по адресу.
        /// </summary>
        /// <param name="fileUrl">Адрес файла.</param>
        /// <param name="loadOptions">Опции загрузки файла.</param>
        /// <param name="throwIfNotExists">Выбросить исключение, если файл не существует?</param>
        /// <returns></returns>
        IFile GetFile(string fileUrl, GetFileOptions loadOptions = null, bool throwIfNotExists = true);

        /// <summary>
        /// Возвращает файл по адресу папки и уникальному идентификатору.
        /// </summary>
        /// <param name="folderUrl">Адрес папки.</param>
        /// <param name="fileUniqueID">Уникальный идентификатор файла.</param>
        /// <param name="loadOptions">Опции загрузки файла.</param>
        /// <param name="throwIfNotExists">Выбросить исключение, если файл не существует?</param>
        /// <returns></returns>
        IFile GetFile(string folderUrl, Guid fileUniqueID, GetFileOptions loadOptions = null, bool throwIfNotExists = true);

        /// <summary>
        /// Возвращает файл из папки по умолчанию по уникальному идентификатору.
        /// </summary>
        /// <param name="fileUniqueID">Уникальный идентификатор файла.</param>
        /// <param name="loadOptions">Опции загрузки файла.</param>
        /// <param name="throwIfNotExists">Выбросить исключение, если файл не существует?</param>
        /// <returns></returns>
        IFile GetFile(Guid fileUniqueID, GetFileOptions loadOptions = null, bool throwIfNotExists = true);

        /// <summary>
        /// Загружает файл хранилище.
        /// </summary>
        /// <param name="folderUrl">Адрес папки, в которую необходимо загрузить файл.</param>
        /// <param name="fileName">Имя файла.</param>
        /// <param name="stream">Содержимое файла.</param>
        /// <returns></returns>
        IFile UploadFile(string folderUrl, string fileName, Stream stream);

        /// <summary>
        /// Загружает файл в хранилище.
        /// </summary>
        /// <param name="fileName">Имя файла.</param>
        /// <param name="stream">Содержимое файла.</param>
        /// <returns></returns>
        IFile UploadFile(string fileName, Stream stream);

        /// <summary>
        /// Удаляет файл.
        /// </summary>
        /// <param name="folderUrl">Адрес папки.</param>
        /// <param name="fileUniqueID">Уникальный идентификатор файла.</param>
        /// <returns></returns>
        bool DeleteFile(string folderUrl, Guid fileUniqueID);

        /// <summary>
        /// Удаляет файл.
        /// </summary>
        /// <param name="file">Файл.</param>
        /// <returns></returns>
        void DeleteFile(IFile file);

        /// <summary>
        /// Возвращает сессионную ссылку на версию файла.
        /// </summary>
        /// <param name="folderUrl">Адрес папки с файлом.</param>
        /// <param name="fileUniqueID">Уникальный идентификатор файла.</param>
        /// <param name="versionUniqueID">Уникальный идентификатор версии файла.</param>
        /// <returns></returns>
        string GetSessionFileLink(string folderUrl, Guid fileUniqueID, Guid versionUniqueID);

        /// <summary>
        /// Возвращает сессионную ссылку на версию файла для определенного пользователя.
        /// </summary>
        /// <param name="folderUrl">Адрес папки с файлом.</param>
        /// <param name="fileUniqueID">Уникальный идентификатор файла.</param>
        /// <param name="versionUniqueID">Уникальный идентификатор версии файла.</param>
        /// <param name="userIdentity">Строка идентификации пользователя.</param>
        /// <returns></returns>
        string GetSessionFileLink(string folderUrl, Guid fileUniqueID, Guid versionUniqueID, string userIdentity);

        /// <summary>
        /// Возвращает сессионную ссылку на файл.
        /// </summary>
        /// <param name="fileUrl">Адрес файла.</param>
        /// <param name="versionUniqueID">Уникальный идентификатор версии файла</param>
        /// <returns></returns>
        string GetSessionFileLink(string fileUrl, Guid versionUniqueID);

        /// <summary>
        /// Возвращает сессионную ссылку на файл для определенного пользователя.
        /// </summary>
        /// <param name="fileUrl">Адрес файла.</param>
        /// <param name="versionUniqueID">Уникальный идентификатор версии файла</param>
        /// <param name="userIdentity">Строка идентификации пользователя.</param>
        /// <returns></returns>
        string GetSessionFileLink(string fileUrl, Guid versionUniqueID, string userIdentity);

        /// <summary>
        /// Запускает потоковое чтение данных файла.
        /// </summary>
        /// <param name="folderUrl">Адрес папки.</param>
        /// <param name="fileUniqueID">Уникальный идентификатор файла.</param>
        /// <returns></returns>
        Stream OpenFile(string folderUrl, Guid fileUniqueID);

        /// <summary>
        /// Запускает потоковое чтение данных версии файла.
        /// </summary>
        /// <param name="folderUrl">Адрес папки.</param>
        /// <param name="fileUniqueID">Уникальный идентификатор файла.</param>
        /// <param name="versionUniqueID">Уникальный идентификатор версии файла.</param>
        /// <returns></returns>
        Stream OpenFile(string folderUrl, Guid fileUniqueID, Guid versionUniqueID);
    }
}