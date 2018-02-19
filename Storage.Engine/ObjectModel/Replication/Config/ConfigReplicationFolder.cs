using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Storage.Lib;

namespace Storage.Engine
{
    internal class ConfigReplicationFolder
    {
        public ConfigReplicationFolder(XmlNode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            this.Node = node;
        }

        private XmlNode Node { get; set; }

        private bool __init_Url;
        private string _Url;
        public string Url
        {
            get
            {
                if (!__init_Url)
                {
                    _Url = XmlAttributeReader.GetValue(this.Node, "Url");
                    if (string.IsNullOrEmpty(_Url))
                        throw new Exception(string.Format("В настройках папки репликации не задан адрес папки."));

                    __init_Url = true;
                }
                return _Url;
            }
        }

        private bool __init_SourceNode;
        private string _SourceNode;
        public string SourceNode
        {
            get
            {
                if (!__init_SourceNode)
                {
                    _SourceNode = XmlAttributeReader.GetValue(this.Node, "SourceNode");
                    if (string.IsNullOrEmpty(_SourceNode))
                        throw new Exception(string.Format("В настройках папки репликации не задан узел источник."));

                    __init_SourceNode = true;
                }
                return _SourceNode;
            }
        }
    }
}