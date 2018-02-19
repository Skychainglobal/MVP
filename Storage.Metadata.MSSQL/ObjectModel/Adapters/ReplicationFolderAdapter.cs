using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Storage.Lib;

namespace Storage.Metadata.MSSQL
{
    /// <summary>
    /// Адаптер для объектов репликации папок.
    /// </summary>
    internal class ReplicationFolderAdapter : SingleTableObjectAdapter<ReplicationFolderMetadata>
    {
        /// <summary>
        /// К-тор.
        /// </summary>
        /// <param name="metadataAdapter">Адаптер работы с метаданными хранилища.</param>
        internal ReplicationFolderAdapter(MetadataAdapter metadataAdapter)
            : base(metadataAdapter)
        { }

        /// <summary>
        /// Обновляет или создает новую папку репликации.
        /// </summary>
        /// <param name="replicationFolder">Метаданные папки репликации.</param>
        internal void UpdateReplicationFolder(ReplicationFolderMetadata replicationFolder)
        {
            if (replicationFolder == null)
                throw new ArgumentNullException("replicationFolder");

            if (replicationFolder.ID == 0)
            {
                if (!this.IsUnique(replicationFolder))
                    throw new Exception(String.Format("Невозможно создать папку репликации Url {0} для узла назначения с идентификатором {1}. Данный элемент репликации уже существует.",
                        replicationFolder.Folder.Url,
                        replicationFolder.SourceStorage.UniqueID));

                replicationFolder.ID = this.InsertObject(replicationFolder);
            }
            else
            {
                this.UpdateObject(replicationFolder);
            }
        }

        /// <summary>
        /// Возвращает признак уникальности папки репликации.
        /// </summary>
        /// <param name="replicationFolder">Папка репликации.</param>
        /// <returns></returns>
        private bool IsUnique(ReplicationFolderMetadata replicationFolder)
        {
            if (replicationFolder == null)
                throw new ArgumentNullException("replicationFolder");

            string query = string.Format(@"SELECT COUNT(ID) FROM {0} WITH(NOLOCK) WHERE [StorageID] = {1} AND [FolderID] = {2}",
                    this.DBSchemaAdapter.TableName,
                    replicationFolder.StorageID,
                    replicationFolder.FolderID);
            int foldersCount = this.DataAdapter.GetDataCount(query);
            bool isUnique = foldersCount == 0;
            return isUnique;
        }

        /// <summary>
        /// Возвращает коллекцию папок репликации.
        /// </summary>
        /// <param name="condition">Условие на выборку метаданных.</param>
        /// <returns></returns>
        internal DBCollection<ReplicationFolderMetadata> GetReplicationFolders(string condition = null)
        {
            DBCollection<ReplicationFolderMetadata> replicationFolders = new DBCollection<ReplicationFolderMetadata>();
            string resultQuery = null;

            if (string.IsNullOrEmpty(condition))
                resultQuery = this.SelectQuery;
            else
                resultQuery = @"{SelectQuery} WHERE {Condition}"
                   .ReplaceKey("SelectQuery", this.SelectQuery)
                   .ReplaceKey("Condition", condition);

            DataTable resultTable = this.DataAdapter.GetDataTable(resultQuery);
            if (resultTable != null)
            {
                //собираем все идентификаторы папок и узлов хранилища
                //чтобы получить их за отдельный запрос
                Dictionary<int, FolderMetadata> uniqueFolders = new Dictionary<int, FolderMetadata>();
                Dictionary<int, StorageMetadata> uniqueStorages = new Dictionary<int, StorageMetadata>();

                //сначала заполняем ключи словарей
                foreach (DataRow row in resultTable.Rows)
                {
                    int folderID = DataRowReader.GetIntegerValue(row, "FolderID");
                    if (folderID > 0 && !uniqueFolders.ContainsKey(folderID))
                        uniqueFolders.Add(folderID, null);


                    int storageID = DataRowReader.GetIntegerValue(row, "StorageID");
                    if (storageID > 0 && !uniqueStorages.ContainsKey(storageID))
                        uniqueStorages.Add(storageID, null);
                }

                //потом получаем объекты по этим ключам и "дозаполняем" словарь
                if (uniqueFolders.Count > 0)
                {
                    string foldersIdentities = string.Join(",", uniqueFolders.Keys.ToArray());
                    string foldersCondition = string.Format("[ID] IN ({0})", foldersIdentities);
                    DBCollection<FolderMetadata> folders = this.MetadataAdapter.FolderAdapter.GetFolders(foldersCondition);

                    foreach (FolderMetadata folder in folders)
                    {
                        if (uniqueFolders.ContainsKey(folder.ID))
                            uniqueFolders[folder.ID] = folder;
                    }
                }

                if (uniqueStorages.Count > 0)
                {
                    string storagesIdentities = string.Join(",", uniqueStorages.Keys.ToArray());
                    string storagesCondition = string.Format("[ID] IN ({0})", storagesIdentities);

                    DBCollection<StorageMetadata> storages = this.MetadataAdapter.StorageAdapter.GetStorages(storagesCondition);
                    foreach (StorageMetadata storage in storages)
                    {
                        if (uniqueStorages.ContainsKey(storage.ID))
                            uniqueStorages[storage.ID] = storage;
                    }
                }

                foreach (DataRow row in resultTable.Rows)
                {
                    FolderMetadata folder = null;
                    StorageMetadata storage = null;

                    int folderID = DataRowReader.GetIntegerValue(row, "FolderID");
                    if (folderID > 0 && uniqueFolders.ContainsKey(folderID))
                        folder = uniqueFolders[folderID];


                    int storageID = DataRowReader.GetIntegerValue(row, "StorageID");
                    if (storageID > 0 && uniqueStorages.ContainsKey(storageID))
                        storage = uniqueStorages[storageID];

                    if (folder == null)
                    {
                        int rowID = DataRowReader.GetIntegerValue(row, "ID");
                        throw new Exception(string.Format("Для строки метаданных папки репликации с идентификатором {0} не найдена папка с идентификатором {1}",
                            rowID,
                            folderID));
                    }

                    if (storage == null)
                    {
                        int rowID = DataRowReader.GetIntegerValue(row, "ID");
                        throw new Exception(string.Format("Для строки метаданных папки репликации с идентификатором {0} не найден узел хранилища с идентификатором {1}",
                            rowID,
                            storageID));
                    }

                    ReplicationFolderMetadata metadata = new ReplicationFolderMetadata(row, folder, storage);
                    replicationFolders.Add(metadata);
                }
            }

            return replicationFolders;
        }
    }
}