using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Storage.Metadata.MSSQL
{
    /// <summary>
    /// Представляет информацию об объектах и действиях, требуемых для инициализации объектов таблицы.
    /// </summary>
    public class DBTableRequiredInitAction
    {
        private DBTableRequiredInitAction(DBTable table)
        {
            if (table == null)
                throw new ArgumentNullException("table");
            this.Table = table;
            this.ActionTable = table;
        }

        private DBTable _ActionTable;
        /// <summary>
        /// Таблица, требующая действий инициализации.
        /// </summary>
        private DBTable ActionTable
        {
            get { return _ActionTable; }
            set { _ActionTable = value; }
        }

        private DBTable _Table;
        /// <summary>
        /// Таблица, требующая действий инициализации.
        /// </summary>
        public DBTable Table
        {
            get { return _Table; }
            private set { _Table = value; }
        }


        #region Table

        internal DBTableRequiredInitAction(DBTableRequiredInitActionType requiredAction, DBTable table)
            : this(table)
        {
            if (requiredAction == DBTableRequiredInitActionType.None)
                throw new ArgumentNullException("requiredAction");
            if (!(requiredAction == DBTableRequiredInitActionType.CreateRequired || requiredAction == DBTableRequiredInitActionType.RenameRequired))
                throw new ArgumentException("Требуемым действием при инициализации таблицы может быть создание.");

            this.TableRequiredAction = requiredAction;
        }

        private DBTableRequiredInitActionType _TableRequiredAction = DBTableRequiredInitActionType.None;
        /// <summary>
        /// Требуемое действие инициализации таблицы.
        /// </summary>
        public DBTableRequiredInitActionType TableRequiredAction
        {
            get { return _TableRequiredAction; }
            private set { _TableRequiredAction = value; }
        }

        #endregion


        #region Column

        internal DBTableRequiredInitAction(DBTableRequiredInitActionType requiredAction, DBColumn column, DBTable table)
            : this(table)
        {
            if (requiredAction == DBTableRequiredInitActionType.None)
                throw new ArgumentNullException("requiredAction");
            if (!(requiredAction == DBTableRequiredInitActionType.CreateRequired ||
                requiredAction == DBTableRequiredInitActionType.UpdateRequired ||
                requiredAction == DBTableRequiredInitActionType.RenameRequired))
                throw new ArgumentException("Требуемым действием при инициализации столбца может быть создание, переименование или обновление.");
            if (column == null)
                throw new ArgumentNullException("column");

            this.ColumnRequiredAction = requiredAction;
            this.Column = column;
        }

        private DBColumn _Column;
        /// <summary>
        /// Столбец, требующий действий инициализации.
        /// </summary>
        public DBColumn Column
        {
            get { return _Column; }
            private set { _Column = value; }
        }

        private DBTableRequiredInitActionType _ColumnRequiredAction = DBTableRequiredInitActionType.None;
        /// <summary>
        /// Требуемое действие инициализации столбца.
        /// </summary>
        public DBTableRequiredInitActionType ColumnRequiredAction
        {
            get { return _ColumnRequiredAction; }
            private set { _ColumnRequiredAction = value; }
        }

        #endregion


        #region Index

        internal DBTableRequiredInitAction(DBTableRequiredInitActionType requiredAction, DBIndex index, DBTable table)
            : this(table)
        {
            if (requiredAction == DBTableRequiredInitActionType.None)
                throw new ArgumentNullException("requiredAction");
            if (!(requiredAction == DBTableRequiredInitActionType.CreateRequired ||
                requiredAction == DBTableRequiredInitActionType.RenameRequired ||
                requiredAction == DBTableRequiredInitActionType.RecreateRequired))
                throw new ArgumentException("Требуемым действием при инициализации индекса может быть создание, переименование или пересоздание.");
            if (index == null)
                throw new ArgumentNullException("index");

            this.IndexRequiredAction = requiredAction;
            this.Index = index;
        }

        private DBIndex _Index;
        /// <summary>
        /// Индекс, требующий действий инициализации.
        /// </summary>
        public DBIndex Index
        {
            get { return _Index; }
            private set { _Index = value; }
        }

        private DBTableRequiredInitActionType _IndexRequiredAction = DBTableRequiredInitActionType.None;
        /// <summary>
        /// Требуемое действие инициализации индекса.
        /// </summary>
        public DBTableRequiredInitActionType IndexRequiredAction
        {
            get { return _IndexRequiredAction; }
            private set { _IndexRequiredAction = value; }
        }

        #endregion


        #region Trigger

        internal DBTableRequiredInitAction(DBTableRequiredInitActionType requiredAction, DBTrigger trigger, DBTable table)
            : this(table)
        {
            if (requiredAction == DBTableRequiredInitActionType.None)
                throw new ArgumentNullException("requiredAction");
            if (!(requiredAction == DBTableRequiredInitActionType.CreateRequired ||
                requiredAction == DBTableRequiredInitActionType.RenameRequired ||
                requiredAction == DBTableRequiredInitActionType.RecreateRequired ||
                requiredAction == DBTableRequiredInitActionType.UpdateRequired))
                throw new ArgumentException("Требуемым действием при инициализации триггера может быть создание, переименование или пересоздание.");
            if (trigger == null)
                throw new ArgumentNullException("trigger");

            this.TriggerRequiredAction = requiredAction;
            this.Trigger = trigger;
        }

        private DBTrigger _Trigger;
        /// <summary>
        /// Триггер, требующий действий инициализации.
        /// </summary>
        public DBTrigger Trigger
        {
            get { return _Trigger; }
            private set { _Trigger = value; }
        }

        private DBTableRequiredInitActionType _TriggerRequiredAction = DBTableRequiredInitActionType.None;
        /// <summary>
        /// Требуемое действие инициализации триггера.
        /// </summary>
        public DBTableRequiredInitActionType TriggerRequiredAction
        {
            get { return _TriggerRequiredAction; }
            private set { _TriggerRequiredAction = value; }
        }

        #endregion


        #region IndexToDelete

        internal DBTableRequiredInitAction(DBIndexInfo indexToDelete, DBTable table)
            : this(table)
        {
            if (indexToDelete == null)
                throw new ArgumentNullException("indexToDelete");
            this.IndexToDelete = indexToDelete;
        }

        private DBIndexInfo _IndexToDelete;
        /// <summary>
        /// Индекс, требующий удаление.
        /// </summary>
        public DBIndexInfo IndexToDelete
        {
            get { return _IndexToDelete; }
            private set { _IndexToDelete = value; }
        }

        #endregion


        #region TriggerToDelete

        internal DBTableRequiredInitAction(DBTriggerInfo triggerToDelete, DBTable table)
            : this(table)
        {
            if (triggerToDelete == null)
                throw new ArgumentNullException("triggerToDelete");
            this.TriggerToDelete = triggerToDelete;
        }

        private DBTriggerInfo _TriggerToDelete;
        /// <summary>
        /// Триггер, требующий удаление.
        /// </summary>
        public DBTriggerInfo TriggerToDelete
        {
            get { return _TriggerToDelete; }
            private set { _TriggerToDelete = value; }
        }

        #endregion


        #region FullTextColumn

        internal DBTableRequiredInitAction(DBColumn fullTextColumnToCreateIndex, DBTable table)
            : this(table)
        {
            if (fullTextColumnToCreateIndex == null)
                throw new ArgumentNullException("fullTextColumnToCreateIndex");
            this.FullTextColumnToCreateIndex = fullTextColumnToCreateIndex;
        }

        private DBColumn _FullTextColumnToCreateIndex;
        /// <summary>
        /// Столбец полнотекстового, требующий включение в полнотекстовый индекс таблицы.
        /// </summary>
        public DBColumn FullTextColumnToCreateIndex
        {
            get { return _FullTextColumnToCreateIndex; }
            private set { _FullTextColumnToCreateIndex = value; }
        }

        internal DBTableRequiredInitAction(bool isFullTextKey, DBIndexInfo primaryKeyToDropFullTextIndex, DBTable table)
            : this(table)
        {
            if (!isFullTextKey)
                throw new ArgumentException("Значение должно быть равно true", "isFullTextKey");
            if (primaryKeyToDropFullTextIndex == null)
                throw new ArgumentNullException("primaryKeyToDropFullTextIndex");
            this.PrimaryKeyToDropFullTextIndex = primaryKeyToDropFullTextIndex;
        }

        private DBIndexInfo _PrimaryKeyToDropFullTextIndex;
        /// <summary>
        /// Первичный ключ таблицы, для которого требуется удалить полнотекстовый индекс.
        /// </summary>
        public DBIndexInfo PrimaryKeyToDropFullTextIndex
        {
            get { return _PrimaryKeyToDropFullTextIndex; }
            private set { _PrimaryKeyToDropFullTextIndex = value; }
        }


        #endregion


        private bool __init_Description = false;
        private string _Description;
        /// <summary>
        /// Описание действия, требующего выполнение.
        /// </summary>
        public string Description
        {
            get
            {
                if (!__init_Description)
                {
                    if (this.Table == null || this.ActionTable == null)
                        throw new Exception("Не задана таблица, требующая выполнение действия.");

                    if (this.TableRequiredAction != DBTableRequiredInitActionType.None)
                    {
                        if (this.TableRequiredAction == DBTableRequiredInitActionType.CreateRequired)
                            _Description = string.Format("Требуется создание таблицы {0}.", this.Table.Name);
                        else if (this.TableRequiredAction == DBTableRequiredInitActionType.RenameRequired)
                            _Description = string.Format("Требуется переименование таблицы {0} в {1}.", this.ActionTable.OriginalName, this.ActionTable.Name);
                        else
                            throw new Exception(string.Format("Не удалось сформулировать описание действия {0} для таблицы {1}.", this.TableRequiredAction, this.Table.Name));
                    }
                    else if (this.Column != null)
                    {
                        if (this.ColumnRequiredAction == DBTableRequiredInitActionType.CreateRequired)
                            _Description = string.Format("Требуется создание столбца {0} в таблице {1}.", this.Column.Name, this.Table.Name);
                        else if (this.ColumnRequiredAction == DBTableRequiredInitActionType.RenameRequired)
                        {
                            if (this.Column.ExistingColumn == null)
                                throw new Exception(string.Format("Не задан существующий столбец, соответствующий столбцу схемы {0}, требующий переименование.", this.Column.Name));
                            _Description = string.Format("Требуется переименование столбца {0} в {1} для таблицы {2}.", this.Column.ExistingColumn.Name, this.Column.Name, this.Table.Name);
                        }
                        else if (this.ColumnRequiredAction == DBTableRequiredInitActionType.UpdateRequired)
                            _Description = string.Format("Требуется обновление столбца {0} в таблице {1}.", this.Column.Name, this.Table.Name);
                        else
                            throw new Exception(string.Format("Не удалось сформулировать описание действия {0} для столбца {1} таблицы {2}.", this.ColumnRequiredAction, this.Column.Name, this.Table.Name));
                    }
                    else if (this.Index != null)
                    {
                        if (this.IndexRequiredAction == DBTableRequiredInitActionType.CreateRequired)
                            _Description = string.Format("Требуется создание индекса {0} в таблице {1}.", this.Index.Name, this.Table.Name);
                        else if (this.IndexRequiredAction == DBTableRequiredInitActionType.RenameRequired)
                        {
                            if (this.Index.ExistingIndex == null)
                                throw new Exception(string.Format("Не задан существующий индекс, соответствующий индексу схемы {0}, требующий переименование.", this.Index.Name));
                            _Description = string.Format("Требуется переименование индекса {0} в {1} для таблицы {2}.", this.Index.ExistingIndex.Name, this.Index.Name, this.Table.Name);
                        }
                        else if (this.IndexRequiredAction == DBTableRequiredInitActionType.RecreateRequired)
                            _Description = string.Format("Требуется пересоздание индекса {0} в таблице {1}.", this.Index.Name, this.Table.Name);
                        else
                            throw new Exception(string.Format("Не удалось сформулировать описание действия {0} для индекса {1} таблицы {2}.", this.IndexRequiredAction, this.Index.Name, this.Table.Name));
                    }
                    else if (this.Trigger != null)
                    {
                        if (this.TriggerRequiredAction == DBTableRequiredInitActionType.CreateRequired)
                            _Description = string.Format("Требуется создание триггера {0} в таблице {1}.", this.Trigger.Name, this.Table.Name);
                        else if (this.TriggerRequiredAction == DBTableRequiredInitActionType.RenameRequired)
                        {
                            if (this.Trigger.ExistingTrigger == null)
                                throw new Exception(string.Format("Не задан существующий триггер, соответствующий триггеру схемы {0}, требующий переименование.", this.Trigger.Name));
                            _Description = string.Format("Требуется переименование триггера {0} в {1} для таблицы {2}.", this.Trigger.ExistingTrigger.Name, this.Trigger.Name, this.Table.Name);
                        }
                        else if (this.TriggerRequiredAction == DBTableRequiredInitActionType.RecreateRequired)
                            _Description = string.Format("Требуется пересоздание триггера {0} в таблице {1}.", this.Trigger.Name, this.Table.Name);
                        else if (this.TriggerRequiredAction == DBTableRequiredInitActionType.UpdateRequired)
                            _Description = string.Format("Требуется обновление триггера {0} в таблице {1}.", this.Trigger.Name, this.Table.Name);
                        else
                            throw new Exception(string.Format("Не удалось сформулировать описание действия {0} для триггера {1} таблицы {2}.", this.TriggerRequiredAction, this.Trigger.Name, this.Table.Name));
                    }
                    else if (this.FullTextColumnToCreateIndex != null)
                        _Description = string.Format("Требуется создание полнотекстового индекса для столбца {0} таблицы {1}.", this.FullTextColumnToCreateIndex.Name, this.Table.Name);
                    else if (this.PrimaryKeyToDropFullTextIndex != null)
                        _Description = string.Format("Требуется удаление полнотекстового индекса для первичного ключа {0} таблицы {1}.", this.PrimaryKeyToDropFullTextIndex.Name, this.Table.Name);
                    else if (this.IndexToDelete != null)
                        _Description = string.Format("Требуется удаление индекса {0} таблицы {1}.", this.IndexToDelete.Name, this.IndexToDelete.Table.Name);
                    else if (this.TriggerToDelete != null)
                        _Description = string.Format("Требуется удаление триггера {0} таблицы {1}.", this.TriggerToDelete.Name, this.TriggerToDelete.Table.Name);

                    if (string.IsNullOrEmpty(_Description))
                        throw new Exception(string.Format("Не удалось сформулировать описание требуемого действия инициализации таблицы {0}.", this.Table.Name));

                    __init_Description = true;
                }
                return _Description;
            }
        }

        /// <summary>
        /// Строковое представление экземпляра DBTableRequiredAction.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(this.Description))
                return this.Description;
            return base.ToString();
        }
    }

    /// <summary>
    /// Тип требуемого действия инициализации таблицы или объекта, принадлежащего таблице.
    /// </summary>
    public enum DBTableRequiredInitActionType
    {
        /// <summary>
        /// Действие не требуется.
        /// </summary>
        None = 0,

        /// <summary>
        /// Требуется создание.
        /// </summary>
        CreateRequired = 1,

        /// <summary>
        /// Требуется обновление.
        /// </summary>
        UpdateRequired = 2,

        /// <summary>
        /// Требуется пересоздание.
        /// </summary>
        RecreateRequired = 3,

        /// <summary>
        /// Требуется переименование.
        /// </summary>
        RenameRequired = 4
    }
}
