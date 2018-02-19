using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skychain.Models.Entity
{
    /// <summary>
    /// Сохраняемые данные версии нейросети.
    /// </summary>
    public class SkyNetworkVersionEntity : SkyEntity
    {
        internal SkyNetworkVersionEntity()
        {
        }

        /// <summary>
        /// Идентификатор нейросети.
        /// </summary>
        public int NetworkID { get; internal set; }

        /// <summary>
        /// Идентификатор версии в рамках нейросети.
        /// </summary>
        public int InternalID { get; internal set; }

        /// <summary>
        /// Инкремент, используемый для формирования идентификатора состояния версии нейросети.
        /// </summary>
        public int StatesInternalIDCounter { get; internal set; }

        /// <summary>
        /// Название версии нейросети.
        /// </summary>
        [StringLength(250)]
        public string Name { get; internal set; }

        /// <summary>
        /// Описание нейросети.
        /// </summary>
        public string Description { get; internal set; }

        /// <summary>
        /// Возвращает true, если версия нейросети является активной.
        /// </summary>
        public bool IsActive { get; internal set; }

        /// <summary>
        /// Манифест введённых пользователем данных и данных результата.
        /// </summary>
        public string ManifestData { get; internal set; }

        /// <summary>
        /// Идентификатор библиотеки нейросети.
        /// </summary>
        public Guid ExecutableLibraryID { get; internal set; }

        /// <summary>
        /// Идентификатор активного состояния нейросети, используемого для выполнения запросов к данной версии нейросети.
        /// </summary>
        public int ActiveNetworkStateID { get; internal set; }
    }
}
