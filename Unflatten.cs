using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Extensions
{
    public class Unflatten
    {
        public static bool HasProperty(object obj, string propertyName)
        {
            return obj.GetType().GetProperty(propertyName) != null;
        }

        public static bool IsComplex(Type typeIn)
        {
            return typeIn.IsSubclassOf(typeof(System.ValueType)) || typeIn.Equals(typeof(string));
        }

        public static Object BuildNestedProperty(Object target, String propertyName, Object finalValue)
        {
            if (String.IsNullOrEmpty(propertyName)) return target;

            var trimmed = Regex.Replace(propertyName, "([A-Z])", " $1", System.Text.RegularExpressions.RegexOptions.Compiled).Trim().Split(' ');
            var originalTarget = target;

            foreach (var prop in trimmed)
            {
                target = RecursiveFind(target, finalValue, prop, trimmed);
            }

            return originalTarget;
        }

        private static Object RecursiveFind(Object target, Object finalValue, String prop, IEnumerable<String> trimmed)
        {
            if (String.IsNullOrEmpty(prop)) return target;

            if (HasProperty(target, prop))
            {
                var pi = target.GetType().GetProperty(prop);
                var type = pi.PropertyType;

                if (IsComplex(type))
                {
                    Object nestedInstance;
                    var currentValue = pi.GetValue(target, null);

                    if (null == currentValue) nestedInstance = Activator.CreateInstance(type);
                    else nestedInstance = currentValue;

                    pi.SetValue(target, nestedInstance);
                    target = nestedInstance;
                }
                else
                {
                    pi.SetValue(target, finalValue);
                }
            }
            else
            {
                var position = trimmed.ToList().IndexOf(prop);
                if (position >= 0)
                {
                    prop += trimmed.ToList().ElementAt(position + 1);
                    return RecursiveFind(target, finalValue, prop, trimmed);
                }
            }

            return target;
        }
    }
}