using Feign.Internal;
using System;
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
            Encoding encoding = FeignClientUtils.GetEncoding(contentType);
            //return new ObjectContent(Content, encoding ?? Encoding.UTF8, contentType);
            return new ObjectStringContent(Content, encoding ?? Encoding.UTF8, contentType.MediaType);
        }
    }
}
