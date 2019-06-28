﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Feign
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class FeignClientJsonRequestContent : FeignClientRequestContent
    {
        public FeignClientJsonRequestContent(object content)
        {
            Content = content;
        }
        public object Content { get; }

        public override HttpContent GetHttpContent(MediaTypeHeaderValue contentType)
        {
            throw new NotImplementedException();
        }
    }
}
