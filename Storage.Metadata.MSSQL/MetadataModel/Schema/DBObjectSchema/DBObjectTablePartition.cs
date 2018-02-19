using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storage.Metadata.MSSQL
{
    /// <summary>
    /// Представляет корневой раздел таблицы DBObjects.
    /// </summary>
    public class DBObjectTablePartition : DBTablePartition
    {
        internal DBObjectTablePartition(DBObjectTableSchemaAdapter objectSchemaAdapter)
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

        protected override string InitName()
        {
            return null;
        }

        protected override string InitOriginalName()
        {
            return null;
        }

        protected override string InitOriginalPrefix()
        {
            return null;
        }

        protected override DBTablePartition InitParentPartition()
        {
            return null;
        }

        protected override string InitPrefix()
        {
            return null;
        }
    }
}
