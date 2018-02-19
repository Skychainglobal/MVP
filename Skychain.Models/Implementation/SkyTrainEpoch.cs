using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skychain.Models.Implementation
{
    /// <summary>
    /// Представляет эпоху прохождения тренировочного сета.
    /// </summary>
    public class SkyTrainEpoch : ISkyTrainEpoch
    {
        internal SkyTrainEpoch(int epochNumber, SkyTrainEpochParams epochParams, SkyTrainScheme trainScheme)
        {
            if (epochNumber <= 0)
                throw new ArgumentException("Epoch number must be greater than zero.", "epochNumber");
            if (epochParams == null)
                throw new ArgumentNullException("epochParams");
            if (trainScheme == null)
                throw new ArgumentNullException("trainScheme");

            this.EpochNumber = epochNumber;
            this.Params = epochParams;
            this.TrainScheme = trainScheme;
        }


        /// <summary>
        /// Номер эпохи.
        /// </summary>
        public int EpochNumber { get; private set; }

        /// <summary>
        /// Параметры прохождения эпохи.
        /// </summary>
        public SkyTrainEpochParams Params { get; private set; }

        /// <summary>
        /// Схема тренировки нейросети, к которой относится эпоха.
        /// </summary>
        public SkyTrainScheme TrainScheme { get; private set; }

        /// <summary>
        /// Текстовое представление экземпляра класса.
        /// </summary>
        public override string ToString()
        {
            return this.EpochNumber.ToString();
        }


        ISkyTrainEpochParams ISkyTrainEpoch.Params => this.Params;

        ISkyTrainScheme ISkyTrainEpoch.TrainScheme => this.TrainScheme;
    }
}
