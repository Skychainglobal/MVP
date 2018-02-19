using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skychain.Models.Entity
{
    /// <summary>
    /// Представляет сохраняемые данные объекта системы.
    /// </summary>
    public abstract class SkyEntity
    {
        internal SkyEntity()
        {
        }

        /// <summary>
        /// Уникальный идентификатор объекта в базе данных.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int ID { get; internal set; }

        /// <summary>
        /// Номер версии объекта, увеличиваемый при любом обновлении объекта.
        /// </summary>
        public int VersionNumber { get; internal set; }

        /// <summary>
        /// Дата и время сохранения нового набора данных.
        /// </summary>
        public DateTime? TimeCreated { get; internal set; }

        /// <summary>
        /// Дата и время последнего изменения свойств набора данных.
        /// </summary>
        public DateTime? TimeModified { get; internal set; }
    }
}
