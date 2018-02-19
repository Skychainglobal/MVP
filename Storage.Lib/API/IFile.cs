using System;
using System.Collections.Generic;
using System.IO;

namespace Storage.Lib
{
    /// <summary>
    /// Файл.
    /// </summary>
    public interface IFile : IDisposable
    {
        /// <summary>
        /// Хранилище.
        /// </summary>
        IStorage Storage { get; }

        /// <summary>
        /// Папка.
        /// </summary>
        IFolder Folder { get; }

        /// <summary>
        /// Уникальный идентификатор файла.
        /// </summary>
        Guid UniqueID { get; }

        /// <summary>
        /// Имя файла.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Содержимое файла.
        /// </summary>
        byte[] Content { get; }

        /// <summary>
        /// Размер файла.
        /// </summary>
        long Size { get; }

        /// <summary>
        /// Расширение файла.
        /// </summary>
        string Extension { get; }

        /// <summary>
        /// Адрес папки файла.
        /// </summary>
        string FolderUrl { get; }

        /// <summary>
        /// Адрес файла.
        /// </summary>
        string Url { get; }

        /// <summary>
        /// Дата создания файла.
        /// </summary>
        DateTime TimeCreated { get; }

        /// <summary>
        /// Дата последнего изменения файла.
        /// </summary>
        DateTime TimeModified { get; }

        /// <summary>
        /// Уникальный идентификатор версии файла.
        /// </summary>
        Guid VersionUniqueID { get; }

        /// <summary>
        /// Коллекция версий файла.
        /// </summary>
        IReadOnlyCollection<IFileVersion> Versions { get; }



        /// <summary>
        /// Возвращает версию файла.
        /// </summary>
        /// <param name="versionUniqueID">Уникальный идентификатор версии.</param>
        /// <param name="throwIfNotexists">Выбросить исключение, если версии не существует?</param>
        /// <returns></returns>
        IFileVersion GetVersion(Guid versionUniqueID, bool throwIfNotexists = true);

        /// <summary>
        /// Удаляет файл из хранилища.
        /// </summary>
        void Delete();

        /// <summary>
        /// Обновляет содержимое файла.
        /// </summary>
        /// <param name="stream">Новое содержимое файла.</param>
        /// <param name="fileName">Новое имя файла.</param>
        void Update(Stream stream, string fileName = null);

        /// <summary>
        /// Открывает поток чтения данных файла.
        /// </summary>
        /// <returns></returns>
        Stream Open();
    }
}