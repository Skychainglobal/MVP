using System;

namespace Storage.Metadata.MSSQL
{
    /// <summary>
    /// Представляет интерфейс объекта метаданных.
    /// </summary>
    public interface IMetadataObject
    {
        /// <summary>
        /// Идентификатор объекта.
        /// </summary>
        int ID { get; }
    }
}
