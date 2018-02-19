
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Storage.Engine;
using Storage.Lib;

namespace Storage.Metadata.MSSQL
{
    /// <summary>
    /// Адаптер работы с метаданными хранилища.
    /// </summary>
    public class MetadataAdapter : IMetadataAdapter
    {
        private static object _locker = new object();
        private static Dictionary<string, bool> _restoredTransactions = new Dictionary<string, bool>();

        public MetadataAdapter()
        { }

        /// <summary>
        /// К-тор.
        /// </summary>
        /// <param name="node">Xml-узел настройки.</param>
        public MetadataAdapter(XmlNode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            this.Node = node;
        }

        private XmlNode _Node;
        /// <summary>
        /// Xml-узел настройки работы с метаданными.
        /// </summary>
        public XmlNode Node
        {
            get { return _Node; }
            set { _Node = value; }
        }

        private bool __init_ConnectionString = false;
        private string _ConnectionString;
        /// <summary>
        /// Строка подключения в БД.
        /// </summary>
        public string ConnectionString
        {
            get
            {
                if (!__init_ConnectionString)
                {
                    if (this.Node == null)
                        throw new Exception("Не задана строка подключеня к базе данных. Адаптер метаданных был создан через пустой конструктор.");

                    XmlNode node = this.Node.SelectSingleNode("ConnectionString");
                    if (node == null)
                        throw new Exception("Не задана строка подключеня к базе данных. В настройке отсутствует узел 'ConnectionString'.");

                    if (String.IsNullOrEmpty(node.InnerText))
                        throw new Exception("Не задана строка подключеня к базе данных. В настройке узел 'ConnectionString' пуст.");

                    _ConnectionString = node.InnerText;
                    __init_ConnectionString = true;
                }
                return _ConnectionString;
            }
        }

        private bool __init_ConnectionContext = false;
        private DBConnectionContext _ConnectionContext;
        /// <summary>
        /// Комтекст подключений к БД.
        /// </summary>
        public DBConnectionContext ConnectionContext
        {
            get
            {
                if (!__init_ConnectionContext)
                {
                    lock (this)
                    {
                        if (!__init_ConnectionContext)
                        {
                            _ConnectionContext = new DBConnectionContext();
                            __init_ConnectionContext = true;
                        }
                    }
                }
                return _ConnectionContext;
            }
        }

        private bool __init_InitialConnection = false;
        private DBConnection _InitialConnection;
        /// <summary>
        /// Подключение к БД.
        /// </summary>
        public DBConnection InitialConnection
        {
            get
            {
                if (!__init_InitialConnection)
                {
                    _InitialConnection = this.ConnectionContext.GetConnection(this.ConnectionString);
                    __init_InitialConnection = true;
                }
                return _InitialConnection;
            }
        }

        private bool __init_FolderAdapter = false;
        private DBFolderAdapter _FolderAdapter;
        /// <summary>
        /// Адаптер работы с папками.
        /// </summary>
        public DBFolderAdapter FolderAdapter
        {
            get
            {
                if (!__init_FolderAdapter)
                {
                    _FolderAdapter = new DBFolderAdapter(this);
                    __init_FolderAdapter = true;
                }
                return _FolderAdapter;
            }
        }

        private bool __init_ReplicationFolderAdapter;
        private ReplicationFolderAdapter _ReplicationFolderAdapter;
        internal ReplicationFolderAdapter ReplicationFolderAdapter
        {
            get
            {
                if (!__init_ReplicationFolderAdapter)
                {
                    _ReplicationFolderAdapter = new ReplicationFolderAdapter(this);
                    __init_ReplicationFolderAdapter = true;
                }
                return _ReplicationFolderAdapter;
            }
        }

