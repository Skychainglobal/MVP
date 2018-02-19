using System;
using Storage.Engine;

namespace Storage.Metadata.MSSQL
{
    /// <summary>
    /// Интерфейс объекта относящегося к файлу, хранение которого может осуществляться в распределенных таблицах.
    /// </summary>
    interface IFileRelativeObject
    {
        /// <summary>
        /// Метаданные файла.
        /// </summary>
        IFileMetadata File { get; }
    }
}
