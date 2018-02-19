using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Storage.Metadata.MSSQL;

namespace Storage.Metadata.MSSQL
{
    /// <summary>
    /// 
    /// </summary>
    public class DBObjectDistributedTablePartition : DBTablePartition
    {
        internal DBObjectDistributedTablePartition(DBObjectTableSchemaAdapter adapter, string tableName)
            : base(adapter)
        {
            if (String.IsNullOrEmpty(tableName))
                throw new ArgumentNullException("tableName");

            this.Name = tableName;
        }

        private string _Name;
        /// <summary>
        /// Имя таблицы.
        /// </summary>
        public string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }

        protected override string InitName()
        {
            return this.Name;
        }

        protected override string InitOriginalName()
        {
            return this.Name;
        }

        protected override string InitOriginalPrefix()
        {
            return this.Name;
        }

        protected override DBTablePartition InitParentPartition()
        {
            return null;
        }

        protected override string InitPrefix()
        {
            return this.Name;
        }
    }
}