        private bool __init_TokenAdapter;
        private TokenAdapter _TokenAdapter;
        /// <summary>
        /// Адаптер работы с токенами.
        /// </summary>
        private TokenAdapter TokenAdapter
        {
            get
            {
                if (!__init_TokenAdapter)
                {
                    _TokenAdapter = new TokenAdapter(this);
                    __init_TokenAdapter = true;
                }
                return _TokenAdapter;
            }
        }

        private bool __init_StorageAdapter = false;
        private StorageAdapter _StorageAdapter;
        /// <summary>
        /// Адаптер работы с хранилищами метаданных.
        /// </summary>
        public StorageAdapter StorageAdapter
        {
            get
            {
                if (!__init_StorageAdapter)
                {
                    _StorageAdapter = new MSSQL.StorageAdapter(this);
                    __init_StorageAdapter = true;
                }
                return _StorageAdapter;
            }
        }

        private bool __init_TableActivator = false;
        private DBObjectTableActivator _TableActivator;
        /// <summary>
        /// Активатор распределенных таблиц.
        /// </summary>
        public DBObjectTableActivator TableActivator
        {
            get
            {
                if (!__init_TableActivator)
                {
                    lock (this)
                    {
                        if (!__init_TableActivator)
                        {
                            _TableActivator = new DBObjectTableActivator(this);
                            __init_TableActivator = true;
                        }
                    }
                }
                return _TableActivator;
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
                    _Logger = ConfigFactory.Instance.Create<ILogProvider>(MetadataConsts.Scopes.MetadataAdapter);
                    __init_Logger = true;
                }
                return _Logger;
            }
        }

        #region TypeDefinitions

        /// <summary>
        /// Получение определения типа метаданных по типу объекта.
        /// </summary>
        /// <param name="type">Тип метаданных.</param>
        /// <returns></returns>
        internal MetadataTypeDefinition GetTypeDefinition(Type type)
        {
            return MetadataTypeProvider.Instance.GetTypeDefinition(type);
        }

        #endregion

        #region IMetadataAdapter Members

        #region Storages

        private IStorageMetadata _CurrentStorage;
        /// <summary>
        /// Текущее хранилище метаданных.
        /// </summary>
        public IStorageMetadata CurrentStorage
        {
            get
            {
                if (_CurrentStorage == null)
                {
                    this.Logger.WriteMessage("Получение CurrentStorage начало.");

                    StorageMetadata storage = this.StorageAdapter.GetCurrent();
                    if (storage != null)
                        _CurrentStorage = (IStorageMetadata)storage;

                    this.Logger.WriteFormatMessage("Получение CurrentStorage Идентификатор: '{0}'", _CurrentStorage != null ? _CurrentStorage.ID.ToString() : "Не удалось получить");
                }
                return _CurrentStorage;
            }
        }

        public IStorageMetadata GetStorage(Guid uniqueID)
        {
            if (uniqueID == Guid.Empty)
                throw new ArgumentNullException("uniqueID");

            this.Logger.WriteFormatMessage("GetStorage начало. uniqueID: '{0}'", uniqueID);
            string condition = string.Format("[UniqueID] = N'{0}'", uniqueID);
            StorageMetadata metadata = this.StorageAdapter.GetStorage(condition);
            this.Logger.WriteFormatMessage("GetStorage начало. uniqueID: '{0}'", uniqueID);

            return metadata;
        }

        public IStorageMetadata GetStorage(string host)
        {
            if (string.IsNullOrEmpty(host))
                throw new ArgumentNullException("host");

            this.Logger.WriteFormatMessage("GetStorage начало. host: '{0}'", host);
            string condition = string.Format("[Host] = N'{0}'", host);
            StorageMetadata metadata = this.StorageAdapter.GetStorage(condition);
            this.Logger.WriteFormatMessage("GetStorage начало. host: '{0}'", host);

            return metadata;
        }

