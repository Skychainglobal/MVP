using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Storage.Engine
{
    /// <summary>
    /// Сопоставление типа с его реализацией.
    /// Тип и реализации могут находится в разных сборках.
    /// </summary>
    internal class ConfigTypeMapping
    {
        public ConfigTypeMapping(XmlNode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            this.Node = node;
        }

        /// <summary>
        /// Xml узел с описанием типа.
        /// </summary>
        private XmlNode Node { get; set; }

        private bool __init_SettingsNode;
        private XmlNode _SettingsNode;
        /// <summary>
        /// Xml узел настроек.
        /// </summary>
        internal XmlNode SettingsNode
        {
            get
            {
                if (!__init_SettingsNode)
                {
                    _SettingsNode = this.Node.SelectSingleNode("Settings");
                    __init_SettingsNode = true;
                }
                return _SettingsNode;
            }
        }

        private bool __init_Interface;
        private string _Interface;
        /// <summary>
        /// Интерфейс, который реализует тип.
        /// </summary>
        internal string Interface
        {
            get
            {
                if (!__init_Interface)
                {
                    _Interface = this.GetAttributeValue("Interface");
                    __init_Interface = true;
                }
                return _Interface;
            }
        }

        private bool __init_ImplementationAssembly;
        private string _ImplementationAssembly;
        /// <summary>
        /// Сборка, в котором реализован тип.
        /// </summary>
        private string ImplementationAssembly
        {
            get
            {
                if (!__init_ImplementationAssembly)
                {
                    _ImplementationAssembly = this.GetAttributeValue("ImplementationAssembly");
                    __init_ImplementationAssembly = true;
                }
                return _ImplementationAssembly;
            }
        }

        private bool __init_ImplementationClass;
        private string _ImplementationClass;
        /// <summary>
        /// Класс, который реализует тип.
        /// </summary>
        private string ImplementationClass
        {
            get
            {
                if (!__init_ImplementationClass)
                {
                    _ImplementationClass = this.GetAttributeValue("ImplementationClass");
                    __init_ImplementationClass = true;
                }
                return _ImplementationClass;
            }
        }

        private bool __init_CLRType;
        private Type _CLRType;
        /// <summary>
        /// Тип данных.
        /// </summary>
        internal Type CLRType
        {
            get
            {
                if (!__init_CLRType)
                {
                    System.Reflection.Assembly interfaceAssembly = System.Reflection.Assembly.Load(this.ImplementationAssembly);
                    if (interfaceAssembly == null)
                        throw new Exception(string.Format("Не удалось загрузить сборку {0}",
                            this.ImplementationAssembly));

                    _CLRType = interfaceAssembly.GetType(this.ImplementationClass);
                    if (_CLRType == null)
                        throw new Exception(string.Format("Не удалось загрузить тип {0} из сборки {1}",
                            this.ImplementationClass,
                            this.ImplementationAssembly));

                    __init_CLRType = true;
                }
                return _CLRType;
            }
        }


        /// <summary>
        /// Получает значение атрибута.
        /// </summary>
        /// <param name="attributeName">Имя атрибута.</param>
        /// <param name="throwIfEmpty">Выбрасывать исключение, если значение атрибута не задано.</param>
        /// <returns></returns>
        private string GetAttributeValue(string attributeName, bool throwIfEmpty = true)
        {
            if (string.IsNullOrEmpty(attributeName))
                throw new ArgumentNullException("attributeName");

            string value = null;
            XmlAttribute attr = this.Node.Attributes[attributeName];
            if (attr != null)
                value = attr.Value;

            if (throwIfEmpty && string.IsNullOrEmpty(value))
                throw new Exception(string.Format("Не задан атрибут {0}",
                    attributeName));

            return value;
        }
    }
}