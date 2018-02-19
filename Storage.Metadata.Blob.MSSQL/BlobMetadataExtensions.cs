using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storage.Metadata.Blob.MSSQL
{
    /// <summary>
    /// Расширения модуля работы с метаданными файлов и сопоставленных им объектов метаданных.
    /// </summary>
    internal static class BlobMetadataExtensions
    {
        /// <summary>
        /// Заменяет ключевое слово в строке значением.
        /// </summary>
        /// <param name="str">Исходная строка.</param>
        /// <param name="key">Ключ.</param>
        /// <param name="value">Значение.</param>
        /// <returns></returns>
        internal static string ReplaceKey(this string str, string key, object value)
        {
            string replaceValue = string.Empty;
            if (value != null)
                replaceValue = value.ToString();
            if (!string.IsNullOrEmpty(str))
                str = str.Replace("{" + key + "}", replaceValue);
            return str;
        }

        /// <summary>
        /// Заменяет одинарную ковычку ' на две одинарных ковычки '' для использования в качестве текстового значения в Sql-запросе.
        /// </summary>
        /// <param name="str">Строка.</param>
        /// <returns></returns>
        internal static string QueryEncode(this string str)
        {
            if (!string.IsNullOrEmpty(str))
                str = str.Replace("'", "''");
            return str;
        }

        /// <summary>
        /// Обрамляет строку в квадратные скобки [str].
        /// </summary>
        /// <param name="str">Строка.</param>
        /// <returns></returns>
        internal static string QueryName(this string str)
        {
            if (!string.IsNullOrEmpty(str))
                str = string.Format("[{0}]", str);
            return str;
        }
    }
}