        public ICollection<IStorageMetadata> GetStorages()
        {
            this.Logger.WriteMessage("GetStorages: Получение всех хранилищ");

            IEnumerable<int> ids = null;
            DBCollection<StorageMetadata> storages = this.StorageAdapter.GetStorages(ids);

            this.Logger.WriteMessage("GetStorages: Получение всех хранилищ завершено.");
            return storages.Cast<IStorageMetadata>().ToList();
        }

        public ICollection<IStorageMetadata> GetStorages(IEnumerable<int> storageIDs)
        {
            if (storageIDs == null)
                throw new ArgumentNullException("storageIDs");

            this.Logger.WriteFormatMessage("GetStorages: начало. Идентификаторы: [{0}]", String.Join(",", storageIDs.Select(x => x.ToString()).ToArray<string>()));

            DBCollection<StorageMetadata> storages = this.StorageAdapter.GetStorages(storageIDs);

            this.Logger.WriteMessage("GetStorages: Получение хранилищ завершено.");

            return storages.Cast<IStorageMetadata>().ToList();
        }

        public IStorageMetadata CreateCurrentStorageNode(string host)
        {
            if (string.IsNullOrEmpty(host))
                throw new ArgumentNullException("host");

            StorageMetadata storage = StorageMetadata.Create(host, true);
            return storage;
        }

        public void SaveStorage(IStorageMetadata storage)
        {
            if (storage == null)
                throw new ArgumentNullException("storage");

            this.Logger.WriteFormatMessage("SaveStorage: начало host: '{0}' isCurrent: '{1}'", storage.Host, storage.IsCurrent);

            StorageMetadata typedStorage = (StorageMetadata)storage;
            this.StorageAdapter.Update(typedStorage);

            this.Logger.WriteMessage("SaveStorage: конец.");
        }

        public IStorageMetadata CreateStorageNode(string host, Guid uniqueID)
        {
            if (string.IsNullOrEmpty(host))
                throw new ArgumentNullException("host");

            if (uniqueID == Guid.Empty)
                throw new ArgumentNullException("uniqueID");

            StorageMetadata storage = StorageMetadata.Create(host, false, uniqueID);
            return storage;
        }
        #endregion

        #region Folders

        public IFolderMetadata EnsureFolder(string folderUrl)
        {
            if (String.IsNullOrEmpty(folderUrl))
                throw new ArgumentNullException("folderUrl");

            this.Logger.WriteFormatMessage("EnsureFolder:начало folderUrl={0}", folderUrl);

            FolderMetadata folder = (FolderMetadata)this.GetFolder(folderUrl, false);
            if (folder == null)
            {
                this.Logger.WriteFormatMessage("EnsureFolder: создание папки.folderUrl={0}", folderUrl);

                lock (_locker)
                {
                    folder = (FolderMetadata)this.GetFolder(folderUrl, false);
                    if (folder == null)
                    {
                        folder = new FolderMetadata();
                        folder.Url = folderUrl;
                        this.FolderAdapter.UpdateFolder(folder);
                    }
                }

                this.Logger.WriteMessage("EnsureFolder: конец создания папки.");
            }

            this.Logger.WriteMessage("EnsureFolder: конец операции.");

            return folder;
        }

        public IFolderMetadata GetFolder(string folderUrl, bool throwIfNotExists = true)
        {
            if (String.IsNullOrEmpty(folderUrl))
                throw new ArgumentNullException("folderUrl");

            FolderMetadata folder = null;

            this.Logger.WriteFormatMessage("GetFolder: начало  folderUrl={0}", folderUrl);

            string query = String.Format("[Url] = N'{0}'", folderUrl);
            folder = this.FolderAdapter.GetFolder(query);

            if (folder == null && throwIfNotExists)
                throw new Exception(String.Format("Не найдена папка с Url: '{0}'", folderUrl));

            this.Logger.WriteMessage("GetFolder конец операции.");

            return folder;
        }

