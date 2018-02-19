using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skychain.Models.Entity
{
    /// <summary>
    /// Сохраняемые данные состояния нейросети, сформированного в результате тренировки.
    /// </summary>
    public class SkyNetworkStateEntity : SkyEntity
    {
        internal SkyNetworkStateEntity()
        {
        }

        /// <summary>
        /// Идентификатор нейросети, которой соответствует состояние.
        /// </summary>
        public int NetworkID { get; internal set; }

        /// <summary>
        /// Идентификатор версии нейросети, которой соответствует состояние.
        /// </summary>
        public int NetworkVersionID { get; internal set; }

        /// <summary>
        /// Идентификатор состояния в рамках версии нейросети.
        /// </summary>
        public int InternalID { get; internal set; }

        /// <summary>
        /// Идентификатор схемы тренировки, по которой было сформировано состояние нейросети.
        /// </summary>
        public int TrainSchemeID { get; internal set; }

        /// <summary>
        /// Идентификатор состояния нейросети, на основе которого было инициализировано данное состояние.
        /// </summary>
        public int InitialStateID { get; internal set; }

        /// <summary>
        /// Описание состояния нейросети.
        /// </summary>
        public string Description { get; internal set; }

        /// <summary>
        /// Идентификатор данных состояния нейросети, через которое будут выполняться запросы к нейросети.
        /// </summary>
        public Guid StateDataID { get; internal set; }

        /// <summary>
        /// Параметры состояния нейросети.
        /// Может содержать ошибку прохожжения тренировочной схему и других параметры.
        /// </summary>
        public string StateParameters { get; internal set; }
    }
}
