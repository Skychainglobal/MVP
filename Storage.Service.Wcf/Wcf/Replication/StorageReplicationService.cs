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
    /// Сервис для репликации данных файлового хранилища.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    internal class StorageReplicationService : IStorageReplicationService, IDisposable
    {
        private bool __init_Engine;
        private StorageEngine _Engine;
        /// <summary>
        /// Движок файлового хранилища.
        /// </summary>
        private StorageEngine Engine
        {
            get
            {
                if (!__init_Engine)
                {
                    _Engine = new StorageEngine();
                    __init_Engine = true;
                }
                return _Engine;
            }
        }

        private bool __init_ReplicationAdapter;
        private IReplicationAdapter _ReplicationAdapter;
        /// <summary>
        /// Адаптер репликации.
        /// </summary>
        private IReplicationAdapter ReplicationAdapter
        {
            get
            {
                if (!__init_ReplicationAdapter)
                {
                    _ReplicationAdapter = ConfigFactory.Instance.Create<IReplicationAdapter>(this.Engine);
                    __init_ReplicationAdapter = true;
                }
                return _ReplicationAdapter;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (__init_Engine)
                this.Engine.Dispose();
        }

        #region IStorageReplicationService Members
        public WcfReplicationSchemaMessage ExchangeSchema(WcfReplicationSchemaMessage remoteSchemaMessage)
        {
            if (remoteSchemaMessage == null)
                throw new ArgumentNullException("remoteSchemaMessage");

            WcfReplicationSchema remoteSchema = new WcfReplicationSchema(remoteSchemaMessage);
            IReplicationSchema currentSchema = this.ReplicationAdapter.UpdateReplicationSchema(remoteSchema);
            WcfReplicationSchema typedCurrentSchema = new WcfReplicationSchema(currentSchema);
            WcfReplicationSchemaMessage responseMessage = new WcfReplicationSchemaMessage(typedCurrentSchema);

            return responseMessage;
        }

        /// <summary>
        /// Возвращает идентификаторы версий файлов для репликации.
        /// </summary>
        /// <param name="requestStorageID">Идентификатор узла, запрашивающего файлы для репликации.</param>
        /// <param name="folderUrl">Адрес папки.</param>
        /// <param name="from">Дата, с которой необходимо забрать файлы с узла источника.</param>
        /// <returns></returns>
        public Tuple<Guid, Guid>[] GetReplicationFiles(Guid requestStorageID, string folderUrl, DateTime from)
        {
            if (requestStorageID == Guid.Empty)
                throw new ArgumentNullException("requestStorageID");

            if (string.IsNullOrEmpty(folderUrl))
                throw new ArgumentNullException("folderUrl");

            Tuple<Guid, Guid>[] files = this.ReplicationAdapter.GetReplicationFiles(requestStorageID, folderUrl, from);
            return files;
        }

        public WcfRemoteFile GetReplicationFile(WcfRemoteFileInfo fileInfo)
        {
            var folder = this.Engine.GetFolder(fileInfo.FolderUrl);
            if (folder == null)
                return null;

            var file = folder.GetFile(fileInfo.FileID, new GetFileOptions { LoadContent = false }, false);
            var version = file.GetVersion(fileInfo.VersionID, false);
            if (version == null)
                return null;

            return WcfRemoteFile.FromFileVersion(version);
        }

        public WcfStorageInfo GetStorageInfo()
        {
            //текущий узел
            WcfStorageInfo currentStorage = new WcfStorageInfo(this.Engine.CurrentNode);
            return currentStorage;
        }
        #endregion
    }
}