using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Storage.Metadata.MSSQL
{
    /// <summary>
    /// Представляет триггер схемы таблицы.
    /// </summary>
    public abstract class DBTriggerSchema
    {
        /// <summary>
        /// Создает экземпляр DBTriggerSchema.
        /// </summary>
        /// <param name="schemaAdapter">Адаптер схемы таблицы, к которой относится триггер.</param>
        protected DBTriggerSchema(DBTableSchemaAdapter schemaAdapter)
        {
            if (schemaAdapter == null)
                throw new ArgumentNullException("schemaAdapter");

            this.SchemaAdapter = schemaAdapter;
        }

        private DBTableSchemaAdapter _SchemaAdapter;
        /// <summary>
        /// Адаптер схемы таблицы.
        /// </summary>
        public DBTableSchemaAdapter SchemaAdapter
        {
            get { return _SchemaAdapter; }
            private set { _SchemaAdapter = value; }
        }

        /// <summary>
        /// Инициализирует относительное название триггера.
        /// </summary>
        /// <returns></returns>
        protected abstract string InitRelativeName();

        private bool __init_RelativeName = false;
        private string _RelativeName;
        /// <summary>
        /// Относительное название триггера, уникальное в рамках таблицы.
        /// </summary>
        public string RelativeName
        {
            get
            {
                if (!__init_RelativeName)
                {
                    _RelativeName = this.InitRelativeName();
                    if (string.IsNullOrEmpty(_RelativeName))
                        throw new Exception(string.Format("Не задано относительное название триггера в схеме таблицы {0}.", this.SchemaAdapter.TableName));

                    __init_RelativeName = true;
                }
                return _RelativeName;
            }
        }

        private bool __init_RelativeNameLow = false;
        private string _RelativeNameLow;
        /// <summary>
        /// Значение свойства RelativeName в нижнем регистре.
        /// </summary>
        protected internal string RelativeNameLow
        {
            get
            {
                if (!__init_RelativeNameLow)
                {
                    if (!string.IsNullOrEmpty(this.RelativeName))
                        _RelativeNameLow = this.RelativeName.ToLower();
                    __init_RelativeNameLow = true;
                }
                return _RelativeNameLow;
            }
        }

        /// <summary>
        /// Возвращает true, если триггер запускается после вставки строки в таблицу.
        /// </summary>
        public abstract bool AfterInsert { get; }

        /// <summary>
        /// Возвращает true, если триггер запускается после обновления строки таблицы.
        /// </summary>
        public abstract bool AfterUpdate { get; }

        /// <summary>
        /// Возвращает true, если триггер запускается после удаления строки таблицы.
        /// </summary>
        public abstract bool AfterDelete { get; }

        protected internal virtual bool UseInArchive
        {
            get { return true; }
        }
        
        /// <summary>
        /// Возвращает функциональное тело триггера для таблицы основных данных.
        /// </summary>
        /// <param name="principalTable">Таблица основных данных.</param>
        /// <returns></returns>
        protected internal virtual string GetBody(DBPrincipalTable principalTable)
        {
            DBTable baseTable = principalTable;
            return this.GetBody(baseTable);
        }        

        /// <summary>
        /// Возвращает функциональное тело триггера для таблицы.
        /// </summary>
        /// <param name="table">Таблица.</param>
        /// <returns></returns>
        protected internal virtual string GetBody(DBTable table)
        {
            throw new NotSupportedException();
        }
    }
}
