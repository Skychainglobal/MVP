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
    /// Предоставляет методы для работы с метаданными blob-ов.
    /// </summary>
    public class BlobAdapter : SingleTableObjectAdapter<BlobMetadata>
    {
        /// <summary>
        /// К-тор.
        /// </summary>
        /// <param name="schemaAdapter">Адаптер метаданных системы.</param>
        internal BlobAdapter(MetadataAdapter schemaAdapter)
            : base(schemaAdapter)
        { }

        /// <summary>
        /// Обновляет метаданные blob в БД, либо создает новую запись.
        /// </summary>
        /// <param name="blob">Метаданные.</param>
        internal void UpdateBlob(BlobMetadata blob)
        {
            if (blob == null)
                throw new ArgumentNullException("folder");

            if (blob.ID == 0)
            {
                blob.ID = this.InsertObject(blob);
            }
            else
            {
                this.UpdateObject(blob);
            }
        }

        /// <summary>
        /// Возвращает метаданные blob по заданному условию.
        /// </summary>
        /// <param name="query">Sql-запрос на выборку blob.</param>
        /// <returns>Метаданные blob-а соответствующего условию.</returns>
        internal BlobMetadata GetBlob(string query)
        {
            BlobMetadata metadata = null;

            if (String.IsNullOrEmpty(query))
                throw new ArgumentNullException("query");

            string resultQuery = @"{SelectQuery} WHERE {Query}"
                .ReplaceKey("SelectQuery", this.SelectQuery)
                .ReplaceKey("Query", query);

            DataRow resultRow = this.DataAdapter.GetDataRow(resultQuery);
            if (resultRow != null)
                metadata = new BlobMetadata(resultRow);

            return metadata;
        }

        /// <summary>
        /// Получение всех метаданных blob по заданному условию.
        /// </summary>
        /// <param name="query">Sql-запрос на выборку blob.</param>
        /// <returns>Список метаданных blob-ов соответствующих условию.</returns>
        internal List<BlobMetadata> GetBlobs(string query)
        {
            List<BlobMetadata> blobs = new List<BlobMetadata>();

            if (String.IsNullOrEmpty(query))
                throw new ArgumentNullException("query");

            string resultQuery = @"{SelectQuery} WHERE {Query}"
                .ReplaceKey("SelectQuery", this.SelectQuery)
                .ReplaceKey("Query", query);

            DataTable resultTable = this.DataAdapter.GetDataTable(resultQuery);
            if (resultTable != null)
                foreach (DataRow row in resultTable.Rows)
                {
                    BlobMetadata metadata = new BlobMetadata(row);
                    blobs.Add(metadata);
                }

            return blobs;
        }
    }
}
