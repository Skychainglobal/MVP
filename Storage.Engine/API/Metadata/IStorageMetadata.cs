using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storage.Engine
{
    /// <summary>
    /// Метаданные файлового хранилища.
    /// </summary>
    public interface IStorageMetadata : IEngineObjectMetadata
    {
        /// <summary>
        /// Уникальный идентификатор.
        /// </summary>
        Guid UniqueID { get; }

        /// <summary>
        /// Адрес службы хранилища.
        /// </summary>
        string Host { get; set; }

        /// <summary>
        /// Признак: является текущей нодой.
        /// </summary>
        bool IsCurrent { get; set; }

        /// <summary>
        /// Дата последней связи с узлом.
        /// </summary>
        DateTime LastAccessTime { get; set; }
    }
}