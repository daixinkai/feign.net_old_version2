using Feign.Formatting;
using Feign.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Feign
{
    public class FeignClientRequest
    {
        public FeignClientRequest(string baseUrl, string mappingUri, string uri, string httpMethod, string contentType, object content, MethodInfo method)
        {
            BaseUrl = baseUrl;
            MappingUri = mappingUri;
            Uri = uri;
            HttpMethod = httpMethod;
            Content = content;
            Method = method;
            if (string.IsNullOrWhiteSpace(contentType))
            {
                contentType = "application/json; charset=utf-8";
            }
            MediaTypeHeaderValue mediaTypeHeaderValue;
            if (!MediaTypeHeaderValue.TryParse(contentType, out mediaTypeHeaderValue))
            {
                throw new ArgumentException("ContentType error");
            }
            MediaType = mediaTypeHeaderValue.MediaType;
            ContentType = mediaTypeHeaderValue;
        }
        public string BaseUrl { get; }
        public string MappingUri { get; }
        public string Uri { get; }
        public string HttpMethod { get; }
        public MediaTypeHeaderValue ContentType { get; }
        public string MediaType { get; }
        public object Content { get; }
        public MethodInfo Method { get; }
        public HttpContent GetHttpContent(MediaTypeFormatterCollection mediaTypeFormatters)
        {
            if (Content != null)
            {
                IMediaTypeFormatter mediaTypeFormatter = mediaTypeFormatters.FindFormatter(MediaType);
                return mediaTypeFormatter?.GetHttpContent(Content, ContentType);
            }
            return null;
        }

    }
}
