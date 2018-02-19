using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Storage.Metadata.MSSQL
{
    /// <summary>
    /// Представляет класс для работы со столбцом таблицы.
    /// </summary>
    public class DBColumn
    {
        /// <summary>
        /// Создает экземпляр DBColumn.
        /// </summary>
        /// <param name="schema">Схема столбца.</param>
        /// <param name="table">Таблица.</param>
        internal DBColumn(DBColumnSchema schema, DBTable table)
        {
            if (schema == null)
                throw new ArgumentNullException("schema");
            if (table == null)
                throw new ArgumentNullException("table");

            this.Schema = schema;
            this.Table = table;
        }

        private DBColumnSchema _Schema;
        /// <summary>
        /// Схема столбца в таблице.
        /// </summary>
        public DBColumnSchema Schema
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


        #region Name

        /// <summary>
        /// Название столбца.
        /// </summary>
        public string Name
        {
            get { return this.Schema.Name; }
        }

        /// <summary>
        /// Название столбца в нижнем регистре.
        /// </summary>
        internal string NameLow
        {
            get { return this.Schema.NameLow; }
        }

        private bool __init_NameChanged = false;
        private bool _NameChanged;
        /// <summary>
        /// Возвращает true, если название столбца было изменено в контексте выполнения кода.
        /// </summary>
        internal bool NameChanged
        {
            get
            {
                if (!__init_NameChanged)
                {
                    _NameChanged = this.Exists && this.ExistingColumn.NameLow != this.NameLow;
                    __init_NameChanged = true;
                }
                return _NameChanged;
            }
        }

        /// <summary>
        /// Сравнивает названия двух столбцов без учета регистра.
        /// </summary>
        /// <param name="columnToCompare">Столбец для сравнения.</param>
        /// <returns></returns>
        protected bool NameEquals(DBColumn columnToCompare)
        {
            bool result = false;
            if (columnToCompare != null && !string.IsNullOrEmpty(this.NameLow) && !string.IsNullOrEmpty(columnToCompare.NameLow))
                result = this.NameLow == columnToCompare.NameLow;
            return result;
        }

        /// <summary>
        /// Проверяет, что столбец не был переименован в контексте выполнения кода, в ином случае - генерирует исключение.
        /// </summary>
        internal void CheckRenameNotRequired()
        {
            if (this.NameChanged)
                throw new Exception(string.Format("Операция недоступна, поскольку столбец {0} должен быть переименован в {1}.", this.ExistingColumn.Name, this.Name));
        }

        /// <summary>
        /// Проверяет, что столбец был переименован в контексте выполнения кода, в ином случае - генерирует исключение.
        /// </summary>
        internal void CheckRenameRequired()
        {
            if (!this.NameChanged)
                throw new Exception(string.Format("Операция недоступна, поскольку столбец {0} не был переименован.", this.Name));
        }

        #endregion


        #region DefinitionProperties

        /// <summary>
        /// Тип столбца.
        /// </summary>
        public SqlDbType Type
        {
            get { return this.Schema.Type; }
        }

        /// <summary>
        /// Размер столбца.
        /// </summary>
        public int Size
        {
            get { return this.Schema.Size; }
        }

        /// <summary>
        /// Возвращает true, если столбец поддерживает значение NULL.
        /// </summary>
        public bool IsNullable
        {
            get { return this.Schema.IsNullable; }
        }

        /// <summary>
        /// Строка определения столбца таблицы, содержащая название, тип, размер и возможность установки значений NULL в столбце.
        /// </summary>
        public string Definition
        {
            get { return this.Schema.Definition; }
        }

        private bool __init_InitialDefinition = false;
        private string _InitialDefinition;
        /// <summary>
        /// Строка определения столбца таблицы, используемая при добавлении столбца таблицу.
        /// </summary>
        internal string InitialDefinition
        {
            get
            {
                if (!__init_InitialDefinition)
                {
                    if (!this.IsIdentity)
                        _InitialDefinition = this.Definition;
                    else
                        _InitialDefinition = string.Format("{0} IDENTITY(1,1) NOT FOR REPLICATION", this.Definition);
                    __init_InitialDefinition = true;
                }
                return _InitialDefinition;
            }
        }

        private bool __init_IsIdentity = false;
        private bool _IsIdentity;
        /// <summary>
        /// Возвращает true, если столбец является идентификационным столбцом таблицы.
        /// </summary>
        public bool IsIdentity
        {
            get
            {
                if (!__init_IsIdentity)
                {
                    _IsIdentity = this.NameEquals(this.Table.IdentityColumn);
                    __init_IsIdentity = true;
                }
                return _IsIdentity;
            }
        }

        private bool __init_IsInitial = false;
        private bool _IsInitial;
        /// <summary>
        /// Возвращает true, если столбец создается в таблице вместе с созданием таблицы.
        /// </summary>
        public bool IsInitial
        {
            get
            {
                if (!__init_IsInitial)
                {
                    _IsInitial = this.NameEquals(this.Table.InitialColumn);
                    __init_IsInitial = true;
                }
                return _IsInitial;
            }
        }

        private bool __init_IsSystem = false;
        private bool _IsSystem;
        /// <summary>
        /// Столбец является системным и создается вместе с инициализацией таблицы.
        /// </summary>
        public bool IsSystem
        {
            get
            {
                if (!__init_IsSystem)
                {
                    _IsSystem = this.Table.Schema.IsSystemColumn(this.Name);
                    __init_IsSystem = true;
                }
                return _IsSystem;
            }
        }

        #endregion


        #region ExistingColumn

        private bool __init_ExistingColumn = false;
        private DBColumnInfo _ExistingColumn;
        /// <summary>
        /// Существующий столбец таблицы.
        /// </summary>
        public DBColumnInfo ExistingColumn
        {
            get
            {
                if (!__init_ExistingColumn)
                {
                    _ExistingColumn = null;

                    //получаем существующий столбец.
                    if (!this.Table.RenameRequired)
                    {
                        if (this.Table.ExistingTable != null)
                            _ExistingColumn = this.GetExistingColumn(this.Table.ExistingTable);
                    }
                    else
                    {
                        if (this.Table.OriginalExistingTable != null)
                            _ExistingColumn = this.GetExistingColumn(this.Table.OriginalExistingTable);
                    }
                    __init_ExistingColumn = true;
                }
                return _ExistingColumn;
            }
        }

        /// <summary>
        /// Возвращает сущуствующий столбец таблицы.
        /// </summary>
        /// <param name="tableInfo">Сущуствующая таблица.</param>
        /// <returns></returns>
        private DBColumnInfo GetExistingColumn(DBTableInfo tableInfo)
        {
            if (tableInfo == null)
                throw new ArgumentNullException("tableInfo");

            DBColumnInfo existingColumn = null;

            //если столбец является инкрементом, получаем существующий столбец с инкрементом таблицы
            if (this.IsIdentity && tableInfo.IdentityColumn != null)
                existingColumn = tableInfo.IdentityColumn;
            //в ином случаем получаем столбец по названию.
            else
                existingColumn = tableInfo.GetColumn(this.Name);

            return existingColumn;
        }

        private bool __init_Exists = false;
        private bool _Exists;
        /// <summary>
        /// Возвращает true, если столбец присутствует в таблице.
        /// </summary>
        public bool Exists
        {
            get
            {
                if (!__init_Exists)
                {
                    _Exists = this.ExistingColumn != null;
                    __init_Exists = true;
                }
                return _Exists;
            }
        }

        /// <summary>
        /// Проверяет существование столбца в таблице. Генерирует исключение в случае отсутствия столбца в таблице.
        /// </summary>
        protected internal void CheckExists()
        {
            if (!this.Exists)
                throw new Exception(string.Format("Операция доступна только для существующего столбца [{0}] в таблице {1}.", this.Name, this.Table.Name));
        }

        /// <summary>
        /// Проверяет отсутствие столбца в таблице. Генерирует исключение в случае наличия столбца в таблице.
        /// </summary>
        protected internal void CheckNotExists()
        {
            if (this.Exists)
                throw new Exception(string.Format("Операция доступна только для несозданного столбца [{0}] в таблице {1}.", this.Name, this.Table.Name));
        }

        private bool __init_ExistingNonSchemaColumn = false;
        private DBColumnInfo _ExistingNonSchemaColumn;
        /// <summary>
        /// Столбец таблицы, не являющийся сопоставленным существующим столбцом, но имеющий название совпадающее с названием столбца схемы.
        /// </summary>
        private DBColumnInfo ExistingNonSchemaColumn
        {
            get
            {
                if (!__init_ExistingNonSchemaColumn)
                {
                    _ExistingNonSchemaColumn = null;

                    //получаем существующий столбец.
                    if (this.NameChanged)
                    {
                        if (!this.Table.RenameRequired)
                        {
                            if (this.Table.ExistingTable != null)
                                _ExistingNonSchemaColumn = this.Table.ExistingTable.GetColumn(this.Name);
                        }
                        else
                        {
                            if (this.Table.OriginalExistingTable != null)
                                _ExistingNonSchemaColumn = this.Table.OriginalExistingTable.GetColumn(this.Name);
                        }
                    }

                    __init_ExistingNonSchemaColumn = true;
                }
                return _ExistingNonSchemaColumn;
            }
        }

        #endregion


        #region Create

        /// <summary>
        /// Возвращает запрос инициализации столбца в таблице: создает столбец в таблице, если он не существует.
        /// </summary>
        internal string GetInitQuery()
        {
            //проверяем, что переименование столбца не требуется.
            this.CheckRenameNotRequired();

            string query = null;

            //формируем запрос установки пустого значения для столбцов, 
            //которые были NULL и становятся NOT NULL либо создаются NOT NULL в таблице с существующими строками.
            string columnQuery = null;
            if (!this.IsNullable && !this.IsIdentity && this.Schema.DefaultEmptyValue != null)
            {
                string updateNotNullValuesQuery = this.GetUpdateDefaultEmptyValuesQuery(this.Name);
                if (!string.IsNullOrEmpty(updateNotNullValuesQuery))
                    columnQuery = @"
    --при наличии строк в таблице для столбца без поддержки NULL-значений сначала добавляем столбец с поддержкой NULL-значений, а затем подифицируем его.
    IF(EXISTS(
        SELECT 1 FROM [{TableName}] WITH(NOLOCK)
        HAVING(COUNT([{InitialColumnName}]) > 0)
    ))
    BEGIN
        --добавляем столбец с поддержкой NULL-значений.
        ALTER TABLE [{TableName}] ADD {ColumnDefinitionNullable}
        --устанавливаем пустые значения по умолчанию в столбец.
        exec sp_executesql N'{UpdateNotNullValuesQuery}'
        --отключаем поддержку NULL-значений для столбца.
        ALTER TABLE [{TableName}] ALTER COLUMN {ColumnDefinition}
    END
    ELSE
        ALTER TABLE [{TableName}] ADD {ColumnDefinition}"
                        .ReplaceKey("TableName", this.Table.Name)
                        .ReplaceKey("InitialColumnName", this.Table.InitialColumn.Name)
                        .ReplaceKey("ColumnName", this.Name)
                        .ReplaceKey("ColumnDefinitionNullable", this.Schema.DefinitionNullable)
                        .ReplaceKey("ColumnDefinition", this.Schema.Definition)
                        .ReplaceKey("UpdateNotNullValuesQuery", updateNotNullValuesQuery.QueryEncode())
                        ;
            }

            //формируем запрос добавления столбца без поддержки NULL-значений или для инкрементного столбца.
            if (string.IsNullOrEmpty(columnQuery))
                columnQuery = @"
    ALTER TABLE [{TableName}] ADD {ColumnInitialDefinition}"
                    .ReplaceKey("TableName", this.Table.Name)
                    .ReplaceKey("ColumnInitialDefinition", this.InitialDefinition)
                    ;

            query = @"
IF(NOT EXISTS(
    SELECT sys.columns.name FROM sys.columns WITH(NOLOCK)
    INNER JOIN sys.tables WITH(NOLOCK)
    ON sys.tables.name = N'{TableNameText}'
    AND sys.tables.object_id = sys.columns.object_id
    AND sys.columns.name = N'{ColumnNameText}'
))
BEGIN{ColumnQuery}
END"
                .ReplaceKey("TableNameText", this.Table.Name.QueryEncode())
                .ReplaceKey("ColumnNameText", this.Name.QueryEncode())
                .ReplaceKey("ColumnQuery", columnQuery)
                ;

            return query;
        }

        /// <summary>
        /// Добавляет столбец к таблице.
        /// </summary>
        internal void Create()
        {
            //проверяем существование таблицы
            this.Table.CheckExists();

            //проверяем, что название таблицы не изменилось и переименование перед операцией не требуется.
            this.Table.CheckRenameNotRequired();

            //проверяем, что столбец не существует.
            this.CheckNotExists();

            //проверяем, что схема автоинкремента не изменилась
            if (this.IsIdentity)
                this.Table.CheckIdentityColumnUnmodified();

            //добавляем физически столбец в таблицу.
            string query = this.GetInitQuery();
            this.Table.DataAdapter.ExecuteQuery(query);

            //сбрасываем флаги инициализации столбцов.
            this.Table.ResetExistingColumns();
        }

        #endregion


        #region Update

        /// <summary>
        /// Проверяет необходимость выполнения обновления схемы стобца. Генерирует исключение в случае отсутствия необходимости обновления схемы столбца таблицы.
        /// </summary>
        protected internal void CheckUpdateRequired()
        {
            if (!this.UpdateRequired)
                throw new Exception(string.Format("Операция доступна только при наличии изменений в схеме столбца [{0}] таблицы {1}.", this.Name, this.Table.Name));
        }

        private bool __init_UpdateRequired = false;
        private bool _UpdateRequired;
        /// <summary>
        /// Возвращает true, если параметры столбца изменились по сравнению с существующим столбцом таблицы, и для столбца требуется обновление операцией ALTER COLUMN.
        /// </summary>
        public bool UpdateRequired
        {
            get
            {
                if (!__init_UpdateRequired)
                {
                    if (this.Exists)
                    {
                        _UpdateRequired = this.Type != this.ExistingColumn.Type ||
                            this.Size != 0 && this.Size != this.ExistingColumn.Size ||
                            this.IsNullable != this.ExistingColumn.IsNullable;
                    }
                    __init_UpdateRequired = true;
                }
                return _UpdateRequired;
            }
        }

        /// <summary>
        /// Сбрасывает флаг инициализации свойства ExistingColumn.
        /// </summary>
        internal void ResetExistingColumn()
        {
            this.__init_ExistingColumn = false;
            this.__init_Exists = false;
            this.__init_NameChanged = false;
            this.__init_ExistingNonSchemaColumn = false;
            this.__init_IsIdentity = false;
            this.__init_UpdateRequired = false;
            this.__init_DependendIndexesRecreateRequired = false;
        }

        /// <summary>
        /// Возвращает запрос обновления пустого значения данного столбца для строк с установленным NULL-значением.
        /// </summary>
        /// <param name="columnName">Название столбца.</param>
        /// <returns></returns>
        private string GetUpdateDefaultEmptyValuesQuery(string columnName)
        {
            if (string.IsNullOrEmpty(columnName))
                throw new ArgumentNullException("columnName", "Не передано название столбца.");

            //формируем запрос установки пустого значения для столбцов, 
            //которые были NULL и становятся NOT NULL либо создаются NOT NULL в таблице с существующими строками.
            string updateNotNullValuesQuery = null;
            if (this.Schema.DefaultEmptyValue != null)
                updateNotNullValuesQuery = @"
    {TriggersDisableQuery}
    UPDATE [{TableName}]
    SET [{ColumnName}] = {DefaultEmptyValue}
    WHERE [{ColumnName}] IS NULL
    {TriggersEnableQuery}
"
                    .ReplaceKey("TableName", this.Table.Name)
                    .ReplaceKey("ColumnName", columnName)
                    .ReplaceKey("DefaultEmptyValue", this.Schema.DefaultEmptyValue)
                    .ReplaceKey("TriggersDisableQuery", this.SchemaAdapter.GetTriggersDisableQuery())
                    .ReplaceKey("TriggersEnableQuery", this.SchemaAdapter.GetTriggersEnableQuery())
                    ;

            //возвращаем запрос.
            return updateNotNullValuesQuery;
        }

        /// <summary>
        /// Возвращает запрос обновления существующего столбца в таблице.
        /// </summary>
        /// <returns></returns>
        internal string GetUpdateQuery()
        {
            //проверяем, что переименование столбца не требуется.
            this.CheckRenameNotRequired();

            //признак изменения свойства AllowNulls на false у столбца таблицы.
            bool setNotNullable = !this.IsNullable && this.ExistingColumn.IsNullable;

            //формируем запрос установки пустого значения для столбцов, 
            //которые были NULL и становятся NOT NULL либо создаются NOT NULL в таблице с сущетсвующими строками.
            string updateNotNullValuesQuery = null;
            if (setNotNullable && this.Schema.DefaultEmptyValue != null)
            {
                updateNotNullValuesQuery = this.GetUpdateDefaultEmptyValuesQuery(this.ExistingColumn.Name);
                if (!string.IsNullOrEmpty(updateNotNullValuesQuery))
                    updateNotNullValuesQuery = @"
IF(EXISTS(
    SELECT [{ColumnName}]
    FROM [{TableName}] WITH(NOLOCK)
    WHERE [{ColumnName}] IS NULL))
BEGIN
    {UpdateNotNullValuesQuery}
END
"
                        .ReplaceKey("TableName", this.Table.Name)
                        .ReplaceKey("ColumnName", this.ExistingColumn.Name)
                        .ReplaceKey("UpdateNotNullValuesQuery", updateNotNullValuesQuery)
                        ;
            }

            //формируем запрос изменения столбца.
            string query = @"
IF(EXISTS(
    SELECT sys.columns.name FROM sys.columns WITH(NOLOCK)
    INNER JOIN sys.tables WITH(NOLOCK)
    ON sys.tables.name = N'{TableNameText}'
    AND sys.tables.object_id = sys.columns.object_id
    AND sys.columns.name = N'{ColumnNameText}'
))
BEGIN
    {UpdateNotNullIntValues}ALTER TABLE [{TableName}] ALTER COLUMN {ColumnDefinition}
END"
                .ReplaceKey("TableNameText", this.Table.Name.QueryEncode())
                .ReplaceKey("ColumnNameText", this.Name.QueryEncode())
                .ReplaceKey("UpdateNotNullIntValues", updateNotNullValuesQuery)
                .ReplaceKey("TableName", this.Table.Name)
                .ReplaceKey("ColumnDefinition", this.Definition)
                ;

            return query;
        }

        /// <summary>
        /// Обновляет схему существующего столбца в таблице при необходимости.
        /// </summary>
        internal void Update()
        {
            //проверяем существование таблицы
            this.Table.CheckExists();

            //проверяем, что столбец существует.
            this.CheckExists();

            //проверяем необходимость обновления схемы столбца.
            this.CheckUpdateRequired();

            //проверяем, что название таблицы не изменилось и переименование перед операцией не требуется.
            this.Table.CheckRenameNotRequired();

            //проверяем, что название таблицы не изменилось и переименование перед операцией не требуется.
            //проверка необходима, т.к. в ходе операции может потребоваться пересоздание индексов, названия которых формируются из префикса таблицы.
            this.Table.CheckRenameNotRequired();

            //проверяем, что столбец не был переименован.
            this.CheckRenameNotRequired();

            //проверяем, что схема автоинкремента не изменилась
            if (this.IsIdentity || this.ExistingColumn.IsIdentity)
                this.Table.CheckIdentityColumnUnmodified();

            //удаляем индексы, в которые был включен данный столбец, если это требуется.
            StringBuilder stDropIndexesQuery = new StringBuilder();
            StringBuilder stRecreateIndexesQuery = new StringBuilder();
            List<DBIndexInfo> changedIndexes = new List<DBIndexInfo>();
            if (this.DependendIndexesRecreateRequired && this.ExistingColumn.DependendIndexes.Count > 0)
            {
                foreach (DBIndexInfo indexInfo in this.ExistingColumn.DependendIndexes)
                {
                    stDropIndexesQuery.Append(indexInfo.GetDropQuery());
                    if (indexInfo.SchemaIndex != null)
                        stRecreateIndexesQuery.Append(indexInfo.SchemaIndex.GetInitQuery());
                    changedIndexes.Add(indexInfo);
                }
            }

            //удаляем, требующие пересоздания индексы, включающие в себя изменяемый столбец.
            if (stDropIndexesQuery.Length > 0)
                this.Table.DataAdapter.ExecuteQuery(stDropIndexesQuery.ToString());

            //выполняем изменения схемы столбца
            string query = this.GetUpdateQuery();
            this.Table.DataAdapter.ExecuteQuery(query);

            //пересоздаем удаленные индексы (с новыми названиями, если названия изменились).
            if (stRecreateIndexesQuery.Length > 0)
                this.Table.DataAdapter.ExecuteQuery(stRecreateIndexesQuery.ToString());

            //сбрасываем параметры существующих столбцов
            this.Table.ResetExistingColumns();

            //сбрасываем параметры пересозданных индексов.
            if (changedIndexes.Count > 0)
                this.Table.ResetExistingIndexes();
        }

        #endregion


        #region Ensure

        /// <summary>
        /// Создает столбец в таблице или обновляет параметры столбца при необходимости.
        /// </summary>
        internal void Ensure()
        {
            if (!this.Exists)
                this.Create();
            else if (this.UpdateRequired)
                this.Update();
        }

        #endregion


        #region Rename

        /// <summary>
        /// Переименовывает столбец в соответствии с новым именем столбца в схеме таблицы.
        /// </summary>
        internal void Rename()
        {
            //проверяем, что название таблицы не изменилось и переименование перед операцией не требуется.
            this.Table.CheckRenameNotRequired();

            //проверяем, что требуется переименование столбца.
            this.CheckRenameRequired();

            //проверяем существование таблицы
            this.Table.CheckExists();

            //проверяем, что столбец существует.
            this.CheckExists();

            //выполняем переименование.
            string renameQuery = this.GetRenameQuery();
            this.Table.DataAdapter.ExecuteQuery(renameQuery);

            //сбрасываем параметры существующих столбцов
            this.Table.ResetExistingColumns();
        }

        /// <summary>
        /// Возвращает запрос переименования столбца.
        /// </summary>
        /// <returns></returns>
        internal string GetRenameQuery()
        {
            StringBuilder stQuery = new StringBuilder();

            //добавляем запрос удаления столбца, не принадлежащего схеме и с совпадающим названием.
            if (this.ExistingNonSchemaColumn != null)
                stQuery.Append(this.ExistingNonSchemaColumn.GetDropQuery());

            stQuery.Append(@"
exec sp_rename N'[dbo].[{TableNameText}].[{OriginalNameText}]', N'{NameText}', N'COLUMN'"
                .ReplaceKey("TableNameText", this.ExistingColumn.Table.Name.QueryEncode())
                .ReplaceKey("OriginalNameText", this.ExistingColumn.Name.QueryEncode())
                .ReplaceKey("NameText", this.Name.QueryEncode())
                );

            string query = stQuery.ToString();
            return query;
        }

        #endregion


        #region DependendIndexes

        private bool __init_DependendIndexesRecreateRequired = false;
        private bool _DependendIndexesRecreateRequired;
        /// <summary>
        /// Возвращает true, если параметры столбца изменились по сравнению с существующим столбцом таблицы таким образом, 
        /// что требуется пересоздание индексов, включающих в себя данный столбец.
        /// </summary>
        internal bool DependendIndexesRecreateRequired
        {
            get
            {
                if (!__init_DependendIndexesRecreateRequired)
                {
                    if (this.Exists)
                    {
                        _DependendIndexesRecreateRequired =
                            this.UpdateRequired && (
                                this.Type != this.ExistingColumn.Type ||
                                this.Size != 0 && this.Size != this.ExistingColumn.Size &&
                                (this.GetComparableSize(this.Size) < this.GetComparableSize(this.ExistingColumn.Size) || this.Size == -1) ||
                                !this.IsNullable && this.ExistingColumn.IsNullable
                            );
                    }
                    __init_DependendIndexesRecreateRequired = true;
                }
                return _DependendIndexesRecreateRequired;
            }
        }

        private int GetComparableSize(int size)
        {
            if (size == -1)
                return int.MaxValue;
            return size;
        }

        private bool __init_DependendIndexes = false;
        private DBCollection<DBIndex> _DependendIndexes;
        /// <summary>
        /// Коллекция индексов, в которых присутствует данный столбец.
        /// </summary>
        public DBCollection<DBIndex> DependendIndexes
        {
            get
            {
                if (!__init_DependendIndexes)
                {
                    _DependendIndexes = new DBCollection<DBIndex>();
                    foreach (DBIndex index in this.Table.Indexes)
                    {
                        if (index.ContainsColumn(this.Name))
                            _DependendIndexes.Add(index);
                    }
                    __init_DependendIndexes = true;
                }
                return _DependendIndexes;
            }
        }

        #endregion


        /// <summary>
        /// Строковое представление экземпляра класса DBColumn.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(this.Definition))
                return this.Definition;
            return base.ToString();
        }
    }
}
