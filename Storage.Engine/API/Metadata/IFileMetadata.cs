using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storage.Engine
{
    /// <summary>
    /// Метаданные файла.
    /// </summary>
    public interface IFileMetadata : IEngineFileMetadata
    {
        /// <summary>
        /// Уникальный идентификатор.
        /// </summary>
        Guid UniqueID { get; }

        /// <summary>
        /// Уникальный идентификатор текущей версии.
        /// </summary>
        Guid VersionUniqueID { get; }

        /// <summary>
        /// Имя файла.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Дата создания.
        /// </summary>
        DateTime TimeCreated { get; }

        /// <summary>
        /// Дата последнего изменения.
        /// </summary>
        DateTime TimeModified { get; }

        /// <summary>
        /// Размер файла.
        /// </summary>
        long Size { get; }

        /// <summary>
        /// Удален?
        /// </summary>
        bool Deleted { get; }

        /// <summary>
        /// Метаданные папки файла.
        /// </summary>
        IFolderMetadata FolderMetadata { get; }
    }
}