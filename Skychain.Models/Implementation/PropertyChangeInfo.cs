using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Skychain.Models.Implementation
{
    /// <summary>
    /// Предоставляет информацию об изменении свойств экземпляров классов.
    /// Содержит методы установки изменений свойств экземпляров.
    /// Экземпляры класса не являются потокобезопасными.
    /// </summary>
    public class PropertyChangeInfo : IPropertyChangeInfo
    {
        /// <summary>
        /// Создаёт новый экземпляр PropertyChangeInfo.
        /// </summary>
        public PropertyChangeInfo()
            : this(true)
        {
        }

        /// <summary>
        /// Создаёт новый экземпляр PropertyChangeInfo.
        /// </summary>
        /// <param name="equalEmptyStringToNull">При установленном значении true результат сравнения пустой строки и строки равной null является равенством.
        /// При отсутствии параметра, значение по умолчанию принимается равным true.</param>
        public PropertyChangeInfo(bool equalEmptyStringToNull)
        {
            this.EqualEmptyStringToNull = equalEmptyStringToNull;
        }

        private bool _EqualEmptyStringToNull;
        /// <summary>
        /// При установленном значении true результат сравнения пустой строки и строки равной null является равенством.
        /// </summary>
        public bool EqualEmptyStringToNull
        {
            get { return _EqualEmptyStringToNull; }
            private set { _EqualEmptyStringToNull = value; }
        }


        private bool __init_PropertyValues = false;
        private Dictionary<string, ValueInfo> _PropertyValues;
        /// <summary>
        /// Регистрочувствительная коллекция значений свойств экземпляра.
        /// </summary>
        private Dictionary<string, ValueInfo> PropertyValues
        {
            get
            {
                if (!__init_PropertyValues)
                {
                    _PropertyValues = new Dictionary<string, ValueInfo>();
                    __init_PropertyValues = true;
                }
                return _PropertyValues;
            }
        }


        /// <summary>
        /// Устанавливает исходное и новое значения свойства.
        /// </summary>
        /// <typeparam name="T">Тип устанавливаемого свойства.</typeparam>
        /// <param name="propertyName">Название свойства.</param>
        /// <param name="originalValue">Исходное значение свойства.</param>
        /// <param name="newValue">Новое значение свойства.</param>
        public void SetPropertyChange<T>(string propertyName, T originalValue, T newValue)
        {
            this.SetPropertyChange(propertyName, typeof(T), originalValue, newValue);
        }

        /// <summary>
        /// Устанавливает исходное и новое значения свойства.
        /// </summary>
        /// <param name="propertyName">Название свойства.</param>
        /// <param name="propertyType">Тип значения свойства.</param>
        /// <param name="originalValue">Исходное значение свойства.</param>
        /// <param name="newValue">Новое значение свойства.</param>
        public void SetPropertyChange(string propertyName, Type propertyType, object originalValue, object newValue)
        {
            if (string.IsNullOrEmpty(propertyName))
                throw new ArgumentNullException("propertyName");
            if (propertyType == null)
                throw new ArgumentNullException("propertyType");

            //получаем информацию о значениях свойства.
            ValueInfo valueInfo = null;
            if (!this.PropertyValues.ContainsKey(propertyName))
            {
                valueInfo = new ValueInfo(propertyName, propertyType, this);
                this.PropertyValues.Add(propertyName, valueInfo);
            }
            else
                valueInfo = this.PropertyValues[propertyName];

            //ругаемся, если не удалось получить экземпляр значений свойства.
            if (valueInfo == null)
                throw new Exception(string.Format("Не удалось получить экземпляр значений свойства {0}.", propertyName));

            //устанавливаем значения свойства.
            valueInfo.SetValues(originalValue, newValue);
        }

        /// <summary>
        /// Возвращает true, если значение свойства изменилось.
        /// </summary>
        /// <param name="propertyName">Название проверяемого свойства.</param>
        /// <returns></returns>
        public bool IsPropertyChanged(string propertyName)
        {
            return this.IsPropertyChanged(propertyName, false);
        }

        /// <summary>
        /// Возвращает true, если значение свойства изменилось.
        /// </summary>
        /// <param name="propertyName">Название проверяемого свойства.</param>
        /// <param name="ignoreCase">При установленном значении true, не учитиывает регистр при сравнении строковых значений. Может быть использовано только при сравнении строковых значений.</param>
        /// <returns></returns>
        public bool IsPropertyChanged(string propertyName, bool ignoreCase)
        {
            if (string.IsNullOrEmpty(propertyName))
                throw new ArgumentNullException("propertyName");

            if (this.PropertyValues.ContainsKey(propertyName))
            {
                //получаем информацию о значениях свойства.
                ValueInfo valueInfo = this.PropertyValues[propertyName];

                //ругаемся, если не удалось получить экземпляр значений свойства.
                if (valueInfo == null)
                    throw new Exception(string.Format("Не удалось получить экземпляр значений свойства {0}.", propertyName));

                //возвращаем результат сравнения исходного и нового значения.
                return valueInfo.IsValueChanged(ignoreCase);
            }

            //возвращаем false, если значение свойства не разу не было изменено.
            return false;
        }


        /// <summary>
        /// Возвращает true, если значение было установлено в контексте выполнения кода.
        /// </summary>
        /// <param name="propertyName">Название проверяемого свойства.</param>
        /// <returns></returns>
        public bool IsPropertySet(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
                throw new ArgumentNullException("propertyName");

            //если свойство содержится в коллекции изменённых свойств,
            //возвращаем положительный признак установки значения свойства в контексте выполнения кода
            if (this.PropertyValues.ContainsKey(propertyName))
                return true;

            //возвращаем false, если значение свойства не разу не было установлено в контексте выполнения кода.
            return false;
        }


        #region GetOriginalValue

        /// <summary>
        /// Возвращает оригинальное значение свойства, установленное в свойстве до его изменения.
        /// Генерирует исключение, если значение свойства не было ни разу изменено.
        /// </summary>
        /// <param name="propertyName">Название свойства.</param>
        /// <returns></returns>
        public object GetOriginalValue(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
                throw new ArgumentNullException("propertyName");

            if (this.PropertyValues.ContainsKey(propertyName))
            {
                //получаем информацию о значениях свойства.
                ValueInfo valueInfo = this.PropertyValues[propertyName];

                //ругаемся, если не удалось получить экземпляр значений свойства.
                if (valueInfo == null)
                    throw new Exception(string.Format("Не удалось получить экземпляр значений свойства {0}.", propertyName));

                //возвращаем исходное значение свойства.
                return valueInfo.OriginalValue;
            }
            else
                throw new Exception(string.Format("Невозможно получить исходное значение свойства {0}, поскольку значение свойства не было ни разу изменено.", propertyName));
        }

        /// <summary>
        /// Возвращает оригинальное значение свойства, установленное в свойстве до его изменения.
        /// Генерирует исключение, если значение свойства не было ни разу изменено.
        /// </summary>
        /// <typeparam name="T">Тип свойства.</typeparam>
        /// <param name="propertyName">Название свойства.</param>
        /// <returns></returns>
        public T GetOriginalValue<T>(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
                throw new ArgumentNullException("propertyName");

            if (this.PropertyValues.ContainsKey(propertyName))
            {
                //получаем информацию о значениях свойства.
                ValueInfo valueInfo = this.PropertyValues[propertyName];

                //ругаемся, если не удалось получить экземпляр значений свойства.
                if (valueInfo == null)
                    throw new Exception(string.Format("Не удалось получить экземпляр значений свойства {0}.", propertyName));

                //возвращаем исходное значение свойства.
                return (T)valueInfo.OriginalValue;
            }
            else
                throw new Exception(string.Format("Невозможно получить исходное значение свойства {0}, поскольку значение свойства не было ни разу изменено.", propertyName));
        }

        /// <summary>
        /// Возвращает оригинальное значение свойства, установленное в свойстве до его изменения.
        /// Возвращает значение свойства по умолчанию, указанное в параметре defaultOriginalValue, если значение свойства не было ни разу изменено.
        /// </summary>
        /// <typeparam name="T">Тип свойства.</typeparam>
        /// <param name="propertyName">Название свойства.</param>
        /// <param name="defaultOriginalValue">Исходное значение свойства по умолчанию, возращаемое в качестве результата, если значение свойства не было ни разу изменено.</param>
        /// <returns></returns>
        public T GetOriginalValue<T>(string propertyName, T defaultOriginalValue)
        {
            if (string.IsNullOrEmpty(propertyName))
                throw new ArgumentNullException("propertyName");

            if (this.PropertyValues.ContainsKey(propertyName))
            {
                //получаем информацию о значениях свойства.
                ValueInfo valueInfo = this.PropertyValues[propertyName];

                //ругаемся, если не удалось получить экземпляр значений свойства.
                if (valueInfo == null)
                    throw new Exception(string.Format("Не удалось получить экземпляр значений свойства {0}.", propertyName));

                //возвращаем исходное значение свойства.
                return (T)valueInfo.OriginalValue;
            }

            //возвращаем исходное значение свойства по умолчанию.
            return defaultOriginalValue;
        }

        #endregion


        #region GetNewValue

        /// <summary>
        /// Возвращает новое значение свойства, установленное в свойстве до его изменения.
        /// Генерирует исключение, если значение свойства не было ни разу изменено.
        /// </summary>
        /// <param name="propertyName">Название свойства.</param>
        /// <returns></returns>
        public object GetNewValue(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
                throw new ArgumentNullException("propertyName");

            if (this.PropertyValues.ContainsKey(propertyName))
            {
                //получаем информацию о значениях свойства.
                ValueInfo valueInfo = this.PropertyValues[propertyName];

                //ругаемся, если не удалось получить экземпляр значений свойства.
                if (valueInfo == null)
                    throw new Exception(string.Format("Не удалось получить экземпляр значений свойства {0}.", propertyName));

                //возвращаем исходное значение свойства.
                return valueInfo.NewValue;
            }
            else
                throw new Exception(string.Format("Невозможно получить новое значение свойства {0}, поскольку значение свойства не было ни разу изменено.", propertyName));
        }

        /// <summary>
        /// Возвращает новое значение свойства, установленное в свойстве до его изменения.
        /// Генерирует исключение, если значение свойства не было ни разу изменено.
        /// </summary>
        /// <typeparam name="T">Тип свойства.</typeparam>
        /// <param name="propertyName">Название свойства.</param>
        /// <returns></returns>
        public T GetNewValue<T>(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
                throw new ArgumentNullException("propertyName");

            if (this.PropertyValues.ContainsKey(propertyName))
            {
                //получаем информацию о значениях свойства.
                ValueInfo valueInfo = this.PropertyValues[propertyName];

                //ругаемся, если не удалось получить экземпляр значений свойства.
                if (valueInfo == null)
                    throw new Exception(string.Format("Не удалось получить экземпляр значений свойства {0}.", propertyName));

                //возвращаем исходное значение свойства.
                return (T)valueInfo.NewValue;
            }
            else
                throw new Exception(string.Format("Невозможно получить новое значение свойства {0}, поскольку значение свойства не было ни разу изменено.", propertyName));
        }

        /// <summary>
        /// Возвращает новое значение свойства, установленное в свойстве до его изменения.
        /// Возвращает значение свойства по умолчанию, указанное в параметре defaultNewValue, если значение свойства не было ни разу изменено.
        /// </summary>
        /// <typeparam name="T">Тип свойства.</typeparam>
        /// <param name="propertyName">Название свойства.</param>
        /// <param name="defaultNewValue">Новое значение свойства по умолчанию, возращаемое в качестве результата, если значение свойства не было ни разу изменено.</param>
        /// <returns></returns>
        public T GetNewValue<T>(string propertyName, T defaultNewValue)
        {
            if (string.IsNullOrEmpty(propertyName))
                throw new ArgumentNullException("propertyName");

            if (this.PropertyValues.ContainsKey(propertyName))
            {
                //получаем информацию о значениях свойства.
                ValueInfo valueInfo = this.PropertyValues[propertyName];

                //ругаемся, если не удалось получить экземпляр значений свойства.
                if (valueInfo == null)
                    throw new Exception(string.Format("Не удалось получить экземпляр значений свойства {0}.", propertyName));

                //возвращаем исходное значение свойства.
                return (T)valueInfo.NewValue;
            }

            //возвращаем исходное значение свойства по умолчанию.
            return defaultNewValue;
        }

        #endregion


        /// <summary>
        /// Сбрасывает статус исходного и нового значения свойства.
        /// </summary>
        /// <param name="propertyName">Название свойства.</param>
        public void ResetPropertyChange(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
                throw new ArgumentNullException("propertyName");

            //удаляем информацию о значениях свойства из словаря.
            if (this.PropertyValues.ContainsKey(propertyName))
                this.PropertyValues.Remove(propertyName);
        }

        /// <summary>
        /// Сбрасывает статус исходного и нового значения всех свойств экземпляра.
        /// </summary>
        public void ResetAllPropertyChanges()
        {
            //очищаем словарь изменений свойств.
            this.PropertyValues.Clear();
        }


        /// <summary>
        /// Представляет исходное и изменённое значения свойства.
        /// </summary>
        internal class ValueInfo
        {
            internal ValueInfo(string propertyName, Type valueType, PropertyChangeInfo changeInfo)
            {
                if (string.IsNullOrEmpty(propertyName))
                    throw new ArgumentNullException("propertyName");
                if (valueType == null)
                    throw new ArgumentNullException("valueType");
                if (changeInfo == null)
                    throw new ArgumentNullException("changeInfo");

                this.PropertyName = propertyName;
                this.ValueType = valueType;
                this.ChangeInfo = changeInfo;
            }

            private string _PropertyName;
            /// <summary>
            /// Название свойства.
            /// </summary>
            public string PropertyName
            {
                get { return _PropertyName; }
                private set { _PropertyName = value; }
            }

            private Type _ValueType;
            /// <summary>
            /// Тип значения свойства.
            /// </summary>
            public Type ValueType
            {
                get { return _ValueType; }
                private set { _ValueType = value; }
            }

            private PropertyChangeInfo _ChangeInfo;
            /// <summary>
            /// Экземпляр информации об изменении свойств, в который добавлено данное свойство.
            /// </summary>
            public PropertyChangeInfo ChangeInfo
            {
                get { return _ChangeInfo; }
                private set { _ChangeInfo = value; }
            }


            private bool __init_IsString = false;
            private bool _IsString;
            /// <summary>
            /// Возвращает true, если тип свойства является string.
            /// </summary>
            private bool IsString
            {
                get
                {
                    if (!__init_IsString)
                    {
                        _IsString = this.ValueType.Equals(typeof(string));
                        __init_IsString = true;
                    }
                    return _IsString;
                }
            }



            private bool __init_IsXmlElement = false;
            private bool _IsXmlElement;
            /// <summary>
            /// Возвращает true, если тип свойства является XmlNode или XmlElement.
            /// </summary>
            private bool IsXmlElement
            {
                get
                {
                    if (!__init_IsXmlElement)
                    {
                        _IsXmlElement = this.ValueType.Equals(typeof(XmlNode)) || this.ValueType.Equals(typeof(XmlElement));
                        __init_IsXmlElement = true;
                    }
                    return _IsXmlElement;
                }
            }



            private bool __wasSet_OriginalValue;
            private object _OriginalValue;
            /// <summary>
            /// Исходное значение свойства.
            /// </summary>
            public object OriginalValue
            {
                get { return _OriginalValue; }
                private set
                {
                    //исходное значение свойства устанавливаем только один раз для экземпляра.
                    if (__wasSet_OriginalValue)
                        return;

                    //устанавливаем исходное значение свойства.
                    _OriginalValue = value;

                    //устанавливаем признак факта установки исходного значения свойства.
                    __wasSet_OriginalValue = true;
                }
            }

            private object _NewValue;
            /// <summary>
            /// Новое значение свойства.
            /// </summary>
            public object NewValue
            {
                get { return _NewValue; }
                private set { _NewValue = value; }
            }


            /// <summary>
            /// Устанавливает исходное и новое значения свойства.
            /// </summary>
            /// <param name="originalValue">Исходное значение свойства.</param>
            /// <param name="newValue">Новое значение свойства.</param>
            internal void SetValues(object originalValue, object newValue)
            {
                this.OriginalValue = originalValue;
                this.NewValue = newValue;
            }

            /// <summary>
            /// Возвращает true, если значение свойства измененилось.
            /// </summary>
            /// <param name="ignoreCase">При установленном значении true, не учитиывает регистр при сравнении строковых значений.</param>
            /// <returns></returns>
            public bool IsValueChanged(bool ignoreCase)
            {
                //проверяем корректность использования ignoreCase.
                if (ignoreCase && !this.IsString)
                    throw new Exception(string.Format("Тип значения {0} свойства {1} не является строкой, поэтому поэтому значения свойства не могут быть сравнены как строки в нижнем регистре.",
                        this.PropertyName, this.ValueType.FullName));

                //сравниваем исходное и новое значения строкового типа.
                if (this.IsString)
                {
                    string originalString = (string)this.OriginalValue;
                    string newString = (string)this.NewValue;

                    //если пустая строка воспринимается как null, приравниваем значения к null.
                    if (this.ChangeInfo.EqualEmptyStringToNull)
                    {
                        if (originalString == string.Empty)
                            originalString = null;
                        if (newString == string.Empty)
                            newString = null;
                    }

                    //если сравнение производится без учёта ригистра, приводим строки к нижнему регистру.
                    if (ignoreCase)
                    {
                        if (originalString != null)
                            originalString = originalString.ToLower();
                        if (newString != null)
                            newString = newString.ToLower();
                    }

                    //возвращаем результат сравнения строк.
                    return !string.Equals(originalString, newString);
                }
                //сравниваем исходное и новое значения типа Xml-элемент.
                else if (this.IsXmlElement)
                {
                    //сравниваем экземпляры объектов, если они отличаются от null.
                    if (this.OriginalValue != null && this.NewValue != null)
                    {
                        XmlElement originalXml = (XmlElement)this.OriginalValue;
                        XmlElement newXml = (XmlElement)this.NewValue;
                        return !string.Equals(originalXml.OuterXml, newXml.OuterXml);
                    }
                    //возвращаем false, если исходное и новое значения пустые, т.е. значение не было изменено.
                    else if (this.OriginalValue == null && this.NewValue == null)
                        return false;
                    //возвращаем true, если одно из значений исходное или новое является пустым, а другое не пустым.
                    else
                        return true;
                }
                //сравниваем исходное и новое значения нестрокового типа.
                else
                {
                    //сравниваем экземпляры объектов, если они отличаются от null.
                    if (this.OriginalValue != null && this.NewValue != null)
                        return !this.OriginalValue.Equals(this.NewValue);
                    //возвращаем false, если исходное и новое значения пустые, т.е. значение не было изменено.
                    else if (this.OriginalValue == null && this.NewValue == null)
                        return false;
                    //возвращаем true, если одно из значений исходное или новое является пустым, а другое не пустым.
                    else
                        return true;
                }
            }

        }

    }

    /// <summary>
    /// Предоставляет информацию об изменении свойств экземпляров классов.
    /// Экземпляры класса не являются потокобезопасными.
    /// </summary>
    public interface IPropertyChangeInfo
    {
        /// <summary>
        /// При установленном значении true результат сравнения пустой строки и строки равной null является равенством.
        /// </summary>
        bool EqualEmptyStringToNull { get; }

        /// <summary>
        /// Возвращает true, если значение свойства изменилось.
        /// </summary>
        /// <param name="propertyName">Название проверяемого свойства.</param>
        /// <returns></returns>
        bool IsPropertyChanged(string propertyName);

        /// <summary>
        /// Возвращает true, если значение свойства изменилось.
        /// </summary>
        /// <param name="propertyName">Название проверяемого свойства.</param>
        /// <param name="ignoreCase">При установленном значении true, не учитиывает регистр при сравнении строковых значений. Может быть использовано только при сравнении строковых значений.</param>
        /// <returns></returns>
        bool IsPropertyChanged(string propertyName, bool ignoreCase);

        /// <summary>
        /// Возвращает true, если значение было установлено в контексте выполнения кода.
        /// </summary>
        /// <param name="propertyName">Название проверяемого свойства.</param>
        /// <returns></returns>
        bool IsPropertySet(string propertyName);

        /// <summary>
        /// Возвращает оригинальное значение свойства, установленное в свойстве до его изменения.
        /// Генерирует исключение, если значение свойства не было ни разу изменено.
        /// </summary>
        /// <param name="propertyName">Название свойства.</param>
        /// <returns></returns>
        object GetOriginalValue(string propertyName);

        /// <summary>
        /// Возвращает оригинальное значение свойства, установленное в свойстве до его изменения.
        /// Генерирует исключение, если значение свойства не было ни разу изменено.
        /// </summary>
        /// <typeparam name="T">Тип свойства.</typeparam>
        /// <param name="propertyName">Название свойства.</param>
        /// <returns></returns>
        T GetOriginalValue<T>(string propertyName);

        /// <summary>
        /// Возвращает оригинальное значение свойства, установленное в свойстве до его изменения.
        /// Возвращает значение свойства по умолчанию, указанное в параметре defaultOriginalValue, если значение свойства не было ни разу изменено.
        /// </summary>
        /// <typeparam name="T">Тип свойства.</typeparam>
        /// <param name="propertyName">Название свойства.</param>
        /// <param name="defaultOriginalValue">Исходное значение свойства по умолчанию, возращаемое в качестве результата, если значение свойства не было ни разу изменено.</param>
        /// <returns></returns>
        T GetOriginalValue<T>(string propertyName, T defaultOriginalValue);

        /// <summary>
        /// Возвращает новое значение свойства, установленное в свойстве до его изменения.
        /// Генерирует исключение, если значение свойства не было ни разу изменено.
        /// </summary>
        /// <param name="propertyName">Название свойства.</param>
        /// <returns></returns>
        object GetNewValue(string propertyName);

        /// <summary>
        /// Возвращает новое значение свойства, установленное в свойстве до его изменения.
        /// Генерирует исключение, если значение свойства не было ни разу изменено.
        /// </summary>
        /// <typeparam name="T">Тип свойства.</typeparam>
        /// <param name="propertyName">Название свойства.</param>
        /// <returns></returns>
        T GetNewValue<T>(string propertyName);

        /// <summary>
        /// Возвращает новое значение свойства, установленное в свойстве до его изменения.
        /// Возвращает значение свойства по умолчанию, указанное в параметре defaultNewValue, если значение свойства не было ни разу изменено.
        /// </summary>
        /// <typeparam name="T">Тип свойства.</typeparam>
        /// <param name="propertyName">Название свойства.</param>
        /// <param name="defaultNewValue">Новое значение свойства по умолчанию, возращаемое в качестве результата, если значение свойства не было ни разу изменено.</param>
        /// <returns></returns>
        T GetNewValue<T>(string propertyName, T defaultNewValue);
    }
}
