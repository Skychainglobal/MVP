using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Storage.Metadata.MSSQL
{
    /// <summary>
    /// Представляет контекст операции.
    /// </summary>
    public class OperationContext : IDisposable
    {
        /// <summary>
        /// Создает экземпляр OperationContext.
        /// </summary>
        /// <param name="name">Название контекста операции.</param>
        /// <param name="contextManager">Менеджер контекстов.</param>
        internal OperationContext(string name, bool isMultiThread, OperationContextManager contextManager)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("operationName");
            if (contextManager == null)
                throw new ArgumentNullException("contextManager");

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
}
