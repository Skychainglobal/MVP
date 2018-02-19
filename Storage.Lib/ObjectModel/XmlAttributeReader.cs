using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Storage.Lib
{
    public class XmlAttributeReader
    {
        public XmlAttributeReader(XmlNode node)
        {
            this._Node = node;
        }

        private XmlNode _Node;
        public XmlNode Node
        {
            get { return _Node; }
        }

        #region Instance Methods

        public string GetValue(string attributeName)
        {
            string value = XmlAttributeReader.GetValue(this.Node, attributeName);
            return value;
        }

        public string GetValue(string attributeName, string defaultValue)
        {
            string value = XmlAttributeReader.GetValue(this.Node, attributeName, defaultValue);
            return value;
        }

        public Guid GetGuidValue(string attributeName)
        {
            Guid typedValue = XmlAttributeReader.GetGuidValue(this.Node, attributeName);
            return typedValue;
        }

        public Guid GetGuidValue(string attributeName, Guid defaultValue)
        {
            Guid typedValue = XmlAttributeReader.GetGuidValue(this.Node, attributeName, defaultValue);
            return typedValue;
        }

        public DateTime GetDateTimeValue(string attributeName)
        {
            DateTime typedValue = XmlAttributeReader.GetDateTimeValue(this.Node, attributeName);
            return typedValue;
        }

        public int GetIntegerValue(string attributeName)
        {
            int typedValue = XmlAttributeReader.GetIntegerValue(this.Node, attributeName);
            return typedValue;
        }

        public int GetIntegerValue(string attributeName, int defaultValue)
        {
            int typedValue = XmlAttributeReader.GetIntegerValue(this.Node, attributeName, defaultValue);
            return typedValue;
        }

        public bool GetBooleanValue(string attributeName, bool defaultValue)
        {
            bool typedValue = XmlAttributeReader.GetBooleanValue(this.Node, attributeName, defaultValue);
            return typedValue;
        }

        public bool GetBooleanValue(string attributeName)
        {
            bool typedValue = XmlAttributeReader.GetBooleanValue(this.Node, attributeName);
            return typedValue;
        }

        public TEnum GetEnumValue<TEnum>(string attributeName, TEnum defaultValue)
        {
            TEnum typedValue = XmlAttributeReader.GetEnumValue<TEnum>(this.Node, attributeName, defaultValue);
            return typedValue;
        }

        public TEnum GetEnumValue<TEnum>(string attributeName, TEnum defaultValue, bool ignoreCase)
        {
            TEnum typedValue = XmlAttributeReader.GetEnumValue<TEnum>(this.Node, attributeName, defaultValue, ignoreCase);
            return typedValue;
        }

        #endregion

        #region Static Methods

        public static string GetValue(XmlNode xmlNode, string attributeName)
        {
            string value = GetValue(xmlNode, attributeName, null);
            return value;
        }

        public static string GetValue(XmlNode xmlNode, string attributeName, string defaultValue)
        {
            string value = defaultValue;
            if (xmlNode != null && !string.IsNullOrEmpty(attributeName) && xmlNode.Attributes != null)
            {
                XmlAttribute attr = xmlNode.Attributes[attributeName];
                if (attr != null)
                    value = attr.Value;
            }
            return value;
        }

        public static Guid GetGuidValue(XmlNode xmlNode, string attributeName)
        {
            Guid typedValue = GetGuidValue(xmlNode, attributeName, Guid.Empty);
            return typedValue;
        }

        public static Guid GetGuidValue(XmlNode xmlNode, string attributeName, Guid defaultValue)
        {
            Guid typedValue = defaultValue;
            string value = GetValue(xmlNode, attributeName);
            if (!string.IsNullOrEmpty(value))
            {
                value = value.Trim('{', '}');
                typedValue = new Guid(value);
            }
            return typedValue;
        }

        public static bool GetBooleanValue(XmlNode xmlNode, string attributeName, bool defaultValue)
        {
            bool typedValue = defaultValue;
            string value = GetValue(xmlNode, attributeName);
            if (!string.IsNullOrEmpty(value))
                typedValue = Convert.ToBoolean(value);
            return typedValue;
        }

        public static bool GetBooleanValue(XmlNode xmlNode, string attributeName)
        {
            bool typedValue = GetBooleanValue(xmlNode, attributeName, false);
            return typedValue;
        }

        public static int GetIntegerValue(XmlNode xmlNode, string attributeName)
        {
            int typedValue = GetIntegerValue(xmlNode, attributeName, 0);
            return typedValue;
        }

        public static int GetIntegerValue(XmlNode xmlNode, string attributeName, int defaultValue)
        {
            int typedValue = defaultValue;
            string value = GetValue(xmlNode, attributeName);
            if (!string.IsNullOrEmpty(value))
                typedValue = Convert.ToInt32(value);
            return typedValue;
        }

        public static DateTime GetDateTimeValue(XmlNode xmlNode, string attributeName)
        {
            DateTime typedValue = DateTime.MinValue;
            string value = GetValue(xmlNode, attributeName);
            if (!string.IsNullOrEmpty(value))
                typedValue = Convert.ToDateTime(value, new CultureInfo("ru-RU"));
            return typedValue;
        }

        public static TEnum GetEnumValue<TEnum>(XmlNode xmlNode, string attributeName, TEnum defaultValue)
        {
            string enumString = GetValue(xmlNode, attributeName);
            TEnum enumValue = ParseEnum<TEnum>(enumString, defaultValue, false);
            return enumValue;
        }

        public static TEnum GetEnumValue<TEnum>(XmlNode xmlNode, string attributeName, TEnum defaultValue, bool ignoreCase)
        {
            string enumString = GetValue(xmlNode, attributeName);
            TEnum enumValue = ParseEnum<TEnum>(enumString, defaultValue, ignoreCase);
            return enumValue;
        }

        private static TEnum ParseEnum<TEnum>(string enumString, TEnum defaultValue, bool ignoreCase)
        {
            TEnum enumValue = defaultValue;
            if (!string.IsNullOrEmpty(enumString))
                enumValue = (TEnum)Enum.Parse(typeof(TEnum), enumString, ignoreCase);
            return enumValue;
        }

        #endregion
    }
}
