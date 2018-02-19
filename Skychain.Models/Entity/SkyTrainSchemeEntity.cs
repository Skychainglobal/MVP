using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skychain.Models.Entity
{
    /// <summary>
    /// Сохраняемые данные схемы тренировки нейросети.
    /// </summary>
    public class SkyTrainSchemeEntity : SkyEntity
    {
        internal SkyTrainSchemeEntity()
        {
        }

        /// <summary>
        /// Идентификатор нейросети.
        /// </summary>
        public int NetworkID { get; internal set; }

        /// <summary>
        /// Название схемы тренировки нейросети.
        /// </summary>
        [StringLength(250)]
        public string Name { get; internal set; }
    }
}
