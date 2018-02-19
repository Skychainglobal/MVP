using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skychain.Models
{
    /// <summary>
    /// Представляет профиль пользователя.
    /// </summary>
    public interface ISkyProfile : ISkyObject
    {
        /// <summary>
        /// Пользователь, к которому относится профиль.
        /// </summary>
        ISkyUser User { get; }

        /// <summary>
        /// Адрес электронной почты профиля.
        /// </summary>
        string Email { get; set; }

        /// <summary>
        /// Определяет, отображать ли адрес электронной почты для всех пользователей системы.
        /// </summary>
        bool ShowEmailForEveryone { get; set; }

        /// <summary>
        /// Имя профиля.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Общая информация о профиле.
        /// </summary>
        string Bio { get; set; }

        /// <summary>
        /// Страна.
        /// </summary>
        string Country { get; set; }

        /// <summary>
        /// Город.
        /// </summary>
        string City { get; set; }

        /// <summary>
        /// Адрес.
        /// </summary>
        string Address { get; set; }

        /// <summary>
        /// Изображение, соответствующее профилю.
        /// </summary>
        byte[] Picture { get; set; }

        /// <summary>
        /// Тип профиля.
        /// </summary>
        SkyProfileType Type { get; set; }

        /// <summary>
        /// Наборы данных, принадлежащие профилю.
        /// </summary>
        IEnumerable<ISkyDataSet> DataSets { get; }

        /// <summary>
        /// Создаёт новый экземпляр набора данных для использования в нейросети, без сохранения в базу данных.
        /// </summary>
        ISkyDataSet CreateDataSet();

        /// <summary>
        /// Нейросети, принадлежащие профилю.
        /// </summary>
        IEnumerable<ISkyNetwork> Networks { get; }

        /// <summary>
        /// Создаёт новый экземпляр нейросети, без сохранения в базу данных.
        /// </summary>
        ISkyNetwork CreateNetwork();
    }


    /// <summary>
    /// Тип профиля пользователя.
    /// </summary>
    public enum SkyProfileType
    {
        /// <summary>
        /// Физическое лицо.
        /// </summary>
        Individual = 1,

        /// <summary>
        /// Юридическое лицо.
        /// </summary>
        LegalEntity = 2
    }
}
