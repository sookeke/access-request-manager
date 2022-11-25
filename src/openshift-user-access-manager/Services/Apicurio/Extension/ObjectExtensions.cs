using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Reflection;

namespace UserAccessManager.Services.Apicurio.Extension
{
    public static class ObjectExtensions
    {

        /// <summary>
        /// Transforms the object into an <see cref="IDictionary{TKey, TValue}"/>
        /// </summary>
        /// <param name="source">The object to transform</param>
        /// <returns>A <see cref="IDictionary{TKey, TValue}"/> containing the specified object's property name/value pairs</returns>
        public static IDictionary<string, object> ToDictionary(this object source)
        {
            if (source is IDictionary<string, object> dictionary)
                return dictionary;
            return source.ToDictionary<object>();
        }

        /// <summary>
        /// Transforms the object into an <see cref="IDictionary{TKey, TValue}"/>
        /// </summary>
        /// <typeparam name="T">The type of values wrapped by the <see cref="IDictionary{TKey, TValue}"/> to create</typeparam>
        /// <param name="source">The object to transform</param>
        /// <returns>A <see cref="IDictionary{TKey, TValue}"/> containing the specified object's property name/value pairs</returns>
        public static IDictionary<string, T> ToDictionary<T>(this object source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            Dictionary<string, T> dictionary = new Dictionary<string, T>();
            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(source))
            {
                object value = property.GetValue(source);
                if (IsOfType<T>(value))
                    dictionary.Add(property.Name, (T)value);
            }
            return dictionary;
        }

        /// <summary>
        /// Merges the object with the specified destination object
        /// </summary>
        /// <param name="left">The object to merge</param>
        /// <param name="right">The object to merge the source with</param>
        /// <returns>A new <see cref="ExpandoObject"/></returns>
        public static ExpandoObject Merge(this object left, object right)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));
            if (right == null)
                throw new ArgumentNullException(nameof(right));
            var expandoObject = new ExpandoObject();
            var resultProperties = (IDictionary<string, object>)expandoObject;
            var leftProperties = left.ToDictionary();
            var rightProperties = right.ToDictionary();
            foreach (var property in leftProperties)
            {
                if (rightProperties.TryGetValue(property.Key, out var value)
                    && !value.GetType().IsPrimitiveType())
                    value = property.Value.Merge(value);
                else
                    value = property.Value;
                resultProperties[property.Key] = value;
            }
            foreach (var property in rightProperties)
            {
                if (resultProperties.TryGetValue(property.Key, out var value)
                    && !value.GetType().IsPrimitiveType())
                    value = value.Merge(property.Value);
                else
                    value = property.Value;
                resultProperties[property.Key] = value;
            }
            return expandoObject;
        }

        /// <summary>
        /// Converts the object to a new <see cref="ExpandoObject"/>
        /// </summary>
        /// <param name="obj">The object to convert</param>
        /// <returns>A new <see cref="ExpandoObject"/></returns>
        public static ExpandoObject ToExpandoObject(this object obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            if (obj is not ExpandoObject expandoObject)
            {
                expandoObject = new();
                var expandoDictionary = (IDictionary<string, object>)expandoObject;
                foreach (var kvp in obj.ToDictionary())
                {
                    expandoDictionary.Add(kvp.Key, kvp.Value);
                }
            }
            return expandoObject;
        }

        private static bool IsOfType<T>(object value)
        {
            return value is T;
        }

    }

    public static class MemberInfoExtensions
    {
        public static bool TryGetCustomAttribute<TAttribute>(this MemberInfo extended, out TAttribute attribute)
    where TAttribute : Attribute
        {
            attribute = extended.GetCustomAttribute<TAttribute>();
            return attribute != null;
        }
    }
}