using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storage.Metadata.MSSQL
{
    /// <summary>
    /// Обеспечивает работу со схемой таблицы DBObjects.
    /// </summary>
    public class DBObjectTableSchemaAdapter : DBTableSchemaAdapter
    {
        public DBObjectTableSchemaAdapter(MetadataTypeDefinition classDefinition, MetadataAdapter schemaAdapter)
        {
            if (classDefinition == null)
                throw new ArgumentNullException("classDefinition");

            if (schemaAdapter == null)
                throw new ArgumentNullException("schemaAdapter");

            this.ClassDefinition = classDefinition;
            this.SchemaAdapter = schemaAdapter;
        }

        private MetadataTypeDefinition _ClassDefinition;
        /// <summary>
        /// Определение метаданных класса.
        /// </summary>
        public MetadataTypeDefinition ClassDefinition
        {
            get { return _ClassDefinition; }
            private set { _ClassDefinition = value; }
        }
        private MetadataAdapter _SchemaAdapter;

        public MetadataAdapter SchemaAdapter
        {
            get { return _SchemaAdapter; }
            set { _SchemaAdapter = value; }
        }

        private bool __init_RootPartition = false;
        private DBObjectTablePartition _RootPartition;
        /// <summary>
        /// 
        /// </summary>
        public DBObjectTablePartition RootPartition
        {
            get
            {
                if (!__init_RootPartition)
                {
                    _RootPartition = new DBObjectTablePartition(this);
                    __init_RootPartition = true;
                }
                return _RootPartition;
            }
        }

        private bool _ArchiveEnabledIniting;
        /// <summary>
        /// Инициализационное значение ArchiveEnabled.
        /// </summary>
        private bool ArchiveEnabledIniting
        {
            get { return _ArchiveEnabledIniting; }
            set { _ArchiveEnabledIniting = value; }
        }       

        protected override string InitTableName()
        {
            return this.ClassDefinition.TableName;
        }

        protected override string InitOriginalTableName()
        {
            return this.ClassDefinition.TableName;
        }

        protected override string InitTablePrefix()
        {
            return this.ClassDefinition.TablePrefix;
        }

        protected override string InitOriginalTablePrefix()
        {
            return this.ClassDefinition.TablePrefix;
        }
        
        protected override DBConnection InitPrimaryDatabaseConnection()
        {
            return this.SchemaAdapter.InitialConnection;
        }

        protected override DBPrincipalTableSchema InitTableSchema()
        {
            return new DBObjectPrincipalTableSchema(this);
        }

        public override DBConnectionContext ConnectionContext
        {
            get { return this.SchemaAdapter.ConnectionContext; }
        }        

        #region New
        
        protected override string InitPartitionTableName()
        {
            return this.ClassDefinition.PartitionTableName;
        }

        protected override string InitOriginalPartitionTableName()
        {
            return this.ClassDefinition.PartitionTableName;
        }

        #endregion
    }
}
