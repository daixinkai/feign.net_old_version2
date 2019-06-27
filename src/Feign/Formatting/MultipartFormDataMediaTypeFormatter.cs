using System;
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
            MediaType = "application/x-www-form-urlencoded";
        }
        public string MediaType { get; }

        public HttpContent GetHttpContent(object content, MediaTypeHeaderValue contentType)
        {
            string boundary = contentType?.Parameters.FirstOrDefault(s => s.Name == "boundary")?.Value;

            if (string.IsNullOrWhiteSpace(boundary))
            {
                boundary = Convert.ToBase64String(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString("N")));
            }
            //if (content == null)
            //{
            //    return null;
            //}
            MultipartFormDataContent multipartFormDataContent = new MultipartFormDataContent(boundary);
            ////IEnumerable<KeyValuePair<string, string>> nameValueCollection = null;
            ////if (typeof(IDictionary<,>).IsAssignableFrom(content.GetType()))
            ////{

            ////}

            //return multipartFormDataContent;
            return multipartFormDataContent;
        }

        public TResult GetResult<TResult>(byte[] buffer, Encoding encoding)
        {
            throw new NotSupportedException();
        }

    }
}
