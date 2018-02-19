using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storage.Lib
{
    /// <summary>
    /// Uri для папки.
    /// </summary>
    public class FolderUri
    {
        /// <summary>
        /// Создает Uri папки по адресу.
        /// </summary>
        /// <param name="url">Адрес папки.</param>
        public FolderUri(string url)
        {
            if (url == null)
                throw new ArgumentNullException("url");


            url = url.Trim('/');
            this.Url = string.Format("/{0}", url);
        }

        /// <summary>
        /// Создает Uri папки.
        /// </summary>
        /// <param name="name">Имя папки.</param>
        /// <param name="parentFolderUrl">Адрес родительской папки.</param>
        public FolderUri(string name, string parentFolderUrl)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            name = name.Trim('/');

            if (!string.IsNullOrEmpty(parentFolderUrl))
            {
                parentFolderUrl = parentFolderUrl.Trim('/');
                this.Url = string.Format("/{0}/{1}", parentFolderUrl, name);
            }
            else
                this.Url = string.Format("/{0}", name);
        }

        /// <summary>
        /// Адрес папки.
        /// </summary>
        public string Url { get; private set; }

        private bool __init_UrlLower;
        private string _UrlLower;
        /// <summary>
        /// Адрес папки в нижнем регистре.
        /// </summary>
        public string UrlLower
        {
            get
            {
                if (!__init_UrlLower)
                {
                    _UrlLower = this.Url.ToLower();
                    __init_UrlLower = true;
                }
                return _UrlLower;
            }
        }

        private bool __init_ParentUri;
        private FolderUri _ParentUri;
        /// <summary>
        /// Builder родительской папки.
        /// </summary>
        public FolderUri ParentUri
        {
            get
            {
                if (!__init_ParentUri)
                {
                    int index = this.Url.LastIndexOf('/');
                    if (index > 0)
                    {
                        string pUrl = this.Url.Substring(0, index);
                        _ParentUri = new FolderUri(pUrl);
                    }

                    __init_ParentUri = true;
                }
                return _ParentUri;
            }
        }

        private bool __init_RootUri;
        private FolderUri _RootUri;
        /// <summary>
        /// Builder корневой папки.
        /// </summary>
        public FolderUri RootUri
        {
            get
            {
                if (!__init_RootUri)
                {
                    FolderUri rootUlr = this;
                    while (!rootUlr.IsRoot && rootUlr != null)
                        rootUlr = rootUlr.ParentUri;

                    _RootUri = rootUlr;

                    __init_RootUri = true;
                }
                return _RootUri;
            }
        }

        private bool __init_IsRoot;
        private bool _IsRoot;
        /// <summary>
        /// Является корневой.
        /// </summary>
        public bool IsRoot
        {
            get
            {
                if (!__init_IsRoot)
                {
                    _IsRoot = this.ParentUri == null;
                    __init_IsRoot = true;
                }
                return _IsRoot;
            }
        }

        private bool __init_FolderName;
        private string _FolderName;
        /// <summary>
        /// Имя папки.
        /// </summary>
        public string FolderName
        {
            get
            {
                if (!__init_FolderName)
                {
                    int index = this.Url.LastIndexOf('/');
                    _FolderName = this.Url.Substring(index + 1);
                    if (string.IsNullOrEmpty(_FolderName))
                        throw new Exception(string.Format("Неверный формат адреса папки: {0}", this.Url));

                    __init_FolderName = true;
                }
                return _FolderName;
            }
        }
    }
}