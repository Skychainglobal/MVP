using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skychain.Models
{
    /// <summary>
    /// Представляет запрос тренировки нейросети.
    /// </summary>
    public interface ISkyTrainRequest : ISkyObject
    {
        /// <summary>
        /// Нейросеть, для которой выполняется тренировка.
        /// </summary>
        ISkyNetwork Network { get; }

        /// <summary>
        /// Тренируемая версия нейросети.
        /// </summary>
        ISkyNetworkVersion NetworkVersion { get; }

        /// <summary>
        /// Схема тренировки нейросети.
        /// </summary>
        ISkyTrainScheme TrainScheme { get; }

        /// <summary>
        /// Состояние нейросети, на основе которого выполняется тренировка.
        /// Возвращает null в случае, если тренировка является первичной.
        /// </summary>
        ISkyNetworkState InitialState { get; }

        /// <summary>
        /// Пользователь, выполняющий тренировку нейросети.
        /// </summary>
        ISkyUser User { get; }

        /// <summary>
        /// Статус запроса тренировки.
        /// </summary>
        SkyTrainRequestStatus Status { get; }

        /// <summary>
        /// Состояние нейросети, сформированное в результате тренировки.
        /// </summary>
        ISkyNetworkState ResultState { get; }

        /// <summary>
        /// Время начала запроса.
        /// </summary>
        DateTime StartTime { get; }

        /// <summary>
        /// Время начала окончания.
        /// </summary>
        DateTime EndTime { get; }

		/// <summary>
		/// Описание результирующего состояния.
		/// </summary>
		string ResultStateDescription { get; set; }

		/// <summary>
		/// Лог, записываемый во время выполнения тренировки.
		/// </summary>
		string TrainLog { get; }

        /// <summary>
        /// Сообщение об ошибке, возникшей при выполнении запроса.
        /// </summary>
        string ErrorMessage { get; set; }

        /// <summary>
        /// Возвращает true, если обработка запроса была выполнена без ошибок.
        /// </summary>
        bool Succeed { get; }

        /// <summary>
        /// Создаёт запрос тренировки.
        /// </summary>
        void Create();

        /// <summary>
        /// Устаналивает статус выполнения тренировки в данный момент времени.
        /// </summary>
        void Process();

        /// <summary>
        /// Выполняет тренировку.
        /// </summary>
        void Train();

        /// <summary>
        /// Записывает сообщение в лог тренировки нейросети.
        /// </summary>
        /// <param name="message">Текст записываемого в лог сообщения.</param>
        void WriteTrainLog(string message);

        /// <summary>
        /// Устанавливает статус завершения тренировки.
        /// </summary>
        void Complete();
    }

    /// <summary>
    /// Представляет статус запроса тренировки нейросети.
    /// </summary>
    public enum SkyTrainRequestStatus
    {
        /// <summary>
        /// Создание запроса.
        /// </summary>
        Creating = 1,

        /// <summary>
        /// Выполнение запроса.
        /// </summary>
        Processing = 2,

        /// <summary>
        /// Запрос выполнен.
        /// </summary>
        Completed = 3
    }
}
