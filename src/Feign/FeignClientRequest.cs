using Feign.Formatting;
using Feign.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Feign
{
    public class FeignClientRequest
    {
        public FeignClientRequest(string baseUrl, string mappingUri, string uri, string method, string contentType, object content)
        {
            BaseUrl = baseUrl;
            MappingUri = mappingUri;
            Uri = uri;
            Method = method;
            ContentType = contentType;
            Content = content;
            if (string.IsNullOrWhiteSpace(ContentType))
            {
                ContentType = "application/json; charset=utf-8";
            }
            MediaTypeHeaderValue mediaTypeHeaderValue;
            if (!MediaTypeHeaderValue.TryParse(ContentType, out mediaTypeHeaderValue))
            {
                throw new ArgumentException("ContentType error");
            }
            MediaType = mediaTypeHeaderValue.MediaType;
        }
        public string BaseUrl { get; }
        public string MappingUri { get; }
        public string Uri { get; }
        public string Method { get; }
        public string ContentType { get; }
        public string MediaType { get; }
        public object Content { get; }

        public HttpContent GetHttpContent(MediaTypeFormatterCollection mediaTypeFormatters)
        {
            if (Content != null)
            {
                IMediaTypeFormatter mediaTypeFormatter = mediaTypeFormatters.FindFormatter(MediaType);
                return mediaTypeFormatter?.GetHttpContent(Content);
            }
            return null;
        }

    }
}
