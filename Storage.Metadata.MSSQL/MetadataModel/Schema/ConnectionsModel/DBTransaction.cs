using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Storage.Metadata.MSSQL
{
    /// <summary>
    /// Представляет транзакцию.
    /// </summary>
    public class DBTransaction
    {
        /// <summary>
        /// Создает экземпляр транзакции.
        /// </summary>
        internal DBTransaction(DBConnectionContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            this.Context = context;

            //при создании транзакции, помечаем её как текущую.
            this.SetIsCurrent(true);
        }

        private bool _IsCurrent;
        /// <summary>
        /// Возвращает true, если транзакция является текущей.
        /// </summary>
        internal bool IsCurrent
        {
            get { return _IsCurrent; }
            private set { _IsCurrent = value; }
        }

        /// <summary>
        /// Устанавливает признак того, что данная транзакция является текущей или не является таковой.
        /// </summary>
        /// <param name="isCurrent">Признак того, что данная транзакция явлеяется текущей.</param>
        internal void SetIsCurrent(bool isCurrent)
        {
            this.IsCurrent = isCurrent;
        }

        /// <summary>
        /// Проверяет, является ли транзакция текущей. Генерирует исключение, если транзакция не является текущей.
        /// </summary>
        internal void CheckIsCurrent()
        {
            //генерируем исключение с деталями трассировки.
            if (!this.IsCurrent)
                throw new Exception(string.Format("Операция доступна только для транзакции, которая является текущей в контексте выполнения кода. Завершите текущую транзакцию, отличающуюся от данной транзакции, и повторите операцию."));
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

        private DBConnection _EntryPointConnection;
        /// <summary>
        /// Подключение, по которому инициализирована транзакция.
        /// </summary>
        internal DBConnection EntryPointConnection
        {
            get
            {
                if (_EntryPointConnection == null)
                    throw new Exception("Не задано подключение точки входа, инициализирующее транзакцию.");
                return _EntryPointConnection;
            }
            private set { _EntryPointConnection = value; }
        }

        private SqlConnection _OpenedConnection;
        /// <summary>
        /// Открытое подключение, ассоциированное с транзакцией.
        /// </summary>
        internal SqlConnection OpenedConnection
        {
            get
            {
                if (_OpenedConnection == null)
                    throw new Exception("Не иницилизировано подключение транзакции.");
                return _OpenedConnection;
            }
            private set { _OpenedConnection = value; }
        }

        private SqlTransaction _OpenedTransaction;
        /// <summary>
        /// Экземпляр открытой транзакции.
        /// </summary>
        internal SqlTransaction OpenedTransaction
        {
            get
            {
                if (_OpenedTransaction == null)
                    throw new Exception("Не иницилизирован экземпляр транзакции.");
                return _OpenedTransaction;
            }
            private set { _OpenedTransaction = value; }
        }

        /// <summary>
        /// Возвращает true, если транзакция была инициализирована.
        /// </summary>
        internal bool WasOpened
        {
            get { return this.__init_InitTransaction; }
        }

        internal bool __init_InitTransaction = false;
        internal void InitTransaction(DBConnection connection)
        {
            if (connection == null)
                throw new ArgumentNullException("connection");

            //проверяем, что транзакция является текущей.
            this.CheckIsCurrent();

            if (!this.__init_InitTransaction)
            {
                //создаем коннект и открываем его
                this.OpenedConnection = new SqlConnection(connection.ConnectionString);
                this.OpenedConnection.Open();

                //устаналиваем коннект и открываем транзакцию для данной транзакции.
                this.OpenedTransaction = this.OpenedConnection.BeginTransaction();
                this.EntryPointConnection = connection;

                this.__init_InitTransaction = true;
            }
            else
            {
                //ругаемся, если сервер не совпадает.
                if (!connection.DatabaseServerEqualsTo(this.EntryPointConnection))
                    throw new Exception(string.Format("Подключение с точкой входа {0} не может быть использовано в контексте данной транзакции, поскольку для транзакции уже открыто подключение с точкой входа {1}.",
                        connection.DatabaseServerName, this.EntryPointConnection.DatabaseServerName));

                //проверяем, что коннект не закрыт.
                this.CheckTransactionConnectionOpened();
            }
        }

        /// <summary>
        /// Проверяет наличие открытого состояние у подключения к базе данных.
        /// </summary>
        internal void CheckConnectionOpened()
        {
            //генерируем исключение с деталями трассировки.
            if (this.OpenedConnection.State != ConnectionState.Open)
                throw new Exception(string.Format("Обнаружено закрытое состояния подключения к базе данных по строке '{0}'.", this.OpenedConnection.ConnectionString));
        }

        /// <summary>
        /// Проверяет наличие открытого состояние у подключения транзакции к базе данных.
        /// </summary>
        private void CheckTransactionConnectionOpened()
        {
            //генерируем исключение с деталями трассировки.
            if (this.OpenedConnection.State != ConnectionState.Open)
                throw new Exception(string.Format("Обнаружено закрытое состояния подключения к базе данных по строке '{0}'.", this.OpenedConnection.ConnectionString));
            else if (this.OpenedTransaction.Connection == null)
                throw new Exception(string.Format("Отсутствует подключение для открытой транзакции."));
            else if (this.OpenedTransaction.Connection.State != ConnectionState.Open)
                throw new Exception(string.Format("Обнаружено закрытое состояния подключения открытой транзакции к базе данных по строке '{0}'.", this.OpenedTransaction.Connection.ConnectionString));
        }


        /// <summary>
        /// Предствляет делегат, обрабатывающий событие Completed.
        /// </summary>
        /// <param name="transaction">Транзакция.</param>
        public delegate void CompletedHandler(DBTransaction transaction);

        /// <summary>
        /// Событие возникающее при завершении транзации в случае отсутствия ошибок во время выполнения кода.
        /// </summary>
        public event CompletedHandler Completed;

        /// <summary>
        /// Инициирует событие Completed.
        /// </summary>
        internal void FireCompleted()
        {
            if (this.WasCompleted && this.Completed != null)
            {
                try
                {
                    this.Completed(this);
                }
                catch (Exception ex)
                {
                    throw new Exception("Ошибка при обработке успешного завершения транзакции.", ex);
                }
            }
        }

        private bool _WasCompleted;
        internal bool WasCompleted
        {
            get { return _WasCompleted; }
            private set { _WasCompleted = value; }
        }

        internal void SetCompleted()
        {
            if (this.WasCompleted)
                throw new Exception("Транзакция уже была помечена как завершенная.");
            this.WasCompleted = true;
        }

        /// <summary>
        /// Текущая транзакция.
        /// </summary>
        public static DBTransaction Current
        {
            get
            {
                Stack<DBTransaction> transactionStack = DBTransaction.TransactionsStack;
                if (transactionStack.Count > 0)
                    return DBTransaction.EnsureCurrent(transactionStack.Peek());
                return null;
            }
        }

        /// <summary>
        /// Проверяет наличие экземпляра текущей транзакции и возвращает её.
        /// </summary>
        /// <param name="transaction">Экземпляр транзакции.</param>
        /// <returns></returns>
        private static DBTransaction EnsureCurrent(DBTransaction transaction)
        {
            if (transaction == null)
                throw new Exception("Не удалось получить текущую транзакцию.");
            return transaction;
        }

        /// <summary>
        /// Возвращает true, если текущая транзакция инициализирована.
        /// </summary>
        internal static bool HasCurrent
        {
            get { return DBTransaction.TransactionsStack.Count > 0; }
        }

        [ThreadStatic]
        private static Stack<DBTransaction> _TransactionsStack;

        /// <summary>
        /// Набор отдельно запущенных транзакций.
        /// </summary>
        private static Stack<DBTransaction> TransactionsStack
        {
            get
            {
                //проверяем наличие стэка транзакций.
                if (_TransactionsStack == null)
                    _TransactionsStack = new Stack<DBTransaction>();
                return _TransactionsStack;
            }
        }

        /// <summary>
        /// Создает текущую транзакцию и добавляет ее в стэк отдельно запущенных транзакций.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        internal static DBTransaction CreateCurrent(DBConnectionContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            //получаем стэк транзакций.
            Stack<DBTransaction> transactionsStack = DBTransaction.TransactionsStack;

            //создаем транзакцию.
            DBTransaction transaction = new DBTransaction(context);

            //убираем признак текущей транзакции с последней текущей транзакции, расположенной в стэке.
            if (transactionsStack.Count > 0)
            {
                DBTransaction currentTransaction = DBTransaction.EnsureCurrent(transactionsStack.Peek());
                currentTransaction.SetIsCurrent(false);
            }

            //добавляем транзакцию в стэк и назначаем текущую транзакцию.
            transactionsStack.Push(transaction);

            //возвращаем созданную новую текущую транзакцию.
            return transaction;
        }

        /// <summary>
        /// Удаляет текущую транзакцию.
        /// </summary>
        internal static void RemoveCurrent()
        {
            //получаем стэк транзакций текущего потока.
            Stack<DBTransaction> transactionsStack = DBTransaction.TransactionsStack;

            //проверяем наличие записей в стэке.
            if (transactionsStack.Count == 0)
                throw new Exception("Отсутствуют транзакции в стэке текущих транзакций.");

            //удаляем текущую транзакцию из коллекции.
            DBTransaction removedTransaction = transactionsStack.Pop();

            //убираем признак текущей транзакции с удалённой транзакции.
            removedTransaction.SetIsCurrent(false);

            //устанавливаем признак текущей транзакции на следующую транзакцию, расположенную в стэке.
            if (transactionsStack.Count > 0)
            {
                DBTransaction currentTransaction = DBTransaction.EnsureCurrent(transactionsStack.Peek());
                currentTransaction.SetIsCurrent(true);
            }
        }

        /// <summary>
        /// Проверяет наличие текущей транзакции в контексте выполнения кода.
        /// </summary>
        public static void CheckCurrent()
        {
            if (!DBTransaction.HasCurrent)
                throw new Exception("Отсутствует текущая транзакция выполения запросов к базам данных.");
        }

        /// <summary>
        /// Применяет действия выполненные в рамках транзакции.
        /// </summary>
        internal void Commit()
        {
            //проверяем наличие открытого подключения.
            this.CheckTransactionConnectionOpened();

            //применяем транзакцию.
            this.TryExecuteSingle(this.OpenedTransaction.Commit);
        }


        private bool _WasRolledBack;
        /// <summary>
        /// Возвращает true, в случае успешного отката транзации.
        /// </summary>
        private bool WasRolledBack
        {
            get { return _WasRolledBack; }
            set { _WasRolledBack = value; }
        }


        /// <summary>
        /// Откытывает действия, выполненые в рамках транзакции.
        /// </summary>
        internal void Rollback()
        {
            //проверяем наличие открытого подключения.
            this.CheckConnectionOpened();

            //откатываем транзакцию.
            this.TryExecute(this.OpenedTransaction.Rollback);

            //устанавливаем признак отката транзации.
            this.WasRolledBack = true;
        }


        /// <summary>
        /// Предствляет делегат, обрабатывающий событие RolledBack.
        /// </summary>
        /// <param name="transaction">Транзакция.</param>
        public delegate void RolledBackHandler(DBTransaction transaction);

        /// <summary>
        /// Событие возникающее после отката транзации в случае наличия ошибок во время выполнения кода.
        /// </summary>
        public event RolledBackHandler RolledBack;

        /// <summary>
        /// Инициирует событие RolledBack.
        /// </summary>
        internal void FireRolledBack()
        {
            if (this.WasRolledBack && this.RolledBack != null)
            {
                try
                {
                    this.RolledBack(this);
                }
                catch (Exception ex)
                {
                    throw new Exception("Ошибка при обработке отката транзакции.", ex);
                }
            }
        }

        /// <summary>
        /// Очищает ресурсы, задействованные экземпляром класса DBTransaction.
        /// </summary>
        internal void Dispose()
        {
            //закрываем коннекты и очищаем транзакцию.
            this.TryExecute(this.OpenedTransaction.Dispose);
            this.OpenedTransaction = null;

            this.TryExecute(this.OpenedConnection.Dispose);
            this.OpenedConnection = null;

            this.EntryPointConnection = null;
            this.__init_InitTransaction = false;
        }

        /// <summary>
        /// Пытается 1 раз выполнить код. Прерывает выполнение в случае возникновения ошибки.
        /// </summary>
        /// <param name="tryCode">Код для выполнения.</param>
        private void TryExecuteSingle(TryExecuteCode tryCode)
        {
            if (tryCode == null)
                throw new ArgumentNullException("tryCode");

            try
            {
                //пытаемся выполнить код.
                tryCode();
            }
            catch (Exception ex)
            {
                ///логируем последюю ошибку.
                this.WriteEventLog(ex);

                //выбрасываем ошибку на верх.
                throw;
            }
        }

        /// <summary>
        /// Пытается 5 раз выполнить код с задержкой 200мс в случае неудачных попыток.
        /// </summary>
        /// <param name="tryCode">Код для выполнения.</param>
        private void TryExecute(TryExecuteCode tryCode)
        {
            this.TryExecuteInternal(0, tryCode);
        }

        private delegate void TryExecuteCode();
        private const int MaxTryCount = 5;
        /// <summary>
        /// Пытается 5 раз выполнить код с задержкой 200мс в случае неудачных попыток.
        /// </summary>
        /// <param name="tryIndex">Индекс попытки. Стартовое значение должно быть равно 0.</param>
        /// <param name="tryCode">Код для выполнения.</param>
        private void TryExecuteInternal(int tryIndex, TryExecuteCode tryCode)
        {
            if (tryCode == null)
                throw new ArgumentNullException("tryCode");

            try
            {
                //пытаемся выполнить код.
                tryCode();
            }
            catch (Exception ex)
            {
                //если количество попыток достигло максимального, выбрасываем исключение наверх.
                if (tryIndex == MaxTryCount - 1)
                {
                    //логируем последюю ошибку.
                    this.WriteEventLog(ex);

                    //выбрасываем ошибку на верх.
                    throw;
                }
                else
                {
                    //логируем первую ошибку.
                    if (tryIndex == 0)
                        this.WriteEventLog(ex);

                    //выполняем задержку в 200мс.
                    Thread.Sleep(200);

                    //предпринимаем следующую попытку выполнения кода, увеличивая индекс попытки.
                    this.TryExecuteInternal(tryIndex + 1, tryCode);
                }
            }
        }

        /// <summary>
        /// Записывает текст исключения в лог операционной системы.
        /// </summary>
        /// <param name="ex">Исключение, текст которого записывается в лог операционной системы.</param>
        private void WriteEventLog(Exception ex)
        {
            if (ex == null)
                throw new ArgumentNullException("ex", "Не передано исключение времени выполнения кода.");

            try
            {
                EventLog.WriteEntry("Storage.Metadata.MSSQL", ex.ToString(), EventLogEntryType.Error);
            }
            catch { }
        }
    }
}
