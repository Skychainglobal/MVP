using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Storage.Engine;
using Storage.Lib;

namespace Storage.Data.Blob
{
    /// <summary>
    /// Метаданные файла.
    /// </summary>
    public interface IBlobFileMetadata : IFileMetadata
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

        /// <summary>
        /// Имя файла.
        /// </summary>
        new string Name { get; set; }

        /// <summary>
        /// Размер файла.
        /// </summary>
        new long Size { get; set; }

        /// <summary>
        /// Генерирует св-ва файла для сохранения.
        /// </summary>
        void EnsureSaveProperties();

        /// <summary>
        /// Устанавливает свойства локального файла и версии по файлу с удаленного узла.
        /// </summary>
        /// <param name="remoteFile">Файл с удаленного узла.</param>
        void EnsureRemoteSaveProperties(IRemoteFile remoteFile);
    }
}