using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Storage.Lib;

namespace Storage.Engine
{
    /// <summary>
    /// Интерфейс для объекта папки репликации.
    /// </summary>
    public interface IReplicationFolder
    {
        /// <summary>
        /// Папка репликации.
        /// </summary>
        IFolder Folder { get; }

        /// <summary>
        /// Целевой узел хранилища.
        /// </summary>
        IStorageNode SourceStorage { get; }

        /// <summary>
        /// Признак: реплицировать рекурсивно.
        /// </summary>
        bool IsRecursive { get; }
    }
}