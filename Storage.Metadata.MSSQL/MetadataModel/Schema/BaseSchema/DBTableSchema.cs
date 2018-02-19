using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Storage.Metadata.MSSQL
{
    /// <summary>
    /// Представляет схему таблиц однородных данных.
    /// </summary>
    public abstract class DBTableSchema
    {
        /// <summary>
        /// Создает экземпляр DBTableSchema.
        /// </summary>
        /// <param name="schemaAdapter">Адаптер схемы таблицы.</param>
        public DBTableSchema(DBTableSchemaAdapter schemaAdapter)
        {
            if (schemaAdapter == null)
                throw new ArgumentNullException("schemaAdapter");

            this.SchemaAdapter = schemaAdapter;
            this.SchemaAdapter.Logger.WriteFormatMessage("Инициализация объекта схемы таблицы '{0}'", this.TableName);
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
        /// Базовое имя таблицы.
        /// </summary>
        protected string TableName
        {
            get { return this.SchemaAdapter.TableName; }
        }


        #region IdentityColumn

        private bool __init_IdentityColumnSupported = false;
        /// <summary>
        /// Возвращает true, если схема таблицы поддерживает инкрементный столбец-идентификатор.
        /// </summary>
        private bool _IdentityColumnSupported;
        internal bool IdentityColumnSupported
        {
            get
            {
                if (!__init_IdentityColumnSupported)
                {
                    _IdentityColumnSupported = this.InitIdentityColumnSupported();
                    __init_IdentityColumnSupported = true;
                }
                return _IdentityColumnSupported;
            }
        }

        /// <summary>
        /// Инициализирует значение свойства IdentityColumnSupported.
        /// </summary>
        /// <returns></returns>
        protected internal virtual bool InitIdentityColumnSupported()
        {
            return !this.SchemaAdapter.IsPermanentSchema;
        }

        /// <summary>
        /// Инициализирует название автоинкрементного столбца-идентификатора таблицы.
        /// </summary>
        protected abstract string InitIdentityColumnName();

        private bool __init_IdentityColumn = false;
        private DBColumnSchema _IdentityColumn;
        /// <summary>
        /// Автоинкрементный столбец-идентификатор базовой таблицы.
        /// </summary>
        public DBColumnSchema IdentityColumn
        {
            get
            {
                if (!__init_IdentityColumn)
                {
                    if (this.IdentityColumnSupported)
                    {
                        //проверяем, что схема таблицы является управляемой.
                        this.SchemaAdapter.CheckManagedSchema();

                        string identityColumnName = this.InitIdentityColumnName();
                        _IdentityColumn = this.GetColumn(identityColumnName, true);

                        if (_IdentityColumn == null)
                            throw new Exception(string.Format("Отсутствует столбец-идентификатор в схеме таблицы {0}.", this.TableName));
                        if (!(_IdentityColumn.Type == SqlDbType.Int || _IdentityColumn.Type == SqlDbType.BigInt))
                            throw new Exception(string.Format("Столбец-идентификатор таблицы {0} должен иметь тип int или bigint.", _IdentityColumn.Definition));
                        if (_IdentityColumn.IsNullable)
                            throw new Exception(string.Format("Столбец-идентификатор таблицы {0} не может поддерживать установку пустых значений.", _IdentityColumn.Definition));

                        //проверяем, что столбец присутствует в коллекции всех столбцов.
                        DBColumnSchema checkColumn = this.GetColumn(_IdentityColumn.Name, true);
                        if (checkColumn != _IdentityColumn)
                            throw new Exception(string.Format("Экземпляр столбца-идентификатора [{0}] отличается от экземпляра одноименного столбца в коллекции столбцов таблицы.", _IdentityColumn.Name));
                    }

                    __init_IdentityColumn = true;
                }
                return _IdentityColumn;
            }
        }

        /// <summary>
        /// Возвращает true, если название столбца является названием столбца автоинкремента.
        /// </summary>
        /// <param name="columnName">Название столбца.</param>
        /// <returns></returns>
        internal bool IsIdentityColumn(string columnName)
        {
            if (string.IsNullOrEmpty(columnName))
                throw new ArgumentNullException("columnName");
            bool result =
                this.IdentityColumn != null &&
                this.IdentityColumn.NameLow == columnName.ToLower();
            return result;
        }

        /// <summary>
        /// Инициализирует значение свойства InitialColumn.
        /// </summary>
        /// <returns></returns>
        protected internal virtual DBColumnSchema InitInitialColumn()
        {
            DBColumnSchema initialColumn = null;
            if (this.IdentityColumn != null)
                initialColumn = this.IdentityColumn;
            else if (this.Columns.Count > 0)
                initialColumn = this.Columns[0];
            return initialColumn;
        }

        private bool __init_InitialColumn = false;
        private DBColumnSchema _InitialColumn;
        /// <summary>
        /// Первичный столбец таблицы, создаваемый вместе с таблицей.
        /// Данный столбец обязательно должен присутствовать в схеме таблицы, поскольку таблица не может быть создана без хотя бы одного столбца.
        /// По умолчанию данным столбцом, при поддержки автоинкремента таблицы, является столбец-автоинкремент, 
        /// в ином случае первичным столбцом является первый столбец из коллекции столбцов схемы таблицы.
        /// Первичный столбец автоматически является системным столбцом таблицы.
        /// </summary>
        public DBColumnSchema InitialColumn
        {
            get
            {
                if (!__init_InitialColumn)
                {
                    //проверяем, что схема таблицы является управляемой.
                    this.SchemaAdapter.CheckManagedSchema();

                    _InitialColumn = this.InitInitialColumn();

                    if (_InitialColumn == null)
                        throw new Exception(string.Format("Отсутсвует хотя бы один столбец в схеме таблицы [{0}], необходимый для создания таблицы.", this.TableName));

                    //проверяем, что столбец присутствует в коллекции всех столбцов.
                    DBColumnSchema checkColumn = this.GetColumn(_InitialColumn.Name, true);
                    if (checkColumn != _InitialColumn)
                        throw new Exception(string.Format("Экземпляр первичного столбца [{0}] отличается от экземпляра одноименного столбца в коллекции столбцов таблицы.", _IdentityColumn.Name));

                    __init_InitialColumn = true;
                }
                return _InitialColumn;
            }
        }

        #endregion


        #region PrimaryKey

        private bool __init_PrimaryKeySupported = false;
        private bool _PrimaryKeySupported;
        /// <summary>
        /// Возвращает true, если схема таблицы поддерживает первичный ключ.
        /// </summary>
        internal bool PrimaryKeySupported
        {
            get
            {
                if (!__init_PrimaryKeySupported)
                {
                    _PrimaryKeySupported = this.InitPrimaryKeySupported();
                    __init_PrimaryKeySupported = true;
                }
                return _PrimaryKeySupported;
            }
        }

        /// <summary>
        /// Инициализирует значение свойства PrimaryKeySupported.
        /// </summary>
        /// <returns></returns>
        protected internal virtual bool InitPrimaryKeySupported()
        {
            return !this.SchemaAdapter.IsPermanentSchema;
        }

        /// <summary>
        /// Проверяет, поддерживает ли схема таблицы первичный ключ. Генерирует исключение в случае отсутствия поддержки первичного ключа.
        /// </summary>
        internal void CheckPrimaryKeySupported()
        {
            if (!this.PrimaryKeySupported)
                throw new Exception(string.Format("Операция недоступна, поскольку схема таблицы {0} не поддерживает первичный ключ.", this.TableName));
        }

        private bool __init_DefaultPrimaryKey = false;
        private DBIndexSchema _DefaultPrimaryKey;
        /// <summary>
        /// Первичный ключ по умолчанию. Создается на основе столбца автоинкремента.
        /// </summary>
        private DBIndexSchema DefaultPrimaryKey
        {
            get
            {
                if (!__init_DefaultPrimaryKey)
                {
                    //если таблица поддерживает автоинкремент, инициализируем первичный ключ по умолчанию со столбцом автоинкремента.
                    if (this.PrimaryKeySupported && this.IdentityColumn != null)
                    {
                        _DefaultPrimaryKey = new DBGenericIndexSchema(new DBGenericIndexSchema.Properties()
                        {
                            RelativeName = MetadataConsts.DefaultIndexNames.PrimaryKey,
                            Columns = new List<DBGenericIndexColumnSchema.Properties>() 
                            { 
                                new DBGenericIndexColumnSchema.Properties() 
                                { 
                                    Name = this.IdentityColumn.Name 
                                } 
                            }
                        }, this.SchemaAdapter);
                    }

                    __init_DefaultPrimaryKey = true;
                }
                return _DefaultPrimaryKey;
            }
        }

        /// <summary>
        /// Инициирует обращение к коллекции столбцов.
        /// </summary>
        internal virtual void PreInitColumnsChange()
        {
            object checkObj = this.Columns;
        }

        /// <summary>
        /// Инициализирует кластеризованный индекс, представляющий первичный ключ таблицы.
        /// При поддержке автоинкремента таблицы, возвращает кластеризованный индекс, в состав которого входит столбец автоинкремента.
        /// </summary>
        /// <returns></returns>
        protected internal virtual string InitPrimaryKeyRelativeName()
        {
            return null;
        }

        private bool __init_PrimaryKeyRelativeNameIniting = false;
        private string _PrimaryKeyRelativeNameIniting;
        /// <summary>
        /// Инициализационное значение относительного названия первичного ключа.
        /// </summary>
        private string PrimaryKeyRelativeNameIniting
        {
            get
            {
                if (!__init_PrimaryKeyRelativeNameIniting)
                {
                    if (this.PrimaryKeySupported)
                        _PrimaryKeyRelativeNameIniting = this.InitPrimaryKeyRelativeName();
                    __init_PrimaryKeyRelativeNameIniting = true;
                }
                return _PrimaryKeyRelativeNameIniting;
            }
        }

        private bool __init_PrimaryKey = false;
        private DBIndexSchema _PrimaryKey;
        /// <summary>
        /// Кластеризованный индекс, представляющий первичный ключ таблицы.
        /// </summary>
        public DBIndexSchema PrimaryKey
        {
            get
            {
                if (!__init_PrimaryKey)
                {
                    if (this.PrimaryKeySupported)
                    {
                        //проверяем, что схема таблицы является управляемой.
                        this.SchemaAdapter.CheckManagedSchema();

                        if (!string.IsNullOrEmpty(this.PrimaryKeyRelativeNameIniting))
                            _PrimaryKey = this.GetIndex(this.PrimaryKeyRelativeNameIniting, true);
                        else if (this.DefaultPrimaryKey != null)
                            _PrimaryKey = this.DefaultPrimaryKey;

                        if (_PrimaryKey == null)
                            throw new Exception(string.Format("Отсутствует первичный ключ в схеме таблицы [{0}].", this.TableName));

                        //проверяем, что столбцы первичного ключа являются системными, т.к. первичный ключ является системным индеском и создается вместе с таблицей
                        //а значит и столбцы, входящие в его состав должны создаваться вместе с таблицей.
                        foreach (DBIndexColumnSchema indexColumn in _PrimaryKey.Columns)
                        {
                            if (!this.IsSystemColumn(indexColumn.Name))
                                throw new Exception(string.Format("Столбец [{0}] первичного ключа таблицы {1} должен быть помечен как системный.", indexColumn.Name, this.TableName));
                        }
                    }
                    __init_PrimaryKey = true;
                }
                return _PrimaryKey;
            }
        }

        private bool __init_HasDefaultPrimaryKey = false;
        private bool _HasDefaultPrimaryKey;
        /// <summary>
        /// Возвращает true, таблица использует первичный ключ по умолчанию.
        /// </summary>
        internal bool HasDefaultPrimaryKey
        {
            get
            {
                if (!__init_HasDefaultPrimaryKey)
                {
                    _HasDefaultPrimaryKey =
                        this.PrimaryKeySupported &&
                        string.IsNullOrEmpty(this.PrimaryKeyRelativeNameIniting) &&
                        this.DefaultPrimaryKey != null;

                    __init_HasDefaultPrimaryKey = true;
                }
                return _HasDefaultPrimaryKey;
            }
        }

        /// <summary>
        /// Возвращает true, если относительное название индекса является названием индекса первичного ключа.
        /// </summary>
        /// <param name="indexRelativeName">Относительное название индекса.</param>
        /// <returns></returns>
        internal bool IsPrimaryKey(string indexRelativeName)
        {
            if (string.IsNullOrEmpty(indexRelativeName))
                throw new ArgumentNullException("indexRelativeName");
            bool result =
                this.PrimaryKey != null &&
                this.PrimaryKey.RelativeNameLow == indexRelativeName.ToLower();
            return result;
        }

        #endregion


        #region Columns

        private bool __init_Columns = false;
        private DBCollection<DBColumnSchema> _Columns;
        /// <summary>
        /// Коллекция столбцов схемы таблицы.
        /// </summary>
        public DBCollection<DBColumnSchema> Columns
        {
            get
            {
                if (!__init_Columns)
                {
                    _Columns = new DBCollection<DBColumnSchema>(this.ColumnsByName.Values);
                    __init_Columns = true;
                }
                return _Columns;
            }
        }

        /// <summary>
        /// Возвращает true, коллекция столбцов схемы таблицы инициализирована.
        /// </summary>
        internal bool ColumnsInited
        {
            get { return this.__init_ColumnsByName; }
        }

        /// <summary>
        /// Инициализирует коллекцию столбцов схемы таблицы, по которой инициализируется коллекция Columns схемы таблицы.
        /// </summary>
        protected internal abstract ICollection<DBColumnSchema> InitColumns();

        private bool __init_ColumnsByName = false;
        private Dictionary<string, DBColumnSchema> _ColumnsByName;
        /// <summary>
        /// Словарь столбцов схемы таблицы.
        /// </summary>
        private Dictionary<string, DBColumnSchema> ColumnsByName
        {
            get
            {
                if (!__init_ColumnsByName)
                {
                    _ColumnsByName = new Dictionary<string, DBColumnSchema>();
                    ICollection<DBColumnSchema> columns = this.InitColumns();

                    if (columns != null)
                    {
                        foreach (DBColumnSchema column in columns)
                        {
                            if (column.SchemaAdapter != this.SchemaAdapter)
                                throw new Exception(string.Format("Схема столбца {0} принадлежит экземпляру адаптера схемы таблицы, отличающемуся от экземпляра адаптера схемы данной таблицы.", column.Name));
                            if (!_ColumnsByName.ContainsKey(column.NameLow))
                                _ColumnsByName.Add(column.NameLow, column);
                            else
                                throw new Exception(string.Format("Столбец с названием [{0}] уже добавлен в схему таблицы {1}.", column.Name, this.TableName));
                        }
                    }

                    if (_ColumnsByName.Count == 0 && !this.SchemaAdapter.IsPermanentSchema)
                        throw new Exception(string.Format("Отсутствуют столбцы в схеме таблицы [{0}].", this.TableName));

                    __init_ColumnsByName = true;
                }
                return _ColumnsByName;
            }
        }

        /// <summary>
        /// Сбрасывает флаг инициализации коллекции столбцов схемы таблицы.
        /// Используется при добавлении и удалении столбца из схемы таблицы.
        /// </summary>
        protected virtual void ResetColumns()
        {
            this.__init_Columns = false;
        }

        /// <summary>
        /// Сбрасывает инициализацию всех коллекций столбцов, в том числе и системной коллекции столбцов.
        /// </summary>
        protected virtual void ResetColumnsFull()
        {
            this.ResetColumns();
            this.__init_IdentityColumn = false;
            this.__init_InitialColumn = false;
            this.__init_ColumnsByName = false;
            this.__init_SystemColumns = false;
            this.__init_SystemColumnsByName = false;

            this.SchemaAdapter.Logger.WriteFormatMessage("Сброс инициализации всех коллекций столбцов в объекте схемы таблицы '{0}'", this.TableName);
        }


        /// <summary>
        /// Возвращает true, если столбец содержится в схеме таблицы.
        /// </summary>
        /// <param name="columnName">Название столбца.</param>
        /// <returns></returns>
        public bool ContainsColumn(string columnName)
        {
            return this.ContainsColumn(columnName, false);
        }

        /// <summary>
        /// Возвращает true, если столбец содержится в схеме таблицы.
        /// </summary>
        /// <param name="columnName">Название столбца.</param>
        /// <param name="throwNotFoundException">При указанном значении true, генерирует исключение в случае отсутствия столбца в схеме таблицы.</param>
        /// <returns></returns>
        public bool ContainsColumn(string columnName, bool throwNotFoundException)
        {
            if (string.IsNullOrEmpty(columnName))
                throw new ArgumentNullException("columnName");
            bool result = this.ColumnsByName.ContainsKey(columnName.ToLower());
            if (!result && throwNotFoundException)
                throw new Exception(string.Format("Столбец [{0}] не содержится в схеме таблицы {1}.", columnName, this.TableName));
            return result;
        }

        /// <summary>
        /// Возвращает столбец таблицы по названию.
        /// В случае отсутствия столбца в схеме таблицы возвращает null.
        /// </summary>
        /// <param name="columnName">Название столбца.</param>
        /// <returns></returns>
        public DBColumnSchema GetColumn(string columnName)
        {
            return this.GetColumn(columnName, false);
        }

        /// <summary>
        /// Возвращает столбец схемы таблицы по названию.
        /// </summary>
        /// <param name="columnName">Название столбца.</param>
        /// <param name="throwNotFoundException">При указанном значении true, генерирует исключение в случае отсутствия столбца в схеме таблицы.</param>
        /// <returns></returns>
        public DBColumnSchema GetColumn(string columnName, bool throwNotFoundException)
        {
            if (string.IsNullOrEmpty(columnName))
                throw new ArgumentNullException("columnName");

            string columnNameLow = columnName.ToLower();
            DBColumnSchema column = null;
            if (this.ColumnsByName.ContainsKey(columnNameLow))
                column = this.ColumnsByName[columnNameLow];

            if (column == null && throwNotFoundException)
                throw new Exception(string.Format("Не удалось получить столбец [{0}] в схеме таблицы {1}.", columnName, this.TableName));

            return column;
        }

        /// <summary>
        /// Добавляет столбец в схему таблицы.
        /// </summary>
        /// <param name="column">Схема столбца.</param>
        internal virtual void AddColumnInternal(DBColumnSchema column)
        {
            if (column == null)
                throw new ArgumentNullException("column");

            this.SchemaAdapter.Logger.WriteFormatMessage("Добавление столбца в схему таблицы '{0}'", this.TableName);

            try
            {
                //проверяем, что столбец создан из данной таблицы.
                if (column.SchemaAdapter != this.SchemaAdapter)
                    throw new Exception("Столбец создан из экземпляра DBTableSchemaAdapter, отличного от экземпляра DBTableSchemaAdapter схемы таблицы.");

                //проверяем, что столбец не содержится в схеме таблицы.
                if (this.ContainsColumn(column.Name))
                    throw new Exception("Столбец уже содержится в схеме таблицы.");

                //проверяем, что столбец не является системным.
                if (this.IsSystemColumn(column.Name))
                    throw new Exception(string.Format("Добавление столбца с системным названием [{0}] запрещено.", column.Name));

                //проверяем корректность схемы столбца.
                column.Validate();

                //добавляем столбец к словарю столбцов.
                this.ColumnsByName.Add(column.NameLow, column);

                //сбрасываем инициализацию коллекции столбцов.
                this.ResetColumns();

                //добавляем столбец схемы в экземпляры таблиц.
                foreach (DBTable tableInstance in this.TableInstances)
                    tableInstance.AddSchemaColumn(column);

                this.SchemaAdapter.Logger.WriteFormatMessage("Добавление столбца в схему таблицы '{0}' завершено.", this.TableName);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Ошибка при добавлении столбца [{0}] в схему таблицы {1}.", column.Name, this.TableName), ex);
            }
        }

        /// <summary>
        /// Удаляет столбец из схемы таблицы.
        /// </summary>
        /// <param name="columnName">Название удаляемого столбца.</param>
        internal virtual void DeleteColumnInternal(string columnName)
        {
            if (string.IsNullOrEmpty(columnName))
                throw new ArgumentNullException("columnName");

            this.SchemaAdapter.Logger.WriteFormatMessage("Удаление столбца из схемы таблицы: '{0}'", this.TableName);

            try
            {
                //проверяем, что столбец не содержится в схеме таблицы.
                if (!this.ContainsColumn(columnName))
                    throw new Exception("Удаляемый столбец должен содержаться в схеме таблицы.");

                //проверяем, что столбец не является системным.
                if (this.IsSystemColumn(columnName))
                    throw new Exception(string.Format("Удаление столбца с системным названием [{0}] запрещено.", columnName));

                //удаляем столбец из словаря столбцов.
                this.ColumnsByName.Remove(columnName.ToLower());

                //сбрасываем инициализацию коллекции столбцов.
                this.ResetColumns();

                //удаляем столбец схемы из экземпляров таблиц.
                foreach (DBTable tableInstance in this.TableInstances)
                    tableInstance.DeleteSchemaColumn(columnName);

                //удаляем индексы, в которые был включен только один удаляемый столбец 
                //или удаляем этот столбец из индекса, если столбцов в индексе более одного.
                List<DBIndexSchema> indexesToDelete = new List<DBIndexSchema>();
                foreach (DBIndexSchema index in this.Indexes)
                {
                    if (index.ContainsColumn(columnName))
                    {
                        //если удаляемый столбец - единственный столбец в индексе - удаляем целиком индекс
                        //в ином случае, удаляем столбец из индекса.
                        if (index.Columns.Count == 1)
                        {
                            indexesToDelete.Add(index);
                            foreach (DBTable tableInstance in this.TableInstances)
                                tableInstance.ResetExistingIndexes();
                        }
                        else
                        {
                            index.DeleteColumn(columnName);

                            //удаляем столбец схемы из соответствующего индекса экземпляров таблиц
                            foreach (DBTable tableInstance in this.TableInstances)
                            {
                                DBIndex indexInstance = tableInstance.GetIndex(index.RelativeName, true);
                                indexInstance.DeleteSchemaColumn(columnName);
                                tableInstance.ResetExistingIndexes();
                            }
                        }
                    }
                }

                //удаляем индексы, которые лишились удаляемого столбца.
                foreach (DBIndexSchema indexToDelete in indexesToDelete)
                {
                    this.DeleteIndexSealed(indexToDelete.RelativeName);

                    //удаляем индекс схемы из экземпляров таблиц.
                    foreach (DBTable tableInstance in this.TableInstances)
                        tableInstance.DeleteSchemaIndex(indexToDelete.RelativeName);
                }

                //сбрасываем коллекцию индексов, если они удалялись
                if (indexesToDelete.Count > 0)
                    this.ResetIndexes();

                this.SchemaAdapter.Logger.WriteFormatMessage("Удаление столбца из схемы таблицы: '{0}' завершено.", this.TableName);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Ошибка при удалении столбца [{0}] из схемы таблицы {1}.", columnName, this.TableName), ex);
            }
        }

        #endregion


        #region SystemColumns

        private bool __init_SystemColumns = false;
        private DBCollection<DBColumnSchema> _SystemColumns;
        /// <summary>
        /// Коллекция системных столбцов схемы таблицы, добавляемых в таблицу при ее инициализации, в том числе и при создании.
        /// </summary>
        public DBCollection<DBColumnSchema> SystemColumns
        {
            get
            {
                if (!__init_SystemColumns)
                {
                    _SystemColumns = new DBCollection<DBColumnSchema>(this.SystemColumnsByName.Values);
                    __init_SystemColumns = true;
                }
                return _SystemColumns;
            }
        }

        /// <summary>
        /// Инициализирует коллекцию системных столбцов таблицы. Данные столбцы создаются вместе с таблицей.
        /// Остальные столбцы могут быть добавлены по отдельности, либо при группой инициализации столбцов.
        /// </summary>
        protected internal virtual ICollection<DBColumnSchema> InitSystemColumns()
        {
            return null;
        }

        private bool __init_SystemColumnsByName = false;
        private Dictionary<string, DBColumnSchema> _SystemColumnsByName;
        /// <summary>
        /// Словарь системных столбцов схемы таблицы.
        /// </summary>
        private Dictionary<string, DBColumnSchema> SystemColumnsByName
        {
            get
            {
                if (!__init_SystemColumnsByName)
                {
                    //проверяем, что схема таблицы является управляемой.
                    this.SchemaAdapter.CheckManagedSchema();

                    _SystemColumnsByName = new Dictionary<string, DBColumnSchema>();

                    //добавляем первичный столбец таблицы в качестве системного. Если в схеме присутсвует автоинкремент, 
                    //то этим системным столбцом становится автоинкремент.
                    _SystemColumnsByName.Add(this.InitialColumn.NameLow, this.InitialColumn);

                    //получаем пользовательскую коллекцию системных столбцов.
                    ICollection<DBColumnSchema> columns = this.InitSystemColumns();

                    if (columns != null)
                    {
                        foreach (DBColumnSchema column in columns)
                        {
                            //пропускаем первичный столбец
                            if (column.NameLow == this.InitialColumn.NameLow)
                                continue;

                            //проверяем, что столбец присутствует в коллекции всех столбцов.
                            DBColumnSchema checkColumn = this.GetColumn(column.Name, true);
                            if (checkColumn != column)
                                throw new Exception(string.Format("Экземпляр системного столбца [{0}] отличается от экземпляра одноименного столбца в коллекции столбцов таблицы.", column.Name));

                            //добавляем столбец в словарь системных столбцов.
                            if (!_SystemColumnsByName.ContainsKey(column.NameLow))
                                _SystemColumnsByName.Add(column.NameLow, column);
                            else
                                throw new Exception(string.Format("Столбец с названием [{0}] уже добавлен в коллекцию системных столбцов схемы таблицы {1}.", column.Name, this.TableName));
                        }
                    }

                    //проверяем наличие системных столбцов в таблице.
                    if (_SystemColumnsByName.Count == 0)
                        throw new Exception(string.Format("Отсутствуют системные столбцы в схеме таблицы [{0}].", this.TableName));

                    __init_SystemColumnsByName = true;
                }
                return _SystemColumnsByName;
            }
        }

        /// <summary>
        /// Возращает true, если столбец с заданным названием является системным для данной таблицы.
        /// </summary>
        /// <param name="columnName">Название столбца.</param>
        /// <returns></returns>
        internal bool IsSystemColumn(string columnName)
        {
            return this.IsSystemColumn(columnName, false);
        }

        /// <summary>
        /// Возращает true, если столбец с заданным названием является системным для данной таблицы.
        /// </summary>
        /// <param name="columnName">Название столбца.</param>
        /// <param name="throwNonSystemColumnException">При указанном значении true, генерирует исключение в случае если столбец не является системным столбцом схемы таблицы или если столбец не содержится в схеме таблицы.</param>
        /// <returns></returns>
        internal bool IsSystemColumn(string columnName, bool throwNonSystemColumnException)
        {
            if (string.IsNullOrEmpty(columnName))
                throw new ArgumentNullException("columnName");
            bool result = this.SystemColumnsByName.ContainsKey(columnName.ToLower());
            if (!result && throwNonSystemColumnException)
            {
                //сначала проверяем, что столбец содержится в схеме таблицы
                this.ContainsColumn(columnName, true);

                //теперь кидаем Exception для несистемного столбца.
                throw new Exception(string.Format("Столбец [{0}] не является системным столбцом таблицы {1}.", columnName, this.TableName));
            }
            return result;
        }

        #endregion


        #region Indexes

        private bool __init_Indexes = false;
        private DBCollection<DBIndexSchema> _Indexes;
        /// <summary>
        /// Коллекция индексов схемы таблицы.
        /// </summary>
        public DBCollection<DBIndexSchema> Indexes
        {
            get
            {
                if (!__init_Indexes)
                {
                    _Indexes = new DBCollection<DBIndexSchema>(this.IndexesByRelativeName.Values);
                    __init_Indexes = true;
                }
                return _Indexes;
            }
        }

        /// <summary>
        /// Возвращает true, коллекция индексов схемы таблицы инициализирована.
        /// </summary>
        internal bool IndexesInited
        {
            get { return this.__init_IndexesByRelativeName; }
        }

        /// <summary>
        /// Инициализирует коллекцию индексов схемы таблицы, по которой инициализируется коллекция Indexes схемы таблицы.
        /// </summary>
        protected internal abstract ICollection<DBIndexSchema> InitIndexes();


        private bool __init_IndexesByRelativeName = false;
        private Dictionary<string, DBIndexSchema> _IndexesByRelativeName;
        /// <summary>
        /// Словарь индексов схемы таблицы.
        /// </summary>
        private Dictionary<string, DBIndexSchema> IndexesByRelativeName
        {
            get
            {
                if (!__init_IndexesByRelativeName)
                {
                    _IndexesByRelativeName = new Dictionary<string, DBIndexSchema>();

                    //добавляем первичный ключ по умолчанию в коллекцию индексов, если не переопределен первичный ключ в свойства PrimaryKey.
                    if (this.HasDefaultPrimaryKey)
                        _IndexesByRelativeName.Add(this.DefaultPrimaryKey.RelativeNameLow, this.DefaultPrimaryKey);

                    ICollection<DBIndexSchema> indexes = this.InitIndexes();
                    if (indexes != null)
                    {
                        foreach (DBIndexSchema index in indexes)
                        {
                            if (index.SchemaAdapter != this.SchemaAdapter)
                                throw new Exception(string.Format("Схема индекса {0} принадлежит экземпляру адаптера схемы таблицы, отличающемуся от экземпляра адаптера схемы данной таблицы.", index.RelativeName));
                            if (!_IndexesByRelativeName.ContainsKey(index.RelativeNameLow))
                                _IndexesByRelativeName.Add(index.RelativeNameLow, index);
                            else
                                throw new Exception(string.Format("Индекс с названием [{0}] уже добавлен в схему таблицы {1}.", index.RelativeName, this.TableName));
                        }
                    }
                    __init_IndexesByRelativeName = true;
                }
                return _IndexesByRelativeName;
            }
        }

        /// <summary>
        /// Сбрасывает флаг инициализации коллекции индексов схемы таблицы.
        /// Используется при добавлении и удалении индекса из схемы таблицы.
        /// </summary>
        private void ResetIndexes()
        {
            this.__init_Indexes = false;
        }

        /// <summary>
        /// Сбрасывает инициализацию всех коллекций индексов, в том числе и системной коллекции индексов.
        /// </summary>
        protected void ResetIndexesFull()
        {
            this.ResetIndexes();
            this.__init_DefaultPrimaryKey = false;
            this.__init_IndexesByRelativeName = false;
            this.__init_PrimaryKey = false;
            this.__init_SystemIndexes = false;
            this.__init_SystemIndexesByName = false;

            this.SchemaAdapter.Logger.WriteFormatMessage("ResetIndexesFull схема таблицы: '{0}'", this.TableName);
        }

        /// <summary>
        /// Возвращает true, если индекс содержится в схеме таблицы.
        /// </summary>
        /// <param name="indexRelativeName">Относительное название индекса, уникальное в рамках таблицы.</param>
        /// <returns></returns>
        public bool ContainsIndex(string indexRelativeName)
        {
            return this.ContainsIndex(indexRelativeName, false);
        }

        /// <summary>
        /// Возвращает true, если индекс содержится в схеме таблицы.
        /// </summary>
        /// <param name="indexRelativeName">Относительное название индекса, уникальное в рамках таблицы.</param>
        /// <param name="throwNotFoundException">При указанном значении true, генерирует исключение в случае отсутствия индекса в схеме таблицы.</param>
        /// <returns></returns>
        public bool ContainsIndex(string indexRelativeName, bool throwNotFoundException)
        {
            if (string.IsNullOrEmpty(indexRelativeName))
                throw new ArgumentNullException("indexName");
            bool result = this.IndexesByRelativeName.ContainsKey(indexRelativeName.ToLower());
            if (!result && throwNotFoundException)
                throw new Exception(string.Format("Индекс [{0}] не содержится в схеме таблицы {1}.", indexRelativeName, this.TableName));
            return result;
        }

        /// <summary>
        /// Возвращает индекс таблицы по названию.
        /// В случае отсутствия индекса в схеме таблицы возвращает null.
        /// </summary>
        /// <param name="indexRelativeName">Относительное название индекса.</param>
        /// <returns></returns>
        public DBIndexSchema GetIndex(string indexRelativeName)
        {
            return this.GetIndex(indexRelativeName, false);
        }

        /// <summary>
        /// Возвращает индекс схемы таблицы по названию.
        /// </summary>
        /// <param name="indexRelativeName">Относительное название индекса.</param>
        /// <param name="throwNotFoundException">При указанном значении true, генерирует исключение в случае отсутствия индекса в схеме таблицы.</param>
        /// <returns></returns>
        public DBIndexSchema GetIndex(string indexRelativeName, bool throwNotFoundException)
        {
            if (string.IsNullOrEmpty(indexRelativeName))
                throw new ArgumentNullException("indexName");

            string indexNameLow = indexRelativeName.ToLower();
            DBIndexSchema index = null;
            if (this.IndexesByRelativeName.ContainsKey(indexNameLow))
                index = this.IndexesByRelativeName[indexNameLow];

            if (index == null && throwNotFoundException)
                throw new Exception(string.Format("Не удалось получить индекс [{0}] в схеме таблицы {1}.", indexRelativeName, this.TableName));

            return index;
        }

        /// <summary>
        /// Добавляет индекс в схему таблицы.
        /// </summary>
        /// <param name="index">Схема индекса.</param>
        internal virtual void AddIndexInternal(DBIndexSchema index)
        {
            if (index == null)
                throw new ArgumentNullException("index");

            try
            {
                //проверяем, что индекс создан из данной таблицы.
                if (index.SchemaAdapter != this.SchemaAdapter)
                    throw new Exception("Индекс создан из экземпляра DBTableSchemaAdapter, отличного от экземпляра DBTableSchemaAdapter схемы таблицы.");

                //проверяем, что индекс не содержится в схеме таблицы.
                if (this.ContainsIndex(index.RelativeName))
                    throw new Exception("Индекс уже содержится в схеме таблицы.");

                //добавляем индекс к словарю индексов.
                this.IndexesByRelativeName.Add(index.RelativeNameLow, index);

                //сбрасываем инициализацию коллекции индексов.
                this.ResetIndexes();

                //добавляем столбец схемы в экземпляры таблиц.
                foreach (DBTable tableInstance in this.TableInstances)
                    tableInstance.AddSchemaIndex(index);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Ошибка при добавлении индекса '{0}' в схему таблицы {1}.", index.RelativeName, this.TableName), ex);
            }
        }

        /// <summary>
        /// Удаляет индекс из схемы таблицы по относительному названию.
        /// Возвращает схему удаленного индекса, по которой можно удалить сущесвтующие индексы с соответветствующей схемой в таблицах.
        /// </summary>
        /// <param name="indexRelativeName">Относительное название индекса, уникальное в рамках схемы таблицы.</param>
        internal virtual DBIndexSchema DeleteIndexInternal(string indexRelativeName)
        {
            return this.DeleteIndexSealed(indexRelativeName);
        }

        /// <summary>
        /// Удаляет базовым способом индекс из схемы таблицы по относительному названию.
        /// Возвращает схему удаленного индекса, по которой можно удалить сущесвтующие индексы с соответветствующей схемой в таблицах.
        /// </summary>
        /// <param name="indexRelativeName">Относительное название индекса, уникальное в рамках схемы таблицы.</param>
        protected DBIndexSchema DeleteIndexSealed(string indexRelativeName)
        {
            if (string.IsNullOrEmpty(indexRelativeName))
                throw new ArgumentNullException("indexRelativeName");

            try
            {
                //проверяем, что индекс не содержится в схеме таблицы.
                if (!this.ContainsIndex(indexRelativeName))
                    throw new Exception("Удаляемый индекс должен содержаться в схеме таблицы.");

                //проверяем, что индекс не является системным.
                if (this.IsSystemIndex(indexRelativeName))
                    throw new Exception(string.Format("Удаление индекса с системным названием '{0}' запрещено.", indexRelativeName));

                //получаем удаляемый индекс
                DBIndexSchema deletedIndex = this.GetIndex(indexRelativeName, true);

                //добавляем индекс к словарю индексов.
                this.IndexesByRelativeName.Remove(deletedIndex.RelativeNameLow);

                //сбрасываем инициализацию коллекции индексов.
                this.ResetIndexes();

                //добавляем столбец схемы в экземпляры таблиц.
                foreach (DBTable tableInstance in this.TableInstances)
                    tableInstance.DeleteSchemaIndex(indexRelativeName);

                //возвращаем удаленный индекс.
                return deletedIndex;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Ошибка при удалении индекса '{0}' из схемы таблицы {1}.", indexRelativeName, this.TableName), ex);
            }
        }


        /// <summary>
        /// Сбрасывает инициализацию относительных названий индексов при переименовании индекса.
        /// </summary>
        internal virtual void ResetIndexRelativeNames()
        {
            //сбрасываем инициализацию всех столбцов.
            this.ResetIndexesFull();
        }

        #endregion


        #region SystemIndexes

        private bool __init_SystemIndexes = false;
        private DBCollection<DBIndexSchema> _SystemIndexes;
        /// <summary>
        /// Коллекция системных индексов схемы таблицы, добавляемых в таблицу при ее инициализации, в том числе и при создании.
        /// </summary>
        public DBCollection<DBIndexSchema> SystemIndexes
        {
            get
            {
                if (!__init_SystemIndexes)
                {
                    _SystemIndexes = new DBCollection<DBIndexSchema>(this.SystemIndexesByName.Values);
                    __init_SystemIndexes = true;
                }
                return _SystemIndexes;
            }
        }

        /// <summary>
        /// Инициализирует коллекцию системных индексов таблицы. Данные индексы создаются вместе с таблицей.
        /// Остальные индексы могут быть добавлены по отдельности, либо при группой инициализации индексов.
        /// </summary>
        protected internal virtual ICollection<DBIndexSchema> InitSystemIndexes()
        {
            return null;
        }

        private bool __init_SystemIndexesByName = false;
        private Dictionary<string, DBIndexSchema> _SystemIndexesByName;
        /// <summary>
        /// Словарь системных индексов схемы таблицы.
        /// </summary>
        private Dictionary<string, DBIndexSchema> SystemIndexesByName
        {
            get
            {
                if (!__init_SystemIndexesByName)
                {
                    //проверяем, что схема таблицы является управляемой.
                    this.SchemaAdapter.CheckManagedSchema();

                    _SystemIndexesByName = new Dictionary<string, DBIndexSchema>();

                    //добавляем первичный индекс таблицы в качестве системного. Если в схеме присутсвует автоинкремент, 
                    //то этим системным индексом становится автоинкремент.
                    if (this.PrimaryKey != null)
                        _SystemIndexesByName.Add(this.PrimaryKey.RelativeNameLow, this.PrimaryKey);

                    //получаем пользовательскую коллекцию системных индексов.
                    ICollection<DBIndexSchema> indexes = this.InitSystemIndexes();
                    if (indexes != null)
                    {
                        foreach (DBIndexSchema index in indexes)
                        {
                            //пропускаем первичный индекс
                            if (this.PrimaryKey != null && index.RelativeNameLow == this.PrimaryKey.RelativeNameLow)
                                continue;

                            //проверяем, что индекс присутствует в коллекции всех индексов.
                            DBIndexSchema checkIndex = this.GetIndex(index.RelativeName, true);
                            if (checkIndex != index)
                                throw new Exception(string.Format("Экземпляр системного индекса '{0}' отличается от экземпляра одноименного индекса в коллекции индексов таблицы.", index.RelativeName));

                            //добавляем индекс в словарь системных индексов.
                            if (!_SystemIndexesByName.ContainsKey(index.RelativeNameLow))
                                _SystemIndexesByName.Add(index.RelativeNameLow, index);
                            else
                                throw new Exception(string.Format("Индекс с названием '{0}' уже добавлен в коллекцию системных индексов схемы таблицы {1}.", index.RelativeName, this.TableName));
                        }
                    }

                    __init_SystemIndexesByName = true;
                }
                return _SystemIndexesByName;
            }
        }

        /// <summary>
        /// Возращает true, если индекс с заданным названием является системным для данной таблицы.
        /// </summary>
        /// <param name="indexName">Название индекса.</param>
        /// <returns></returns>
        internal bool IsSystemIndex(string indexName)
        {
            return this.IsSystemIndex(indexName, false);
        }

        /// <summary>
        /// Возращает true, если индекс с заданным названием является системным для данной таблицы.
        /// </summary>
        /// <param name="indexRelativeName">Название индекса.</param>
        /// <param name="throwNonSystemIndexException">При указанном значении true, генерирует исключение в случае если индекс не является системным индексом схемы таблицы или если индекс не содержится в схеме таблицы.</param>
        /// <returns></returns>
        internal bool IsSystemIndex(string indexRelativeName, bool throwNonSystemIndexException)
        {
            if (string.IsNullOrEmpty(indexRelativeName))
                throw new ArgumentNullException("indexName");
            bool result = this.SystemIndexesByName.ContainsKey(indexRelativeName.ToLower());
            if (!result && throwNonSystemIndexException)
            {
                //сначала проверяем, что индекс содержится в схеме таблицы
                this.ContainsIndex(indexRelativeName, true);

                //теперь кидаем Exception для несистемного индекса.
                throw new Exception(string.Format("Индекс '{0}' не является системным индексом таблицы {1}.", indexRelativeName, this.TableName));
            }
            return result;
        }

        #endregion


        #region Triggers

        private bool __init_Triggers = false;
        private DBCollection<DBTriggerSchema> _Triggers;
        /// <summary>
        /// Коллекция триггеров схемы таблицы.
        /// </summary>
        public DBCollection<DBTriggerSchema> Triggers
        {
            get
            {
                if (!__init_Triggers)
                {
                    _Triggers = new DBCollection<DBTriggerSchema>(this.TriggersByRelativeName.Values);
                    __init_Triggers = true;
                }
                return _Triggers;
            }
        }

        /// <summary>
        /// Инициализирует коллекцию триггеров схемы таблицы, по которой инициализируется коллекция Triggers схемы таблицы.
        /// </summary>
        protected internal abstract ICollection<DBTriggerSchema> InitTriggers();

        private bool __init_TriggersByRelativeName = false;
        private Dictionary<string, DBTriggerSchema> _TriggersByRelativeName;
        /// <summary>
        /// Словарь триггеров схемы таблицы.
        /// </summary>
        private Dictionary<string, DBTriggerSchema> TriggersByRelativeName
        {
            get
            {
                if (!__init_TriggersByRelativeName)
                {
                    _TriggersByRelativeName = new Dictionary<string, DBTriggerSchema>();

                    ICollection<DBTriggerSchema> triggers = this.InitTriggers();
                    if (triggers != null)
                    {
                        foreach (DBTriggerSchema trigger in triggers)
                        {
                            if (trigger.SchemaAdapter != this.SchemaAdapter)
                                throw new Exception(string.Format("Схема триггера {0} принадлежит экземпляру адаптера схемы таблицы, отличающемуся от экземпляра адаптера схемы данной таблицы.", trigger.RelativeName));
                            if (!_TriggersByRelativeName.ContainsKey(trigger.RelativeNameLow))
                                _TriggersByRelativeName.Add(trigger.RelativeNameLow, trigger);
                            else
                                throw new Exception(string.Format("Триггер с названием [{0}] уже добавлен в схему таблицы {1}.", trigger.RelativeName, this.TableName));
                        }
                    }
                    __init_TriggersByRelativeName = true;
                }
                return _TriggersByRelativeName;
            }
        }

        /// <summary>
        /// Сбрасывает флаг инициализации всех коллекций триггеров схемы таблицы.
        /// </summary>
        protected void ResetTriggersFull()
        {
            this.__init_Triggers = false;
            this.__init_TriggersByRelativeName = false;
        }

        /// <summary>
        /// Возвращает true, если триггер содержится в схеме таблицы.
        /// </summary>
        /// <param name="triggerRelativeName">Относительное название триггера, уникальное в рамках таблицы.</param>
        /// <returns></returns>
        public bool ContainsTrigger(string triggerRelativeName)
        {
            return this.ContainsTrigger(triggerRelativeName, false);
        }

        /// <summary>
        /// Возвращает true, если триггер содержится в схеме таблицы.
        /// </summary>
        /// <param name="triggerRelativeName">Относительное название триггера, уникальное в рамках таблицы.</param>
        /// <param name="throwNotFoundException">При указанном значении true, генерирует исключение в случае отсутствия триггера в схеме таблицы.</param>
        /// <returns></returns>
        public bool ContainsTrigger(string triggerRelativeName, bool throwNotFoundException)
        {
            if (string.IsNullOrEmpty(triggerRelativeName))
                throw new ArgumentNullException("triggerName");
            bool result = this.TriggersByRelativeName.ContainsKey(triggerRelativeName.ToLower());
            if (!result && throwNotFoundException)
                throw new Exception(string.Format("Триггер [{0}] не содержится в схеме таблицы {1}.", triggerRelativeName, this.TableName));
            return result;
        }

        /// <summary>
        /// Возвращает триггер таблицы по названию.
        /// В случае отсутствия триггера в схеме таблицы возвращает null.
        /// </summary>
        /// <param name="triggerRelativeName">Относительное название триггера.</param>
        /// <returns></returns>
        public DBTriggerSchema GetTrigger(string triggerRelativeName)
        {
            return this.GetTrigger(triggerRelativeName, false);
        }

        /// <summary>
        /// Возвращает триггер схемы таблицы по названию.
        /// </summary>
        /// <param name="triggerRelativeName">Относительное название триггера.</param>
        /// <param name="throwNotFoundException">При указанном значении true, генерирует исключение в случае отсутствия триггера в схеме таблицы.</param>
        /// <returns></returns>
        public DBTriggerSchema GetTrigger(string triggerRelativeName, bool throwNotFoundException)
        {
            if (string.IsNullOrEmpty(triggerRelativeName))
                throw new ArgumentNullException("triggerName");

            string triggerNameLow = triggerRelativeName.ToLower();
            DBTriggerSchema trigger = null;
            if (this.TriggersByRelativeName.ContainsKey(triggerNameLow))
                trigger = this.TriggersByRelativeName[triggerNameLow];

            if (trigger == null && throwNotFoundException)
                throw new Exception(string.Format("Не удалось получить триггер [{0}] в схеме таблицы {1}.", triggerRelativeName, this.TableName));

            return trigger;
        }

        #endregion


        private bool __init_TableInstances = false;
        private List<DBTable> _TableInstances;
        /// <summary>
        /// Коллекция экземпляров таблиц, созданных по данной схеме.
        /// </summary>
        private List<DBTable> TableInstances
        {
            get
            {
                if (!__init_TableInstances)
                {
                    _TableInstances = new List<DBTable>();
                    __init_TableInstances = true;
                }
                return _TableInstances;
            }
        }

        internal void AddTableInstance(DBTable tableInstance)
        {
            if (tableInstance == null)
                throw new ArgumentNullException("tableInstance");
            this.TableInstances.Add(tableInstance);
        }

        /// <summary>
        /// Строковое представление экземпляра адаптера схемы таблицы DBTableSchema.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(this.TableName))
                return this.TableName;
            return base.ToString();
        }
    }
}