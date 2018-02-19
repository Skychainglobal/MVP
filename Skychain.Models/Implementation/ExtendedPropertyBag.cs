using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Skychain.Models.Implementation
{
    /// <summary>
    /// Представляет класс для работы с расширенными свойствами объектов, 
    /// которые могут задаваться сторонними модулями.
    /// </summary>
    /// <typeparam name="THolder">Тип объекта хранилища свойств.</typeparam>
    public class ExtendedPropertyBag<THolder>
    {
        /// <summary>
        /// Создает экземпляр ExtendedPropertyBag.
        /// </summary>
        public ExtendedPropertyBag(THolder holder)
        {
            if (holder == null)
                throw new ArgumentNullException("holder");
            this.Holder = holder;
            this.PropertyValues = new Dictionary<string, PropertyContainer>();
        }

        private THolder _Holder;
        /// <summary>
        /// Объект-хранилище свойств.
        /// </summary>
        public THolder Holder
        {
            get { return _Holder; }
            private set { _Holder = value; }
        }


        private Dictionary<string, PropertyContainer> _PropertyValues;
        /// <summary>
        /// Словарь расширенных свойств объекта.
        /// </summary>
        private Dictionary<string, PropertyContainer> PropertyValues
        {
            get { return _PropertyValues; }
            set { _PropertyValues = value; }
        }


        /// <summary>
        /// Проверяет наличие ключа расширенного свойства объекта в словаре свойств.
        /// </summary>
        /// <param name="propertyName">Имя расширенного свойства.</param>
        /// <returns></returns>
        public bool ContainsProperty(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
                throw new ArgumentNullException("propertyName");

            bool result = this.PropertyValues.ContainsKey(propertyName);
            return result;
        }

        /// <summary>
        /// Удаляет ключ и значение расширенного свойства из словаря свойств. 
        /// Если словарь свойств не содержит имени переданного свойства - ничего не происходит.
        /// </summary>
        /// <param name="propertyName">Имя расширенного свойства.</param>
        public void RemoveProperty(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
                throw new ArgumentNullException("propertyName");

            if (this.PropertyValues.ContainsKey(propertyName))
            {
                PropertyContainer container = this.GetPropertyContainer(propertyName);
                container.CheckEditable();
                this.PropertyValues.Remove(propertyName);
            }
        }

        #region PropertyContainer

        /// <summary>
        /// Возвращает контэйнер свойства из хранилища свойств.
        /// </summary>
        /// <param name="propertyName"></param>
        internal PropertyContainer GetPropertyContainer(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
                throw new ArgumentNullException("propertyName");

            if (!this.PropertyValues.ContainsKey(propertyName))
                throw new Exception(string.Format("Хранилище свойств не содержит свойство с названием {0}.", propertyName));

            PropertyContainer container = this.PropertyValues[propertyName];
            if (container == null)
                this.ThrowMissingContainerException(propertyName);
            return container;
        }

        private void ThrowMissingContainerException(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
                throw new ArgumentNullException("propertyName");
            throw new Exception(string.Format("Не удалось получить свойство с названием {0} из хранилища свойств.", propertyName));
        }

        /// <summary>
        /// Представляет класс для хранения значения свойства объекта.
        /// </summary>
        internal class PropertyContainer
        {
            internal PropertyContainer(string name)
            {
                if (string.IsNullOrEmpty(name))
                    throw new ArgumentNullException("name");
                this.Name = name;
                this.Editable = true;
            }

            private string _Name;
            /// <summary>
            /// Название свойства.
            /// </summary>
            public string Name
            {
                get { return _Name; }
                private set { _Name = value; }
            }


            private bool _Editable;
            /// <summary>
            /// Возвращает true, если свойство поддерживает операцию SetProperty для существующего значения и RemoveProperty.
            /// Значение по умолчанию равно true.
            /// </summary>
            public bool Editable
            {
                get { return _Editable; }
                internal set { _Editable = value; }
            }


            private object _Value;
            /// <summary>
            /// Значение свойства.
            /// </summary>
            public object Value
            {
                get { return _Value; }
                internal set { _Value = value; }
            }

            /// <summary>
            /// Текстовое представление экземпляра класса PropertyValue.
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                if (!string.IsNullOrEmpty(this.Name))
                    return this.Name;
                return base.ToString();
            }

            /// <summary>
            /// Проверяет возможность проведения операции изменения свойства.
            /// </summary>
            public void CheckEditable()
            {
                if (!this.Editable)
                    throw new Exception(string.Format("Операция недоступна для свойства {0} поскольку данное свойство не поддерживает изменения.", this.Name));
            }
        }

        #endregion


        #region GetProperty

        /// <summary>
        /// Возвращает значение расширенного свойства. 
        /// Если словарь свойств не содержит имени переданного свойства - возвращает null.
        /// </summary>
        /// <param name="propertyName">Имя расширенного свойства.</param>
        /// <returns></returns>
        public object GetProperty(string propertyName)
        {
            return this.GetProperty(propertyName, null);
        }

        /// <summary>
        /// Возвращает значение расширенного свойства. 
        /// Если словарь свойств не содержит имени переданного свойства - возвращает null.
        /// </summary>
        /// <param name="propertyName">Имя расширенного свойства.</param>
        /// <param name="defaultValue">Значение свойства по умолчанию. 
        /// Устанавливается, если свойство не содержится в словаре свойств.</param>
        /// <returns></returns>
        public object GetProperty(string propertyName, object defaultValue)
        {
            if (string.IsNullOrEmpty(propertyName))
                throw new ArgumentNullException("propertyName");

            object propertyValue = defaultValue;
            if (this.PropertyValues.ContainsKey(propertyName))
            {
                PropertyContainer container = this.PropertyValues[propertyName];
                if (container == null)
                    this.ThrowMissingContainerException(propertyName);
                propertyValue = container.Value;
            }
            return propertyValue;
        }

        /// <summary>
        /// Возвращает значение расширенного свойства. 
        /// Если словарь свойств не содержит имени переданного свойства - возвращает null.
        /// </summary>
        /// <param name="propertyName">Имя расширенного свойства.</param>
        /// <typeparam name="T">Тип значения расширенного свойства.</typeparam>
        /// <returns></returns>
        public T GetProperty<T>(string propertyName)
        {
            return this.GetProperty<T>(propertyName, default(T));
        }

        /// <summary>
        /// Возвращает значение расширенного свойства. 
        /// Если словарь свойств не содержит имени переданного свойства - возвращает null.
        /// </summary>
        /// <typeparam name="T">Тип значения расширенного свойства.</typeparam>
        /// <param name="propertyName">Имя расширенного свойства.</param>
        /// <param name="defaultValue">Значение свойства по умолчанию. 
        /// Устанавливается, если свойство не содержится в словаре свойств.</param>
        /// <returns></returns>
        public T GetProperty<T>(string propertyName, T defaultValue)
        {
            return (T)this.GetProperty(propertyName, (object)defaultValue);
        }

        #endregion


        #region SetProperty

        /// <summary>
        /// Устанаваливает значение расширенного свойства объекта.
        /// </summary>
        /// <param name="propertyName">Имя расширенного свойства.</param>
        /// <param name="propertyValue">Значение свойства.</param>
        public void SetProperty(string propertyName, object propertyValue)
        {
            this.SetProperty(propertyName, propertyValue, true);
        }

        /// <summary>
        /// Устанаваливает значение расширенного свойства объекта.
        /// </summary>
        /// <param name="propertyName">Имя расширенного свойства.</param>
        /// <param name="propertyValue">Значение свойства.</param>
        /// <param name="editable">При переданном true, позволяет изменять/удалять значение свойства после его первичной установки.</param>
        public void SetProperty(string propertyName, object propertyValue, bool editable)
        {
            if (string.IsNullOrEmpty(propertyName))
                throw new ArgumentNullException("propertyName");
            if (!this.PropertyValues.ContainsKey(propertyName))
            {
                PropertyContainer container = new PropertyContainer(propertyName);
                container.Value = propertyValue;
                container.Editable = editable;
                this.PropertyValues.Add(propertyName, container);
            }
            else
            {
                PropertyContainer container = this.GetPropertyContainer(propertyName);
                container.CheckEditable();
                container.Value = propertyValue;
            }
        }

        #endregion


        #region GetSingleton

        /// <summary>
        /// Возвращает гарантированно единственный экземпляр класса на уровне данного экземпляра ExtendedProperties и ключа, заданного в параметре propertyName, 
        /// и создаваемый при первом обращении методом, переданным в параметре singletonConstructor.
        /// Данный метод не является потокобезопасным. Корректность работы метода гарантируется при однопоточном использовании.
        /// </summary>
        /// <param name="propertyName">Имя расширенного свойства, в котором хранится одиночный объект.</param>
        /// <param name="singletonConstructor">Метод, инициализирующий одиночный объект.</param>
        /// <returns></returns>
        public object GetSingleton(string propertyName, SingletonConstructor singletonConstructor)
        {
            return this.GetSingleton(propertyName, singletonConstructor, false);
        }

        /// <summary>
        /// Возвращает гарантированно единственный экземпляр класса на уровне данного экземпляра ExtendedProperties и ключа, заданного в параметре propertyName, 
        /// и создаваемый при первом обращении методом, переданным в параметре singletonConstructor.
        /// Данный метод не является потокобезопасным. Корректность работы метода гарантируется при однопоточном использовании.
        /// </summary>
        /// <param name="propertyName">Имя расширенного свойства, в котором хранится одиночный объект.</param>
        /// <param name="singletonConstructor">Метод, инициализирующий одиночный объект.</param>
        /// <param name="resetable">При переданном true, позволяет осуществить сброс singleton-свойства.</param>
        /// <returns></returns>
        public object GetSingleton(string propertyName, SingletonConstructor singletonConstructor, bool resetable)
        {
            if (string.IsNullOrEmpty(propertyName))
                throw new ArgumentNullException("propertyName");
            if (singletonConstructor == null)
                throw new ArgumentNullException("singletonConstructor");

            object singleton = null;

            //если свойство содержится в ExtendedProperties, возвращаем его
            //иначе - создаем и устанавливаем.
            if (this.ContainsProperty(propertyName))
            {
                //получаем объект-одиночку
                singleton = this.GetProperty(propertyName, null);

                //ругаемся, если он пустой
                if (singleton == null)
                    throw new Exception(string.Format("Не задана ссылка на одиночный объект с названием {0}.", propertyName));
            }
            else
            {
                //создаем объект одиночку, если он не задан.
                singleton = singletonConstructor(this.Holder);

                //ругаемся, если конструктор объекта вернул пустоту.
                if (singleton == null)
                    throw new Exception(string.Format("Конструктор одиночного объекта с названием {0} вернул ссылку на пустой объект.", propertyName));

                //устанавливаем объект-одиночку в коллекцию ExtendedProperteis.
                this.SetProperty(propertyName, singleton);
            }

            return singleton;
        }


        /// <summary>
        /// Возвращает гарантированно единственный экземпляр класса на уровне данного экземпляра ExtendedProperties и ключа, заданного в параметре propertyName, 
        /// и создаваемый при первом обращении методом, переданным в параметре singletonConstructor.
        /// Данный метод не является потокобезопасным. Корректность работы метода гарантируется при однопоточном использовании.
        /// </summary>
        /// <typeparam name="T">Тип объекта-одиночки.</typeparam>
        /// <param name="propertyName">Имя расширенного свойства, в котором хранится одиночный объект.</param>
        /// <param name="singletonConstructor">Метод, инициализирующий одиночный объект.</param>
        /// <returns></returns>
        public T GetSingleton<T>(string propertyName, SingletonConstructor singletonConstructor)
        {
            return (T)this.GetSingleton(propertyName, singletonConstructor);
        }

        /// <summary>
        /// Возвращает гарантированно единственный экземпляр класса на уровне данного экземпляра ExtendedProperties и ключа, заданного в параметре propertyName, 
        /// и создаваемый при первом обращении методом, переданным в параметре singletonConstructor.
        /// Данный метод не является потокобезопасным. Корректность работы метода гарантируется при однопоточном использовании.
        /// </summary>
        /// <typeparam name="T">Тип объекта-одиночки.</typeparam>
        /// <param name="propertyName">Имя расширенного свойства, в котором хранится одиночный объект.</param>
        /// <param name="singletonConstructor">Метод, инициализирующий одиночный объект.</param>
        /// <param name="resetable">При переданном true, позволяет осуществить сброс singleton-свойства.</param>
        /// <returns></returns>
        public T GetSingleton<T>(string propertyName, SingletonConstructor singletonConstructor, bool resetable)
        {
            return (T)this.GetSingleton(propertyName, singletonConstructor, resetable);
        }

        /// <summary>
        /// Сбрасывает значение инициализированного singleton-свойства.
        /// </summary>
        /// <param name="propertyName">Название singleton-свойства.</param>
        public void ResetSingleton(string propertyName)
        {
            this.RemoveProperty(propertyName);
        }

        /// <summary>
        /// Создаёт экземпляр объекта, связанный с экземпляром holder.
        /// </summary>
        /// <param name="holder">Экземпляр, с которым связан создаваемый экземпляр.</param>
        public delegate object SingletonConstructor(THolder holder);

        #endregion

        #region ExecuteOnce

        /// <summary>
        /// Выполняет действие один раз для заданного ключа.
        /// </summary>
        /// <param name="executionKey">Ключ выполнения действия.</param>
        /// <param name="codeToExecute">Код выполняемый один раз по заданному ключу.</param>
        public void ExecuteOnce(string executionKey, Action<THolder> codeToExecute)
        {
            if (string.IsNullOrEmpty(executionKey))
                throw new ArgumentNullException("executionKey");
            if (codeToExecute == null)
                throw new ArgumentNullException("codeToExecute");

            //выходим, если свойство с названием ключа выполнения кода было уже установлено.
            if (this.ContainsProperty(executionKey))
                return;

            //выполняем код.
            codeToExecute(this.Holder);

            //устанавливаем признак выполнения кода.
            this.SetProperty(executionKey, true);
        }

        #endregion
    }

}
