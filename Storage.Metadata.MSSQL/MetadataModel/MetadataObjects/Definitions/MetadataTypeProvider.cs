using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Storage.Engine;

namespace Storage.Metadata.MSSQL
{
    /// <summary>
    /// Представляет провайдер типов метаданных.
    /// </summary>
    public class MetadataTypeProvider
    {
        private static volatile MetadataTypeProvider instance;
        private static object syncRoot = new Object();

        private MetadataTypeProvider() { }

        public static MetadataTypeProvider Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new MetadataTypeProvider();
                    }
                }

                return instance;
            }
        }

        private bool __init_Logger;
        private ILogProvider _Logger;
        /// <summary>
        /// Логгер.
        /// </summary>
        internal ILogProvider Logger
        {
            get
            {
                if (!__init_Logger)
                {
                    _Logger = ConfigFactory.Instance.Create<ILogProvider>(MetadataConsts.Scopes.TypeProvider);
                    __init_Logger = true;
                }
                return _Logger;
            }
        }

        private static Dictionary<string, MetadataTypeDefinition> TypeDefinitions = new Dictionary<string, MetadataTypeDefinition>();
        private static Dictionary<string, object> Lockers = new Dictionary<string, object>();
        private static object Locker = new object();

        /// <summary>
        /// Получение определения типа метаданных по типу объекта.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public MetadataTypeDefinition GetTypeDefinition(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            this.Logger.WriteFormatMessage("Получение определения типа '{0}'", type.Name);

            MetadataTypeDefinition typeDefinition = null;
            string typeKey = type.AssemblyQualifiedName;
            if (!MetadataTypeProvider.TypeDefinitions.ContainsKey(typeKey))
            {
                if (!MetadataTypeProvider.Lockers.ContainsKey(typeKey))
                {
                    lock (MetadataTypeProvider.Locker)
                    {
                        if (!MetadataTypeProvider.Lockers.ContainsKey(typeKey))
                            MetadataTypeProvider.Lockers.Add(typeKey, new object());
                    }
                }

                lock (MetadataTypeProvider.Lockers[typeKey])
                {
                    if (!MetadataTypeProvider.TypeDefinitions.ContainsKey(typeKey))
                    {
                        this.Logger.WriteFormatMessage("Вызов конструктора определения типа '{0}'", type.Name);

                        typeDefinition = new MetadataTypeDefinition(type);
                        lock (MetadataTypeProvider.Locker)
                        {
                            MetadataTypeProvider.TypeDefinitions.Add(typeKey, typeDefinition);
                        }
                    }
                    else
                    {
                        typeDefinition = MetadataTypeProvider.TypeDefinitions[typeKey];
                    }
                }
            }
            else
            {
                typeDefinition = MetadataTypeProvider.TypeDefinitions[typeKey];
            }

            this.Logger.WriteFormatMessage("Получение определения типа '{0}' завершено.", type.Name);

            if (typeDefinition == null)
                throw new Exception(string.Format("Не удалось получить определение типа метаданных {0}.", type.FullName));

            return typeDefinition;
        }
    }
}
