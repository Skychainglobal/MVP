using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Storage.Engine;
using Storage.Lib;

namespace Storage.Data.Blob
{
    /// <summary>
    /// Конфигурация настроек адаптера данных.
    /// </summary>
    internal class BlobDataAdapterConfiguration
    {
        public BlobDataAdapterConfiguration()
        {

        }

        private bool __init_Storage;
        private StorageEngine _Storage;
        /// <summary>
        /// Движок файлового хранилища.
        /// </summary>
        private StorageEngine Storage
        {
            get
            {
                if (!__init_Storage)
                {
                    _Storage = new StorageEngine();
                    __init_Storage = true;
                }
                return _Storage;
            }
        }

        private bool __init_BlobMetadataAdapter;
        private IBlobMetadataAdapter _BlobMetadataAdapter;
        /// <summary>
        /// Адаптер метаданных блобов.
        /// </summary>
        internal IBlobMetadataAdapter BlobMetadataAdapter
        {
            get
            {
                if (!__init_BlobMetadataAdapter)
                {
                    _BlobMetadataAdapter = ConfigFactory.Instance.Create<IBlobMetadataAdapter>(this.Storage.MetadataAdapter);
                    __init_BlobMetadataAdapter = true;
                }
                return _BlobMetadataAdapter;
            }
        }

        private bool __init_ConfigurationNode;
        private XmlNode _ConfigurationNode;
        private XmlNode ConfigurationNode
        {
            get
            {
                if (!__init_ConfigurationNode)
                {
                    object configNode = ConfigurationManager.GetSection("StorageInitConfiguration");
                    if (configNode == null)
                        throw new Exception(string.Format("Не задана секция конфигурации хранилища"));

                    _ConfigurationNode = (XmlNode)configNode;

                    __init_ConfigurationNode = true;
                }
                return _ConfigurationNode;
            }
        }

        private bool __init_ConfigurationContainersByPath;
        private Dictionary<string, BlobContainerConfiguration> _ConfigurationContainersByPath;
        private Dictionary<string, BlobContainerConfiguration> ConfigurationContainersByPath
        {
            get
            {
                if (!__init_ConfigurationContainersByPath)
                {
                    _ConfigurationContainersByPath = new Dictionary<string, BlobContainerConfiguration>();

                    foreach (BlobContainerConfiguration containerConfig in this.ConfigurationContainersByName.Values)
                    {
                        string key = containerConfig.Path.ToLower();
                        if (_ConfigurationContainersByPath.ContainsKey(key))
                            throw new Exception(string.Format("В конфигурационном файле уже задан контейнер для директории {0}", containerConfig.Name));

                        _ConfigurationContainersByPath.Add(key, containerConfig);
                    }

                    __init_ConfigurationContainersByPath = true;
                }
                return _ConfigurationContainersByPath;
            }
        }

        private bool __init_ConfigurationContainersByName;
        private Dictionary<string, BlobContainerConfiguration> _ConfigurationContainersByName;
        private Dictionary<string, BlobContainerConfiguration> ConfigurationContainersByName
        {
            get
            {
                if (!__init_ConfigurationContainersByName)
                {
                    _ConfigurationContainersByName = new Dictionary<string, BlobContainerConfiguration>();

                    XmlNodeList nodes = this.ConfigurationNode.SelectNodes("BlobContainers/BlobContainer");
                    if (nodes == null || nodes.Count == 0)
                        throw new Exception(string.Format("В секции инициализации конфигурации хранилища не задано ни одного контейнера блобов"));

                    foreach (XmlNode node in nodes)
                    {
                        BlobContainerConfiguration containerConfig = new BlobContainerConfiguration(node);
                        string key = containerConfig.Name.ToLower();
                        if (_ConfigurationContainersByName.ContainsKey(key))
                            throw new Exception(string.Format("В конфигурационном файле уже задан контейнер с именем {0}", containerConfig.Name));

                        _ConfigurationContainersByName.Add(key, containerConfig);
                    }

                    __init_ConfigurationContainersByName = true;
                }
                return _ConfigurationContainersByName;
            }
        }

