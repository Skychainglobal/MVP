using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.DirectoryServices.AccountManagement;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Permissions;
using System.ServiceModel.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Storage.Engine;
using Storage.Lib;

namespace Storage.Service.Wcf
{
    internal class ContentDeliveryManager
    {
        private bool __init_Engine;
        private StorageEngine _Engine;
        /// <summary>
        /// Файловое хранилище.
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

        private bool __init_Logger;
        private ILogProvider _Logger;
        private ILogProvider Logger
        {
            get
            {
                if (!__init_Logger)
                {
                    _Logger = ConfigFactory.Instance.Create<ILogProvider>(ServiceConsts.Logs.Scopes.WcfContentDeliveryManager);
                    __init_Logger = true;
                }
                return _Logger;
            }
        }

        private bool __init_PermissionAdapter;
        private PermissionAdapter _PermissionAdapter;
        private PermissionAdapter PermissionAdapter
        {
            get
            {
                if (!__init_PermissionAdapter)
                {
                    _PermissionAdapter = new PermissionAdapter();
                    __init_PermissionAdapter = true;
                }
                return _PermissionAdapter;
            }
        }

        private bool __init_Configuration;
        private ServiceConfiguration _Configuration;
        private ServiceConfiguration Configuration
        {
            get
            {
                if (!__init_Configuration)
                {
                    _Configuration = new ServiceConfiguration();
                    __init_Configuration = true;
                }
                return _Configuration;
            }
        }

        public async Task Process(HttpListenerContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            this.Logger.WriteMessage("Process:Начало отдачи содержимого по сессионной ссылке");

            Stream sourceStream = null;
            string fileName = null;
            bool isInline = false;
            long contentLength = 0;

            string url = context.Request.Url.AbsoluteUri;

            this.Logger.WriteMessage(String.Format("Process:url: {0}", url));

            try
            {
                this.Authorize(context);

                HttpParamsReader parametersReader = new HttpParamsReader(url);
                isInline = parametersReader.GetBooleanParameterValue("inline");

                using (IFileVersion version = this.Engine.ResolveSessionLink(url, context.User.Identity))
                {
                    this.Logger.WriteMessage(String.Format("Process:url: {0}, version.Name: {1}, version.UniqueID: {2}", url, version.Name, version.UniqueID));

                    contentLength = version.Size;
                    fileName = version.Name;
                    sourceStream = version.Open();
                }
            }
            catch (Exception ex)
            {
                string responseString = string.Format("Ошибка при обработке ссылки {0}. Текст ошибки: {1}",
                    url,
                    ex);
                fileName = "error.txt";

                byte[] content = Encoding.UTF8.GetBytes(responseString);
                contentLength = content.Length;
                sourceStream = new MemoryStream(content);
                context.Response.ContentEncoding = Encoding.UTF8;

                this.Logger.WriteMessage(String.Format("Process:{0}", responseString), LogLevel.Error);
            }
            finally
            {
                try
                {
                    string title = string.Format(string.Format("filename*=UTF-8''{0}", Uri.EscapeDataString(fileName)));

                    context.Response.ContentLength64 = contentLength;
                    context.Response.ContentType = "application/octet-stream";
                    string fileDisposition = isInline ? "inline" : "attachment";
                    context.Response.Headers.Add("Content-Disposition", string.Format("{0}; {1}", fileDisposition, title));
                    context.Response.StatusCode = (int)HttpStatusCode.OK;

                    using (sourceStream)
                    {
                        byte[] buffer = new byte[1024 * 1024];
                        int read = 0;
                        while ((read = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            context.Response.OutputStream.Write(buffer, 0, read);
                        }
                    }
                    this.Logger.WriteMessage(
                        String.Format("Process:Окончание отдачи содержимого по сессионной ссылке, url: {0}", url));
                }
                catch (Exception innerEx)
                {
                    this.Logger.WriteMessage(
                        String.Format("Process:Ошибка отдачи содержимого клиенту, текст ошибки: {0}", innerEx),
                        LogLevel.Error);
                }
            }
        }

        /// <summary>
        /// Авторизует пользователя (проверка прав доступа).
        /// </summary>
        private void Authorize(HttpListenerContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            this.Logger.WriteMessage("Authorize:Начало авторизации пользователя");

            bool inGroup = false;
            if (context.User != null)
            {
                foreach (string groupName in this.Configuration.ContentDeliveryPermissionGroups)
                {
                    inGroup = this.PermissionAdapter.IsUserMemberOf(groupName, context.User.Identity.Name);
                    if (inGroup)
                        break;
                }
            }

            if (!inGroup)
                throw new SecurityAccessDeniedException(string.Format("Access is denied."));

            this.Logger.WriteMessage("Authorize:Пользователь успешно авторизован");
        }
    }
}