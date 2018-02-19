using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Storage.Metadata.MSSQL
{
    /// <summary>
    /// Представляет столбец схемы таблицы.
    /// </summary>
    public abstract class DBColumnSchema 
    {
        /// <summary>
        /// Создает экземпляр DBColumnSchema.
        /// </summary>
        /// <param name="schemaAdapter">Схема таблицы.</param>
        public DBColumnSchema(DBTableSchemaAdapter schemaAdapter)
        {
            if (schemaAdapter == null)
                throw new ArgumentNullException("schemaAdapter");

            this.SchemaAdapter = schemaAdapter;
        }

        private DBTableSchemaAdapter _SchemaAdapter;
        /// <summary>
        /// Адаптер схемы таблицы, для которого был создан данный столбец.
        /// </summary>
        public DBTableSchemaAdapter SchemaAdapter
        {
            get { return _SchemaAdapter; }
            private set { _SchemaAdapter = value; }
        }


        #region Name

        /// <summary>
        /// Инициализирует название столбца.
        /// </summary>
        /// <returns></returns>
        protected abstract string InitName();

        private bool __init_Name = false;
        private string _Name;
        /// <summary>
        /// Название столбца.
        /// </summary>
        public string Name
        {
            get
            {
                if (!__init_Name)
                {
                    _Name = this.InitName();
                    if (string.IsNullOrEmpty(_Name))
                        throw new Exception(string.Format("Не задано название столбца в схеме таблицы {0}.", this.SchemaAdapter.TableName));

                    __init_Name = true;
                }
                return _Name;
            }
        }

        private bool __init_NameLow = false;
        private string _NameLow;
        /// <summary>
        /// Название столбца в нижнем регистре.
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

        #endregion


        #region Type

        /// <summary>
        /// Инициализирует тип столбца.
        /// </summary>
        /// <returns></returns>
        protected abstract SqlDbType InitType();

        private bool __init_Type = false;
        private SqlDbType _Type;
        /// <summary>
        /// Тип столбца.
        /// </summary>
        public SqlDbType Type
        {
            get
            {
                if (!__init_Type)
                {
                    _Type = this.InitType();
                    __init_Type = true;
                }
                return _Type;
            }
            set
            {
                _Type = value;
                __init_Type = true;

                this.ResetDefinition();
            }
        }

        #endregion


        #region Size

        /// <summary>
        /// Инициализирует размер столбца.
        /// </summary>
        protected abstract int InitSize();

        private bool __init_Size = false;
        private int _Size;
        /// <summary>
        /// Размер столбца.
        /// </summary>
        public int Size
        {
            get
            {
                if (!__init_Size)
                {
                    _Size = this.InitSize();
                    this.ValidateSize(_Size);
                    __init_Size = true;
                }
                return _Size;
            }
            set
            {
                this.ValidateSize(value);
                _Size = value;
                __init_Size = true;

                this.ResetDefinition();
            }
        }

        /// <summary>
        /// Проверяет корретность размера столбца.
        /// </summary>
        /// <param name="value"></param>
        private void ValidateSize(int value)
        {
            if (value < 0 && value != -1)
                throw new Exception(string.Format("Отрицательное значение размера столбца [{0}] может быть равным только -1 для использования размера столбца (max). Значение размера столбца {1} недопустимо.", this.Name, value));
            if (value == 0 && (
                this.Type == SqlDbType.NVarChar || this.Type == SqlDbType.NChar ||
                this.Type == SqlDbType.VarChar || this.Type == SqlDbType.Char ||
                this.Type == SqlDbType.VarBinary || this.Type == SqlDbType.Binary))
            {
                throw new Exception(string.Format("Не задан размер столбца [{0}] типа {1}. Размер столбца должен быть инициализирован для данного типа столбца.", this.Name, this.Type));
            }
        }

        #endregion


        #region IsNullable

        /// <summary>
        /// Инициализирует значение свойства IsNullable.
        /// </summary>
        /// <returns></returns>
        protected abstract bool InitIsNullable();


        private bool __init_IsNullable = false;
        private bool _IsNullable;
        /// <summary>
        /// Возвращает true, если столбец поддерживает значение NULL.
        /// </summary>
        public bool IsNullable
        {
            get
            {
                if (!__init_IsNullable)
                {
                    _IsNullable = this.InitIsNullable();
                    __init_IsNullable = true;
                }
                return _IsNullable;
            }
            set
            {
                _IsNullable = value;
                __init_IsNullable = true;

                this.ResetDefinition();
            }
        }

        #endregion


        #region Definition

        private void ResetDefinition()
        {
            this.__init_Definition = false;
            this.__init_DefinitionNullable = false;
        }

        private bool __init_Definition = false;
        private string _Definition;
        /// <summary>
        /// Строка определения столбца таблицы, содержащая название, тип, размер и возможность установки значений NULL в столбце.
        /// </summary>
        public string Definition
        {
            get
            {
                if (!__init_Definition)
                {
                    //получаем строку определения столбца.
                    _Definition = DBColumnSchema.GetColumnDefinition(this.Name, this.Type, this.Size, this.IsNullable);

                    __init_Definition = true;
                }
                return _Definition;
            }
        }

        private bool __init_DefinitionNullable = false;
        private string _DefinitionNullable;
        /// <summary>
        /// Строка определения столбца таблицы, содержащая название, тип, размер и положительную возможность установки значений NULL в столбце.
        /// </summary>
        internal string DefinitionNullable
        {
            get
            {
                if (!__init_DefinitionNullable)
                {
                    _DefinitionNullable = DBColumnSchema.GetColumnDefinition(this.Name, this.Type, this.Size, true);
                    __init_DefinitionNullable = true;
                }
                return _DefinitionNullable;
            }
        }

        private bool __init_DefaultEmptyValue = false;
        private string _DefaultEmptyValue;
        /// <summary>
        /// Пустое значение столбца по умолчанию, устанавливаемое в существующие строки таблицы при отсутствии поддержки NULL-значений для столбца.
        /// </summary>
        internal string DefaultEmptyValue
        {
            get
            {
                if (!__init_DefaultEmptyValue)
                {
                    if (this.Type == SqlDbType.Int ||
                        this.Type == SqlDbType.BigInt ||
                        this.Type == SqlDbType.Bit ||
                        this.Type == SqlDbType.Float)
                        _DefaultEmptyValue = "0";
                    else if (this.Type == SqlDbType.NVarChar ||
                        this.Type == SqlDbType.VarChar ||
                        this.Type == SqlDbType.NText ||
                        this.Type == SqlDbType.Text)
                        _DefaultEmptyValue = "N''";
                    else if (this.Type == SqlDbType.UniqueIdentifier)
                        _DefaultEmptyValue = string.Format("N'{0}'", Guid.Empty);
                    else if (this.Type == SqlDbType.DateTime)
                        _DefaultEmptyValue = "N'1900-01-01'";
                    else if (this.Type == SqlDbType.VarBinary ||
                        this.Type == SqlDbType.Binary ||
                        this.Type == SqlDbType.Image)
                        _DefaultEmptyValue = "0x";

                    __init_DefaultEmptyValue = true;
                }
                return _DefaultEmptyValue;
            }
        }

        /// <summary>
        /// Проверяет корректность схемы столбца.
        /// </summary>
        public void Validate()
        {
            object checkObj = this.Definition;
        }

        /// <summary>
        /// Возвращает строку определения столбца таблицы, содержащую название, тип, размер и возможность установки значений NULL в столбце.
        /// </summary>
        /// <param name="columnName">Название столбца.</param>
        /// <param name="dataType">Тип данных столбца.</param>
        /// <param name="size">Размер столбца. Для столбцов, не требующие указание размера, необходимо передать значение 0.</param>
        /// <param name="isNullable">Показывает, возможно ли установка значения NULL в строках таблицы для данного столбца.</param>
        /// <returns></returns>
        internal static string GetColumnDefinition(string columnName, SqlDbType dataType, int size, bool isNullable)
        {
            if (string.IsNullOrEmpty(columnName))
                throw new ArgumentNullException("columnName");

            //задаем строку с размером.
            string sizeString = null;
            if (size > 0)
                sizeString = string.Format("({0})", size);
            else if (size == -1)
                sizeString = "(max)";
            else if (size < 0)
                throw new Exception(string.Format("Обнаружено некорректное значение размера столбца [{0}]: {1}.", columnName, size));

            //проверяем ненулевой timestamp.
            if (dataType == SqlDbType.Timestamp && isNullable)
                throw new Exception(string.Format("Столбец [{0}] типа {1} не должен поддерживать нулевые значения.", columnName, dataType.ToString().ToLower()));

            //задаем параметры возможности пустых значений.
            string nullableString = isNullable ? "NULL" : "NOT NULL";

            //формируем определение столбца.
            string columnDefinition = @"[{ColumnName}] {Type}{Size} {Nullable}"
                .ReplaceKey("ColumnName", columnName)
                .ReplaceKey("Type", dataType.ToString().ToLower())
                .ReplaceKey("Size", sizeString)
                .ReplaceKey("Nullable", nullableString)
                ;

            return columnDefinition;
        }

        #endregion              


        /// <summary>
        /// Строковое представление экземпляра класса DBColumnSchema.
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
