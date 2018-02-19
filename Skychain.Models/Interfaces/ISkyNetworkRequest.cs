using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skychain.Models
{
    /// <summary>
    /// Представляет запрос пользователя к нейросети.
    /// </summary>
    public interface ISkyNetworkRequest : ISkyObject
    {
        /// <summary>
        /// Нейросеть, к которой выполняется запрос.
        /// </summary>
        ISkyNetwork Network { get; }

        /// <summary>
        /// Используемая версия нейросети.
        /// </summary>
        ISkyNetworkVersion NetworkVersion { get; }

        /// <summary>
        /// Состояние нейросети, на основе которого формируется результат.
        /// </summary>
        ISkyNetworkState NetworkState { get; }

        /// <summary>
        /// Пользователь, формирующий запрос к нейросети.
        /// </summary>
        ISkyUser User { get; }

        /// <summary>
        /// Статус запроса к нейросети.
        /// </summary>
        SkyNetworkRequestStatus Status { get; }

        /// <summary>
        /// Стоимость использования нейросети на момент создания запроса.
        /// </summary>
        decimal NetworkCost { get; }

        /// <summary>
        /// Данные, введённые пользователем, в соответствии с форматом манифеста.
        /// </summary>
        string InputData { get; set; }

        /// <summary>
        /// Проверяет наличие данных, введённых пользователем, при формировании запроса.
        /// </summary>
        void CheckInputData();

        /// <summary>
        /// Создаёт экземпляр для загрузки файла, выбранного пользователем на форме входных данных запроса к нейросети.
        /// </summary>
        ISkyFile CreateInputFile();

        /// <summary>
        /// Возвращает экземпляр файла, выбранного пользователем на форме входных данных запроса к нейросети.
        /// Метод всегда возвращает значение, отличающееся от null.
        /// </summary>
        /// <param name="fileID">Идентификатор файла.</param>
        ISkyFile GetInputFile(Guid fileID);

        /// <summary>
        /// Данные результата запроса, в соответствии с форматом манифеста.
        /// </summary>
        string ResultData { get; set; }

        /// <summary>
        /// Проверяет наличие данных результата запроса.
        /// </summary>
        void CheckResultData();

        /// <summary>
        /// Время начала запроса.
        /// </summary>
        DateTime StartTime { get;  }

        /// <summary>
        /// Время начала окончания.
        /// </summary>
        DateTime EndTime { get; }

        /// <summary>
        /// Сообщение об ошибке, возникшей при выполнении запроса.
        /// </summary>
        string ErrorMessage { get; set; }

        /// <summary>
        /// Возвращает true, если обработка запроса была выполнена без ошибок.
        /// </summary>
        bool Succeed { get; }

        /// <summary>
        /// Создаёт запрос к нейросети.
        /// </summary>
        void Create();

        /// <summary>
        /// Устаналивает статус обработки запроса нейросетью в данный момент времени.
        /// </summary>
        void Process();

        /// <summary>
        /// Вычисляет результат обработки запроса к нейросети.
        /// </summary>
        void ComputeResult();

        /// <summary>
        /// Устанавливает статус завершения запроса к нейросети.
        /// </summary>
        void Complete();
    }

    /// <summary>
    /// Представляет статус запроса к нейросети.
    /// </summary>
    public enum SkyNetworkRequestStatus
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
