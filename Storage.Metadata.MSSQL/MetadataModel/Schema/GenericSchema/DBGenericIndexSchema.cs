using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Storage.Metadata.MSSQL
{
    /// <summary>
    /// Универсальный класс работы со схемой индекса, не представленный в частных реализациях схем таблиц.
    /// </summary>
    public class DBGenericIndexSchema : DBIndexSchema
    {
        /// <summary>
        /// Создает экземпляр DBGenericIndexSchema.
        /// </summary>
        /// <param name="initialProperties">Инициализационные свойства схемы индекса.</param>
        /// <param name="schemaAdapter">Адаптер схемы таблицы.</param>
        public DBGenericIndexSchema(DBGenericIndexSchema.Properties initialProperties, DBTableSchemaAdapter schemaAdapter)
            : base(schemaAdapter)
        {
            if (initialProperties == null)
                throw new ArgumentNullException("initialProperties");
            this.InitialProperties = initialProperties;
        }


        #region InitialProperties

        private DBGenericIndexSchema.Properties _InitialProperties;
        /// <summary>
        /// Инициализационные свойства схемы индекса.
        /// </summary>
        private DBGenericIndexSchema.Properties InitialProperties
        {
            get { return _InitialProperties; }
            set { _InitialProperties = value; }
        }

        #endregion


        /// <summary>
        /// Инициализирует название индекса.
        /// </summary>
        /// <returns></returns>
        protected override string InitRelativeName()
        {
            return this.InitialProperties.RelativeName;
        }

        /// <summary>
        /// Инициализирует коллекцию столбцов индекса.
        /// </summary>
        protected override ICollection<DBIndexColumnSchema> InitColumns()
        {
            List<DBIndexColumnSchema> columns = new List<DBIndexColumnSchema>();
            if (this.InitialProperties.Columns != null && this.InitialProperties.Columns.Count > 0)
            {
                if (!string.IsNullOrEmpty(this.InitialProperties.ColumnName))
                    throw new Exception("Должно быть задано только одно из свойств DBGenericIndexSchema.Properties.Columns или DBGenericIndexSchema.Properties.ColumnName.");

                foreach (DBGenericIndexColumnSchema.Properties columnProperties in this.InitialProperties.Columns)
                {
                    if (columnProperties != null)
                    {
                        DBGenericIndexColumnSchema column = new DBGenericIndexColumnSchema(columnProperties, this);
                        columns.Add(column);
                    }
                }
            }
            else if (!string.IsNullOrEmpty(this.InitialProperties.ColumnName))
            {
                DBGenericIndexColumnSchema column = new DBGenericIndexColumnSchema(new DBGenericIndexColumnSchema.Properties()
                {
                    Name = this.InitialProperties.ColumnName,
                    IsDescending = false
                }, this);
                columns.Add(column);
            }
            return columns;
        }


        #region Properties Class

        /// <summary>
        /// Представляет набор инициализационных свойств индекса.
        /// </summary>
        public class Properties
        {
            /// <summary>
            /// Создает экземпляр DBGenericIndexSchema.Properties.
            /// </summary>
            public Properties()
            {
                this.Columns = new List<DBGenericIndexColumnSchema.Properties>();
            }


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
                        _RelativeName = this.ColumnName;
                        __init_RelativeName = true;
                    }
                    return _RelativeName;
                }
                set
                {
                    _RelativeName = value;
                    __init_RelativeName = true;
                }
            }



            private bool __init_OriginalRelativeName = false;
            private string _OriginalRelativeName;
            /// <summary>
            /// Исходное относительное название индекса до переименования.
            /// </summary>
            public string OriginalRelativeName
            {
                get
                {
                    if (!__init_OriginalRelativeName)
                    {
                        _OriginalRelativeName = this.RelativeName;
                        __init_OriginalRelativeName = true;
                    }
                    return _OriginalRelativeName;
                }
                set
                {
                    _OriginalRelativeName = value;
                    __init_OriginalRelativeName = true;
                }
            }

            private string _ColumnName;
            /// <summary>
            /// Название единственного столбца, добавляемого в индекс.
            /// </summary>
            public string ColumnName
            {
                get { return _ColumnName; }
                set { _ColumnName = value; }
            }


            private ICollection<DBGenericIndexColumnSchema.Properties> _Columns;
            /// <summary>
            /// Коллекция свойств столбцов индекса.
            /// </summary>
            public ICollection<DBGenericIndexColumnSchema.Properties> Columns
            {
                get { return _Columns; }
                set { _Columns = value; }
            }

            /// <summary>
            /// Строковое представление класса DBGenericIndexSchema.Properties.
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                if (!string.IsNullOrEmpty(this.RelativeName))
                    return this.RelativeName;
                return base.ToString();
            }
        }

        #endregion
    }
}
