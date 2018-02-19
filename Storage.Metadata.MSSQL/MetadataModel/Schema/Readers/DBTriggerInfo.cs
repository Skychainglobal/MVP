using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Storage.Metadata.MSSQL
{
    /// <summary>
    /// Представляет класс, содержащий параметры существующего триггера таблицы.
    /// </summary>
    public class DBTriggerInfo
    {
        /// <summary>
        /// Создает экземпляр DBTriggerInfo.
        /// </summary>
        /// <param name="data">Метаданные триггера.</param>
        /// <param name="table">Таблица, к которой относится триггер.</param>
        internal DBTriggerInfo(DataRow data, DBTableInfo table)
        {
            if (data == null)
                throw new ArgumentNullException("data");
            if (table == null)
                throw new ArgumentNullException("table");

            this.Data = data;
            this.Table = table;
        }

        private DataRow _Data;
        /// <summary>
        /// Метаданые триггера.
        /// </summary>
        private DataRow Data
        {
            get { return _Data; }
            set { _Data = value; }
        }

        private DBTableInfo _Table;
        /// <summary>
        /// Таблица в которой содержится триггер.
        /// </summary>
        public DBTableInfo Table
        {
            get { return _Table; }
            private set { _Table = value; }
        }

        private bool __init_Reader = false;
        private DataRowReader _Reader;
        private DataRowReader Reader
        {
            get
            {
                if (!__init_Reader)
                {
                    _Reader = new DataRowReader(this.Data);
                    __init_Reader = true;
                }
                return _Reader;
            }
        }


        private bool __init_Name = false;
        private string _Name;
        /// <summary>
        /// Название триггера.
        /// </summary>
        public string Name
        {
            get
            {
                if (!__init_Name)
                {
                    _Name = this.Reader.GetStringValue("Name");
                    if (string.IsNullOrEmpty(_Name))
                        throw new Exception("Не удалось получить название триггера.");
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

        private bool __init_IsCustom = false;
        private bool _IsCustom;
        /// <summary>
        /// Возвращает true, если триггер является кастомизированным и не должен быть удален при обновлении таблиц, даже при отсустствии триггера в схеме таблицы.
        /// </summary>
        public bool IsCustom
        {
            get
            {
                if (!__init_IsCustom)
                {
                    _IsCustom = this.NameLow.StartsWith("custom_");
                    __init_IsCustom = true;
                }
                return _IsCustom;
            }
        }


        private bool __init_SchemaTrigger = false;
        private DBTrigger _SchemaTrigger;
        /// <summary>
        /// Триггер схемы таблицы, соответствующий триггеру, присутствующему в таблице.
        /// </summary>
        internal DBTrigger SchemaTrigger
        {
            get
            {
                if (!__init_SchemaTrigger)
                {
                    _SchemaTrigger = this.Table.SchemaTable.GetExistingSchemaTrigger(this.Name);
                    __init_SchemaTrigger = true;
                }
                return _SchemaTrigger;
            }
        }


        private bool __init_Disabled = false;
        private bool _Disabled;
        /// <summary>
        /// Возвращает true, если триггер отключен.
        /// </summary>
        public bool Disabled
        {
            get
            {
                if (!__init_Disabled)
                {
                    _Disabled = this.Reader.GetBooleanValue("Disabled");
                    __init_Disabled = true;
                }
                return _Disabled;
            }
        }


        private bool __init_InitializingEvents = false;
        private Dictionary<int, bool> _InitializingEvents;
        /// <summary>
        /// Инициализационный словарь событий триггера.
        /// </summary>
        internal Dictionary<int, bool> InitializingEvents
        {
            get
            {
                if (!__init_InitializingEvents)
                {
                    _InitializingEvents = new Dictionary<int, bool>();
                    __init_InitializingEvents = true;
                }
                return _InitializingEvents;
            }
        }


        private bool __init_AfterInsert = false;
        private bool _AfterInsert;
        /// <summary>
        /// Возвращает true, если триггер запускается после вставки строки в таблицу.
        /// </summary>
        public bool AfterInsert
        {
            get
            {
                if (!__init_AfterInsert)
                {
                    _AfterInsert = this.InitializingEvents.ContainsKey(1);
                    __init_AfterInsert = true;
                }
                return _AfterInsert;
            }
        }


        private bool __init_AfterUpdate = false;
        private bool _AfterUpdate;
        /// <summary>
        /// Возвращает true, если триггер запускается после обновления строки таблицы.
        /// </summary>
        public bool AfterUpdate
        {
            get
            {
                if (!__init_AfterUpdate)
                {
                    _AfterUpdate = this.InitializingEvents.ContainsKey(2);
                    __init_AfterUpdate = true;
                }
                return _AfterUpdate;
            }
        }


        private bool __init_AfterDelete = false;
        private bool _AfterDelete;
        /// <summary>
        /// Возвращает true, если триггер запускается после удаления строки таблицы.
        /// </summary>
        public bool AfterDelete
        {
            get
            {
                if (!__init_AfterDelete)
                {
                    _AfterDelete = this.InitializingEvents.ContainsKey(3);
                    __init_AfterDelete = true;
                }
                return _AfterDelete;
            }
        }

        /// <summary>
        /// Возвращает текст запроса удаления триггера.
        /// </summary>
        /// <returns></returns>
        internal string GetDropQuery()
        {
            //формируем запрос на основе существующего триггера.
            string query = @"
IF EXISTS(SELECT name FROM sys.triggers WITH(NOLOCK) WHERE name = N'{TriggerNameText}')
BEGIN
    DROP TRIGGER [dbo].[{TriggerName}]
END"
                .ReplaceKey("TriggerNameText", this.Name.QueryEncode())
                .ReplaceKey("TriggerName", this.Name)
                ;
            return query;
        }


        private bool __init_Body = false;
        private string _Body;
        /// <summary>
        /// Полное функциональное тело триггера.
        /// </summary>
        public string Body
        {
            get
            {
                if (!__init_Body)
                {
                    //устанавливаем пустое значение по умолчанию.
                    _Body = null;

                    //получаем таблицу строк кода тела триггера.
                    string query = string.Format("exec sp_HelpText '[dbo].[{0}]'", this.Name.QueryEncode());
                    DataTable dtBody = this.Table.SchemaTable.DataAdapter.GetDataTable(query);
                    
                    //формируем тело триггера.
                    StringBuilder bodyBuilder = new StringBuilder();
                    foreach (DataRow codeRow in dtBody.Rows)
                    {
                        //получаем строку кода
                        string codeLine = DataRowReader.GetStringValue(codeRow, "Text");
                        if (codeLine == null)
                            codeLine = string.Empty;

                        //добавляем строку кода
                        bodyBuilder.Append(codeLine);
                    }

                    //получаем тело триггера.
                    if (bodyBuilder.Length > 0)
                        _Body = bodyBuilder.ToString();

                    __init_Body = true;
                }
                return _Body;
            }
        }

        /// <summary>
        /// Возвращает строковое представление экземпляра DBTriggerInfo.
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
