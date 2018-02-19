using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storage.Data.Blob
{
    /// <summary>
    /// Метаданные блоба.
    /// </summary>
    public interface IBlobMetadata
    {
        /// <summary>
        /// Локальный идентификатор блоба.
        /// </summary>
        int ID { get; }

        /// <summary>
        /// Локальный идентификатор контейнера блобов.
        /// </summary>
        int ContainerID { get; }

        /// <summary>
        /// Имя блоба.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Блоб закрыт для редактирования.
        /// </summary>
        bool Closed { get; set; }

        /// <summary>
        /// Позиция, файлы до которой имеют целостность метаданных.
        /// </summary>
        long IntegrityPosition { get; }

        /// <summary>
        /// Обновляет позицию, файлы до которой имеют целостность метаданных.
        /// </summary>
        /// <param name="integrityPosition"></param>
        void UpdateIntegrityPosition(long integrityPosition);
    }
}