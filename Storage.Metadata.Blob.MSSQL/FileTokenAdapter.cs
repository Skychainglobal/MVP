using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Storage.Engine;
using Storage.Metadata.MSSQL;

namespace Storage.Metadata.Blob.MSSQL
{
    /// <summary>
    /// Предоставляет методы для работы с токенами файлов.
    /// </summary>
    public class FileTokenAdapter : FileRelativeObjectAdapter<FileToken>, IFileTokenAdapter
    {
        /// <summary>
        /// К-тор.
        /// </summary>
        /// <param name="metadataAdapter">Адаптер работы с метаданными системы.</param>
        public FileTokenAdapter(MetadataAdapter metadataAdapter)
            : base(metadataAdapter)
        {
            if (metadataAdapter == null)
                throw new ArgumentNullException("metadataAdapter");

            this.MetadataAdapter = metadataAdapter;
            object checkObj = this.TypedMetadataAdapter;
        }

        private IMetadataAdapter _MetadataAdapter;
        /// <summary>
        /// Адаптер работы с метаданными системы.
        /// </summary>
        public IMetadataAdapter MetadataAdapter
        {
            get { return _MetadataAdapter; }
            set { _MetadataAdapter = value; }
        }

        private bool __init_TypedMetadataAdapter = false;
        private MetadataAdapter _TypedMetadataAdapter;
        /// <summary>
        /// Реализация адаптера для MSSQL.
        /// </summary>
        private MetadataAdapter TypedMetadataAdapter
        {
            get
            {
                if (!__init_TypedMetadataAdapter)
                {
                    if (this.MetadataAdapter is MetadataAdapter)
                        _TypedMetadataAdapter = this.MetadataAdapter as MetadataAdapter;
                    else
                        throw new Exception("");
                    __init_TypedMetadataAdapter = true;
                }
                return _TypedMetadataAdapter;
            }
        }

        private bool __init_Logger;
        private ILogProvider _Logger;
        /// <summary>
        /// Логгер.
        /// </summary>
        internal ILogProvider Logger
        {
            get
            {
                if (!__init_Logger)
                {
                    _Logger = ConfigFactory.Instance.Create<ILogProvider>(BlobMetadataConsts.Scopes.FileTokenAdapter);
                    __init_Logger = true;
                }
                return _Logger;
            }
        }

        /// <summary>
        /// Проверяет наличие уже выданного токена на файл и при отсутствии создает новый.
        /// </summary>
        /// <param name="fileVersionMetadata">Метаданные версии файла.</param>
        /// <returns>Токен доступа к версии файла.</returns>
        private FileToken EnsureFileToken(IFileVersionMetadata fileVersionMetadata)
        {
            if (fileVersionMetadata == null)
                throw new ArgumentNullException("fileVersionMetadata");

            this.Logger.WriteFormatMessage("EnsureFileToken: Начало операции. fileUniqueID: {0}, folderUrl: {1}",
                fileVersionMetadata.FileMetadata.UniqueID, fileVersionMetadata.FileMetadata.FolderMetadata.Url);

            FileToken fileToken = null;

            this.Logger.WriteMessage("EnsureFileToken: Получение таблицы.");

            string tableName = this.GetTableName(fileVersionMetadata.FileMetadata);
            DBObjectDistributedTable table = this.TypedMetadataAdapter.TableActivator.EnsureDistributedTable(this.DBSchemaAdapter, tableName);
            MetadataQueryBuilder queryBuilder = new MetadataQueryBuilder(table.TablePartition.Table, this.TypeDefinition);

            this.Logger.WriteMessage("EnsureFileToken: Конец получения таблицы.");

            string resultQuery = @"DELETE 
FROM {TableName} 
WHERE [Expired] < GETDATE()
{SelectQuery}
WHERE [FileID] = {FileID} AND [VersionID] = {VersionID} AND ([SecurityIdentifier] IS NULL OR [SecurityIdentifier] = '')"
                .ReplaceKey("TableName", tableName)
                .ReplaceKey("SelectQuery", queryBuilder.SelectQuery)
                .ReplaceKey("FileID", fileVersionMetadata.FileMetadata.ID)
                .ReplaceKey("VersionID", fileVersionMetadata.ID);

            this.Logger.WriteFormatMessage("EnsureFileToken: Начало запроса в БД. Запрос:{0}", resultQuery);
            DataRow tokenRow = this.DataAdapter.GetDataRow(resultQuery);
            this.Logger.WriteMessage("EnsureFileToken: Конец запроса в БД.");

            if (tokenRow != null)
                fileToken = new FileToken(tokenRow, fileVersionMetadata);
            else
                fileToken = this.CreateToken(fileVersionMetadata, null);

            this.Logger.WriteMessage("EnsureFileToken: Конец операции.");

            return fileToken;
        }

