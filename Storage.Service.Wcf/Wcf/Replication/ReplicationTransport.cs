using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Storage.Engine;
using Storage.Lib;

namespace Storage.Service.Wcf
{
    /// <summary>
    /// Транспорт для транзкций репликации данных файлового хранилища.
    /// </summary>
    internal class ReplicationTransport : IReplicationTransport
    {
        public ReplicationTransport(IReplicationAdapter replicationAdapter)
        {
            if (replicationAdapter == null)
                throw new ArgumentNullException("replicationAdapter");

            this.ReplicationAdapter = replicationAdapter;
        }

        public IReplicationAdapter ReplicationAdapter { get; private set; }

        private bool __init_Binding;
        private BasicHttpBinding _Binding;
        /// <summary>
        /// Биндинг соединения.
        /// </summary>
        private BasicHttpBinding Binding
        {
            get
            {
                if (!__init_Binding)
                {
                    //TODO потом поменять на https
                    _Binding = new BasicHttpBinding();
                    _Binding.Security.Mode = BasicHttpSecurityMode.TransportCredentialOnly;
                    _Binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Ntlm;

                    _Binding.MaxReceivedMessageSize = ServiceConsts.TransportParams.Binding.MaxMessageSize;

                    _Binding.ReaderQuotas.MaxDepth = ServiceConsts.TransportParams.ReaderQuotas.MaxDepth;
                    _Binding.ReaderQuotas.MaxStringContentLength = ServiceConsts.TransportParams.ReaderQuotas.MaxStringContentLength;
                    _Binding.ReaderQuotas.MaxBytesPerRead = ServiceConsts.TransportParams.ReaderQuotas.MaxBytesPerRead;

                    __init_Binding = true;
                }
                return _Binding;
            }
        }

        private bool __init_Logger;
        private ILogProvider _Logger;
        private ILogProvider Logger
        {
            get
            {
                if (!__init_Logger)
                {
                    _Logger = ConfigFactory.Instance.Create<ILogProvider>();
                    __init_Logger = true;
                }
                return _Logger;
            }
        }

        private IStorageNode CurrentNode
        {
            get
            {
                if (this.ReplicationAdapter.CurrentNode == null)
                    throw new Exception("Не удалось получить текущий узел файлового хранилища.");

                return this.ReplicationAdapter.CurrentNode;
            }
        }

        /// <summary>
        /// Создает клиента сервиса репликации.
        /// </summary>
        /// <param name="storageNode">Целевой узел файлового хранилища.</param>
        /// <returns></returns>
        private ChannelFactory<IStorageReplicationService> CreateServiceChannel(IStorageNode storageNode)
        {
            if (storageNode == null)
                throw new ArgumentNullException("storageNode");

            ChannelFactory<IStorageReplicationService> channel = this.CreateServiceChannel(storageNode.Host);
            return channel;
        }

        /// <summary>
        /// Создает клиента сервиса репликации.
        /// </summary>
        /// <param name="host">Целевой адрес узла файлового хранилища.</param>
        /// <returns></returns>
        private ChannelFactory<IStorageReplicationService> CreateServiceChannel(string host)
        {
            if (string.IsNullOrEmpty(host))
                throw new ArgumentNullException("host");

            //TODO DM поменять протокол
            string connectionString = string.Format("http://{0}:{1}",
                host,
                ServiceConsts.Replication.ReplicationPort);
            EndpointAddress endpoint = new EndpointAddress(connectionString);
            ChannelFactory<IStorageReplicationService> channel = new ChannelFactory<IStorageReplicationService>(this.Binding, endpoint);
            return channel;
        }

        #region IDisposable Members

        public void Dispose()
        {

        }

        #endregion

        #region IReplicationTransport Members

