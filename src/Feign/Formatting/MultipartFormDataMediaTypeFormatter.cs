using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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

        public HttpContent GetHttpContent(object content)
        {
            //if (content == null)
            //{
            //    return null;
            //}
            //MultipartFormDataContent multipartFormDataContent = new MultipartFormDataContent();
            ////IEnumerable<KeyValuePair<string, string>> nameValueCollection = null;
            ////if (typeof(IDictionary<,>).IsAssignableFrom(content.GetType()))
            ////{

            ////}

            //return multipartFormDataContent;
            throw new NotImplementedException();
        }

        public TResult GetResult<TResult>(byte[] buffer, Encoding encoding)
        {
            throw new NotSupportedException();
        }

    }
}
