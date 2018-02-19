using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Storage.Engine;

namespace Storage.Metadata.MSSQL
{
    /// <summary>
    /// Адаптер работы с метаданными, хранящихся в распределенных таблицах.
    /// </summary>
    public class DistributedTableAdapter<T> : MetadataObjectAdapter<T>
    {
        /// <summary>
        /// К-тор.
        /// </summary>
        /// <param name="schemaAdapter"></param>        
        public DistributedTableAdapter(MetadataAdapter metadataAdapter)
            : base(metadataAdapter)
        { }

        /// <summary>
        /// Добавляет новый объект в базу данных.
        /// </summary>
        /// <param name="metadataObject">Объект метаданных.</param>
        /// <param name="tableName">Название таблицы</param>
        /// <returns></returns>
        protected int InsertObject(T metadataObject)
        {
            if (metadataObject == null)
                throw new ArgumentNullException("metadataObject");

            //идентификатор созданного объекта.
            int createdID = 0;

            string tableName = this.GetTableName(metadataObject);
            if (String.IsNullOrEmpty(tableName))
                throw new Exception(String.Format("Не удалось определить конечную таблицу для объекта {0} типа {1}", metadataObject.ToString(), metadataObject.GetType().Name));

            DBObjectDistributedTable table = this.MetadataAdapter.TableActivator.EnsureDistributedTable(this.DBSchemaAdapter, tableName);
            MetadataQueryBuilder queryBuilder = new MetadataQueryBuilder(table.TablePartition.Table, this.TypeDefinition);

            string insertQuery = @"
            {InsertQuery}
            SELECT SCOPE_IDENTITY() AS CreatedID, @@ROWCOUNT AS AffectedRowsCount"
                .ReplaceKey("InsertQuery", queryBuilder.InsertQuery);

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

            string tableName = this.GetTableName(metadataObject);
            if (String.IsNullOrEmpty(tableName))
                throw new Exception(String.Format("Не удалось определить конечную таблицу для объекта {0} типа {1}", metadataObject.ToString(), metadataObject.GetType().Name));

            DBObjectDistributedTable table = this.MetadataAdapter.TableActivator.GetDistributedTable(this.DBSchemaAdapter, tableName);
            MetadataQueryBuilder queryBuilder = new MetadataQueryBuilder(table.TablePartition.Table, this.TypeDefinition);

            //приводим объект к типу метаданных.
            IMetadataObject metadata = this.GetMetadataObject(metadataObject);
            if (metadata.ID == 0)
                throw new ArgumentNullException("metadata.ID", "Не передан идентификатор объекта.");

            //формируем условие обновления объекта по идентификатору.
            string identityCondition = string.Format("[ID] = {1}", table.TablePartition.Table.IdentityColumn.Name, metadata.ID);

            //запрос обновления объекта метаданных.
            string updateQuery = @"
{UpdateQuery}
WHERE {IdentityCondition}
SELECT @@ROWCOUNT"
                    .ReplaceKey("UpdateQuery", queryBuilder.UpdateQuery)
                    .ReplaceKey("IdentityCondition", identityCondition)
                    ;


            //формируем параметры обновления, соответствующие значениям свойств объекта.
            DBCollection<SqlParameter> updateParams = this.CreateInsertParameters(metadataObject);

            //выполняем запрос.
            int affectedRowsCount = this.DataAdapter.GetScalarValue<int>(updateQuery, updateParams.ToArray());
            return affectedRowsCount > 0;
        }

        /// <summary>
        /// получения названия конечного таблицы, куда сохраняется объект.
        /// </summary>
        /// <param name="metadataObject">Объект метаданных.</param>
        /// <returns></returns>
        protected virtual string GetTableName(T metadataObject)
        {
            throw new NotImplementedException();
        }
    }
}
