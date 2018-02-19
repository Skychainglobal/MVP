using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Storage.Lib;

namespace Storage.Engine
{
    /// <summary>
    /// Фабрика, создающая типы на основе настройки конфигурационного файла.
    /// Экземплар класса является потокобезопасным Singleton объектом.
    /// </summary>
    public sealed class ConfigFactory : IFactory
    {
        #region Singleton
        public static object _locker = new object();

        private ConfigFactory() { }

        private static readonly ConfigFactory _instance = new ConfigFactory();

        public static ConfigFactory Instance
        {
            get { return _instance; }
        }
        #endregion

        private bool __init_Types;
        private Dictionary<string, ConfigTypeMapping> _Types;
        /// <summary>
        /// Зарегистрированные типы фабрики.
        /// </summary>
        private Dictionary<string, ConfigTypeMapping> Types
        {
            get
            {
                if (!__init_Types)
                {
                    lock (_locker)
                    {
                        if (!__init_Types)
                        {
                            _Types = new Dictionary<string, ConfigTypeMapping>();

                            XmlNode typesNode = (XmlNode)ConfigurationManager.GetSection("ConfigFactoryTypes");
                            if (typesNode == null)
                                throw new Exception(string.Format("Не удалось найти секцию конфигурации с именем ConfigFactoryTypes"));

                            XmlNodeList nodes = typesNode.SelectNodes("Type");
                            foreach (XmlNode node in nodes)
                            {
                                ConfigTypeMapping configType = new ConfigTypeMapping(node);
                                if (_Types.ContainsKey(configType.Interface))
                                    throw new Exception(string.Format("Тип с именем {0} уже зарегистрирован",
                                        configType.Interface));

                                _Types.Add(configType.Interface, configType);
                            }

                            __init_Types = true;
                        }
                    }
                }
                return _Types;
            }
        }

        /// <summary>
        /// Создает новый экземпляр объекта типа Т.
        /// </summary>
        /// <typeparam name="T">Тип объекта.</typeparam>
        /// <returns></returns>
        public T Create<T>()
        {
            T instance = this.CreateInstance<T>();
            return instance;
        }

        /// <summary>
        /// Создает новый экземпляр объекта типа Т с параметром.
        /// </summary>
        /// <typeparam name="T">Тип объекта.</typeparam>
        /// <param name="args">Параметры конструктора объекта.</param>
        /// <returns></returns>
        public T Create<T>(params object[] args)
        {
            T instance = this.CreateInstance<T>(args);
            return instance;
        }

        /// <summary>
        /// Создает экземпляр объекта.
        /// </summary>
        /// <typeparam name="T">Тип.</typeparam>
        /// <param name="args">Параметры конструктора объекта.</param>
        /// <returns></returns>
        private T CreateInstance<T>(params object[] args)
        {
            try
            {
                ConfigTypeMapping implementationTypeDefinition = this.EnsureTypeDefinition<T>();
                if (args == null || args.Length == 0)
                {
                    if (implementationTypeDefinition.SettingsNode != null)
                    {
                        object ctorParam = implementationTypeDefinition.SettingsNode;
                        args = new object[1];
                        args[0] = ctorParam;
                    }
                }

                T instance;
                if (args == null || args.Length == 0)
                    instance = (T)Activator.CreateInstance(implementationTypeDefinition.CLRType);
                else
                    instance = (T)Activator.CreateInstance(implementationTypeDefinition.CLRType, args);

                return instance;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    String.Format("Не удалось создать имплементацию интерфейса '{0}'", typeof(T).FullName),
                    ex);
            }
        }

        /// <summary>
        /// Получает сопоставление типа Т.
        /// </summary>
        /// <typeparam name="T">Тип.</typeparam>
        /// <returns></returns>
        private ConfigTypeMapping EnsureTypeDefinition<T>()
        {
            Type interfaceType = typeof(T);
            if (!this.Types.ContainsKey(interfaceType.FullName))
                throw new Exception(string.Format("Тип с именем {0} не зарегистрирован в конфигурационном файла",
                    interfaceType.FullName));

            ConfigTypeMapping implementationTypeDefinition = this.Types[interfaceType.FullName];
            return implementationTypeDefinition;
        }
    }
}