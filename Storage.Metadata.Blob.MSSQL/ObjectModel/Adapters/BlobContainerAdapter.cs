using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Storage.Data.Blob;
using Storage.Metadata.MSSQL;

namespace Storage.Metadata.Blob.MSSQL
{
    /// <summary>
    /// Предоставляет методы для работы с метаданными контейнеров blob.
    /// </summary>
    internal class BlobContainerAdapter : SingleTableObjectAdapter<BlobContainerMetadata>
    {
        /// <summary>
        /// К-тор.
        /// </summary>
        /// <param name="schemaAdapter">Адаптер метаданных системы.</param>
        internal BlobContainerAdapter(MetadataAdapter schemaAdapter)
            : base(schemaAdapter)
        { }

        /// <summary>
        /// Обновляет метаданные контейнера blob в БД, либо создает новую запись.
        /// </summary>
        /// <param name="container">Контейнер blob.</param>
        internal void UpdateBlobContainer(BlobContainerMetadata container)
        {
            if (container == null)
                throw new ArgumentNullException("folder");

            if (container.ID == 0)
            {
                container.ID = this.InsertObject(container);
            }
            else
            {
                this.UpdateObject(container);
            }
        }

        /// <summary>
        /// Возвращает метаданные контейнера blob по заданному условию.
        /// </summary>
        /// <param name="query">Sql-запрос на выборку контейнеров blob.</param>
        /// <returns>Метаданные blob-контейнера соответствующего условию.</returns>
        internal BlobContainerMetadata GetBlobContainer(string query)
        {
            BlobContainerMetadata metadata = null;

            if (String.IsNullOrEmpty(query))
                throw new ArgumentNullException("query");

            string resultQuery = @"{SelectQuery} WHERE {Query}"
                .ReplaceKey("SelectQuery", this.SelectQuery)
                .ReplaceKey("Query", query);

            DataRow resultRow = this.DataAdapter.GetDataRow(resultQuery);
            if (resultRow != null)
                metadata = new BlobContainerMetadata(resultRow);

            return metadata;
        }

        /// <summary>
        ///  Получение всех метаданных контейнеров blob по заданному условию.
        /// </summary>
        /// <param name="query">Sql-запрос на выборку контейнеров blob.</param>
        /// <returns>Список метаданных blob-контейнеров соответствующих условию.</returns>
        internal List<BlobContainerMetadata> GetBlobContainers(string query = null)
        {
            List<BlobContainerMetadata> containers = new List<BlobContainerMetadata>();

            string resultQuery = @"{SelectQuery}";
            if (!string.IsNullOrEmpty(query))
                resultQuery += @" WHERE {Query}";

            resultQuery = resultQuery
                .ReplaceKey("SelectQuery", this.SelectQuery)
                .ReplaceKey("Query", query);

            DataTable resultTable = this.DataAdapter.GetDataTable(resultQuery);
            if (resultTable != null)
            {
                foreach (DataRow row in resultTable.Rows)
                {
                    BlobContainerMetadata metadata = new BlobContainerMetadata(row);
                    containers.Add(metadata);
                }
            }

            return containers;
        }
    }
}
