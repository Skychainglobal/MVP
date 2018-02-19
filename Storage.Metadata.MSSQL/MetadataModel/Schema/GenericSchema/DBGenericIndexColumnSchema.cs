using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Storage.Metadata.MSSQL
{
    /// <summary>
    /// Универсальный класс работы со схемой столбца индекса, не представленный в частных реализациях схем таблиц.
    /// </summary>
    public class DBGenericIndexColumnSchema : DBIndexColumnSchema
    {
        /// <summary>
        /// Создает экземпляр DBGenericIndexColumnSchema.
        /// </summary>
        /// <param name="initialProperties">Инициализационные свойства схемы столбца индекса.</param>
        /// <param name="index">Схема индекса.</param>
        public DBGenericIndexColumnSchema(DBGenericIndexColumnSchema.Properties initialProperties, DBIndexSchema index)
            : base(index)
        {
            if (initialProperties == null)
                throw new ArgumentNullException("initialProperties");
            this.InitialProperties = initialProperties;
        }


        #region InitialProperties

        private DBGenericIndexColumnSchema.Properties _InitialProperties;
        /// <summary>
        /// Инициализационные свойства схемы столбца индекса.
        /// </summary>
        private DBGenericIndexColumnSchema.Properties InitialProperties
        {
            get { return _InitialProperties; }
            set { _InitialProperties = value; }
        } 

        #endregion


        /// <summary>
        /// Инициализирует название столбца индекса.
        /// </summary>
        /// <returns></returns>
        protected override string InitName()
        {
            return this.InitialProperties.Name;
        }

        /// <summary>
        /// Инициализирует значение свойства IsDescending.
        /// </summary>
        /// <returns></returns>
        protected override bool InitIsDescending()
        {
            return this.InitialProperties.IsDescending;
        }

        #region Properties Class

        /// <summary>
        /// Представляет набор инициализационных свойств столбца индекса.
        /// </summary>
        public class Properties
        {
            /// <summary>
            /// Создает экземпляр DBGenericIndexColumnSchema.Properties.
            /// </summary>
            public Properties()
            {
                this.IsDescending = false;
            }

            private string _Name;
            /// <summary>
            /// Название столбца индекса.
            /// </summary>
            public string Name
            {
                get { return _Name; }
                set { _Name = value; }
            }

            private bool _IsDescending;
            /// <summary>
            /// Возвращает true, если столбец в индексе сортируется по убыванию.
            /// </summary>
            public bool IsDescending
            {
                get { return _IsDescending; }
                set { _IsDescending = value; }
            }

            /// <summary>
            /// Строковое представление класса DBGenericIndexColumnSchema.Properties.
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
