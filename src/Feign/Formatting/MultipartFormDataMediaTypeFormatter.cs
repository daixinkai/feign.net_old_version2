﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Feign.Formatting
{
    public class MultipartFormDataMediaTypeFormatter : IMediaTypeFormatter
    {
        public MultipartFormDataMediaTypeFormatter()
        {
            MediaType = "multipart/form-data";
        }
        public string MediaType { get; }

        public TResult GetResult<TResult>(byte[] buffer, Encoding encoding)
        {
            throw new NotSupportedException();
        }

    }
}
