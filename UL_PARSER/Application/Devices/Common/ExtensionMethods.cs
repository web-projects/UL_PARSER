using System.ComponentModel;
using System.Reflection;
using UL_PARSER.Devices.Common.Helpers;

namespace System
{
    public static class ExtensionMethods
    {
        public const string NOTFOUND = "STRING VALUE NOT FOUND";

        public static string GetStringValue(this Enum value)
        {
            string stringValue = value?.ToString();
            Type type = value?.GetType();
            FieldInfo fieldInfo = type.GetField(value?.ToString());
            if (fieldInfo is null)
            {
                return NOTFOUND;
            }
            StringValueAttribute[] attrs = fieldInfo.GetCustomAttributes(typeof(StringValueAttribute), false) as StringValueAttribute[];
            if (attrs is null)
            {
                return NOTFOUND;
            }
            if (attrs.Length > 0)
            {
                stringValue = attrs[0].Value;
            }
            return stringValue;
        }

        public static string GetDescriptionValue(this Enum value)
        {
            if (value is null)
            {
                return null;
            }

            string stringValue = value.ToString();
            Type type = value.GetType();
            FieldInfo fieldInfo = type.GetField(value.ToString());
            if (fieldInfo is null)
            {
                return NOTFOUND;
            }
            DescriptionAttribute[] attrs = fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];
            if (attrs is null)
            {
                return NOTFOUND;
            }
            if (attrs.Length > 0)
            {
                stringValue = attrs[0].Description;
            }
            return stringValue;
        }
    }
}

