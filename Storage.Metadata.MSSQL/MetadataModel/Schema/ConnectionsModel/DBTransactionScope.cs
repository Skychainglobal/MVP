using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storage.Metadata.MSSQL
{
    /// <summary>
    /// Представляет область транзакции.
    /// </summary>
    public class DBTransactionScope : IDisposable
    {
        /// <summary>
        /// Создает экземпляр DBTransactionScope.
        /// </summary>
        internal DBTransactionScope(bool createNew, DBConnectionContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            //создаём текущую транзакцию при необходимости.
            if (!DBTransaction.HasCurrent || createNew)
                this.OwnedTransaction = DBTransaction.CreateCurrent(context);

            this.Context = context;
        }

        private DBTransaction _OwnedTransaction;
        /// <summary>
        /// Транзакция, инициализированная данной областью транзакций.
        /// </summary>
        private DBTransaction OwnedTransaction
        {
            get { return _OwnedTransaction; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                _OwnedTransaction = value;
            }
        }

        private DBConnectionContext _Context;
        /// <summary>
        /// Контекст подключений к базам данных.
        /// </summary>
        public DBConnectionContext Context
        {
            get { return _Context; }
            private set { _Context = value; }
        }

        /// <summary>
        /// Сигнализирует о том, что операции выполненные в рамках данного экземпляра DBTransactionScope успешно завершены.
        /// </summary>
        public void Complete()
        {
            //обрабатываем Complete для корневой области.
            //помечаем транзакцию как завершенную.
            if (this.OwnedTransaction != null)
                this.OwnedTransaction.SetCompleted();
        }

        private bool _WasDisposed;
        /// <summary>
        /// Возвращает true, если метод Dispose был вызыван для области транзакции, владеющей транзакцией.
        /// </summary>
        private bool WasDisposed
        {
            get { return _WasDisposed; }
            set { _WasDisposed = value; }
        }

        /// <summary>
        /// Очищает ресурсы, связанные с транзакцией.
        /// </summary>
        public void Dispose()
        {
            //получаем текущую транзакцию.
            DBTransaction ownedTransaction = this.OwnedTransaction;
            if (ownedTransaction != null)
            {
                //ругаемся, если для области транзакции, владующей транзакцией уже был вызван Dispose().
                if (this.WasDisposed)
                    throw new Exception("Область транзакции уже была завершена. Повторное завершение области транзакции, владеющей транзакцией недопустимо.");

                //проверяем, является ли транзакция текущей.
                ownedTransaction.CheckIsCurrent();

                try
                {
                    //если транзакция была инициализирована, применяем или откатываем ее.
                    if (ownedTransaction.WasOpened)
                    {
                        this.DisposeTransaction(ownedTransaction);
                    }
                }
                finally
                {
                    //очищаем текущую транзакцию.
                    DBTransaction.RemoveCurrent();

                    //устанавливаем признак завершённой области транзакции.
                    this.WasDisposed = true;

                    //вызываем обработку события завершения транзакции.
                    ownedTransaction.FireCompleted();

                    //вызываем обработку отката транзации.
                    ownedTransaction.FireRolledBack();
                }
            }
        }

        /// <summary>
        /// Применяет или откатывает транзакцию.
        /// </summary>
        /// <param name="ownedTransaction">Транзакция для применения или отката.</param>
        private void DisposeTransaction(DBTransaction ownedTransaction)
        {
            if (ownedTransaction == null)
                throw new ArgumentNullException("ownedTransaction");

            //ругаемся, если транзакция не была проинициализирована.
            if (!ownedTransaction.WasOpened)
                throw new Exception("Операция недоступна, поскольку транзакция не была инициализирована.");

            //если транзакция помечена как завершенная, применяем ее, в ином случае откатываем.
            try
            {
                //применяем транзакцию транзакцию.
                if (ownedTransaction.WasCompleted)
                    ownedTransaction.Commit();

                //откатываем транзакцию.
                else
                    ownedTransaction.Rollback();
            }
            finally
            {
                //очищаем ресурсы и закрываем коннекты
                ownedTransaction.Dispose();
            }
        }
    }
}
