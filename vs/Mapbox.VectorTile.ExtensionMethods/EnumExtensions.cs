using System;
using System.ComponentModel;
using System.Reflection;

namespace Mapbox.VectorTile.ExtensionMethods
{
    public static class EnumExtensions
    {
        public static string Description(this Enum value)
        {
            Type type = value.GetType();
            FieldInfo field = type.GetField(value.ToString());
            object[] customAttributes = field.GetCustomAttributes(typeof(DescriptionAttribute), false);
            return (customAttributes.Length == 0) ? value.ToString() : ((DescriptionAttribute)customAttributes[0]).Description;
        }
    }
}
