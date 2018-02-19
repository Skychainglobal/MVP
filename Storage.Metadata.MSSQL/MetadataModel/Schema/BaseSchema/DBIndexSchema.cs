using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Storage.Metadata.MSSQL
{
    /// <summary>
    /// Представляет индекс схемы таблицы.
    /// </summary>
    public abstract class DBIndexSchema
    {
        /// <summary>
        /// Создает экземпляр DBIndexSchema.
        /// </summary>
        /// <param name="schemaAdapter">Адаптер схемы таблицы, к которой относится индекс.</param>
        protected DBIndexSchema(DBTableSchemaAdapter schemaAdapter)
        {
            if (schemaAdapter == null)
                throw new ArgumentNullException("schemaAdapter");

            this.SchemaAdapter = schemaAdapter;
        }

        private DBTableSchemaAdapter _SchemaAdapter;
        /// <summary>
        /// Адаптер схемы таблицы.
        /// </summary>
        public DBTableSchemaAdapter SchemaAdapter
        {
            get { return _SchemaAdapter; }
            private set { _SchemaAdapter = value; }
        }

        /// <summary>
        /// Инициализирует относительное название индекса.
        /// </summary>
        /// <returns></returns>
        protected abstract string InitRelativeName();

        private bool __init_RelativeName = false;
        private string _RelativeName;
        /// <summary>
        /// Относительное название индекса, уникальное в рамках таблицы.
        /// </summary>
        public string RelativeName
        {
            get
            {
                if (!__init_RelativeName)
                {
                    _RelativeName = this.InitRelativeName();
                    if (string.IsNullOrEmpty(_RelativeName))
                        throw new Exception(string.Format("Не задано относительное название индекса в схеме таблицы {0}.", this.SchemaAdapter.TableName));

                    __init_RelativeName = true;
                }
                return _RelativeName;
            }
        }

        private bool __init_RelativeNameLow = false;
        private string _RelativeNameLow;
        /// <summary>
        /// Значение свойства RelativeName в нижнем регистре.
        /// </summary>
        protected internal string RelativeNameLow
        {
            get
            {
                if (!__init_RelativeNameLow)
                {
                    if (!string.IsNullOrEmpty(this.RelativeName))
                        _RelativeNameLow = this.RelativeName.ToLower();
                    __init_RelativeNameLow = true;
                }
                return _RelativeNameLow;
            }
        }

        private bool __init_Columns = false;
        private DBCollection<DBIndexColumnSchema> _Columns;
        /// <summary>
        /// Коллекция столбцов индекса.
        /// </summary>
        public DBCollection<DBIndexColumnSchema> Columns
        {
            get
            {
                if (!__init_Columns)
                {
                    _Columns = new DBCollection<DBIndexColumnSchema>(this.ColumnsByName.Values);
                    __init_Columns = true;
                }
                return _Columns;
            }
        }

        /// <summary>
        /// Инициализирует коллекцию столбцов индекса.
        /// </summary>
        protected abstract ICollection<DBIndexColumnSchema> InitColumns();

        private bool __init_ColumnsByName = false;
        private Dictionary<string, DBIndexColumnSchema> _ColumnsByName;
        /// <summary>
        /// Словарь столбцов индекса по имени.
        /// </summary>
        private Dictionary<string, DBIndexColumnSchema> ColumnsByName
        {
            get
            {
                if (!__init_ColumnsByName)
                {
                    _ColumnsByName = new Dictionary<string, DBIndexColumnSchema>();
                    ICollection<DBIndexColumnSchema> columns = this.InitColumns();

                    if (columns != null)
                    {
                        int columnOrdinal = 1;
                        foreach (DBIndexColumnSchema column in columns)
                        {
                            if (!_ColumnsByName.ContainsKey(column.NameLow))
                            {
                                _ColumnsByName.Add(column.NameLow, column);
                                column.Ordinal = columnOrdinal;
                                columnOrdinal++;
                            }
                            else
                                throw new Exception(string.Format("Столбец [{0}] уже добавлен в индекс '{1}' схемы таблицы {2}.", this.RelativeName, column.Name, this.SchemaAdapter.TableName));
                        }
                    }

                    //проверяем наличие столбцов в индексе.
                    if (_ColumnsByName.Count == 0)
                        throw new Exception(string.Format("Отсутствуют столбцы в индексе '{0}' схемы таблицы {1}.", this.RelativeName, this.SchemaAdapter.TableName));

                    __init_ColumnsByName = true;
                }
                return _ColumnsByName;
            }
        }

        /// <summary>
        /// Сбрасывает флаг инициализации свойства ColumnsByName.
        /// </summary>
        private void ResetColumns()
        {
            this.__init_Columns = false;            
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
        public DBIndexColumnSchema GetColumn(string columnName)
        {
            if (string.IsNullOrEmpty(columnName))
                throw new ArgumentNullException("columnName");

            DBIndexColumnSchema column = null;
            if (this.ColumnsByName.ContainsKey(columnName.ToLower()))
                column = this.ColumnsByName[columnName.ToLower()];
            return column;
        }

        /// <summary>
        /// Удаляет столбец из состава столбцов индекса.
        /// </summary>
        /// <param name="columnName">Название удаляемого столбца.</param>
        internal void DeleteColumn(string columnName)
        {
            if (string.IsNullOrEmpty(columnName))
                throw new ArgumentNullException("columnName");

            if (!this.ContainsColumn(columnName))
                throw new Exception(string.Format("Удаляемый столбец [{0}] должен входить в состав столбцов индекса '{1}'.", columnName, this.RelativeName));

            this.ColumnsByName.Remove(columnName.ToLower());
            this.ResetColumns();
        }

        /// <summary>
        /// Возвращает true, если сравниваемый с данным индексом существующий индекс имеет одинаковый набор столбцов,
        /// расположенных в одинаковом порядке, имеющих одинаковые направления сортировки.
        /// </summary>
        /// <param name="existingIndexToCompare">Существующий индекс таблицы, сравниваемый с данным индексом.</param>
        /// <returns></returns>
        internal bool ColumnsEqual(DBIndexInfo existingIndexToCompare)
        {
            if (existingIndexToCompare == null)
                throw new ArgumentNullException("existingIndexToCompare");

            //флаг равенства индексов
            bool equal = false;

            //сначала сравниваем количество столбцов
            if (this.Columns.Count == existingIndexToCompare.Columns.Count)
            {
                equal = true;
                for (int i = 0; i < this.Columns.Count; i++)
                {
                    DBIndexColumnSchema column = this.Columns[i];
                    DBIndexColumnInfo columnToCompare = existingIndexToCompare.Columns[i];
                    //если хотя бы один столбец не равен соответствующему по то му же порядковому номеру,
                    //прекращаем сравнение и возвращаем false.
                    if (!column.EqualsTo(columnToCompare))
                    {
                        equal = false;
                        break;
                    }
                }
            }
            return equal;
        }

        /// <summary>
        /// Строковое представление экземпляра DBIndexSchema.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(this.RelativeName))
                return this.RelativeName;
            return base.ToString();
        }
    }
}
