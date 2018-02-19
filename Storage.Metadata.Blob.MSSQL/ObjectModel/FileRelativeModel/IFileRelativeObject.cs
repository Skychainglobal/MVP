using System;
using Storage.Engine;

namespace Storage.Metadata.Blob.MSSQL
{
    /// <summary>
    /// Интерфейс объекта относящегося к файлу, хранение которого может осуществляться в распределенных таблицах относительно файла.
    /// </summary>
    interface IFileRelativeObject
    {
        /// <summary>
        /// Метаданные файла.
        /// </summary>
        IFileMetadata FileMetadata { get; }
    }
}
