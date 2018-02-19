using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storage.Metadata.MSSQL
{
    /// <summary>
    /// Представляет столбец индекса схемы таблицы DBObjects.
    /// </summary>
    public class DBObjectIndexColumnSchema : DBIndexColumnSchema
    {
        internal DBObjectIndexColumnSchema(MetadataIndexColumnDefinition indexColumnDefinition, DBObjectIndexSchema objectIndexSchema)
            : base(objectIndexSchema)
        {
            if (indexColumnDefinition == null)
                throw new ArgumentNullException("indexColumnDefinition");
            if (objectIndexSchema == null)
                throw new ArgumentNullException("objectIndexSchema");

            this.IndexColumnDefinition = indexColumnDefinition;
            this.ObjectIndexSchema = objectIndexSchema;
        }

        private MetadataIndexColumnDefinition _IndexColumnDefinition;
        /// <summary>
        /// Определение столбца индекса схемы таблицы DBObjects.
        /// </summary>
        public MetadataIndexColumnDefinition IndexColumnDefinition
        {
            get { return _IndexColumnDefinition; }
            private set { _IndexColumnDefinition = value; }
        }

        private DBObjectIndexSchema _ObjectIndexSchema;
        /// <summary>
        /// Индекс схемы таблицы DBObjects.
        /// </summary>
        public DBObjectIndexSchema ObjectIndexSchema
        {
            get { return _ObjectIndexSchema; }
            private set { _ObjectIndexSchema = value; }
        }

        protected override bool InitIsDescending()
        {
            return this.IndexColumnDefinition.IsDescending;
        }

        protected override string InitName()
        {
            return this.IndexColumnDefinition.ColumnName;
        }
    }
}
