using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ArcGisPlannerToolbox.WPF.Extensions
{
    public static class ExpandoExtensions
    {
        public static IDictionary<string, object> AsIDictionary(this ExpandoObject exObj)
        {
            return exObj as IDictionary<string, object>;
        }
        public static TValue GetValue<TValue>(this ExpandoObject exObj, string key)
        {
            return (TValue)exObj?.AsIDictionary()[key];
        }
        public static TValue GetValueOrDefault<TValue>(this ExpandoObject exObj, string key, TValue defaultValue = default)
        {
            var dict = exObj?.AsIDictionary();
            return dict.ContainsKey(key) ? (TValue)dict[key] : defaultValue;
        }

        public static void UpdateValue<TValue>(this ExpandoObject exObj, string key, TValue value)
        {
            exObj.AsIDictionary()[key] = value;
        }

        public static void AddOrUpdate<TValue>(this ExpandoObject exObj, string key, TValue value)
        {
            var dict = exObj?.AsIDictionary();

            if (dict == null)
                return;

            if (dict.ContainsKey(key))
                dict[key] = value;
            else
                dict.Add(key, value);
        }
        public static bool ExistsValue(this ExpandoObject exObj, string key)
        {
            return exObj.AsIDictionary().ContainsKey(key);
        }

        public static bool TryGetValue<TValue>(this ExpandoObject exObj, string key, out TValue value)
        {
            IDictionary<string, object> dict = exObj.AsIDictionary();
            if (dict.ContainsKey(key) && dict[key] is TValue)
            {
                value = (TValue)dict[key];
                return true;
            }

            value = default;
            return false;
        }

        //  Maps the properties of a dynamic object to a concrete class.  When using this, you will need to cast the
        //  dynamic object to a ExpandoObject explicitly.
        public static T ToConcrete<T>(this ExpandoObject dynObject)
        {
            T instance = Activator.CreateInstance<T>();
            var dict = dynObject as IDictionary<string, object>;
            PropertyInfo[] targetProperties = instance.GetType().GetProperties();

            //If the provided object is null, return null.
            if (dict == null) return default(T);

            foreach (PropertyInfo property in targetProperties)
            {
                object propVal;
                if (dict.TryGetValue(property.Name, out propVal))
                {
                    if (property.PropertyType == typeof(DateTime) || property.PropertyType == typeof(DateTime?))
                    {
                        if (propVal == null) property.SetValue(instance, new DateTime?(), null);
                        else property.SetValue(instance, DateTime.Parse(propVal.ToString()), null);
                    }
                    else
                    {
                        property.SetValue(instance, propVal, null);
                    }
                }
            }

            return instance;
        }


        /// <summary>
        /// Maps the properties of an IEnumberable<dynamic> to a List<T> of a concrete class.  When using this, you will need to cast the dynamic object to IEnumberable<dynamic> explicity.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dynObjects"></param>
        /// <returns></returns>
        public static List<T> ToConcrete<T>(this IEnumerable<dynamic> dynObjects)
        {
            var returnList = new List<T>();

            foreach (ExpandoObject item in dynObjects)
            {
                returnList.Add(item.ToConcrete<T>());
            }

            return returnList;
        }
    }
}
