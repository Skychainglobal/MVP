using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storage.Data.Blob
{
    public interface IFileHeader
    {
        /// <summary>
        /// Имя файла.
        /// </summary>
        string FileName { get; set; }

        /// <summary>
        /// Адрес папки файла.
        /// </summary>
        string FolderUrl { get; set; }

        /// <summary>
        /// Уникальный идентификатор файла.
        /// </summary>
        Guid UniqueID { get; set; }

        /// <summary>
        /// Дата создания файла.
        /// </summary>
        DateTime TimeCreated { get; set; }

        /// <summary>
        /// Идентификатор версии данного файла.
        /// </summary>
        Guid VersionUniqueID { get; set; }

        /// <summary>
        /// Идентификатор пользователя, изменившего файл.
        /// </summary>
        int ModifiedUserID { get; set; }
    }
}