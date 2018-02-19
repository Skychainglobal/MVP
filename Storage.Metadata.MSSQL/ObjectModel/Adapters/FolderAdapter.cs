using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Storage.Lib;
using Storage.Engine;

namespace Storage.Metadata.MSSQL
{
    /// <summary>
    /// Адаптер работы с метаданными папок.
    /// </summary>
    public class DBFolderAdapter : SingleTableObjectAdapter<FolderMetadata>
    {
        /// <summary>
        /// К-тор.
        /// </summary>
        /// <param name="metadataAdapter">Адаптер работы с метаданными хранилища.</param>
        internal DBFolderAdapter(MetadataAdapter metadataAdapter)
            : base(metadataAdapter)
        { }

        /// <summary>
        /// Обновляет или создает новую запись в БД.
        /// </summary>
        /// <param name="folder">Метаданные папки.</param>
        internal void UpdateFolder(FolderMetadata folder)
        {
            if (folder == null)
                throw new ArgumentNullException("folder");

            if (folder.ID == 0)
            {
                if (String.IsNullOrEmpty(folder.Url))
                    throw new Exception("Не задан Url папки.");

                if (!this.IsUniqueFolderUri(folder))
                    throw new Exception(String.Format("Невозможно создать папку с неуникальным Url '{0}'", folder.Url));

                folder.UniqueID = Guid.NewGuid();

                FolderUri urlBuilder = new FolderUri(folder.Url);
                folder.Name = urlBuilder.FolderName;

                if (urlBuilder.ParentUri != null)
                {
                    FolderMetadata parentFolder = this.GetFolder(String.Format("[Url] = N'{0}'", urlBuilder.ParentUri.Url));
                    if (parentFolder != null)
                        folder.ParentID = parentFolder.ID;
                }

                folder.ID = this.InsertObject(folder);
            }
            else
            {
                this.UpdateObject(folder);
            }
        }

        /// <summary>
        /// Получение метаданных папки из БД.
        /// </summary>
        /// <param name="query">Запрос на получение метаданных.</param>
        /// <returns></returns>
        internal FolderMetadata GetFolder(string query)
        {
            if (String.IsNullOrEmpty(query))
                throw new ArgumentNullException("query");

            FolderMetadata metadata = null;

            string resultQuery = @"{SelectQuery} WHERE {Query}"
                .ReplaceKey("SelectQuery", this.SelectQuery)
                .ReplaceKey("Query", query);

            DataRow resultRow = this.DataAdapter.GetDataRow(resultQuery);
            if (resultRow != null)
                metadata = new FolderMetadata(resultRow);

            return metadata;
        }

        /// <summary>
        /// Получение метаданных папок из БД.
        /// </summary>
        /// <param name="query">Запрос на получение метаданных.</param>
        /// <returns></returns>
        internal DBCollection<FolderMetadata> GetFolders(string query)
        {
            DBCollection<FolderMetadata> folders = new DBCollection<FolderMetadata>();
            string resultQuery = null;

            if (String.IsNullOrEmpty(query))
                resultQuery = this.SelectQuery;
            else
                resultQuery = @"{SelectQuery} WHERE {Query}"
                   .ReplaceKey("SelectQuery", this.SelectQuery)
                   .ReplaceKey("Query", query);

            DataTable resultTable = this.DataAdapter.GetDataTable(resultQuery);
            if (resultTable != null)
                foreach (DataRow row in resultTable.Rows)
                {
                    FolderMetadata metadata = new FolderMetadata(row);
                    folders.Add(metadata);
                }

            return folders;
        }

        /// <summary>
        /// Проверка Uri папки на уникальность.
        /// </summary>
        /// <param name="folder">True, если uri папки в таблице уникален.</param>
        /// <returns></returns>
        private bool IsUniqueFolderUri(FolderMetadata folder)
        {
            if (folder == null)
                throw new ArgumentNullException("folder");

            string query = string.Format(@"SELECT COUNT(ID) FROM {0} WITH(NOLOCK) WHERE Url = N'{1}'",
                    this.DBSchemaAdapter.TableName, folder.Url);
            int websCount = this.DataAdapter.GetDataCount(query);
            bool isUnique = websCount == 0;
            return isUnique;
        }
    }
}
