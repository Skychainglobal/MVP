using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Storage.Lib;

namespace Storage.Engine
{
    internal class ReplicationConfiguration
    {
        private bool __init_Node;
        private XmlNode _Node;
        private XmlNode Node
        {
            get
            {
                if (!__init_Node)
                {
                    object initConfig = ConfigurationManager.GetSection("StorageInitConfiguration");
                    if (initConfig == null)
                        throw new Exception(string.Format("Не задана секция инициализации конфигурации хранилища"));

                    _Node = (XmlNode)initConfig;
                    __init_Node = true;
                }
                return _Node;
            }
        }

        private bool __init_UniqueFolders;
        private Dictionary<string, ConfigReplicationFolder> _UniqueFolders;
        private Dictionary<string, ConfigReplicationFolder> UniqueFolders
        {
            get
            {
                if (!__init_UniqueFolders)
                {
                    _UniqueFolders = new Dictionary<string, ConfigReplicationFolder>();

                    XmlNodeList nodes = this.Node.SelectNodes("Replication/Folder");
                    foreach (XmlNode node in nodes)
                    {
                        ConfigReplicationFolder configFolder = new ConfigReplicationFolder(node);
                        FolderUri uri = new FolderUri(configFolder.Url);
                        if (_UniqueFolders.ContainsKey(uri.UrlLower))
                            throw new Exception(string.Format("В настройках репликации уже задана папка с адресом {0}",
                                uri.Url));

                        _UniqueFolders.Add(uri.UrlLower, configFolder);
                    }

                    __init_UniqueFolders = true;
                }
                return _UniqueFolders;
            }
        }

        private bool __init_ConfigFolders;
        private List<ConfigReplicationFolder> _ConfigFolders;
        public List<ConfigReplicationFolder> ConfigFolders
        {
            get
            {
                if (!__init_ConfigFolders)
                {
                    _ConfigFolders = this.UniqueFolders.Values.ToList();
                    __init_ConfigFolders = true;
                }
                return _ConfigFolders;
            }
        }

        public bool ContainsFolder(string url)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException("url");

            FolderUri uri = new FolderUri(url);
            bool contains = this.UniqueFolders.ContainsKey(uri.UrlLower);

            return contains;
        }
    }
}