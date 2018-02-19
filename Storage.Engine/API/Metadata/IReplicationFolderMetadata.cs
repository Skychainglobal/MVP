using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storage.Engine
{
    /// <summary>
    /// Метаданные директории для репликации.
    /// </summary>
    public interface IReplicationFolderMetadata : IEngineObjectMetadata
    {
        /// <summary>
        /// Папка репликации.
        /// </summary>
        IFolderMetadata Folder { get; }

        /// <summary>
        /// Целевой узел.
        /// </summary>
        IStorageMetadata SourceStorage { get; }

        /// <summary>
        /// Признак: реплицировать рекурсивно.
        /// </summary>
        bool IsRecursive { get; set; }

        /// <summary>
        /// Признак: не производить репликацию.
        /// </summary>
        bool Deleted { get; set; }

        /// <summary>
        /// Метка времени - последняя дата получения файлов.
        /// </summary>
        DateTime LastSyncTime { get; set; }

        /// <summary>
        /// Настройка с текущего узла файлового хранилища.
        /// </summary>
        bool IsCurrentNodeSettings { get; set; }
    }
}
