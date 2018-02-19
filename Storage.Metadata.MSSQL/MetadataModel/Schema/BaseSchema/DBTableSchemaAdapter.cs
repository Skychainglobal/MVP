using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Storage.Engine;

namespace Storage.Metadata.MSSQL
{
    /// <summary>
    /// Обеспечивает работу со схемой набора таблиц однородных данных.
    /// </summary>
    public abstract class DBTableSchemaAdapter
    {
        /// <summary>
        /// Создает экземпляр DBTableSchemaAdapter.
        /// </summary>
        protected DBTableSchemaAdapter()
        {
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
                    _Logger = ConfigFactory.Instance.Create<ILogProvider>(MetadataConsts.Scopes.DBSchema);
                    __init_Logger = true;
                }
                return _Logger;
            }
        }

        #region TableName

        /// <summary>
        /// Инициализирует базовое название таблицы.
        /// </summary>
        /// <returns></returns>
        protected abstract string InitTableName();

        private bool __init_TableName = false;
        private string _TableName;
        /// <summary>
        /// Базовое название таблицы.
        /// </summary>
        public string TableName
        {
            get
            {
                if (!__init_TableName)
                {
                    _TableName = this.InitTableName();
                    if (string.IsNullOrEmpty(_TableName))
                        throw new Exception("Не задано базовое название таблицы в адаптере схемы таблицы.");
                    __init_TableName = true;
                }
                return _TableName;
            }
        }

        /// <summary>
        /// Инициализирует исходное, до переименования, базовое название таблицы.
        /// </summary>
        /// <returns></returns>
        protected abstract string InitOriginalTableName();

        private bool __init_OriginalTableName = false;
        private string _OriginalTableName;
        /// <summary>
        /// Базовое название таблицы до переименования.
        /// Если название таблицы не изменилось, возвращает актуальное название таблицы из свойства TableName.
        /// </summary>
        internal string OriginalTableName
        {
            get
            {
                if (!__init_OriginalTableName)
                {
                    _OriginalTableName = this.InitOriginalTableName();
                    if (string.IsNullOrEmpty(_OriginalTableName))
                        _OriginalTableName = this.TableName;

                    if (string.IsNullOrEmpty(_OriginalTableName))
                        throw new Exception("Не задано базовое название таблицы до переименования в адаптере схемы таблицы.");

                    __init_OriginalTableName = true;
                }
                return _OriginalTableName;
            }
        }

        private bool __init_TableNameChanged = false;
        private bool _TableNameChanged;
        /// <summary>
        /// Возвращает true, если название таблицы было изменено в контексте выполнения кода.
        /// </summary>
        internal bool TableNameChanged
        {
            get
            {
                if (!__init_TableNameChanged)
                {
                    _TableNameChanged = this.TableName.ToLower() != this.OriginalTableName.ToLower();
                    __init_TableNameChanged = true;
                }
                return _TableNameChanged;
            }
        }

        /// <summary>
        /// Инициализирует базовое название таблицы в разделе.
        /// </summary>
        /// <returns></returns>
        protected abstract string InitPartitionTableName();


        private bool __init_PartitionTableName = false;
        private string _PartitionTableName;
        /// <summary>
        /// Базовое название таблицы в разделе.
        /// </summary>
        public string PartitionTableName
        {
            get
            {
                if (!__init_PartitionTableName)
                {
                    _PartitionTableName = this.InitPartitionTableName();
                    if (string.IsNullOrEmpty(_PartitionTableName))
                        throw new Exception("Не задано базовое название таблицы в разделе в адаптере схемы таблицы.");
                    __init_PartitionTableName = true;
                }
                return _PartitionTableName;
            }
        }

        /// <summary>
        /// Инициализирует исходное, до переименования, базовое название таблицы в разделе.
        /// </summary>
        /// <returns></returns>
        protected abstract string InitOriginalPartitionTableName();

        private bool __init_OriginalPartitionTableName = false;
        private string _OriginalPartitionTableName;
        /// <summary>
        /// Базовое название таблицы до переименования.
        /// Если название таблицы не изменилось, возвращает актуальное название таблицы из свойства PartitionTableName.
        /// </summary>
        internal string OriginalPartitionTableName
        {
            get
            {
                if (!__init_OriginalPartitionTableName)
                {
                    _OriginalPartitionTableName = this.InitOriginalPartitionTableName();
                    if (string.IsNullOrEmpty(_OriginalPartitionTableName))
                        _OriginalPartitionTableName = this.PartitionTableName;

                    if (string.IsNullOrEmpty(_OriginalPartitionTableName))
                        throw new Exception("Не задано базовое название таблицы в разделе до переименования в адаптере схемы таблицы.");

                    __init_OriginalPartitionTableName = true;
                }
                return _OriginalPartitionTableName;
            }
        }

        private bool __init_PartitionTableNameChanged = false;
        private bool _PartitionTableNameChanged;
        /// <summary>
        /// Возвращает true, если название таблицы в разделе было изменено в контексте выполнения кода.
        /// </summary>
        internal bool PartitionTableNameChanged
        {
            get
            {
                if (!__init_PartitionTableNameChanged)
                {
                    _PartitionTableNameChanged = this.PartitionTableName.ToLower() != this.OriginalPartitionTableName.ToLower();
                    __init_PartitionTableNameChanged = true;
                }
                return _PartitionTableNameChanged;
            }
        }

        /// <summary>
        /// Сбрасывает флаги инициализации названия таблицы и зависимых от него свойств.
        /// </summary>
        public void ResetTableName()
        {
            //сбрасываем базовое имя таблицы
            this.__init_TableName = false;
            this.__init_OriginalTableName = false;
            this.__init_TableNameChanged = false;
            this.__init_PartitionTableName = false;
            this.__init_OriginalPartitionTableName = false;
            this.__init_PartitionTableNameChanged = false;

            //сбрасываем префикс
            this.ResetTablePrefixInternal();

            //сбрасываем имя таблицы в разделах и в экземплярах таблиц.
            if (this.TableNameChanged)
            {
                foreach (DBTablePartition partitionInstance in this.PartitionInstances)
                    partitionInstance.ResetTableName();
            }
            else if (this.TablePrefixChanged)
            {
                foreach (DBTablePartition partitionInstance in this.PartitionInstances)
                    partitionInstance.ResetTablePrefix();
            }
        }

        #endregion


        #region TablePrefix

        /// <summary>
        /// Инициализирует префикс таблицы.
        /// </summary>
        /// <returns></returns>
        protected abstract string InitTablePrefix();

        private bool __init_TablePrefix = false;
        private string _TablePrefix;
        /// <summary>
        /// Логическое название таблицы, используемое в качестве префикса названий индексов и триггеров таблицы.
        /// </summary>
        public string TablePrefix
        {
            get
            {
                if (!__init_TablePrefix)
                {
                    _TablePrefix = this.InitTablePrefix();
                    if (string.IsNullOrEmpty(_TablePrefix))
                        throw new Exception("Не задан префикс таблицы в адаптере схемы таблицы.");
                    __init_TablePrefix = true;
                }
                return _TablePrefix;
            }
        }

        /// <summary>
        /// Инициализирует исходный, до переименования, префикс таблицы.
        /// </summary>
        /// <returns></returns>
        protected abstract string InitOriginalTablePrefix();

        private bool __init_OriginalTablePrefix = false;
        private string _OriginalTablePrefix;
        /// <summary>
        /// Префикс таблицы до переименования.
        /// Если префикс не изменился, возвращает актуальный префикс из свойства TablePrefix.
        /// </summary>
        internal string OriginalTablePrefix
        {
            get
            {
                if (!__init_OriginalTablePrefix)
                {
                    _OriginalTablePrefix = this.InitOriginalTablePrefix();
                    if (string.IsNullOrEmpty(_OriginalTablePrefix))
                        _OriginalTablePrefix = this.TablePrefix;

                    if (string.IsNullOrEmpty(_OriginalTablePrefix))
                        throw new Exception("Не задан префикс таблицы до переименования в адаптере схемы таблицы.");

                    __init_OriginalTablePrefix = true;
                }
                return _OriginalTablePrefix;
            }
        }

        private bool __init_TablePrefixChanged = false;
        private bool _TablePrefixChanged;
        /// <summary>
        /// Возвращает true, если префикс таблицы был изменен в контексте выполнения кода.
        /// </summary>
        internal bool TablePrefixChanged
        {
            get
            {
                if (!__init_TablePrefixChanged)
                {
                    _TablePrefixChanged = this.TablePrefix.ToLower() != this.OriginalTablePrefix.ToLower();
                    __init_TablePrefixChanged = true;
                }
                return _TablePrefixChanged;
            }
        }

        /// <summary>
        /// Сбрасывает флаги инициализации префикса таблицы и зависимых от него свойств.
        /// </summary>
        public void ResetTablePrefix()
        {
            //сбрасываем префикс таблицы
            this.ResetTablePrefixInternal();

            //сбрасываем префикс таблицы в разделах и в экземплярах таблиц.
            if (this.TablePrefixChanged)
            {
                foreach (DBTablePartition partitionInstance in this.PartitionInstances)
                    partitionInstance.ResetTablePrefix();
            }
        }

        private void ResetTablePrefixInternal()
        {
            this.__init_TablePrefix = false;
            this.__init_OriginalTablePrefix = false;
            this.__init_TablePrefixChanged = false;
        }

        #endregion


        #region ConnectionContext

        public abstract DBConnectionContext ConnectionContext { get; }

        #endregion


        #region PrimaryConnection

        /// <summary>
        /// Инициализирует подключение к базе основных данных.
        /// </summary>
        /// <returns></returns>
        protected abstract DBConnection InitPrimaryDatabaseConnection();

        private bool __init_PrimaryDatabaseConnection = false;
        private DBConnection _PrimaryDatabaseConnection;
        /// <summary>
        /// Подключение к базе основных данных.
        /// </summary>
        public DBConnection PrimaryDatabaseConnection
        {
            get
            {
                if (!__init_PrimaryDatabaseConnection)
                {
                    _PrimaryDatabaseConnection = this.InitPrimaryDatabaseConnection();
                    if (_PrimaryDatabaseConnection == null)
                        throw new Exception("Не задано подлключение к базе основных данных.");
                    __init_PrimaryDatabaseConnection = true;
                }
                return _PrimaryDatabaseConnection;
            }
        }

        #endregion


        #region PartitionInstances

        /// <summary>
        /// Добавляет экземпляр раздела таблиц в коллекцию экземпляров разделов, созданных в данном адаптере.
        /// </summary>
        /// <param name="partitionInstance">Экземпляр таблицы.</param>
        internal void AddPartitionInstance(DBTablePartition partitionInstance)
        {
            if (partitionInstance == null)
                throw new ArgumentNullException("instance");
            this.PartitionInstances.Add(partitionInstance);
        }

        private bool __init_Instances = false;
        private DBCollection<DBTablePartition> _PartitionInstances;
        /// <summary>
        /// Коллекция экземпляров разделов, созданных на основе данного адаптера схем таблиц.
        /// Заполняется автоматически при создании экземпляров разделов.
        /// Используется для переинициализации параметров разделов.
        /// </summary>
        private DBCollection<DBTablePartition> PartitionInstances
        {
            get
            {
                if (!__init_Instances)
                {
                    _PartitionInstances = new DBCollection<DBTablePartition>();
                    __init_Instances = true;
                }
                return _PartitionInstances;
            }
        }

        #endregion


        #region TableSchema

        /// <summary>
        /// Инициализирует схему таблицы основных данных.
        /// </summary>
        /// <returns></returns>
        protected abstract DBPrincipalTableSchema InitTableSchema();

        private bool __init_TableSchema = false;
        private DBPrincipalTableSchema _TableSchema;
        /// <summary>
        /// Схема таблицы основных данных.
        /// </summary>
        public DBPrincipalTableSchema TableSchema
        {
            get
            {
                if (!__init_TableSchema)
                {                    
                    _TableSchema = this.InitTableSchema();

                    if (_TableSchema == null)
                        throw new Exception(string.Format("Не удалось получить экземпляр схемы таблицы для адаптера схемы таблицы {0}.", this.TableName));

                    __init_TableSchema = true;
                }
                return _TableSchema;
            }
        }

        #endregion


        #region TriggerDisableQueries

        /// <summary>
        /// Возвращает текст запроса установки контекста отключения выполнения триггеров в рамках выполнения SQL-запроса.
        /// </summary>
        /// <returns></returns>
        public string GetTriggersDisableQuery()
        {
            return this.GetTriggersDisableQuery(false);
        }

        /// <summary>
        /// Возвращает текст запроса установки контекста отключения выполнения триггеров триггеров в рамках выполнения SQL-запроса.
        /// </summary>
        /// <param name="allTriggers">При установленном true, формируется условие установки контекста дизэйбла всех триггеров.</param>
        /// <returns></returns>
        internal string GetTriggersDisableQuery(bool allTriggers)
        {
            string contextName = !allTriggers ?
                MetadataConsts.TriggerContexts.DisableTriggers.Name :
                MetadataConsts.TriggerContexts.DisableAllTriggers.Name;

            string contextBinaryCode = !allTriggers ?
                MetadataConsts.TriggerContexts.DisableTriggers.BinaryCode :
                MetadataConsts.TriggerContexts.DisableAllTriggers.BinaryCode;

            string query = @"
--устанавливаем контекст отключения триггеров {ContextName}: значение, обрабатываемое триггерами, по которому триггеры не срабатывают.
SET CONTEXT_INFO {ContextBinaryCode}"
                .ReplaceKey("ContextName", contextName)
                .ReplaceKey("ContextBinaryCode", contextBinaryCode)
                ;
            return query;
        }

        /// <summary>
        /// Возвращает текст запроса установки контекста включения выполнения триггеров в рамках выполнения SQL-запроса.
        /// </summary>
        public string GetTriggersEnableQuery()
        {
            string query = @"
--устанавливаем контекст включения триггеров.
SET CONTEXT_INFO 0x";
            return query;
        }

        /// <summary>
        /// Возвращает текст запроса проверки отключения триггеров в контексте выполнения SQL-запроса.
        /// </summary>
        /// <returns></returns>
        public string GetTriggersCheckDisabledQuery()
        {
            return this.GetTriggersCheckDisabledQuery(false, null);
        }

        /// <summary>
        /// Возвращает текст запроса проверки отключения триггеров в контексте выполнения SQL-запроса.
        /// </summary>
        /// <param name="disableActionsQuery">Запрос, выполняемый при выполнении условия отключения триггеров.</param>
        /// <returns></returns>
        public string GetTriggersCheckDisabledQuery(string disableActionsQuery)
        {
            return this.GetTriggersCheckDisabledQuery(false, disableActionsQuery);
        }

        /// <summary>
        /// Возвращает текст запроса проверки отключения триггеров в контексте выполнения SQL-запроса.
        /// </summary>
        /// <param name="allTriggers">При установленном true, формируется условие проверки контекста дизэйбла всех триггеров, включая системные.</param>
        /// <param name="disableActionsQuery">Запрос, выполняемый при выполнении условия отключения триггеров.</param>
        /// <returns></returns>
        internal string GetTriggersCheckDisabledQuery(bool allTriggers, string disableActionsQuery)
        {
            string query = null;
            //действия, выполняемые при выполнении условия отключения триггеров.
            string disableActions = string.IsNullOrEmpty(disableActionsQuery) ?
                @"
    RETURN" :
                string.Format(@"
BEGIN
    {0}
    RETURN
END", disableActionsQuery);

            if (!allTriggers)
            {
                query = @"
--если в контекст SQL-сессии передан параметр CONTEXT_INFO, соответствующий контексту отключения триггеров {DisableContext} или {DisableAllContext}, то прерываем выполнение триггера.
DECLARE @triggerDisableContext binary(8)
SET @triggerDisableContext = CONTEXT_INFO()
IF(@triggerDisableContext = {DisableContextCode} OR @triggerDisableContext = {DisableAllContextCode}){DisableActions}"
                    .ReplaceKey("DisableContext", MetadataConsts.TriggerContexts.DisableTriggers.Name)
                    .ReplaceKey("DisableAllContext", MetadataConsts.TriggerContexts.DisableAllTriggers.Name)
                    .ReplaceKey("DisableContextCode", MetadataConsts.TriggerContexts.DisableTriggers.BinaryCode)
                    .ReplaceKey("DisableAllContextCode", MetadataConsts.TriggerContexts.DisableAllTriggers.BinaryCode)
                    .ReplaceKey("DisableActions", disableActions);
                ;
            }
            else
            {
                query = @"
--если в контекст SQL-сессии передан параметр CONTEXT_INFO, соответствующий контексту отключения триггеров {DisableAllContext}, то прерываем выполнение триггера.
DECLARE @triggerDisableContext binary(8)
SET @triggerDisableContext = CONTEXT_INFO()
IF(@triggerDisableContext = {DisableAllContextCode}){DisableActions}"
                    .ReplaceKey("DisableAllContext", MetadataConsts.TriggerContexts.DisableAllTriggers.Name)
                    .ReplaceKey("DisableAllContextCode", MetadataConsts.TriggerContexts.DisableAllTriggers.BinaryCode)
                    .ReplaceKey("DisableActions", disableActions);
                ;
            }
            return query;
        }

        #endregion


        #region PermanentSchema

        /// <summary>
        /// Инициализирует значение свойства IsPermanentSchema.
        /// </summary>
        /// <returns></returns>
        protected virtual bool InitIsPermanentSchema()
        {
            return false;
        }

        private bool __init_IsPermanentSchema = false;
        private bool _IsPermanentSchema;
        /// <summary>
        /// Возвращает true, если схема таблицы является статической и неизменной.
        /// Таблицы с соответствующим набором столбцов для неизменной схемы уже должны существовать в базе данных.
        /// Адаптеру неизменной схемы таблицы доступно только изменение индексов и триггеров таблицы, а также доступно чтение схемы существующей таблицы.
        /// </summary>
        public bool IsPermanentSchema
        {
            get
            {
                if (!__init_IsPermanentSchema)
                {
                    _IsPermanentSchema = this.InitIsPermanentSchema();
                    __init_IsPermanentSchema = true;
                }
                return _IsPermanentSchema;
            }
        }

        internal void CheckManagedSchema()
        {
            if (this.IsPermanentSchema)
                throw new Exception(string.Format("Операция недоступна для таблицы с неизменямой схемой {0}.", this.TableName));
        }

        /// <summary>
        /// Строковое представление экземпляра адаптера схемы таблицы DBTableSchemaAdapter.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(this.TableName))
                return this.TableName;
            return base.ToString();
        }

        #endregion
    }
}
