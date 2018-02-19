using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skychain.Models
{
    /// <summary>
    /// Представляет пользователя системы.
    /// </summary>
    public interface ISkyUser
    {
        /// <summary>
        /// Идентификатор пользователя.
        /// </summary>
        Guid ID { get; }

        /// <summary>
        /// Контекст системы.
        /// </summary>
        ISkyContext Context { get; }

        /// <summary>
        /// Профиль пользователя.
        /// Генерирует исключение в случае отсутствия профиля.
        /// </summary>
        ISkyProfile Profile { get; }

        /// <summary>
        /// Возвращает true, если для пользователя создан профиль.
        /// </summary>
        bool HasProfile { get; }

        /// <summary>
        /// Проверяет наличие профиля.
        /// Генерирует исключение в случае отсутствия профиля.
        /// </summary>
        void CheckProfile();

        /// <summary>
        /// Создаёт новый экземпляр профиля, соответствующего пользователю системы, без сохранения в базу данных.
        /// </summary>
        ISkyProfile CreateProfile();

        /// <summary>
        /// Последние запросы пользователя к нейросетям.
        /// </summary>
        IEnumerable<ISkyNetworkRequest> RecentRequests { get; }
    }
}
