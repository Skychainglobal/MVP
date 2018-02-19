using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Storage.Engine
{
    /// <summary>
    /// Секция конфигурационного файла.
    /// </summary>
    public class ConfigSection : System.Configuration.IConfigurationSectionHandler
    {
        /// <summary>
        /// Создает секцию конфигурационного файла.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="configContext">Контекс кофигурации.</param>
        /// <param name="section">Узел секции.</param>
        /// <returns></returns>
        public object Create(object parent, object configContext, XmlNode section)
        {
            //только возвращаем сам узел секции (дальше работаем только с XmlNode)
            return section;
        }
    }
}