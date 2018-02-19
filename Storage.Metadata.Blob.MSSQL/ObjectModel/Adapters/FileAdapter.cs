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
    /// Предоставляет методы для работы с метаданными файлов.
    /// </summary>
    public class FileAdapter : DistributedTableAdapter<FileMetadata>
    {
        /// <summary>
        /// К-тор.
        /// </summary>
        /// <param name="schemaAdapter">Адаптер метаданных системы.</param>
        internal FileAdapter(MetadataAdapter schemaAdapter)
            : base(schemaAdapter)
        { }

        private bool __init_VersionAdapter = false;
        private FileVersionsAdapter _VersionAdapter;
        /// <summary>
        /// Адаптер для работы с метаданными версий файлов.
        /// </summary>
        internal FileVersionsAdapter VersionAdapter
        {
            get
            {
                if (!__init_VersionAdapter)
                {
                    _VersionAdapter = new FileVersionsAdapter(this);
                    __init_VersionAdapter = true;
                }
                return _VersionAdapter;
            }
        }

        /// <summary>
        /// Создает новый экземпляр метаданных файлов.
        /// </summary>
        /// <param name="folderMetadata">Метаданные папки.</param>
        /// <param name="fileName">Название файла.</param>
        /// <returns></returns>
        internal FileMetadata CreateFile(IFolderMetadata folderMetadata, string fileName)
        {
            if (folderMetadata == null)
                throw new ArgumentNullException("folderMetadata");

            if (String.IsNullOrEmpty(fileName))
                throw new ArgumentNullException("fileName");

            FileMetadata file = new FileMetadata(this);
            file.UniqueID = Guid.NewGuid();
            file.VersionUniqueID = Guid.NewGuid();
            file.FolderID = folderMetadata.ID;
            file.FolderMetadata = folderMetadata;
            file.Name = fileName;

            return file;
        }

        /// <summary>
        /// Обновляет метаданные файла в БД, либо создает новую запись.
        /// </summary>
        /// <param name="file">Метаданные файла.</param>
        internal FileVersionMetadata UpdateFile(FileMetadata file, DateTime versionTimeCreated)
        {
            if (file == null)
                throw new ArgumentNullException("folder");

            if (versionTimeCreated == null)
                throw new ArgumentNullException("versionTimeCreated");

            if (file.ID == 0)
            {
                file.TimeCreated = versionTimeCreated;
                file.ModifiedUserID = 0;
                file.TimeModified = versionTimeCreated;

                file.ID = this.InsertObject(file);
            }
            else
            {
                if (file.RemoteFile == null)
                    file.TimeModified = versionTimeCreated;

                //если не пришла более ранняя версия файла с удаленного узла,
                //либо пришла версия с локального хранилища, то сохраняем ее
                if (!file.PreventSavePreviousVersion)
                    this.UpdateObject(file);
            }

            //при создании/обновлении создаем новые метаданные версии.
            FileVersionMetadata version = this.VersionAdapter.CreateVerion(file);
            return version;
        }

        /// <summary>
        /// Обновляет метаданные файла в БД, либо создает новую запись.
        /// </summary>
        /// <param name="file">Метаданные файла.</param>
        internal void UpdateFileTransparent(FileMetadata file)
        {
            if (file == null)
                throw new ArgumentNullException("folder");

            if (file.ID == 0)
            {
                file.ID = this.InsertObject(file);
            }
            else
            {
                this.UpdateObject(file);
            }
        }

        /// <summary>
        /// Возвращает метаданные файла по уникальному идентификатору.
        /// </summary>
        /// <param name="uniqueID">Уникальный идентификатор.</param>
        /// <param name="folderMetadata">Метаданные папки в которой располагается файл.</param>
        /// <returns></returns>
        internal FileMetadata GetFile(Guid uniqueID, IFolderMetadata folderMetadata)
        {
            if (uniqueID == Guid.Empty)
                throw new ArgumentNullException("uniqueID");

            if (folderMetadata == null)
                throw new ArgumentNullException("folderMetadata");

            FileMetadata metadata = null;

            string query = String.Format("[UniqueID] = '{0}' AND Deleted != 1", uniqueID);
            string tableName = this.GetTableName(uniqueID, folderMetadata);
            DBObjectDistributedTable table = this.MetadataAdapter.TableActivator.GetDistributedTable(this.DBSchemaAdapter, tableName);
            MetadataQueryBuilder queryBuilder = new MetadataQueryBuilder(table.TablePartition.Table, this.TypeDefinition);

            string resultQuery = @"{SelectQuery} WHERE {Query}"
                .ReplaceKey("SelectQuery", queryBuilder.SelectQuery)
                .ReplaceKey("Query", query);

            DataRow resultRow = this.DataAdapter.GetDataRow(resultQuery);
            if (resultRow != null)
                metadata = new FileMetadata(resultRow, this, folderMetadata);

            return metadata;
        }

        /// <summary>
        /// Удаляет метаданные файла.
        /// </summary>
        /// <param name="file">Метаданные файла.</param>
        internal void DeleteFile(FileMetadata file)
        {
            if (file == null)
                throw new ArgumentNullException("file");

            file.Deleted = true;
            file.TimeModified = DateTime.Now;

            string tableName = this.GetTableName(file.UniqueID, file.FolderMetadata);
            this.UpdateObject(file);
        }

        /// <summary>
        /// Получение названия раздела таблицы по метаданным файла.
        /// </summary>
        /// <param name="file">Метаданные файла.</param>
        /// <returns>Название раздела таблицы БД.</returns>
        protected override string GetTableName(FileMetadata file)
        {
            if (file == null)
                throw new ArgumentNullException("file");

            return this.GetTableName(file.UniqueID, file.FolderMetadata);
        }

        /// <summary>
        /// Получение названия раздела таблицы по уникальному идентификатору файла и метаданным папки.
        /// </summary>
        /// <param name="uniqueID">Идентификатор файла.</param>
        /// <param name="folder">Метаданные папки.</param>
        /// <returns>Название раздела таблицы БД.</returns>
        internal string GetTableName(Guid uniqueID, IFolderMetadata folder)
        {
            if (uniqueID == Guid.Empty)
                throw new ArgumentNullException("file");

            if (folder == null)
                throw new ArgumentNullException("folder");

            int tableIndex = Math.Abs(uniqueID.GetHashCode() % 10);
            return String.Format("{0}_{1}_{2}", this.DBSchemaAdapter.TableName, folder.Url.Trim("/".ToCharArray()).Replace('/', '_'), tableIndex != 0 ? tableIndex : 10);
        }

        public string GetTableName(IFileMetadata metadata)
        {
            if (metadata == null)
                throw new ArgumentNullException("metadata");

            FileMetadata typedMetadata = (FileMetadata)metadata;
            return this.GetTableName(typedMetadata);
        }
    }
}
