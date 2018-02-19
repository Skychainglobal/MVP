using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skychain.Models
{
    /// <summary>
    /// Представляет интерфейс объекта системы.
    /// </summary>
    public interface ISkyObject
    {
        /// <summary>
        /// Контекст системы.
        /// </summary>
        ISkyContext Context { get; }

        /// <summary>
        /// Уникальный идентификатор объекта в базе данных.
        /// </summary>
        int ID { get; }


        /// <summary>
        /// Возвращает true, если объект является новым и не был сохранён в базу данных.
        /// </summary>
        bool IsNew { get; }

        /// <summary>
        /// Возвращает true, если экземпляр объекта был создан в текущем контексте выполнения кода.
        /// </summary>
        bool JustCreated { get; }

        /// <summary>
        /// Номер версии объекта, увеличиваемый при каждом обновлении объекта.
        /// </summary>
        int VersionNumber { get; }

        /// <summary>
        /// Дата и время сохранения нового объекта.
        /// </summary>
        DateTime TimeCreated { get; }

        /// <summary>
        /// Дата и время последнего изменения объекта.
        /// </summary>
        DateTime TimeModified { get; }

        /// <summary>
        /// Обновляет объект в базе данных.
        /// </summary>
        void Update();

        /// <summary>
        /// Удаляет объект из базы данных.
        /// </summary>
        void Delete();

        /// <summary>
        /// Проверяет, что объект существует и был сохранён в базу данных.
        /// Генерирует исключение в ином случае.
        /// </summary>
        void CheckExists();

        /// <summary>
        /// Проверяет, что объект является новым и ещё не был сохранён в базу данных.
        /// Генерирует исключение в ином случае.
        /// </summary>
        void CheckIsNew();
    }
}
