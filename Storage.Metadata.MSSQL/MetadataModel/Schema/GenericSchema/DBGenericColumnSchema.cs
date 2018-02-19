using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Storage.Metadata.MSSQL
{
    /// <summary>
    /// Универсальный класс работы со схемой столбца, не представленный в частных реализациях схем таблиц.
    /// </summary>
    public class DBGenericColumnSchema : DBColumnSchema
    {
        /// <summary>
        /// Создает экземпляр DBGenericColumnSchema.
        /// </summary>
        /// <param name="initialProperties">Инициализационные свойства схемы столбца.</param>
        /// <param name="schemaAdapter">Адаптер схемы таблицы.</param>
        public DBGenericColumnSchema(DBGenericColumnSchema.Properties initialProperties, DBTableSchemaAdapter schemaAdapter)
            : base(schemaAdapter)
        {
            if (initialProperties == null)
                throw new ArgumentNullException("initialProperties");
            this.InitialProperties = initialProperties;
        }


        #region InitialProperties

        private DBGenericColumnSchema.Properties _InitialProperties;
        /// <summary>
        /// Инициализационные свойства столбца.
        /// </summary>
        private DBGenericColumnSchema.Properties InitialProperties
        {
            get { return _InitialProperties; }
            set { _InitialProperties = value; }
        }

        #endregion


        /// <summary>
        /// Инициализирует название столбца.
        /// </summary>
        /// <returns></returns>
        protected override string InitName()
        {
            return this.InitialProperties.Name;
        }

        /// <summary>
        /// Инициализирует тип столбца.
        /// </summary>
        /// <returns></returns>
        protected override SqlDbType InitType()
        {
            return this.InitialProperties.Type;
        }

        /// <summary>
        /// Инициализирует размер столбца.
        /// </summary>
        protected override int InitSize()
        {
            return this.InitialProperties.Size;
        }

        /// <summary>
        /// Инициализирует значение свойства IsNullable.
        /// </summary>
        /// <returns></returns>
        protected override bool InitIsNullable()
        {
            return this.InitialProperties.IsNullable;
        }


        #region Properties Class

        /// <summary>
        /// Представляет набор инициализационных свойств столбца.
        /// </summary>
        public class Properties
        {
            /// <summary>
            /// Создает экземпляр DBGenericColumnSchema.Properties.
            /// </summary>
            public Properties()
            {
                this.IsNullable = true;
            }

            private string _Name;
            /// <summary>
            /// Название столбца.
            /// </summary>
            public string Name
            {
                get { return _Name; }
                set
                {
                    this.CheckEditable();
                    _Name = value;
                }
            }

            private bool __wasSet_Type = false;
            private SqlDbType _Type = SqlDbType.Int;
            /// <summary>
            /// Тип столбца.
            /// </summary>
            public SqlDbType Type
            {
                get
                {
                    if (!__wasSet_Type)
                        throw new Exception(string.Format("Не задан тип столбца {0} в свойстве Type.", this.Name));
                    return _Type;
                }
                set
                {
                    this.CheckEditable();
                    _Type = value;
                    __wasSet_Type = true;
                }
            }

            private int _Size;
            /// <summary>
            /// Размер столбца.
            /// </summary>
            public int Size
            {
                get { return _Size; }
                set
                {
                    this.CheckEditable();
                    _Size = value;
                }
            }

            private bool _IsNullable;
            /// <summary>
            /// Возвращает true, если столбец поддерживает значение NULL.
            /// </summary>
            public bool IsNullable
            {
                get { return _IsNullable; }
                set
                {
                    this.CheckEditable();
                    _IsNullable = value;
                }
            }

            private bool _ReadOnly;
            /// <summary>
            /// При установленном значении true свойства экземпляра недоступны для изменения.
            /// </summary>
            private bool ReadOnly
            {
                get { return _ReadOnly; }
                set { _ReadOnly = value; }
            }

            /// <summary>
            /// Устанавливает объект в состояние "Только для чтения", обратная операция неподдерживается.
            /// Используется для предоставления в открытый интерфейс инициализированных экземпляров столбцов без возможности изменения из внешнего кода.
            /// </summary>
            public void MakeReadOnly()
            {
                this.ReadOnly = true;
            }

            /// <summary>
            /// Проверяет возможность редактирования объекта.
            /// Генерирует исключение, если объект находится в состоянии "Только для чтения".
            /// </summary>
            private void CheckEditable()
            {
                if (this.ReadOnly)
                    throw new Exception("Свойство объекта доступно только для чтения.");
            }


            /// <summary>
            /// Строковое представление класса DBGenericColumnSchema.Properties.
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                if (!string.IsNullOrEmpty(this.Name))
                    return this.Name;
                return base.ToString();
            }
        }

        #endregion

    }
}
