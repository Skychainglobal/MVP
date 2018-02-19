using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Storage.Metadata.MSSQL
{
    /// <summary>
    /// Представляет триггер таблицы.
    /// </summary>
    public class DBTrigger
    {
        /// <summary>
        /// Создает экземпляр DBTrigger.
        /// </summary>
        /// <param name="schema">Схема триггера.</param>
        /// <param name="table">Таблица.</param>
        internal DBTrigger(DBTriggerSchema schema, DBTable table)
        {
            if (schema == null)
                throw new ArgumentNullException("schema");
            if (table == null)
                throw new ArgumentNullException("table");

            this.Schema = schema;
            this.Table = table;
        }

        private DBTriggerSchema _Schema;
        /// <summary>
        /// Схема триггера в таблице.
        /// </summary>
        public DBTriggerSchema Schema
        {
            get { return _Schema; }
            private set { _Schema = value; }
        }

        private DBTable _Table;
        /// <summary>
        /// Таблица.
        /// </summary>
        public DBTable Table
        {
            get { return _Table; }
            private set { _Table = value; }
        }

        /// <summary>
        /// Адаптер схемы таблицы.
        /// </summary>
        public DBTableSchemaAdapter SchemaAdapter
        {
            get { return this.Table.SchemaAdapter; }
        }

        /// <summary>
        /// Возвращает true, если триггер запускается после вставки строки в таблицу.
        /// </summary>
        public bool AfterInsert
        {
            get { return this.Schema.AfterInsert; }
        }

        /// <summary>
        /// Возвращает true, если триггер запускается после обновления строки таблицы.
        /// </summary>
        public bool AfterUpdate
        {
            get { return this.Schema.AfterUpdate; }
        }

        /// <summary>
        /// Возвращает true, если триггер запускается после удаления строки таблицы.
        /// </summary>
        public bool AfterDelete
        {
            get { return this.Schema.AfterDelete; }
        }


        #region Name

        private bool __init_Name = false;
        private string _Name;
        /// <summary>
        /// Полное название триггера, уникальное в рамках базы данных.
        /// </summary>
        public string Name
        {
            get
            {
                if (!__init_Name)
                {
                    _Name = string.Format("TR_{0}_{1}", this.Table.Prefix, this.Schema.RelativeName);
                    __init_Name = true;
                }
                return _Name;
            }
        }

        private bool __init_NameLow = false;
        private string _NameLow;
        /// <summary>
        /// Название триггера в нижнем регистре.
        /// </summary>
        internal string NameLow
        {
            get
            {
                if (!__init_NameLow)
                {
                    if (!string.IsNullOrEmpty(this.Name))
                        _NameLow = this.Name.ToLower();
                    __init_NameLow = true;
                }
                return _NameLow;
            }
        }

        private bool __init_NameChanged = false;
        private bool _NameChanged;
        /// <summary>
        /// Возвращает true, если название триггера изменилось в контексте выполнения кода.
        /// </summary>
        internal bool NameChanged
        {
            get
            {
                if (!__init_NameChanged)
                {
                    _NameChanged = this.Exists && this.ExistingTrigger.NameLow != this.NameLow;
                    __init_NameChanged = true;
                }
                return _NameChanged;
            }
        }

        /// <summary>
        /// Сбрасываем инициализацию названия триггера и зависимых от него свойств.
        /// </summary>
        internal virtual void ResetName()
        {
            this.__init_Name = false;
            this.__init_NameLow = false;
            this.__init_NameChanged = false;

            //сбрасываем инициализацию существующего триггера.
            if (this.NameChanged)
                this.ResetExistingTrigger();
        }

        #endregion


        #region ExistingTrigger

        private bool __init_ExistingTrigger = false;
        private DBTriggerInfo _ExistingTrigger;
        /// <summary>
        /// Существующий триггер таблицы.
        /// </summary>
        public DBTriggerInfo ExistingTrigger
        {
            get
            {
                if (!__init_ExistingTrigger)
                {
                    _ExistingTrigger = null;
                    if (!this.Table.RenameRequired)
                    {
                        if (this.Table.ExistingTable != null)
                            _ExistingTrigger = this.GetExistingTrigger(this.Table.ExistingTable);
                    }
                    else
                    {
                        if (this.Table.OriginalExistingTable != null)
                            _ExistingTrigger = this.GetExistingTrigger(this.Table.OriginalExistingTable);
                    }
                    __init_ExistingTrigger = true;
                }
                return _ExistingTrigger;
            }
        }

        /// <summary>
        /// Возвращает существующий триггер существующей таблицы, соответвующий схеме триггера.
        /// </summary>
        /// <param name="existingTable">Существующая таблица.</param>
        /// <returns></returns>
        private DBTriggerInfo GetExistingTrigger(DBTableInfo existingTable)
        {
            if (existingTable == null)
                throw new ArgumentNullException("existingTable");

            //получаем триггер по имени
            DBTriggerInfo existingTrigger = existingTable.GetTrigger(this.Name);

            //если триггер по полному имени не получили, пытаемся найти триггер относительному имени.
            if (existingTrigger == null)
            {
                foreach (DBTriggerInfo triggerInfo in existingTable.Triggers)
                {
                    if (triggerInfo.NameLow.EndsWith("_" + this.Schema.RelativeNameLow))
                    {
                        existingTrigger = triggerInfo;
                        break;
                    }
                }
            }
            return existingTrigger;
        }

        private bool __init_Exists = false;
        private bool _Exists;
        /// <summary>
        /// Возвращает true, если триггер присутствует в таблице.
        /// </summary>
        public bool Exists
        {
            get
            {
                if (!__init_Exists)
                {
                    _Exists = this.ExistingTrigger != null;
                    __init_Exists = true;
                }
                return _Exists;
            }
        }

        /// <summary>
        /// Проверяет существование триггера в таблице. Генерирует исключение в случае отсутствия триггера в таблице.
        /// </summary>
        protected internal void CheckExists()
        {
            if (!this.Exists)
                throw new Exception(string.Format("Операция доступна только для существующего триггера [{0}] в таблице {1}.", this.Name, this.Table.Name));
        }

        /// <summary>
        /// Проверяет отсутствие триггера в таблице. Генерирует исключение в случае наличия триггера в таблице.
        /// </summary>
        protected internal void CheckNotExists()
        {
            if (this.Exists)
                throw new Exception(string.Format("Операция доступна только для несозданного триггера [{0}] в таблице {1}.", this.Name, this.Table.Name));
        }

        /// <summary>
        /// Сбрасывает значения свойств существующего триггера.
        /// </summary>
        internal void ResetExistingTrigger()
        {
            this.__init_ExistingTrigger = false;
            this.__init_Exists = false;
            this.__init_RecreateRequired = false;
            this.__init_UpdateRequired = false;
            this.__init_NameChanged = false;
        }

        #endregion


        #region UpdateRequired

        private bool __init_RecreateRequired = false;
        private bool _RecreateRequired;
        /// <summary>
        /// Возвращает true, требуется пересоздать триггер, например, когда название триггера изменилось.
        /// Поскольку триггеры не поддерживают переименование, необходимо его пересоздать.
        /// </summary>
        internal bool RecreateRequired
        {
            get
            {
                if (!__init_RecreateRequired)
                {
                    _RecreateRequired = this.NameChanged;
                    __init_RecreateRequired = true;
                }
                return _RecreateRequired;
            }
        }

        private bool __init_UpdateRequired = false;
        private bool _UpdateRequired;
        /// <summary>
        /// Возвращает true, если требуется обновление существующего триггера вследствие изменения кода триггера.
        /// </summary>
        internal bool UpdateRequired
        {
            get
            {
                if (!__init_UpdateRequired)
                {
                    //устанавливаем значение по умолчанию, как нетребующее обновления.
                    _UpdateRequired = false;

                    //сравниваем тело существующего триггера с телом триггера по схеме.
                    if (this.Exists)
                    {
                        //получаем полное тело триггера для создания в соответствии со схемой.
                        string triggerFullBody = this.GetTriggerBody(true);

                        //определяем необходимость обновления, если тело триггера схемы отличается от тела существующего триггера.
                        _UpdateRequired = triggerFullBody != this.ExistingTrigger.Body;
                    }
                    __init_UpdateRequired = true;
                }
                return _UpdateRequired;
            }
        }

        /// <summary>
        /// Сбрасывает флаг инициализации свойства UpdateRequired.
        /// </summary>
        internal void ResetUpdateRequired()
        {
            this.__init_UpdateRequired = false;
        }

        #endregion


        #region GetTriggerBody

        /// <summary>
        /// Возвращает текст создания или обновления триггера.
        /// </summary>
        /// <param name="isCreate">При установленном значении true возвращает текст запроса создания триггера, иначе возвращает текст обновления триггера.</param>
        /// <returns></returns>
        private string GetTriggerBody(bool isCreate)
        {
            //получаем функциональное тело триггера.
            string triggerBody = this.Table.GetTriggerBody(this.Schema);
            if (string.IsNullOrEmpty(triggerBody))
                throw new Exception(string.Format("Не удалось получить тело триггера {0}.", this.Name));

            //получаем строку, содержащую перечень событий, по которым срабатывает триггер.
            StringBuilder triggerEvents = new StringBuilder();

            //добавляем обработку вставки строки.
            if (this.AfterInsert)
            {
                if (triggerEvents.Length > 0)
                    triggerEvents.Append(", ");
                triggerEvents.Append("INSERT");
            }

            //добавляем обработку обновления строки.
            if (this.AfterUpdate)
            {
                if (triggerEvents.Length > 0)
                    triggerEvents.Append(", ");
                triggerEvents.Append("UPDATE");
            }

            //добавляем обработку удаления строки.
            if (this.AfterDelete)
            {
                if (triggerEvents.Length > 0)
                    triggerEvents.Append(", ");
                triggerEvents.Append("DELETE");
            }

            if (triggerEvents.Length == 0)
                throw new Exception(string.Format("Отсутствуют события выполнения триггера {0}.", this.Name));

            //устанавливаем действие создания или удаления.
            string metadataAction = isCreate ? "CREATE" : "ALTER";

            //полное тело триггера
            string triggerFullBody = @"
{MetadataAction} TRIGGER [dbo].[{TriggerName}]
ON [dbo].[{TableName}] AFTER {TriggerEvents}
AS
BEGIN

{TriggerBody}

END"
                .ReplaceKey("MetadataAction", metadataAction)
                .ReplaceKey("TriggerName", this.Name)
                .ReplaceKey("TableName", this.Table.Name)
                .ReplaceKey("TriggerEvents", triggerEvents)
                .ReplaceKey("TriggerBody", triggerBody)
                ;

            //возвращаем полное тело триггера.
            return triggerFullBody;
        }

        #endregion


        #region GetCreateQuery

        /// <summary>
        /// Возвращает запрос создания триггера.
        /// </summary>
        internal string GetCreateQuery()
        {
            //получаем полное тело триггера для создания.
            string triggerFullBody = this.GetTriggerBody(true);

            //формируем полный текст запроса создания триггера.
            string query = @"
IF NOT EXISTS(SELECT name FROM sys.triggers WITH(NOLOCK) WHERE name = N'{TriggerNameText}')
BEGIN
    exec sp_executesql N'{TriggerFullBody}'
END"
                .ReplaceKey("TriggerNameText", this.Name.QueryEncode())
                .ReplaceKey("TriggerFullBody", triggerFullBody.QueryEncode())
                ;

            //возвращает запрос создания триггера.
            return query;
        }

        #endregion


        #region GetUpdateQuery

        /// <summary>
        /// Возвращает запрос обновления триггера.
        /// </summary>
        internal string GetUpdateQuery()
        {
            //проверяем существование триггера.
            this.CheckExists();

            //получаем полное тело триггера для обновления.
            string triggerFullBody = this.GetTriggerBody(false);

            //формируем полный текст запроса обновления триггера.
            string query = @"
IF EXISTS(SELECT name FROM sys.triggers WITH(NOLOCK) WHERE name = N'{TriggerNameText}')
BEGIN
    exec sp_executesql N'{TriggerFullBody}'
END"
                .ReplaceKey("TriggerNameText", this.Name.QueryEncode())
                .ReplaceKey("TriggerFullBody", triggerFullBody.QueryEncode())
                ;

            //возвращает запрос обновления триггера.
            return query;
        }

        #endregion


        #region GetRecreateQuery

        /// <summary>
        /// Возвращает запрос пересоздания триггера.
        /// </summary>
        /// <returns></returns>
        internal string GetRecreateQuery()
        {
            //проверяем существование триггера.
            this.CheckExists();

            StringBuilder query = new StringBuilder();

            //получаем запрос удаления существующего триггера.
            string deleteQuery = this.ExistingTrigger.GetDropQuery();
            query.Append(deleteQuery);

            //получаем запрос создания триггера
            string createQuery = this.GetCreateQuery();
            query.Append(createQuery);

            //возвращает результирующий запрос
            return query.ToString();
        }

        #endregion


        /// <summary>
        /// Строковое представление класса DBTrigger.
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
