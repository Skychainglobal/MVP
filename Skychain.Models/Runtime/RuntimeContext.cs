using Skychain.Models.Implementation;
using System;
using System.Web;

namespace Skychain.Models.Runtime
{
    /// <summary>
    /// Представляет контекст времени выполнения кода.
    /// </summary>
    public sealed class RuntimeContext
    {
        /// <summary>
        /// Создает экземпляр RuntimeContext.
        /// </summary>
        private RuntimeContext(HttpContext httpContext)
        {
            this.HttpContext = httpContext;

            //если текущий контекст не является http-контекстом, а является контекстом windows-сервиса, проверяем, 
            //что перед первым обращением к контексту, он был сброшен.
            if (!RuntimeContextScope.IsRuntimeScope && this.BoundPointsUnmanaged)
                this.CheckCurrentThreadContextReseted();
        }

        /// <summary>
        /// Ссылка на контекст времени выполнения в рамках текущего потока.
        /// </summary>
        [ThreadStatic]
        private static RuntimeContext _CurrentThreadContext;

        /// <summary>
        /// Ссылка на контекст времени выполнения в рамках области выполнения кода текущего потока.
        /// </summary>
        [ThreadStatic]
        internal static RuntimeContext _CurrentScopeContext;

        /// <summary>
        /// Устанавливается в true, если контекст выполнения был сброшен хотя бы один раз.
        /// </summary>
        [ThreadStatic]
        private static bool __reseted_CurrentThreadContext = false;

        /// <summary>
        /// Проверяет, что контекст выполнения был сброшен хотя бы один раз.
        /// </summary>
        private void CheckCurrentThreadContextReseted()
        {
            if (!__reseted_CurrentThreadContext)
                throw new Exception("Контекст выполнения кода должен быть сброшен в точке входа выполнения потока.");
        }

        /// <summary>
        /// Ключ по которому хранится контекст выполняния в хэш-таблице HttpContext.Items.
        /// </summary>
        private const string _HttpContextCurrentKey = "CurrentRuntimeContext";

        /// <summary>
        /// Текущий контекст времени выполняния кода. В случае наличия текущего HttpContext, экземпляр RuntimeContext иницилизируется в рамках HttpContext.
        /// В ином случае экземляр RuntimeContext инициализируется и живет в рамках текущего потока.
        /// </summary>
        public static RuntimeContext Current
        {
            get
            {
                //обрабатываем контекст в рамках области кода.
                if (RuntimeContextScope.IsRuntimeScope)
                {
                    //возвращаем существующий контекст в рамках области кода.
                    if (_CurrentScopeContext != null)
                        return _CurrentScopeContext;
                    //иначе, создаём контекст в рамках области кода.
                    else
                    {
                        _CurrentScopeContext = new RuntimeContext(HttpContext.Current);
                        return _CurrentScopeContext;
                    }
                }

                //текущий HttpContext
                HttpContext httpContext = HttpContext.Current;
                if (httpContext != null)
                {
                    //текущий контекст выполнения.
                    RuntimeContext current = null;

                    //если есть текущий контекст запроса к веб-приложению, то инициализируем RuntimeContext в рамках HttpContext.
                    if (httpContext.Items.Contains(_HttpContextCurrentKey))
                    {
                        current = (RuntimeContext)httpContext.Items[_HttpContextCurrentKey];
                        if (current == null)
                            throw new Exception("Отсутствует текущий контекст времени выполнения в рамках HttpContext.");
                    }
                    else
                    {
                        current = new RuntimeContext(httpContext);
                        httpContext.Items.Add(_HttpContextCurrentKey, current);
                    }

                    //возвращаем контекст.
                    return current;
                }
                else
                {
                    //возвращаем существующий контекст из текущего потока.
                    if (_CurrentThreadContext != null)
                        return _CurrentThreadContext;
                    //иначе, создаем контекст в рамках текущего потока.
                    else
                    {
                        _CurrentThreadContext = new RuntimeContext(null);
                        return _CurrentThreadContext;
                    }
                }
            }
        }

