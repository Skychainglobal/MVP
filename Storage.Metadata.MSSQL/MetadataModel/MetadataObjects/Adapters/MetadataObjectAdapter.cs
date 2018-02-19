using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storage.Metadata.MSSQL
{
    /// <summary>
    /// Базовый адаптер объектов метаданных хранилища.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class MetadataObjectAdapter<T>
    {
        /// <summary>
        /// К-тор.
        /// </summary>
        /// <param name="metadataAdapter">Адаптер схемы метаданных.</param>
        /// <param name="connection">Подключение к БД.</param>
        protected MetadataObjectAdapter(MetadataAdapter metadataAdapter)
        {
            if (metadataAdapter == null)
                throw new ArgumentNullException("MetadataAdapter");

            this.MetadataAdapter = metadataAdapter;
        }

        private MetadataAdapter _SchemaAdapter;
        /// <summary>
        /// Адаптер схемы метаданных.
        /// </summary>
        public MetadataAdapter MetadataAdapter
        {
            get { return _SchemaAdapter; }
            set { _SchemaAdapter = value; }
        }

        private bool __init_Type = false;
        private Type _Type;
        /// <summary>
        /// Тип метаданных, с которым работает адаптер.
        /// </summary>
        protected Type Type
        {
            get
            {
                if (!__init_Type)
                {
                    _Type = typeof(T);
                    __init_Type = true;
                }
                return _Type;
            }
        }

        private bool __init_TypeDefinition = false;
        private MetadataTypeDefinition _TypeDefinition;
        /// <summary>
        /// Определение типа метаданных хранилища.
        /// </summary>
        protected MetadataTypeDefinition TypeDefinition
        {
            get
            {
                if (!__init_TypeDefinition)
                {
                    _TypeDefinition = this.MetadataAdapter.GetTypeDefinition(this.Type);
                    __init_TypeDefinition = true;
                }
                return _TypeDefinition;
            }
        }

        private bool __init_DataAdapter = false;
        private DBAdapter _DataAdapter;
        /// <summary>
        /// Адаптер запросов к БД.
        /// </summary>
        protected DBAdapter DataAdapter
        {
            get
            {
                if (!__init_DataAdapter)
                {
                    _DataAdapter = new DBAdapter(this.MetadataAdapter.InitialConnection);
                    __init_DataAdapter = true;
                }
                return _DataAdapter;
            }
        }

        private bool __init_DBSchemaAdapter = false;
        private DBObjectTableSchemaAdapter _DBSchemaAdapter;
        /// <summary>
        /// Адаптер схемы метаданных.
        /// </summary>
        protected DBObjectTableSchemaAdapter DBSchemaAdapter
        {
            get
            {
                if (!__init_DBSchemaAdapter)
                {
                    _DBSchemaAdapter = new DBObjectTableSchemaAdapter(this.TypeDefinition, this.MetadataAdapter);
                    __init_DBSchemaAdapter = true;
                }
                return _DBSchemaAdapter;
            }
        }

        /// <summary>
        /// Создает набор Sql-параметров для вставки объекта в базу данных.
        /// </summary>
        /// <param name="metadataObject">Объект метаданных для вставки.</param>
        /// <returns></returns>
        protected DBCollection<SqlParameter> CreateInsertParameters(T metadataObject)
        {
            return this.CreateParameters(metadataObject, this.TypeDefinition.ParameterProperties);
        }

        private DBCollection<SqlParameter> CreateParameters(T metadataObject, DBCollection<MetadataPropertyDefinition> metadataProperties)
        {
            if (metadataObject == null)
                throw new ArgumentNullException("metadataObject");

            DBCollection<SqlParameter> parameters = new DBCollection<SqlParameter>();
            foreach (MetadataPropertyDefinition property in metadataProperties)
            {
                object value = property.Property.GetValue(metadataObject, null);

                DBColumnSchema columnSchema = this.DBSchemaAdapter.TableSchema.GetColumn(property.ColumnName, true);
                SqlParameter param = new SqlParameter(property.ParameterName, columnSchema.Type);
                param.Value = this.EnsureParameterValue(value);
                parameters.Add(param);
            }
            return parameters;
        }

        /// <summary>
        /// Проверяет устанавливаемое в качестве Sql-параметра значение и возвращает DBNull, если значение является пустым.
        /// </summary>
        /// <param name="paramValue">Значение, устанавливаемое в качестве значения Sql-параметра.</param>
        /// <returns></returns>
        protected object EnsureParameterValue(object paramValue)
        {
            if (paramValue == null || paramValue.Equals(DateTime.MinValue))
                return DBNull.Value;
            return paramValue;
        }

        /// <summary>
        /// Возвращает интерфейс объекта метаданных.
        /// </summary>
        /// <param name="metadataObject">Объект метаданных.</param>
        /// <returns></returns>
        protected IMetadataObject GetMetadataObject(T metadataObject)
        {
            //проверяем наличие определения интерфейса метаданных.
            this.TypeDefinition.CheckIsMetadataObject();

            //получаем экземпляр информации о метаданных объекта.
            object metadataObj = metadataObject;
            IMetadataObject metadataObjInterface = (IMetadataObject)metadataObj;

            //возвращаем интерфейс метаданных.
            return metadataObjInterface;
        }
    }
}
