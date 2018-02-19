using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Storage.Engine;
using Storage.Metadata.MSSQL;

namespace Storage.Metadata.Blob.MSSQL
{
    /// <summary>
    /// Предоставляет методы работы с метаданными версий файлов.
    /// </summary>
    internal class FileVersionsAdapter : FileRelativeObjectAdapter<FileVersionMetadata>
    {
        /// <summary>
        /// К-тор.
        /// </summary>
        /// <param name="fileAdapter">Адаптер метаданных файлов.</param>
        internal FileVersionsAdapter(FileAdapter fileAdapter)
            : base(fileAdapter.MetadataAdapter)
        {
            if (fileAdapter == null)
                throw new ArgumentNullException("fileAdapter");

            this.FileAdapter = fileAdapter;
        }

        private FileAdapter _FileAdapter;
        /// <summary>
        /// Адаптер метаданных файлов.
        /// </summary>
        public FileAdapter FileAdapter
        {
            get { return _FileAdapter; }
            set { _FileAdapter = value; }
        }

        /// <summary>
        /// Создает новые метаданные версии файла и записывает в БД.
        /// </summary>
        /// <param name="file">Метаданные файла.</param>
        internal FileVersionMetadata CreateVerion(FileMetadata file)
        {
            if (file == null)
                throw new ArgumentNullException("file");

            FileVersionMetadata fileVersion = new FileVersionMetadata(file);

            if (file.RemoteFile == null)
                fileVersion.CreatedStorageID = this.FileAdapter.MetadataAdapter.CurrentStorage.ID;
            else
            {
                //файл с удаленного узла
                fileVersion.CreatedStorageID = file.RemoteFile.CreatedStorageNode.ID;
            }

            string tableName = String.Format("{0}_Versions", this.FileAdapter.GetTableName(file.UniqueID, file.FolderMetadata));

            this.InsertObject(fileVersion);
            file.ResetVersions();

            return fileVersion;
        }

        /// <summary>
        /// Создает новые метаданные версии файла и записывает в БД.
        /// </summary>
        /// <param name="file">Метаданные файла.</param>
        internal void InsertVerion(FileMetadata file, FileVersionMetadata fileVersion)
        {
            if (file == null)
                throw new ArgumentNullException("file");

            if (fileVersion == null)
                throw new ArgumentNullException("fileVersion");

            string tableName = String.Format("{0}_Versions", this.FileAdapter.GetTableName(file.UniqueID, file.FolderMetadata));

            this.InsertObject(fileVersion);
            file.ResetVersions();
        }

        /// <summary>
        /// Возвращает метаданные всех версий файла.
        /// </summary>
        /// <param name="file">Метаданные файла.</param>
        /// <returns>Коллекцию метаданных версий файла.</returns>
        internal FileVersionsCollection GetFileVersions(FileMetadata file)
        {
            if (file == null)
                throw new ArgumentNullException("file");

            FileVersionsCollection versions = new FileVersionsCollection(file);

            string query = String.Format("[FileID] = '{0}'", file.ID);
            string tableName = this.GetTableName(file);

            DBObjectDistributedTable table = this.MetadataAdapter.TableActivator.GetDistributedTable(this.DBSchemaAdapter, tableName);
            MetadataQueryBuilder queryBuilder = new MetadataQueryBuilder(table.TablePartition.Table, this.TypeDefinition);

            string resultQuery = @"{SelectQuery} WHERE {Query} ORDER BY [TimeCreated] ASC"
                .ReplaceKey("SelectQuery", queryBuilder.SelectQuery)
                .ReplaceKey("Query", query);

            DataTable resultTable = this.DataAdapter.GetDataTable(resultQuery);
            if (resultTable != null)
                foreach (DataRow row in resultTable.Rows)
                {
                    FileVersionMetadata metadata = new FileVersionMetadata(row, versions);
                    versions.AddVersion(metadata);
                }

            return versions;
        }
    }
}