        public IFolderMetadata GetFolder(int id, bool throwIfNotExists = true)
        {
            FolderMetadata folder = null;

            this.Logger.WriteFormatMessage("GetFolder: начало  id={0}", id);

            string query = String.Format("[ID] = {0}", id);
            folder = this.FolderAdapter.GetFolder(query);

            if (folder == null && throwIfNotExists)
                throw new Exception(String.Format("Не найдена папка с ID: '{0}'", id));

            this.Logger.WriteMessage("GetFolder: конец операции.");

            return folder;
        }

        public ICollection<IFolderMetadata> GetFolders(ICollection<string> folderUrls)
        {
            if (folderUrls == null)
                throw new ArgumentNullException("folderUrls");

            this.Logger.WriteFormatMessage("GetFolders: начало операции. folderUrls=[{0}]", String.Join(",", folderUrls.ToArray()));

            DBCollection<FolderMetadata> resultFolders = this.FolderAdapter.GetFolders(String.Format("[Url] in ({0})",
                String.Join(",", folderUrls.Select(x => String.Format("N'{0}'", x)).ToArray())));

            this.Logger.WriteMessage("GetFolders по folderUrls конец операции.");

            return resultFolders.Cast<IFolderMetadata>().ToList();
        }

        public ICollection<IFolderMetadata> GetFolders(int parentFolderID = 0)
        {
            this.Logger.WriteFormatMessage("GetFolders: начало операции. parentFolderID={0}", parentFolderID);

            DBCollection<FolderMetadata> resultFolders = this.FolderAdapter.GetFolders(String.Format("[ParentID] = {0}", parentFolderID));

            this.Logger.WriteMessage("GetFolders: конец операции.");

            return resultFolders.Cast<IFolderMetadata>().ToList();
        }

        #endregion

        #region Tokens
        /// <summary>
        /// Возвращает токен из хранилища метаданных.
        /// </summary>
        /// <param name="tokenUniqueID">Уникальный идентификатор токена.</param>
        /// <returns></returns>
        public IToken GetToken(Guid tokenUniqueID)
        {
            if (tokenUniqueID == Guid.Empty)
                throw new ArgumentNullException("tokenUniqueID");

            TokenMetadata token = this.TokenAdapter.GetToken(tokenUniqueID);
            return token;
        }

        /// <summary>
        /// Удаляет токен из хранилища метаданных.
        /// </summary>
        /// <param name="tokenUniqueID"></param>
        public void RemoveToken(Guid tokenUniqueID)
        {
            if (tokenUniqueID == Guid.Empty)
                throw new ArgumentNullException("tokenUniqueID");

            this.TokenAdapter.RemoveToken(tokenUniqueID);
        }

