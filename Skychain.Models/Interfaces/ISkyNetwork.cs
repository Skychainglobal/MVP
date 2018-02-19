using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skychain.Models
{
    /// <summary>
    /// Представляет нейросеть.
    /// </summary>
    public interface ISkyNetwork : ISkyObject
    {
        /// <summary>
        /// Профиль, которому принадлежит нейросеть.
        /// </summary>
        ISkyProfile Profile { get; }

        /// <summary>
        /// Название нейросети.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Описание нейросети.
        /// </summary>
        string Description { get; set; }

        /// <summary>
        /// Стоимость использования нейросети.
        /// </summary>
        decimal Cost { get; set; }

        /// <summary>
        /// Возвращает true, если нейросеть опубликована.
        /// </summary>
        bool Published { get; set; }

        /// <summary>
        /// Данные текущего состояния нейросети.
        /// Изменяются после выполнения тренировки сети.
        /// </summary>
        byte[] StateData { get; set; }

        /// <summary>
        /// Проверяет, является ли нейросеть опубликованной, в ином случае генерирует исключение.
        /// </summary>
        void CheckPublished();

        /// <summary>
        /// Рейтинг нейросети.
        /// </summary>
        double Rating { get; set; }

        /// <summary>
        /// Количество использований сети.
        /// </summary>
        int UsagesCount { get; }

        /// <summary>
        /// Активная версия нейросети.
        /// </summary>
        ISkyNetworkVersion ActiveVersion { get; }

        /// <summary>
        /// Проверяет наличие активной версии нейросети.
        /// Генерирует исключение в случае отсутствия активной версии.
        /// </summary>
        void CheckActiveVersion();

        /// <summary>
        /// Версии нейросети.
        /// </summary>
        IEnumerable<ISkyNetworkVersion> Versions { get; }

        /// <summary>
        /// Создаёт новый экземпляр версии нейросети, без сохранения в базу данных.
        /// </summary>
        ISkyNetworkVersion CreateVersion();

        /// <summary>
        /// Возвращает версию нейросети по идентификатору версии в рамках нейросети.
        /// </summary>
        /// <param name="versionInternalID">Идентификатор версии в рамках нейсросети.</param>
        ISkyNetworkVersion GetVersion(int versionInternalID);

        /// <summary>
        /// Возвращает версию нейросети по идентификатору версии в рамках нейросети.
        /// </summary>
        /// <param name="versionInternalID">Идентификатор версии в рамках нейсросети.</param>
        /// <param name="throwNotFoundException">Генерирует исключение в случае отсутствия версии с заданным идентификатором в рамках нейросети.</param>
        ISkyNetworkVersion GetVersion(int versionInternalID, bool throwNotFoundException);

        /// <summary>
        /// Схемы тренировок нейросети.
        /// </summary>
        IEnumerable<ISkyTrainScheme> TrainSchemes { get; }

        /// <summary>
        /// Создаёт новый экземпляр схемы тренировки нейросети, без сохранения в базу данных.
        /// </summary>
        ISkyTrainScheme CreateTrainScheme();

        /// <summary>
        /// Создаёт новый экземляр запроса к активному состоянию активной версии данной нейросети.
        /// </summary>
        /// <param name="user">Пользователь, формирующий запрос.</param>
        ISkyNetworkRequest CreateRequest(ISkyUser user);
    }
}
