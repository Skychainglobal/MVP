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
    public class DBObjectDistributedTable
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="adapter"></param>
        /// <param name="tableName"></param>
        internal DBObjectDistributedTable(DBObjectTableSchemaAdapter adapter, string tableName)
        {
            if (adapter == null)
                throw new ArgumentNullException("adapter");

            if (String.IsNullOrEmpty(tableName))
                throw new ArgumentNullException("tableName");

            this.SchemaAdapter = adapter;
            this.TableName = tableName;
        }

        private string _TableName;

        public string TableName
        {
            get { return _TableName; }
            set { _TableName = value; }
        }

        private DBObjectTableSchemaAdapter _SchemaAdapter;

        public DBObjectTableSchemaAdapter SchemaAdapter
        {
            get { return _SchemaAdapter; }
            set { _SchemaAdapter = value; }
        }

        private bool __init_TablePartition = false;
        private DBObjectDistributedTablePartition _TablePartition;
        /// <summary>
        /// 
        /// </summary>
        public DBObjectDistributedTablePartition TablePartition
        {
            get
            {
                if (!__init_TablePartition)
                {
                    lock (this)
                    {
                        if (!__init_TablePartition)
                        {
                            _TablePartition = new DBObjectDistributedTablePartition(this.SchemaAdapter, this.TableName);
                            __init_TablePartition = true;
                        }
                    }
                }
                return _TablePartition;
            }
        }
    }
}
