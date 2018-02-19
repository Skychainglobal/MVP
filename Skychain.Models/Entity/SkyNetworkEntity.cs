using Skychain.Models.Implementation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skychain.Models.Entity
{
    /// <summary>
    /// Сохраняемые данные нейросети.
    /// </summary>
    public class SkyNetworkEntity : SkyEntity
    {
        internal SkyNetworkEntity()
        {
        }

        /// <summary>
        /// Идентификатор профиля.
        /// </summary>
        public int ProfileID { get; internal set; }

        /// <summary>
        /// Инкремент, используемый для формирования идентификатора версии нейросети в рамках нейросети.
        /// </summary>
        public int VersionsInternalIDCounter { get; internal set; }

        /// <summary>
        /// Название нейросети.
        /// </summary>
        [StringLength(250)]
        public string Name { get; internal set; }

        /// <summary>
        /// Описание нейросети.
        /// </summary>
        public string Description { get; internal set; }

        /// <summary>
        /// Стоимость использования нейросети.
        /// </summary>
        public decimal Cost { get; internal set; }

        /// <summary>
        /// Возвращает true, если набор данных опубликован.
        /// </summary>
        public bool Published { get; internal set; }

        /// <summary>
        /// Данные текущего состояния нейросети.
        /// Изменяются после выполнения тренировки сети.
        /// </summary>
        public byte[] StateData { get; internal set; }

        /// <summary>
        /// Рейтинг нейросети.
        /// </summary>
        public double Rating { get; internal set; }

    }
}
