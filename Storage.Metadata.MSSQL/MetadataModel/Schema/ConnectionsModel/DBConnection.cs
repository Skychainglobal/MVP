using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storage.Metadata.MSSQL
{
    /// <summary>
    /// Представляет класс для выполнения действий в рамках открытого подключения к базе данных.
    /// </summary>
    public class DBConnection
    {
        /// <summary>
        /// К-тор.
        /// </summary>
        /// <param name="databaseServerName">Название экземпляра сервера базы данных.</param>
        /// <param name="databaseName">Название базы данных.</param>
        /// <param name="context">Контекст подключений к базам данных.</param>
        internal DBConnection(string databaseServerName, string databaseName, DBConnectionContext context)
        {
            if (string.IsNullOrEmpty(databaseServerName))
                throw new ArgumentNullException("databaseServerName");

            if (string.IsNullOrEmpty(databaseName))
                throw new ArgumentNullException("databaseName");

            this.DatabaseServerName = databaseServerName;
            this.DatabaseName = databaseName;
            this.Context = context;
        }

        private string _DatabaseServerName;
        /// <summary>
        /// Название экземпляра сервера базы данных.
        /// </summary>
        public string DatabaseServerName
        {
            get { return _DatabaseServerName; }
            private set { _DatabaseServerName = value; }
        }

        private string _DatabaseName;
        /// <summary>
        /// Название базы данных.
        /// </summary>
        public string DatabaseName
        {
            get { return _DatabaseName; }
            private set { _DatabaseName = value; }
        }

        private DBConnectionContext _Context;
        /// <summary>
        /// Контекст подключений к базам данных.
        /// </summary>
        public DBConnectionContext Context
        {
            get { return _Context; }
            set { _Context = value; }
        }

        private bool __init_ConnectionString = false;
        private string _ConnectionString;
        /// <summary>
        /// Строка подключения к базе данных.
        /// </summary>
        public string ConnectionString
        {
            get
            {
                if (!__init_ConnectionString)
                {
                    _ConnectionString = string.Format("Data Source={0};Initial Catalog={1};Integrated Security=True;Application Name=SkychainDataAdapter",
                        this.DatabaseServerName, this.DatabaseName);
                    __init_ConnectionString = true;
                }
                return _ConnectionString;
            }
            internal set
            {
                _ConnectionString = value;
                __init_ConnectionString = true;
            }
        }

        private bool __init_DisplayName = false;
        private string _DisplayName;
        /// <summary>
        /// Отображаемое название подлкючения, состоящее из имени экземпляра сервера баз данных и названия базы данных.
        /// </summary>
        public string DisplayName
        {
            get
            {
                if (!__init_DisplayName)
                {
                    _DisplayName = string.Format("{0}.{1}", this.DatabaseServerName, this.DatabaseName);
                    __init_DisplayName = true;
                }
                return _DisplayName;
            }
        }

        private bool __init_UniqueKey = false;
        private string _UniqueKey;
        /// <summary>
        /// Уникальный ключ базы данных, состоящий из имени экземпляра сервера баз данных и названия базы данных в нижнем регистре.
        /// </summary>
        public string UniqueKey
        {
            get
            {
                if (!__init_UniqueKey)
                {
                    _UniqueKey = DBConnection.GetConnectionKey(this.DatabaseServerName, this.DatabaseName);
                    __init_UniqueKey = true;
                }
                return _UniqueKey;
            }
        }


        #region CreateAdapter

        private bool __init_DataAdapter = false;
        private DBAdapter _DataAdapter;
        /// <summary>
        /// Адаптер по умолчанию доступа к данным для данного подключения.        
        /// </summary>
        public DBAdapter DataAdapter
        {
            get
            {
                if (!__init_DataAdapter)
                {
                    _DataAdapter = this.CreateDataAdapter();
                    __init_DataAdapter = true;
                }
                return _DataAdapter;
            }
        }

        /// <summary>
        /// Создает адаптер доступа к данным для данного подключения.
        /// </summary>
        /// <returns></returns>
        public DBAdapter CreateDataAdapter()
        {
            return new DBAdapter(this);
        }

        #endregion


        /// <summary>
        /// Возвращает уникальный ключ подключения к базе данных, состоящий из имени экземпляра сервера баз данных и названия базы данных в нижнем регистре.
        /// </summary>
        /// <param name="databaseServerName">Имя экземпляра сервера баз данных.</param>
        /// <param name="databaseName">Название базы данных.</param>
        /// <returns></returns>
        internal static string GetConnectionKey(string databaseServerName, string databaseName)
        {
            if (string.IsNullOrEmpty(databaseServerName))
                throw new Exception("databaseServerName");
            if (string.IsNullOrEmpty(databaseName))
                throw new Exception("databaseName");

            string databaseKey = string.Format("{0}.{1}", databaseServerName, databaseName).ToLower();
            return databaseKey;
        }

        /// <summary>
        /// Возвращает true, если сравниваемое с данным подключением подключение имеет одинаковый сервер баз данных и одинаковое название базы данных. 
        /// </summary>
        /// <param name="connectionToCompare">Подключение для сравнения.</param>
        /// <returns></returns>
        public bool DatabaseEqualsTo(DBConnection connectionToCompare)
        {
            if (connectionToCompare == null)
                throw new ArgumentNullException("existingColumnToCompare");

            bool result = this.UniqueKey == connectionToCompare.UniqueKey;
            return result;
        }

        /// <summary>
        /// Возвращает true, если сравниваемое с данным подключением подключение имеет одинаковый сервер баз данных. 
        /// </summary>
        /// <param name="connectionToCompare">Подключение для сравнения.</param>
        /// <returns></returns>
        public bool DatabaseServerEqualsTo(DBConnection connectionToCompare)
        {
            if (connectionToCompare == null)
                throw new ArgumentNullException("existingColumnToCompare");

            bool result = this.DatabaseServerName.ToLower() == connectionToCompare.DatabaseServerName.ToLower();
            return result;
        }

        /// <summary>
        /// Возвращает имя объекта базы данных для использования в запросах.
        /// Если сравниваемое подключение отсутствует, то название объекта возращается в формате [dbo].[Имя объекта].
        /// Если база данных подключения queryConnection совпадает с базой данных в которой хранится объект, то название объекта возращается в формате [dbo].[Имя объекта].
        /// Если база данных подключения queryConnection не совпадает с базой данных в которой хранится объект, но совпадает сервер баз данных, то название объекта возращается в формате [Имя базы данных].[dbo].[Имя объекта].
        /// Если сервер базы данных подключения queryConnection не совпадает с сервером базы данных в которой хранится объект, то название объекта возращается в формате [Имя сервера базы данных].[Имя базы данных].[dbo].[Имя объекта].
        /// </summary>
        /// <param name="objectName">Название объекта в базе данных, в которой он расположен.</param>
        /// <param name="queryConnection">Подключение к базе данных, в контексте которого выполняется запрос к объекту базы данных.</param>
        /// <returns></returns>
        public string GetQueryObjectName(string objectName, DBConnection queryConnection)
        {
            if (objectName == null)
                throw new ArgumentNullException("objectName");

            string prefix = this.GetQueryObjectPrefix(queryConnection);
            string queryName = string.Format("{0}[{1}]", prefix, objectName);
            return queryName;
        }

        /// <summary>
        /// Возвращает префикс объекта базы данных для использования в запросах.
        /// Если сравниваемое подключение отсутствует, то префикс возращается в формате [dbo].
        /// Если база данных подключения queryConnection совпадает с базой данных данного подключения, то префикс возращается в формате [dbo].
        /// Если база данных подключения queryConnection не совпадает с базой данных данного подключения, но совпадает сервер баз данных, то префикс возращается в формате [Имя базы данных].[dbo].
        /// Если сервер базы данных подключения queryConnection не совпадает с сервером базы данных данного, то префикс возращается в формате [Имя сервера базы данных].[Имя базы данных].[dbo].
        /// </summary>
        /// <param name="queryConnection">Подключение к базе данных, в контексте которого выполняется запрос.</param>
        /// <returns></returns>
        public string GetQueryObjectPrefix(DBConnection queryConnection)
        {
            string objectPrefix = "[dbo].";
            if (queryConnection != null)
            {
                if (this.DatabaseEqualsTo(queryConnection))
                    objectPrefix = "[dbo].";
                else if (this.DatabaseServerEqualsTo(queryConnection))
                    objectPrefix = string.Format("[{0}].[dbo].", this.DatabaseName);
                else
                    objectPrefix = string.Format("[{0}].[{1}].[dbo].", this.DatabaseServerName, this.DatabaseName);
            }
            return objectPrefix;
        }

        private bool __init_DatabaseServerMachineName = false;
        private string _DatabaseServerMachineName;
        /// <summary>
        /// Название машины, на которой запущен сервер баз данных.
        /// </summary>
        public string DatabaseServerMachineName
        {
            get
            {
                if (!__init_DatabaseServerMachineName)
                {
                    string result = this.DataAdapter.GetScalarValue<string>("SELECT SERVERPROPERTY('MachineName')");
                    if (string.IsNullOrEmpty(result))
                        throw new Exception(string.Format("Не удалось получить название машины, на которой запущен экземпляр сервера баз данных {0}.", this.DatabaseServerName));

                    _DatabaseServerMachineName = result;
                    __init_DatabaseServerMachineName = true;
                }
                return _DatabaseServerMachineName;
            }
        }

        private bool __init_DatabaseCollation = false;
        private string _DatabaseCollation;
        /// <summary>
        /// Название машины, на которой запущен сервер баз данных.
        /// </summary>
        public string DatabaseCollation
        {
            get
            {
                if (!__init_DatabaseCollation)
                {
                    string query = string.Format("SELECT DATABASEPROPERTYEX(N'{0}', N'Collation')", this.DatabaseName);
                    string result = this.DataAdapter.GetScalarValue<string>(query);
                    if (string.IsNullOrEmpty(result))
                        throw new Exception(string.Format("Не удалось получить COLLATION базы данных {0}.", this.DatabaseName));

                    _DatabaseCollation = result;
                    __init_DatabaseCollation = true;
                }
                return _DatabaseCollation;
            }
        }

        private bool __init_DatabaseServerVersion = false;
        private Version _DatabaseServerVersion;
        /// <summary>
        /// Версия сервера базы данных.
        /// </summary>
        public Version DatabaseServerVersion
        {
            get
            {
                if (!__init_DatabaseServerVersion)
                {
                    string versionString = this.DataAdapter.GetScalarValue<string>("SELECT SERVERPROPERTY('ProductVersion')");
                    if (string.IsNullOrEmpty(versionString))
                        throw new Exception(string.Format("Не удалось получить версию сервера баз данных для подключения {0}.", this.DisplayName));

                    _DatabaseServerVersion = new Version(versionString);

                    __init_DatabaseServerVersion = true;
                }
                return _DatabaseServerVersion;
            }
        }

        private bool __init_TableInfoAdapter = false;
        private DBTableInfoAdapter _TableInfoAdapter;
        /// <summary>
        /// Адаптер доступа к метаданным существующих таблиц.
        /// </summary>
        internal DBTableInfoAdapter TableInfoAdapter
        {
            get
            {
                if (!__init_TableInfoAdapter)
                {
                    _TableInfoAdapter = new DBTableInfoAdapter(this);
                    __init_TableInfoAdapter = true;
                }
                return _TableInfoAdapter;
            }
        }

        /// <summary>
        /// Строковое представление класса DBConnection.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(this.DisplayName))
                return this.DisplayName;
            return base.ToString();
        }
    }
}
