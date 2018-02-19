using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Storage.Metadata.MSSQL
{
    /// <summary>
    /// Представляет класс, содержащий параметры существующей таблицы.
    /// </summary>
    public class DBTableInfo
    {
        /// <summary>
        /// Создает экземпляр DBTableInfo.
        /// </summary>
        /// <param name="data">Метаданные таблицы.</param>
        /// <param name="schemaTable">Таблица, соответствующая схеме, для которой получена данная существующая таблица.</param>
        internal DBTableInfo(string key, DataRow data, DBTable schemaTable)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException("key");
            if (data == null)
                throw new ArgumentNullException("data");
            if (schemaTable == null)
                throw new ArgumentNullException("schemaTable");

            this.Key = key;
            this.Data = data;
            this.SchemaTable = schemaTable;
        }

        private string _Key;
        /// <summary>
        /// Ключ названия таблицы в модели таблиц. Используется для проверки уникальности при получении метаданных.
        /// </summary>
        public string Key
        {
            get { return _Key; }
            private set { _Key = value; }
        }

        private DataRow _Data;
        /// <summary>
        /// Метаданные таблицы.
        /// </summary>
        private DataRow Data
        {
            get { return _Data; }
            set { _Data = value; }
        }

        private DBTable _SchemaTable;
        /// <summary>
        /// Таблица, соответствующая схеме, для которой получена данная существующая таблица.
        /// </summary>
        internal DBTable SchemaTable
        {
            get { return _SchemaTable; }
            private set { _SchemaTable = value; }
        }

        private bool __init_Reader = false;
        private DataRowReader _Reader;
        private DataRowReader Reader
        {
            get
            {
                if (!__init_Reader)
                {
                    _Reader = new DataRowReader(this.Data);
                    __init_Reader = true;
                }
                return _Reader;
            }
        }

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
                    _Name = this.Reader.GetStringValue("Name");
                    if (string.IsNullOrEmpty(_Name))
                        throw new Exception("Не удалось получить название таблицы.");
                    __init_Name = true;
                }
                return _Name;
            }
        }

        private bool __init_NameLow = false;
        private string _NameLow;
        /// <summary>
        /// Название таблицы в нижнем регистре.
        /// </summary>
        internal string NameLow
        {
            get
            {
                if (!__init_NameLow)
                {
                    if (!string.IsNullOrEmpty(this.Name))
                        _NameLow = this.Name.ToLower();
                    __init_NameLow = true;
                }
                return _NameLow;
            }
        }


        #region Columns

        /// <summary>
        /// Сбрасывает инициализацию коллекции столбцов таблицы.
        /// </summary>
        internal void ResetColumns()
        {
            this.__init_Columns = false;
            this.__init_ColumnsByName = false;
            this.__init_IdentityColumn = false;
        }

        private bool __init_IdentityColumn = false;
        private DBColumnInfo _IdentityColumn;
        /// <summary>
        /// Ключевой инкрементный столбец-идентификатор таблицы.
        /// </summary>
        public DBColumnInfo IdentityColumn
        {
            get
            {
                if (!__init_IdentityColumn)
                {
                    foreach (DBColumnInfo column in this.Columns)
                    {
                        if (column.IsIdentity)
                        {
                            _IdentityColumn = column;
                            break;
                        }
                    }
                    __init_IdentityColumn = true;
                }
                return _IdentityColumn;
            }
        }

        private bool __init_Columns = false;
        private DBCollection<DBColumnInfo> _Columns;
        /// <summary>
        /// Коллекция существующих столбцов таблицы.
        /// </summary>
        public DBCollection<DBColumnInfo> Columns
        {
            get
            {
                if (!__init_Columns)
                {
                    _Columns = new DBCollection<DBColumnInfo>(this.ColumnsByName.Values);
                    __init_Columns = true;
                }
                return _Columns;
            }
        }

        private bool __init_ColumnsByName = false;
        private Dictionary<string, DBColumnInfo> _ColumnsByName;
        /// <summary>
        /// Существующие столбцы таблицы.
        /// </summary>
        private Dictionary<string, DBColumnInfo> ColumnsByName
        {
            get
            {
                if (!__init_ColumnsByName)
                {
                    _ColumnsByName = new Dictionary<string, DBColumnInfo>();

                    ICollection columnsData = this.SchemaTable.Connection.TableInfoAdapter.GetColumnsData(this.Key, this.Name);
                    foreach (DataRow columnInfo in columnsData)
                    {
                        DBColumnInfo column = new DBColumnInfo(columnInfo, this);
                        if (!_ColumnsByName.ContainsKey(column.NameLow))
                            _ColumnsByName.Add(column.NameLow, column);
                    }

                    __init_ColumnsByName = true;
                }
                return _ColumnsByName;
            }
        }

        /// <summary>
        /// Возвращает true, если таблица содержит столбец с заданным названием.
        /// </summary>
        /// <param name="columnName">Название столбца.</param>
        /// <returns></returns>
        public bool ContainsColumn(string columnName)
        {
            if (string.IsNullOrEmpty(columnName))
                throw new ArgumentNullException("columnName");

            //проверяем наличие существующего столбца в таблице.
            bool result = this.ColumnsByName.ContainsKey(columnName.ToLower());
            return result;
        }

        /// <summary>
        /// Возвращает существующий столбец таблицы.
        /// </summary>
        /// <param name="columnName">Название столбца.</param>
        /// <returns></returns>
        public DBColumnInfo GetColumn(string columnName)
        {
            return this.GetColumn(columnName, false);
        }


        /// <summary>
        /// Возвращает существующий столбец таблицы.
        /// </summary>
        /// <param name="columnName">Название столбца.</param>
        /// <param name="throwNotFoundException">При переданном значение true генерирует исключение в случае отсутствия столбца в таблице.</param>
        /// <returns></returns>
        public DBColumnInfo GetColumn(string columnName, bool throwNotFoundException)
        {
            if (string.IsNullOrEmpty(columnName))
                throw new ArgumentNullException("columnName");

            //получаем столбец
            string columnNameLow = columnName.ToLower();
            DBColumnInfo column = null;
            if (this.ColumnsByName.ContainsKey(columnNameLow))
                column = this.ColumnsByName[columnNameLow];

            if (column == null && throwNotFoundException)
                throw new Exception(string.Format("Не удалось получить существующий столбец [{0}] в таблице {1}.", columnName, this.Name));

            return column;
        }

        #endregion


        #region Indexes

        /// <summary>
        /// Сбрасывает инициализацию коллекции индексов таблицы.
        /// </summary>
        internal void ResetIndexes()
        {
            this.__init_Indexes = false;
            this.__init_IndexesByName = false;
            this.__init_PrimaryKey = false;            
        }

        private bool __init_PrimaryKey = false;
        private DBIndexInfo _PrimaryKey;
        /// <summary>
        /// Индекс первичного ключа таблицы.
        /// </summary>
        public DBIndexInfo PrimaryKey
        {
            get
            {
                if (!__init_PrimaryKey)
                {
                    foreach (DBIndexInfo index in this.Indexes)
                    {
                        if (index.IsPrimaryKey)
                        {
                            _PrimaryKey = index;
                            break;
                        }
                    }
                    __init_PrimaryKey = true;
                }
                return _PrimaryKey;
            }
        }

        private bool __init_Indexes = false;
        private DBCollection<DBIndexInfo> _Indexes;
        /// <summary>
        /// Коллекция существующих индексов таблицы.
        /// </summary>
        public DBCollection<DBIndexInfo> Indexes
        {
            get
            {
                if (!__init_Indexes)
                {
                    _Indexes = new DBCollection<DBIndexInfo>(this.IndexesByName.Values);
                    __init_Indexes = true;
                }
                return _Indexes;
            }
        }


        private bool __init_IndexesByName = false;
        private Dictionary<string, DBIndexInfo> _IndexesByName;
        /// <summary>
        /// Существующие индексы таблицы.
        /// </summary>
        private Dictionary<string, DBIndexInfo> IndexesByName
        {
            get
            {
                if (!__init_IndexesByName)
                {
                    _IndexesByName = new Dictionary<string, DBIndexInfo>();

                    ICollection indexesData = this.SchemaTable.Connection.TableInfoAdapter.GetIndexesData(this.Key, this.Name);
                    foreach (DataRow indexInfo in indexesData)
                    {
                        DBIndexInfo index = new DBIndexInfo(indexInfo, this);
                        if (!_IndexesByName.ContainsKey(index.NameLow))
                            _IndexesByName.Add(index.NameLow, index);
                    }

                    ICollection indexColumnsData = this.SchemaTable.Connection.TableInfoAdapter.GetIndexColumnsData(this.Key, this.Name);
                    foreach (DataRow indexColumnInfo in indexColumnsData)
                    {
                        string indexName = DataRowReader.GetStringValue(indexColumnInfo, "IndexName");
                        if (string.IsNullOrEmpty(indexName))
                            throw new Exception("Не задано имя индекса для столбца индекса.");
                        string indexColumnName = DataRowReader.GetStringValue(indexColumnInfo, "ColumnName");

                        string indexNameLow = indexName.ToLower();
                        if (!_IndexesByName.ContainsKey(indexNameLow))
                        {
                            StringBuilder stExistingIndexes = new StringBuilder();
                            foreach (DBIndexInfo existingIndex in _IndexesByName.Values)
                                stExistingIndexes.AppendFormat("{0}; ", existingIndex.Name);
                            throw new Exception(string.Format("Не удалось получить информацию и существующем индексе {0} при добавлении столбца {1} в коллекцию столбцов данного индекса. Перечень существующих индексов таблицы {2}: {3}", 
                                indexName, indexColumnName, this.Name, stExistingIndexes));
                        }
                        DBIndexInfo index = _IndexesByName[indexNameLow];

                        DBIndexColumnInfo indexColumn = new DBIndexColumnInfo(indexColumnInfo, index);
                        if (!index.InitializingColumns.ContainsKey(indexColumn.NameLow))
                            index.InitializingColumns.Add(indexColumn.NameLow, indexColumn);
                        else
                            throw new Exception(string.Format("Обнаружено дублирование столбца {0} в индексе {1}.", indexColumn.Name, index.Name));
                    }

                    __init_IndexesByName = true;
                }
                return _IndexesByName;
            }
        }

        /// <summary>
        /// Возвращает true, если таблица содержит индекс с заданным названием.
        /// </summary>
        /// <param name="indexName">Название индекса.</param>
        /// <returns></returns>
        public bool ContainsIndex(string indexName)
        {
            if (string.IsNullOrEmpty(indexName))
                throw new ArgumentNullException("indexName");

            //проверяем наличие существующего индекса в таблице.
            bool result = this.IndexesByName.ContainsKey(indexName.ToLower());
            return result;
        }

        /// <summary>
        /// Возвращает существующий индекс таблицы.
        /// </summary>
        /// <param name="indexName">Название индекса.</param>
        /// <returns></returns>
        public DBIndexInfo GetIndex(string indexName)
        {
            return this.GetIndex(indexName, false);
        }

        /// <summary>
        /// Возвращает существующий индекс таблицы.
        /// </summary>
        /// <param name="indexName">Название индекса.</param>
        /// <param name="throwNotFoundException">При переданном значение true генерирует исключение в случае отсутствия индекса в таблице.</param>
        /// <returns></returns>
        public DBIndexInfo GetIndex(string indexName, bool throwNotFoundException)
        {
            if (string.IsNullOrEmpty(indexName))
                throw new ArgumentNullException("indexName");

            //получаем индекс
            string indexNameLow = indexName.ToLower();
            DBIndexInfo index = null;
            if (this.IndexesByName.ContainsKey(indexNameLow))
                index = this.IndexesByName[indexNameLow];

            if (index == null && throwNotFoundException)
                throw new Exception(string.Format("Не удалось получить существующий индекс [{0}] в таблице {1}.", indexName, this.Name));

            return index;
        }

        #endregion


        #region Triggers

        /// <summary>
        /// Сбрасывает инициализацию коллекции триггеров таблицы.
        /// </summary>
        internal void ResetTriggers()
        {
            this.__init_Triggers = false;
            this.__init_TriggersByName = false;
        }

        private bool __init_Triggers = false;
        private DBCollection<DBTriggerInfo> _Triggers;
        /// <summary>
        /// Коллекция существующих триггеров таблицы.
        /// </summary>
        public DBCollection<DBTriggerInfo> Triggers
        {
            get
            {
                if (!__init_Triggers)
                {
                    _Triggers = new DBCollection<DBTriggerInfo>(this.TriggersByName.Values);
                    __init_Triggers = true;
                }
                return _Triggers;
            }
        }


        private bool __init_TriggersByName = false;
        private Dictionary<string, DBTriggerInfo> _TriggersByName;
        /// <summary>
        /// Существующие триггеры таблицы.
        /// </summary>
        private Dictionary<string, DBTriggerInfo> TriggersByName
        {
            get
            {
                if (!__init_TriggersByName)
                {
                    _TriggersByName = new Dictionary<string, DBTriggerInfo>();

                    ICollection triggersData = this.SchemaTable.Connection.TableInfoAdapter.GetTriggersData(this.Key, this.Name);
                    foreach (DataRow triggerInfo in triggersData)
                    {
                        DBTriggerInfo trigger = new DBTriggerInfo(triggerInfo, this);
                        if (!_TriggersByName.ContainsKey(trigger.NameLow))
                            _TriggersByName.Add(trigger.NameLow, trigger);
                    }

                    ICollection triggerEventsData = this.SchemaTable.Connection.TableInfoAdapter.GetTriggerEventsData(this.Key, this.Name);
                    foreach (DataRow triggerEventInfo in triggerEventsData)
                    {
                        string triggerName = DataRowReader.GetStringValue(triggerEventInfo, "TriggerName");
                        if (string.IsNullOrEmpty(triggerName))
                            throw new Exception("Не задано имя триггера для события триггера.");

                        string triggerNameLow = triggerName.ToLower();
                        if (!_TriggersByName.ContainsKey(triggerNameLow))
                            throw new Exception(string.Format("Не удалось получить экземпляр триггера {0} для события.", triggerName));
                        DBTriggerInfo trigger = _TriggersByName[triggerNameLow];

                        int eventType = DataRowReader.GetIntegerValue(triggerEventInfo, "Type");
                        if (eventType < 1 || eventType > 3)
                            throw new Exception(string.Format("Тип события {0} триггера {1} должен быть в интервале от 1 до 3.", eventType, triggerName));
                        if (!trigger.InitializingEvents.ContainsKey(eventType))
                            trigger.InitializingEvents.Add(eventType, true);
                        else
                            throw new Exception(string.Format("Обнаружено дублирование события {0} в триггере {1}.", eventType, trigger.Name));
                    }

                    __init_TriggersByName = true;
                }
                return _TriggersByName;
            }
        }

        /// <summary>
        /// Возвращает true, если таблица содержит триггер с заданным названием.
        /// </summary>
        /// <param name="triggerName">Название триггера.</param>
        /// <returns></returns>
        public bool ContainsTrigger(string triggerName)
        {
            if (string.IsNullOrEmpty(triggerName))
                throw new ArgumentNullException("triggerName");

            //проверяем наличие существующего триггера в таблице.
            bool result = this.TriggersByName.ContainsKey(triggerName.ToLower());
            return result;
        }

        /// <summary>
        /// Возвращает существующий триггер таблицы.
        /// </summary>
        /// <param name="triggerName">Название триггера.</param>
        /// <returns></returns>
        public DBTriggerInfo GetTrigger(string triggerName)
        {
            if (string.IsNullOrEmpty(triggerName))
                throw new ArgumentNullException("triggerName");

            //получаем триггер
            string triggerNameLow = triggerName.ToLower();
            DBTriggerInfo trigger = null;
            if (this.TriggersByName.ContainsKey(triggerNameLow))
                trigger = this.TriggersByName[triggerNameLow];

            return trigger;
        }

        #endregion


        /// <summary>
        /// Возвращает текст запроса удаления таблицы.
        /// </summary>
        /// <returns></returns>
        internal string GetDropQuery()
        {
            //формируем запрос удаления таблицы.
            string query = @"
IF(EXISTS(
	SELECT name FROM sys.tables WITH(NOLOCK)
	WHERE name = N'{TableNameText}'))
BEGIN
	DROP TABLE [dbo].[{TableName}]
END"
                    .ReplaceKey("TableNameText", this.Name.QueryEncode())
                    .ReplaceKey("TableName", this.Name)
                    ;
            return query;
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
