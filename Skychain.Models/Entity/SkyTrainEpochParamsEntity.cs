using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skychain.Models.Entity
{
    /// <summary>
    /// Сохраняемые данные параметров прохождения тренировочного сета для набора эпох.
    /// </summary>
    public class SkyTrainEpochParamsEntity : SkyEntity
    {
        internal SkyTrainEpochParamsEntity()
        {
        }

        /// <summary>
        /// Идентификатор схемы тренировок.
        /// </summary>
        public int TrainSchemeID { get; internal set; }

        /// <summary>
        /// Начальная эпоха, которой соответствуют параметры прохождения тренировочного сета.
        /// </summary>
        public int StartEpochNumber { get; internal set; }

        /// <summary>
        /// Конечная эпоха, которой соответствуют параметры прохождения тренировочного сета.
        /// </summary>
        public int EndEpochNumber { get; internal set; }

        /// <summary>
        /// Набор данных для тренировки.
        /// </summary>
        public int DataSetID { get; internal set; }
        
        /// <summary>
        /// Произвольные параметры тренировки.
        /// </summary>
        public string CustomParameters { get; internal set; }
    }
}
