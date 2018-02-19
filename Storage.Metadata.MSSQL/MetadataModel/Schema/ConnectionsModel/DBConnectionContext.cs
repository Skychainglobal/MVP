using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Collections.Generic;
using Storage.Lib;

namespace Storage.Metadata.MSSQL
{
    /// <summary>
    /// Представляет класс для работы с подключениями к базам данных в рамках контекста выполнения кода, 
    /// а также для выполнения распределенной транзакцией запросов между разными базами данных или разными серверами баз данных.
    /// </summary>
    public class DBConnectionContext
    {
        internal DBConnectionContext() { }


        #region CreateTransactionScope

        /// <summary>
        /// Создает область Sql-транзакций.
        /// </summary>
        /// <returns></returns>
        public DBTransactionScope CreateTransactionScope()
        {
            return this.CreateTransactionScope(false);
        }

        /// <summary>
        /// Создает область Sql-транзакций.
        /// </summary>
        /// <param name="createNew">При установленном значении true создает новую транзакцию для данной области транзакций даже при наличии существующей текущей транзакции.</param>
        /// <returns></returns>
        public DBTransactionScope CreateTransactionScope(bool createNew)
        {
            return new DBTransactionScope(createNew, this);
        }

        #endregion


        #region ExecuteTransaction

        /// <summary>
        /// Представляет делегат кода для выполнения в транзакции.
        /// </summary>
        public delegate void TransactionCode();

        /// <summary>
        /// Выполняет код в транзакции запроса к базам данных.
        /// </summary>
        /// <param name="transactionCode">Код для выполнения в транзакции.</param>
        public void ExecuteTransaction(TransactionCode transactionCode)
        {
            this.ExecuteTransaction(transactionCode, false);
        }

        /// <summary>
        /// Выполняет код в транзакции запроса к базам данных.
        /// </summary>
        /// <param name="transactionCode">Код для выполнения в транзакции.</param>
        /// <param name="createNew">При установленном значении true, создаёт новую транзакцию даже при наличии транзакции уже инициализированной в контексте выполнения кода, 
        /// в ином случае использует существующую инициализированную транзакцию в контексте выполнения кода.</param>
        public void ExecuteTransaction(TransactionCode transactionCode, bool createNew)
        {
            if (transactionCode == null)
                throw new ArgumentNullException("transactionCode");

            this.ExecuteTransactionInternal(transactionCode, createNew);

        }

