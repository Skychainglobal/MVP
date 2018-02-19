using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storage.Metadata.MSSQL
{
    public abstract class DBTablePartition
    {
        // <summary>
        /// Создает экземпляр DBTablePartition.
        /// </summary>
        /// <param name="schemaAdapter">Схема таблицы.</param>
        protected DBTablePartition(DBTableSchemaAdapter schemaAdapter)
        {
            if (schemaAdapter == null)
                throw new ArgumentNullException("schemaAdapter");

            this.SchemaAdapter = schemaAdapter;

            //добавляем экземпляр раздела в коллекцию экземпляров разделов адаптера.
            this.SchemaAdapter.AddPartitionInstance(this);
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

        #region ParentPartition


        /// <summary>
        /// Инициализирует родительский раздел таблиц.
        /// </summary>
        /// <returns></returns>
        protected abstract DBTablePartition InitParentPartition();

        private bool __init_ParentPartition = false;
        private DBTablePartition _ParentPartition;
        /// <summary>
        /// Родительский раздел таблиц. Для корневого раздела данных равен null.
        /// </summary>
        public DBTablePartition ParentPartition
        {
            get
            {
                if (!__init_ParentPartition)
                {
                    _ParentPartition = this.InitParentPartition();

                    //проверяем, что схема таблицы является управляемой.
                    if (_ParentPartition != null)
                        this.SchemaAdapter.CheckManagedSchema();

                    __init_ParentPartition = true;
                }
                return _ParentPartition;
            }
        }

        private bool __init_AllParentPartitions = false;
        private DBCollection<DBTablePartition> _AllParentPartitions;
        /// <summary>
        /// Коллекция всех родительских разделов данного раздела.
        /// </summary>
        internal DBCollection<DBTablePartition> AllParentPartitions
        {
            get
            {
                if (!__init_AllParentPartitions)
                {
                    _AllParentPartitions = new DBCollection<DBTablePartition>();
                    DBTablePartition partition = this;
                    while (partition.ParentPartition != null)
                    {
                        _AllParentPartitions.Add(partition.ParentPartition);
                        partition = partition.ParentPartition;
                    }
                    __init_AllParentPartitions = true;
                }
                return _AllParentPartitions;
            }
        }


        #endregion

        #region Name

        /// <summary>
        /// Инициализирует название раздела данных таблицы.
        /// </summary>
        /// <returns></returns>
        protected abstract string InitName();


        private bool __init_Name = false;
        private string _Name;
        /// <summary>
        /// Название раздела данных таблицы.
        /// </summary>
        public string Name
        {
            get
            {
                if (!__init_Name)
                {
                    _Name = this.InitName();
                    __init_Name = true;
                }
                return _Name;
            }
        }

        /// <summary>
        /// Инициализирует название раздела данных таблицы до переименования раздела.
        /// </summary>
        /// <returns></returns>
        protected abstract string InitOriginalName();


        private bool __init_OriginalName = false;
        private string _OriginalName;
        /// <summary>
        /// Название раздела данных таблицы до переименования раздела.
        /// Если раздел не был переименован, то значение исходного имени актуальному имени, указаннному в свойстве Name.
        /// </summary>
        internal string OriginalName
        {
            get
            {
                if (!__init_OriginalName)
                {
                    _OriginalName = this.InitOriginalName();

                    if (string.IsNullOrEmpty(_OriginalName))
                        _OriginalName = this.Name;
                    __init_OriginalName = true;
                }
                return _OriginalName;
            }
        }

        #endregion


        #region FullName

        private bool __init_FullName = false;
        private string _FullName;
        /// <summary>
        /// Полное имя раздела, включающее имена всех родительских разделов и свое собственное имя.
        /// </summary>
        public string FullName
        {
            get
            {
                if (!__init_FullName)
                {
                    //формируем полное имя раздела. 
                    _FullName = this.Name;
                    __init_FullName = true;
                }
                return _FullName;
            }
        }

        private bool __init_OriginalFullName = false;
        private string _OriginalFullName;
        /// <summary>
        /// Полное имя раздела до переименования раздела, включающее имена всех родительских разделов и свое собственное имя.
        /// </summary>
        internal string OriginalFullName
        {
            get
            {
                if (!__init_OriginalFullName)
                {
                    //полное имя раздела равно самому оригинальному имени раздела.
                    _OriginalFullName = this.OriginalName;
                    __init_OriginalFullName = true;
                }
                return _OriginalFullName;
            }
        }

        private bool __init_FullNameChanged = false;
        private bool _FullNameChanged;
        /// <summary>
        /// Вовзращает true, если название раздела изменилось в контексте выполнения кода.
        /// </summary>
        internal bool FullNameChanged
        {
            get
            {
                if (!__init_FullNameChanged)
                {
                    _FullNameChanged = this.OriginalFullName.ToLower() != this.FullName.ToLower();
                    __init_FullNameChanged = true;
                }
                return _FullNameChanged;
            }
        }

        /// <summary>
        /// Сбрасывает флаг инициализации имени раздела и зависимых от него свойств.
        /// </summary>
        public void ResetFullName()
        {
            this.__init_Name = false;
            this.__init_OriginalName = false;
            this.__init_FullName = false;
            this.__init_OriginalFullName = false;
            this.__init_FullNameChanged = false;

            this.ResetFullPrefix();

            if (this.FullNameChanged || this.FullPrefixChanged)
                this.ResetTableName();
        }

        #endregion


        #region Prefix

        /// <summary>
        /// Инициализирует префикс раздела данных таблицы.
        /// </summary>
        /// <returns></returns>
        protected abstract string InitPrefix();


        private bool __init_Prefix = false;
        private string _Prefix;
        /// <summary>
        /// Префикс раздела данных таблицы.
        /// </summary>
        public string Prefix
        {
            get
            {
                if (!__init_Prefix)
                {
                    _Prefix = this.InitPrefix();
                    __init_Prefix = true;
                }
                return _Prefix;
            }
        }


        /// <summary>
        /// Инициализирует префикс раздела данных таблицы до переименования раздела.
        /// </summary>
        /// <returns></returns>
        protected abstract string InitOriginalPrefix();


        private bool __init_OriginalPrefix = false;
        private string _OriginalPrefix;
        /// <summary>
        /// Префикс раздела данных таблицы до переименования раздела.
        /// Если раздел не был переименован, то значение исходного имени актуальному имени, указаннному в свойстве Prefix.
        /// </summary>
        internal string OriginalPrefix
        {
            get
            {
                if (!__init_OriginalPrefix)
                {
                    _OriginalPrefix = this.InitOriginalPrefix();

                    if (string.IsNullOrEmpty(_OriginalPrefix))
                        _OriginalPrefix = this.Prefix;
                    __init_OriginalPrefix = true;
                }
                return _OriginalPrefix;
            }
        }

        #endregion


        #region FullPrefix

        private bool __init_FullPrefix = false;
        private string _FullPrefix;
        /// <summary>
        /// Полный префикс раздела, включающее префиксы всех родительских разделов и свой собственный префикс.
        /// </summary>
        public string FullPrefix
        {
            get
            {
                if (!__init_FullPrefix)
                {
                    //формируем полное префикс раздела.

                    //префикс полное префикс раздела равно самому имени раздела.
                    _FullPrefix = this.Prefix;
                    __init_FullPrefix = true;
                }
                return _FullPrefix;
            }
        }


        private bool __init_OriginalFullPrefix = false;
        private string _OriginalFullPrefix;
        /// <summary>
        /// Полный префикс раздела до переименования раздела, включающий префиксы всех родительских разделов и свой собственный префикс.
        /// </summary>
        internal string OriginalFullPrefix
        {
            get
            {
                if (!__init_OriginalFullPrefix)
                {
                    //формируем полное префикс раздела.

                    // префикс полное префикс раздела равно самому оригинальному имени раздела.
                    _OriginalFullPrefix = this.OriginalPrefix;
                    __init_OriginalFullPrefix = true;
                }
                return _OriginalFullPrefix;
            }
        }


        private bool __init_FullPrefixChanged = false;
        private bool _FullPrefixChanged;
        /// <summary>
        /// Вовзращает true, если префикс раздела изменился в контексте выполнения кода.
        /// </summary>
        internal bool FullPrefixChanged
        {
            get
            {
                if (!__init_FullPrefixChanged)
                {
                    _FullPrefixChanged = this.OriginalFullPrefix.ToLower() != this.FullPrefix.ToLower();
                    __init_FullPrefixChanged = true;
                }
                return _FullPrefixChanged;
            }
        }


        /// <summary>
        /// Сбрасывает флаг инициализации префикса раздела и зависимых от него свойств.
        /// </summary>
        private void ResetFullPrefix()
        {
            this.__init_Prefix = false;
            this.__init_OriginalPrefix = false;
            this.__init_FullPrefix = false;
            this.__init_OriginalFullPrefix = false;
            this.__init_FullPrefixChanged = false;
        }

        #endregion


        #region TableName

        private bool __init_TableName = false;
        private string _TableName;
        /// <summary>
        /// Общее название таблицы раздела данных, используемое для названий таблиц раздела.
        /// </summary>
        public string TableName
        {
            get
            {
                if (!__init_TableName)
                {
                    //формируем имя таблицы.                    
                    _TableName = this.SchemaAdapter.TableName;
                    if (string.IsNullOrEmpty(_TableName))
                        throw new Exception(string.Format("Не инициализировано основное имя таблицы раздела {0} для схемы таблицы {1}.", this.FullName, this.SchemaAdapter.TableName));

                    __init_TableName = true;
                }
                return _TableName;
            }
        }

        private bool __init_OriginalTableName = false;
        private string _OriginalTableName;
        /// <summary>
        /// Общее название таблицы раздела данных до переименования.
        /// </summary>
        internal string OriginalTableName
        {
            get
            {
                if (!__init_OriginalTableName)
                {
                    //формируем имя таблицы.                    
                    _OriginalTableName = this.SchemaAdapter.OriginalTableName;

                    if (string.IsNullOrEmpty(_OriginalTableName))
                        throw new Exception(string.Format("Не инициализировано основное имя таблицы раздела до переименования {0} для схемы таблицы {1}.", this.OriginalFullName, this.SchemaAdapter.OriginalTableName));

                    __init_OriginalTableName = true;
                }
                return _OriginalTableName;
            }
        }

        private bool __init_TableNameChanged = false;
        private bool _TableNameChanged;
        /// <summary>
        /// Возвращает true, если общее название таблицы раздела данных было изменено в контексте выполнения кода 
        /// вследствие переименования таблицы в провадере схем таблиц либо вследствие переименования раздела.
        /// </summary>
        internal bool TableNameChanged
        {
            get
            {
                if (!__init_TableNameChanged)
                {
                    _TableNameChanged = this.OriginalTableName.ToLower() != this.TableName.ToLower();
                    __init_TableNameChanged = true;
                }
                return _TableNameChanged;
            }
        }

        /// <summary>
        /// Сбрасывает флаг инициализации общего названия таблицы раздела и зависимых от него свойств.
        /// </summary>
        internal void ResetTableName()
        {
            this.__init_TableName = false;
            this.__init_OriginalTableName = false;
            this.__init_TableNameChanged = false;

            //сбрасываем префикс таблицы.
            this.ResetTablePrefixInternal();

            if (this.TableNameChanged)
                this.Table.ResetName();
            else if (this.TablePrefixChanged)
                this.Table.ResetPrefix();
        }

        #endregion


        #region TablePrefix

        private bool __init_TablePrefix = false;
        private string _TablePrefix;
        /// <summary>
        /// Логическое название таблицы, используемое в качестве префикса названий индексов и триггеров таблицы, определенное для раздела данных.
        /// </summary>
        public string TablePrefix
        {
            get
            {
                if (!__init_TablePrefix)
                {
                    //формируем префикс индексов.

                    _TablePrefix = this.SchemaAdapter.TablePrefix;

                    if (string.IsNullOrEmpty(_TablePrefix))
                        throw new Exception(string.Format("Не инициализирован префикс таблицы раздела {0} для схемы таблицы {1}.", this.FullName, this.SchemaAdapter.TableName));

                    __init_TablePrefix = true;
                }
                return _TablePrefix;
            }
        }


        private bool __init_OriginalTablePrefix = false;
        private string _OriginalTablePrefix;
        /// <summary>
        /// Префикс таблицы для раздела данных до переименования.
        /// </summary>
        internal string OriginalTablePrefix
        {
            get
            {
                if (!__init_OriginalTablePrefix)
                {
                    _OriginalTablePrefix = this.SchemaAdapter.OriginalTablePrefix;
                    if (string.IsNullOrEmpty(_OriginalTablePrefix))
                        throw new Exception(string.Format("Не инициализирован префикс таблицы раздела до переименования {0} для схемы таблицы {1}.", this.OriginalFullName, this.SchemaAdapter.OriginalTableName));

                    __init_OriginalTablePrefix = true;
                }
                return _OriginalTablePrefix;
            }
        }


        private bool __init_TablePrefixChanged = false;
        private bool _TablePrefixChanged;
        /// <summary>
        /// Возвращает true, если префикс таблицы раздела данных был изменен в контексте выполнения кода 
        /// вследствие переименования таблицы в провадере схем таблиц либо вследствие переименования раздела.
        /// </summary>
        internal bool TablePrefixChanged
        {
            get
            {
                if (!__init_TablePrefixChanged)
                {
                    _TablePrefixChanged = this.OriginalTablePrefix.ToLower() != this.TablePrefix.ToLower();
                    __init_TablePrefixChanged = true;
                }
                return _TablePrefixChanged;
            }
        }

        /// <summary>
        /// Сбрасывает флаг инициализации префикса таблицы и зависимых от него свойств.
        /// </summary>
        internal void ResetTablePrefix()
        {
            this.ResetTablePrefixInternal();

            //сбрасываем префикс в таблицах.
            if (this.TablePrefixChanged)
                this.Table.ResetPrefix();
        }

        private void ResetTablePrefixInternal()
        {
            this.__init_TablePrefix = false;
            this.__init_OriginalTablePrefix = false;
            this.__init_TablePrefixChanged = false;
        }

        #endregion


        private bool __init_Table = false;
        private DBPrincipalTable _Table;
        /// <summary>
        /// Таблица основных данных.
        /// </summary>
        public DBPrincipalTable Table
        {
            get
            {
                if (!__init_Table)
                {
                    _Table = new DBPrincipalTable(this);
                    __init_Table = true;
                }
                return _Table;
            }
        }

        /// <summary>
        /// Строковое представление класса DBTablePartition.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(this.FullName))
                return this.FullName;
            return base.ToString();
        }
    }
}
