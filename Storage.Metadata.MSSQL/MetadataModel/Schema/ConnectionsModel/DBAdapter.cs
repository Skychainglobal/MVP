using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Storage.Metadata.MSSQL
{
    /// <summary>
    /// Адаптер работы с данными в БД.
    /// </summary>
    public class DBAdapter
    {
        /// <summary>
        /// К-тор.
        /// </summary>
        /// <param name="connection">Подключение к БД.</param>
        internal DBAdapter(DBConnection connection)
        {
            if (connection == null)
                throw new ArgumentNullException("connection");

            this.Connection = connection;
        }

        private int _CommandTimeout;
        /// <summary>
        /// Максимальное время ожидания запроса.
        /// </summary>
        public int CommandTimeout
        {
            get { return _CommandTimeout; }
            set { _CommandTimeout = value; }
        }

        /// <summary>
        /// Устанавливает максимальное время ожидания запроса.
        /// </summary>
        /// <param name="command"></param>
        private void SetCommandTimeout(SqlCommand command)
        {
            if (command == null)
                throw new ArgumentNullException("command");
            if (this.CommandTimeout > 0)
                command.CommandTimeout = this.CommandTimeout;
        }

        private DBConnection _Connection;
        /// <summary>
        /// Подключение к базе данных.
        /// </summary>
        public DBConnection Connection
        {
            get { return _Connection; }
            private set { _Connection = value; }
        }


        #region GetDataTable

        /// <summary>
        /// Возвращает таблицу данных по тексту запроса.
        /// </summary>
        /// <param name="commandText">Текст запроса.</param>
        /// <returns></returns>
        public DataTable GetDataTable(string commandText)
        {
            return this.GetDataTable(commandText, (SqlParameter[])null);
        }

        /// <summary>
        /// Возвращает таблицу данных по тексту запроса и набору параметров запроса.
        /// </summary>
        /// <param name="commandText">Текст запроса.</param>
        /// <param name="sqlParameters">Набор параметров запроса.</param>
        /// <returns></returns>
        public DataTable GetDataTable(string commandText, IEnumerable<SqlParameter> sqlParameters)
        {
            DataTable dtResult = new DataTable();
            SqlParameter[] parameters = null;
            if (sqlParameters != null)
                parameters = new List<SqlParameter>(sqlParameters).ToArray();
            this.FillData(commandText, dtResult, parameters);
            return dtResult;
        }

        /// <summary>
        /// Возвращает таблицу данных по тексту запроса и набору параметров запроса.
        /// </summary>
        /// <param name="commandText">Текст запроса.</param>
        /// <param name="sqlParameters">Набор параметров запроса.</param>
        /// <returns></returns>
        public DataTable GetDataTable(string commandText, params SqlParameter[] sqlParameters)
        {
            DataTable dtResult = new DataTable();
            this.FillData(commandText, dtResult, sqlParameters);
            return dtResult;
        }

        #endregion


        #region GetDataRow

        /// <summary>
        /// Возвращает строку таблицы данных по тексту запроса.
        /// </summary>
        /// <param name="commandText">Текст запроса.</param>
        /// <returns></returns>
        public DataRow GetDataRow(string commandText)
        {
            return this.GetDataRow(commandText, (SqlParameter[])null);
        }

        /// <summary>
        /// Возвращает строку таблицы данных по тексту запроса.
        /// </summary>
        /// <param name="commandText">Текст запроса.</param>
        /// <param name="sqlParameters">Набор параметров запроса.</param>
        /// <returns></returns>
        public DataRow GetDataRow(string commandText, IEnumerable<SqlParameter> sqlParameters)
        {
            DataRow dataRow = null;
            DataTable dtResult = this.GetDataTable(commandText, sqlParameters);
            if (dtResult.Rows.Count > 0)
                dataRow = dtResult.Rows[0];
            return dataRow;
        }

        /// <summary>
        /// Возвращает строку таблицы данных по тексту запроса.
        /// </summary>
        /// <param name="commandText">Текст запроса.</param>
        /// <param name="sqlParameters">Набор параметров запроса.</param>
        /// <returns></returns>
        public DataRow GetDataRow(string commandText, params SqlParameter[] sqlParameters)
        {
            DataRow dataRow = null;
            DataTable dtResult = this.GetDataTable(commandText, sqlParameters);
            if (dtResult.Rows.Count > 0)
                dataRow = dtResult.Rows[0];
            return dataRow;
        }

        #endregion


        #region GetDataCount

        /// <summary>
        /// Возвращает целочисленное значение в качестве результата запроса.
        /// </summary>
        /// <param name="commandText">Текст запроса.</param>
        /// <returns></returns>
        public int GetDataCount(string commandText)
        {
            int count = this.GetScalarValue<int>(commandText);
            return count;
        }

        #endregion


        #region ExecuteProcedure

        /// <summary>
        /// Запускает хранимую процедуру.
        /// </summary>
        /// <param name="procedureName">Название хранимой процедуры.</param>
        /// <param name="sqlParameters">Параметры хранимой процедуры.</param>
        public void ExecuteProcedure(string procedureName, params SqlParameter[] sqlParameters)
        {
            this.ExecuteCommand(procedureName, delegate(SqlCommand cmd)
            {
                //если транзакция не задана(проверка с учетом Suppress), 
                //то оставляем запуск хранимой процедуры стандартным способом.
                if (cmd.Transaction == null)
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.ExecuteNonQuery();
                }
                //если транзакция задана, тогда формируем запрос выполнения процедуры с заданным контекстом базы данных.
                else
                {
                    //формируем текст набора параметров процедуры.
                    StringBuilder stParams = new StringBuilder();
                    StringBuilder stOutParams = new StringBuilder();
                    if (sqlParameters != null && sqlParameters.Length > 0)
                    {

                        foreach (SqlParameter sqlParameter in sqlParameters)
                        {
                            string paramName = sqlParameter.ParameterName;
                            if (!paramName.StartsWith("@"))
                                paramName = string.Format("@{0}", paramName);

                            if (stParams.Length > 0)
                                stParams.Append(",");

                            if (sqlParameter.Direction == ParameterDirection.Output || sqlParameter.Direction == ParameterDirection.InputOutput)
                            {
                                stParams.AppendFormat(" {0} OUTPUT", paramName);
                                stOutParams.AppendFormat(@"
SELECT {0}", paramName);
                            }
                            else
                                stParams.AppendFormat(" {0}", paramName);
                        }
                    }

                    //формируем запрос выполнения хранимки.
                    cmd.CommandText = @"USE [{DatabaseName}]
EXEC {ProcedureName}{Parameters}{OutputParameters}"
                        .ReplaceKey("DatabaseName", this.Connection.DatabaseName)
                        .ReplaceKey("ProcedureName", procedureName)
                        .ReplaceKey("Parameters", stParams)
                        .ReplaceKey("OutputParameters", stOutParams)
                        ;

                    //выполняем запрос хранимой процедуры.
                    cmd.ExecuteNonQuery();
                }
            }, sqlParameters);
        }

        #endregion


        #region GetScalarValue


        /// <summary>
        /// Возвращает скалярное значение в качестве результата запроса.
        /// </summary>
        /// <typeparam name="TResult">Тип возвращаемого скалярного значения.</typeparam>
        /// <param name="commandText">Текст запроса.</param>
        public TResult GetScalarValue<TResult>(string commandText)
        {
            TResult result = this.GetScalarValue<TResult>(commandText, (SqlParameter[])null);
            return result;
        }

        /// <summary>
        /// Возвращает скалярное значение в качестве результата запроса.
        /// </summary>
        /// <typeparam name="TResult">Тип возвращаемого скалярного значения.</typeparam>
        /// <param name="commandText">Текст запроса.</param>
        /// <param name="sqlParameters">Параметры запроса.</param>
        /// <returns></returns>
        public TResult GetScalarValue<TResult>(string commandText, params SqlParameter[] sqlParameters)
        {
            bool resultIsNull = false;
            return this.GetScalarValue<TResult>(commandText, out resultIsNull, sqlParameters);
        }

        /// <summary>
        /// Возвращает скалярное значение в качестве результата запроса.
        /// </summary>
        /// <typeparam name="TResult">Тип возвращаемого скалярного значения.</typeparam>
        /// <param name="commandText">Текст запроса.</param>
        /// <param name="resultIsNull">Устанавливается в true, если результат запроса отсутствует или равен DBNull.Value.</param>
        /// <param name="sqlParameters">Параметры запроса.</param>
        /// <returns></returns>
        public TResult GetScalarValue<TResult>(string commandText, out bool resultIsNull, params SqlParameter[] sqlParameters)
        {
            TResult result = default(TResult);
            bool resultIsNullLocal = false;
            this.ExecuteCommand(commandText, delegate(SqlCommand cmd)
            {
                object resultObj = cmd.ExecuteScalar();

                if (resultObj != null && resultObj != DBNull.Value)
                    result = (TResult)resultObj;
                else
                    resultIsNullLocal = true;
            }, sqlParameters);
            resultIsNull = resultIsNullLocal;
            return result;
        }

        #endregion


        #region ExecuteQuery

        /// <summary>
        /// Выполняет запрос.
        /// </summary>
        /// <param name="commandText">Текст выполняемого запроса.</param>
        public void ExecuteQuery(string commandText)
        {
            this.ExecuteQuery(commandText, null);
        }

        /// <summary>
        /// Выполняет запрос.
        /// </summary>
        /// <param name="commandText">Текст выполняемого запроса.</param>
        /// <param name="sqlParameters">Параметры запроса.</param>
        public void ExecuteQuery(string commandText, params SqlParameter[] sqlParameters)
        {
            this.ExecuteCommand(commandText, delegate(SqlCommand cmd)
            {
                cmd.ExecuteNonQuery();
            }, sqlParameters);
        }

        /// <summary>
        /// Выполняет запрос.
        /// </summary>
        /// <param name="commandText">Текст выполняемого запроса.</param>
        /// <param name="sqlParameters">Параметры запроса.</param>
        public void ExecuteOutOfTransactionQuery(string commandText, params SqlParameter[] sqlParameters)
        {
            DBTransaction currentTransaction = DBTransaction.Current;
            if (currentTransaction == null)
                this.ExecuteQuery(commandText, sqlParameters);
            else
            {
                string commandTextLocal = commandText;
                SqlParameter[] sqlParametersLocal = sqlParameters;
                currentTransaction.Completed += delegate(DBTransaction transaction)
                {
                    this.ExecuteOutOfTransactionQueryInternal(commandTextLocal, sqlParametersLocal);
                };
            }
        }

        /// <summary>
        /// Выполняет запрос.
        /// </summary>
        /// <param name="commandText">Текст выполняемого запроса.</param>
        /// <param name="sqlParameters">Параметры запроса.</param>
        private void ExecuteOutOfTransactionQueryInternal(string commandText, params SqlParameter[] sqlParameters)
        {
            DBTransaction currentTransaction = DBTransaction.Current;
            if (currentTransaction == null)
                this.ExecuteQuery(commandText, sqlParameters);
            else
            {
                this.Connection.Context.SuppressTransaction(delegate()
                {
                    this.ExecuteQuery(commandText, sqlParameters);
                });
            }
        }

        #endregion


        #region FillData

        private void FillData(string query, DataTable dataTable, params SqlParameter[] sqlParameters)
        {
            this.ExecuteCommand(query, delegate(SqlCommand cmd)
            {
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dataTable);
                }
            }, sqlParameters);
        }

        private void FillData(string query, DataSet dataSet, params SqlParameter[] sqlParameters)
        {
            this.ExecuteCommand(query, delegate(SqlCommand cmd)
            {
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dataSet);
                }
            }, sqlParameters);
        }

        #endregion


        #region ExecuteCommand

        /// <summary>
        /// Представляет действие, выполняемое с экземпляром SqlCommand.
        /// </summary>
        /// <param name="cmd">Экземпляр SqlCommand, над которым выполняется действие.</param>
        public delegate void ExecuteCommandCode(SqlCommand cmd);

        /// <summary>
        /// Выполняет действие с экземпляром SqlCommand, созданным по подключению соответсвующему данному адаптеру.
        /// </summary>
        /// <param name="query">Текст команды.</param>
        /// <param name="executeCommandCode">Действие выполняемой с экземпляром SqlCommand.</param>
        /// <param name="sqlParameters">Параметры команды.</param>
        public void ExecuteCommand(string query, ExecuteCommandCode executeCommandCode, params SqlParameter[] sqlParameters)
        {
            this.ExecuteCommandInternal(query, executeCommandCode, sqlParameters);
        }

        /// <summary>
        /// Выполняет действие с экземпляром SqlCommand, созданным по подключению соответсвующему данному адаптеру.
        /// </summary>
        /// <param name="query">Текст команды.</param>
        /// <param name="executeCommandCode">Действие выполняемой с экземпляром SqlCommand.</param>
        /// <param name="sqlParameters">Параметры команды.</param>
        private void ExecuteCommandInternal(string query, ExecuteCommandCode executeCommandCode, params SqlParameter[] sqlParameters)
        {
            if (string.IsNullOrEmpty(query))
                throw new ArgumentNullException("query");
            if (executeCommandCode == null)
                throw new ArgumentNullException("executeCommandCode");

            //получаем текущую транзакцию из контекста.
            DBTransaction currentTransaction = DBTransaction.Current;

            //если sql-транзакция отсутствует, выполняем обычный запрос не в транзакции, используя общий коннект для всех вложенных комманд, если он имеется.
            //иначе - создаем открытый коннект, действующий в продолжение всей транзакции.
            if (currentTransaction == null || this.Connection.Context.IsSuppressContext)
            {
                //выполняем действие без Sql-транзакции.
                //создаем подключение, команду и выполняем запрос
                using (SqlConnection contextConnection = new SqlConnection(this.Connection.ConnectionString))
                {
                    contextConnection.Open();

                    //выполняем команду в рамках открытого подключения.
                    this.TryExecuteCommand(0, query, contextConnection, null, executeCommandCode, sqlParameters);
                }
            }
            else
            {
                //выполняем действие в Sql-транзакции.
                //инициализируем транзакцию данным подключением.
                currentTransaction.InitTransaction(this.Connection);

                //выполняем команду, используя подключение транзакции.
                this.TryExecuteCommand(0, query, currentTransaction.OpenedConnection, currentTransaction.OpenedTransaction, executeCommandCode, sqlParameters);

                //проверяем, что коннект не закрыт, для возможности выполнения следующих комманд при помощи данного коннекта.
                //если даже код до сюда не дойдет, то это не вызовет утечку ресурсов, поэтому блок try/finaly можно здесь не ставить.
                currentTransaction.CheckConnectionOpened();
            }
        }

        private const int MaxTryCounts = 5;
        private void TryExecuteCommand(int tryCount, string query, SqlConnection contextConnection, SqlTransaction contextTransaction, ExecuteCommandCode executeCommandCode, params SqlParameter[] sqlParameters)
        {
            if (string.IsNullOrEmpty(query))
                throw new ArgumentNullException("query");
            if (contextConnection == null)
                throw new ArgumentNullException("contextConnection");
            if (executeCommandCode == null)
                throw new ArgumentNullException("executeCommandCode");

            //инициализируем текст команды.
            string cmdText = query;

            //если задана контекстная транзакция всегда задаем контекст БД, 
            //поскольку после выполнения запроса в экземпляре SqlConnection свойство Database изменяется и становится отличным от Initial Catalog.
            if (contextTransaction != null)
                cmdText = string.Format(@"USE [{0}]
{1}", this.Connection.DatabaseName, query);

            try
            {
                using (SqlCommand cmd = new SqlCommand(cmdText, contextConnection))
                {
                    //если задана транзакция, назначаем ее.
                    if (contextTransaction != null)
                        cmd.Transaction = contextTransaction;

                    this.SetCommandTimeout(cmd);
                    if (sqlParameters != null)
                    {
                        foreach (SqlParameter parameter in sqlParameters)
                        {
                            if (parameter == null)
                                continue;

                            SqlParameter ensuredParam = this.EnsureParameter(tryCount, parameter);
                            cmd.Parameters.Add(ensuredParam);
                        }
                    }

                    //выполняем команду.
                    executeCommandCode(cmd);
                }
            }
            catch (Exception ex)
            {
                string errorText = ex.ToString().ToLower();
                //разрешаем перезапуск по взаимоблокировке только, если код выполняется вне транзакции.
                bool canRetry =
                    (errorText.Contains("deadlock") ||
                    errorText.Contains("взаимоблокировк")) &&
                    contextTransaction == null;
                if (!canRetry || tryCount == MaxTryCounts - 1)
                {
                    //формируем окончательную ошибку и отправляем её наверх.
                    Exception completedEx = new Exception(string.Format("Ошибка при выполнении запроса: {0}Не удалось выполнить запрос в базе данных {1} ***** {2} *****.", this.GetErrorsStack(ex), this.Connection.DisplayName, cmdText), ex);

                    //логируем ошибку, если не применяется логирование методом DBConnectionContext.ExecuteTransactionInternal.
                    if (!this.Connection.Context.IsExecuteTransactionContext)
                        this.Connection.Context.WriteEventLog(completedEx);

                    //отправляем ошибку.
                    throw completedEx;
                }
                else
                {
                    //делаем обработку взаимоблокировок: засыпаем поток, и пытаемся выполнить запрос еще раз.
                    Thread.Sleep(200);

                    //если запрос выполняется в транзакции, то обработку взаимоблокировки делаем вне транзакции.
                    using (SqlConnection suppressedConnection = new SqlConnection(this.Connection.ConnectionString))
                    {
                        suppressedConnection.Open();
                        this.TryExecuteCommand(tryCount + 1, query, suppressedConnection, null, executeCommandCode, sqlParameters);
                    }
                }
            }
        }

        private string GetErrorsStack(Exception ex)
        {
            StringBuilder stMessages = new StringBuilder();
            while (ex != null)
            {
                stMessages.AppendFormat("{0}: {1} {2}", ex.GetType().FullName, ex.Message, Environment.NewLine);
                ex = ex.InnerException;
            }

            return stMessages.ToString();
        }

        private SqlParameter EnsureParameter(int tryCount, SqlParameter sqlParameter)
        {
            if (sqlParameter == null)
                throw new ArgumentNullException("sqlParameter");
            if (tryCount == 0)
                return sqlParameter;
            else
            {
                //пересоздаем параметр для использования в новом экземпляре SqlCommand.
                SqlParameter retryParameter = new SqlParameter(sqlParameter.ParameterName, sqlParameter.SqlDbType);
                retryParameter.Direction = sqlParameter.Direction;
                if (sqlParameter.Direction == ParameterDirection.Input || sqlParameter.Direction == ParameterDirection.InputOutput)
                    retryParameter.Value = sqlParameter.Value;

                return retryParameter;
            }
        }

        #endregion


        #region CreateProgramUnit

        /// <summary>
        /// Возвращает текст запроса создания хранимой функции или процедуры. Результирующий текст включает запрос удаления хранимой функции или процедуры при ее наличии перед созданием.
        /// </summary>
        /// <param name="programUnitName">Название хранимой функции или процедуры.</param>
        /// <param name="programUnitCode">Текст хранимой функции или процедуры. Например: CREATE FUNCTION...</param>
        /// <returns></returns>
        public string GetCreateProgramUnitCode(string programUnitName, string programUnitCode)
        {
            if (string.IsNullOrEmpty(programUnitName))
                throw new ArgumentNullException("programUnitName");
            if (string.IsNullOrEmpty(programUnitCode))
                throw new ArgumentNullException("programUnitCode");

            //обрезаем название юнита.
            programUnitName = programUnitName.Trim('[', ']');

            //проверяем что текст программного юнита является запросом создания хранимой функции или процедуры.
            Regex regCreateUnit = new Regex(@"(?<=^\s*((--.*\s*)*\s+|(/\*[^*]*\*+([^/*][^*]*\*+)*/)*\s+)*)CREATE\s+(?<UnitType>(PROCEDURE|PROC|FUNCTION))\s+(\[dbo\]\s*\.\s*|dbo\s*\.\s*)?(?<UnitName>(\[[^\]]+\]|\w[\w\d]*))", RegexOptions.IgnoreCase);
            //получаем соответствие текста создания юнита.
            Match matchCreateUnit = regCreateUnit.Match(programUnitCode);
            //проверяем корректность текста создания юнита.
            if (!matchCreateUnit.Success)
                throw new ArgumentException("programUnitCode", string.Format("Текст программного юнита [{0}] не является запросом создания хранимой функции или процедуры.", programUnitName));

            //проверяем и получаем тип программного юнита.
            Group unitTypeGroup = matchCreateUnit.Groups["UnitType"];
            if (unitTypeGroup == null || !unitTypeGroup.Success || string.IsNullOrEmpty(unitTypeGroup.Value))
                throw new Exception(string.Format("Не удалось определить тип програмного юнита [{0}] по соответствию текста [{1}].",
                    programUnitName, matchCreateUnit.Value));
            string unitTypeText = unitTypeGroup.Value.ToLower();

            //определяем признак того, что программный юнит является хранимой функцией.
            bool isFunction = unitTypeText == "function";
            bool isProcedure = unitTypeText == "procedure" || unitTypeText == "proc";
            //ругаемся, если тип программного юнита не определён.
            if (!isFunction && !isProcedure)
                throw new Exception(string.Format("Тип программного юнита [{0}] не определён по тексту [{1}].", programUnitName, unitTypeText));

            string unitTitle = isFunction ? "хранимой функции" : "хранимой процедуры";
            //проверяем соответствие названия юнита переданного в параметре метода и название в теле юнита.
            Group unitNameGroup = matchCreateUnit.Groups["UnitName"];
            if (unitNameGroup == null || !unitNameGroup.Success || string.IsNullOrEmpty(unitNameGroup.Value))
                throw new Exception(string.Format("Не удалось определить название {0} юнита [{1}] из тела {0}.", unitTitle, programUnitName));
            string unitNameText = unitNameGroup.Value.Trim('[', ']');

            //ругаемся, если название юнита, переданное в параметре метода, не соответствует названию юнита из тела юнита.
            if (programUnitName.ToLower() != unitNameText.ToLower())
                throw new Exception(string.Format("Название {0} [{1}], полученное из тела {0}, не соответствует названию [{2}].", unitTitle, unitNameText, programUnitName));

            //заменяем операцию создания на динамически определяемую операцию создания или изменения.
            Regex regReplaceCreate = new Regex(@"(?<=^\s*((--.*\s*)*\s+|(/\*[^*]*\*+([^/*][^*]*\*+)*/)*\s+)*)CREATE(?=\s+(PROCEDURE|PROC|FUNCTION)\s+(\[dbo\]\s*\.\s*|dbo\s*\.\s*)?(\[[^\]]+\]|\w[\w\d]*))", RegexOptions.IgnoreCase);
            if (!regReplaceCreate.IsMatch(programUnitCode.QueryEncode()))
                throw new Exception(string.Format("Не удалось обнаружить текст операции CREATE в коде создания {0} [{1}].", unitTitle, programUnitName));
            string programUnitCodeEditor = regReplaceCreate.Replace(programUnitCode.QueryEncode(), "' + @unitAction + N'");

            string resultQuery = @"
/*------------------- {UnitName} ----------------------*/
DECLARE 
    --тип существующего в базе данных программного юнита.
    @currentType nvarchar(10),
    --актуальный тип программного юнита.
    @actualType nvarchar(10),
    --действие по созданию/изменению программного юнита.
    @unitAction nvarchar(10)

--устанавливаем актуальный тип программого юнита.
SET @actualType = N'{ActualUnitType}'

--получаем существующий тип программного юнита.
SELECT @currentType = type FROM sys.objects WITH(NOLOCK) WHERE Name = N'{UnitNameEncoded}'
--если программный юнит не существует или изменил свой тип, например с хранимой функции на хранимую процедуру, 
--устанавливаем в качестве действия операцию создания.
IF(@currentType IS NULL OR @currentType <> @actualType)
BEGIN
    SET @unitAction = N'CREATE'

    --если требуется операция создания и программный юнит существует, удаляем программный юнит.
    IF(@currentType = N'P')
        DROP PROCEDURE [dbo].[{UnitName}]
    ELSE IF(@currentType = N'FN')
        DROP FUNCTION [dbo].[{UnitName}]
END
ELSE
    SET @unitAction = N'ALTER'

--создаём или изменяем программный юнит (хранимую функцию или процедуру).
IF(@unitAction = N'ALTER' OR NOT EXISTS(SELECT Name FROM sys.objects WITH(NOLOCK) WHERE Name = N'{UnitNameEncoded}' AND Type = @actualType))
BEGIN
DECLARE @unitQuery nvarchar(max)
SET @unitQuery = N'
{UnitCode}
'
exec sp_executesql @unitQuery
END
"
                .ReplaceKey("ActualUnitType", isProcedure ? "P" : "FN")
                .ReplaceKey("UnitNameEncoded", programUnitName.QueryEncode())
                .ReplaceKey("UnitName", programUnitName)
                .ReplaceKey("UnitCode", programUnitCodeEditor)
                ;

            return resultQuery;
        }

        /// <summary>
        /// Удаляет при наличии и создает хранимую функцию или процедуру с заданным именем и текстом.
        /// </summary>
        /// <param name="programUnitName">Название хранимой функции или процедуры.</param>
        /// <param name="programUnitCode">Текст хранимой функции или процедуры. Например: CREATE FUNCTION...</param>
        public void CreateProgramUnit(string programUnitName, string programUnitCode)
        {
            if (string.IsNullOrEmpty(programUnitName))
                throw new ArgumentNullException("programUnitName");
            if (string.IsNullOrEmpty(programUnitCode))
                throw new ArgumentNullException("programUnitCode");

            string query = this.GetCreateProgramUnitCode(programUnitName, programUnitCode);
            if (string.IsNullOrEmpty(query))
                throw new Exception(string.Format("Не удалось получить запрос создания функции {0}.", programUnitName));

            this.ExecuteQuery(query);
        }

        #endregion


        /// <summary>
        /// Текстовое представление экземпляра DBAdapter.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(this.Connection.DisplayName))
                return this.Connection.DisplayName;
            return base.ToString();
        }
    }
}
