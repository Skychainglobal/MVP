using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storage.Metadata.MSSQL
{
    /// <summary>
    /// Представляет индекс схемы таблицы DBObjects.
    /// </summary>
    public class DBObjectIndexSchema : DBIndexSchema
    {
        internal DBObjectIndexSchema(MetadataIndexDefinition indexDefinition, DBObjectTableSchemaAdapter objectSchemaAdapter)
            : base(objectSchemaAdapter)
        {
            if (indexDefinition == null)
                throw new ArgumentNullException("indexDefinition");
            if (objectSchemaAdapter == null)
                throw new ArgumentNullException("objectSchemaAdapter");

            this.IndexDefinition = indexDefinition;
            this.ObjectSchemaAdapter = objectSchemaAdapter;
        }

        private MetadataIndexDefinition _IndexDefinition;
        /// <summary>
        /// Определение индекса DBObjects.
        /// </summary>
        public MetadataIndexDefinition IndexDefinition
        {
            get { return _IndexDefinition; }
            private set { _IndexDefinition = value; }
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

        protected override ICollection<DBIndexColumnSchema> InitColumns()
        {
            List<DBIndexColumnSchema> indexColumns = new List<DBIndexColumnSchema>();
            foreach (MetadataIndexColumnDefinition indexColumnDefinition in this.IndexDefinition.IndexColumns)
            {
                DBObjectIndexColumnSchema indexColumnSchema = new DBObjectIndexColumnSchema(indexColumnDefinition, this);
                indexColumns.Add(indexColumnSchema);
            }
            return indexColumns;
        }

        protected override string InitRelativeName()
        {
            return this.IndexDefinition.RelativeName;
        }
    }
}
