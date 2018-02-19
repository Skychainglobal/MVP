using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skychain.Models
{
    /// <summary>
    /// Представляет схему тренировки нейросети.
    /// </summary>
    public interface ISkyTrainScheme : ISkyObject
    {
        /// <summary>
        /// Нейросеть, для которой соответствует данная схема тренировки.
        /// </summary>
        ISkyNetwork Network { get; }

        /// <summary>
        /// Название схемы тренировки нейросети.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Эпохи прохождения тренировочного сета.
        /// </summary>
        IEnumerable<ISkyTrainEpoch> Epochs { get; }

        /// <summary>
        /// Коллекция параметров прохождения тренировочного сета для наборов эпох.
        /// </summary>
        IEnumerable<ISkyTrainEpochParams> EpochParamsSet { get; }

        /// <summary>
        /// Создаёт экземпляр параметров прохождения тренировочного сета для набора эпох, без сохранения в базу данных.
        /// </summary>
        ISkyTrainEpochParams CreateEpochParams();

        /// <summary>
        /// Проверяет корректность и обновляет номера эпох, а также обновляет остальные свойства для всех параметров эпох.
        /// </summary>
        void UpdateEpochs();
    }
}