        private bool __init_Containers;
        private ICollection<IBlobContainerMetadata> _Containers;
        /// <summary>
        /// Существующие контейнеры.
        /// </summary>
        private ICollection<IBlobContainerMetadata> Containers
        {
            get
            {
                if (!__init_Containers)
                {
                    _Containers = this.BlobMetadataAdapter.GetBlobContainers();
                    __init_Containers = true;
                }
                return _Containers;
            }
        }

        private bool __init_ContainersByPath;
        private Dictionary<string, IBlobContainerMetadata> _ContainersByPath;
        private Dictionary<string, IBlobContainerMetadata> ContainersByPath
        {
            get
            {
                if (!__init_ContainersByPath)
                {
                    _ContainersByPath = new Dictionary<string, IBlobContainerMetadata>();

                    foreach (IBlobContainerMetadata containerMetadata in this.Containers)
                    {
                        if (string.IsNullOrEmpty(containerMetadata.Path))
                            throw new Exception(string.Format("В метаданных для контейнера с идентификатором {0} не задан путь до директории",
                                containerMetadata.ID));

                        string key = containerMetadata.Path.ToLower();
                        if (_ContainersByPath.ContainsKey(key))
                            throw new Exception(string.Format("В метаданных уже существует контейнер для директории {0}", containerMetadata.Path));

                        _ContainersByPath.Add(key, containerMetadata);
                    }

                    __init_ContainersByPath = true;
                }
                return _ContainersByPath;
            }
        }

        private bool __init_ContainersByName;
        private Dictionary<string, IBlobContainerMetadata> _ContainersByName;
        private Dictionary<string, IBlobContainerMetadata> ContainersByName
        {
            get
            {
                if (!__init_ContainersByName)
                {
                    _ContainersByName = new Dictionary<string, IBlobContainerMetadata>();

                    foreach (IBlobContainerMetadata containerMetadata in this.Containers)
                    {
                        if (!string.IsNullOrEmpty(containerMetadata.Name))
                        {
                            string key = containerMetadata.Name.ToLower();
                            if (_ContainersByName.ContainsKey(key))
                                throw new Exception(string.Format("В метаданных уже существует контейнер для директории {0}", containerMetadata.Path));

                            _ContainersByName.Add(key, containerMetadata);
                        }
                    }

                    __init_ContainersByName = true;
                }
                return _ContainersByName;
            }
        }

        /// <summary>
        /// Обновляет схему в хранилище метаданных.
        /// </summary>
        internal void UpdateMetadataSchema()
        {
            bool allContainersHasKey = true;
            bool allContainersKeysAreEmpty = true;

            if (this.Containers != null)
            {
                foreach (IBlobContainerMetadata container in this.Containers)
                {
                    if (string.IsNullOrEmpty(container.Name))
                        allContainersHasKey = false;
                    else
                        allContainersKeysAreEmpty = false;
                }
            }

            if (allContainersHasKey)
            {
                //все ключи заполнены - валидация контейнеров по ключам
                this.UpdateMetadataSchemaByKey();
            }
            else if (allContainersKeysAreEmpty)
            {
                //все ключи пустые - первичная установка ключей
                this.UpdateEmptyMetadataSchema();
            }
            else
            {
                //у некоторых контейнеров есть ключи, у некоторых нет.
                if (this.Containers != null && this.Containers.Count > 0)
                    throw new Exception(string.Format("Некоторые контейнеры не синхронизированы с хранилищем метаданных. Необходимо выполнить синхронизацию в ручном режиме."));
            }
        }

