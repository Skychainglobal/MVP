using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Skychain.Models
{
    /// <summary>
    /// Представляет контекст работы системы Skychain в рамках http-запроса.
    /// </summary>
    public interface ISkyWebContext
    {
        /// <summary>
        /// Контекст выполнения http-запроса.
        /// </summary>
        HttpContext HttpContext { get; }

        /// <summary>
        /// Возвращает текущий контекст системы.
        /// </summary>
        ISkyContext SkyContext { get; }

        /// <summary>
        /// Текущий пользователь, для которого выполняется http-запрос.
        /// Генерирует исключение при отсутствии текущего пользователя.
        /// </summary>
        ISkyUser CurrentUser { get; }

        /// <summary>
        /// Возвращает true, при наличии текущего пользователя, для которого выполняется http-запрос.
        /// </summary>
        bool HasCurrentUser { get; }

        /// <summary>
        /// Кодирует текст для представления всех его символов в отображаемом виде в html.
        /// </summary>
        /// <param name="text">Кодируемый текст.</param>
        string EncodeHtml(string text);

        /// <summary>
        /// Кодирует текст формата Xml для представления всех его символов в отображаемом виде в html.
        /// </summary>
        /// <param name="xmlData">Кодируемый текст в формате Xml.</param>
        string EncodeXml(string xmlData);
    }
}