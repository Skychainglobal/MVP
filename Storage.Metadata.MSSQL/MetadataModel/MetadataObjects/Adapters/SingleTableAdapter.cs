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
    /// Адаптер работы с метаданными, сохраняющихся в единственную таблицу.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class SingleTableObjectAdapter<T> : MetadataObjectAdapter<T>
    {
        /// <summary>
        /// К-тор.
        /// </summary>
        /// <param name="schemaAdapter">Адаптер схемы метаданных.</param>
        /// <param name="connection">Подключение к БД.</param>
        protected SingleTableObjectAdapter(MetadataAdapter schemaAdapter)
            : base(schemaAdapter)
        {
            this.Init();
        }

        private bool __init_QueryBuilder = false;
        private MetadataQueryBuilder _QueryBuilder;
        /// <summary>
        /// Посторитель запросов к единственной таблице.
        /// </summary>
        public MetadataQueryBuilder QueryBuilder
        {
            get
            {
                if (!__init_QueryBuilder)
                {
                    _QueryBuilder = new MetadataQueryBuilder(this.DBSchemaAdapter.RootPartition.Table, this.TypeDefinition);
                    __init_QueryBuilder = true;
                }
                return _QueryBuilder;
            }
        }

        private bool __init_IdentityColumn = false;
        private DBColumn _IdentityColumn;
        /// <summary>
        /// Столбец идентификатор таблицы.
        /// </summary>
        protected DBColumn IdentityColumn
        {
            get
            {
                if (!__init_IdentityColumn)
                {
                    _IdentityColumn = this.DBSchemaAdapter.RootPartition.Table.IdentityColumn;
                    if (_IdentityColumn == null)
                        throw new Exception(string.Format("Отсутсвует инкрементный столбец идентификатор для таблицы {0}.", this.DBSchemaAdapter.RootPartition.Table.Name));
                    __init_IdentityColumn = true;
                }
                return _IdentityColumn;
            }
        }

        /// <summary>
        /// Инициализирует адаптер метаданных.
        /// </summary>
        protected internal virtual void Init()
        {
            this.InitSchema();
        }

        private static object InitLock = new object();

        /// <summary>
        /// Инициализирует и проверяет схему таблицы в БД.
        /// </summary>
        protected void InitSchema()
        {
            lock (InitLock)
            {
                this.DBSchemaAdapter.RootPartition.Table.Init();
            }
        }

        /// <summary>
        /// Запрос вставки данных в таблицу типа метаданных.
        /// </summary>
        protected string InsertQuery
        {
            get { return this.QueryBuilder.InsertQuery; }
        }

        /// <summary>
        /// Запрос обновления данных в таблице типа метаданных.
        /// </summary>
        protected string UpdateQuery
        {
            get { return this.QueryBuilder.UpdateQuery; }
        }

        /// <summary>
        /// Запрос выборки данных из таблицы типа метаданных.
        /// </summary>
        protected string SelectQuery
        {
            get { return this.QueryBuilder.SelectQuery; }
        }

        protected int InsertObject(T metadataObject)
        {
            //идентификатор созданного объекта.
            int createdID = 0;

            string insertQuery = @"
            {InsertQuery}
            SELECT SCOPE_IDENTITY() AS CreatedID, @@ROWCOUNT AS AffectedRowsCount"
                .ReplaceKey("InsertQuery", this.InsertQuery);

            DBCollection<SqlParameter> insertParams = this.CreateInsertParameters(metadataObject);
            DataRow insertResult = this.DataAdapter.GetDataRow(insertQuery, insertParams.ToArray());
            if (insertResult == null)
                throw new Exception(string.Format("Не удалось получить результат создания объекта."));

            DataRowReader insertResultReader = new DataRowReader(insertResult);
            int affectedRowsCount = insertResultReader.GetIntegerValue("AffectedRowsCount");

            //если объект по заданному условию уже существовал, устанавливаем идентификатор созданного элемента равный -1.
            if (affectedRowsCount == 0)
                createdID = -1;
            //проставляем свойства созданного списка
            else
            {
                createdID = insertResultReader.GetIntegerValue("CreatedID");
                if (createdID == 0)
                    throw new Exception("Не удалось получить идентификатор созданного объекта.");
            }

            //возвращаем идентификатор созданного объекта.
            return createdID;
        }

        /// <summary>
        /// Обновляет существующий объект в базе данных.
        /// </summary>
        /// <param name="metadataObject">Объект метаданных.</param>
        /// <returns></returns>
        protected bool UpdateObject(T metadataObject)
        {
            if (metadataObject == null)
                throw new ArgumentNullException("metadataObject");

            //приводим объект к типу метаданных.
            IMetadataObject metadata = this.GetMetadataObject(metadataObject);
            if (metadata.ID == 0)
                throw new ArgumentNullException("metadata.ID", "Не передан идентификатор объекта.");

            //формируем условие обновления объекта по идентификатору.
            string identityCondition = string.Format("[ID] = {1}", this.IdentityColumn.Name, metadata.ID);

            //запрос обновления объекта метаданных.
            string updateQuery = @"
{UpdateQuery}
WHERE {IdentityCondition}
SELECT @@ROWCOUNT"
                    .ReplaceKey("UpdateQuery", this.UpdateQuery)
                    .ReplaceKey("IdentityCondition", identityCondition)
                    ;


            //формируем параметры обновления, соответствующие значениям свойств объекта.
            DBCollection<SqlParameter> updateParams = this.CreateInsertParameters(metadataObject);

            //выполняем запрос.
            int affectedRowsCount = this.DataAdapter.GetScalarValue<int>(updateQuery, updateParams.ToArray());
            return affectedRowsCount > 0;
        }
    }
}
