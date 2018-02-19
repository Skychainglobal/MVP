using Skychain.Models.Entity;
using Storage.Client.Wcf;
using Storage.Lib;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;

namespace Skychain.Models.Implementation
{
    /// <summary>
    /// Представляет контекст работы с системой.
    /// Экземпляр класса не является потокобезопасным, как и все экземпляры, которые могут быть получены через данный экземпляр.
    /// </summary>
    public class SkyContext : ISkyContext
    {
        /// <summary>
        /// Создаёт новый экземпляр SkyContext.
        /// </summary>
        internal SkyContext()
        {
        }


        private bool __init_ObjectAdapters = false;
        private SkyObjectAdapterRepository _ObjectAdapters;
        /// <summary>
        /// Содержит адаптеры всех типов объектов системы.
        /// </summary>
        public SkyObjectAdapterRepository ObjectAdapters
        {
            get
            {
                if (!__init_ObjectAdapters)
                {
                    _ObjectAdapters = new SkyObjectAdapterRepository(this);
                    __init_ObjectAdapters = true;
                }
                return _ObjectAdapters;
            }
        }


        private bool __init_UsersByID = false;
        private Dictionary<Guid, SkyUser> _UsersByID;
        private Dictionary<Guid, SkyUser> UsersByID
        {
            get
            {
                if (!__init_UsersByID)
                {
                    _UsersByID = new Dictionary<Guid, SkyUser>();
                    __init_UsersByID = true;
                }
                return _UsersByID;
            }
        }


        /// <summary>
        /// Возвращает новый или существующий экземпляр пользователя.
        /// Метод выполняет сопоставление между идентификатором и экземпляром, соответствующим данному идентификатору,
        /// какого-либо поиска пользователя по идентификатору не производится.
        /// </summary>
        /// <param name="userID">Идентификатор пользователя, для которого производится сопоставление.</param>
        public SkyUser GetUser(Guid userID)
        {
            if (userID == Guid.Empty)
                throw new ArgumentNullException("userID");

            SkyUser user = null;
            if (!this.UsersByID.ContainsKey(userID))
            {
                user = new SkyUser(userID, this);
                this.UsersByID.Add(userID, user);
            }
            else
                user = this.UsersByID[userID];

            if (user == null)
                throw new Exception(string.Format("Failed to get user by ID={0}.", userID));
            return user;
        }

        private bool __init_StorageServiceUrl;
        private string _StorageServiceUrl;
        private string StorageServiceUrl
        {
            get
            {
                if (!__init_StorageServiceUrl)
                {
                    _StorageServiceUrl = ConfigurationManager.AppSettings["StorageServiceUrl"];
                    if (string.IsNullOrEmpty(_StorageServiceUrl))
                        throw new Exception("Unknown storage service url. Parameter StorageServiceUrl does not exists.");

                    __init_StorageServiceUrl = true;
                }
                return _StorageServiceUrl;
            }
        }


        private bool __init_Storage = false;
        private IStorage _Storage;
        /// <summary>
        /// Хранилище файлов системы.
        /// </summary>
        internal IStorage Storage
        {
            get
            {
                if (!__init_Storage)
                {
                    string connectionString = this.StorageServiceUrl;

                    /*tmp*/
                    string storageAccount = ConfigurationManager.AppSettings["StorageAccount"];
                    if (string.IsNullOrEmpty(storageAccount))
                        throw new ArgumentNullException("storageAccount");

                    string storageAccountPassword = ConfigurationManager.AppSettings["StorageAccountPassword"];
                    if (string.IsNullOrEmpty(storageAccountPassword))
                        throw new ArgumentNullException("storageAccountPassword");


                    ClientCredentials credentials = new ClientCredentials();
                    credentials.Windows.ClientCredential.UserName = storageAccount;
                    credentials.Windows.ClientCredential.Password = storageAccountPassword;

                    _Storage = WcfStorage.Open(connectionString, credentials);
                    if (_Storage == null)
                        throw new Exception(string.Format("Failed to connect to storage by connection string: {0}.", connectionString));
                    __init_Storage = true;
                }
                return _Storage;
            }
        }


        private bool __init_ContextManager = false;
        private OperationContextManager _ContextManager;
        /// <summary>
        /// Управляет контекстами операций данного экземпляра.
        /// </summary>
        private OperationContextManager ContextManager
        {
            get
            {
                if (!__init_ContextManager)
                {
                    _ContextManager = new OperationContextManager();
                    __init_ContextManager = true;
                }
                return _ContextManager;
            }
        }

        /// <summary>
        /// Запускает контекст удаления объектов.
        /// </summary>
        internal OperationContext RunDeletingContext()
        {
            return this.ContextManager.BeginContext("DeletingContext");
        }

        /// <summary>
        /// Возвращает true, если в данный момент выполняется контекст удаления объектов.
        /// </summary>
        internal bool IsDeletingContext()
        {
            return this.ContextManager.IsContext("DeletingContext");
        }

        ISkyUser ISkyContext.GetUser(Guid userID)
        {
            return this.GetUser(userID);
        }

        ISkyObjectAdapterRepository ISkyContext.ObjectAdapters => this.ObjectAdapters;
    }
}