        /// <summary>
        /// Создает новый токен доступа к файлу и записывает в БД.
        /// </summary>
        /// <param name="fileVersionMetadata">Метаданные версии файла.</param>
        /// <param name="securityIdentifier">Идентификатор безопасности токена.</param>
        /// <returns>Новый токен доступа.</returns>
        private FileToken CreateToken(IFileVersionMetadata fileVersionMetadata, string securityIdentifier)
        {
            if (fileVersionMetadata == null)
                throw new ArgumentNullException("fileVersionMetadata");

            this.Logger.WriteFormatMessage("CreateToken: Начало операции создания токена. fileUniqueID: {0}, folderUrl: {1}, securityIdentifier: {2}",
                fileVersionMetadata.FileMetadata.UniqueID, fileVersionMetadata.FileMetadata.FolderMetadata.Url, securityIdentifier ?? String.Empty);

            FileToken fileToken = new FileToken(null, fileVersionMetadata);
            fileToken.FileID = fileVersionMetadata.FileMetadata.ID;
            fileToken.VersionID = fileVersionMetadata.ID;
            fileToken.Expired = DateTime.Now.AddDays(1);
            fileToken.UniqueID = Guid.NewGuid();
            fileToken.SecurityIdentifier = securityIdentifier;

            this.Logger.WriteMessage("CreateToken: Сохранение токена в БД.");
            fileToken.ID = this.InsertObject(fileToken);
            this.Logger.WriteMessage("CreateToken: Конец операции.");

            return fileToken;
        }

        #region IFileTokenAdapter members

        /// <summary>
        /// Метод генерации токена файла.
        /// </summary>
        /// <param name="fileVersionMetadata">Метаданные версии файла.</param>
        /// <param name="securityIdentifier">Идентификатор безопасности токена.</param>
        /// <returns></returns>
        public IFileToken GenerateFileToken(IFileVersionMetadata fileVersionMetadata, string securityIdentifier)
        {
            //проверяем наличие токена и создаем при необходимости
            if (String.IsNullOrEmpty(securityIdentifier))
                return this.EnsureFileToken(fileVersionMetadata);
            //при выдаче защищенного доступа всегда генерируем новый токен
            else
                return this.CreateToken(fileVersionMetadata, securityIdentifier);
        }

        /// <summary>
        /// Получение токена доступа к версии файла по уникальносму идентификатору.
        /// </summary>
        /// <param name="fileVersionMetadata">Метаданные версии файла.</param>
        /// <param name="tokenUniqueID">Уникальный идентификатор токена.</param>
        /// <returns></returns>
        public IFileToken GetToken(IFileVersionMetadata fileVersionMetadata, Guid tokenUniqueID)
        {
            FileToken token = null;

            this.Logger.WriteFormatMessage("GetToken: Начало операции получения токена.  tokenUniqueID='{0}' fileUniqueID='{1}' folderUrl='{2}'",
                tokenUniqueID, fileVersionMetadata.FileMetadata.UniqueID, fileVersionMetadata.FileMetadata.FolderMetadata.Url);

            string tableName = this.GetTableName(fileVersionMetadata.FileMetadata);
            DBObjectDistributedTable table = null;

            this.Logger.WriteMessage("GetToken: Получение таблицы.");
            try
            {
                table = this.TypedMetadataAdapter.TableActivator.GetDistributedTable(this.DBSchemaAdapter, tableName);
            }
            //В случае когда для данного файла и папки генерация токена не запрашивалась, таблица токенов могла быть не создана.
            //В данном случае ошибки нет, т.к. токен еще не генерировался.
            catch (DistributedTableNotFoundException notFoundEx)
            {
                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            this.Logger.WriteMessage("GetToken: Конец получения таблицы.");

            MetadataQueryBuilder queryBuilder = new MetadataQueryBuilder(table.TablePartition.Table, this.TypeDefinition);

            string query = @"
{SelectQuery}  
WHERE [UniqueID] = '{UniqueID}' AND [Expired] > GETDATE()"
                .ReplaceKey("SelectQuery", queryBuilder.SelectQuery)
                .ReplaceKey("UniqueID", tokenUniqueID);

            this.Logger.WriteFormatMessage("GetToken: Начало запроса в БД.  Запрос:'{0}'", query);
            DataRow resultRow = this.DataAdapter.GetDataRow(query);
            this.Logger.WriteMessage("GetToken: Конец запроса в БД.");

            if (resultRow != null)
                token = new FileToken(resultRow, fileVersionMetadata);

            this.Logger.WriteMessage("GetToken: Конец.");

            return token;
        }

        #endregion
    }
}
