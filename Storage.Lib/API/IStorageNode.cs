using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storage.Lib
{
    public interface IStorageNode
    {
        /// <summary>
        /// Уникальный идентификатор хранилища.
        /// </summary>
        Guid UniqueID { get; }

        /// <summary>
        /// Хост узла файлового хранилища.
        /// </summary>
        string Host { get; }

        /// <summary>
        /// Узел файлового хранилища является текущим по отношению к текущему контексту кода.
        /// </summary>
        bool IsCurrent { get; }
    }
}