        /// <summary>
        /// Сбрасывает контекст времени выполнения для текущего потока.
        /// Операция доступна только вне запроса к веб-приложению.
        /// </summary>
        public static void ResetCurrent()
        {
            //обрабатываем сброс в рамках области кода.
            if (RuntimeContextScope.IsRuntimeScope)
                _CurrentScopeContext = null;

            //обрабатываем сброс в рамках потока.
            else
            {
                if (HttpContext.Current != null)
                    throw new Exception("Операция доступна только вне контекста запроса к веб-приложению.");

                //сбрасываем контекст текущего потока.
                _CurrentThreadContext = null;
                __reseted_CurrentThreadContext = true;
            }
        }

        private HttpContext _HttpContext;
        /// <summary>
        /// Текущий контекст выполнения запроса к веб-приложению.
        /// </summary>
        public HttpContext HttpContext
        {
            get { return _HttpContext; }
            private set { _HttpContext = value; }
        }

        /// <summary>
        /// Проверяет, что операция выполняется в контексте http-запроса к веб-приложению.
        /// В ином случае генерирует исключение.
        /// </summary>
        public void CheckHttpContext()
        {
            if (this.HttpContext == null)
                throw new Exception("The operation is only available in the context of the execution of the HTTP-request.");
        }

        private bool __init_BoundPointsUnmanaged = false;
        private bool _BoundPointsUnmanaged;
        /// <summary>
        /// Возвращает true, если границы выполнения контекста - точка входа и точка выхода из контекста не являются управлемым и четко не определены.
        /// Характеризует состояние контекста выполнения службы Windows, параллельного потока в http-контексте, а также обработчика файлов SharePoint.
        /// </summary>
        public bool BoundPointsUnmanaged
        {
            get
            {
                if (!__init_BoundPointsUnmanaged)
                {
                    _BoundPointsUnmanaged = this.HttpContext == null && !Environment.UserInteractive;
                    __init_BoundPointsUnmanaged = true;
                }
                return _BoundPointsUnmanaged;
            }
        }



        private bool __init_Properties = false;
        private ExtendedPropertyBag<RuntimeContext> _Properties;
        /// <summary>
        /// Свойства, инициализируемые внешним кодом в рамках текущего контекста времени выполнения.
        /// </summary>
        public ExtendedPropertyBag<RuntimeContext> Properties
        {
            get
            {
                if (!__init_Properties)
                {
                    _Properties = new ExtendedPropertyBag<RuntimeContext>(this);
                    __init_Properties = true;
                }
                return _Properties;
            }
        }


        private bool __init_ContextManager = false;
        private OperationContextManager _ContextManager;
        /// <summary>
        /// Менеджер операций контекста выполнения кода.
        /// </summary>
        public OperationContextManager ContextManager
        {
            get
            {
                if (!__init_ContextManager)
                {
                    _ContextManager = new OperationContextManager();
                    __init_ContextManager = true;
                }
                return _ContextManager;
            }
        }

    }

    /// <summary>
    /// Содержит методы для установки нового экземпляра текущего контекста выполнения с момента создания экземпляра и до вызова метода Dispose.
    /// </summary>
    public class RuntimeContextScope : IDisposable
    {
        /// <summary>
        /// Возвращает true, если в качестве текущего контекста используется контекст области кода.
        /// </summary>
        [ThreadStatic]
        private static bool _IsRuntimeScope = false;

        /// <summary>
        /// Возвращает true, если в качестве текущего контекста используется контекст области кода.
        /// </summary>
        internal static bool IsRuntimeScope
        {
            get { return RuntimeContextScope._IsRuntimeScope; }
        }

        /// <summary>
        /// Инициализирует область кода с новым экземпляром текущего контекста выполнения.
        /// </summary>
        public RuntimeContextScope()
        {
            if (RuntimeContextScope.IsRuntimeScope)
                throw new Exception("Текущий контекст области выполнения кода уже был установлен.");

            //включаем контекст области кода.
            RuntimeContextScope._IsRuntimeScope = true;
        }

        /// <summary>
        /// Отключает действие текущего контекста RuntimeContext.Current, установленного при помощи данного экземпляра.
        /// </summary>
        public void Dispose()
        {
            if (!RuntimeContextScope.IsRuntimeScope)
                throw new Exception("Текущий контекст области выполнения кода уже был отключён.");

            //отключаем контекст области кода.
            RuntimeContext._CurrentScopeContext = null;
            RuntimeContextScope._IsRuntimeScope = false;
        }
    }
}
