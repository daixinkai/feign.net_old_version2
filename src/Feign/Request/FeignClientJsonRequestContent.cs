﻿using Feign.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Feign.Request
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class FeignClientJsonRequestContent<T> : FeignClientRequestContent
    {
        public FeignClientJsonRequestContent(string name, T content)
        {
            Name = name;
            Content = content;
        }
        public string Name { get; }
        public T Content { get; }

        public override HttpContent GetHttpContent(MediaTypeHeaderValue contentType)
        {
            Type type = typeof(T);
            if (type == typeof(byte[]) || typeof(Stream).IsAssignableFrom(type))
            {
                //throw new NotSupportedException($"不支持{type.FullName}类型的参数");
                return null;
            }
            Encoding encoding = FeignClientUtils.GetEncoding(contentType);
            //return new ObjectContent(Content, encoding ?? Encoding.UTF8, contentType);
            return new ObjectStringContent(Content, encoding ?? Encoding.UTF8, contentType.MediaType);
        }
    }
}
