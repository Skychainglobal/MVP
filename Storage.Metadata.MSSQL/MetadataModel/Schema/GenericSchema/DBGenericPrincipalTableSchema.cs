using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Storage.Metadata.MSSQL
{
    /// <summary>
    /// Представляет универсальную схему основной таблицы однородных данных, определенной в формате.
    /// </summary>
    public class DBGenericPrincipalTableSchema : DBPrincipalTableSchema
    {
        /// <summary>
        /// Создает экземпляр DBGenericPrincipalTableSchema.
        /// </summary>
        /// <param name="schemaAdapter">Адаптер схемы таблицы.</param>
        internal DBGenericPrincipalTableSchema(DBGenericTableSchemaAdapter schemaAdapter)
            : base(schemaAdapter)
        {
            if (schemaAdapter == null)
                throw new ArgumentNullException("schemaAdapter");

            this.GenericSchemaAdapter = schemaAdapter;
        }


        private DBGenericTableSchemaAdapter _GenericSchemaAdapter;
        /// <summary>
        /// Адаптер схемы таблицы.
        /// </summary>
        internal DBGenericTableSchemaAdapter GenericSchemaAdapter
        {
            get { return _GenericSchemaAdapter; }
            private set { _GenericSchemaAdapter = value; }
        }

        protected internal override ICollection<DBColumnSchema> InitColumns()
        {
            List<DBColumnSchema> columns = new List<DBColumnSchema>();
            ICollection<DBGenericColumnSchema.Properties> columnPropertiesCollection = this.GenericSchemaAdapter.InitialProperties.Columns;
            if (columnPropertiesCollection != null)
            {
                foreach (DBGenericColumnSchema.Properties columnProperties in columnPropertiesCollection)
                {
                    if (columnProperties != null)
                        columns.Add(new DBGenericColumnSchema(columnProperties, this.GenericSchemaAdapter));
                }
            }
            return columns;
        }

        protected internal override ICollection<DBIndexSchema> InitIndexes()
        {
            List<DBIndexSchema> indexes = new List<DBIndexSchema>();
            ICollection<DBGenericIndexSchema.Properties> indexPropertiesCollection = this.GenericSchemaAdapter.InitialProperties.Indexes;
            if (indexPropertiesCollection != null)
            {
                foreach (DBGenericIndexSchema.Properties indexProperties in indexPropertiesCollection)
                {
                    if (indexProperties != null)
                        indexes.Add(new DBGenericIndexSchema(indexProperties, this.GenericSchemaAdapter));
                }
            }
            return indexes;
        }

        protected internal override ICollection<DBColumnSchema> InitSystemColumns()
        {
            return null;
        }

        protected internal override ICollection<DBTriggerSchema> InitTriggers()
        {
            return null;
        }

        protected internal override bool InitIdentityColumnSupported()
        {
            return this.GenericSchemaAdapter.InitialProperties.IdentityColumnSupported;
        }

        protected override string InitIdentityColumnName()
        {
            return this.GenericSchemaAdapter.InitialProperties.IdentityColumnName;
        }

        protected internal override bool InitPrimaryKeySupported()
        {
            return this.GenericSchemaAdapter.InitialProperties.PrimaryKeySupported;
        }

        protected internal override string InitPrimaryKeyRelativeName()
        {
            return this.GenericSchemaAdapter.InitialProperties.PrimaryKeyRelativeName;
        }
    }
}