        /// <summary>
        /// Обновление схемы метаданных без ключей.
        /// </summary>
        private void UpdateEmptyMetadataSchema()
        {
            //валидация контейнеров на соответствие FolderID
            foreach (string configurationPath in this.ConfigurationContainersByPath.Keys)
            {
                if (this.ContainersByPath.ContainsKey(configurationPath))
                {
                    BlobContainerConfiguration containerConfig = this.ConfigurationContainersByPath[configurationPath];
                    IBlobContainerMetadata containerMetadata = this.ContainersByPath[configurationPath];

                    if (containerMetadata.FolderID != containerConfig.FolderID)
                        throw new Exception(string.Format("В конфигурации файлового хранилища указан контейнер {0} для которого идентификатор папки = {1}, в то время как в хранилище метаданных для него задан идентификатор папки = {2}. Некорректная настройка!",
                            configurationPath,
                            containerConfig.FolderID,
                            containerMetadata.FolderID));
                }
            }

            //создание несуществующих в метаданных контейнеров
            foreach (string configurationPath in this.ConfigurationContainersByPath.Keys)
            {
                BlobContainerConfiguration containerConfig = this.ConfigurationContainersByPath[configurationPath];

                if (!this.ContainersByPath.ContainsKey(configurationPath))
                {
                    //создание контейнера
                    IBlobContainerMetadata container = this.BlobMetadataAdapter.CreateBlobContainer(containerConfig.Name, configurationPath, containerConfig.FolderID);
                    this.BlobMetadataAdapter.SaveBlobContainer(container);
                }
                else
                {
                    //обновление контейнера
                    IBlobContainerMetadata existsContainer = this.ContainersByPath[configurationPath];
                    existsContainer.Name = containerConfig.Name;
                    this.BlobMetadataAdapter.SaveBlobContainer(existsContainer);
                }
            }
        }

