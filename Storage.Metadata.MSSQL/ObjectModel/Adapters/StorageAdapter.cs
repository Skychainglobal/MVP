using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storage.Metadata.MSSQL
{
    /// <summary>
    /// Адаптер работы с метаданными объектов хранилищ.
    /// </summary>
    public class StorageAdapter : SingleTableObjectAdapter<StorageMetadata>
    {
        /// <summary>
        /// К-тор.
        /// </summary>
        /// <param name="metadataAdapter">Адаптер метаданных.</param>
        internal StorageAdapter(MetadataAdapter metadataAdapter)
            : base(metadataAdapter)
        { }

        /// <summary>
        /// Получение текущего хранилища метаданных.
        /// </summary>
        /// <returns>Объект, соответствующей первой записи в БД.</returns>
        internal StorageMetadata GetCurrent()
        {
            string condition = string.Format("[IsCurrent] = 1");
            StorageMetadata metadata = this.GetStorage(condition);
            return metadata;
        }

        /// <summary>
        /// Получение объекта хранилища по условию.
        /// </summary>
        /// <param name="condition">Условие.</param>
        /// <returns></returns>
        internal StorageMetadata GetStorage(string condition)
        {
            string resultQuery = this.SelectQuery;
            if (!string.IsNullOrEmpty(condition))
                resultQuery += string.Format("WHERE {0}", condition);

            StorageMetadata metadata = null;
            DataRow resultRow = this.DataAdapter.GetDataRow(resultQuery);
            if (resultRow != null)
                metadata = new StorageMetadata(resultRow);

            return metadata;
        }

        /// <summary>
        /// Получение коллекции объектов хранилища по условию.
        /// </summary>
        /// <param name="condition">Условие.</param>
        /// <returns></returns>
        internal DBCollection<StorageMetadata> GetStorages(string condition)
        {
            string resultQuery = this.SelectQuery;
            if (!string.IsNullOrEmpty(condition))
                resultQuery += string.Format("WHERE {0}", condition);

            DataTable resultTable = this.DataAdapter.GetDataTable(resultQuery);
            DBCollection<StorageMetadata> result = new DBCollection<StorageMetadata>();
            if (resultTable != null)
            {
                foreach (DataRow resultRow in resultTable.Rows)
                {
                    StorageMetadata metadata = new StorageMetadata(resultRow);
                    result.Add(metadata);
                }
            }

            return result;
        }

        /// <summary>
        /// Получение объектов по идентификаторам.
        /// </summary>
        /// <param name="identities">Идентификаторы хранилищ.</param>
        /// <returns></returns>
        internal DBCollection<StorageMetadata> GetStorages(IEnumerable<int> identities)
        {
            DBCollection<StorageMetadata> result;
            string condition = null;

            if (identities != null && identities.Count() > 0)
            {
                string queryCondition = String.Format("[ID] in ({0})", String.Join(",", identities.Select(x => x.ToString()).ToArray()));
            }

            result = this.GetStorages(condition);
            return result;
        }

        /// <summary>
        /// Обновление либо создание новой записи в БД.
        /// </summary>
        /// <param name="storage">Объект метаданных.</param>
        internal void Update(StorageMetadata storage)
        {
            if (storage == null)
                throw new ArgumentNullException("storage");

            if (storage.ID == 0)
            {
                if (string.IsNullOrEmpty(storage.Host))
                    throw new Exception(string.Format("Для хранилища должен быть задан Host"));

                if (storage.UniqueID == Guid.Empty)
                    storage.UniqueID = new Guid();

                storage.ID = this.InsertObject(storage);
            }
            else
                this.UpdateObject(storage);
        }
    }
}
