using Skychain.Models.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.Identity;

namespace Skychain.Models.Implementation
{
    /// <summary>
    /// Представляет контекст работы системы Skychain в рамках http-запроса.
    /// </summary>
    public class SkyWebContext : ISkyWebContext
    {
        internal SkyWebContext(HttpContext httpContext)
        {
            if (httpContext == null)
                throw new ArgumentNullException("httpContext");

            this.HttpContext = httpContext;
        }

        /// <summary>
        /// Контекст выполнения http-запроса.
        /// </summary>
        public HttpContext HttpContext { get; private set; }

        private bool __init_SkyContext = false;
        private SkyContext _SkyContext;
        /// <summary>
        /// Возвращает текущий контекст системы.
        /// </summary>
        public SkyContext SkyContext
        {
            get
            {
                if (!__init_SkyContext)
                {
                    _SkyContext = new SkyContext();
                    __init_SkyContext = true;
                }
                return _SkyContext;
            }
        }


        private bool __init_CurrentUser = false;
        private SkyUser _CurrentUser;
        /// <summary>
        /// Текущий пользователь, для которого выполняется http-запрос.
        /// Генерирует исключение при отсутствии текущего пользователя.
        /// </summary>
        public SkyUser CurrentUser
        {
            get
            {
                if (!__init_CurrentUser)
                {
                    _CurrentUser = null;

                    if (this.HttpContext.User == null || this.HttpContext.User.Identity == null)
                        throw new Exception("Failed to get current http-context user.");

                    //получаем идентификатор пользователя.
                    string userIDText = this.HttpContext.User.Identity.GetUserId();
                    if(string.IsNullOrEmpty(userIDText))
                        throw new Exception("Failed to get identity of the current http-context user.");

                    Guid userID = new Guid(userIDText);
                    _CurrentUser = this.SkyContext.GetUser(userID);
                    
                    __init_CurrentUser = true;
                }
                return _CurrentUser;
            }
        }


        private bool __init_HasCurrentUser = false;
        private bool _HasCurrentUser;
        /// <summary>
        /// Возвращает true, при наличии текущего пользователя, для которого выполняется http-запрос.
        /// </summary>
        public bool HasCurrentUser
        {
            get
            {
                if (!__init_HasCurrentUser)
                {
                    _HasCurrentUser = false;
                    if (this.HttpContext.User != null && this.HttpContext.User.Identity != null)
                    {
                        //получаем идентификатор пользователя.
                        string userIDText = this.HttpContext.User.Identity.GetUserId();
                        if (!string.IsNullOrEmpty(userIDText))
                        {
                            Guid userID = new Guid(userIDText);

                            //определяем признак наличия пользователя.
                            _HasCurrentUser = userID != Guid.Empty;
                        }
                    }
                    __init_HasCurrentUser = true;
                }
                return _HasCurrentUser;
            }
        }


        /// <summary>
        /// Кодирует текст для представления всех его символов в отображаемом виде в html.
        /// </summary>
        /// <param name="text">Кодируемый текст.</param>
        public string EncodeHtml(string text)
        {
            return Runtime.SkyWebContext.EncodeHtml(text);
        }


        /// <summary>
        /// Кодирует текст формата Xml для представления всех его символов в отображаемом виде в html.
        /// </summary>
        /// <param name="xmlData">Кодируемый текст в формате Xml.</param>
        public string EncodeXml(string xmlData)
        {
            return Runtime.SkyWebContext.EncodeXml(xmlData);
        }


        ISkyContext ISkyWebContext.SkyContext => this.SkyContext;

        ISkyUser ISkyWebContext.CurrentUser => this.CurrentUser;
    }
}
