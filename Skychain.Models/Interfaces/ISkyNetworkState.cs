using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skychain.Models
{
    /// <summary>
    /// Представляет состояние нейросети, сформированное в результате тренировки.
    /// Используется для выполнения запросов к нейросети.
    /// </summary>
    public interface ISkyNetworkState : ISkyObject
    {
        /// <summary>
        /// Нейросеть, которой соответствует состояние.
        /// </summary>
        ISkyNetwork Network { get; }

        /// <summary>
        /// Версия нейросети, которой соответствует состояние.
        /// </summary>
        ISkyNetworkVersion NetworkVersion { get; }

        /// <summary>
        /// Идентификатор состояния в рамках версии нейросети.
        /// </summary>
        int InternalID { get; }

        /// <summary>
        /// Проверяет наличие идентификатора состояния в рамках версии нейросети.
        /// Генерирует исключение в случае отсутствия.
        /// </summary>
        void CheckInternalID();

        /// <summary>
        /// Схема тренировки, по которой было сформировано состояние нейросети.
        /// Возвращает null в случае удаления схемы тренировки нейросети.
        /// </summary>
        ISkyTrainScheme TrainScheme { get; }

        /// <summary>
        /// Состояние нейросети, на основе которого было инициализировано данное состояние.
        /// Возвращает null в случае, если данное состояние является первичным.
        /// </summary>
        ISkyNetworkState InitialState { get; }

        /// <summary>
        /// Описание состояния нейросети.
        /// </summary>
        string Description { get; set; }

        /// <summary>
        /// Отображаемое название состояния нейросети.
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Коллекция состояний сети, инициализированных на основе данного состояния.
        /// </summary>
        IEnumerable<ISkyNetworkState> DerivedStates { get; }

        /// <summary>
        /// Данные состояния нейросети, через которое будут выполняться запросы к нейросети.
        /// Свойство всегда возвращает существующий экземпляр класса.
        /// </summary>
        ISkyFile StateData { get; }

        /// <summary>
        /// Проверяет наличие данных состояния нейросети.
        /// Генерирует исключение в случае отсутствия.
        /// </summary>
        void CheckStateData();

        /// <summary>
        /// Параметры состояния нейросети.
        /// Может содержать ошибку прохождения тренировочной схему и другие параметры.
        /// </summary>
        string StateParameters { get; set; }

        /// <summary>
        /// Создаёт новый экземляр запроса к данному состоянию нейросети.
        /// </summary>
        /// <param name="user">Пользователь, формирующий запрос.</param>
        ISkyNetworkRequest CreateRequest(ISkyUser user);
    }
}
