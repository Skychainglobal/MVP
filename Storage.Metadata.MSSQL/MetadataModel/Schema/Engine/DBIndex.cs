using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Storage.Metadata.MSSQL
{
    /// <summary>
    /// Представляет индекс таблицы.
    /// </summary>
    public abstract class DBIndex
    {
        /// <summary>
        /// Создает экземпляр DBIndex.
        /// </summary>
        /// <param name="schema">Схема индекса.</param>
        /// <param name="table">Таблица.</param>
        internal DBIndex(DBIndexSchema schema, DBTable table)
        {
            if (schema == null)
                throw new ArgumentNullException("schema");
            if (table == null)
                throw new ArgumentNullException("table");

            this.Schema = schema;
            this.Table = table;
        }

        private DBIndexSchema _Schema;
        /// <summary>
        /// Схема индекса в таблице.
        /// </summary>
        public DBIndexSchema Schema
        {
            get { return _Schema; }
            private set { _Schema = value; }
        }

        private DBTable _Table;
        /// <summary>
        /// Таблица.
        /// </summary>
        public DBTable Table
        {
            get { return _Table; }
            private set { _Table = value; }
        }

        /// <summary>
        /// Адаптер схемы таблицы.
        /// </summary>
        public DBTableSchemaAdapter SchemaAdapter
        {
            get { return this.Table.SchemaAdapter; }
        }

        private bool __init_IsSystem = false;
        private bool _IsSystem;
        /// <summary>
        /// Индекс является системным и создается вместе с инициализацией таблицы.
        /// </summary>
        public bool IsSystem
        {
            get
            {
                if (!__init_IsSystem)
                {
                    _IsSystem = this.Table.Schema.IsSystemIndex(this.Schema.RelativeName);
                    __init_IsSystem = true;
                }
                return _IsSystem;
            }
        }


        #region Name

        /// <summary>
        /// Полное название индекса, уникальное в рамках базы данных.
        /// </summary>
        public abstract string Name { get; }

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

        private bool __init_NameChanged = false;
        private bool _NameChanged;
        /// <summary>
        /// Возвращает true, если название индекса изменилось в контексте выполнения кода.
        /// </summary>
        internal bool NameChanged
        {
            get
            {
                if (!__init_NameChanged)
                {
                    _NameChanged = this.Exists && this.ExistingIndex.NameLow != this.NameLow;
                    __init_NameChanged = true;
                }
                return _NameChanged;
            }
        }

        /// <summary>
        /// Сбрасываем инициализацию названия индекса и зависимых от него свойств.
        /// </summary>
        internal virtual void ResetName()
        {
            this.__init_NameLow = false;
            this.__init_NameChanged = false;

            //сбрасываем инициализацию существующего индекса.
            if (this.NameChanged)
                this.ResetExistingIndex();
        }

        /// <summary>
        /// Проверяет, требуется ли пересоздание индекса. В случае если пересоздание индекса не требуется, генерирует исключение.
        /// </summary>
        internal void CheckRenameRequired()
        {
            if (!this.NameChanged)
                throw new Exception(string.Format("Операция доступа только при необходимости переименования индекса [{0}] в таблице {1}.", this.Name, this.Table.Name));
        }

        /// <summary>
        /// Проверяет, что индекс не был переименован в контексте выполнения кода, в ином случае - генерирует исключение.
        /// </summary>
        internal void CheckRenameNotRequired()
        {
            if (this.NameChanged)
                throw new Exception(string.Format("Операция недоступна, поскольку индекс {0} должен быть переименован в {1}.", this.ExistingIndex.Name, this.Name));
        }

        #endregion


        #region ExistingIndex

        private bool __init_ExistingIndex = false;
        private DBIndexInfo _ExistingIndex;
        /// <summary>
        /// Существующий индекс таблицы.
        /// </summary>
        public DBIndexInfo ExistingIndex
        {
            get
            {
                if (!__init_ExistingIndex)
                {
                    _ExistingIndex = null;
                    if (!this.Table.RenameRequired)
                    {
                        if (this.Table.ExistingTable != null)
                            _ExistingIndex = this.GetExistingIndex(this.Table.ExistingTable);
                    }
                    __init_ExistingIndex = true;
                }
                return _ExistingIndex;
            }
        }

        /// <summary>
        /// Возвращает существующий индекс существующей таблицы, соответвующий схеме индекса.
        /// </summary>
        /// <param name="existingTable">Существующая таблица.</param>
        /// <returns></returns>
        protected abstract DBIndexInfo GetExistingIndex(DBTableInfo existingTable);

        private bool __init_Exists = false;
        private bool _Exists;
        /// <summary>
        /// Возвращает true, если индекс присутствует в таблице.
        /// </summary>
        public bool Exists
        {
            get
            {
                if (!__init_Exists)
                {
                    _Exists = this.ExistingIndex != null;
                    __init_Exists = true;
                }
                return _Exists;
            }
        }

        /// <summary>
        /// Проверяет существование индекса в таблице. Генерирует исключение в случае отсутствия инлекса в таблице.
        /// </summary>
        protected internal void CheckExists()
        {
            if (!this.Exists)
                throw new Exception(string.Format("Операция доступна только для существующего индекса [{0}] в таблице {1}.", this.Name, this.Table.Name));
        }

        /// <summary>
        /// Проверяет отсутствие индекса в таблице. Генерирует исключение в случае наличия индекса в таблице.
        /// </summary>
        protected internal void CheckNotExists()
        {
            if (this.Exists)
                throw new Exception(string.Format("Операция доступна только для несозданного индекса [{0}] в таблице {1}.", this.Name, this.Table.Name));
        }

        /// <summary>
        /// Сбрасывает значения свойств существующего индекса.
        /// </summary>
        internal virtual void ResetExistingIndex()
        {
            this.__init_ExistingIndex = false;
            this.__init_Exists = false;
            this.__init_NameChanged = false;
            this.__init_RecreateRequired = false;
        }

        #endregion


        #region RecreateRequired

        private bool __init_RecreateRequired = false;
        private bool _RecreateRequired;
        /// <summary>
        /// Возвращает true, если параметры индекса изменились по сравнению с существующим индексом таблицы, и требуется пересоздание индекса.
        /// </summary>
        public bool RecreateRequired
        {
            get
            {
                if (!__init_RecreateRequired)
                {
                    if (this.Exists)
                    {
                        //если набор столбцов изменился, то требуется пересоздать индекс.
                        _RecreateRequired = !this.ColumnsEqual(this.ExistingIndex);

                        //если набор столбцов не изменился, проверяем параметры столбцов.
                        if (!_RecreateRequired)
                        {
                            foreach (DBIndexColumn indexColumn in this.Columns)
                            {
                                if (indexColumn.Column.DependendIndexesRecreateRequired)
                                {
                                    _RecreateRequired = true;
                                    break;
                                }
                            }
                        }
                    }
                    __init_RecreateRequired = true;
                }
                return _RecreateRequired;
            }
        }

        /// <summary>
        /// Проверяет, требуется ли пересоздание индекса. В случае если пересоздание индекса не требуется, генерирует исключение.
        /// </summary>
        internal void CheckRecreateRequired()
        {
            if (!this.RecreateRequired)
                throw new Exception(string.Format("Операция доступа только при необходимости пересоздания индекса [{0}] в таблице {1}.", this.Name, this.Table.Name));
        }

        #endregion


        #region Columns

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
                    DBIndexColumn column = this.Columns[i];
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
        /// Сбрасывает инициализацию коллекции столбцов.
        /// </summary>
        private void ResetColumns()
        {
            this.__init_Columns = false;
        }

        private bool __init_Columns = false;
        private DBCollection<DBIndexColumn> _Columns;
        /// <summary>
        /// Коллекция столбцов индекса.
        /// </summary>
        public DBCollection<DBIndexColumn> Columns
        {
            get
            {
                if (!__init_Columns)
                {
                    _Columns = new DBCollection<DBIndexColumn>(this.ColumnsByName.Values);
                    __init_Columns = true;
                }
                return _Columns;
            }
        }

        /// <summary>
        /// Возвращает значение флага инициализации свойства ColumnsByName.
        /// </summary>
        private bool ColumnsByNameInited
        {
            get { return this.__init_ColumnsByName; }
        }

        private bool __init_ColumnsByName = false;
        private Dictionary<string, DBIndexColumn> _ColumnsByName;
        /// <summary>
        /// Словарь столбцов индекса по имени.
        /// </summary>
        private Dictionary<string, DBIndexColumn> ColumnsByName
        {
            get
            {
                if (!__init_ColumnsByName)
                {
                    _ColumnsByName = new Dictionary<string, DBIndexColumn>();

                    foreach (DBIndexColumnSchema indexColumnSchema in this.Schema.Columns)
                    {
                        //проверяем, что столбец содержится в схеме таблицы.
                        if (this.Table.Schema.ContainsColumn(indexColumnSchema.Name, true))
                        {
                            if (!_ColumnsByName.ContainsKey(indexColumnSchema.NameLow))
                            {
                                DBIndexColumn indexColumn = new DBIndexColumn(indexColumnSchema, this);
                                _ColumnsByName.Add(indexColumnSchema.NameLow, indexColumn);
                            }
                        }
                    }
                    __init_ColumnsByName = true;
                }
                return _ColumnsByName;
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
                }
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
        public DBIndexColumn GetColumn(string columnName)
        {
            if (string.IsNullOrEmpty(columnName))
                throw new ArgumentNullException("columnName");

            DBIndexColumn column = null;
            if (this.ColumnsByName.ContainsKey(columnName.ToLower()))
                column = this.ColumnsByName[columnName.ToLower()];
            return column;
        }

        /// <summary>
        /// Возвращает текст перченя столбцов индекса.
        /// </summary>
        /// <returns></returns>
        internal string GetColumnsQuery()
        {
            StringBuilder stIndexColumns = new StringBuilder();
            foreach (DBIndexColumn indexColumn in this.Columns)
            {
                //проверяем, что столбец не требует переименования
                indexColumn.Column.CheckRenameNotRequired();

                //формируем перечень столбцов индекса.
                if (stIndexColumns.Length > 0)
                    stIndexColumns.Append(",");
                stIndexColumns.AppendFormat(@"
                        {0}", indexColumn.Definition);
            }
            if (stIndexColumns.Length == 0)
                throw new Exception(string.Format("Не удалось получить набор столбцов для индекса [{0}].", this.Name));

            return stIndexColumns.ToString();
        }

        #endregion


        /// <summary>
        /// Возвращает текст запроса создания индекса.
        /// </summary>
        internal abstract string GetInitQuery();

        /// <summary>
        /// Возвращает запрос переименования индекса.
        /// </summary>
        /// <returns></returns>
        internal string GetRenameQuery()
        {
            string query = @"
exec sp_rename N'[dbo].[{TableNameText}].[{OriginalNameText}]', N'{NameText}', N'INDEX'"
                .ReplaceKey("TableNameText", this.ExistingIndex.Table.Name.QueryEncode())
                .ReplaceKey("OriginalNameText", this.ExistingIndex.Name.QueryEncode())
                .ReplaceKey("NameText", this.Name.QueryEncode())
                ;

            return query;
        }

        /// <summary>
        /// Создает индекс в таблице.
        /// </summary>
        internal void Create()
        {
            //проверяем существование таблицы
            this.Table.CheckExists();

            //проверяем, что название таблицы не изменилось и переименование перед операцией не требуется.
            this.Table.CheckRenameNotRequired();

            //проверяем, что префикс таблицы не изменился
            this.Table.CheckPrefixChangeNotRequired();

            //проверяем, что индекс не существует.
            this.CheckNotExists();

            //добавляем физически индекс в таблицу.
            string query = this.GetInitQuery();
            this.Table.DataAdapter.ExecuteQuery(query);

            //сбрасываем флаги инициализации индексов.
            this.Table.ResetExistingIndexes();
        }

        /// <summary>
        /// Пересоздает индекс, если изменение параметров индекса требует его пересоздания.
        /// </summary>
        internal void Recreate()
        {
            //проверяем существование таблицы
            this.Table.CheckExists();

            //проверяем, что название таблицы не изменилось и переименование перед операцией не требуется.
            this.Table.CheckRenameNotRequired();

            //проверяем, что префикс таблицы не изменился
            this.Table.CheckPrefixChangeNotRequired();

            //проверяем, что индекс существует
            this.CheckExists();

            //проверяем, что не требуется переименование индекса
            this.CheckRenameNotRequired();

            //проверяем, что требуется пересоздание индекса.
            this.CheckRecreateRequired();

            //пересоздаем индекс под актуальным именем.
            StringBuilder recreateQuery = new StringBuilder();
            recreateQuery.Append(this.ExistingIndex.GetDropQuery());
            recreateQuery.Append(this.GetInitQuery());
            this.Table.DataAdapter.ExecuteQuery(recreateQuery.ToString());

            //сбрасываем флаги инициализации индексов.
            this.Table.ResetExistingIndexes();
        }

        /// <summary>
        /// Переименовывает индекс при необходимости.
        /// </summary>
        internal void Rename()
        {
            //проверяем, что схема таблицы содержит данный индекс.
            this.Table.Schema.ContainsIndex(this.Schema.RelativeName, true);

            //проверяем существование таблицы
            this.Table.CheckExists();

            //проверяем, что индекс существует
            this.CheckExists();

            //проверяем, что требуется переименование индекса
            this.CheckRenameRequired();

            //переименовываем индекс.
            string renameQuery = this.GetRenameQuery();
            this.Table.DataAdapter.ExecuteQuery(renameQuery);

            //сбрасываем флаги инициализации индексов.
            this.Table.ResetExistingIndexes();
        }

        /// <summary>
        /// Строковое представление экземпляра класса DBIndex.
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
