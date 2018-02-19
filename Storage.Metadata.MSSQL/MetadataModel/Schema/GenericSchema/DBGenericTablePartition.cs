using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Storage.Metadata.MSSQL
{
    /// <summary>
    /// Универсальный класс работы с разделом таблиц, не представленный в частных реализациях разделов таблиц.
    /// </summary>
    public class DBGenericTablePartition : DBTablePartition
    {
        /// <summary>
        /// Создает экземпляр DBGenericTablePartition.
        /// </summary>
        /// <param name="initialProperties">Инициализационные свойства раздела таблиц.</param>
        /// <param name="schemaAdapter">Адаптер схемы таблицы.</param>
        public DBGenericTablePartition(DBGenericTablePartition.Properties initialProperties, DBTableSchemaAdapter schemaAdapter)
            : base(schemaAdapter)
        {
            if (initialProperties == null)
                throw new ArgumentNullException("initialProperties");
            this.InitialProperties = initialProperties;
        }

        private DBGenericTablePartition.Properties _InitialProperties;
        /// <summary>
        /// Инициализационные свойства раздела таблиц.
        /// </summary>
        private DBGenericTablePartition.Properties InitialProperties
        {
            get { return _InitialProperties; }
            set { _InitialProperties = value; }
        }

        protected override DBTablePartition InitParentPartition()
        {
            return this.InitialProperties.ParentPartition;
        }

        protected override string InitName()
        {
            return this.InitialProperties.Name;
        }

        protected override string InitOriginalName()
        {
            return this.InitialProperties.OriginalName;
        }

        protected override string InitPrefix()
        {
            return this.InitialProperties.Prefix;
        }

        protected override string InitOriginalPrefix()
        {
            return this.InitialProperties.OriginalPrefix;
        }

        #region Properties

        /// <summary>
        /// Набор инициализационных свойств раздела таблиц.
        /// </summary>
        public class Properties
        {
            /// <summary>
            /// Создает экземпляр DBGenericTablePartition.Properties.
            /// </summary>
            public Properties()
            {
            }

            private DBTablePartition _ParentPartition;
            /// <summary>
            /// Родительский раздел таблиц. Для корневого раздела данных равен null.
            /// </summary>
            public DBTablePartition ParentPartition
            {
                get { return _ParentPartition; }
                set { _ParentPartition = value; }
            }

            private string _Name;
            /// <summary>
            /// Название раздела данных таблицы.
            /// </summary>
            public string Name
            {
                get { return _Name; }
                set { _Name = value; }
            }


            private bool __init_OriginalName = false;
            private string _OriginalName;
            /// <summary>
            /// Название раздела данных таблицы до переименования раздела.
            /// Если раздел не был переименован, то значение исходного имени актуальному имени, указаннному в свойстве Name.
            /// </summary>
            public string OriginalName
            {
                get
                {
                    if (!__init_OriginalName)
                    {
                        _OriginalName = this.Name;
                        __init_OriginalName = true;
                    }
                    return _OriginalName;
                }
                set
                {
                    _OriginalName = value;
                    __init_OriginalName = true;
                }
            }

            private string _Prefix;
            /// <summary>
            /// Префикс раздела данных таблицы.
            /// </summary>
            public string Prefix
            {
                get { return _Prefix; }
                set { _Prefix = value; }
            }

            private bool __init_OriginalPrefix = false;
            private string _OriginalPrefix;
            /// <summary>
            /// Префикс раздела данных таблицы до переименования раздела.
            /// Если раздел не был переименован, то значение исходного префикса равно актуальному префиксу, указаннному в свойстве Prefix.
            /// </summary>
            public string OriginalPrefix
            {
                get
                {
                    if (!__init_OriginalPrefix)
                    {
                        _OriginalPrefix = this.Prefix;
                        __init_OriginalPrefix = true;
                    }
                    return _OriginalPrefix;
                }
                set
                {
                    _OriginalPrefix = value;
                    __init_OriginalPrefix = true;
                }
            }

            private string _DataCondition;
            /// <summary>
            /// Условие размещения данных в данном разделе таблиц.
            /// </summary>
            public string DataCondition
            {
                get { return _DataCondition; }
                set { _DataCondition = value; }
            }
        }

        #endregion

    }
}
