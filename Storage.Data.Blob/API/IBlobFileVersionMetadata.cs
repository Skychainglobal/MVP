using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Storage.Engine;

namespace Storage.Data.Blob
{
    /// <summary>
    /// Метаданные файла.
    /// </summary>
    public interface IBlobFileVersionMetadata : IFileVersionMetadata
    {
        /// <summary>
        /// Локальный идентификатор блоба.
        /// </summary>
        int BlobID { get; set; }

        /// <summary>
        /// Начальная позиция в блобе.
        /// </summary>
        long BlobStartPosition { get; set; }

        /// <summary>
        /// Окончательная позиция в блобе.
        /// </summary>
        long BlobEndPosition { get; set; }
    }
}