        /// <summary>
        /// Обновление заполненной по ключам схемы метаданных.
        /// </summary>
        private void UpdateMetadataSchemaByKey()
        {
            //валидация контейнеров на соответствие FolderID
            foreach (string configurationName in this.ConfigurationContainersByName.Keys)
            {
                if (this.ContainersByName.ContainsKey(configurationName))
                {
                    BlobContainerConfiguration containerConfig = this.ConfigurationContainersByName[configurationName];
                    IBlobContainerMetadata containerMetadata = this.ContainersByName[configurationName];

                    if (!string.IsNullOrEmpty(containerConfig.FolderUrl))
                    {
                        //если задан и адрес папки и идентификатор
                        if (containerConfig.FolderID > 0)
                        {
                            //то получаем папку и сравниваем идентификаторы у объекта папки
                            IFolder folder = this.Storage.EnsureFolder(containerConfig.FolderUrl);
                            IEngineObjectMetadata engineFolderMetadata = (IEngineObjectMetadata)folder;

                            if (engineFolderMetadata.ID != containerMetadata.FolderID)
                                throw new Exception(string.Format("В конфигурации файлового хранилища указан контейнер {0} для которого идентификатор папки = {1}, в то время как для него же задан адрес папки {2}, а идентификатор папки с таким адресом = {3}. Некорректная настройка!",
                                    configurationName,
                                    containerConfig.FolderID,
                                    containerConfig.FolderUrl,
                                    engineFolderMetadata.ID));
                        }
                    }
                    else if (containerMetadata.FolderID != containerConfig.FolderID)
                    {
                        //адрес папки не задан и идентификаторы не совпадают
                        throw new Exception(string.Format("В конфигурации файлового хранилища указан контейнер {0} для которого идентификатор папки = {1}, в то время как в хранилище метаданных для него задан идентификатор папки = {2}. Некорректная настройка!",
                            configurationName,
                            containerConfig.FolderID,
                            containerMetadata.FolderID));
                    }
                }
            }

            //создание несуществующих в метаданных контейнеров
            foreach (string configurationName in this.ConfigurationContainersByName.Keys)
            {
                BlobContainerConfiguration containerConfig = this.ConfigurationContainersByName[configurationName];

                if (!this.ContainersByName.ContainsKey(configurationName))
                {
                    //создание контейнера с новым именем

                    //адрес в настройке первичен, поэтому идентификатор папки получаем
                    //из объекта полученного по адресу
                    int folderID = containerConfig.FolderID;
                    if (!string.IsNullOrEmpty(containerConfig.FolderUrl))
                    {
                        IFolder folder = this.Storage.EnsureFolder(containerConfig.FolderUrl);
                        IEngineObjectMetadata engineFolderMetadata = (IEngineObjectMetadata)folder;
                        folderID = engineFolderMetadata.ID;
                    }

                    IBlobContainerMetadata container = this.BlobMetadataAdapter.CreateBlobContainer(containerConfig.Name, containerConfig.Path, folderID);
                    this.BlobMetadataAdapter.SaveBlobContainer(container);
                }
                else
                {
                    IBlobContainerMetadata existsContainer = this.ContainersByName[configurationName];
                    if (string.IsNullOrEmpty(existsContainer.Path))
                        throw new Exception(string.Format("В метаданных существует контейнер без адреса директории."));

                    if (existsContainer.Path.ToLower() != containerConfig.Path.ToLower())
                    {
                        //контейнер есть, но директория отличается
                        //старый нужно закрыть
                        this.CloseContainer(existsContainer);

                        //адрес в настройке первичен, поэтому идентификатор папки получаем
                        //из объекта полученного по адресу
                        int folderID = containerConfig.FolderID;
                        if (!string.IsNullOrEmpty(containerConfig.FolderUrl))
                        {
                            IFolder folder = this.Storage.EnsureFolder(containerConfig.FolderUrl);
                            IEngineObjectMetadata engineFolderMetadata = (IEngineObjectMetadata)folder;
                            folderID = engineFolderMetadata.ID;
                        }

                        //и так же нужно создать новый с существующим именем и новой директорией
                        IBlobContainerMetadata container = this.BlobMetadataAdapter.CreateBlobContainer(containerConfig.Name, containerConfig.Path, folderID);
                        this.BlobMetadataAdapter.SaveBlobContainer(container);
                    }
                }
            }

            //закрытие в метаданных контейнеров, которых больше нет в конфиге
            foreach (string metadataContainerName in this.ContainersByName.Keys)
            {
                bool closeRequired = false;
                IBlobContainerMetadata existsContainer = this.ContainersByName[metadataContainerName];

                if (!this.ConfigurationContainersByName.ContainsKey(metadataContainerName))
                {
                    //в метаданных есть контейнер, которого нет в конфиге => закрываем его
                    closeRequired = true;
                }
                else
                {
                    //в метаданных есть контейнер, которого нет в конфиге => закрываем его
                    if (string.IsNullOrEmpty(existsContainer.Path))
                        throw new Exception(string.Format("В метаданных существует контейнер без адреса директории."));

                    BlobContainerConfiguration containerConfig = this.ConfigurationContainersByName[metadataContainerName];
                    closeRequired = existsContainer.Path.ToLower() != containerConfig.Path.ToLower();
                }

                if (closeRequired)
                {
                    //закрываем контейнер
                    this.CloseContainer(existsContainer);
                }
            }
        }

        /// <summary>
        /// Закрытие контейнера и его блобов.
        /// </summary>
        /// <param name="container">Контейнер.</param>
        private void CloseContainer(IBlobContainerMetadata container)
        {
            if (container == null)
                throw new ArgumentNullException("container");

            if (container.ID < 1)
                throw new Exception(string.Format("Невозможно закрыть не существующий контейнер"));

            ICollection<IBlobMetadata> containerBlobs = this.BlobMetadataAdapter.GetBlobs(container.ID);
            if (containerBlobs != null)
            {
                foreach (IBlobMetadata blob in containerBlobs)
                {
                    if (!blob.Closed)
                    {
                        blob.Closed = true;
                        this.BlobMetadataAdapter.SaveBlob(blob);
                    }
                }
            }

            if (!container.Closed)
            {
                container.Closed = true;
                this.BlobMetadataAdapter.SaveBlobContainer(container);
            }
        }
    }
}