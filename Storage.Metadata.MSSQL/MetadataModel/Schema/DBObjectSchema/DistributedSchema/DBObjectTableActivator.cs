using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Storage.Engine;

namespace Storage.Metadata.MSSQL
{
    /// <summary>
    /// Представляет активатор таблиц распределенных данных.
    /// </summary>
    public class DBObjectTableActivator
    {
        /// <summary>
        /// К-тор.
        /// </summary>        
        /// <param name="metadataAdapter">Адаптер работы с метаданными системы.</param>
        internal DBObjectTableActivator(MetadataAdapter metadataAdapter)
        {
            if (metadataAdapter == null)
                throw new ArgumentNullException("metadataAdapter");

            this.MetadataAdapter = metadataAdapter;
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
                    _Logger = ConfigFactory.Instance.Create<ILogProvider>(MetadataConsts.Scopes.TableActivator);
                    __init_Logger = true;
                }
                return _Logger;
            }
        }

        private MetadataAdapter _MetadataAdapter;
        /// <summary>
        /// Адаптер работы с метаданными системы.
        /// </summary>
        public MetadataAdapter MetadataAdapter
        {
            get { return _MetadataAdapter; }
            set { _MetadataAdapter = value; }
        }

        private static Dictionary<string, DBObjectDistributedTable> TablesByKey = new Dictionary<string, DBObjectDistributedTable>();
        private static Dictionary<string, object> LockObjects = new Dictionary<string, object>();
        private static object LokersLocker = new object();

        public DBObjectDistributedTable GetDistributedTable(DBObjectTableSchemaAdapter schemaAdapter, string tableName)//DBConnection connection, string tableName)
        {
            if (schemaAdapter == null)
                throw new ArgumentNullException("schemaAdapter");

            if (String.IsNullOrEmpty(tableName))
                throw new ArgumentNullException("tableName");

            DBConnection connection = schemaAdapter.PrimaryDatabaseConnection;
            string tableKey = string.Format("{0}.{1}.{2}", connection.DisplayName.ToLower(), schemaAdapter.ClassDefinition.TypeUniqueKey, tableName);

            this.Logger.WriteFormatMessage("Получение распределенной таблицы. Ключ: '{0}'", tableKey);

            DBObjectDistributedTable table = null;
            if (DBObjectTableActivator.TablesByKey.ContainsKey(tableKey))
            {
                table = DBObjectTableActivator.TablesByKey[tableKey];
            }
            else
            {
                if (!DBObjectTableActivator.LockObjects.ContainsKey(tableKey))
                {
                    lock (DBObjectTableActivator.LokersLocker)
                    {
                        if (!DBObjectTableActivator.LockObjects.ContainsKey(tableKey))
                            DBObjectTableActivator.LockObjects.Add(tableKey, new object());
                    }
                }

                lock (DBObjectTableActivator.LockObjects[tableKey])
                {
                    if (!DBObjectTableActivator.TablesByKey.ContainsKey(tableKey))
                    {
                        this.Logger.WriteFormatMessage("Вызов конструктора объекта таблицы. Ключ: '{0}'", tableKey);
                        table = new DBObjectDistributedTable(schemaAdapter, tableName);
                        if (!table.TablePartition.Table.Exists)
                            throw new DistributedTableNotFoundException(connection.DisplayName, tableName);
                    }
                    else
                        table = DBObjectTableActivator.TablesByKey[tableKey];
                }
            }

            this.Logger.WriteFormatMessage("Получение распределенной таблицы завершено. Ключ:  '{0}'", tableKey);

            return table;
        }

        public DBObjectDistributedTable EnsureDistributedTable(DBObjectTableSchemaAdapter schemaAdapter, string tableName)
        {
            if (schemaAdapter == null)
                throw new ArgumentNullException("schemaAdapter");

            if (String.IsNullOrEmpty(tableName))
                throw new ArgumentNullException("tableName");

            DBConnection connection = schemaAdapter.PrimaryDatabaseConnection;
            string tableKey = string.Format("{0}.{1}.{2}", connection.DisplayName.ToLower(), schemaAdapter.ClassDefinition.TypeUniqueKey, tableName);
            DBObjectDistributedTable table = null;

            this.Logger.WriteFormatMessage("Получение/создание распределенной таблицы. Ключ: '{0}'", tableKey);

            if (!DBObjectTableActivator.TablesByKey.ContainsKey(tableKey))
            {
                if (!DBObjectTableActivator.LockObjects.ContainsKey(tableKey))
                {
                    lock (DBObjectTableActivator.LokersLocker)
                    {
                        if (!DBObjectTableActivator.LockObjects.ContainsKey(tableKey))
                            DBObjectTableActivator.LockObjects.Add(tableKey, new object());
                    }
                }

                lock (DBObjectTableActivator.LockObjects[tableKey])
                {
                    if (!DBObjectTableActivator.TablesByKey.ContainsKey(tableKey))
                    {
                        this.Logger.WriteFormatMessage("Вызов конструктора объекта таблицы. Ключ: '{0}'", tableKey);
                        table = new DBObjectDistributedTable(schemaAdapter, tableName);
                        this.Logger.WriteFormatMessage("Инициализация объекта таблицы. Ключ: '{0}'", tableKey);
                        table.TablePartition.Table.Init();
                        this.Logger.WriteFormatMessage("Инициализация объекта таблицы завершена. Ключ: '{0}'", tableKey);

                        lock (DBObjectTableActivator.LokersLocker)
                        {
                            DBObjectTableActivator.TablesByKey.Add(tableKey, table);
                        }
                    }
                    else
                        table = DBObjectTableActivator.TablesByKey[tableKey];
                }
            }
            else
            {
                table = DBObjectTableActivator.TablesByKey[tableKey];
            }

            if (table == null)
                throw new Exception(string.Format("Не удалось получить таблицу файлов, с названием {0}.", tableName));

            this.Logger.WriteFormatMessage("Получение/создание распределенной таблицы завершено. Ключ: '{0}'", tableKey);

            return table;
        }
    }

    public class DistributedTableNotFoundException : Exception
    {
        public DistributedTableNotFoundException(string connection, string tableName)
            : base(String.Format("Не найдена таблица, подключение [{0}], название '{1}'", connection, tableName)) { }
    }
}
