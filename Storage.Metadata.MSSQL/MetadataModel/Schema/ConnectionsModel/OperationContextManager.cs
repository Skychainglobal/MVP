using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storage.Metadata.MSSQL
{
    public class OperationContextManager
    {
        /// <summary>
        /// Создает экземпляр OperationContextManager.
        /// </summary>
        public OperationContextManager()
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
                throw new Exception(string.Format("Операция доступна только в контексте выполнения операции {0}.", contextName));
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
}
