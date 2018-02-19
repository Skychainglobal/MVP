using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Storage.Metadata.MSSQL
{
    /// <summary>
    /// 
    /// </summary>
    public class DataRowReader
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="row"></param>
        public DataRowReader(DataRow row)
        {
            this._Row = row;
        }

        private DataRow _Row;
        public DataRow Row
        {
            get { return _Row; }
            set { _Row = value; }
        }

        #region Instance Methods

        public object GetValue(string columnName)
        {
            object value = DataRowReader.GetValue(this.Row, columnName);
            return value;
        }

        public string GetStringValue(string columnName)
        {
            string typedValue = DataRowReader.GetStringValue(this.Row, columnName);
            return typedValue;
        }

        public DateTime GetDateTimeValue(string columnName)
        {
            DateTime typedValue = DataRowReader.GetDateTimeValue(this.Row, columnName);
            return typedValue;
        }

        public DateTime GetDateTimeValue(string columnName, TimeZone timeZone)
        {
            DateTime typedValue = DataRowReader.GetDateTimeValue(this.Row, columnName, timeZone);
            return typedValue;
        }

        public Guid GetGuidValue(string columnName)
        {
            Guid typedValue = DataRowReader.GetGuidValue(this.Row, columnName);
            return typedValue;
        }

        public int GetIntegerValue(string columnName)
        {
            int typedValue = DataRowReader.GetIntegerValue(this.Row, columnName);
            return typedValue;
        }

        public int GetIntegerValue(string columnName, int defaultValue)
        {
            int typedValue = DataRowReader.GetIntegerValue(this.Row, columnName, defaultValue);
            return typedValue;
        }

        public double GetDoubleValue(string columnName)
        {
            double typedValue = DataRowReader.GetDoubleValue(this.Row, columnName);
            return typedValue;
        }

        public double GetDoubleValue(string columnName, double defaultValue)
        {
            double typedValue = DataRowReader.GetDoubleValue(this.Row, columnName, defaultValue);
            return typedValue;
        }

        public bool GetBooleanValue(string columnName)
        {
            bool typedValue = DataRowReader.GetBooleanValue(this.Row, columnName);
            return typedValue;
        }

        public bool GetBooleanValue(string columnName, bool defaultValue)
        {
            bool typedValue = DataRowReader.GetBooleanValue(this.Row, columnName, defaultValue);
            return typedValue;
        }

        public byte[] GetBytesValue(string columnName)
        {
            byte[] typedValue = DataRowReader.GetBytesValue(this.Row, columnName);
            return typedValue;
        }

        public TEnum GetEnumValue<TEnum>(string columnName, TEnum defaultValue)
        {
            TEnum typedValue = DataRowReader.GetEnumValue<TEnum>(this.Row, columnName, defaultValue);
            return typedValue;
        }

        #endregion

        #region Static Methods

        public static object GetValue(DataRow row, string columnName)
        {
            object value = null;
            if (row != null)
            {
                object rowValue = row[columnName];
                if (rowValue != DBNull.Value)
                    value = rowValue;
            }
            return value;
        }

        public static string GetStringValue(DataRow row, string columnName)
        {
            string typedValue = null;
            object value = GetValue(row, columnName);
            if (value != null)
                typedValue = value.ToString();
            return typedValue;
        }

        public static DateTime GetDateTimeValue(DataRow row, string columnName)
        {
            DateTime typedValue = DateTime.MinValue;
            object value = GetValue(row, columnName);
            if (value != null)
                typedValue = (DateTime)value;
            return typedValue;
        }

        public static DateTime GetDateTimeValue(DataRow row, string columnName, TimeZone timeZone)
        {
            DateTime typedValue = DateTime.MinValue;
            object value = GetValue(row, columnName);
            if (value != null)
            {
                typedValue = (DateTime)value;
                if (typedValue != DateTime.MinValue)
                {
                    if (timeZone != null)
                        typedValue = timeZone.ToLocalTime(typedValue);
                    else
                        typedValue = DateTime.SpecifyKind(typedValue, DateTimeKind.Utc).ToLocalTime();
                }
            }
            return typedValue;
        }

        public static Guid GetGuidValue(DataRow row, string columnName)
        {
            Guid typedValue = Guid.Empty;
            object value = GetValue(row, columnName);
            if (value != null)
                typedValue = (Guid)value;
            return typedValue;
        }

        public static int GetIntegerValue(DataRow row, string columnName)
        {
            return DataRowReader.GetIntegerValue(row, columnName, 0);
        }

        public static int GetIntegerValue(DataRow row, string columnName, int defaultValue)
        {
            int typedValue = defaultValue;
            object value = GetValue(row, columnName);
            if (value != null)
            {
                if (value is int)
                    typedValue = (int)value;
                else
                {
                    try
                    {
                        typedValue = Convert.ToInt32(value);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(string.Format("Не удалось привести значение к типу Int. Column: {0}; Value: {1}. SourceException={2}", columnName, value, ex));
                    }
                }
            }
            return typedValue;
        }


        private static CultureInfo EnglishCulture = new CultureInfo("en-US");

        public static double GetDoubleValue(DataRow row, string columnName)
        {
            return GetDoubleValue(row, columnName, 0);
        }

        public static double GetDoubleValue(DataRow row, string columnName, double defaultValue)
        {
            double typedValue = defaultValue;
            object value = GetValue(row, columnName);
            if (value != null)
            {
                if (value is double)
                    typedValue = (double)value;
                else
                {
                    try
                    {
                        string stValue = value.ToString().Replace(',', '.');
                        typedValue = Convert.ToDouble(stValue, EnglishCulture);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(string.Format("Не удалось привести значение к типу Double. Column: {0}; Value: {1}. SourceException={2}", columnName, value, ex));
                    }
                }
            }
            return typedValue;
        }

        public static bool GetBooleanValue(DataRow row, string columnName)
        {
            return GetBooleanValue(row, columnName, false);
        }

        public static bool GetBooleanValue(DataRow row, string columnName, bool defaultValue)
        {
            bool typedValue = defaultValue;
            object value = GetValue(row, columnName);
            if (value != null)
                typedValue = Convert.ToBoolean(value);
            return typedValue;
        }

        public static byte[] GetBytesValue(DataRow row, string columnName)
        {
            byte[] bytes = null;
            object value = GetValue(row, columnName);
            if (value != null)
                bytes = (byte[])value;
            return bytes;
        }

        public static TEnum GetEnumValue<TEnum>(DataRow row, string columnName, TEnum defaultValue)
        {
            object enumObject = GetValue(row, columnName);
            TEnum enumValue = ParseEnum<TEnum>(enumObject, defaultValue);
            return enumValue;
        }

        private static TEnum ParseEnum<TEnum>(object enumObject, TEnum defaultValue)
        {
            TEnum enumValue = defaultValue;
            if (enumObject != null)
                enumValue = (TEnum)Enum.ToObject(typeof(TEnum), enumObject);
            return enumValue;
        }

        #endregion
    }
}
