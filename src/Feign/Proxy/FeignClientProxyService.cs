using Feign.Cache;
using Feign.Discovery;
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
            _globalFeignClientPipeline?.GetServicePipeline(this.ServiceId)?.OnInitializing(this, initializingEventArgs);
            _globalFeignClientPipeline?.OnInitializing(this, initializingEventArgs);

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

        GlobalFeignClientPipelineBuilder _globalFeignClientPipeline;

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


        HttpContent GetHttpContent(FeignClientRequest request)
        {
            if (request.Content != null)
            {
                return new ObjectStringContent(request.Content);
            }
            return null;
        }

        HttpResponseMessage GetResponseMessage(FeignClientRequest request)
        {
            try
            {
                return SendAsync(request, GetHttpContent(request)).GetResult();
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
                _globalFeignClientPipeline?.GetServicePipeline(this.ServiceId)?.OnErrorRequest(this, errorArgs);
                if (!errorArgs.ExceptionHandled)
                {
                    _globalFeignClientPipeline?.OnErrorRequest(this, errorArgs);
                }
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
                return await SendAsync(request, GetHttpContent(request));
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
                _globalFeignClientPipeline?.GetServicePipeline(this.ServiceId)?.OnErrorRequest(this, errorArgs);
                if (!errorArgs.ExceptionHandled)
                {
                    _globalFeignClientPipeline?.OnErrorRequest(this, errorArgs);
                }
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
            if (receivingResponseEventArgs.Result != null)
            {
                return (TResult)receivingResponseEventArgs.Result;
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
            string text = responseMessage.Content.ReadAsStringAsync().GetResult();
            if (typeof(TResult) == typeof(string))
            {
                return (TResult)(object)text;
            }
            return Newtonsoft.Json.JsonConvert.DeserializeObject<TResult>(text);
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
            if (receivingResponseEventArgs.Result != null)
            {
                return (TResult)receivingResponseEventArgs.Result;
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
            string text = await responseMessage.Content.ReadAsStringAsync();
            if (typeof(TResult) == typeof(string))
            {
                return (TResult)(object)text;
            }
            return Newtonsoft.Json.JsonConvert.DeserializeObject<TResult>(text);
        }

        #endregion

        Task<HttpResponseMessage> SendAsync(FeignClientRequest request, HttpContent httpContent)
        {
            HttpMethod httpMethod = GetHttpMethod(request.Method);
            HttpRequestMessage httpRequestMessage = CreateRequestMessage(request, httpMethod, CreateUri(BuildUri(request.Uri)));
            if (httpContent != null)
            {
                // if support content
                if (httpMethod == HttpMethod.Post || httpMethod == HttpMethod.Put)
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
        private HttpRequestMessage CreateRequestMessage(FeignClientRequest request, HttpMethod method, Uri uri) =>
            new FeignHttpRequestMessage(request, method, uri);
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
                _globalFeignClientPipeline?.GetServicePipeline(this.ServiceId)?.OnDisposing(this, disposingEventArgs);
                _globalFeignClientPipeline?.OnDisposing(this, disposingEventArgs);
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
