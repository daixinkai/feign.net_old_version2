using Feign.Cache;
using Feign.Discovery;
using Feign.Formatting;
using Feign.Internal;
using Feign.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Feign.Proxy
{
    public abstract class FeignClientProxyService : IFeignClient, IDisposable
    {

        public FeignClientProxyService(IFeignOptions feignOptions, IServiceDiscovery serviceDiscovery, IServiceCacheProvider serviceCacheProvider, ILoggerFactory loggerFactory)
        {
            _feignOptions = feignOptions;
            //_logger = loggerFactory?.CreateLogger(this.GetType());
            _logger = loggerFactory?.CreateLogger(typeof(FeignClientProxyService));
            _globalFeignClientPipeline = _feignOptions?.FeignClientPipeline as GlobalFeignClientPipelineBuilder;
            ServiceDiscoveryHttpClientHandler serviceDiscoveryHttpClientHandler = new ServiceDiscoveryHttpClientHandler(this, serviceDiscovery, _globalFeignClientPipeline, serviceCacheProvider, _logger);
            serviceDiscoveryHttpClientHandler.ShouldResolveService = string.IsNullOrWhiteSpace(Url);
            serviceDiscoveryHttpClientHandler.AllowAutoRedirect = false;
            HttpClient = new HttpClient(serviceDiscoveryHttpClientHandler);
            string baseUrl = serviceDiscoveryHttpClientHandler.ShouldResolveService ? ServiceId ?? "" : Url;
            if (!baseUrl.StartsWith("http"))
            {
                baseUrl = $"http://{baseUrl}";
            }
            if (!string.IsNullOrWhiteSpace(BaseUri))
            {
                if (baseUrl.EndsWith("/"))
                {
                    baseUrl = baseUrl.TrimEnd('/');
                }
                if (BaseUri.StartsWith("/"))
                {
                    baseUrl += BaseUri;
                }
                else
                {
                    baseUrl += "/" + BaseUri;
                }
            }

            if (baseUrl.EndsWith("/"))
            {
                baseUrl = baseUrl.TrimEnd('/');
            }
            BaseUrl = baseUrl;

            InitializingEventArgs initializingEventArgs = new InitializingEventArgs(this);
            initializingEventArgs.HttpClient = HttpClient;
            _globalFeignClientPipeline?.InvokeInitializing(this, initializingEventArgs);
            HttpClient = initializingEventArgs.HttpClient;
            if (HttpClient == null)
            {
                throw new ArgumentNullException(nameof(HttpClient));
            }

        }


        public abstract string ServiceId { get; }

        protected virtual bool IsResponseSuspendedRequest => true;

        public virtual string BaseUri { get { return null; } }

        public virtual string Url { get { return null; } }

        protected string BaseUrl { get; }

        ILogger _logger;

        internal GlobalFeignClientPipelineBuilder _globalFeignClientPipeline;

        IFeignOptions _feignOptions;

        protected HttpClient HttpClient { get; }

        #region Send Request

        #region Define

        internal static readonly MethodInfo HTTP_SEND_GENERIC_METHOD = typeof(FeignClientProxyService).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).Where(o => o.IsGenericMethod).FirstOrDefault(o => o.Name == "Send");
        internal static readonly MethodInfo HTTP_SEND_ASYNC_GENERIC_METHOD = typeof(FeignClientProxyService).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).Where(o => o.IsGenericMethod).FirstOrDefault(o => o.Name == "SendAsync");

        internal static readonly MethodInfo HTTP_SEND_METHOD = typeof(FeignClientProxyService).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).Where(o => !o.IsGenericMethod).FirstOrDefault(o => o.Name == "Send");
        internal static readonly MethodInfo HTTP_SEND_ASYNC_METHOD = typeof(FeignClientProxyService).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).Where(o => !o.IsGenericMethod).FirstOrDefault(o => o.Name == "SendAsync");

        #endregion

        protected async Task SendAsync(FeignClientRequest request)
        {
            HttpResponseMessage response = await GetResponseMessageAsync(request);
            await GetResultAsync<string>(request, response);
        }
        protected async Task<TResult> SendAsync<TResult>(FeignClientRequest request)
        {
            HttpResponseMessage response = await GetResponseMessageAsync(request);
            return await GetResultAsync<TResult>(request, response);
        }
        protected void Send(FeignClientRequest request)
        {
            HttpResponseMessage response = GetResponseMessage(request);
            GetResult<string>(request, response);
        }
        protected TResult Send<TResult>(FeignClientRequest request)
        {
            HttpResponseMessage response = GetResponseMessage(request);
            return GetResult<TResult>(request, response);
        }

        HttpResponseMessage GetResponseMessage(FeignClientRequest request)
        {
            try
            {
                return SendAsyncInternal(request).GetResult();
            }
            catch (SuspendedRequestException)
            {
                if (IsResponseSuspendedRequest)
                {
                    return null;
                }
                throw;
            }
            catch (Exception ex)
            {
                #region ErrorRequest
                ErrorRequestEventArgs errorArgs = new ErrorRequestEventArgs(this, ex);
                _globalFeignClientPipeline?.InvokeErrorRequest(this, errorArgs);
                if (errorArgs.ExceptionHandled)
                {
                    return null;
                }
                #endregion
                throw;
            }
        }

        async Task<HttpResponseMessage> GetResponseMessageAsync(FeignClientRequest request)
        {
            try
            {
                return await SendAsyncInternal(request);
            }
            catch (SuspendedRequestException)
            {
                if (IsResponseSuspendedRequest)
                {
                    return null;
                }
                throw;
            }
            catch (Exception ex)
            {
                #region ErrorRequest
                ErrorRequestEventArgs errorArgs = new ErrorRequestEventArgs(this, ex);
                _globalFeignClientPipeline?.InvokeErrorRequest(this, errorArgs);
                if (errorArgs.ExceptionHandled)
                {
                    return null;
                }
                #endregion
                throw;
            }
        }

        void EnsureSuccess(FeignClientRequest request, HttpResponseMessage responseMessage)
        {
            if (!responseMessage.IsSuccessStatusCode)
            {
                string content = responseMessage.Content.ReadAsStringAsync().GetResult();
                _logger?.LogError($"request on \"{responseMessage.RequestMessage.RequestUri.ToString()}\" status code : " + responseMessage.StatusCode.GetHashCode() + " content : " + content);
                throw new FeignHttpRequestException(this,
                    responseMessage.RequestMessage as FeignHttpRequestMessage,
                    new HttpRequestException($"Response status code does not indicate success: {responseMessage.StatusCode.GetHashCode()} ({responseMessage.ReasonPhrase}).\r\nContent : {content}"));
            }
        }

        async Task EnsureSuccessAsync(FeignClientRequest request, HttpResponseMessage responseMessage)
        {
            if (!responseMessage.IsSuccessStatusCode)
            {
                string content = await responseMessage.Content.ReadAsStringAsync();
                _logger?.LogError($"request on \"{responseMessage.RequestMessage.RequestUri.ToString()}\" status code : " + responseMessage.StatusCode.GetHashCode() + " content : " + content);
                throw new FeignHttpRequestException(this,
                    responseMessage.RequestMessage as FeignHttpRequestMessage,
                    new HttpRequestException($"Response status code does not indicate success: {responseMessage.StatusCode.GetHashCode()} ({responseMessage.ReasonPhrase}).\r\nContent : {content}"));
            }
        }

        TResult GetResult<TResult>(FeignClientRequest request, HttpResponseMessage responseMessage)
        {
            if (responseMessage == null)
            {
                return default(TResult);
            }
            #region ReceivingResponse
            ReceivingResponseEventArgs<TResult> receivingResponseEventArgs = new ReceivingResponseEventArgs<TResult>(this, responseMessage);
            _globalFeignClientPipeline?.GetServicePipeline(this.ServiceId)?.OnReceivingResponse(this, receivingResponseEventArgs);
            _globalFeignClientPipeline?.OnReceivingResponse(this, receivingResponseEventArgs);
            //if (receivingResponseEventArgs.Result != null)
            if (receivingResponseEventArgs._isSetResult)
            {
                return receivingResponseEventArgs.GetResult<TResult>();
            }
            #endregion
            EnsureSuccess(request, responseMessage);
            if (typeof(TResult) == typeof(Task))
            {
#if NET45
                return (TResult)(object)Task.FromResult<object>(null);
#endif
#if NETSTANDARD
                return (TResult)(object)Task.CompletedTask;
#endif
            }
            if (typeof(TResult) == typeof(string))
            {
                return (TResult)(object)responseMessage.Content.ReadAsStringAsync().GetResult();
            }
            IMediaTypeFormatter mediaTypeFormatter = _feignOptions.MediaTypeFormatters.FindFormatter(responseMessage.Content.Headers.ContentType?.MediaType);
            if (mediaTypeFormatter == null)
            {
                throw new FeignHttpRequestException(this,
                 responseMessage.RequestMessage as FeignHttpRequestMessage,
                 new HttpRequestException($"Content type '{responseMessage.Content.Headers.ContentType.ToString()}' not supported"));
            }
            return mediaTypeFormatter.GetResult<TResult>(responseMessage.Content.ReadAsByteArrayAsync().GetResult(), GetEncoding(responseMessage.Content.Headers.ContentType));
        }

        async Task<TResult> GetResultAsync<TResult>(FeignClientRequest request, HttpResponseMessage responseMessage)
        {
            if (responseMessage == null)
            {
                return default(TResult);
            }
            #region ReceivingResponse
            ReceivingResponseEventArgs<TResult> receivingResponseEventArgs = new ReceivingResponseEventArgs<TResult>(this, responseMessage);
            _globalFeignClientPipeline?.GetServicePipeline(this.ServiceId)?.OnReceivingResponse(this, receivingResponseEventArgs);
            _globalFeignClientPipeline?.OnReceivingResponse(this, receivingResponseEventArgs);
            //if (receivingResponseEventArgs.Result != null)
            if (receivingResponseEventArgs._isSetResult)
            {
                return receivingResponseEventArgs.GetResult<TResult>();
            }
            #endregion
            await EnsureSuccessAsync(request, responseMessage);
            if (typeof(TResult) == typeof(Task))
            {
#if NET45
                return (TResult)(object)Task.FromResult<object>(null);
#endif
#if NETSTANDARD
                return (TResult)(object)Task.CompletedTask;
#endif
            }
            if (typeof(TResult) == typeof(string))
            {
                return (TResult)(object)await responseMessage.Content.ReadAsStringAsync();
            }
            IMediaTypeFormatter mediaTypeFormatter = _feignOptions.MediaTypeFormatters.FindFormatter(responseMessage.Content.Headers.ContentType?.MediaType);
            if (mediaTypeFormatter == null)
            {
                throw new FeignHttpRequestException(this,
                     responseMessage.RequestMessage as FeignHttpRequestMessage,
                     new HttpRequestException($"Content type '{responseMessage.Content.Headers.ContentType.ToString()}' not supported"));
            }
            return mediaTypeFormatter.GetResult<TResult>(await responseMessage.Content.ReadAsByteArrayAsync(), GetEncoding(responseMessage.Content.Headers.ContentType));
        }

        Encoding GetEncoding(System.Net.Http.Headers.MediaTypeHeaderValue mediaTypeHeaderValue)
        {
            string charset = mediaTypeHeaderValue?.CharSet;

            // If we do have encoding information in the 'Content-Type' header, use that information to convert
            // the content to a string.
            if (charset != null)
            {
                try
                {
                    // Remove at most a single set of quotes.
                    if (charset.Length > 2 &&
                        charset[0] == '\"' &&
                        charset[charset.Length - 1] == '\"')
                    {
                        return Encoding.GetEncoding(charset.Substring(1, charset.Length - 2));
                    }
                    else
                    {
                        return Encoding.GetEncoding(charset);
                    }
                }
                catch (ArgumentException e)
                {
                    throw new InvalidOperationException("The character set provided in ContentType is invalid. Cannot read content as string using an invalid character set.", e);
                }
            }
            return null;
        }

        #endregion

        Task<HttpResponseMessage> SendAsyncInternal(FeignClientRequest request)
        {
            HttpMethod httpMethod = GetHttpMethod(request.Method);
            HttpRequestMessage httpRequestMessage = CreateRequestMessage(request, httpMethod, CreateUri(BuildUri(request.Uri)));
            // if support content
            if (httpMethod == HttpMethod.Post || httpMethod == HttpMethod.Put)
            {
                HttpContent httpContent = request.GetHttpContent(_feignOptions.MediaTypeFormatters);
                if (httpContent != null)
                {
                    httpRequestMessage.Content = httpContent;
                }
            }
            return HttpClient.SendAsync(httpRequestMessage);
        }

        private HttpMethod GetHttpMethod(string method)
        {
            HttpMethod httpMethod;
            switch (method.ToUpper())
            {
                case "GET":
                    httpMethod = HttpMethod.Get;
                    break;
                case "POST":
                    httpMethod = HttpMethod.Post;
                    break;
                case "PUT":
                    httpMethod = HttpMethod.Put;
                    break;
                case "DELETE":
                    httpMethod = HttpMethod.Delete;
                    break;
                case "HEAD":
                    httpMethod = HttpMethod.Head;
                    break;
                case "OPTIONS":
                    httpMethod = HttpMethod.Options;
                    break;
                case "TRACE":
                    httpMethod = HttpMethod.Trace;
                    break;
                default:
                    httpMethod = new HttpMethod(method);
                    break;
            }
            return httpMethod;
        }
        private HttpRequestMessage CreateRequestMessage(FeignClientRequest request, HttpMethod method, Uri uri)
        {
            FeignHttpRequestMessage requestMessage = new FeignHttpRequestMessage(request, method, uri);
            return requestMessage;
        }

        private Uri CreateUri(string uri) =>
            string.IsNullOrEmpty(uri) ? null : new Uri(uri, UriKind.RelativeOrAbsolute);

        string BuildUri(string uri)
        {
            if (uri.StartsWith("/"))
            {
                return BaseUrl + uri;
            }
            return BaseUrl + "/" + uri;
        }

        #region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                DisposingEventArgs disposingEventArgs = new DisposingEventArgs(this, disposing);
                _globalFeignClientPipeline?.InvokeDisposing(this, disposingEventArgs);
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)。
                }

                // TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // TODO: 将大型字段设置为 null。
                HttpClient.Dispose();
                disposedValue = true;
            }
        }

        // TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        //~FeignClientServiceBase()
        //{
        //    // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
        //    Dispose(false);
        //}

        // 添加此代码以正确实现可处置模式。
        void IDisposable.Dispose()
        {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(true);
            // TODO: 如果在以上内容中替代了终结器，则取消注释以下行。
            //GC.SuppressFinalize(this);
        }
        #endregion


        #region PathVariable
        protected string ReplacePathVariable<T>(string uri, string name, T value)
        {
            return FeignClientUtils.ReplacePathVariable<T>(_feignOptions.Converters, uri, name, value);
        }
        #endregion
        #region RequestParam
        protected string ReplaceRequestParam<T>(string uri, string name, T value)
        {
            return FeignClientUtils.ReplaceRequestParam<T>(_feignOptions.Converters, uri, name, value);
        }
        #endregion
        #region RequestQuery
        protected string ReplaceRequestQuery<T>(string uri, string name, T value)
        {
            return FeignClientUtils.ReplaceRequestQuery<T>(_feignOptions.Converters, uri, name, value);
        }
        #endregion

    }
}
