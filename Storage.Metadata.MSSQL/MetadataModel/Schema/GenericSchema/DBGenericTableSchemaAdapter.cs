using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Storage.Metadata.MSSQL
{

    /// <summary>
    /// Обеспечивает работу со универсальной схемой набора таблиц однородных данных.
    /// </summary>
    public class DBGenericTableSchemaAdapter : DBTableSchemaAdapter
    {
        /// <summary>
        /// Создает экземпляр DBGenericTableSchemaAdapter.
        /// </summary>
        /// <param name="initialProperties">Инициализационные свойства экземпляра класса DBGenericTableSchemaAdapter.</param>
        public DBGenericTableSchemaAdapter(DBGenericTableSchemaAdapter.Properties initialProperties)
        {
            if (initialProperties == null)
                throw new ArgumentNullException("initialProperties");
            this.InitialProperties = initialProperties;
        }


        #region InitialProperties

        private DBGenericTableSchemaAdapter.Properties _InitialProperties;
        /// <summary>
        /// Инициализационные свойства адаптера схемы таблицы.
        /// </summary>
        internal DBGenericTableSchemaAdapter.Properties InitialProperties
        {
            get { return _InitialProperties; }
            private set { _InitialProperties = value; }
        }

        #endregion


        private bool __init_GenericTableSchema = false;
        private DBGenericPrincipalTableSchema _GenericTableSchema;
        /// <summary>
        /// Схема таблицы основных данных.
        /// </summary>
        protected DBGenericPrincipalTableSchema GenericTableSchema
        {
            get
            {
                if (!__init_GenericTableSchema)
                {

                    _GenericTableSchema = new DBGenericPrincipalTableSchema(this);
                    __init_GenericTableSchema = true;
                }
                return _GenericTableSchema;
            }
        }

        protected override DBPrincipalTableSchema InitTableSchema()
        {
            return this.GenericTableSchema;
        }

        private bool __init_RootPartition = false;
        private DBGenericTablePartition _RootPartition;
        /// <summary>
        /// Корневой раздел таблиц, соответствующий схеме таблицы.
        /// </summary>
        public DBGenericTablePartition RootPartition
        {
            get
            {
                if (!__init_RootPartition)
                {
                    _RootPartition = new DBGenericTablePartition(new DBGenericTablePartition.Properties(), this);
                    __init_RootPartition = true;
                }
                return _RootPartition;
            }
        }

        public override DBConnectionContext ConnectionContext
        {
            get { return this.InitialProperties.PrimaryDatabaseConnection.Context; }
        }

        protected override string InitTableName()
        {
            return this.InitialProperties.TableName;
        }

        protected override string InitOriginalTableName()
        {
            return this.InitialProperties.OriginalTableName;
        }

        protected override string InitPartitionTableName()
        {
            return this.InitialProperties.PartitionTableName;
        }

        protected override string InitOriginalPartitionTableName()
        {
            return this.InitialProperties.OriginalPartitionTableName;
        }

        protected override string InitTablePrefix()
        {
            return this.InitialProperties.TablePrefix;
        }

        protected override string InitOriginalTablePrefix()
        {
            return this.InitialProperties.OriginalTablePrefix;
        }

        protected override bool InitIsPermanentSchema()
        {
            return this.InitialProperties.IsPermanentSchema;
        }

        protected override DBConnection InitPrimaryDatabaseConnection()
        {
            return this.InitialProperties.PrimaryDatabaseConnection;
        }

        #region Properties

        /// <summary>
        /// Представляет набор инициализационных свойств провайдера схемы таблицы.
        /// </summary>
        public class Properties
        {
            /// <summary>
            /// Создает экземпляр DBGenericTableSchemaAdapter.Properties.
            /// </summary>
            public Properties()
            {
                this.Columns = new List<DBGenericColumnSchema.Properties>();
                this.Indexes = new List<DBGenericIndexSchema.Properties>();                
            }

            private string _TableName;
            /// <summary>
            /// Базовое название таблицы.
            /// </summary>
            public string TableName
            {
                get { return _TableName; }
                set { _TableName = value; }
            }


            private bool __init_OriginalTableName = false;
            private string _OriginalTableName;
            /// <summary>
            /// Базовое название таблицы до переименования.
            /// Если название таблицы не изменилось, возвращает актуальное название таблицы из свойства TableName.
            /// </summary>
            public string OriginalTableName
            {
                get
                {
                    if (!__init_OriginalTableName)
                    {
                        _OriginalTableName = this.TableName;
                        __init_OriginalTableName = true;
                    }
                    return _OriginalTableName;
                }
                set
                {
                    _OriginalTableName = value;
                    __init_OriginalTableName = true;
                }
            }

            private string _PartitionTableName;
            /// <summary>
            /// Базовое название таблицы в разделе.
            /// </summary>
            public string PartitionTableName
            {
                get { return _PartitionTableName; }
                set { _PartitionTableName = value; }
            }


            private bool __init_OriginalPartitionTableName = false;
            private string _OriginalPartitionTableName;
            /// <summary>
            /// Базовое название таблицы до переименования.
            /// Если название таблицы не изменилось, возвращает актуальное название таблицы из свойства PartitionTableName.
            /// </summary>
            public string OriginalPartitionTableName
            {
                get
                {
                    if (!__init_OriginalPartitionTableName)
                    {
                        _OriginalPartitionTableName = this.PartitionTableName;
                        __init_OriginalPartitionTableName = true;
                    }
                    return _OriginalPartitionTableName;
                }
                set
                {
                    _OriginalPartitionTableName = value;
                    __init_OriginalPartitionTableName = true;
                }
            }

            private string _TablePrefix;
            /// <summary>
            /// Логическое название таблицы, используемое в качестве префикса названий индексов и триггеров таблицы.
            /// </summary>
            public string TablePrefix
            {
                get { return _TablePrefix; }
                set { _TablePrefix = value; }
            }


            private bool __init_OriginalTablePrefix = false;
            private string _OriginalTablePrefix;
            /// <summary>
            /// Префикс таблицы до переименования.
            /// Если префикс не изменился, возвращает актуальный префикс из свойства TablePrefix.
            /// </summary>
            public string OriginalTablePrefix
            {
                get
                {
                    if (!__init_OriginalTablePrefix)
                    {
                        _OriginalTablePrefix = this.TablePrefix;
                        __init_OriginalTablePrefix = true;
                    }
                    return _OriginalTablePrefix;
                }
                set
                {
                    _OriginalTablePrefix = value;
                    __init_OriginalTablePrefix = true;
                }
            }

            private bool _IsPermanentSchema;
            /// <summary>
            /// Возвращает true, если схема таблицы является статической и неизменной.
            /// Таблицы с соответствующим набором столбцов для неизменной схемы уже должны существовать в базе данных.
            /// Адаптеру неизменной схемы таблицы доступно только изменение индексов и триггеров таблицы, а также доступно чтение схемы существующей таблицы.
            /// </summary>
            public bool IsPermanentSchema
            {
                get { return _IsPermanentSchema; }
                set { _IsPermanentSchema = value; }
            }

            private DBConnection _PrimaryDatabaseConnection;
            /// <summary>
            /// Подключение к базе основных данных.
            /// </summary>
            public DBConnection PrimaryDatabaseConnection
            {
                get { return _PrimaryDatabaseConnection; }
                set { _PrimaryDatabaseConnection = value; }
            }

            private ICollection<DBGenericColumnSchema.Properties> _Columns;
            /// <summary>
            /// Коллекция столбцов схемы таблицы.
            /// </summary>
            public ICollection<DBGenericColumnSchema.Properties> Columns
            {
                get { return _Columns; }
                set { _Columns = value; }
            }

            private ICollection<DBGenericIndexSchema.Properties> _Indexes;
            /// <summary>
            /// Коллекция индексов схемы таблицы.
            /// </summary>
            public ICollection<DBGenericIndexSchema.Properties> Indexes
            {
                get { return _Indexes; }
                set { _Indexes = value; }
            }           

            private bool _IdentityColumnSupported = true;
            /// <summary>
            /// Возвращает true, если схема таблицы поддерживает инкрементный столбец-идентификатор.
            /// </summary>
            public bool IdentityColumnSupported
            {
                get { return _IdentityColumnSupported; }
                set { _IdentityColumnSupported = value; }
            }

            private string _IdentityColumnName;
            /// <summary>
            /// Название автоинкрементного столбца-идентификатора таблицы.
            /// </summary>
            public string IdentityColumnName
            {
                get { return _IdentityColumnName; }
                set { _IdentityColumnName = value; }
            }

            private bool _PrimaryKeySupported = true;
            /// <summary>
            /// Возвращает true, если схема таблицы поддерживает первичный ключ.
            /// </summary>
            public bool PrimaryKeySupported
            {
                get { return _PrimaryKeySupported; }
                set { _PrimaryKeySupported = value; }
            }

            private string _PrimaryKeyRelativeName;
            /// <summary>
            /// Название кластеризованного индекса, представляющего первичный ключ таблицы.
            /// </summary>
            public string PrimaryKeyRelativeName
            {
                get { return _PrimaryKeyRelativeName; }
                set { _PrimaryKeyRelativeName = value; }
            }
        }

        #endregion
    }
}