        /// <summary>
        /// Обмен схем репликации текущего узла с узлом storageHost.
        /// </summary>
        /// <param name="remoteNode">Удаленный узел хранилища.</param>
        /// <param name="currentReplicationSchema">Схема репликации текущего узла.</param>
        /// <returns></returns>
        public IReplicationSchema ExchangeSchema(IStorageNode remoteNode, IReplicationSchema currentReplicationSchema)
        {
            if (remoteNode == null)
                throw new ArgumentNullException("remoteNode");

            if (currentReplicationSchema == null)
                throw new ArgumentNullException("currentReplicationSchema");

            return this.MakeRequest(x =>
                                    {
                                        //схема репликации сети, известная текущему узлу
                                        WcfReplicationSchema typedCurrentReplicationSchema =
                                            new WcfReplicationSchema(currentReplicationSchema);
                                        WcfReplicationSchemaMessage exchangeMessage =
                                            new WcfReplicationSchemaMessage(typedCurrentReplicationSchema);
                                        //посылаем текущую схему узлу, к которому обращаемся
                                        //и получаем его схему с узла назначения
                                        WcfReplicationSchemaMessage remoteSchemaMessage =
                                            x.ExchangeSchema(exchangeMessage);
                                        return new WcfReplicationSchema(remoteSchemaMessage);
                                    }, remoteNode.Host);
        }

        /// <summary>
        /// Возвращает информацию по узлу файлового хранилища.
        /// </summary>
        /// <param name="host">Хост удаленного узла.</param>
        /// <returns></returns>
        public IStorageNode GetStorageInfo(string host)
        {
            if (string.IsNullOrEmpty(host))
                throw new ArgumentNullException("host");

            return this.MakeRequest(x => new WcfStorageNode(host, x.GetStorageInfo().UniqueID), host);
        }

        /// <summary>
        /// Возвращает набор идентификаторов версий файлов с удаленного узла.
        /// </summary>
        /// <param name="remoteNode">Удаленный узел хранилища.</param>
        /// <param name="requestStorageID">Идентификатор узла, запрашиваюшего информацию (текущий узел)</param>
        /// <param name="folderUrl">Адрес папки.</param>
        /// <param name="from">Дата, с которой необходимо забрать изменения.</param>
        /// <returns></returns>/// <summary>
        public Tuple<Guid, Guid>[] GetReplicationFiles(IStorageNode remoteNode, Guid requestStorageID, string folderUrl, DateTime @from)
        {
            return
                this.MakeRequest(
                    x => x.GetReplicationFiles(this.CurrentNode.UniqueID, folderUrl, @from),
                    remoteNode.Host);
        }

        /// <summary>
        /// Возвращает версию файла для репликации.
        /// </summary>
        /// <param name="remoteNode">Удаленный узел хранилища.</param>
        /// <param name="folderUrl">Папка файла.</param>
        /// <param name="fileUniqueID">Идентификатор файла.</param>
        /// <param name="fileVersionUniqueID">Идентификатор версии.</param>
        /// <returns></returns>
        public IRemoteFile GetReplicationFile(IStorageNode remoteNode, string folderUrl, Guid fileUniqueID, Guid fileVersionUniqueID)
        {
            if (remoteNode == null)
                throw new ArgumentNullException("remoteNode");

            if (string.IsNullOrEmpty(folderUrl))
                throw new ArgumentNullException("folderUrl");

            WcfRemoteFile remoteFile = null;
            try
            {
                ChannelFactory<IStorageReplicationService> channelFactory = this.CreateServiceChannel(remoteNode.Host);
                using (channelFactory)
                {
                    IStorageReplicationService client = channelFactory.CreateChannel();
                    WcfRemoteFileInfo fileInfo = WcfRemoteFileInfo.FromRemoteFileInfo(folderUrl, fileUniqueID, fileVersionUniqueID);
                    remoteFile = client.GetReplicationFile(fileInfo);
                }
            }
            catch (Exception ex)
            {
                this.Logger.WriteMessage(ex.ToString(), LogLevel.Error);
            }

            return remoteFile;
        }

        #endregion

        private T MakeRequest<T>(Func<IStorageReplicationService, T> call, string host)
        {
            if (call == null)
                throw new ArgumentNullException("call");
            if (String.IsNullOrEmpty(host))
                throw new ArgumentNullException("host");

            T result = default(T);
            try
            {
                ChannelFactory<IStorageReplicationService> channelFactory = this.CreateServiceChannel(host);
                using (channelFactory)
                {
                    IStorageReplicationService client = channelFactory.CreateChannel();
                    result = call(client);
                }
            }
            catch (Exception ex)
            {
                this.Logger.WriteMessage(ex.ToString(), LogLevel.Error);
            }

            return result;
        }
    }
}