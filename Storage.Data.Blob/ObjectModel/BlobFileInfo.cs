using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storage.Data.Blob
{
    /// <summary>
    /// Информация о файле в блобе.
    /// </summary>
    internal struct BlobFileInfo
    {
        /// <summary>
        /// Идентификатор блоба.
        /// </summary>
        public int BlobID { get; set; }

        /// <summary>
        /// Начальная позиция файла в блобе.
        /// </summary>
        public long BlobStartPosition { get; set; }

        /// <summary>
        /// Конечная позиция файла в блобе.
        /// </summary>
        public long BlobEndPosition { get; set; }

        /// <summary>
        /// Дата создания версии файла.
        /// </summary>
        public DateTime TimeCreated { get; set; }
    }
}