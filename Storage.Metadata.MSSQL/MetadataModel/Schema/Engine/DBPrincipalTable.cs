using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storage.Metadata.MSSQL
{
    /// <summary>
    /// Представляет класс для работы с таблицей данных.
    /// </summary>
    public class DBPrincipalTable : DBTable
    {
        /// <summary>
        /// Создает экземпляр DBPrincipalTable.
        /// </summary>
        /// <param name="partition">Раздел данных, к которому относится таблица.</param>
        internal DBPrincipalTable(DBTablePartition partition)
            : base(partition.SchemaAdapter.TableSchema)
        {
            this.Partition = partition;
        }

        /// <summary>
        /// Схема таблицы исходных данных.
        /// </summary>
        internal DBPrincipalTableSchema PrincipalSchema
        {
            get { return this.SchemaAdapter.TableSchema; }
        }

        private DBTablePartition _Partition;

        public DBTablePartition Partition
        {
            get { return _Partition; }
            set { _Partition = value; }
        }

        /// <summary>
        /// Контекст подключений к базам данных.
        /// </summary>
        internal DBConnectionContext ConnectionContext
        {
            get { return this.SchemaAdapter.ConnectionContext; }
        }

        /// <summary>
        /// Инициализирует подключение к базе данных, в которой располагается таблица основных данных.
        /// </summary>
        /// <returns></returns>
        public override DBConnection Connection
        {
            get { return this.SchemaAdapter.PrimaryDatabaseConnection; }
        }

        #region Name

        /// <summary>
        /// Базовое название таблицы, на основе которого формируется название данной таблицы.
        /// </summary>
        internal override string BaseName
        {
            get { return this.Partition.TableName; }
        }

        /// <summary>
        /// Базовое название таблицы до переименования, на основе которого формируется название данной таблицы до переименования.
        /// </summary>
        internal override string BaseOriginalName
        {
            get { return this.Partition.OriginalTableName; }
        }

        /// <summary>
        /// Базоый префикс таблицы, на основе которого формируется префикс данной таблицы.
        /// </summary>
        internal override string BasePrefix
        {
            get { return this.Partition.TablePrefix; }
        }

        /// <summary>
        /// Базовый префикс таблицы до переименования, на основе которого формируется префикс данной таблицы до переименования.
        /// </summary>
        internal override string BaseOriginalPrefix
        {
            get { return this.Partition.OriginalTablePrefix; }
        }

        /// <summary>
        /// Сбрасывает флаги инициализации названия таблицы и зависимых от него свойств.
        /// </summary>
        internal override void ResetName()
        {
            //вызываем базовый сброс свойств.
            base.ResetName();
        }

        /// <summary>
        /// Сбрасывает флаги инициализации прифиска таблицы и зависимых от него свойств.
        /// </summary>
        internal override void ResetPrefix()
        {
            //вызываем базовый сброс свойств.
            base.ResetPrefix();
        }

        /// <summary>
        /// Возвращает имя объекта базы данных относительно исходного имени основной таблицы.
        /// Используется для формирования имен таблицы, индексов для основной таблицы.
        /// Возвращаемое значение зависит от поддержки схемой таблицы архивирования данных.
        /// </summary>
        /// <param name="sourceName">Исходное имя объекта.</param>
        internal override string GetRelativeName(string sourceName)
        {
            if (string.IsNullOrEmpty(sourceName))
                throw new ArgumentNullException("sourceName");

            if (!String.IsNullOrEmpty(this.Partition.Name))
                return this.Partition.Name;
            else
                return sourceName;
        }

        #endregion


        #region Init

        /// <summary>
        /// Содает таблицу если она не существует, добавляет столбцы, изменяет параметры столбцов, переименовывает столбцы, создает индексы, 
        /// пересоздает индексы при изменении параметров индексов, переименовывает индексы, 
        /// в случае если данные операции требуются в соответствии со схемой таблицы.
        /// </summary>
        public void Init()
        {
            this.InitInternal(true, true);
        }

        /// <summary>
        /// Создает таблицу в базе данных, если она не существует.
        /// Добавляет системные столбцы к таблице, если они не существуют.
        /// Создает первичный ключ в таблице, если он не существует.
        /// Данный метод может быть вызван только в контексте транзакции DBTablePartition.
        /// </summary>
        public void InitTable()
        {
            this.InitInternal(false, false);
        }

        /// <summary>
        /// Содает таблицу если она не существует, добавляет столбцы, изменяет параметры столбцов, переименовывает столбцы, 
        /// в случае если данные операции требуются в соответствии со схемой таблицы.
        /// </summary>
        public void InitColumns()
        {
            this.InitInternal(true, false);
        }

        /// <summary>
        /// Содает таблицу если она не существует, создает индексы, пересоздает индексы при изменении параметров индексов, переименовывает индексы, 
        /// в случае если данные операции требуются в соответствии со схемой таблицы.
        /// </summary>
        public void InitIndexes()
        {
            this.InitInternal(false, true);
        }

        /// <summary>
        /// Создает таблицу если она не существует, добавляет столбцы, изменяет параметры столбцов, переименовывает столбцы, создает индексы, 
        /// пересоздает индексы при изменении параметров индексов, переименовывает индексы, 
        /// в случае если данные операции требуются в соответствии со схемой таблицы.
        /// </summary>
        /// <param name="ensureColumns">При установленном true, применяет изменения к стобцам таблицы.</param>
        /// <param name="ensureIndexes">При установленном true, применяет изменения к индексам таблицы.</param>
        internal override DBTable.InitResult InitInternal(bool ensureColumns, bool ensureIndexes)
        {
            //стартуем операцию инициализации таблицы.
            DBTable.InitResult result = null;

            //запускаем код в одной общей Sql-транзакции.
            using (DBTransactionScope transactionScope = this.ConnectionContext.CreateTransactionScope())
            {
                //инициализируем основную таблицу.
                result = base.InitInternal(ensureColumns, ensureIndexes);

                //после создания/инициализации всех таблиц и их столбцов
                //инициализируем триггеры.
                if (ensureColumns)
                    this.InitPrincipalTriggers();

                //завершаем область транзакций.
                transactionScope.Complete();
            }


            //возвращаем результат.
            return result;
        }

        internal override bool InitRequiredInternal()
        {
            bool result = base.InitRequiredInternal();

            return result;
        }

        internal override DBTableRequiredInitAction RequiredInitActionInternal()
        {
            DBTableRequiredInitAction result = base.RequiredInitActionInternal();

            return result;
        }

        #endregion


        #region Rename

        /// <summary>
        /// Переименовывает таблицу.
        /// </summary>
        public void Rename()
        {
            this.RenameInternal();
        }

        /// <summary>
        /// Переименовывает таблицу.
        /// </summary>
        internal override void RenameInternal()
        {
            //запускаем код в одной общей Sql-транзакции.
            using (DBTransactionScope transactionScope = this.ConnectionContext.CreateTransactionScope())
            {
                //переименовываем таблицу.
                base.RenameInternal();

                //завершаем область транзакций.
                transactionScope.Complete();
            }
        }

        #endregion


        #region DeleteData

        /// <summary>
        /// Удаляет данные из таблицы в соответствии с заданным условием.
        /// </summary>
        /// <param name="deleteCondition">Условие удаления данных.</param>
        public void DeleteData(string deleteCondition)
        {
            this.DeleteDataInternal(deleteCondition);
        }

        /// <summary>
        /// Удаляет данные из таблицы в соответствии с заданным условием.
        /// </summary>
        /// <param name="deleteCondition">Условие удаления данных.</param>
        internal override void DeleteDataInternal(string deleteCondition)
        {
            //запускаем код в одной общей Sql-транзакции.
            using (DBTransactionScope transactionScope = this.ConnectionContext.CreateTransactionScope())
            {
                //удаляем данные из исходной таблицы.
                base.DeleteDataInternal(deleteCondition);

                //завершаем область транзакций.
                transactionScope.Complete();
            }
        }

        #endregion


        #region TruncateData

        /// <summary>
        /// Очищает все данные таблицы, сбрасывая автоинкремент на 0.
        /// </summary>
        public void TruncateData()
        {
            this.TruncateDataInternal();
        }

        /// <summary>
        /// Очищает все данные таблицы, сбрасывая автоинкремент на 0.
        /// </summary>
        internal override void TruncateDataInternal()
        {

            //запускаем код в одной общей Sql-транзакции.
            using (DBTransactionScope transactionScope = this.ConnectionContext.CreateTransactionScope())
            {
                //очищаем основную таблицу.
                base.TruncateDataInternal();

                //завершаем область транзакций.
                transactionScope.Complete();
            }
        }

        #endregion


        #region ChangePrefix

        /// <summary>
        /// Переименовывает индексы и триггеры в соответствии с новым префиксом.
        /// </summary>
        public void ChangePrefix()
        {
            this.ChangePrefixInternal();
        }

        /// <summary>
        /// Переименовывает индексы и триггеры в соответствии с новым префиксом.
        /// </summary>
        internal override void ChangePrefixInternal()
        {
            //запускаем код в одной общей Sql-транзакции.
            using (DBTransactionScope transactionScope = this.ConnectionContext.CreateTransactionScope())
            {
                //переименовываем префикс в основной таблице.
                base.ChangePrefixInternal();

                //завершаем область транзакций.
                transactionScope.Complete();
            }
        }

        #endregion


        #region Delete

        /// <summary>
        /// Удаляет таблицу из базы данных.
        /// </summary>
        public void Delete()
        {
            this.DeleteInternal();
        }

        /// <summary>
        /// Удаляет таблицу из базы данных.
        /// </summary>
        internal override void DeleteInternal()
        {
            //запускаем код в одной общей Sql-транзакции.
            using (DBTransactionScope transactionScope = this.ConnectionContext.CreateTransactionScope())
            {
                //удаляем основную таблицу.
                base.DeleteInternal();

                //завершаем область транзакций.
                transactionScope.Complete();
            }
        }

        #endregion


        #region AddColumn

        /// <summary>
        /// Добавляет столбец в таблицу.
        /// </summary>
        /// <param name="columnName"></param>
        public void AddColumn(string columnName)
        {
            this.AddColumnInternal(columnName);
        }

        /// <summary>
        /// Добавляет столбец в таблицу.
        /// </summary>
        /// <param name="columnName"></param>
        internal override void AddColumnInternal(string columnName)
        {
            if (string.IsNullOrEmpty(columnName))
                throw new ArgumentNullException("columnName");

            //запускаем код в одной общей Sql-транзакции.
            using (DBTransactionScope transactionScope = this.ConnectionContext.CreateTransactionScope())
            {
                //добавляем столбец в основную таблицу.
                base.AddColumnInternal(columnName);

                //инициализируем триггеры
                this.InitPrincipalTriggers();

                //завершаем область транзакций.
                transactionScope.Complete();
            }
        }

        #endregion


        #region DeleteColumn

        /// <summary>
        /// Удаляет столбец из таблицы.
        /// </summary>
        /// <param name="columnName">Название столбца.</param>
        public void DeleteColumn(string columnName)
        {
            this.DeleteColumnInternal(columnName);
        }

        /// <summary>
        /// Удаляет столбец из таблицы.
        /// </summary>
        /// <param name="columnName">Название столбца.</param>
        internal override void DeleteColumnInternal(string columnName)
        {
            if (string.IsNullOrEmpty(columnName))
                throw new ArgumentNullException("columnName");

            //запускаем код в одной общей Sql-транзакции.
            using (DBTransactionScope transactionScope = this.ConnectionContext.CreateTransactionScope())
            {
                //удаляем столбец из основной таблицы.
                base.DeleteColumnInternal(columnName);

                //инициализируем триггеры
                this.InitPrincipalTriggers();

                //завершаем область транзакций.
                transactionScope.Complete();
            }
        }

        #endregion


        #region RenameColumn

        /// <summary>
        /// Переименовывает столбец, с названием columnName.
        /// </summary>
        /// <param name="columnName">Новое название переименовываемого столбца.</param>
        public void RenameColumn(string columnName)
        {
            this.RenameColumnInternal(columnName);
        }

        /// <summary>
        /// Переименовывает столбец, с названием columnName.
        /// </summary>
        /// <param name="columnName">Новое название переименовываемого столбца.</param>
        internal override void RenameColumnInternal(string columnName)
        {
            if (string.IsNullOrEmpty(columnName))
                throw new ArgumentNullException("columnName");

            //запускаем код в одной общей Sql-транзакции.
            using (DBTransactionScope transactionScope = this.ConnectionContext.CreateTransactionScope())
            {
                //переименовываем столбец в таблице.
                base.RenameColumnInternal(columnName);

                //инициализируем триггеры
                this.InitPrincipalTriggers();

                //завершаем область транзакций.
                transactionScope.Complete();
            }
        }

        #endregion


        #region UpdateColumn

        /// <summary>
        /// Обновляет параметры столбца.
        /// </summary>
        /// <param name="columnName">Название столбца.</param>
        public void UpdateColumn(string columnName)
        {
            this.UpdateColumnInternal(columnName);
        }

        /// <summary>
        /// Обновляет параметры столбца.
        /// </summary>
        /// <param name="columnName">Название столбца.</param>
        internal override void UpdateColumnInternal(string columnName)
        {
            if (string.IsNullOrEmpty(columnName))
                throw new ArgumentNullException("columnName");

            //запускаем код в одной общей Sql-транзакции.
            using (DBTransactionScope transactionScope = this.ConnectionContext.CreateTransactionScope())
            {
                //обновляем столбец в основной таблице.
                base.UpdateColumnInternal(columnName);

                //завершаем область транзакций.
                transactionScope.Complete();
            }
        }

        #endregion


        #region InitTriggers

        private bool _IsDemandTriggersInitPrincipal;
        private bool IsDemandTriggersInitPrincipal
        {
            get { return _IsDemandTriggersInitPrincipal; }
            set { _IsDemandTriggersInitPrincipal = value; }
        }

        /// <summary>
        /// Инициализирует триггеры таблицы.
        /// </summary>
        public void InitTriggers()
        {
            this.InitTriggersInternal();
        }

        /// <summary>
        /// Инициализирует триггеры таблицы.
        /// </summary>
        internal override void InitTriggersInternal()
        {
            //запускаем код в одной общей Sql-транзакции.
            using (DBTransactionScope transactionScope = this.ConnectionContext.CreateTransactionScope())
            {
                //инициализируем триггеры данной таблицы.
                this.InitPrincipalTriggers();

                //завершаем область транзакций.
                transactionScope.Complete();
            }
        }

        /// <summary>
        /// Служебный метод инициализации триггеров.
        /// </summary>
        private void InitPrincipalTriggers()
        {
            //инициализируем  триггеры основной таблицы.
            base.InitTriggersInternal();
        }

        #endregion


        #region GetTriggerBody

        /// <summary>
        /// Возвращает функциональное тело триггера для таблицы основных данных.
        /// </summary>
        /// <param name="triggerSchema">Схема триггера.</param>
        /// <returns></returns>
        internal override string GetTriggerBody(DBTriggerSchema triggerSchema)
        {
            if (triggerSchema == null)
                throw new ArgumentNullException("triggerSchema");

            //возвращаем тело триггера для основной таблицы.
            return triggerSchema.GetBody(this);
        }

        #endregion


        /// <summary>
        /// Сбрасывает инициализацию названий столбцов при переименовании столбца.
        /// </summary>
        internal override void ResetColumnNames()
        {
            //сбрасываем инициализацию у основной таблицы.
            base.ResetColumnNames();
        }

        /// <summary>
        /// Сбрасывает инициализацию относительных названий индексов при переименовании индекса.
        /// </summary>
        internal override void ResetIndexRelativeNames()
        {
            //сбрасываем инициализацию у основной таблицы.
            base.ResetIndexRelativeNames();
        }
    }
}
