using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storage.Lib
{
    /// <summary>
    /// Интерфейс объекта фабрики.
    /// </summary>
    public interface IFactory
    {
        /// <summary>
        /// Создает новый экземпляр объекта типа Т.
        /// </summary>
        /// <typeparam name="T">Тип объекта.</typeparam>
        /// <returns></returns>
        T Create<T>();

        /// <summary>
        /// Создает новый экземпляр объекта типа Т с параметром.
        /// </summary>
        /// <typeparam name="T">Тип объекта.</typeparam>
        /// <param name="args">Параметры конструктора объекта.</param>
        /// <returns></returns>
        T Create<T>(params object[] args);
    }
}