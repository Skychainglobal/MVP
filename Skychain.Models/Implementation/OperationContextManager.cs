using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Skychain.Models.Implementation
{

    /// <summary>
    /// Предоставляет методы для работа с контекстами операции.
    /// </summary>
    public class OperationContextManager
    {
        /// <summary>
        /// Создает экземпляр OperationContextManager.
        /// </summary>
        internal OperationContextManager()
        {
        }

        private bool __init_Contexts = false;
        private Dictionary<string, OperationContext> _Contexts;
        /// <summary>
        /// Коллекция конекстов операций.
        /// </summary>
        private Dictionary<string, OperationContext> Contexts
        {
            get
            {
                if (!__init_Contexts)
                {
                    _Contexts = new Dictionary<string, OperationContext>();
                    __init_Contexts = true;
                }
                return _Contexts;
            }
        }


        /// <summary>
        /// Стартует контекст операции с названием operationName.
        /// </summary>
        /// <param name="contextName">Название контекста операции.</param>
        public OperationContext BeginContext(string contextName)
        {
            if (string.IsNullOrEmpty(contextName))
                throw new ArgumentNullException("operationName");
            string operationNameLow = contextName.ToLower();
            OperationContext context = null;
            if (!this.Contexts.ContainsKey(operationNameLow))
            {
                context = new OperationContext(contextName, false, this);
                this.Contexts.Add(operationNameLow, context);
            }
            else
            {
                context = this.Contexts[operationNameLow];
            }
            if (context == null)
                throw new Exception(string.Format("Не удалось получить контекст операции с названием {0}.", contextName));

            //увеличиваем счетчик использования.
            context.UsageCounter++;

            return context;
        }

        /// <summary>
        /// Завершает контекст операции с названием operationName.
        /// </summary>
        /// <param name="contextName">Название контекста операции.</param>
        internal void EndContext(string contextName)
        {
            if (string.IsNullOrEmpty(contextName))
                throw new ArgumentNullException("contextName");
            string operationNameLow = contextName.ToLower();
            if (this.Contexts.ContainsKey(operationNameLow))
                this.Contexts.Remove(operationNameLow);
        }

        /// <summary>
        /// Проверяем наличие текущего контекста выполнения операции с названием contextName.
        /// </summary>
        /// <param name="contextName">Название контекста операции.</param>
        public void CheckContext(string contextName)
        {
            if (string.IsNullOrEmpty(contextName))
                throw new ArgumentNullException("contextName");
            if (!this.Contexts.ContainsKey(contextName.ToLower()))
                throw new Exception(string.Format("The operation can only be performed in the context of the [{0}].", contextName));
        }

        /// <summary>
        /// Возвращает true, если в данный момент присутствует контекст выполнения операции с названием contextName.
        /// </summary>
        /// <param name="contextName">Название контекста операции.</param>
        public bool IsContext(string contextName)
        {
            if (string.IsNullOrEmpty(contextName))
                throw new ArgumentNullException("contextName");
            bool result = this.Contexts.ContainsKey(contextName.ToLower());
            return result;
        }


        #region Static

        private static Dictionary<string, OperationContext> __static_StaticContexts = new Dictionary<string, OperationContext>();
        private Dictionary<string, OperationContext> StaticContexts
        {
            get
            {
                if (__static_StaticContexts == null)
                    __static_StaticContexts = new Dictionary<string, OperationContext>();
                return __static_StaticContexts;
            }
        }

        private OperationContext AddRemoveContext(bool isAdd, string contextName)
        {
            if (string.IsNullOrEmpty(contextName))
                throw new ArgumentNullException("operationName");

            string operationNameLow = contextName.ToLower();
            OperationContext context = null;
            lock (this.StaticContexts)
            {
                //все потоки ждут, пока первый поток добавит/удалит объект...
                if (isAdd)
                {
                    if (!this.StaticContexts.ContainsKey(operationNameLow))
                    {
                        context = new OperationContext(contextName, true, this);

                        //увеличиваем счетчик использования.
                        context.UsageCounter++;

                        this.StaticContexts.Add(operationNameLow, context);
                    }
                    else
                    {
                        context = this.StaticContexts[operationNameLow];
                        if (context != null)
                            context.IncrementUsage();
                    }
                    if (context == null)
                        throw new Exception(string.Format("Не удалось получить статический контекст по ключу {0}.", contextName));
                }
                else
                {
                    if (this.StaticContexts.ContainsKey(operationNameLow))
                        this.StaticContexts.Remove(operationNameLow);
                }
            }
            return context;
        }

        /// <summary>
        /// Стартует многопоточный контекст операции с названием operationName.
        /// </summary>
        /// <param name="contextName">Название контекста операции.</param>
        public OperationContext BeginMultiThreadContext(string contextName)
        {
            if (string.IsNullOrEmpty(contextName))
                throw new ArgumentNullException("operationName");
            string operationNameLow = contextName.ToLower();
            OperationContext context = null;
            if (!this.StaticContexts.ContainsKey(operationNameLow))
            {
                context = this.AddRemoveContext(true, contextName);
            }
            else
            {
                context = this.StaticContexts[operationNameLow];

                //увеличиваем счетчик использования.
                if (context != null)
                    context.IncrementUsage();
            }
            if (context == null)
                throw new Exception(string.Format("Не удалось получить контекст операции с названием {0}.", contextName));

            return context;
        }

        /// <summary>
        /// Завершает контекст операции с названием operationName.
        /// </summary>
        /// <param name="contextName">Название контекста операции.</param>
        internal void EndMultiThreadContext(string contextName)
        {
            if (string.IsNullOrEmpty(contextName))
                throw new ArgumentNullException("contextName");
            string operationNameLow = contextName.ToLower();
            if (this.StaticContexts.ContainsKey(operationNameLow))
                this.AddRemoveContext(false, contextName);
        }

        /// <summary>
        /// Проверяем наличие текущего контекста выполнения операции с названием contextName.
        /// </summary>
        /// <param name="contextName">Название контекста операции.</param>
        public void CheckMultiThreadContext(string contextName)
        {
            if (string.IsNullOrEmpty(contextName))
                throw new ArgumentNullException("contextName");
            if (!this.StaticContexts.ContainsKey(contextName.ToLower()))
                throw new Exception(string.Format("Операция доступна только в контексте выполнения операции {0}.", contextName));
        }

        /// <summary>
        /// Возвращает true, если в данный момент присутствует контекст выполнения операции с названием contextName.
        /// </summary>
        /// <param name="contextName">Название контекста операции.</param>
        public bool IsMultiThreadContext(string contextName)
        {
            if (string.IsNullOrEmpty(contextName))
                throw new ArgumentNullException("contextName");
            bool result = this.StaticContexts.ContainsKey(contextName.ToLower());
            return result;
        }

        #endregion
    }

    #region Context

    /// <summary>
    /// Представляет контекст операции.
    /// </summary>
    public class OperationContext : IDisposable
    {
        /// <summary>
        /// Создает экземпляр OperationContext.
        /// </summary>
        /// <param name="name">Название контекста операции.</param>
        /// <param name="isMultiThread">Параметр не поддерживается.</param>
        /// <param name="contextManager">Менеджер контекстов.</param>
        internal OperationContext(string name, bool isMultiThread, OperationContextManager contextManager)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("operationName");
            if (contextManager == null)
                throw new ArgumentNullException("contextManager");
            if (isMultiThread)
                throw new NotSupportedException();

            this.IsMultiThread = isMultiThread;

            this.Name = name;
            this.ContextManager = contextManager;

            //устанавливаем счетчик вложенности вызовов на нулевой уровень.
            this.UsageCounter = 0;
        }

        private bool _IsMultiThread;
        private bool IsMultiThread
        {
            get { return _IsMultiThread; }
            set { _IsMultiThread = value; }
        }


        private string _Name;
        /// <summary>
        /// Название контекста выполняемой операции.
        /// </summary>
        public string Name
        {
            get { return _Name; }
            private set { _Name = value; }
        }

        private OperationContextManager _ContextManager;
        /// <summary>
        /// Менеджер контекстов операций.
        /// </summary>
        public OperationContextManager ContextManager
        {
            get { return _ContextManager; }
            private set { _ContextManager = value; }
        }

        private int _UsageCounter;
        /// <summary>
        /// Счетчик использования контекста..
        /// </summary>
        internal int UsageCounter
        {
            get { return _UsageCounter; }
            set { _UsageCounter = value; }
        }

        internal void IncrementUsage()
        {
            if (!this.IsMultiThread)
                this.UsageCounter++;
            else
                Interlocked.Increment(ref this._UsageCounter);
        }

        internal void DecrementUsage()
        {
            if (!this.IsMultiThread)
                this.UsageCounter--;
            else
                Interlocked.Decrement(ref this._UsageCounter);
        }

        /// <summary>
        /// Прекращает контекст данной операции.
        /// </summary>
        public void Dispose()
        {
            //уменьшаем счетчик вложенности вызовов.
            this.DecrementUsage();

            //если достигли корневого вызова - очищаем контекст,
            if (this.UsageCounter == 0)
            {
                if (!this.IsMultiThread)
                    this.ContextManager.EndContext(this.Name);
                else
                    this.ContextManager.EndMultiThreadContext(this.Name);
            }
        }

        /// <summary>
        /// Строковое представление экземпляра класса Context.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(this.Name))
                return this.Name;
            return base.ToString();
        }
    }

    #endregion
}
