using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Storage.Metadata.MSSQL
{
    /// <summary>
    /// Представляет класс, содержащий параметры существующего столбца таблицы.
    /// </summary>
    public class DBColumnInfo
    {
        /// <summary>
        /// Создает экземпляр DBColumnInfo.
        /// </summary>
        /// <param name="data">Метаданные столбца.</param>
        /// <param name="table"></param>
        internal DBColumnInfo(DataRow data, DBTableInfo table)
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
        /// Метаданные столбца.
        /// </summary>
        private DataRow Data
        {
            get { return _Data; }
            set { _Data = value; }
        }

        private DBTableInfo _Table;
        /// <summary>
        /// Таблица в которой содержится столбец.
        /// </summary>
        public DBTableInfo Table
        {
            get { return _Table; }
            private set { _Table = value; }
        }

        private bool __init_Reader = false;
        private DataRowReader _Reader;
        /// <summary>
        /// 
        /// </summary>
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
        /// Название столбца.
        /// </summary>
        public string Name
        {
            get
            {
                if (!__init_Name)
                {
                    _Name = this.Reader.GetStringValue("Name");
                    if (string.IsNullOrEmpty(_Name))
                        throw new Exception("Не удалось получить название столбца.");
                    __init_Name = true;
                }
                return _Name;
            }
        }

        private bool __init_NameLow = false;
        private string _NameLow;
        /// <summary>
        /// Название столбца в нижнем регистре.
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

        private bool __init_Type = false;
        private SqlDbType _Type;
        /// <summary>
        /// Тип данных столбца.
        /// </summary>
        public SqlDbType Type
        {
            get
            {
                if (!__init_Type)
                {
                    string sqlTypeText = this.Reader.GetStringValue("Type");
                    if (string.IsNullOrEmpty(sqlTypeText))
                        throw new Exception(string.Format("Не удалось получить тип столбца {0}.", this.Name));

                    _Type = (SqlDbType)Enum.Parse(typeof(SqlDbType), sqlTypeText, true);
                    __init_Type = true;
                }
                return _Type;
            }
        }

        private bool __init_Size = false;
        private int _Size;
        /// <summary>
        /// Размер столбца.
        /// </summary>
        public int Size
        {
            get
            {
                if (!__init_Size)
                {
                    _Size = this.Reader.GetIntegerValue("Size");
                    __init_Size = true;
                }
                return _Size;
            }
        }


        private bool __init_IsNullable = false;
        private bool _IsNullable;
        /// <summary>
        /// Возвращает true, если столбец поддерживает значения NULL в строках таблицы.
        /// </summary>
        public bool IsNullable
        {
            get
            {
                if (!__init_IsNullable)
                {
                    _IsNullable = this.Reader.GetBooleanValue("IsNullable");
                    __init_IsNullable = true;
                }
                return _IsNullable;
            }
        }

        private bool __init_IsIdentity = false;
        private bool _IsIdentity;
        /// <summary>
        /// Возвращает true, если столбец является столбцом автоинкремента таблицы.
        /// </summary>
        public bool IsIdentity
        {
            get
            {
                if (!__init_IsIdentity)
                {
                    _IsIdentity = this.Reader.GetBooleanValue("IsIdentity");
                    __init_IsIdentity = true;
                }
                return _IsIdentity;
            }
        }

        private bool __init_DependendIndexes = false;
        private DBCollection<DBIndexInfo> _DependendIndexes;
        /// <summary>
        /// Индексы, в которые включен данный столбец.
        /// </summary>
        public DBCollection<DBIndexInfo> DependendIndexes
        {
            get
            {
                if (!__init_DependendIndexes)
                {
                    _DependendIndexes = new DBCollection<DBIndexInfo>();
                    foreach (DBIndexInfo index in this.Table.Indexes)
                    {
                        if (index.ContainsColumn(this.Name))
                        {                            
                            _DependendIndexes.Add(index);
                        }
                    }
                    __init_DependendIndexes = true;
                }
                return _DependendIndexes;
            }
        }

        private bool __init_Definition = false;
        private string _Definition;
        /// <summary>
        /// Строка определения столбца таблицы, содержащая название, тип, размер и возможность установки значений NULL в столбце.
        /// </summary>
        public string Definition
        {
            get
            {
                if (!__init_Definition)
                {
                    //получаем строку определения столбца.
                    _Definition = DBColumnSchema.GetColumnDefinition(this.Name, this.Type, this.Size, this.IsNullable);
                    __init_Definition = true;
                }
                return _Definition;
            }
        }

        /// <summary>
        /// Возвращает текст запроса удаления столбца из таблицы.
        /// </summary>
        /// <returns></returns>
        internal string GetDropQuery()
        {
            string query = @"
IF(EXISTS(
    SELECT sys.columns.name FROM sys.columns WITH(NOLOCK)
    INNER JOIN sys.tables WITH(NOLOCK)
    ON sys.tables.name = N'{TableNameText}'
    AND sys.tables.object_id = sys.columns.object_id
    AND sys.columns.name = N'{ColumnNameText}'
))
BEGIN
    ALTER TABLE [{TableName}] DROP COLUMN [{ColumnName}]
END"
                .ReplaceKey("TableNameText", this.Table.Name.QueryEncode())
                .ReplaceKey("ColumnName", this.Name)
                .ReplaceKey("ColumnNameText", this.Name.QueryEncode())
                .ReplaceKey("TableName", this.Table.Name)
                ;
            return query;
        }

        /// <summary>
        /// Текстовое представление экземпляра DBColumnInfo.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(this.Definition))
                return this.Definition;
            return base.ToString();
        }
    }
}
