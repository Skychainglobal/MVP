using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storage.Engine
{
    /// <summary>
    /// Схема репликации сети узлов.
    /// </summary>
    public interface IReplicationSchema
    {
        /// <summary>
        /// Узел хранилища, на котором сформирована данная схема репликации сети.
        /// </summary>
        Guid StorageID { get; }

        /// <summary>
        /// Адрес хранилища, на котором сформирована данная схема репликации сети.
        /// </summary>
        string Host { get; }

        /// <summary>
        /// Элементы репликации с других узлов. Могут быть неактуальными.
        /// </summary>
        IReplicationSchemaItem[] WeakItems { get; }

        /// <summary>
        /// Элементы репликации с текущего узла.
        /// </summary>
        IReplicationSchemaItem[] StrongItems { get; }
    }
}