using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storage.Metadata.MSSQL
{
    /// <summary>
    /// Атрибут класса, представляющего метаданные хранилища.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class MetadataClassAttribute : Attribute
    {
        /// <summary>
        /// К-тор.
        /// </summary>
        /// <param name="tableName">Название таблицы.</param>
        public MetadataClassAttribute(string tableName)
        {
            this.TableName = tableName;
        }

        private string _TableName;
        /// <summary>
        /// Название таблицы.
        /// </summary>
        public string TableName
        {
            get { return _TableName; }
            set { _TableName = value; }
        }

        private bool __init_TablePrefix = false;
        private string _TablePrefix;
        /// <summary>
        /// Прфикс таблицы в БД. 
        /// По умолчанию название таблицы.
        /// </summary>
        public string TablePrefix
        {
            get
            {
                if (!__init_TablePrefix)
                {
                    _TablePrefix = this.TableName;
                    __init_TablePrefix = true;
                }
                return _TablePrefix;
            }
            set
            {
                _TablePrefix = value;
                __init_TablePrefix = true;
            }
        }

        private bool __init_PartitionTableName = false;
        private string _PartitionTableName;
        /// <summary>
        /// Название раздела таблицы в БД. 
        /// По умолчанию название таблицы.
        /// </summary>
        public string PartitionTableName
        {
            get
            {
                if (!__init_PartitionTableName)
                {
                    _PartitionTableName = this.TableName;
                    __init_PartitionTableName = true;
                }
                return _PartitionTableName;
            }
        }

        private Type[] _TriggerTypes;
        /// <summary>
        /// Коллекция типов триггеров, создаваемых в таблице основных данных.
        /// </summary>
        public Type[] TriggerTypes
        {
            get { return _TriggerTypes; }
            set { _TriggerTypes = value; }
        }
    }
}
