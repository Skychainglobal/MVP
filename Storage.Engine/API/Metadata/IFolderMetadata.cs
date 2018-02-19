using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storage.Engine
{
    /// <summary>
    /// Метаданные папки.
    /// </summary>
    public interface IFolderMetadata
    {
        /// <summary>
        /// Локальный идентификатор.
        /// </summary>
        int ID { get; }

        /// <summary>
        /// Уникальный идентификатор.
        /// </summary>
        Guid UniqueID { get; }

        /// <summary>
        /// Адрес.
        /// </summary>
        string Url { get; }

        /// <summary>
        /// Имя.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Локальный идентификатор родительской папки.
        /// </summary>
        int ParentID { get; }
    }
}