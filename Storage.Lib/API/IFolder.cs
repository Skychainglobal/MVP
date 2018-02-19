using System;
using System.Collections.Generic;
using System.IO;

namespace Storage.Lib
{
    /// <summary>
    /// Папка.
    /// </summary>
    public interface IFolder : IDisposable
    {
        /// <summary>
        /// Уникальный идентификатор папки.
        /// </summary>
        Guid UniqueID { get; }

        /// <summary>
        /// Имя папки.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Адрес папки.
        /// </summary>
        string Url { get; }

        /// <summary>
        /// Хранилище.
        /// </summary>
        IStorage Storage { get; }

        /// <summary>
        /// Коллекция дочерних папок.
        /// </summary>
        IReadOnlyCollection<IFolder> Folders { get; }

        /// <summary>
        /// Родительская папка.
        /// </summary>
        IFolder ParentFolder { get; }

        /// <summary>
        /// Возвращает дочернюю папку по имени.
        /// </summary>
        /// <param name="name">Имя дочерней папки.</param>
        /// <param name="throwIfNotExists">Выбросить исключение, если папка не найдена?</param>
        /// <returns></returns>
        IFolder GetFolder(string name, bool throwIfNotExists = true);

        /// <summary>
        /// Возвращает дочернюю папку по имени и создает ее, если папка не существует.
        /// </summary>
        /// <param name="name">Имя дочерней папки.</param>
        /// <returns></returns>
        IFolder EnsureFolder(string name);

        /// <summary>
        /// Возвращает файл по идентификатору.
        /// </summary>
        /// <param name="fileUniqueID">Уникальный идентификатор файла.</param>
        /// <param name="loadOptions">Опции загрузки файла.</param>
        /// <param name="throwIfNotExists">Выбросить исключение, если файл не существует?</param>
        /// <returns></returns>
        IFile GetFile(Guid fileUniqueID, GetFileOptions loadOptions = null, bool throwIfNotExists = true);

        /// <summary>
        /// Загружает файл в хранилище.
        /// </summary>
        /// <param name="fileName">Имя файла.</param>
        /// <param name="stream">Содержимое файла.</param>
        /// <returns></returns>
        IFile UploadFile(string fileName, Stream stream);
    }
}