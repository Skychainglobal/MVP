using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storage.Data.Blob
{
    /// <summary>
    /// Интерфейс заголовка файла.
    /// </summary>
    public interface IBlobFileHeader
    {
        /// <summary>
        /// Начальная позиция системного заголовка файла в блобе.
        /// </summary>
        long ContentAbsoluteStartPosition { get; }

        /// <summary>
        /// Длина содержимого.
        /// </summary>
        long ContentLength { get; }

        /// <summary>
        /// Длина заголовка.
        /// </summary>
        int HeaderLength { get; }
    }
}