        /// <summary>
        /// Генерирует новый токен в хранилище метаданных.
        /// </summary>
        /// <returns></returns>
        public IToken GenerateToken()
        {
            TokenMetadata token = this.TokenAdapter.GenerateToken();
            return token;
        }
        #endregion

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            //TODO [AO] ???
        }

        #endregion

        private bool __init_DataAdapter = false;
        private DBAdapter _DataAdapter;
        /// <summary>
        /// Адаптер запросов к БД.
        /// </summary>
        private DBAdapter DataAdapter
        {
            get
            {
                if (!__init_DataAdapter)
                {
                    _DataAdapter = new DBAdapter(this.InitialConnection);
                    __init_DataAdapter = true;
                }
                return _DataAdapter;
            }
        }

        #region Replication

        public IReplicationFolderMetadata CreateReplicationFolder(IFolderMetadata folder, IStorageMetadata targetStorage)
        {
            if (folder == null)
                throw new ArgumentNullException("folder");

            if (targetStorage == null)
                throw new ArgumentNullException("targetStorage");

            ReplicationFolderMetadata metadata = new ReplicationFolderMetadata(folder, targetStorage);
            return metadata;
        }

        public IReplicationFolderMetadata GetReplicationFolder(IFolderMetadata folder, IStorageMetadata storage)
        {
            if (folder == null)
                throw new ArgumentNullException("folder");

            if (storage == null)
                throw new ArgumentNullException("storage");

            string condition = string.Format("[StorageID] = {0} AND [FolderID] = {1}",
                storage.ID,
                folder.ID);

            IReplicationFolderMetadata result = null;
            DBCollection<ReplicationFolderMetadata> replicationFolders = this.ReplicationFolderAdapter.GetReplicationFolders(condition);
            if (replicationFolders != null && replicationFolders.Count > 0)
                result = replicationFolders[0];

            return result;
        }

        public ICollection<IReplicationFolderMetadata> GetReplicationFolders()
        {
            ICollection<IReplicationFolderMetadata> result;
            lock (_locker)
            {
                ICollection<ReplicationFolderMetadata> replicationFolders = this.ReplicationFolderAdapter.GetReplicationFolders();
                result = replicationFolders.Cast<IReplicationFolderMetadata>().ToList();
            }

            return result;
        }

        public void SaveReplicationFolder(IReplicationFolderMetadata replicationFolder)
        {
            if (replicationFolder == null)
                throw new ArgumentNullException("replicationFolder");

            ReplicationFolderMetadata typedReplicationFolder = (ReplicationFolderMetadata)replicationFolder;
            this.ReplicationFolderAdapter.UpdateReplicationFolder(typedReplicationFolder);
        }

        internal Dictionary<string, bool> GetExistsTables(string serchPattern)
        {
            if (string.IsNullOrEmpty(serchPattern))
                throw new ArgumentNullException("serchPattern");

            string tablesQuery = string.Format(@"SELECT *
                FROM INFORMATION_SCHEMA.TABLES
                WHERE TABLE_TYPE = 'BASE TABLE' AND TABLE_NAME LIKE '{0}'", serchPattern);

            Dictionary<string, bool> uniqueTables = new Dictionary<string, bool>();
            DataTable tablesInfo = this.DataAdapter.GetDataTable(tablesQuery);
            foreach (DataRow tableInfoRow in tablesInfo.Rows)
            {
                string tableName = DataRowReader.GetStringValue(tableInfoRow, "TABLE_NAME");
                if (string.IsNullOrEmpty(tableName))
                    continue;

                if (!uniqueTables.ContainsKey(tableName))
                    uniqueTables.Add(tableName, true);
            }

            return uniqueTables;
        }
        #endregion

        #region IMetadataAdapter Members
        public Tuple<Guid, Guid>[] GetReplicationFiles(IStorageMetadata requestNode, IFolderMetadata folder, DateTime from)
        {
            if (requestNode == null)
                throw new ArgumentNullException("requestNode");

            if (folder == null)
                throw new ArgumentNullException("folder");

            List<Tuple<Guid, Guid>> files = new List<Tuple<Guid, Guid>>();

            string folderDBName = folder.Url.Trim('/').Replace('/', '_');
            string versionsPostfix = "Versions";
            string searchPattern = string.Format("Files_{0}_%_{1}", folderDBName, versionsPostfix);
            Dictionary<string, bool> partitions = this.GetExistsTables(searchPattern);

            List<FileVersionData> allVersions = new List<FileVersionData>();

            //идентификатор узла хранилища, который запрашивает файлы
            //т.е. для текущего узла, на который пришел запрос он будет являтся
            //узлом назначения для отправки файлов.
            int targetStorageID = requestNode.ID;

            if (partitions != null && partitions.Count > 0)
            {
                int n = MetadataConsts.Replication.BatchSize / partitions.Count;
                //1 - получить TOP(n) файлов из каждого partition
                string partitionsQuery = string.Empty;
                foreach (string partition in partitions.Keys)
                {
                    if (!string.IsNullOrEmpty(partitionsQuery))
                        partitionsQuery += string.Format(" {0}UNION ALL{0} ", Environment.NewLine);

                    string filesTableName = partition.TrimEnd(string.Format("_{0}", versionsPostfix).ToCharArray());

                    partitionsQuery += string.Format(@"SELECT TOP {0} f.UniqueID as FID, v.UniqueID as VID, v.TimeCreated
                        FROM [{1}] v
                        WITH(NOLOCK)
                        INNER JOIN [{2}] f
                        ON f.ID = v.FileID
                        AND v.CreatedStorageID <> {3}
                        AND v.TimeCreated > @startDate",
                                      n,
                                      partition,
                                      filesTableName,
                                      targetStorageID);
                }

                if (!string.IsNullOrEmpty(partitionsQuery))
                {
                    string resultQuery = string.Format("WITH AllPartitions AS ({0}) SELECT FID, VID, TimeCreated FROM AllPartitions ORDER BY TimeCreated ASC",
                        partitionsQuery);

                    DateTime startDate;
                    if (from == DateTime.MinValue)
                    {
                        //пришла пустая дата
                        //выдаем минимальную дату ms sql
                        startDate = new DateTime(1753, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    }
                    else
                    {
                        //пришла дата
                        startDate = new DateTime(from.Year, from.Month, from.Day, from.Hour, from.Minute, from.Second, 0, DateTimeKind.Utc);
                    }
                    SqlParameter startDateParam = new SqlParameter("startDate", startDate);
                    DataTable resultTable = this.DataAdapter.GetDataTable(resultQuery, startDateParam);
                    if (resultTable != null && resultTable.Rows != null)
                    {
                        using (resultTable)
                        {
                            foreach (DataRow row in resultTable.Rows)
                            {
                                //FID, VID, TimeCreated
                                Guid fileID = DataRowReader.GetGuidValue(row, "FID");
                                if (fileID == Guid.Empty)
                                    throw new Exception(string.Format("Не удалось получить идентификатор файла"));

                                Guid versionID = DataRowReader.GetGuidValue(row, "VID");
                                if (versionID == Guid.Empty)
                                    throw new Exception(string.Format("Не удалось получить идентификатор версии файла"));

                                DateTime versionTime = DataRowReader.GetDateTimeValue(row, "TimeCreated");
                                if (versionTime == DateTime.MinValue)
                                    throw new Exception(string.Format("Не удалось получить дату создания версии файла"));

                                FileVersionData fileVersionData = new FileVersionData(fileID, versionID, versionTime);
                                allVersions.Add(fileVersionData);
                            }
                        }
                    }

                    //сортируем и формируем кортеж идентификаторов
                    if (allVersions.Count > 1)
                    {
                        var sortedVersions = allVersions.OrderBy(v => v.VersionTime);
                        foreach (FileVersionData version in allVersions)
                        {
                            files.Add(Tuple.Create<Guid, Guid>(version.FileID, version.VersionID));
                        }
                    }
                }
            }

            Tuple<Guid, Guid>[] filesToReplication = files.ToArray();

            return filesToReplication;
        }

        /// <summary>
        /// Возвращает метаданные папки репликации. Если ее не существует, то создает ее.
        /// </summary>
        /// <param name="folder">Папка.</param>
        /// <param name="sourceStorage">Узел источник для репликации.</param>
        /// <returns></returns>
        public IReplicationFolderMetadata EnsureReplicationFolder(IFolderMetadata folder, IStorageMetadata sourceStorage)
        {
            if (folder == null)
                throw new ArgumentNullException("folder");

            if (sourceStorage == null)
                throw new ArgumentNullException("sourceStorage");

            IReplicationFolderMetadata replicationFolder = this.GetReplicationFolder(folder, sourceStorage);
            if (replicationFolder == null)
            {
                replicationFolder = new ReplicationFolderMetadata(folder, sourceStorage);
                this.SaveReplicationFolder(replicationFolder);
            }

            return replicationFolder;
        }
        #endregion
    }
}