        /// <summary>
        /// Выполняет код в транзакции запроса к базам данных.
        /// </summary>
        /// <param name="transactionCode">Код для выполнения в транзакции.</param>
        /// <param name="createNew">При установленном значении true, создаёт новую транзакцию даже при наличии транзакции уже инициализированной в контексте выполнения кода.</param>
        private void ExecuteTransactionInternal(TransactionCode transactionCode, bool createNew)
        {
            //создаем область транзакции.
            DBTransactionScope transactionScope = this.CreateTransactionScope(createNew);
            Exception transactionException = null;
            try
            {
                using (OperationContext executeTransactionContext = this.RunExecuteTransactionContext())
                {
                    //выполняем код.
                    transactionCode();

                    //помечаем транзакцию, как успешно выполненную.
                    transactionScope.Complete();
                }
            }
            catch (Exception ex)
            {
                //устанавливаем ошибку транзакции.
                transactionException = ex;

                //логируем ошибку транзакции.
                this.WriteEventLog(transactionException);

                //перенаправляем ошибку транзакции вверх по стэку.
                throw;
            }
            finally
            {
                //выполняем очистку области транзакции.
                Exception disposeException = null;
                try
                {
                    //очищаем область транзакции.
                    transactionScope.Dispose();
                }
                catch (Exception ex)
                {
                    //устанавливаем ошибку очистки.
                    disposeException = ex;

                    //логируем ошибку очистки.
                    this.WriteEventLog(disposeException);

                    //перенаправляем ошибку очистки вверх по стэку.
                    throw;
                }
                finally
                {
                    //обрабатываем ошибку транзакции и ошибку очистки.
                    if (transactionException != null && disposeException != null)
                    {
                        //перенаправляем вверх по стэку суммарную ошибку транзакции и очистки.
                        throw new Exception(string.Format(@"Возникли ошибки при выполнении и очистки транзакции. 
ExecuteTransactionError: {0} 
DisposeTransactionError: {1}", transactionException, disposeException));
                    }
                }

            }
        }

        /// <summary>
        /// Записывает текст исключения в лог операционной системы.
        /// </summary>
        /// <param name="ex">Исключение, текст которого записывается в лог операционной системы.</param>
        internal void WriteEventLog(Exception ex)
        {
            if (ex == null)
                throw new ArgumentNullException("ex", "Не передано исключение времени выполнения кода.");

            try
            {
                EventLog.WriteEntry("Storage.Metadata.MSSQL", string.Format("{0}{1}{1}{1}Stack trace above exception:{1}{2}",
                    ex, Environment.NewLine, Environment.StackTrace), EventLogEntryType.Error);
            }
            catch { }
        }

        #endregion


        #region Contexts

        private bool __init_ContextManager = false;
        private OperationContextManager _ContextManager;
        /// <summary>
        /// Управляет контекстами операций.
        /// </summary>
        internal OperationContextManager ContextManager
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


        #region ExecuteTransactionContext

        private const string __context_ExecuteTransaction = "ExecuteTransactionContext";
        /// <summary>
        /// Запускает контекст выполнения кода в рамках метода ExecuteTransactionInternal.
        /// </summary>
        /// <returns></returns>
        private OperationContext RunExecuteTransactionContext()
        {
            return this.ContextManager.BeginContext(__context_ExecuteTransaction);
        }

        /// <summary>
        /// Возвращает true, если метод выполняется в контексте выполнения метода ExecuteTransactionInternal.
        /// </summary>
        internal bool IsExecuteTransactionContext
        {
            get { return this.ContextManager.IsContext(__context_ExecuteTransaction); }
        }

        #endregion


        #region SuppressContext


        private const string __context_SuppressContext = "SuppressContext";

        /// <summary>
        /// Запускает контекст операции выполнения кода вне транзакции.
        /// </summary>
        /// <returns></returns>
        private OperationContext RunSuppressContext()
        {
            return this.ContextManager.BeginContext(__context_SuppressContext);
        }

        /// <summary>
        /// Возвращает true, если текущий контекст операции является выполнением кода вне транзакции.
        /// </summary>
        /// <returns></returns>
        internal bool IsSuppressContext
        {
            get { return this.ContextManager.IsContext(__context_SuppressContext); }
        }

        /// <summary>
        /// Код, выполняемый вне транзакции.
        /// </summary>
        public delegate void SuppressTransactionCode();

        /// <summary>
        /// Выполняет код вне текущей транзакции, если она существует.
        /// </summary>
        /// <param name="suppressTransactionCode">Код, выполняемый вне транзакции.</param>
        public void SuppressTransaction(SuppressTransactionCode suppressTransactionCode)
        {
            if (suppressTransactionCode == null)
                throw new ArgumentNullException("suppressTransactionCode");

            //запускаем код, отключая контекст транзакции, т.е. любая операция, выполняемая в коде suppressTransactionCode в рамках транакции, 
            //не будет использовать транзакцию.
            using (OperationContext suppressContext = this.RunSuppressContext())
            {
                suppressTransactionCode();
            }
        }

        #endregion


        #region SummaryTablesMetadataContext

        private const string __context_SummaryTablesMetadata = "SummaryTablesMetadataContext";
        /// <summary>
        /// Запускает контекст выполнения кода, при котором получение метаданных существующих таблиц производится одним запросом, возвращающим метаданных всех таблиц базы данных.
        /// </summary>
        /// <returns></returns>
        public OperationContext RunSummaryTablesMetadataContext()
        {
            return this.ContextManager.BeginContext(__context_SummaryTablesMetadata);
        }

        /// <summary>
        /// Возвращает true, если текущий контекст выполнения является SummaryTablesMetadataContext.
        /// </summary>
        internal bool IsSummaryTablesMetadataContext
        {
            get
            {
                return this.ContextManager.IsContext(__context_SummaryTablesMetadata);
            }
        }

        #endregion

        #endregion


        #region Connections

        private bool __init_Connections = false;
        private Dictionary<string, DBConnection> _Connections;
        /// <summary>
        /// Коллекция подключений к базам данных, используемых в контексте выполнения.
        /// </summary>
        private Dictionary<string, DBConnection> Connections
        {
            get
            {
                if (!__init_Connections)
                {
                    _Connections = new Dictionary<string, DBConnection>();
                    __init_Connections = true;
                }
                return _Connections;
            }
        }

        /// <summary>
        /// Возвращает новый или существующий экземпляр подключения к базе данных по имени сервера и названию базы данных.
        /// </summary>
        /// <param name="databaseServerName">Имя сервера баз данных.</param>
        /// <param name="databaseName">Название базы данных.</param>
        /// <returns></returns>
        public DBConnection GetConnection(string databaseServerName, string databaseName)
        {
            if (string.IsNullOrEmpty(databaseServerName))
                throw new Exception("databaseServerName");
            if (string.IsNullOrEmpty(databaseName))
                throw new Exception("databaseName");

            DBConnection connection = null;
            string databaseKey = DBConnection.GetConnectionKey(databaseServerName, databaseName);
            if (!this.Connections.ContainsKey(databaseKey))
            {
                //добавляем подключение в контекст.
                connection = new DBConnection(databaseServerName, databaseName, this);
                this.Connections.Add(databaseKey, connection);
            }
            else
            {
                connection = this.Connections[databaseKey];
            }
            if (connection == null)
                throw new Exception(string.Format("Не удалось получить подключение к базе данных по ключу {0}.", databaseKey));
            return connection;
        }

        /// <summary>
        /// Возвращает новый или существующий экземпляр подключения к базе данных по строке подключения к базе данных.
        /// </summary>
        /// <param name="connectionString">Строка подключения к базе данных.</param>
        /// <returns></returns>
        public DBConnection GetConnection(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException("connectionString");

            SqlConnectionStringBuilder cnBuilder = new SqlConnectionStringBuilder(connectionString);
            return this.GetConnection(cnBuilder.DataSource, cnBuilder.InitialCatalog);
        }

        #endregion
    }
}
