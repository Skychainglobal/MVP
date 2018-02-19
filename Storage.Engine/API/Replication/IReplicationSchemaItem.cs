using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storage.Engine
{
    /// <summary>
    /// Настройка элемента схемы репликации для узла хранилища.
    /// </summary>
    public interface IReplicationSchemaItem
    {
        /// <summary>
        /// Название хранилища.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// ID хранилища.
        /// </summary>
        Guid StorageID { get; }

        /// <summary>
        /// Тип настройки элемента схемы репликации.
        /// </summary>
        ReplicationRelation RelationType { get; }

        /// <summary>
        /// Реплицируемые папки.
        /// </summary>
        IEnumerable<string> Folders { get; }
    }
}
