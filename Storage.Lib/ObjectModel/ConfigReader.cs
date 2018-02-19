using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storage.Lib
{
    /// <summary>
    /// Класс для чтения конфигурационного файла.
    /// </summary>
    public class ConfigReader
    {
        /// <summary>
        /// Возвращает текстовое значение параметра.
        /// </summary>
        /// <param name="paramName">Имя параметра конфигурационного файла.</param>
        /// <param name="throwIfNotExists">Выбросить исключение, если параметр не задан.</param>
        /// <returns></returns>
        public static string GetStringValue(string paramName, bool throwIfNotExists = true)
        {
            if (string.IsNullOrEmpty(paramName))
                throw new ArgumentNullException("paramName");

            string value = ConfigurationManager.AppSettings[paramName];
            if (throwIfNotExists && string.IsNullOrEmpty(value))
                throw new Exception(string.Format("Не удалось получить значение параметра конфигурационного файла с именем {0}",
                    paramName));

            return value;
        }

        /// <summary>
        /// Возвращает целочисленное значение параметра.
        /// </summary>
        /// <param name="paramName">Имя параметра конфигурационного файла.</param>
        /// <param name="throwIfNotExists">Выбросить исключение, если параметр не задан.</param>
        /// <returns></returns>
        public static int GetIntegerValue(string paramName, bool throwIfNotExists = true)
        {
            if (string.IsNullOrEmpty(paramName))
                throw new ArgumentNullException("paramName");

            string sValue = ConfigReader.GetStringValue(paramName, throwIfNotExists);
            int value;
            bool valid = int.TryParse(sValue, out value);
            if (throwIfNotExists && !valid)
                throw new Exception(string.Format("Не удалось получить значение параметра конфигурационного файла с именем {0}",
                    paramName));

            return value;
        }

        /// <summary>
        /// Возвращает значение параметра в виде числа с плавающей точкой.
        /// </summary>
        /// <param name="paramName">Имя параметра конфигурационного файла.</param>
        /// <param name="throwIfNotExists">Выбросить исключение, если параметр не задан.</param>
        /// <returns></returns>
        public static double GetDoubleValue(string paramName, bool throwIfNotExists = true)
        {
            if (string.IsNullOrEmpty(paramName))
                throw new ArgumentNullException("paramName");

            string sValue = ConfigReader.GetStringValue(paramName, throwIfNotExists);
            double value;
            bool valid = double.TryParse(sValue, out value);
            if (throwIfNotExists && !valid)
                throw new Exception(string.Format("Не удалось получить значение параметра конфигурационного файла с именем {0}",
                    paramName));

            return value;
        }

        /// <summary>
        /// Возвращает логическое значение параметра.
        /// </summary>
        /// <param name="paramName">Имя параметра конфигурационного файла.</param>
        /// <param name="throwIfNotExists">Выбросить исключение, если параметр не задан.</param>
        /// <returns></returns>
        public static bool GetBooleanValue(string paramName, bool throwIfNotExists = true)
        {
            if (string.IsNullOrEmpty(paramName))
                throw new ArgumentNullException("paramName");

            string sValue = ConfigReader.GetStringValue(paramName, throwIfNotExists);
            bool value;
            bool valid = bool.TryParse(sValue, out value);
            if (throwIfNotExists && !valid)
                throw new Exception(string.Format("Не удалось получить значение параметра конфигурационного файла с именем {0}",
                    paramName));

            return value;
        }
    }
}