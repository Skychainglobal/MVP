using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Storage.Data.Blob
{
    /// <summary>
    /// Параметры конфигурации контейнера блобов.
    /// </summary>
    internal class BlobContainerConfiguration
    {
        public BlobContainerConfiguration(XmlNode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            this.Node = node;
        }

        private XmlNode Node { get; set; }

        private bool __init_Name;
        private string _Name;
        /// <summary>
        /// Имя контейнера.
        /// </summary>
        public string Name
        {
            get
            {
                if (!__init_Name)
                {
                    _Name = this.GetAttributeValue("Name");
                    __init_Name = true;
                }
                return _Name;
            }
        }

        private bool __init_Path;
        private string _Path;
        /// <summary>
        /// Путь до физической папки контейнера.
        /// </summary>
        public string Path
        {
            get
            {
                if (!__init_Path)
                {
                    _Path = this.GetAttributeValue("Path");
                    __init_Path = true;
                }
                return _Path;
            }
        }

        private bool __init_FolderID;
        private int _FolderID;
        /// <summary>
        /// Идентификатор логической папки контейнера.
        /// </summary>
        public int FolderID
        {
            get
            {
                if (!__init_FolderID)
                {
                    string folderIDString = this.GetAttributeValue("FolderID", false);
                    if (!string.IsNullOrEmpty(folderIDString))
                    {
                        int tmpFolderID = Convert.ToInt32(folderIDString);
                        if (tmpFolderID < 0)
                            throw new Exception(string.Format("Параметр FolderID для контейнера должен быть больше или равен 0"));

                        _FolderID = tmpFolderID;
                    }

                    __init_FolderID = true;
                }
                return _FolderID;
            }
        }

        private bool __init_FolderUrl;
        private string _FolderUrl;
        /// <summary>
        /// Адрес логической папки контейнера.
        /// </summary>
        public string FolderUrl
        {
            get
            {
                if (!__init_FolderUrl)
                {
                    _FolderUrl = this.GetAttributeValue("FolderUrl", false);
                    __init_FolderUrl = true;
                }
                return _FolderUrl;
            }
        }

        private string GetAttributeValue(string attrName, bool throwIfEmpty = true)
        {
            if (string.IsNullOrEmpty(attrName))
                throw new ArgumentNullException("attrName");

            string value = null;
            XmlAttribute attr = this.Node.Attributes[attrName];
            if (attr != null)
                value = attr.Value;

            if (throwIfEmpty && string.IsNullOrEmpty(value))
                throw new Exception(string.Format("Не задан параметр {0}", attrName));

            return value;
        }
    }
}