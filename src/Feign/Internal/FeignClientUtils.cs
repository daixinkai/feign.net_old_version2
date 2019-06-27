using Feign.Formatting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Feign.Internal
{
    static class FeignClientUtils
    {
        #region PathVariable
        public static string ReplacePathVariable(string uri, string name, string value)
        {
            name = "{" + name + "}";
            return uri.Replace(name, value);
        }

        public static string ReplacePathVariable<T>(ConverterCollection converters, string uri, string name, T value)
        {
            return ReplacePathVariable(uri, name, converters.ConvertValue<T, string>(value, true));
        }
        #endregion

        #region RequestParam
        public static string ReplaceRequestParam(string uri, string name, string value)
        {
            //string pattern = "(&?)" + name + "={" + name + "}(&?)([.^&]*)";
            //return Regex.Replace(uri, pattern, match =>
            // {
            //     return match.Value.Replace("{" + name + "}", value);
            // });
            if (uri.IndexOf("?") >= 0)
            {
                return uri + $"&{name}={value}";
            }
            else
            {
                return uri + $"?{name}={value}";
            }
        }

        public static string ReplaceRequestParam<T>(ConverterCollection converters, string uri, string name, T value)
        {
            if (Type.GetTypeCode(typeof(T)) == TypeCode.Object)
            {
                var converter = converters.FindConverter<T, string>();
                if (converter != null)
                {
                    return converter.Convert(value);
                }

                // get properties

                foreach (var property in typeof(T).GetProperties())
                {
                    if (property.GetMethod == null)
                    {
                        continue;
                    }
                    object propertyValue = property.GetValue(value);
                    if (propertyValue == null)
                    {
                        continue;
                    }
                    if (propertyValue is string)
                    {
                        uri = ReplaceRequestParam(uri, property.Name, propertyValue.ToString());
                        continue;
                    }
                    if (propertyValue is IEnumerable)
                    {
                        foreach (var item in propertyValue as IEnumerable)
                        {
                            if (item == null)
                            {
                                continue;
                            }
                            uri = ReplaceRequestParam(uri, property.Name, converters.ConvertValue<string>(item, true));
                        }
                        continue;
                    }
                    uri = ReplaceRequestParam(uri, property.Name, converters.ConvertValue<string>(propertyValue, true));
                }

                return uri;

            }

            return ReplaceRequestParam(uri, name, converters.ConvertValue<T, string>(value, true));
        }

        #endregion

        #region RequestQuery
        public static string ReplaceRequestQuery(string uri, string name, string value)
        {
            if (uri.IndexOf("?") >= 0)
            {
                return uri + $"&{name}={value}";
            }
            else
            {
                return uri + $"?{name}={value}";
            }
        }
        public static string ReplaceRequestQuery<T>(ConverterCollection converters, string uri, string name, T value)
        {
            var typeCode = Type.GetTypeCode(typeof(T));
            if (typeCode == TypeCode.Object)
            {
                if (value == null)
                {
                    return uri;
                }
                //TODO: ReplaceRequestQuery
                //foreach (var property in value.GetType().GetProperties())
                //{
                //    object propertyValue = property.GetValue(value);
                //    if (propertyValue == null)
                //    {
                //        continue;
                //    }
                //    if (propertyValue is IEnumerable&&propertyValue)
                //    {

                //    }
                //}
                return uri;
            }
            else
            {
                return ReplaceRequestQuery(uri, name, converters.ConvertValue<T, string>(value, true));
            }
        }
        #endregion


    }
}
