using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skychain.Models.Entity
{
    /// <summary>
    /// Сохраняемые данные запроса запроса тренировки нейросети.
    /// </summary>
    public class SkyTrainRequestEntity : SkyEntity
    {
        internal SkyTrainRequestEntity()
        {
        }

        /// <summary>
        /// Идентификатор нейросети, для которой выполняется тренировка.
        /// </summary>
        public int NetworkID { get; internal set; }

        /// <summary>
        /// Идентификатор тренируемой версии нейросети.
        /// </summary>
        public int NetworkVersionID { get; internal set; }

        /// <summary>
        /// Схема тренировки нейросети.
        /// </summary>
        public int TrainSchemeID { get; internal set; }

        /// <summary>
        /// Идентификатор состояния нейросети, на основе которого выполняется тренировка.
        /// </summary>
        public int InitialStateID { get; internal set; }

        /// <summary>
        /// Идентификатор пользователя, выполняющего тренировку нейросети.
        /// </summary>
        public Guid UserID { get; internal set; }

        /// <summary>
        /// Статус запроса тренировки.
        /// </summary>
        public SkyTrainRequestStatus Status { get; internal set; } = SkyTrainRequestStatus.Creating;

        /// <summary>
        /// Состояние нейросети, сформированное в результате тренировки.
        /// </summary>
        public int ResultStateID { get; internal set; }

		/// <summary>
		/// Описание результирующего состояния.
		/// </summary>
		public string ResultStateDescription { get; internal set; }

        /// <summary>
        /// Время начала запроса.
        /// </summary>
        public DateTime? StartTime { get; internal set; }

        /// <summary>
        /// Время начала окончания.
        /// </summary>
        public DateTime? EndTime { get; internal set; }

        /// <summary>
        /// Лог, записываемый во время выполнения тренировки.
        /// </summary>
        public string TrainLog { get; internal set; }

        /// <summary>
        /// Сообщение об ошибке, возникшей при выполнении запроса.
        /// </summary>
        public string ErrorMessage { get; internal set; }

        /// <summary>
        /// Возвращает true, если обработка запроса была выполнена без ошибок.
        /// </summary>
        public bool Succeed { get; internal set; }
    }
}
