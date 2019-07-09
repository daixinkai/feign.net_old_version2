﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Feign
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class FeignClientFileRequestContent : FeignClientRequestContent
    {
        public FeignClientFileRequestContent(string name, IRequestFile requestFile)
        {
            Name = name;
            RequestFile = requestFile;
        }
        public string Name { get; }
        public IRequestFile RequestFile { get; }

        public override HttpContent GetHttpContent(MediaTypeHeaderValue contentType)
        {
            HttpContent httpContent = RequestFile?.GetHttpContent();
            if (httpContent != null && string.IsNullOrWhiteSpace(httpContent.Headers.ContentDisposition.Name))
            {
                httpContent.Headers.ContentDisposition.Name = Name;
            }
            return httpContent;
        }
    }
}
