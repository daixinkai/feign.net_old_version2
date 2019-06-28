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
    public class FeignClientMultipartFormRequestContent : FeignClientRequestContent
    {
        public FeignClientMultipartFormRequestContent()
        {
            _contents = new List<object>();
        }


        List<object> _contents;

        public void AddContent(object content)
        {
            _contents.Add(content);
        }

        public override HttpContent GetHttpContent(MediaTypeHeaderValue contentType)
        {
            string boundary = contentType?.Parameters.FirstOrDefault(s => s.Name == "boundary")?.Value;
            if (string.IsNullOrWhiteSpace(boundary))
            {
                boundary = Convert.ToBase64String(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString("N")));
            }
            MultipartFormDataContent multipartFormDataContent = new MultipartFormDataContent(boundary);
            foreach (var content in _contents)
            {
                if (content is HttpContent)
                {
                    multipartFormDataContent.Add((HttpContent)content);
                }
                else if (content is FeignClientRequestContent)
                {
                    multipartFormDataContent.Add(((FeignClientRequestContent)content).GetHttpContent(contentType));
                }
            }
            return multipartFormDataContent;
        }
    }
}
