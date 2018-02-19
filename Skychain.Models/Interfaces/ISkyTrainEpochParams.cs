using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skychain.Models
{
    /// <summary>
    /// Представляет параметры прохождения тренировочного сета для набора эпох.
    /// </summary>
    public interface ISkyTrainEpochParams : ISkyObject
    {
        /// <summary>
        /// Схема тренировок, к которой относятся параметры прохождения.
        /// </summary>
        ISkyTrainScheme TrainScheme { get; }

        /// <summary>
        /// Начальная эпоха, которой соответствуют параметры прохождения тренировочного сета.
        /// </summary>
        int StartEpochNumber { get; set; }

        /// <summary>
        /// Конечная эпоха, которой соответствуют параметры прохождения тренировочного сета.
        /// </summary>
        int EndEpochNumber { get; set; }

        /// <summary>
        /// Набор данных для тренировки.
        /// Генерирует исключение в случае отсутствия набора данных.
        /// </summary>
        ISkyDataSet DataSet { get; }

        /// <summary>
        /// Устанавливает набор данных для тренировки сети.
        /// </summary>
        /// <param name="dataSetID">Идентификатор набора данных.</param>
        void ChangeDataSet(int dataSetID);

        /// <summary>
        /// Возвращает true, если в параметрах тренировки указан набор данных.
        /// </summary>
        bool HasDataSet { get; }

        /// <summary>
        /// Произвольные параметры тренировки.
        /// </summary>
        string CustomParameters { get; set; }
    }
}
