﻿using Feign.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace Feign
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class FeignClientFormRequestContent<T> : FeignClientRequestContent
    {
        public FeignClientFormRequestContent(string name, T content)
        {
            Name = name;
            Content = content;
        }
        public string Name { get; private set; }
        public T Content { get; }

        public override HttpContent GetHttpContent(MediaTypeHeaderValue contentType)
        {
            Type type = typeof(T);
            if (!type.IsValueType && Content == null)
            {
                return null;
            }
            if (type == typeof(byte[]))
            {
                //throw new NotSupportedException();
                return null;
            }
            if (typeof(Stream).IsAssignableFrom(type))
            {
                //throw new NotSupportedException();
                return null;
            }

            if (Type.GetTypeCode(type) != TypeCode.Object)
            {
                return new StringContent(Content.ToString());
            }

            List<KeyValuePair<string, string>> nameValueCollection = new List<KeyValuePair<string, string>>();
            if (typeof(IDictionary).IsAssignableFrom(type))
            {
                IDictionary map = ((IDictionary)Content);
                foreach (var item in map.Keys)
                {
                    if (item != null)
                    {
                        nameValueCollection.Add(new KeyValuePair<string, string>(item.ToString(), SafeToString(map[item])));
                    }
                }
            }
            else if (typeof(IEnumerable).IsAssignableFrom(type) && type != typeof(string))
            {
                IEnumerable enumerable = ((IEnumerable)Content);
                foreach (var item in enumerable)
                {
                    if (item != null)
                    {
                        nameValueCollection.Add(new KeyValuePair<string, string>(Name, item.ToString()));
                    }
                }
            }
            else if (Type.GetTypeCode(type) == TypeCode.Object)
            {
                foreach (var item in type.GetProperties())
                {
                    if (item.GetMethod == null)
                    {
                        continue;
                    }
                    nameValueCollection.Add(new KeyValuePair<string, string>(item.Name, SafeToString(item.GetValue(Content))));
                }
            }
            FormUrlEncodedContent formUrlEncodedContent = new FormUrlEncodedContent(nameValueCollection);
            return formUrlEncodedContent;
        }


        string SafeToString(object content)
        {
            if (content == null)
            {
                return null;
            }
            return content.ToString();
        }

    }
}
