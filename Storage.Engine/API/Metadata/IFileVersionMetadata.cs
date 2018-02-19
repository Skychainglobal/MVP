using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storage.Engine
{
    /// <summary>
    /// Метаданные версии файла.
    /// </summary>
    public interface IFileVersionMetadata : IEngineFileVersionMetadata
    {
        /// <summary>
        /// Уникальный идентификатор.
        /// </summary>
        Guid UniqueID { get; }

        /// <summary>
        /// Уникальный идентификатор хранилища, на котором создана данная версия.
        /// </summary>
        Guid CreatedStorageUniqueID { get; }

        /// <summary>
        /// Имя файла.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Дата создания.
        /// </summary>
        DateTime TimeCreated { get; }

        /// <summary>
        /// Размер версии.
        /// </summary>
        long Size { get; }

        /// <summary>
        /// Метаданные файла.
        /// </summary>
        IFileMetadata FileMetadata { get; }
    }
}