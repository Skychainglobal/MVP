using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Storage.Metadata.MSSQL
{
    /// <summary>
    /// Представляет класс, содержащий параметры существующего индекса таблицы.
    /// </summary>
    public class DBIndexInfo
    {
        /// <summary>
        /// Создает экземпляр DBIndexInfo.
        /// </summary>
        /// <param name="data">Метаданные индекса.</param>
        /// <param name="table">Таблица, к которой относится индекс.</param>
        internal DBIndexInfo(DataRow data, DBTableInfo table)
        {
            if (data == null)
                throw new ArgumentNullException("data");
            if (table == null)
                throw new ArgumentNullException("table");

            this.Data = data;
            this.Table = table;
        }

        private DataRow _Data;
        /// <summary>
        /// Метаданые индекса.
        /// </summary>
        private DataRow Data
        {
            get { return _Data; }
            set { _Data = value; }
        }

        private DBTableInfo _Table;
        /// <summary>
        /// Таблица в которой содержится индекс.
        /// </summary>
        public DBTableInfo Table
        {
            get { return _Table; }
            private set { _Table = value; }
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
        /// Название индекса.
        /// </summary>
        public string Name
        {
            get
            {
                if (!__init_Name)
                {
                    _Name = this.Reader.GetStringValue("Name");
                    if (string.IsNullOrEmpty(_Name))
                        throw new Exception("Не удалось получить название индекса.");
                    __init_Name = true;
                }
                return _Name;
            }
        }

        private bool __init_NameLow = false;
        private string _NameLow;
        /// <summary>
        /// Название индекса в нижнем регистре.
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

        private bool __init_IsCustom = false;
        private bool _IsCustom;
        /// <summary>
        /// Возвращает true, если индекс является кастомизированным и не должен быть удален при обновлении таблиц, даже при отсустствии индекса в схеме таблицы.
        /// </summary>
        public bool IsCustom
        {
            get
            {
                if (!__init_IsCustom)
                {
                    _IsCustom = this.NameLow.StartsWith("custom_");
                    __init_IsCustom = true;
                }
                return _IsCustom;
            }
        }

        private bool __init_Disabled = false;
        private bool _Disabled;
        /// <summary>
        /// Возвращает true, если индекс отключен и игнорируется оптимизатором запросов.
        /// </summary>
        public bool Disabled
        {
            get
            {
                if (!__init_Disabled)
                {
                    _Disabled = this.Reader.GetBooleanValue("Disabled");
                    __init_Disabled = true;
                }
                return _Disabled;
            }
        }

        private bool __init_IsPrimaryKey = false;
        private bool _IsPrimaryKey;
        /// <summary>
        /// Возвращает true, если индекс является первичным ключом.
        /// </summary>
        public bool IsPrimaryKey
        {
            get
            {
                if (!__init_IsPrimaryKey)
                {
                    _IsPrimaryKey = this.Reader.GetBooleanValue("IsPrimaryKey");
                    __init_IsPrimaryKey = true;
                }
                return _IsPrimaryKey;
            }
        }

        private bool __init_IsUnique = false;
        private bool _IsUnique;
        /// <summary>
        /// Возвращает true, если ключи, хранящиеся в индексе являются уникальными.
        /// </summary>
        public bool IsUnique
        {
            get
            {
                if (!__init_IsUnique)
                {
                    _IsUnique = this.Reader.GetBooleanValue("IsUnique");
                    __init_IsUnique = true;
                }
                return _IsUnique;
            }
        }

        private bool __init_IsConstraint = false;
        private bool _IsConstraint;
        /// <summary>
        /// Возвращает true, если индекс является ограничением.
        /// </summary>
        public bool IsConstraint
        {
            get
            {
                if (!__init_IsConstraint)
                {
                    _IsConstraint = this.Reader.GetBooleanValue("IsConstraint");
                    __init_IsConstraint = true;
                }
                return _IsConstraint;
            }
        }

        private bool __init_InitializingColumns = false;
        private Dictionary<string, DBIndexColumnInfo> _InitializingColumns;
        /// <summary>
        /// Столбцы индекса, заполняемые при инициализации экземпляра DBIndexReader во внешнем, по отношению к данному экземпляру, коде. 
        /// Данное internal-свойство используются для последующей инициализации public-свойства Columns, уже доступного для чтения.
        /// </summary>
        internal Dictionary<string, DBIndexColumnInfo> InitializingColumns
        {
            get
            {
                if (!__init_InitializingColumns)
                {
                    _InitializingColumns = new Dictionary<string, DBIndexColumnInfo>();
                    __init_InitializingColumns = true;
                }
                return _InitializingColumns;
            }
        }

        private bool __init_ColumnsByName = false;
        private Dictionary<string, DBIndexColumnInfo> _ColumnsByName;
        /// <summary>
        /// Словарь столбцов индекса по имени.
        /// </summary>
        private Dictionary<string, DBIndexColumnInfo> ColumnsByName
        {
            get
            {
                if (!__init_ColumnsByName)
                {
                    if (this.InitializingColumns.Count == 0)
                        throw new Exception(string.Format("Индекс {0} не содержит ни одного столбца.", this.Name));
                    _ColumnsByName = this.InitializingColumns;
                    __init_ColumnsByName = true;
                }
                return _ColumnsByName;
            }
        }

        private bool __init_Columns = false;
        private DBCollection<DBIndexColumnInfo> _Columns;
        /// <summary>
        /// Коллекция столбцов индекса.
        /// </summary>
        public DBCollection<DBIndexColumnInfo> Columns
        {
            get
            {
                if (!__init_Columns)
                {
                    _Columns = new DBCollection<DBIndexColumnInfo>(this.ColumnsByName.Values);
                    __init_Columns = true;
                }
                return _Columns;
            }
        }

        private bool __init_SchemaIndex = false;
        private DBIndex _SchemaIndex;
        /// <summary>
        /// Индекс схемы таблицы, соответствующий индексу, присутствующему в таблице.
        /// </summary>
        internal DBIndex SchemaIndex
        {
            get
            {
                if (!__init_SchemaIndex)
                {
                    _SchemaIndex = this.Table.SchemaTable.GetExistingSchemaIndex(this.Name);
                    __init_SchemaIndex = true;
                }
                return _SchemaIndex;
            }
        }

        /// <summary>
        /// Возвращает true, если индекс содержит столбец с названием columnName.
        /// </summary>
        /// <param name="columnName">Название столбца.</param>
        /// <returns></returns>
        public bool ContainsColumn(string columnName)
        {
            if (string.IsNullOrEmpty(columnName))
                throw new ArgumentNullException("columnName");

            //проверяем наличие существующего столбца в индексе.
            bool result = this.ColumnsByName.ContainsKey(columnName.ToLower());
            return result;
        }

        /// <summary>
        /// Возвращает столбец индекса по названию столбца.
        /// </summary>
        /// <param name="columnName">Название столбца индекса.</param>
        /// <returns></returns>
        public DBIndexColumnInfo GetColumn(string columnName)
        {
            if (string.IsNullOrEmpty(columnName))
                throw new ArgumentNullException("columnName");

            DBIndexColumnInfo column = null;
            if (this.ColumnsByName.ContainsKey(columnName.ToLower()))
                column = this.ColumnsByName[columnName.ToLower()];
            return column;
        }

        /// <summary>
        /// Возвращает текст запроса удаления индекса.
        /// </summary>
        /// <returns></returns>
        internal string GetDropQuery()
        {
            //проверяем, что название таблицы не изменилось и переименование перед операцией не требуется.
            this.Table.SchemaTable.CheckRenameNotRequired();

            if (!(this.IsPrimaryKey || this.IsConstraint))
                return this.GetNonclusteredDropQuery();
            else
                return this.GetConstraintDropQuery();
        }

        /// <summary>
        /// Возвращает текст запроса удаления индекса первичного ключа.
        /// </summary>
        /// <returns></returns>
        private string GetConstraintDropQuery()
        {
            //формируем запрос на основе существующего индекса.
            string query = @"
IF EXISTS(SELECT name FROM sys.indexes WITH(NOLOCK) WHERE name = N'{IndexNameText}')
BEGIN
    ALTER TABLE [dbo].[{TableName}] DROP CONSTRAINT [{IndexName}]
END"
                .ReplaceKey("IndexNameText", this.Name.QueryEncode())
                .ReplaceKey("IndexName", this.Name)
                .ReplaceKey("TableName", this.Table.Name)
                ;
            return query;
        }

        /// <summary>
        /// Возвращает текст запроса удаления некластеризованного индекса.
        /// </summary>
        /// <returns></returns>
        private string GetNonclusteredDropQuery()
        {
            //формируем запрос на основе существующего индекса.
            string query = @"
IF EXISTS(SELECT name FROM sys.indexes WITH(NOLOCK) WHERE name = N'{IndexNameText}')
BEGIN
    DROP INDEX [{IndexName}] ON [dbo].[{TableName}] WITH ( ONLINE = OFF )
END"
                 .ReplaceKey("IndexNameText", this.Name.QueryEncode())
                 .ReplaceKey("IndexName", this.Name)
                 .ReplaceKey("TableName", this.Table.Name)
                 ;
            return query;
        }

        internal string GetDropFullTextIndexQuery()
        {
            string query = @"
IF EXISTS(SELECT name FROM sys.indexes WITH(NOLOCK) WHERE name = N'{IndexNameText}')
BEGIN
    DROP FULLTEXT INDEX ON [dbo].[{TableName}]
END"
                    .ReplaceKey("IndexNameText", this.Name.QueryEncode())
                    .ReplaceKey("IndexName", this.Name)
                    .ReplaceKey("TableName", this.Table.Name)
                    ;
            return query;
        }

        /// <summary>
        /// Текстовое представление экземпляра DBIndexInfo.
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
