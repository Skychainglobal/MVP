using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Storage.Metadata.MSSQL
{
    /// <summary>
    /// Представляет базовый класс для работы со схемой таблицы.
    /// </summary>
    public abstract class DBTable
    {
        /// <summary>
        /// Создает экземпляр DBTable.
        /// </summary>
        /// <param name="partition">Раздел данных, к которому относится таблица.</param>
        protected DBTable(DBTableSchema schema)
        {
            if (schema == null)
                throw new ArgumentNullException("partition");

            this.Schema = schema;
            this.SchemaAdapter.Logger.WriteMessage("Инициализация объекта DBTable.");
        }

        private DBTableSchema _Schema;
        /// <summary>
        /// Раздел данных таблицы, к которой относится таблица.
        /// </summary>
        public DBTableSchema Schema
        {
            get { return _Schema; }
            private set { _Schema = value; }
        }

        /// <summary>
        /// Адаптер схемы таблицы.
        /// </summary>
        public DBTableSchemaAdapter SchemaAdapter
        {
            get { return this.Schema.SchemaAdapter; }
        }

        /// <summary>
        /// Подключение к базе данных, в которой располагается таблица.
        /// </summary>
        public abstract DBConnection Connection { get; }

        private bool __init_DataAdapter = false;
        private DBAdapter _DataAdapter;
        /// <summary>
        /// Адаптер работы с данными базы данных, в которой располагается таблица.
        /// </summary>
        public DBAdapter DataAdapter
        {
            get
            {
                if (!__init_DataAdapter)
                {
                    //создаем адаптер.
                    _DataAdapter = this.Connection.CreateDataAdapter();

                    //устанавливаем таймаут выполнения команды равным 10 часов.                    
                    _DataAdapter.CommandTimeout = MetadataConsts.CommandTimeout;

                    __init_DataAdapter = true;
                }
                return _DataAdapter;
            }
        }

        /// <summary>
        /// Возвращает имя объекта базы данных относительно исходного имени.
        /// Используется для формирования имен таблиц, индексов в зависимости от роли таблицы.
        /// </summary>
        /// <param name="sourceName">Исходное имя объекта.</param>
        /// <returns></returns>
        internal abstract string GetRelativeName(string sourceName);

        /// <summary>
        /// Возвращает имя объекта базы данных относительно исходного имени для исходной схемы таблиц до ее изменения.
        /// Используется для формирования имен таблиц, индексов в зависимости от роли таблицы.
        /// </summary>
        /// <param name="sourceName">Исходное имя объекта.</param>
        /// <returns></returns>
        internal virtual string GetOriginalRelativeName(string sourceName)
        {
            return this.GetRelativeName(sourceName);
        }


        #region Name

        /// <summary>
        /// Базовое название таблицы, на основе которого формируется название данной таблицы.
        /// </summary>
        internal abstract string BaseName { get; }

        /// <summary>
        /// Базовое название таблицы до переименования, на основе которого формируется название данной таблицы до переименования.
        /// </summary>
        internal abstract string BaseOriginalName { get; }

        /// <summary>
        /// Базоый префикс таблицы, на основе которого формируется префикс данной таблицы.
        /// </summary>
        internal abstract string BasePrefix { get; }

        /// <summary>
        /// Базовый префикс таблицы до переименования, на основе которого формируется префикс данной таблицы до переименования.
        /// </summary>
        internal abstract string BaseOriginalPrefix { get; }

        private bool __init_Name = false;
        private string _Name;
        /// <summary>
        /// Название таблицы.
        /// </summary>
        public string Name
        {
            get
            {
                if (!__init_Name)
                {
                    _Name = this.GetRelativeName(this.BaseName);
                    __init_Name = true;
                }
                return _Name;
            }
        }

        private bool __init_OriginalName = false;
        private string _OriginalName;
        /// <summary>
        /// Исходное название таблицы до переименования.
        /// </summary>
        internal string OriginalName
        {
            get
            {
                if (!__init_OriginalName)
                {
                    _OriginalName = this.GetOriginalRelativeName(this.BaseOriginalName);
                    __init_OriginalName = true;
                }
                return _OriginalName;
            }
        }

        private bool __init_Prefix = false;
        private string _Prefix;
        /// <summary>
        /// Префикс таблицы.
        /// </summary>
        public string Prefix
        {
            get
            {
                if (!__init_Prefix)
                {
                    _Prefix = this.GetRelativeName(this.BasePrefix);
                    __init_Prefix = true;
                }
                return _Prefix;
            }
        }

        private bool __init_OriginalPrefix = false;
        private string _OriginalPrefix;
        /// <summary>
        /// Исходный префикс таблицы до переименования.
        /// </summary>
        internal string OriginalPrefix
        {
            get
            {
                if (!__init_OriginalPrefix)
                {
                    _OriginalPrefix = this.GetOriginalRelativeName(this.BaseOriginalPrefix);
                    __init_OriginalPrefix = true;
                }
                return _OriginalPrefix;
            }
        }

        private bool __init_NameChanged = false;
        private bool _NameChanged;
        /// <summary>
        /// Возвращает true, если таблица была переименована с измененным названием в контексте выполнения кода.
        /// </summary>
        internal bool NameChanged
        {
            get
            {
                if (!__init_NameChanged)
                {
                    _NameChanged = this.OriginalName.ToLower() != this.Name.ToLower();
                    __init_NameChanged = true;
                }
                return _NameChanged;
            }
        }

        private bool __init_PrefixChanged = false;
        private bool _PrefixChanged;
        /// <summary>
        /// Возвращает true, если префикс таблицы был изменен в контексте выполнения кода.
        /// </summary>
        internal bool PrefixChanged
        {
            get
            {
                if (!__init_PrefixChanged)
                {
                    _PrefixChanged = this.OriginalPrefix.ToLower() != this.Prefix.ToLower();
                    __init_PrefixChanged = true;
                }
                return _PrefixChanged;
            }
        }

        /// <summary>
        /// Проверяет, что префиск таблицы не был изменен в контексте выполнения кода, в ином случае - генерирует исключение.
        /// </summary>
        internal void CheckPrefixChangeNotRequired()
        {
            if (this.PrefixChanged && this.OriginalExists)
                throw new Exception(string.Format("Операция недоступна, поскольку префиск таблицы {0} был изменен в {1} и зависимые от префикса индексы и триггеры должны быть переименованы.", this.OriginalPrefix, this.Prefix));
        }

        /// <summary>
        /// Проверяет, что префиск таблицы был изменен в контексте выполнения кода, в ином случае - генерирует исключение.
        /// </summary>
        internal void CheckPrefixChangeRequired()
        {
            if (!this.PrefixChanged)
                throw new Exception(string.Format("Операция недоступна, поскольку префикс таблицы {0} не был изменен.", this.Name));
        }

        /// <summary>
        /// Сбрасывает флаги инициализации названия таблицы и зависимых от него свойств.
        /// </summary>
        internal virtual void ResetName()
        {
            this.__init_Name = false;
            this.__init_OriginalName = false;
            this.__init_NameChanged = false;
            this.__init_RenameRequired = false;

            //если имя таблицы изменилось - инициализацию сущ. таблицы со столбцами и индексами.
            if (this.NameChanged)
                this.ResetExistingTable();

            //сбрасываем префикс таблицы, т.к. он мог измениться
            this.ResetPrefix();
        }

        internal virtual void ResetPrefix()
        {
            this.__init_Prefix = false;
            this.__init_OriginalPrefix = false;
            this.__init_PrefixChanged = false;

            //если префикс таблицы изменился, сбрасываем названия индексов.
            if (this.PrefixChanged)
            {
                //сбрасываем названия у индексов
                if (this.IndexesByRelativeNameInited)
                {
                    foreach (DBIndex index in this.Indexes)
                        index.ResetName();
                }

                //сбрасываем названия у триггеров.
                if (this.TriggersByRelativeNameInited)
                {
                    foreach (DBTrigger trigger in this.Triggers)
                        trigger.ResetName();
                }
            }

        }

        #endregion


        #region ExistingSchemaIndexes

        private bool __init_ExistingSchemaIndexes = false;
        private Dictionary<string, DBIndex> _ExistingSchemaIndexes;
        /// <summary>
        /// Актуальные существующие индексы, соответсвующие схеме индексов таблицы.
        /// </summary>
        private Dictionary<string, DBIndex> ExistingSchemaIndexes
        {
            get
            {
                if (!__init_ExistingSchemaIndexes)
                {
                    _ExistingSchemaIndexes = new Dictionary<string, DBIndex>();
                    foreach (DBIndex index in this.Indexes)
                    {
                        if (index.ExistingIndex != null && !_ExistingSchemaIndexes.ContainsKey(index.ExistingIndex.NameLow))
                            _ExistingSchemaIndexes.Add(index.ExistingIndex.NameLow, index);
                    }
                    __init_ExistingSchemaIndexes = true;
                }
                return _ExistingSchemaIndexes;
            }
        }

        /// <summary>
        /// Возвращает true, если индекс с названием indexName присутствует в таблице и в схеме таблицы.
        /// </summary>
        /// <param name="indexName">Полное название индекса.</param>
        /// <returns></returns>
        internal bool ContainsExistingSchemaIndex(string indexName)
        {
            if (string.IsNullOrEmpty(indexName))
                throw new ArgumentNullException("indexName");

            bool result = this.ExistingSchemaIndexes.ContainsKey(indexName.ToLower());
            return result;
        }

        /// <summary>
        /// Возвращает индекс, соответсвующий схеме, который имеет существующий в таблице индекс.
        /// </summary>
        /// <param name="indexName">Полное название индекса.</param>
        /// <returns></returns>
        internal DBIndex GetExistingSchemaIndex(string indexName)
        {
            if (string.IsNullOrEmpty(indexName))
                throw new ArgumentNullException("indexName");

            DBIndex index = null;
            if (this.ExistingSchemaIndexes.ContainsKey(indexName.ToLower()))
                index = this.ExistingSchemaIndexes[indexName.ToLower()];
            return index;
        }

        #endregion


        #region ExistingSchemaTriggers

        private bool __init_ExistingSchemaTriggers = false;
        private Dictionary<string, DBTrigger> _ExistingSchemaTriggers;
        /// <summary>
        /// Актуальные существующие триггеры, соответсвующие схеме триггеров таблицы.
        /// </summary>
        private Dictionary<string, DBTrigger> ExistingSchemaTriggers
        {
            get
            {
                if (!__init_ExistingSchemaTriggers)
                {
                    _ExistingSchemaTriggers = new Dictionary<string, DBTrigger>();
                    foreach (DBTrigger trigger in this.Triggers)
                    {
                        if (trigger.ExistingTrigger != null && !_ExistingSchemaTriggers.ContainsKey(trigger.ExistingTrigger.NameLow))
                            _ExistingSchemaTriggers.Add(trigger.ExistingTrigger.NameLow, trigger);
                    }
                    __init_ExistingSchemaTriggers = true;
                }
                return _ExistingSchemaTriggers;
            }
        }

        /// <summary>
        /// Возвращает true, если триггер с названием triggerName присутствует в таблице и в схеме таблицы.
        /// </summary>
        /// <param name="triggerName">Полное название триггера.</param>
        /// <returns></returns>
        internal bool ContainsExistingSchemaTrigger(string triggerName)
        {
            if (string.IsNullOrEmpty(triggerName))
                throw new ArgumentNullException("triggerName");

            bool result = this.ExistingSchemaTriggers.ContainsKey(triggerName.ToLower());
            return result;
        }

        /// <summary>
        /// Возвращает триггер, соответсвующий схеме, который имеет существующий в таблице триггер.
        /// </summary>
        /// <param name="triggerName">Полное название триггера.</param>
        /// <returns></returns>
        internal DBTrigger GetExistingSchemaTrigger(string triggerName)
        {
            if (string.IsNullOrEmpty(triggerName))
                throw new ArgumentNullException("triggerName");

            DBTrigger trigger = null;
            if (this.ExistingSchemaTriggers.ContainsKey(triggerName.ToLower()))
                trigger = this.ExistingSchemaTriggers[triggerName.ToLower()];
            return trigger;
        }

        #endregion


        #region ResetExisting Columns, Indexes and Triggers

        internal void ResetExistingColumns()
        {
            this.ResetExistingColumns(false);
        }

        private void ResetExistingColumns(bool isExistingTableReseting)
        {
            //выполняем ExistingTable.ResetColumns(), только если метод вызывается не в контексте this.ResetExistingTable().
            if (!isExistingTableReseting && this.ExistingTable != null)
            {
                this.ExistingTable.ResetColumns();
                this.__init_InitRequired = false;
                this.__init_RequiredInitAction = false;
            }

            //если индексы инициализированы, сбрасываем, ссылки на существующие индексы в индексах.
            if (this.ColumnsByNameInited)
            {
                foreach (DBColumn column in this.Columns)
                    column.ResetExistingColumn();
            }
        }

        internal void ResetExistingIndexes()
        {
            this.ResetExistingIndexes(false);
        }

        private void ResetExistingIndexes(bool isExistingTableReseting)
        {
            if (!isExistingTableReseting && this.ExistingTable != null)
            {
                this.ExistingTable.ResetIndexes();
                this.__init_InitRequired = false;
                this.__init_RequiredInitAction = false;
            }

            this.__init_ExistingSchemaIndexes = false;

            //если индексы инициализированы, сбрасываем, ссылки на существующие индексы в индексах.
            if (this.IndexesByRelativeNameInited)
            {
                foreach (DBIndex index in this.Indexes)
                {
                    index.ResetExistingIndex();
                }
            }
        }

        internal void ResetExistingTriggers()
        {
            this.ResetExistingTriggers(false);
        }

        private void ResetExistingTriggers(bool isExistingTableReseting)
        {
            if (!isExistingTableReseting && this.ExistingTable != null)
            {
                this.ExistingTable.ResetTriggers();
                this.__init_InitRequired = false;
                this.__init_RequiredInitAction = false;
            }

            this.__init_ExistingSchemaTriggers = false;

            //если триггеры инициализированы, сбрасываем, ссылки на существующие триггеры в триггерах.
            if (this.TriggersByRelativeNameInited)
            {
                foreach (DBTrigger trigger in this.Triggers)
                {
                    trigger.ResetExistingTrigger();
                }
            }
        }

        #endregion


        #region IdentityColumn

        private bool __init_IdentityColumn = false;
        private DBColumn _IdentityColumn;
        /// <summary>
        /// Ключевой инкрементный столбец-идентификатор таблицы.
        /// </summary>
        public DBColumn IdentityColumn
        {
            get
            {
                if (!__init_IdentityColumn)
                {
                    if (this.Schema.IdentityColumnSupported)
                        _IdentityColumn = this.GetColumn(this.Schema.IdentityColumn.Name, true);
                    __init_IdentityColumn = true;
                }
                return _IdentityColumn;
            }
        }

        /// <summary>
        /// Проверяет, что схема столбца автоинкремента не изменилась по сравнению с существующей таблицей.
        /// </summary>
        internal void CheckIdentityColumnUnmodified()
        {
            if (this.IdentityColumn != null)
            {
                if (this.ExistingTable != null && this.ExistingTable.IdentityColumn != null && !this.IdentityColumn.Exists)
                    throw new Exception(string.Format("Изменение столбца автоинкремента с [{0}] на [{1}] не поддерживается для таблицы {2}.",
                        this.ExistingTable.IdentityColumn.Name, this.IdentityColumn.Name, this.Name));
            }
        }

        #endregion


        #region InitialColumn

        private bool __init_InitialColumn = false;
        private DBColumn _InitialColumn;
        /// <summary>
        /// Столбец таблицы, создаваемый вместе с таблицей.
        /// </summary>
        internal DBColumn InitialColumn
        {
            get
            {
                if (!__init_InitialColumn)
                {
                    _InitialColumn = this.GetColumn(this.Schema.InitialColumn.Name, true);
                    __init_InitialColumn = true;
                }
                return _InitialColumn;
            }
        }

        #endregion


        #region PrimaryKey

        private bool __init_PrimaryKey = false;
        private DBClusteredIndex _PrimaryKey;
        /// <summary>
        /// Индекс первичного ключа таблицы.
        /// </summary>
        public DBClusteredIndex PrimaryKey
        {
            get
            {
                if (!__init_PrimaryKey)
                {
                    if (this.Schema.PrimaryKeySupported)
                        _PrimaryKey = (DBClusteredIndex)this.GetIndex(this.Schema.PrimaryKey.RelativeName, true);
                    __init_PrimaryKey = true;
                }
                return _PrimaryKey;
            }
        }


        #endregion


        #region ExistingTable

        private bool __init_ExistingTable = false;
        private DBTableInfo _ExistingTable;
        /// <summary>
        /// Существующая таблица в базе данных.
        /// </summary>
        public DBTableInfo ExistingTable
        {
            get
            {
                if (!__init_ExistingTable)
                {
                    //проверяем, что переименование таблицы не требуется.
                    this.CheckRenameNotRequired();

                    //получаем данные таблицы.
                    _ExistingTable = this.GetTableInfo("ExistingTable", this.Name);

                    __init_ExistingTable = true;
                }
                return _ExistingTable;
            }
        }

        private bool __init_OriginalExistingTable = false;
        private DBTableInfo _OriginalExistingTable;
        /// <summary>
        /// Существующая таблица в базе данных, до переименования таблицы.
        /// </summary>
        internal DBTableInfo OriginalExistingTable
        {
            get
            {
                if (!__init_OriginalExistingTable)
                {
                    //обнуляем переменную, т.к. после сброса ResetExistingTable поле _OriginalExistingTable может уже быть не пустым.
                    _OriginalExistingTable = this.GetTableInfo("OriginalExistingTable", this.OriginalName);

                    __init_OriginalExistingTable = true;
                }
                return _OriginalExistingTable;
            }
        }

        private bool __init_Exists = false;
        private bool _Exists;
        /// <summary>
        /// Возвращает true, если таблица существует а базе данных.
        /// </summary>
        public bool Exists
        {
            get
            {
                if (!__init_Exists)
                {
                    //проверяем существование таблицы.
                    _Exists = this.ExistingTable != null;

                    __init_Exists = true;
                }
                return _Exists;
            }
        }

        private bool __init_OriginalExists = false;
        private bool _OriginalExists;
        /// <summary>
        /// Возвращает true, если таблица с исходным названием до переименования существует в базе данных.
        /// </summary>
        public bool OriginalExists
        {
            get
            {
                if (!__init_OriginalExists)
                {
                    _OriginalExists = this.OriginalExistingTable != null;
                    __init_OriginalExists = true;
                }
                return _OriginalExists;
            }
        }

        /// <summary>
        /// Возвращает true, если таблица с названием tableName существует в базе данных.
        /// </summary>
        /// <param name="tableName">Название таблицы.</param>
        /// <param name="isOriginalTable">При установленном true выполняет операцию с оригнальной таблицей до переименования или до смены базы данных.</param>
        /// <returns></returns>
        protected DBTableInfo GetTableInfo(string tableKey, string tableName)
        {
            if (string.IsNullOrEmpty(tableKey))
                throw new ArgumentNullException("tableKey");
            if (string.IsNullOrEmpty(tableName))
                throw new ArgumentNullException("tableName");

            DBTableInfo tableInfo = null;
            DataRow result = this.Connection.TableInfoAdapter.GetTableData(tableKey, tableName);
            if (result != null)
                tableInfo = new DBTableInfo(tableKey, result, this);
            return tableInfo;
        }

        private void ResetExistingTable()
        {
            this.__init_ExistingTable = false;
            this.__init_OriginalExistingTable = false;
            this.__init_Exists = false;
            this.__init_OriginalExists = false;
            this.__init_RenameRequired = false;
            this.__init_InitRequired = false;
            this.__init_RequiredInitAction = false;

            this.ResetExistingColumns(true);
            this.ResetExistingIndexes(true);
            this.ResetExistingTriggers(true);
        }

        /// <summary>
        /// Проверяет существование таблицы в базе данных. Генерирует исключение в случае отсутствия таблицы в базе данных.
        /// </summary>
        internal void CheckExists()
        {
            if (!this.Exists)
                throw new Exception(string.Format("Операция доступна только для существующей в базе данных таблицы {0}.", this.Name));
        }

        /// <summary>
        /// Проверяет существование таблицы с исходным названием в базе данных. Генерирует исключение в случае отсутствия таблицы с исходным названием в базе данных.
        /// </summary>
        internal void CheckOriginalExists()
        {
            if (!this.OriginalExists)
                throw new Exception(string.Format("Операция доступна только для существующей в базе данных таблицы {0}.", this.OriginalName));
        }

        #endregion


        #region Columns

        /// <summary>
        /// Создает экземпляр столбца таблицы.
        /// </summary>
        /// <param name="columnSchema">Схема столбца.</param>
        /// <returns></returns>
        private DBColumn CreateColumn(DBColumnSchema columnSchema)
        {
            if (columnSchema == null)
                throw new ArgumentNullException("columnSchema");

            return new DBColumn(columnSchema, this);
        }

        /// <summary>
        /// Возвращает значение флага инициализации свойства ColumnsByName.
        /// </summary>
        private bool ColumnsByNameInited
        {
            get { return this.__init_ColumnsByName; }
        }

        /// <summary>
        /// Добавляет столбец схемы в коллекцию столбцов таблицы.
        /// </summary>
        /// <param name="columnSchema">Схема столбца.</param>
        internal void AddSchemaColumn(DBColumnSchema columnSchema)
        {
            if (columnSchema == null)
                throw new ArgumentNullException("columnSchema");

            //добавляем столбец в таблицу, только если словарь столбцов инициализирован.
            if (this.ColumnsByNameInited)
            {
                if (!this.ContainsColumn(columnSchema.Name))
                {
                    DBColumn column = this.CreateColumn(columnSchema);
                    this.ColumnsByName.Add(columnSchema.NameLow, column);

                    //сбрасываем инициализацию столбцов.
                    this.ResetColumns();

                    //сбрасываем инициализацию флага необходимости обновления тела триггера.
                    this.ResetTriggersUpdateRequired();
                }
            }
        }

        /// <summary>
        /// Удаляет столбец схемы из таблицы.
        /// </summary>
        /// <param name="columnName">Название столбца.</param>
        internal void DeleteSchemaColumn(string columnName)
        {
            if (string.IsNullOrEmpty(columnName))
                throw new ArgumentNullException("columnName");

            //удаляем столбец из таблицы.
            if (this.ColumnsByNameInited)
            {
                if (this.ContainsColumn(columnName))
                {
                    this.ColumnsByName.Remove(columnName.ToLower());

                    //сбрасываем инициализацию столбцов.
                    this.ResetColumns();

                    //сбрасываем инициализацию флага необходимости обновления тела триггера.
                    this.ResetTriggersUpdateRequired();
                }
            }
        }

        private bool __init_ColumnsByName = false;
        private Dictionary<string, DBColumn> _ColumnsByName;
        /// <summary>
        /// Словарь столбцов таблицы по имени.
        /// </summary>
        private Dictionary<string, DBColumn> ColumnsByName
        {
            get
            {
                if (!__init_ColumnsByName)
                {
                    _ColumnsByName = new Dictionary<string, DBColumn>();

                    foreach (DBColumnSchema columnSchema in this.Schema.Columns)
                    {
                        DBColumn column = this.CreateColumn(columnSchema);
                        if (!_ColumnsByName.ContainsKey(column.NameLow))
                            _ColumnsByName.Add(column.NameLow, column);
                        else
                            throw new Exception(string.Format("Столбец с названием [{0}] уже присутствует в схеме таблицы [{1}].", column.Name, this.Name));
                    }
                    __init_ColumnsByName = true;
                }
                return _ColumnsByName;
            }
        }

        /// <summary>
        /// Возвращает true, если столбец входит в состав столбцов таблицы.
        /// </summary>
        /// <param name="columnName">Название столбца.</param>
        /// <returns></returns>
        public bool ContainsColumn(string columnName)
        {
            if (string.IsNullOrEmpty(columnName))
                throw new ArgumentNullException("columnName");
            bool result = this.ColumnsByName.ContainsKey(columnName.ToLower());
            return result;
        }

        /// <summary>
        /// Возвращает экземпляр столбца по схеме столбца.
        /// </summary>
        /// <param name="columnName">Название столбца.</param>
        /// <returns></returns>
        public DBColumn GetColumn(string columnName)
        {
            return this.GetColumn(columnName, false);
        }

        /// <summary>
        /// Возвращает экземпляр столбца по схеме столбца.
        /// </summary>
        /// <param name="columnName">Название столбца.</param>
        /// <param name="throwNotFoundException">При установленном значении true генерирует исключения в случае отустствия экземпляра столбца в таблице.</param>
        /// <returns></returns>
        public DBColumn GetColumn(string columnName, bool throwNotFoundException)
        {
            if (string.IsNullOrEmpty(columnName))
                throw new ArgumentNullException("columnName");

            DBColumn column = null;
            string columnNameLow = columnName.ToLower();
            if (this.ColumnsByName.ContainsKey(columnNameLow))
                column = this.ColumnsByName[columnNameLow];

            if (throwNotFoundException && column == null)
                throw new Exception(string.Format("Не удалось получить экземпляр столбеца [{0}] для таблицы {1}.", columnName, this.Name));

            return column;
        }

        private bool __init_Columns = false;
        private DBCollection<DBColumn> _Columns;
        /// <summary>
        /// Коллекция всех столбцов таблицы.
        /// </summary>
        public DBCollection<DBColumn> Columns
        {
            get
            {
                if (!__init_Columns)
                {
                    _Columns = new DBCollection<DBColumn>(this.ColumnsByName.Values);
                    __init_Columns = true;
                }
                return _Columns;
            }
        }

        /// <summary>
        /// Сбрасывает инициализацию коллекции столбцов.
        /// </summary>
        private void ResetColumns()
        {
            this.__init_Columns = false;
        }

        /// <summary>
        /// Сбрасывает инициализацию всех коллекций столбцов, в том числе и системной коллекции столбцов.
        /// </summary>
        protected void ResetColumnsFull()
        {
            this.ResetColumns();
            this.__init_IdentityColumn = false;
            this.__init_InitialColumn = false;
            this.__init_ColumnsByName = false;
            this.__init_SystemColumns = false;
        }

        private bool __init_SystemColumns = false;
        private DBCollection<DBColumn> _SystemColumns;
        /// <summary>
        /// Коллекция системных столбцов таблицы. Данные столбцы создаются вместе с таблицей.
        /// Остальные столбцы могут быть добавлены по отдельности, либо при группой инициализации столбцов.
        /// </summary>
        public DBCollection<DBColumn> SystemColumns
        {
            get
            {
                if (!__init_SystemColumns)
                {
                    _SystemColumns = new DBCollection<DBColumn>();
                    foreach (DBColumn column in this.Columns)
                    {
                        if (column.IsSystem)
                            _SystemColumns.Add(column);
                    }
                    if (_SystemColumns.Count == 0)
                        throw new Exception(string.Format("Отсутствуют системные столбцы для таблицы {0}.", this.Name));
                    __init_SystemColumns = true;
                }
                return _SystemColumns;
            }
        }

        /// <summary>
        /// Добавляет столбец, присутствующий в схеме, в таблицу.
        /// </summary>
        /// <param name="columnName">Название столбца, присутствующего в схеме таблицы.</param>
        internal virtual void AddColumnInternal(string columnName)
        {
            if (string.IsNullOrEmpty(columnName))
                throw new ArgumentNullException("columnName");

            //проверяем существование столбца в схеме таблицы, чтобы получить корректный exception в случае отсустствия.
            this.Schema.ContainsColumn(columnName, true);

            //получаем столбец схемы.
            DBColumn column = this.GetColumn(columnName, true);

            //создаем столбец
            column.Create();
        }

        /// <summary>
        /// Удаляет столбец, отсутствующий в схеме, из таблицы.
        /// </summary>
        /// <param name="columnName">Название столбца.</param>
        internal virtual void DeleteColumnInternal(string columnName)
        {
            if (string.IsNullOrEmpty(columnName))
                throw new ArgumentNullException("columnName");

            //проверяем, что название таблицы не изменилось и переименование перед операцией не требуется.
            this.CheckRenameNotRequired();

            //проверяем, что префикс таблицы не изменился
            this.CheckPrefixChangeNotRequired();

            //проверяем существование таблицы
            this.CheckExists();

            //проверяем, что схема столбца удалена из коллекции столбцов схемы таблицы.
            if (this.ContainsColumn(columnName))
                throw new Exception(string.Format("Столбец [{0}] должен быть удален из схемы таблицы {1} перед физическим удалением из таблицы.", columnName, this.SchemaAdapter.TableName));

            //получаем существующий столбец и проверяем, что столбец входит в состав таблицы.
            DBColumnInfo column = this.ExistingTable.GetColumn(columnName, true);

            //проверяем, что пытаемся удалить не единственный столбец в таблице.
            if (this.ExistingTable.Columns.Count == 1)
                throw new Exception(string.Format("Удаление столбца [{0}] запрещено, поскольку он является единственным столбцом таблицы {1}.", columnName, this.Name));


            //удаляем индексы, в которые был включен данный столбец.
            //формируем запрос на пересоздание индексов, в которые был включен не только удаляемый столбец.
            StringBuilder stDropIndexesQuery = new StringBuilder();
            StringBuilder stRecreateIndexesQuery = new StringBuilder();
            List<DBIndex> recreatingIndexes = new List<DBIndex>();
            foreach (DBIndexInfo index in column.DependendIndexes)
            {
                stDropIndexesQuery.Append(index.GetDropQuery());

                //не проверяем на количество столбцов, т.к. индекс с одним столбцом должен был быть удален из схемы на стадии удаления индекса из схемы.
                if (index.SchemaIndex != null)
                {
                    //если индекс содержит не только удаляемый столбец, то добавляем этот индекс в коллекцию пересоздаваемых.
                    recreatingIndexes.Add(index.SchemaIndex);
                    stRecreateIndexesQuery.Append(index.SchemaIndex.GetInitQuery());
                }
                else
                {
                    //если индекс не содержится в схеме, проверяем, что индекс удален из схемы таблицы.
                    if (this.ContainsExistingSchemaIndex(index.Name))
                        throw new Exception(string.Format("Схема индекса [{0}] должена быть удалена из схемы таблицы {1} перед физическим удалением из таблицы.", index.Name, this.Name));
                }
            }
            //удаляем индексы
            if (stDropIndexesQuery.Length > 0)
                this.DataAdapter.ExecuteQuery(stDropIndexesQuery.ToString());

            //удаляем столбец из таблицы
            string query = column.GetDropQuery();
            this.DataAdapter.ExecuteQuery(query);

            //пересоздаем индексы без столбца
            if (stRecreateIndexesQuery.Length > 0)
                this.DataAdapter.ExecuteQuery(stRecreateIndexesQuery.ToString());

            //сбрасываем флаги инициализации существующих столбцов и индексов.
            this.ResetExistingColumns();

            //сбрасываем флаги инициализации существующих индексов, если были изменения индексов.
            if (stDropIndexesQuery.Length > 0 || stRecreateIndexesQuery.Length > 0)
                this.ResetExistingIndexes();
        }

        /// <summary>
        /// Сбрасывает инициализацию названий столбцов при переименовании столбца.
        /// </summary>
        internal virtual void ResetColumnNames()
        {
            //сбрасываем инициализацию всех столбцов.
            this.ResetColumnsFull();
            this.ResetIndexesFull();
            this.ResetTriggersFull();
        }

        /// <summary>
        /// Переименовывает столбец, с названием columnName.
        /// </summary>
        /// <param name="columnName">Новое название переименовываемого столбца.</param>
        internal virtual void RenameColumnInternal(string columnName)
        {
            if (string.IsNullOrEmpty(columnName))
                throw new ArgumentNullException("columnName");

            //проверяем существование столбца в схеме таблицы, чтобы получить корректный exception в случае отсустствия.
            this.Schema.ContainsColumn(columnName, true);

            //получаем столбец схемы.
            DBColumn column = this.GetColumn(columnName, true);

            //переименовываем столбец
            column.Rename();
        }

        /// <summary>
        /// Обновляет параметры столбца.
        /// </summary>
        /// <param name="columnName">Название столбца.</param>
        internal virtual void UpdateColumnInternal(string columnName)
        {
            if (string.IsNullOrEmpty(columnName))
                throw new ArgumentNullException("columnName");

            //проверяем существование столбца в схеме таблицы, чтобы получить корректный exception в случае отсустствия.
            this.Schema.ContainsColumn(columnName, true);

            //получаем столбец схемы.
            DBColumn column = this.GetColumn(columnName, true);

            //обновляем столбец
            column.Update();
        }

        #endregion


        #region Indexes

        /// <summary>
        /// Возвращает значение флага инициализации свойства IndexesByRelativeName.
        /// </summary>
        private bool IndexesByRelativeNameInited
        {
            get { return this.__init_IndexesByRelativeName; }
        }

        /// <summary>
        /// Добавляет индекс схемы в коллекцию индексов таблицы.
        /// </summary>
        /// <param name="indexSchema">Схема столбца.</param>
        internal void AddSchemaIndex(DBIndexSchema indexSchema)
        {
            if (indexSchema == null)
                throw new ArgumentNullException("indexSchema");

            //добавляем столбец в таблицу, только если словарь столбцов инициализирован.
            if (this.IndexesByRelativeNameInited)
            {
                if (!this.IndexesByRelativeName.ContainsKey(indexSchema.RelativeNameLow))
                {
                    DBIndex index = null;
                    if (this.Schema.IsPrimaryKey(indexSchema.RelativeName))
                        index = new DBClusteredIndex(indexSchema, this);
                    else
                        index = new DBNonclusteredIndex(indexSchema, this);
                    this.IndexesByRelativeName.Add(indexSchema.RelativeNameLow, index);

                    //сбрасываем инициализацию столбцов.
                    this.ResetIndexes();
                }
            }
        }

        /// <summary>
        /// Удаляет индекс схемы из таблицы.
        /// </summary>
        /// <param name="indexRelativeName">Относительное название индекса.</param>
        internal void DeleteSchemaIndex(string indexRelativeName)
        {
            if (string.IsNullOrEmpty(indexRelativeName))
                throw new ArgumentNullException("indexName");

            //удаляем столбец из таблицы.
            if (this.IndexesByRelativeNameInited)
            {
                if (this.IndexesByRelativeName.ContainsKey(indexRelativeName.ToLower()))
                {
                    this.IndexesByRelativeName.Remove(indexRelativeName.ToLower());

                    //сбрасываем инициализацию столбцов.
                    this.ResetIndexes();
                }
            }
        }

        private bool __init_IndexesByRelativeName = false;
        private Dictionary<string, DBIndex> _IndexesByRelativeName;
        /// <summary>
        /// Словарь индексов таблицы по относительному имени.
        /// </summary>
        private Dictionary<string, DBIndex> IndexesByRelativeName
        {
            get
            {
                if (!__init_IndexesByRelativeName)
                {
                    _IndexesByRelativeName = new Dictionary<string, DBIndex>();

                    foreach (DBIndexSchema indexSchema in this.Schema.Indexes)
                    {
                        //создаем экземпляр обычного или кластеризованного индекса.
                        DBIndex index = null;
                        if (!this.Schema.IsPrimaryKey(indexSchema.RelativeName))
                            index = new DBNonclusteredIndex(indexSchema, this);
                        else
                            index = new DBClusteredIndex(indexSchema, this);

                        //добавляем индекс в словарь.
                        if (!_IndexesByRelativeName.ContainsKey(index.Schema.RelativeNameLow))
                            _IndexesByRelativeName.Add(index.Schema.RelativeNameLow, index);
                        else
                            throw new Exception(string.Format("Индекс с названием [{0}] уже присутствует в схеме таблицы [{1}].", index.Schema.RelativeName, this.Name));
                    }
                    __init_IndexesByRelativeName = true;
                }
                return _IndexesByRelativeName;
            }
        }

        /// <summary>
        /// Возвращает экземпляр индекса по схеме индекса.
        /// </summary>
        /// <param name="indexRelativeName">Относительное название индекса.</param>
        /// <returns></returns>
        public DBIndex GetIndex(string indexRelativeName)
        {
            return this.GetIndex(indexRelativeName, false);
        }

        /// <summary>
        /// Возвращает экземпляр индекса по схеме индекса.
        /// </summary>
        /// <param name="indexRelativeName">Относительное название индекса, уникальное в рамках таблицы.</param>
        /// <param name="throwNotFoundException">При установленном значении true генерирует исключения в случае отустствия экземпляра индекса в таблице.</param>
        /// <returns></returns>
        public DBIndex GetIndex(string indexRelativeName, bool throwNotFoundException)
        {
            if (string.IsNullOrEmpty(indexRelativeName))
                throw new ArgumentNullException("indexRelativeName");

            DBIndex index = null;
            string indexNameLow = indexRelativeName.ToLower();
            if (this.IndexesByRelativeName.ContainsKey(indexNameLow))
                index = this.IndexesByRelativeName[indexNameLow];

            if (throwNotFoundException && index == null)
                throw new Exception(string.Format("Не удалось получить экземпляр индекса '{0}' для таблицы {1}.", indexRelativeName, this.Name));

            return index;
        }

        /// <summary>
        /// Сбрасывает коллекцию индексов.
        /// </summary>
        private void ResetIndexes()
        {
            this.__init_Indexes = false;
            this.__init_IndexesByFullName = false;
        }

        /// <summary>
        /// Сбрасывает инициализацию всех коллекций индексов, в том числе и системной коллекции индексов.
        /// </summary>
        protected void ResetIndexesFull()
        {
            this.ResetIndexes();
            this.__init_PrimaryKey = false;
            this.__init_IndexesByRelativeName = false;
            this.__init_SystemIndexes = false;
        }

        private bool __init_Indexes = false;
        private DBCollection<DBIndex> _Indexes;
        /// <summary>
        /// Коллекция всех индексов таблицы.
        /// </summary>
        public DBCollection<DBIndex> Indexes
        {
            get
            {
                if (!__init_Indexes)
                {
                    _Indexes = new DBCollection<DBIndex>(this.IndexesByRelativeName.Values);
                    __init_Indexes = true;
                }
                return _Indexes;
            }
        }

        private bool __init_IndexesByFullName = false;
        private Dictionary<string, DBIndex> _IndexesByFullName;
        /// <summary>
        /// Словарь индексов таблицы по полному названию индексов.
        /// </summary>
        private Dictionary<string, DBIndex> IndexesByFullName
        {
            get
            {
                if (!__init_IndexesByFullName)
                {
                    _IndexesByFullName = new Dictionary<string, DBIndex>();
                    foreach (DBIndex index in this.Indexes)
                    {
                        if (!_IndexesByFullName.ContainsKey(index.NameLow))
                            _IndexesByFullName.Add(index.NameLow, index);
                    }
                    __init_IndexesByFullName = true;
                }
                return _IndexesByFullName;
            }
        }

        /// <summary>
        /// Возвращает true, если индекс с полным названием indexName содержится в коллекции индексов таблицы.
        /// </summary>
        /// <param name="indexName">Полное название индекса.</param>
        /// <returns></returns>
        internal bool ContainsIndex(string indexName)
        {
            if (string.IsNullOrEmpty(indexName))
                throw new ArgumentNullException("indexName");

            bool result = this.IndexesByFullName.ContainsKey(indexName.ToLower());
            return result;
        }

        private bool __init_SystemIndexes = false;
        private DBCollection<DBIndex> _SystemIndexes;
        /// <summary>
        /// Коллекция системных индексов таблицы. Данные индексы создаются вместе с таблицей.
        /// Остальные индексы могут быть добавлены по отдельности, либо при группой инициализации индексов.
        /// </summary>
        public DBCollection<DBIndex> SystemIndexes
        {
            get
            {
                if (!__init_SystemIndexes)
                {
                    _SystemIndexes = new DBCollection<DBIndex>();

                    //делаем цикл по системным индексам схемы, т.к. эта коллекция инициирует обращение к PrimaryKey,
                    //и, соответственно, валидирует его. В ином случае был ошибочный кейс когда был задан PrimaryKeyRelativeName, коллеция индексов в схеме была пустой
                    //и обращение к PrimaryKey не инициировалось и не валидировалось.
                    foreach (DBIndexSchema systemIndexSchema in this.Schema.SystemIndexes)
                    {
                        //получаем индек
                        DBIndex systemIndex = this.GetIndex(systemIndexSchema.RelativeName, true);
                        if (!systemIndex.IsSystem)
                            throw new Exception(string.Format("Индекс {0} должен быть системным.", systemIndexSchema.RelativeName));
                        _SystemIndexes.Add(systemIndex);
                    }
                    __init_SystemIndexes = true;
                }
                return _SystemIndexes;
            }
        }

        /// <summary>
        /// Создает индекс, присутствующий в схеме таблицы.
        /// </summary>
        /// <param name="indexRelativeName">Относительное название индекса, присутствующего в схеме таблицы.</param>
        internal virtual void AddIndexInternal(string indexRelativeName)
        {
            if (string.IsNullOrEmpty(indexRelativeName))
                throw new ArgumentNullException("indexRelativeName");

            //проверяем существование индекса в схеме таблицы, чтобы получить корректный exception в случае отсустствия.
            this.Schema.ContainsIndex(indexRelativeName, true);

            //получаем индекс схемы.
            DBIndex index = this.GetIndex(indexRelativeName, true);

            //создаем индекс
            index.Create();
        }

        /// <summary>
        /// Удаляет индекс, отсутствующий в схеме, из таблицы.
        /// </summary>
        /// <param name="deletedIndexSchema">Схема удаленного индекса, по которой определяются индексы необходимые для удаления из таблицы.</param>
        internal virtual void DeleteIndexInternal(DBIndexSchema deletedIndexSchema)
        {
            if (deletedIndexSchema == null)
                throw new ArgumentNullException("deletedIndexSchema");

            this.DeleteIndexSealed(null, deletedIndexSchema);
        }

        /// <summary>
        /// Удаляет индекс с заданным относительным названием.
        /// </summary>
        /// <param name="indexRelativeName">Относительное название индекса.</param>
        internal virtual void DeleteIndexInternal(string indexRelativeName)
        {
            if (string.IsNullOrEmpty(indexRelativeName))
                throw new ArgumentNullException("indexRelativeName");

            this.DeleteIndexSealed(indexRelativeName, null);
        }

        /// <summary>
        /// Удаляет индекс, отсутствующий в схеме, из таблицы.
        /// </summary>
        /// <param name="indexRelativeName">Относительное название индекса.</param>
        /// <param name="deletedIndexSchema">Схема удаленного индекса, по которой определяются индексы необходимые для удаления из таблицы.</param>
        private void DeleteIndexSealed(string indexRelativeName, DBIndexSchema deletedIndexSchema)
        {
            if (string.IsNullOrEmpty(indexRelativeName) && deletedIndexSchema == null)
                throw new ArgumentException("Не задан ни один из параметров определения удаляемого индекса indexRelativeName или deletedIndexSchema.");

            //проверяем, что название таблицы не изменилось и переименование перед операцией не требуется.
            this.CheckRenameNotRequired();

            //проверяем, что префикс таблицы не изменился
            this.CheckPrefixChangeNotRequired();

            //проверяем существование таблицы
            this.CheckExists();

            //находим индексы, требуемые для удаления.
            StringBuilder stDroppedIndexes = new StringBuilder();
            if (!string.IsNullOrEmpty(indexRelativeName))
            {
                DBIndex schemaIndex = this.GetIndex(indexRelativeName);
                if (schemaIndex != null)
                    throw new Exception(string.Format("Индекс с относительным названием [{0}] должен быть удален из схемы таблицы {1} перед физическим удалением из таблицы.", indexRelativeName, this.SchemaAdapter.TableName));

                string deletedIndexName = DBNonclusteredIndex.GetIndexName(this.Prefix, indexRelativeName);
                DBIndexInfo deletingIndex = this.ExistingTable.GetIndex(deletedIndexName, true);
                stDroppedIndexes.Append(deletingIndex.GetDropQuery());
            }
            else
            {
                if (deletedIndexSchema == null)
                    throw new ArgumentNullException("deletedIndexSchema");

                DBIndex schemaIndex = this.GetIndex(deletedIndexSchema.RelativeName);
                if (schemaIndex != null)
                    throw new Exception(string.Format("Индекс с относительным названием [{0}] должен быть удален из схемы таблицы {1} перед физическим удалением из таблицы.", deletedIndexSchema.RelativeName, this.SchemaAdapter.TableName));

                foreach (DBIndexInfo existingIndex in this.ExistingTable.Indexes)
                {
                    //пропускаем индексы, соответсвующие схеме.
                    if (existingIndex.SchemaIndex != null)
                        continue;

                    bool isDeletingIndex = false;
                    if (existingIndex.IsPrimaryKey)
                    {
                        //для первичного ключа удаляем только по равенству набора столбцов.
                        if (deletedIndexSchema.ColumnsEqual(existingIndex))
                            isDeletingIndex = true;
                    }
                    else
                    {
                        string schemaIndexName = DBNonclusteredIndex.GetIndexName(this.Prefix, deletedIndexSchema.RelativeName);
                        if (string.IsNullOrEmpty(schemaIndexName))
                            throw new Exception(string.Format("Не удалось получить название индекса по относительному имени '{0}' для таблицы {1}.", deletedIndexSchema.RelativeName, this.Name));

                        //сравниваем на точное равенство имя индекса в схеме и имя существующего индекса, либо сравниваем набор столбцов индекса.
                        if (schemaIndexName.ToLower() == existingIndex.NameLow)
                            isDeletingIndex = true;
                        else if (deletedIndexSchema.ColumnsEqual(existingIndex))
                            isDeletingIndex = true;
                    }

                    //если индекс опеределился как удаляемый, добавляем запрос на удаление данного индекса.
                    if (isDeletingIndex)
                        stDroppedIndexes.Append(existingIndex.GetDropQuery());
                }
            }

            //ругаемся, если не нашлось индексов для удаления.
            if (stDroppedIndexes.Length == 0)
                throw new Exception(string.Format("Не удалось определить удаляемые индексы, соответствующие схеме индекса '{0}' для таблицы {1}.", deletedIndexSchema.RelativeName, this.Name));

            //выполняем запрос по удалению индексов.
            this.DataAdapter.ExecuteQuery(stDroppedIndexes.ToString());

            //сбрасываем инициализацию сущеуствующих индексов.
            this.ResetExistingIndexes();
        }

        /// <summary>
        /// Сбрасывает инициализацию относительных названий индексов при переименовании индекса.
        /// </summary>
        internal virtual void ResetIndexRelativeNames()
        {
            //сбрасываем инициализацию всех столбцов.
            this.ResetIndexesFull();
        }

        /// <summary>
        /// Пересоздает индекс, если изменение параметров индекса требует его пересоздания.
        /// </summary>
        /// <param name="indexRelativeName">Относительное название индекса.</param>
        internal virtual void RecreateIndexInternal(string indexRelativeName)
        {
            if (string.IsNullOrEmpty(indexRelativeName))
                throw new ArgumentNullException("indexRelativeName");

            //получаем индекс схемы.
            DBIndex index = this.GetIndex(indexRelativeName, true);

            //пересоздаем индекс
            index.Recreate();
        }


        /// <summary>
        /// Переименовывает индекс с относительным названием indexRelativeName.
        /// </summary>
        /// <param name="indexRelativeName">Новое относительное название переименовываемого индекса.</param>
        internal virtual void RenameIndexInternal(string indexRelativeName)
        {
            if (string.IsNullOrEmpty(indexRelativeName))
                throw new ArgumentNullException("indexRelativeName");

            //получаем индекс схемы.
            DBIndex index = this.GetIndex(indexRelativeName, true);

            //переименовываем индекс
            index.Rename();
        }

        #endregion


        #region Triggers

        /// <summary>
        /// Возвращает значение флага инициализации свойства TriggersByRelativeName.
        /// </summary>
        private bool TriggersByRelativeNameInited
        {
            get { return this.__init_TriggersByRelativeName; }
        }

        private bool __init_TriggersByRelativeName = false;
        private Dictionary<string, DBTrigger> _TriggersByRelativeName;
        /// <summary>
        /// Словарь триггеров таблицы по относительному имени.
        /// </summary>
        private Dictionary<string, DBTrigger> TriggersByRelativeName
        {
            get
            {
                if (!__init_TriggersByRelativeName)
                {
                    _TriggersByRelativeName = new Dictionary<string, DBTrigger>();

                    foreach (DBTriggerSchema triggerSchema in this.Schema.Triggers)
                    {
                        //создаем экземпляр триггера.
                        DBTrigger trigger = new DBTrigger(triggerSchema, this);

                        //добавляем триггер в словарь.
                        if (!_TriggersByRelativeName.ContainsKey(trigger.Schema.RelativeNameLow))
                            _TriggersByRelativeName.Add(trigger.Schema.RelativeNameLow, trigger);
                        else
                            throw new Exception(string.Format("Триггер с относительным названием [{0}] уже присутствует в схеме таблицы [{1}].", trigger.Schema.RelativeName, this.Name));
                    }
                    __init_TriggersByRelativeName = true;
                }
                return _TriggersByRelativeName;
            }
        }

        /// <summary>
        /// Возвращает экземпляр триггера по схеме триггера.
        /// </summary>
        /// <param name="triggerRelativeName">Относительное название триггера.</param>
        /// <returns></returns>
        public DBTrigger GetTrigger(string triggerRelativeName)
        {
            return this.GetTrigger(triggerRelativeName, false);
        }

        /// <summary>
        /// Возвращает экземпляр триггера по схеме триггера.
        /// </summary>
        /// <param name="triggerRelativeName">Относительное название триггера, уникальное в рамках таблицы.</param>
        /// <param name="throwNotFoundException">При установленном значении true генерирует исключения в случае отустствия экземпляра триггера в таблице.</param>
        /// <returns></returns>
        public DBTrigger GetTrigger(string triggerRelativeName, bool throwNotFoundException)
        {
            if (string.IsNullOrEmpty(triggerRelativeName))
                throw new ArgumentNullException("triggerRelativeName");

            DBTrigger trigger = null;
            string triggerNameLow = triggerRelativeName.ToLower();
            if (this.TriggersByRelativeName.ContainsKey(triggerNameLow))
                trigger = this.TriggersByRelativeName[triggerNameLow];

            if (throwNotFoundException && trigger == null)
                throw new Exception(string.Format("Не удалось получить экземпляр триггера '{0}' для таблицы {1}.", triggerRelativeName, this.Name));

            return trigger;
        }

        /// <summary>
        /// Сбрасывает инициализацию всех коллекций триггеров.
        /// </summary>
        internal void ResetTriggersFull()
        {
            this.__init_Triggers = false;
            this.__init_TriggersByRelativeName = false;
        }

        /// <summary>
        /// Сбрасывает признак необходимости обновления тела триггера.
        /// </summary>
        private void ResetTriggersUpdateRequired()
        {
            if (this.TriggersByRelativeNameInited)
            {
                foreach (DBTrigger trigger in this.Triggers)
                    trigger.ResetUpdateRequired();
            }
        }

        private bool __init_Triggers = false;
        private DBCollection<DBTrigger> _Triggers;
        /// <summary>
        /// Коллекция всех триггеров таблицы.
        /// </summary>
        public DBCollection<DBTrigger> Triggers
        {
            get
            {
                if (!__init_Triggers)
                {
                    _Triggers = new DBCollection<DBTrigger>(this.TriggersByRelativeName.Values);
                    __init_Triggers = true;
                }
                return _Triggers;
            }
        }

        /// <summary>
        /// Возвращает функциональное тело триггера в соответствии со схемой триггера.
        /// </summary>
        /// <param name="triggerSchema">Схема триггера.</param>
        /// <returns></returns>
        internal abstract string GetTriggerBody(DBTriggerSchema triggerSchema);

        #endregion


        #region InitTable

        /// <summary>
        /// Возвращает запрос инициализации таблицы с системными столбцам, если таблица или столбцы не существуют.
        /// </summary>
        /// <returns></returns>
        private string GetInitTableQuery()
        {
            //запрос создания таблицы и добавления системных столбцов, добавляемых вместе с созданием таблицы.
            StringBuilder queryBuilder = new StringBuilder();

            //проверяем, что схема автоинкремента не изменилась
            this.CheckIdentityColumnUnmodified();

            if (!this.Exists)
            {
                //получаем текст запроса инициализации таблицы.
                string createTableQuery = @"
IF(NOT EXISTS(
	SELECT name FROM sys.tables WITH(NOLOCK)
	WHERE name = N'{TableNameText}'))
BEGIN
	CREATE TABLE [dbo].[{TableName}](
	    {InitialColumnDefinition}
    ) ON [PRIMARY]
END"
                    .ReplaceKey("TableNameText", this.Name.QueryEncode())
                    .ReplaceKey("TableName", this.Name)
                    .ReplaceKey("InitialColumnDefinition", this.InitialColumn.InitialDefinition)
                    ;
                queryBuilder.Append(createTableQuery);
            }
            foreach (DBColumn systemColumn in this.SystemColumns)
            {
                //пропускаем первичным столбец, если таблица не существует - это значит, 
                //что запрос именно с эти столбцом будет выполнен в запросе создания таблицы
                //также пропускаем остальные существтующие системные столбцы.
                if (!systemColumn.IsInitial && systemColumn.Exists ||
                    systemColumn.IsInitial && (!this.Exists || systemColumn.Exists))
                    continue;

                //добавляем запрос добавления столбца к общему запросу.
                queryBuilder.Append(systemColumn.GetInitQuery());
            }

            //добавляем системные индексы, в том числе и индекс первичного ключа, после того как будут добавлены все системные столбцы
            //первичный ключ может быть добавлен уже для существующей таблицы.
            foreach (DBIndex systemIndex in this.SystemIndexes)
            {
                //пропускаем существтующие системные индексы.
                if (systemIndex.Exists)
                    continue;

                //добавляем запрос добавления индекса к общему запросу.
                queryBuilder.Append(systemIndex.GetInitQuery());
            }

            string query = null;
            if (queryBuilder.Length > 0)
                query = queryBuilder.ToString();

            return query;
        }

        #endregion


        #region Init


        #region InitResult

        /// <summary>
        /// Представляет результат метода InitInternal.
        /// </summary>
        internal class InitResult
        {
            /// <summary>
            /// Создает экземпляр InitResult.
            /// </summary>
            internal InitResult()
            {
            }

            private bool _TableChanged;
            /// <summary>
            /// Возвращает true, если было выполнено создание таблицы.
            /// </summary>
            public bool TableChanged
            {
                get { return _TableChanged; }
                internal set { _TableChanged = value; }
            }

            private bool _ColumnsChanged;
            /// <summary>
            /// Возвращает true, если была выполнено добавление столбцов таблицы.
            /// </summary>
            public bool ColumnsChanged
            {
                get { return _ColumnsChanged; }
                internal set { _ColumnsChanged = value; }
            }

            private bool _IndexesChanged;
            /// <summary>
            /// Возвращает true, если была выполнено добавление, удаление или переименование индексов таблицы.
            /// </summary>
            public bool IndexesChanged
            {
                get { return _IndexesChanged; }
                internal set { _IndexesChanged = value; }
            }
        }

        #endregion


        private bool __init_InitRequired = false;
        private bool _InitRequired;
        /// <summary>
        /// Возвращает true, если таблица требует инициализации в случае отсутствия таблицы в базе данных, или в случае изменения состава или параметров столбцов, индексов или триггеров.
        /// </summary>
        public bool InitRequired
        {
            get
            {
                if (!__init_InitRequired)
                {
                    _InitRequired = this.InitRequiredInternal();
                    __init_InitRequired = true;
                }
                return _InitRequired;
            }
        }

        /// <summary>
        /// Возвращает true, если таблица требует инициализации в случае отсутствия таблицы в базе данных, или в случае изменения состава или параметров столбцов, индексов или триггеров.
        /// </summary>
        /// <returns></returns>
        internal virtual bool InitRequiredInternal()
        {
            //проверяем, что название таблицы не изменилось и переименование перед операцией не требуется.
            this.CheckRenameNotRequired();

            //проверяем, что префикс таблицы не изменился
            this.CheckPrefixChangeNotRequired();

            if (!this.OriginalExists)
                return true;
            else
            {
                foreach (DBColumn column in this.Columns)
                {
                    if (!column.Exists || column.NameChanged || column.UpdateRequired)
                        return true;
                }
                foreach (DBIndex index in this.Indexes)
                {
                    if (!index.Exists || index.NameChanged || index.RecreateRequired)
                        return true;
                }
                foreach (DBTrigger trigger in this.Triggers)
                {
                    if (!trigger.Exists || trigger.NameChanged || trigger.RecreateRequired || trigger.UpdateRequired)
                        return true;
                }
                foreach (DBIndexInfo indexInfo in this.OriginalExistingTable.Indexes)
                {
                    if (indexInfo.SchemaIndex == null && !indexInfo.IsCustom)
                        return true;
                }
                foreach (DBTriggerInfo triggerInfo in this.OriginalExistingTable.Triggers)
                {
                    if (triggerInfo.SchemaTrigger == null && !triggerInfo.IsCustom)
                        return true;
                }
            }
            return false;
        }

        private bool __init_RequiredInitAction = false;
        private DBTableRequiredInitAction _RequiredInitAction;
        /// <summary>
        /// Возвращает описание требуемого действия инициализации таблицы, если таблица требует инициализации в случае отсутствия таблицы в базе данных, или в случае изменения состава или параметров столбцов, индексов или триггеров.
        /// </summary>
        public DBTableRequiredInitAction RequiredInitAction
        {
            get
            {
                if (!__init_RequiredInitAction)
                {
                    _RequiredInitAction = this.RequiredInitActionInternal();
                    __init_RequiredInitAction = true;
                }
                return _RequiredInitAction;
            }
        }

        /// <summary>
        /// Возвращает описание требуемого действия инициализации таблицы, если таблица требует инициализации в случае отсутствия таблицы в базе данных, или в случае изменения состава или параметров столбцов, индексов или триггеров.
        /// </summary>
        /// <returns></returns>
        internal virtual DBTableRequiredInitAction RequiredInitActionInternal()
        {
            //проверяем, что название таблицы не изменилось и переименование перед операцией не требуется.
            this.CheckRenameNotRequired();

            //проверяем, что префикс таблицы не изменился
            this.CheckPrefixChangeNotRequired();

            if (!this.OriginalExists)
                return new DBTableRequiredInitAction(DBTableRequiredInitActionType.CreateRequired, this);
            else
            {
                foreach (DBColumn column in this.Columns)
                {
                    if (!column.Exists)
                        return new DBTableRequiredInitAction(DBTableRequiredInitActionType.CreateRequired, column, this);
                    else if (column.NameChanged)
                        return new DBTableRequiredInitAction(DBTableRequiredInitActionType.RenameRequired, column, this);
                    else if (column.UpdateRequired)
                        return new DBTableRequiredInitAction(DBTableRequiredInitActionType.UpdateRequired, column, this);
                }
                foreach (DBIndex index in this.Indexes)
                {
                    if (!index.Exists)
                        return new DBTableRequiredInitAction(DBTableRequiredInitActionType.CreateRequired, index, this);
                    else if (index.NameChanged)
                        return new DBTableRequiredInitAction(DBTableRequiredInitActionType.RenameRequired, index, this);
                    else if (index.RecreateRequired)
                        return new DBTableRequiredInitAction(DBTableRequiredInitActionType.RecreateRequired, index, this);
                }
                foreach (DBTrigger trigger in this.Triggers)
                {
                    if (!trigger.Exists)
                        return new DBTableRequiredInitAction(DBTableRequiredInitActionType.CreateRequired, trigger, this);
                    else if (trigger.NameChanged)
                        return new DBTableRequiredInitAction(DBTableRequiredInitActionType.RenameRequired, trigger, this);
                    else if (trigger.RecreateRequired)
                        return new DBTableRequiredInitAction(DBTableRequiredInitActionType.RecreateRequired, trigger, this);
                    else if (trigger.UpdateRequired)
                        return new DBTableRequiredInitAction(DBTableRequiredInitActionType.UpdateRequired, trigger, this);
                }
                foreach (DBIndexInfo indexInfo in this.OriginalExistingTable.Indexes)
                {
                    if (indexInfo.SchemaIndex == null && !indexInfo.IsCustom)
                        return new DBTableRequiredInitAction(indexInfo, this);
                }
                foreach (DBTriggerInfo triggerInfo in this.OriginalExistingTable.Triggers)
                {
                    if (triggerInfo.SchemaTrigger == null && !triggerInfo.IsCustom)
                        return new DBTableRequiredInitAction(triggerInfo, this);
                }
            }
            return null;
        }

        private bool _JustCreated;
        internal bool JustCreated
        {
            get { return _JustCreated; }
            private set { _JustCreated = value; }
        }

        /// <summary>
        /// Создает таблицу если она не существует, добавляет столбцы, изменяет параметры столбцов, переименовывает столбцы, создает индексы, 
        /// пересоздает индексы при изменении параметров индексов, переименовывает индексы, 
        /// в случае если данные операции требуются в соответствии со схемой таблицы.
        /// </summary>
        /// <param name="ensureColumns">При установленном true, применяет изменения к стобцам таблицы.</param>
        /// <param name="ensureIndexes">При установленном true, применяет изменения к индексам таблицы.</param>
        internal virtual InitResult InitInternal(bool ensureColumns, bool ensureIndexes)
        {
            this.SchemaAdapter.Logger.WriteFormatMessage("InitInternal. Таблица: '{0}' ensureColumns: {1}, ensureIndexes: {2}", this.Name, ensureColumns, ensureIndexes);

            //проверяем, что название таблицы не изменилось и переименование перед операцией не требуется.
            this.CheckRenameNotRequired();

            //проверяем, что префикс таблицы не изменился
            this.CheckPrefixChangeNotRequired();

            //запрос инициализации таблицы.
            string initQuery = null;
            bool markJustCreated = false;
            if (!this.SchemaAdapter.IsPermanentSchema)
            {
                //получаем запрос инициализации таблицы, если схема таблицы является управляемой.
                initQuery = this.GetInitTableQuery();
                if (!string.IsNullOrEmpty(initQuery) && !this.Exists)
                    markJustCreated = true;
                this.SchemaAdapter.Logger.WriteFormatMessage("InitInternal. Таблица: '{0}' Запрос на создание таблицы: {1}", this.Name, initQuery);
            }
            else
            {
                //проверяем существование таблицы, если схема таблицы является неизменной.
                this.CheckExists();
            }

            //удаляемые индексы
            Dictionary<string, bool> droppingIndexes = new Dictionary<string, bool>();
            StringBuilder stDroppingIndexesQuery = new StringBuilder();

            //создаваемый/пересоздаваемые индексы.
            Dictionary<string, bool> creatingIndexes = new Dictionary<string, bool>();
            StringBuilder stCreatingIndexesQuery = new StringBuilder();

            //создаваемые/обновляемые столбцы.
            Dictionary<string, bool> renamedColumns = new Dictionary<string, bool>();
            StringBuilder stColumnsQuery = new StringBuilder();

            //удаляемые столбцы
            StringBuilder stDropColumnsQuery = new StringBuilder();

            if (ensureColumns)
            {
                foreach (DBColumn column in this.Columns)
                {
                    if (!column.Exists)
                    {
                        //для несуществующих столбцов добавляем запросы только по несистемным столбцам, 
                        //т.к. несуществующие системные столбцы были созданы при инициализации таблицы.
                        if (!column.IsSystem)
                            stColumnsQuery.Append(column.GetInitQuery());
                    }
                    else
                    {
                        if (column.NameChanged)
                        {
                            //перименовываем столбец при необходимости.
                            stColumnsQuery.Append(column.GetRenameQuery());
                            if (!renamedColumns.ContainsKey(column.ExistingColumn.Name))
                                renamedColumns.Add(column.ExistingColumn.Name, true);
                        }
                        if (column.UpdateRequired)
                        {
                            stColumnsQuery.Append(column.GetUpdateQuery());

                            //формируем запросы удаления и пересоздания индексов, если изменения столбца требуют пересоздания индексов по этому столбцу.
                            if (column.DependendIndexesRecreateRequired)
                            {
                                foreach (DBIndexInfo indexInfo in column.ExistingColumn.DependendIndexes)
                                {
                                    if (!droppingIndexes.ContainsKey(indexInfo.NameLow))
                                    {
                                        stDroppingIndexesQuery.Append(indexInfo.GetDropQuery());
                                        droppingIndexes.Add(indexInfo.NameLow, true);
                                    }
                                    if (indexInfo.SchemaIndex != null)
                                    {
                                        if (!creatingIndexes.ContainsKey(indexInfo.SchemaIndex.NameLow))
                                        {
                                            stCreatingIndexesQuery.Append(indexInfo.SchemaIndex.GetInitQuery());
                                            creatingIndexes.Add(indexInfo.SchemaIndex.NameLow, true);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                //если таблица не создана только что, удаляем столбцы не соответствующие схеме
                if (this.ExistingTable != null && !this.SchemaAdapter.IsPermanentSchema)
                {
                    foreach (DBColumnInfo columnInfo in this.ExistingTable.Columns)
                    {
                        //если столбец отсутствует в текущей схеме и не был переименован, удаляем его
                        if (!this.ContainsColumn(columnInfo.Name) && !renamedColumns.ContainsKey(columnInfo.Name))
                        {
                            bool hasCustomIndexes = false;

                            //проверяем остались ли на столбец кастомные индексы
                            foreach (DBIndexInfo indexInfo in columnInfo.DependendIndexes)
                            {
                                if (indexInfo.IsCustom)
                                {
                                    hasCustomIndexes = true;
                                    break;
                                }
                            }

                            //не удаляем столбец имеющий кастомные индексы.
                            if (!hasCustomIndexes)
                            {
                                stDropColumnsQuery.Append(columnInfo.GetDropQuery());
                                ensureIndexes = true;
                            }
                        }
                    }
                }
            }

            //запрос переименования индексов.
            StringBuilder stRenamingIndexesQuery = new StringBuilder();

            if (ensureIndexes)
            {
                //создаваемые/обновляемые индексы
                foreach (DBIndex index in this.Indexes)
                {
                    if (!index.Exists)
                    {
                        //если индекс не существует, добавляем запрос на создание индекса.
                        if (!creatingIndexes.ContainsKey(index.NameLow))
                        {
                            stCreatingIndexesQuery.Append(index.GetInitQuery());
                            creatingIndexes.Add(index.NameLow, true);
                        }
                    }
                    else
                    {
                        //если индекс существует и требует пересоздания, то пересоздаем его.
                        if (index.RecreateRequired)
                        {
                            if (!droppingIndexes.ContainsKey(index.ExistingIndex.NameLow))
                            {
                                stDroppingIndexesQuery.Append(index.ExistingIndex.GetDropQuery());
                                droppingIndexes.Add(index.ExistingIndex.NameLow, true);
                            }
                            if (!creatingIndexes.ContainsKey(index.NameLow))
                            {
                                stCreatingIndexesQuery.Append(index.GetInitQuery());
                                creatingIndexes.Add(index.NameLow, true);
                            }
                        }

                        //если индекс существует и требует переименования, то переименовываем его.
                        //при этом индекс не должен попасть в удаляемые и создаваемые индексы.
                        if (index.NameChanged &&
                            !droppingIndexes.ContainsKey(index.ExistingIndex.NameLow) &&
                            !creatingIndexes.ContainsKey(index.NameLow))
                        {
                            stRenamingIndexesQuery.Append(index.GetRenameQuery());
                        }
                    }
                }

                //удаляем индексы не соответствующие схеме индексов.
                if (this.ExistingTable != null && !this.SchemaAdapter.IsPermanentSchema)
                {
                    foreach (DBIndexInfo indexInfo in this.ExistingTable.Indexes)
                    {
                        if (indexInfo.SchemaIndex == null && !indexInfo.IsCustom)
                        {
                            if (!droppingIndexes.ContainsKey(indexInfo.NameLow))
                            {
                                stDroppingIndexesQuery.Append(indexInfo.GetDropQuery());
                                droppingIndexes.Add(indexInfo.NameLow, true);
                            }
                        }
                    }
                }
            }

            this.SchemaAdapter.Logger.WriteFormatMessage("InitInternal. Начало работы с БД. Таблица: '{0}' ensureColumns: {1}, ensureIndexes: {2}", this.Name, ensureColumns, ensureIndexes);

            InitResult result = new InitResult();

            //инициализируем таблицу
            if (!string.IsNullOrEmpty(initQuery))
            {
                this.SchemaAdapter.Logger.WriteFormatMessage("InitInternal. инициализируем таблицу. Таблица: '{0}' Запрос: {1}", this.Name, initQuery);

                this.DataAdapter.ExecuteQuery(initQuery);
                result.TableChanged = true;

                if (markJustCreated)
                    this.JustCreated = true;
            }

            //удаляем индексы
            if (stDroppingIndexesQuery.Length > 0)
            {
                this.SchemaAdapter.Logger.WriteFormatMessage("InitInternal. Удаление индексов. Таблица: '{0}' Запрос: {1}", this.Name, stDroppingIndexesQuery.ToString());
                this.DataAdapter.ExecuteQuery(stDroppingIndexesQuery.ToString());
                result.IndexesChanged = true;
            }

            //создаем/изменяем столбцы
            if (stColumnsQuery.Length > 0)
            {
                this.SchemaAdapter.Logger.WriteFormatMessage("InitInternal. создаем/изменяем столбцы. Таблица: '{0}' Запрос: {1}", this.Name, stColumnsQuery.ToString());

                //проверяем, что схема таблицы является управляемой.
                this.SchemaAdapter.CheckManagedSchema();

                this.DataAdapter.ExecuteQuery(stColumnsQuery.ToString());
                result.ColumnsChanged = true;
            }

            //удаляем ненужные столбцы
            if (stDropColumnsQuery.Length > 0)
            {
                this.SchemaAdapter.Logger.WriteFormatMessage("InitInternal. удаляем ненужные столбцы. Таблица: '{0}' Запрос: {1}", this.Name, stDropColumnsQuery.ToString());

                //проверяем, что схема таблицы является управляемой.
                this.SchemaAdapter.CheckManagedSchema();

                this.DataAdapter.ExecuteQuery(stDropColumnsQuery.ToString());
                result.ColumnsChanged = true;
            }

            //создаем/пересоздаем индексы
            if (stCreatingIndexesQuery.Length > 0)
            {
                this.SchemaAdapter.Logger.WriteFormatMessage("InitInternal. создаем/пересоздаем индексы Таблица: '{0}' Запрос: {1}", this.Name, stCreatingIndexesQuery.ToString());

                this.DataAdapter.ExecuteQuery(stCreatingIndexesQuery.ToString());
                result.IndexesChanged = true;
            }

            //переименовываем индексы
            if (stRenamingIndexesQuery.Length > 0)
            {
                this.SchemaAdapter.Logger.WriteFormatMessage("InitInternal. переименовываем индексы Таблица: '{0}' Запрос: {1}", this.Name, stRenamingIndexesQuery.ToString());

                this.DataAdapter.ExecuteQuery(stRenamingIndexesQuery.ToString());
                result.IndexesChanged = true;
            }

            this.SchemaAdapter.Logger.WriteFormatMessage("InitInternal. сбрасываем инициализацию существующей таблицы/столбцов/индексов Таблица: '{0}' ", this.Name);
            //сбрасываем инициализацию существующей таблицы/столбцов/индексов
            if (result.TableChanged)
                this.ResetExistingTable();
            else
            {
                if (result.ColumnsChanged)
                    this.ResetExistingColumns();
                if (result.IndexesChanged)
                    this.ResetExistingIndexes();
            }

            this.SchemaAdapter.Logger.WriteFormatMessage("InitInternal. Завершено. Таблица: '{0}' ensureColumns: {1}, ensureIndexes: {2}", this.Name, ensureColumns, ensureIndexes);

            //возвращаем результат.
            return result;
        }

        #endregion


        #region InitTriggers

        /// <summary>
        /// Инициализирует триггеры таблицы.
        /// </summary>
        internal virtual void InitTriggersInternal()
        {
            this.InitTriggersSealed();
        }

        /// <summary>
        /// Инициализирует триггеры таблицы.
        /// </summary>
        private void InitTriggersSealed()
        {
            //проверяем существование таблицы
            this.CheckExists();

            //формируем запросы создания/обновления триггеров
            StringBuilder query = new StringBuilder();
            foreach (DBTrigger trigger in this.Triggers)
            {
                if (!trigger.Exists)
                    query.Append(trigger.GetCreateQuery());
                else
                {
                    if (trigger.RecreateRequired)
                        query.Append(trigger.GetRecreateQuery());
                    else if (trigger.UpdateRequired)
                        query.Append(trigger.GetUpdateQuery());
                }
            }

            //формируем запросы удаления неактуальных триггеров, несоответствующие схеме.
            if (this.ExistingTable != null && !this.SchemaAdapter.IsPermanentSchema)
            {
                foreach (DBTriggerInfo triggerInfo in this.ExistingTable.Triggers)
                {
                    if (triggerInfo.SchemaTrigger == null && !triggerInfo.IsCustom)
                        query.Append(triggerInfo.GetDropQuery());
                }
            }

            //если запрос был сформирован, выполняем его, сбрасываем существующие триггеры
            if (query.Length > 0)
            {
                //выполняем запрос
                this.DataAdapter.ExecuteQuery(query.ToString());

                //сбрасываем существующие триггеры
                this.ResetExistingTriggers();
            }
        }

        #endregion


        #region Delete

        /// <summary>
        /// Удаляет таблицу из базы данных.
        /// </summary>
        internal virtual void DeleteInternal()
        {
            //удаляем таблицу.
            this.DeleteSealed();
        }

        internal void DeleteSealed()
        {
            //проверяем, что название таблицы не изменилось и переименование перед операцией не требуется.
            this.CheckRenameNotRequired();

            //проверяем существование таблицы.
            this.CheckExists();

            //формируем запрос удаления таблицы.
            string deleteTableQuery = this.ExistingTable.GetDropQuery();

            //удаляем таблицу из БД
            this.DataAdapter.ExecuteQuery(deleteTableQuery);

            //сбрасываем флаги инициализации
            this.ResetExistingTable();
        }

        #endregion


        #region Rename

        /// <summary>
        /// Переименовывает таблицу.
        /// </summary>
        internal virtual void RenameInternal()
        {
            //проверяем необходимость переименования.
            this.CheckRenameRequired();

            //проверяем существование таблицы с исходным названием.
            this.CheckOriginalExists();

            //если префикс таблицы был изменен переименовываем индексы.
            //выполняем это перед переименованием самой таблицы иначе если сделать это после,
            //то коллекции индексов окажутся пустыми и будет пытаться произвестить переименование таблицы со старым именем,
            //которой уже нет в базе данных
            if (this.PrefixChanged)
                this.RenameIndexesOnRename();

            //инициализируем коллекцию существующих триггеров, чтобы она не оказалась пустой при инициализации после переименования таблицы.
            object checkObj = this.OriginalExistingTable.Triggers;

            //выполняем переименование.
            string query = @"
exec sp_rename N'[dbo].[{OriginalNameText}]', N'{NameText}'"
                .ReplaceKey("OriginalNameText", this.OriginalName.QueryEncode())
                .ReplaceKey("NameText", this.Name.QueryEncode())
                ;
            this.DataAdapter.ExecuteQuery(query);

            //добавляем название таблицы в коллекцию переименованных, 
            //поскольку метаданные такой таблицы уже не были получены общим запросом, если использовался контекст IsSummaryTablesMetadataContext.
            this.Connection.TableInfoAdapter.AddRenamedTable(this.Name);

            //пересоздаем триггеры
            this.RecreateTriggersOnRename();

            //сбрасываем параметры существующей таблицы
            this.ResetExistingTable();
        }

        /// <summary>
        /// Переименовывает индексы и триггеры в соответствии с новым префиксом.
        /// </summary>
        internal virtual void ChangePrefixInternal()
        {
            //проверяем, что таблица не требует переименования
            this.CheckRenameNotRequired();

            //проверяем, что требуется переименование префикса таблицы.
            this.CheckPrefixChangeRequired();

            StringBuilder query = new StringBuilder();

            //формируем запросы переименования индексов.
            foreach (DBIndex index in this.Indexes)
            {
                //добавляем запрос переименования индекса.
                if (index.NameChanged)
                    query.Append(index.GetRenameQuery());
            }

            //формируем запросы пересоздания триггеров с новыми названиями.
            foreach (DBTrigger trigger in this.Triggers)
            {
                //добавляем запрос пересоздания триггера с новым названием.
                if (trigger.RecreateRequired)
                    query.Append(trigger.GetRecreateQuery());
            }

            //выполняем запросы переименования индексов и пересоздания триггеров.
            if (query.Length > 0)
                this.DataAdapter.ExecuteQuery(query.ToString());

            //сбрасываем параметры существующих индексов и триггеров
            this.ResetExistingIndexes();
            this.ResetExistingTriggers();
        }

        #region RenameDependendObjects

        /// <summary>
        /// Переименовывает индексы при переименовании префикса таблицы и переименовании самой таблицы.
        /// </summary>
        private void RenameIndexesOnRename()
        {
            //проверяем, что требуется переименование таблицы.
            this.CheckRenameRequired();

            //проверяем, что требуется переименование префикса таблицы.
            this.CheckPrefixChangeRequired();

            StringBuilder query = new StringBuilder();

            //формируем запросы переименования индексов.
            foreach (DBIndex index in this.Indexes)
            {
                //добавляем запрос переименования индекса.
                if (index.NameChanged)
                    query.Append(index.GetRenameQuery());
            }

            //выполняем запросы переименования индексов.
            if (query.Length > 0)
                this.DataAdapter.ExecuteQuery(query.ToString());
        }

        /// <summary>
        /// Пересоздает триггеры таблицы при переименовании таблицы.
        /// </summary>
        private void RecreateTriggersOnRename()
        {
            //проверяем, что требуется переименование таблицы.
            this.CheckRenameRequired();

            StringBuilder query = new StringBuilder();

            //формируем запросы пересоздания триггеров, в теле которых может фигурировать старое название таблицы.
            foreach (DBTrigger trigger in this.Triggers)
            {
                //добавляем запрос пересоздания триггера, использующего старые названия таблиц.
                //триггер нужно проверять на существование, т.к. триггер может не существовать, 
                //если переименование производится вследствие изменение статуса архивности, и тогда нужно триггеры создавать только в конце.
                if (trigger.Exists)
                    query.Append(trigger.GetRecreateQuery());
            }

            //выполняем запросы пересоздания триггеров.
            if (query.Length > 0)
                this.DataAdapter.ExecuteQuery(query.ToString());
        }



        #endregion


        private bool __init_RenameRequired = false;
        private bool _RenameRequired;
        /// <summary>
        /// Возвращает true, если название таблицы было изменено в контексте выполнения кода
        /// и в базе данных существует таблица со старым названием.
        /// </summary>
        internal bool RenameRequired
        {
            get
            {
                if (!__init_RenameRequired)
                {
                    _RenameRequired = this.NameChanged && this.OriginalExists;
                    __init_RenameRequired = true;
                }
                return _RenameRequired;
            }
        }

        /// <summary>
        /// Проверяет, что таблица не была переименована в контексте выполнения кода, в ином случае - генерирует исключение.
        /// </summary>
        internal void CheckRenameNotRequired()
        {
            if (this.RenameRequired)
                throw new Exception(string.Format("Операция недоступна, поскольку таблица {0} должна быть переименована в {1}.", this.OriginalName, this.Name));
        }

        /// <summary>
        /// Проверяет, что таблица была переименована в контексте выполнения кода, в ином случае - генерирует исключение.
        /// </summary>
        internal void CheckRenameRequired()
        {
            if (!this.NameChanged)
                throw new Exception(string.Format("Операция недоступна, поскольку название таблицы {0} не было изменено.", this.Name));
        }

        #endregion


        /// <summary>
        /// Удаляет данные из таблицы в соответствии с заданным условием.
        /// </summary>
        /// <param name="deleteCondition">Условие удаления данных.</param>
        internal virtual void DeleteDataInternal(string deleteCondition)
        {
            if (string.IsNullOrEmpty(deleteCondition))
                throw new ArgumentNullException("deleteCondition");

            //проверяем существование таблицы
            this.CheckExists();

            //формируем запрос удаления данных и выполняем его.
            string deleteQuery = string.Format("DELETE FROM [{0}] WHERE {1}", this.Name, deleteCondition);
            this.DataAdapter.ExecuteQuery(deleteQuery);
        }

        /// <summary>
        /// Очищает все данные таблицы, сбрасывая автоинкремент на 0.
        /// </summary>
        internal virtual void TruncateDataInternal()
        {
            //проверяем существование таблицы
            this.CheckExists();

            //формируем запрос очистки данных и выполняем его.
            string deleteQuery = string.Format("TRUNCATE TABLE [{0}]", this.Name);
            this.DataAdapter.ExecuteQuery(deleteQuery);
        }

        /// <summary>
        /// Возвращает имя таблицы для использования в запросах в качестве источника выборки.
        /// Если сравниваемое подключение отсутствует, то название таблицы возращается в формате [dbo].[Имя таблицы].
        /// Если база данных подключения queryConnection совпадает с базой данных в которой хранится таблица, то название таблицы возращается в формате [dbo].[Имя таблицы].
        /// Если база данных подключения queryConnection не совпадает с базой данных в которой хранится таблица, но совпадает сервер баз данных, то название таблицы возращается в формате [Имя базы данных].[dbo].[Имя таблицы].
        /// Если сервер базы данных подключения queryConnection не совпадает с сервером базы данных в которой хранится таблица, то название таблицы возращается в формате [Имя сервера базы данных].[Имя базы данных].[dbo].[Имя таблицы].
        /// </summary>
        /// <param name="queryConnection">Подключение к базе данных, в контексте которого выполняется запрос к таблице.</param>
        /// <returns></returns>
        public string GetQueryName(DBConnection queryConnection)
        {
            string queryName = this.Connection.GetQueryObjectName(this.Name, queryConnection);
            return queryName;
        }

        /// <summary>
        /// Возвращает имя таблицы для использования в запросах в качестве источника выборки.
        /// Если сравниваемое подключение отсутствует, то название таблицы возращается в формате [dbo].[Имя таблицы].
        /// Если база данных подключения queryConnection совпадает с базой данных в которой хранится таблица, то название таблицы возращается в формате [dbo].[Имя таблицы].
        /// Если база данных подключения queryConnection не совпадает с базой данных в которой хранится таблица, но совпадает сервер баз данных, то название таблицы возращается в формате [Имя базы данных].[dbo].[Имя таблицы].
        /// Если сервер базы данных подключения queryConnection не совпадает с сервером базы данных в которой хранится таблица, то название таблицы возращается в формате [Имя сервера базы данных].[Имя базы данных].[dbo].[Имя таблицы].
        /// </summary>
        /// <param name="tableAlias">Альтернативное название таблицы.</param>
        /// <param name="queryConnection">Подключение к базе данных, в контексте которого выполняется запрос к таблице.</param>
        /// <returns></returns>
        public string GetQueryName(string tableAlias, DBConnection queryConnection)
        {
            string queryName = this.GetQueryName(queryConnection);
            if (!string.IsNullOrEmpty(tableAlias))
                queryName = string.Format("{0} AS [{1}]", queryName, tableAlias);
            return queryName;
        }

        /// <summary>
        /// Строковое представление таблицы.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(this.Name))
                return this.Name;
            return base.ToString();
        }
    }
}
