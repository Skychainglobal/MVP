using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storage.Engine
{
    /// <summary>
    /// Файловый токен.
    /// </summary>
    public interface IFileToken
    {
        /// <summary>
        /// Уникальный идентификатор токена.
        /// </summary>
        Guid UniqueID { get; }

        /// <summary>
        /// Адрес папки с файлом.
        /// </summary>
        string FolderUrl { get; }

        /// <summary>
        /// Уникальный идентификатор файла.
        /// </summary>
        Guid FileUniqueID { get; }

        /// <summary>
        /// Уникальный идентификатор версии файла.
        /// </summary>
        Guid VersionUniqueID { get; }

        /// <summary>
        /// Идентификатор безопасности токена.
        /// </summary>
        string SecurityIdentifier { get; }

        /// <summary>
        /// Дата, до которой выдан токен.
        /// </summary>
        DateTime Expired { get; }
    }
}