using System;
using System.Collections.Generic;
using System.IO;

namespace Storage.Lib
{
    /// <summary>
    /// Версия файла.
    /// </summary>
    public interface IFileVersion : IDisposable
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
        /// Файл.
        /// </summary>
        IFile File { get; }

        /// <summary>
        /// Уникальный идентификатор версии.
        /// </summary>
        Guid UniqueID { get; }

        /// <summary>
        /// Имя файла версии.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Содержимое файла версии.
        /// </summary>
        byte[] Content { get; }

        /// <summary>
        /// Размер файла версии.
        /// </summary>
        long Size { get; }

        /// <summary>
        /// Расширение файла версии.
        /// </summary>
        string Extension { get; }

        /// <summary>
        /// Дата создания версии.
        /// </summary>
        DateTime TimeCreated { get; }

        /// <summary>
        /// Текущая версия?
        /// </summary>
        bool IsCurrent { get; }

        /// <summary>
        /// Идентификатор хранилища, на котором была создана версия.
        /// </summary>
        Guid CreatedStorageID { get; }

        /// <summary>
        /// Открывает поток чтения данных версии файла.
        /// </summary>
        /// <returns></returns>
        Stream Open();
    }
}