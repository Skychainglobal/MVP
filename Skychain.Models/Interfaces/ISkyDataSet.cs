using Storage.Lib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skychain.Models
{
    /// <summary>
    /// Представляет набор данных для обучения и тренировки сети.
    /// </summary>
    public interface ISkyDataSet : ISkyObject
    {
        /// <summary>
        /// Название набора данных.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Профиль, которому принадлежит набор данных.
        /// </summary>
        ISkyProfile Profile { get; }

        /// <summary>
        /// Стоимость использования набора данных.
        /// </summary>
        decimal Cost { get; set; }

        /// <summary>
        /// Рейтинг данных.
        /// </summary>
        double Rating { get; set; }

        /// <summary>
        /// Описание набора данных.
        /// </summary>
        string Description { get; set; }

        /// <summary>
        /// Описание данных, хранящихся в наборе.
        /// </summary>
        string DataDescription { get; set; }

        /// <summary>
        /// Тренировочный набор данных.
        /// Свойство всегда возвращает существующий экземпляр класса.
        /// </summary>
        ISkyFile TrainSet { get; }

        /// <summary>
        /// Набор данных для тестирования нейросети.
        /// Свойство всегда возвращает существующий экземпляр класса.
        /// </summary>
        ISkyFile TestSet { get; }

        /// <summary>
        /// Образец набор данных. 
        /// Свойство всегда возвращает существующий экземпляр класса.
        /// </summary>
        ISkyFile SampleSet { get; }
        
        /// <summary>
        /// Возвращает true, если набор данных опубликован.
        /// </summary>
        bool Published { get; set; }

        /// <summary>
        /// Проверяет, является ли набор данных опубликованным, в ином случае генерирует исключение.
        /// </summary>
        void CheckPublished();
    }
}
