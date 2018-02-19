using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Storage.Metadata.MSSQL
{
    /// <summary>
    /// Представляет класс для работы со схемой таблицы основных данных.
    /// </summary>
    public class DBObjectPrincipalTableSchema : DBPrincipalTableSchema
    {
        internal DBObjectPrincipalTableSchema(DBObjectTableSchemaAdapter objectSchemaAdapter)
            : base(objectSchemaAdapter)
        {
            if (objectSchemaAdapter == null)
                throw new ArgumentNullException("objectSchemaAdapter");

            this.ObjectSchemaAdapter = objectSchemaAdapter;
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

        protected internal override ICollection<DBColumnSchema> InitColumns()
        {
            List<DBColumnSchema> columns = new List<DBColumnSchema>();

            //добавляем все основные столбцы.
            foreach (MetadataPropertyDefinition propertyDefinition in this.ObjectSchemaAdapter.ClassDefinition.AllMetadataProperties)
            {
                DBObjectColumnSchema columnSchema = new DBObjectColumnSchema(propertyDefinition, this.ObjectSchemaAdapter);
                columns.Add(columnSchema);
            }

            return columns;
        }

        protected override string InitIdentityColumnName()
        {
            return this.ObjectSchemaAdapter.ClassDefinition.IdentityProperty.ColumnName;
        }

        protected internal override ICollection<DBIndexSchema> InitIndexes()
        {
            List<DBIndexSchema> indexes = new List<DBIndexSchema>();
            foreach (MetadataIndexDefinition indexDefinition in this.ObjectSchemaAdapter.ClassDefinition.Indexes)
            {
                DBObjectIndexSchema objectIndex = new DBObjectIndexSchema(indexDefinition, this.ObjectSchemaAdapter);
                indexes.Add(objectIndex);
            }
            return indexes;
        }

        protected internal override ICollection<DBTriggerSchema> InitTriggers()
        {
            List<DBTriggerSchema> triggers = new List<DBTriggerSchema>();

            //добавляем кастомные триггеры, если они присутствуют в схеме.
            foreach (ConstructorInfo triggerConstructor in this.ObjectSchemaAdapter.ClassDefinition.TriggerTypeConstructors)
            {
                DBTriggerSchema triggerSchema = (DBTriggerSchema)triggerConstructor.Invoke(new object[] { this.ObjectSchemaAdapter });
                if (triggerSchema == null)
                    throw new Exception(string.Format("Не удалось создать экземпляр схемы триггера типа {0}.", triggerConstructor.DeclaringType.FullName));
                triggers.Add(triggerSchema);
            }

            return triggers;
        }
    }
}
