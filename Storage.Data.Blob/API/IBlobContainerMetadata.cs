using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storage.Data.Blob
{
    /// <summary>
    /// Метаданные контейнера блобов.
    /// </summary>
    public interface IBlobContainerMetadata
    {
        /// <summary>
        /// Локальный идентификатор контейнера.
        /// </summary>
        int ID { get; }

        /// <summary>
        /// Локальный идентификатор папки контейнера.
        /// </summary>
        int FolderID { get; }

        /// <summary>
        /// Путь до физической директории контейнера.
        /// </summary>
        string Path { get; }

        /// <summary>
        /// Имя контейнера.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Контейнер закрыт?
        /// </summary>
        bool Closed { get; set; }
    }
}