using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storage.Metadata.MSSQL
{
    /// <summary>
    /// Представляет столбец схемы таблицы DBObjects.
    /// </summary>
    public class DBObjectColumnSchema : DBColumnSchema
    {
        internal DBObjectColumnSchema(MetadataPropertyDefinition propertyDefinition, DBObjectTableSchemaAdapter objectSchemaAdapter)
            : base(objectSchemaAdapter)
        {
            if (propertyDefinition == null)
                throw new ArgumentNullException("propertyDefinition");
            if (objectSchemaAdapter == null)
                throw new ArgumentNullException("objectSchemaAdapter");

            this.PropertyDefinition = propertyDefinition;
            this.ObjectSchemaAdapter = objectSchemaAdapter;
        }

        private MetadataPropertyDefinition _PropertyDefinition;
        /// <summary>
        /// Определение свойства DBObjects.
        /// </summary>
        public MetadataPropertyDefinition PropertyDefinition
        {
            get { return _PropertyDefinition; }
            private set { _PropertyDefinition = value; }
        }

        private DBObjectTableSchemaAdapter _ObjectSchemaAdapter;
        /// <summary>
        /// Адаптер схемы таблицы DBObjects.
        /// </summary>
        public DBObjectTableSchemaAdapter ObjectSchemaAdapter
        {
            get { return _ObjectSchemaAdapter; }
            private set { _ObjectSchemaAdapter = value; }
        }

        protected override bool InitIsNullable()
        {
            return this.PropertyDefinition.IsNullable;
        }

        protected override string InitName()
        {
            return this.PropertyDefinition.ColumnName;
        }

        protected override int InitSize()
        {
            return this.PropertyDefinition.Size;
        }

        protected override SqlDbType InitType()
        {
            return this.PropertyDefinition.SqlType;
        }
    }
}
