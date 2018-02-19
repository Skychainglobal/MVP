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
    /// Представляет сохраняемые данные набора данных обучения нейросети.
    /// </summary>
    public class SkyDataSetEntity : SkyEntity
    {
        internal SkyDataSetEntity()
        {
        }

        /// <summary>
        /// Название набора данных.
        /// </summary>
        [StringLength(250)]
        public string Name { get; internal set; }

        /// <summary>
        /// Идентификатор профиля.
        /// </summary>
        public int ProfileID { get; internal set; }

        /// <summary>
        /// Стоимость использования набора данных.
        /// </summary>
        public decimal Cost { get; internal set; }

        /// <summary>
        /// Рейтинг данных.
        /// </summary>
        public double Rating { get; internal set; }

        /// <summary>
        /// Описание набора данных.
        /// </summary>
        public string Description { get; internal set; }

        /// <summary>
        /// Описание данных, хранящихся в наборе.
        /// </summary>
        public string DataDescription { get; internal set; }

        /// <summary>
        /// Идентификатор тренировочного набора данных.
        /// </summary>
        public Guid TrainSetID { get; internal set; }

        /// <summary>
        /// Идентификатор набора данных для тестирования нейросети.
        /// </summary>
        public Guid TestSetID { get; internal set; }

        /// <summary>
        /// Идентификатор образца набора данных, доступного для скачивания.
        /// </summary>
        public Guid SampleSetID { get; internal set; }

        /// <summary>
        /// Возвращает true, если набор данных опубликован.
        /// </summary>
        public bool Published { get; internal set; }
    }
}
