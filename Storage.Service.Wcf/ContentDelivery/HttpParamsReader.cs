using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storage.Service.Wcf
{
    internal class HttpParamsReader
    {
        public HttpParamsReader(string url)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException("url");

            this.Url = url;
        }

        public string Url { get; private set; }

        private bool __init_Uri;
        private Uri _Uri;
        private Uri Uri
        {
            get
            {
                if (!__init_Uri)
                {
                    _Uri = new System.Uri(this.Url);
                    __init_Uri = true;
                }
                return _Uri;
            }
        }

        private bool __init_NameValues;
        private NameValueCollection _NameValues;
        private NameValueCollection NameValues
        {
            get
            {
                if (!__init_NameValues)
                {
                    _NameValues = System.Web.HttpUtility.ParseQueryString(this.Uri.Query);
                    __init_NameValues = true;
                }
                return _NameValues;
            }
        }

        public string GetParameterValue(string parameterName)
        {
            if (string.IsNullOrEmpty(parameterName))
                throw new ArgumentNullException("parameterName");

            string value = this.NameValues.Get(parameterName);
            return value;
        }

        public bool GetBooleanParameterValue(string parameterName)
        {
            if (string.IsNullOrEmpty(parameterName))
                throw new ArgumentNullException("parameterName");

            string stringValue = this.GetParameterValue(parameterName);
            bool result = false;
            if (!string.IsNullOrEmpty(stringValue))
                bool.TryParse(stringValue, out result);

            return result;
        }
    }
}