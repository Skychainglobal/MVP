using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skychain.Models.Entity
{
    /// <summary>
    /// Сохраняемые данные профиля.
    /// </summary>
    public class SkyProfileEntity : SkyEntity
    {
        internal SkyProfileEntity()
        {
        }

        /// <summary>
        /// Идентификатор пользователя, к которому относится профиль.
        /// </summary>
        public Guid UserID { get; internal set; }

        /// <summary>
        /// Адрес электронной почты профиля.
        /// </summary>
        [StringLength(250)]
        public string Email { get; internal set; }

        /// <summary>
        /// Определяет, отображать ли адрес электронной почты для всех пользователей системы.
        /// </summary>
        public bool ShowEmailForEveryone { get; internal set; }

        /// <summary>
        /// Имя профиля.
        /// </summary>
        [StringLength(250)]
        public string Name { get; internal set; }

        /// <summary>
        /// Общая информация о профиле.
        /// </summary>
        public string Bio { get; internal set; }

        /// <summary>
        /// Страна.
        /// </summary>
        [StringLength(250)]
        public string Country { get; internal set; }

        /// <summary>
        /// Город.
        /// </summary>
        [StringLength(250)]
        public string City { get; internal set; }

        /// <summary>
        /// Адрес.
        /// </summary>
        [StringLength(400)]
        public string Address { get; internal set; }

        /// <summary>
        /// Изображение, соответствующее профилю.
        /// </summary>
        public byte[] Picture { get; set; }

        /// <summary>
        /// Тип профиля.
        /// </summary>
        public SkyProfileType Type { get; internal set; } = SkyProfileType.Individual;
    }
}
