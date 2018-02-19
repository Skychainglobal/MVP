using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skychain.Models.Entity
{
    /// <summary>
    /// Сохраняемые данные запроса пользователя к нейросети.
    /// </summary>
    public class SkyNetworkRequestEntity : SkyEntity
    {
        internal SkyNetworkRequestEntity()
        {
        }

        /// <summary>
        /// Идентификатор нейросети, к которой выполняется запрос.
        /// </summary>
        public int NetworkID { get; internal set; }

        /// <summary>
        /// Идентификатор используемой версии нейросети.
        /// </summary>
        public int NetworkVersionID { get; internal set; }

        /// <summary>
        /// Состояние нейросети, на основе которого формируется результат.
        /// </summary>
        public int NetworkStateID { get; internal set; }

        /// <summary>
        /// Идентификатор пользователя, формирующего запрос к нейросети.
        /// </summary>
        public Guid UserID { get; internal set; }

        /// <summary>
        /// Статус запроса к нейросети.
        /// </summary>
        public SkyNetworkRequestStatus Status { get; internal set; } = SkyNetworkRequestStatus.Creating;

        /// <summary>
        /// Стоимость использования нейросети на момент создания запроса.
        /// </summary>
        public decimal NetworkCost { get; internal set; }

        /// <summary>
        /// Данные, введённые пользователем, в соответствии с форматом манифеста.
        /// </summary>
        public string InputData { get; internal set; }

        /// <summary>
        /// Содержит идентификаторы файлов, загруженных пользователем при формировании запроса.
        /// </summary>
        public string InputFiles { get; internal set; }

        /// <summary>
        /// Данные результата запроса, в соответствии с форматом манифеста.
        /// </summary>
        public string ResultData { get; internal set; }

        /// <summary>
        /// Время начала запроса.
        /// </summary>
        public DateTime? StartTime { get; internal set; }

        /// <summary>
        /// Время начала окончания.
        /// </summary>
        public DateTime? EndTime { get; internal set; }

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
