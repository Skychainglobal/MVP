using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Storage.Metadata.MSSQL
{
    /// <summary>
    /// Предоставляет методы получения данных таблиц, столбцов, индексов и триггеров базы данных.
    /// </summary>
    internal class DBTableInfoAdapter
    {
        internal DBTableInfoAdapter(DBConnection connection)
        {
            if (connection == null)
                throw new ArgumentNullException("connection");

            this.Connection = connection;
        }

        private DBConnection _Connection;
        /// <summary>
        /// Подключение к базе данных.
        /// </summary>
        public DBConnection Connection
        {
            get { return _Connection; }
            private set { _Connection = value; }
        }

        private bool __init_DataAdapter = false;
        private DBAdapter _DataAdapter;
        /// <summary>
        /// Адаптер работы с данными базы данных, соответствующей подключению.
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
                    //TODO [AO] Timeout
                    _DataAdapter.CommandTimeout = 72000;

                    __init_DataAdapter = true;
                }
                return _DataAdapter;
            }
        }

        private Dictionary<string, bool> _RenamedTables = new Dictionary<string, bool>();
        internal void AddRenamedTable(string renamedTableName)
        {
            if (string.IsNullOrEmpty(renamedTableName))
                throw new ArgumentNullException("renamedTableName");

            if (!this.Connection.Context.IsSummaryTablesMetadataContext)
                return;

            if (!_RenamedTables.ContainsKey(renamedTableName.ToLower()))
                _RenamedTables.Add(renamedTableName.ToLower(), true);
        }

        private bool IsRenamedTable(string tableName)
        {
            if (string.IsNullOrEmpty(tableName))
                throw new ArgumentNullException("tableName");

            bool isRenamed = _RenamedTables.ContainsKey(tableName.ToLower());
            return isRenamed;
        }

        private bool __init_TablesData = false;
        private Dictionary<string, DataRow> _TablesData;
        private Dictionary<string, bool> __extracted_TablesData = new Dictionary<string, bool>();
        /// <summary>
        /// Возвращает метаданные существующей в базе данных таблицы.
        /// </summary>
        /// <param name="tableName">Название таблицы.</param>
        /// <returns></returns>
        public DataRow GetTableData(string tableKey, string tableName)
        {
            if (string.IsNullOrEmpty(tableName))
                throw new ArgumentNullException("tableName");

            string tableNameLow = tableName.ToLower();

            DataRow tableData = null;

            //используем совместные данные всех таблиц только при первом вызове и установленном флаге контекста подключений ExtractSummaryTablesMetadata.
            string extractingKey = string.Format("{0}.{1}", tableKey, tableName).ToLower();
            bool extractSummaryMetadata = this.Connection.Context.IsSummaryTablesMetadataContext && !__extracted_TablesData.ContainsKey(extractingKey) && !this.IsRenamedTable(tableName);
            if (!extractSummaryMetadata)
            {
                string query = @"
SELECT name AS [Name] FROM sys.tables WITH(NOLOCK)
WHERE name = N'{TableNameText}'"
                        .ReplaceKey("TableNameText", tableName.QueryEncode());

                tableData = this.DataAdapter.GetDataRow(query);
            }
            else
            {
                if (!__init_TablesData)
                {
                    _TablesData = new Dictionary<string, DataRow>();

                    //получаем метаданные всех таблиц.
                    string query = @"
--получение метаданных таблиц базы {DatabaseName}
SELECT name AS [Name] FROM sys.tables WITH(NOLOCK)"
                        .ReplaceKey("DatabaseName", this.Connection.DisplayName)
                        ;

                    DataTable dtResult = this.DataAdapter.GetDataTable(query);
                    foreach (DataRow resultRow in dtResult.Rows)
                    {
                        string anyTableName = DataRowReader.GetStringValue(resultRow, "Name");
                        if (string.IsNullOrEmpty(anyTableName))
                            continue;

                        string anyTableNameLow = anyTableName.ToLower();
                        if (!_TablesData.ContainsKey(anyTableNameLow))
                            _TablesData.Add(anyTableNameLow, resultRow);
                    }

                    __init_TablesData = true;
                }

                //получаем метаданные таблицы из общей выборки.
                if (_TablesData.ContainsKey(tableNameLow))
                    tableData = _TablesData[tableNameLow];

                if (!__extracted_TablesData.ContainsKey(extractingKey))
                    __extracted_TablesData.Add(extractingKey, true);
            }
            return tableData;
        }


        private Dictionary<string, ICollection> GetTablesMetadata(string query)
        {
            if (string.IsNullOrEmpty(query))
                throw new ArgumentNullException("query");

            Dictionary<string, ICollection> metadataCollection = new Dictionary<string, ICollection>();

            DataTable dtResult = this.DataAdapter.GetDataTable(query);
            foreach (DataRow resultRow in dtResult.Rows)
            {
                string tableName = DataRowReader.GetStringValue(resultRow, "TableName");
                if (string.IsNullOrEmpty(tableName))
                    continue;

                string tableNameLow = tableName.ToLower();
                List<DataRow> metadataRows = null;
                if (metadataCollection.ContainsKey(tableNameLow))
                {
                    metadataRows = (List<DataRow>)metadataCollection[tableNameLow];
                }
                else
                {
                    metadataRows = new List<DataRow>();
                    metadataCollection.Add(tableNameLow, metadataRows);
                }
                if (metadataRows == null)
                    throw new Exception(string.Format("Не удалось сформировать коллекцию метаданных для таблицы {0}.", tableName));
                metadataRows.Add(resultRow);
            }

            return metadataCollection;
        }

        private bool __init_ColumnsData = false;
        private Dictionary<string, ICollection> _ColumnsData;
        private Dictionary<string, bool> __extracted_ColumnsData = new Dictionary<string, bool>();
        /// <summary>
        /// Возвращает метаданные столбцов существующей в базе данных таблицы.
        /// </summary>
        /// <param name="tableName">Название таблицы.</param>
        /// <returns></returns>
        public ICollection GetColumnsData(string tableKey, string tableName)
        {
            if (string.IsNullOrEmpty(tableName))
                throw new ArgumentNullException("tableName");

            string tableNameLow = tableName.ToLower();

            ICollection columnsData = null;

            //используем совместные данные всех таблиц только при первом вызове и установленном флаге контекста подключений ExtractSummaryTablesMetadata.
            string extractingKey = string.Format("{0}.{1}", tableKey, tableName).ToLower();
            bool extractSummaryMetadata = this.Connection.Context.IsSummaryTablesMetadataContext && !__extracted_ColumnsData.ContainsKey(extractingKey) && !this.IsRenamedTable(tableName);
            if (!extractSummaryMetadata)
            {
                string query = @"
SELECT 
	sys.columns.name AS [Name],
	SchemaColumns.DATA_TYPE AS [Type],
	SchemaColumns.CHARACTER_MAXIMUM_LENGTH AS [Size],
	sys.columns.is_nullable AS [IsNullable],
	sys.columns.is_identity AS [IsIdentity],
    FullTextColumns.column_id AS [FullTextColumnID]
FROM sys.columns WITH(NOLOCK)
INNER JOIN sys.tables WITH(NOLOCK)
ON sys.tables.object_id = sys.columns.object_id
AND sys.tables.name = N'{TableNameText}'
INNER JOIN INFORMATION_SCHEMA.COLUMNS AS SchemaColumns WITH(NOLOCK)
ON SchemaColumns.COLUMN_NAME = sys.columns.name
AND SchemaColumns.TABLE_NAME = sys.tables.name
LEFT OUTER JOIN sys.fulltext_index_columns AS FullTextColumns WITH(NOLOCK)
ON FullTextColumns.object_id = sys.tables.object_id
AND FullTextColumns.column_id = sys.columns.column_id
ORDER BY SchemaColumns.ORDINAL_POSITION ASC
"
                        .ReplaceKey("TableNameText", tableName.QueryEncode())
                        ;

                DataTable dtResult = this.DataAdapter.GetDataTable(query);
                columnsData = dtResult.Rows;
            }
            else
            {
                if (!__init_ColumnsData)
                {
                    string query = @"
--получение метаданных столбцов базы {DatabaseName}
SELECT 
    sys.tables.name AS [TableName],
	sys.columns.name AS [Name],
	SchemaColumns.DATA_TYPE AS [Type],
	SchemaColumns.CHARACTER_MAXIMUM_LENGTH AS [Size],
	sys.columns.is_nullable AS [IsNullable],
	sys.columns.is_identity AS [IsIdentity],
    FullTextColumns.column_id AS [FullTextColumnID]
FROM sys.columns WITH(NOLOCK)
INNER JOIN sys.tables WITH(NOLOCK)
ON sys.tables.object_id = sys.columns.object_id
INNER JOIN INFORMATION_SCHEMA.COLUMNS AS SchemaColumns WITH(NOLOCK)
ON SchemaColumns.COLUMN_NAME = sys.columns.name
AND SchemaColumns.TABLE_NAME = sys.tables.name
LEFT OUTER JOIN sys.fulltext_index_columns AS FullTextColumns WITH(NOLOCK)
ON FullTextColumns.object_id = sys.tables.object_id
AND FullTextColumns.column_id = sys.columns.column_id
ORDER BY sys.tables.name ASC, SchemaColumns.ORDINAL_POSITION ASC
"
                        .ReplaceKey("DatabaseName", this.Connection.DisplayName)
                        ;

                    _ColumnsData = this.GetTablesMetadata(query);

                    __init_ColumnsData = true;
                }

                //получаем метаданные таблицы из общей выборки.
                if (_ColumnsData.ContainsKey(tableNameLow))
                    columnsData = _ColumnsData[tableNameLow];

                if (!__extracted_ColumnsData.ContainsKey(extractingKey))
                    __extracted_ColumnsData.Add(extractingKey, true);
            }

            if (columnsData == null)
                columnsData = new List<DataRow>();

            return columnsData;
        }


        private bool __init_IndexesData = false;
        private Dictionary<string, ICollection> _IndexesData;
        private Dictionary<string, bool> __extracted_IndexesData = new Dictionary<string, bool>();
        /// <summary>
        /// Возвращает метаданные индексов существующей в базе данных таблицы.
        /// </summary>
        /// <param name="tableName">Название таблицы.</param>
        /// <returns></returns>
        public ICollection GetIndexesData(string tableKey, string tableName)
        {
            if (string.IsNullOrEmpty(tableName))
                throw new ArgumentNullException("tableName");

            string tableNameLow = tableName.ToLower();

            ICollection indexesData = null;

            //используем совместные данные всех таблиц только при первом вызове и установленном флаге контекста подключений ExtractSummaryTablesMetadata.
            string extractingKey = string.Format("{0}.{1}", tableKey, tableName).ToLower();
            bool extractSummaryMetadata = this.Connection.Context.IsSummaryTablesMetadataContext && !__extracted_IndexesData.ContainsKey(extractingKey) && !this.IsRenamedTable(tableName);
            if (!extractSummaryMetadata)
            {
                string query = @"
SELECT 
	sys.indexes.name AS [Name], 
    sys.indexes.is_disabled AS [Disabled],
    sys.indexes.is_primary_key AS [IsPrimaryKey],
    sys.indexes.is_unique AS [IsUnique],
    sys.indexes.is_unique_constraint AS [IsConstraint],
    sys.fulltext_indexes.unique_index_id AS [FullTextIndexID]
FROM sys.indexes WITH(NOLOCK)
INNER JOIN sys.tables WITH(NOLOCK)
ON sys.tables.object_id = sys.indexes.object_id
AND sys.tables.name = N'{NameText}'
--игнорируем индексы-кучи
AND sys.indexes.type > 0
LEFT OUTER JOIN sys.fulltext_indexes WITH(NOLOCK)
ON sys.fulltext_indexes.object_id = sys.indexes.object_id
AND sys.fulltext_indexes.unique_index_id = sys.indexes.index_id

ORDER BY Name ASC
"
                        .ReplaceKey("NameText", tableName.QueryEncode())
                        ;
                DataTable dtResult = this.DataAdapter.GetDataTable(query);
                indexesData = dtResult.Rows;
            }
            else
            {
                if (!__init_IndexesData)
                {
                    string query = @"
--получение метаданных индексов базы {DatabaseName}
SELECT 
    sys.tables.name AS [TableName],
	sys.indexes.name AS [Name], 
    sys.indexes.is_disabled AS [Disabled],
    sys.indexes.is_primary_key AS [IsPrimaryKey],
    sys.indexes.is_unique AS [IsUnique],
    sys.indexes.is_unique_constraint AS [IsConstraint],
    sys.fulltext_indexes.unique_index_id AS [FullTextIndexID]
FROM sys.indexes WITH(NOLOCK)
INNER JOIN sys.tables WITH(NOLOCK)
ON sys.tables.object_id = sys.indexes.object_id
--игнорируем индексы-кучи
AND sys.indexes.type > 0
LEFT OUTER JOIN sys.fulltext_indexes WITH(NOLOCK)
ON sys.fulltext_indexes.object_id = sys.indexes.object_id
AND sys.fulltext_indexes.unique_index_id = sys.indexes.index_id

ORDER BY sys.tables.name ASC, Name ASC
"
                        .ReplaceKey("DatabaseName", this.Connection.DisplayName)
                        ;

                    _IndexesData = this.GetTablesMetadata(query);

                    __init_IndexesData = true;
                }

                //получаем метаданные таблицы из общей выборки.
                if (_IndexesData.ContainsKey(tableNameLow))
                    indexesData = _IndexesData[tableNameLow];

                if (!__extracted_IndexesData.ContainsKey(extractingKey))
                    __extracted_IndexesData.Add(extractingKey, true);
            }

            if (indexesData == null)
                indexesData = new List<DataRow>();

            return indexesData;
        }


        private bool __init_IndexColumnsData = false;
        private Dictionary<string, ICollection> _IndexColumnsData;
        private Dictionary<string, bool> __extracted_IndexColumnsData = new Dictionary<string, bool>();
        /// <summary>
        /// Возвращает метаданные столбцов индексов существующей в базе данных таблицы.
        /// </summary>
        /// <param name="tableName">Название таблицы.</param>
        /// <returns></returns>
        public ICollection GetIndexColumnsData(string tableKey, string tableName)
        {
            if (string.IsNullOrEmpty(tableName))
                throw new ArgumentNullException("tableName");

            string tableNameLow = tableName.ToLower();

            ICollection indexColumnsData = null;

            //используем совместные данные всех таблиц только при первом вызове и установленном флаге контекста подключений ExtractSummaryTablesMetadata.
            string extractingKey = string.Format("{0}.{1}", tableKey, tableName).ToLower();
            bool extractSummaryMetadata = this.Connection.Context.IsSummaryTablesMetadataContext && !__extracted_IndexColumnsData.ContainsKey(extractingKey) && !this.IsRenamedTable(tableName);
            if (!extractSummaryMetadata)
            {
                //инициализируем набор столбцов индексов.
                string query = @"
SELECT 
	sys.indexes.name AS IndexName, 
	sys.columns.name AS ColumnName, 
	sys.index_columns.key_ordinal AS Ordinal, 
	sys.index_columns.is_descending_key AS IsDescending
FROM sys.index_columns WITH(NOLOCK)

INNER JOIN sys.tables WITH(NOLOCK)
ON sys.tables.object_id = sys.index_columns.object_id
AND sys.tables.name = N'{NameText}'

INNER JOIN sys.indexes WITH(NOLOCK)
ON sys.indexes.object_id = sys.index_columns.object_id
AND sys.indexes.index_id = sys.index_columns.index_id

INNER JOIN sys.columns WITH(NOLOCK)
ON sys.columns.object_id = sys.index_columns.object_id
AND sys.columns.column_id = sys.index_columns.column_id
ORDER BY IndexName ASC, Ordinal ASC
"
                    .ReplaceKey("NameText", tableName.QueryEncode())
                    ;

                DataTable dtResult = this.DataAdapter.GetDataTable(query);
                indexColumnsData = dtResult.Rows;
            }
            else
            {
                if (!__init_IndexColumnsData)
                {
                    string query = @"
--получение метаданных столбцов индексов базы {DatabaseName}
SELECT 
    sys.tables.name AS [TableName],
	sys.indexes.name AS IndexName, 
	sys.columns.name AS ColumnName, 
	sys.index_columns.key_ordinal AS Ordinal, 
	sys.index_columns.is_descending_key AS IsDescending
FROM sys.index_columns WITH(NOLOCK)

INNER JOIN sys.tables WITH(NOLOCK)
ON sys.tables.object_id = sys.index_columns.object_id

INNER JOIN sys.indexes WITH(NOLOCK)
ON sys.indexes.object_id = sys.index_columns.object_id
AND sys.indexes.index_id = sys.index_columns.index_id

INNER JOIN sys.columns WITH(NOLOCK)
ON sys.columns.object_id = sys.index_columns.object_id
AND sys.columns.column_id = sys.index_columns.column_id
ORDER BY sys.tables.name ASC, IndexName ASC, Ordinal ASC
"
                    .ReplaceKey("DatabaseName", this.Connection.DisplayName)
                    ;

                    _IndexColumnsData = this.GetTablesMetadata(query);

                    __init_IndexColumnsData = true;
                }

                //получаем метаданные таблицы из общей выборки.
                if (_IndexColumnsData.ContainsKey(tableNameLow))
                    indexColumnsData = _IndexColumnsData[tableNameLow];

                if (!__extracted_IndexColumnsData.ContainsKey(extractingKey))
                    __extracted_IndexColumnsData.Add(extractingKey, true);
            }

            if (indexColumnsData == null)
                indexColumnsData = new List<DataRow>();

            return indexColumnsData;
        }


        private bool __init_TriggersData = false;
        private Dictionary<string, ICollection> _TriggersData;
        private Dictionary<string, bool> __extracted_TriggersData = new Dictionary<string, bool>();
        /// <summary>
        /// Возвращает метаданные триггеров существующей в базе данных таблицы.
        /// </summary>
        /// <param name="tableName">Название таблицы.</param>
        /// <returns></returns>
        public ICollection GetTriggersData(string tableKey, string tableName)
        {
            if (string.IsNullOrEmpty(tableName))
                throw new ArgumentNullException("tableName");

            string tableNameLow = tableName.ToLower();

            ICollection triggersData = null;

            //используем совместные данные всех таблиц только при первом вызове и установленном флаге контекста подключений ExtractSummaryTablesMetadata.
            string extractingKey = string.Format("{0}.{1}", tableKey, tableName).ToLower();
            bool extractSummaryMetadata = this.Connection.Context.IsSummaryTablesMetadataContext && !__extracted_TriggersData.ContainsKey(extractingKey) && !this.IsRenamedTable(tableName);
            if (!extractSummaryMetadata)
            {
                //инициализируем перечень существующих триггеров таблицы.
                string query = @"
SELECT 
	sys.triggers.name AS [Name], 
    sys.triggers.is_disabled AS [Disabled]
FROM sys.triggers WITH(NOLOCK)
INNER JOIN sys.tables WITH(NOLOCK)
ON sys.tables.object_id = sys.triggers.parent_id
AND sys.tables.name = N'{NameText}'

ORDER BY Name ASC
"
                    .ReplaceKey("NameText", tableName.QueryEncode())
                    ;

                DataTable dtResult = this.DataAdapter.GetDataTable(query);
                triggersData = dtResult.Rows;
            }
            else
            {
                if (!__init_TriggersData)
                {
                    string query = @"
--получение метаданных триггеров базы {DatabaseName}
SELECT 
    sys.tables.name AS [TableName],
	sys.triggers.name AS [Name], 
    sys.triggers.is_disabled AS [Disabled]
FROM sys.triggers WITH(NOLOCK)
INNER JOIN sys.tables WITH(NOLOCK)
ON sys.tables.object_id = sys.triggers.parent_id

ORDER BY sys.tables.name ASC, Name ASC
"
                    .ReplaceKey("DatabaseName", this.Connection.DisplayName)
                    ;

                    _TriggersData = this.GetTablesMetadata(query);

                    __init_TriggersData = true;
                }

                //получаем метаданные таблицы из общей выборки.
                if (_TriggersData.ContainsKey(tableNameLow))
                    triggersData = _TriggersData[tableNameLow];

                if (!__extracted_TriggersData.ContainsKey(extractingKey))
                    __extracted_TriggersData.Add(extractingKey, true);
            }

            if (triggersData == null)
                triggersData = new List<DataRow>();

            return triggersData;
        }


        private bool __init_TriggerEventsData = false;
        private Dictionary<string, ICollection> _TriggerEventsData;
        private Dictionary<string, bool> __extracted_TriggerEventsData = new Dictionary<string, bool>();
        /// <summary>
        /// Возвращает метаданные событий триггеров существующей в базе данных таблицы.
        /// </summary>
        /// <param name="tableName">Название таблицы.</param>
        /// <returns></returns>
        public ICollection GetTriggerEventsData(string tableKey, string tableName)
        {
            if (string.IsNullOrEmpty(tableName))
                throw new ArgumentNullException("tableName");

            string tableNameLow = tableName.ToLower();

            ICollection triggerEventsData = null;

            //используем совместные данные всех таблиц только при первом вызове и установленном флаге контекста подключений ExtractSummaryTablesMetadata.
            string extractingKey = string.Format("{0}.{1}", tableKey, tableName).ToLower();
            bool extractSummaryMetadata = this.Connection.Context.IsSummaryTablesMetadataContext && !__extracted_TriggerEventsData.ContainsKey(extractingKey) && !this.IsRenamedTable(tableName);
            if (!extractSummaryMetadata)
            {
                //инициализируем коллекцию событий триггеров.
                string query = @"
SELECT 
	sys.triggers.name AS [TriggerName], 
	sys.trigger_events.type AS [Type]
FROM sys.trigger_events WITH(NOLOCK)

INNER JOIN sys.triggers WITH(NOLOCK)
ON sys.triggers.object_id = sys.trigger_events.object_id

INNER JOIN sys.tables WITH(NOLOCK)
ON sys.tables.object_id = sys.triggers.parent_id
AND sys.tables.name = N'{NameText}'

ORDER BY [TriggerName] ASC, [Type] ASC
"
                    .ReplaceKey("NameText", tableName.QueryEncode())
                    ;

                DataTable dtResult = this.DataAdapter.GetDataTable(query);
                triggerEventsData = dtResult.Rows;
            }
            else
            {
                if (!__init_TriggerEventsData)
                {
                    string query = @"
--получение метаданных событий триггеров базы {DatabaseName}
SELECT 
    sys.tables.name AS [TableName],
	sys.triggers.name AS [TriggerName], 
	sys.trigger_events.type AS [Type]
FROM sys.trigger_events WITH(NOLOCK)

INNER JOIN sys.triggers WITH(NOLOCK)
ON sys.triggers.object_id = sys.trigger_events.object_id

INNER JOIN sys.tables WITH(NOLOCK)
ON sys.tables.object_id = sys.triggers.parent_id

ORDER BY sys.tables.name ASC, [TriggerName] ASC, [Type] ASC
"
                    .ReplaceKey("DatabaseName", this.Connection.DisplayName)
                    ;

                    _TriggerEventsData = this.GetTablesMetadata(query);

                    __init_TriggerEventsData = true;
                }

                //получаем метаданные таблицы из общей выборки.
                if (_TriggerEventsData.ContainsKey(tableNameLow))
                    triggerEventsData = _TriggerEventsData[tableNameLow];

                if (!__extracted_TriggerEventsData.ContainsKey(extractingKey))
                    __extracted_TriggerEventsData.Add(extractingKey, true);
            }

            if (triggerEventsData == null)
                triggerEventsData = new List<DataRow>();

            return triggerEventsData;
        }
    }
}
