using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skychain.Models
{
    /// <summary>
    /// Представляет эпоху прохождения тренировочного сета.
    /// </summary>
    public interface ISkyTrainEpoch
    {
        /// <summary>
        /// Номер эпохи.
        /// </summary>
        int EpochNumber { get; }

        /// <summary>
        /// Параметры прохождения эпохи.
        /// </summary>
        ISkyTrainEpochParams Params { get; }

        /// <summary>
        /// Схема тренировки нейросети, к которой относится эпоха.
        /// </summary>
        ISkyTrainScheme TrainScheme { get; }
    }
}
