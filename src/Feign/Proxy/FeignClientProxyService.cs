﻿using Feign.Cache;
using Feign.Discovery;
using Feign.Internal;
using Feign.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
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
            _globalFeignClientPipeline = _feignOptions?.FeignClientPipeline as GlobalFeignClientPipelineBuilder; ;
            ServiceDiscoveryHttpClientHandler serviceDiscoveryHttpClientHandler = new ServiceDiscoveryHttpClientHandler(serviceDiscovery, this, _globalFeignClientPipeline, serviceCacheProvider, _logger);
            serviceDiscoveryHttpClientHandler.ShouldResolveService = string.IsNullOrWhiteSpace(Url);
            serviceDiscoveryHttpClientHandler.AllowAutoRedirect = false;
            _httpClient = new HttpClient(serviceDiscoveryHttpClientHandler);
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
            _baseUrl = baseUrl;
        }


        public abstract string ServiceId { get; }




        public virtual string BaseUri { get { return null; } }

        public virtual string Url { get { return null; } }

        string _baseUrl;

        ILogger _logger;

        GlobalFeignClientPipelineBuilder _globalFeignClientPipeline;

        IFeignOptions _feignOptions;

        HttpClient _httpClient;

        protected HttpClient HttpClient
        {
            get
            {
                return _httpClient;
            }
        }


        #region HttpMethod

        #region get
        internal static readonly MethodInfo HTTP_GET_GENERIC_METHOD = typeof(FeignClientProxyService).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).Where(o => o.IsGenericMethod).FirstOrDefault(o => o.Name == "Get");
        internal static readonly MethodInfo HTTP_GET_ASYNC_GENERIC_METHOD = typeof(FeignClientProxyService).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).Where(o => o.IsGenericMethod).FirstOrDefault(o => o.Name == "GetAsync");

        internal static readonly MethodInfo HTTP_GET_METHOD = typeof(FeignClientProxyService).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).Where(o => !o.IsGenericMethod).FirstOrDefault(o => o.Name == "Get");
        internal static readonly MethodInfo HTTP_GET_ASYNC_METHOD = typeof(FeignClientProxyService).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).Where(o => !o.IsGenericMethod).FirstOrDefault(o => o.Name == "GetAsync");
        #endregion

        #region post
        internal static readonly MethodInfo HTTP_POST_GENERIC_METHOD = typeof(FeignClientProxyService).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).Where(o => o.IsGenericMethod).FirstOrDefault(o => o.Name == "Post");
        internal static readonly MethodInfo HTTP_POST_ASYNC_GENERIC_METHOD = typeof(FeignClientProxyService).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).Where(o => o.IsGenericMethod).FirstOrDefault(o => o.Name == "PostAsync");

        internal static readonly MethodInfo HTTP_POST_METHOD = typeof(FeignClientProxyService).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).Where(o => !o.IsGenericMethod).FirstOrDefault(o => o.Name == "Post");
        internal static readonly MethodInfo HTTP_POST_ASYNC_METHOD = typeof(FeignClientProxyService).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).Where(o => !o.IsGenericMethod).FirstOrDefault(o => o.Name == "PostAsync");
        #endregion

        #region put
        internal static readonly MethodInfo HTTP_PUT_GENERIC_METHOD = typeof(FeignClientProxyService).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).Where(o => o.IsGenericMethod).FirstOrDefault(o => o.Name == "Put");
        internal static readonly MethodInfo HTTP_PUT_ASYNC_GENERIC_METHOD = typeof(FeignClientProxyService).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).Where(o => o.IsGenericMethod).FirstOrDefault(o => o.Name == "PutAsync");

        internal static readonly MethodInfo HTTP_PUT_METHOD = typeof(FeignClientProxyService).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).Where(o => !o.IsGenericMethod).FirstOrDefault(o => o.Name == "Put");
        internal static readonly MethodInfo HTTP_PUT_ASYNC_METHOD = typeof(FeignClientProxyService).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).Where(o => !o.IsGenericMethod).FirstOrDefault(o => o.Name == "PutAsync");
        #endregion

        #region delete
        internal static readonly MethodInfo HTTP_DELETE_GENERIC_METHOD = typeof(FeignClientProxyService).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).Where(o => o.IsGenericMethod).FirstOrDefault(o => o.Name == "Delete");
        internal static readonly MethodInfo HTTP_DELETE_ASYNC_GENERIC_METHOD = typeof(FeignClientProxyService).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).Where(o => o.IsGenericMethod).FirstOrDefault(o => o.Name == "DeleteAsync");

        internal static readonly MethodInfo HTTP_DELETE_METHOD = typeof(FeignClientProxyService).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).Where(o => !o.IsGenericMethod).FirstOrDefault(o => o.Name == "Delete");
        internal static readonly MethodInfo HTTP_DELETE_ASYNC_METHOD = typeof(FeignClientProxyService).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).Where(o => !o.IsGenericMethod).FirstOrDefault(o => o.Name == "DeleteAsync");
        #endregion


        protected string BuildUri(string uri)
        {
            if (uri.StartsWith("/"))
            {
                return _baseUrl + uri;
            }
            return _baseUrl + "/" + uri;
        }

        #region get
        protected virtual void Get(string uri)
        {
            HttpResponseMessage response = GetResponseMessage(() => _httpClient.GetAsync(BuildUri(uri)).GetResult());
            GetResult<string>(response);
        }
        protected virtual TResult Get<TResult>(string uri)
        {
            HttpResponseMessage response = GetResponseMessage(() => _httpClient.GetAsync(BuildUri(uri)).GetResult());
            return GetResult<TResult>(response);
        }
        protected virtual async Task<TResult> GetAsync<TResult>(string uri)
        {
            HttpResponseMessage response = await GetResponseMessageAsync(() => _httpClient.GetAsync(BuildUri(uri))); ;
            return await GetResultAsync<TResult>(response);
        }

        protected virtual async Task GetAsync(string uri)
        {
            HttpResponseMessage response = await GetResponseMessageAsync(() => _httpClient.GetAsync(BuildUri(uri))); ;
            await GetResultAsync<string>(response);
        }
        #endregion

        #region post
        protected virtual TResult Post<TResult>(string uri, object value)
        {
            using (HttpContent httpContent = new ObjectStringContent(value))
            {
                HttpResponseMessage response = PostMessage(_httpClient, BuildUri(uri), value);
                return GetResult<TResult>(response);
            }
        }
        protected virtual void Post(string uri, object value)
        {
            using (HttpContent httpContent = new ObjectStringContent(value))
            {
                HttpResponseMessage response = PostMessage(_httpClient, BuildUri(uri), value);
                GetResult<string>(response);
            }
        }
        protected virtual async Task<TResult> PostAsync<TResult>(string uri, object value)
        {
            using (HttpContent httpContent = new ObjectStringContent(value))
            {
                HttpResponseMessage response = await PostMessageAsync(_httpClient, BuildUri(uri), value);
                return await GetResultAsync<TResult>(response);
            }
        }

        protected virtual async Task PostAsync(string uri, object value)
        {
            using (HttpContent httpContent = new ObjectStringContent(value))
            {
                HttpResponseMessage response = await PostMessageAsync(_httpClient, BuildUri(uri), value);
                await GetResultAsync<string>(response);
            }
        }

        HttpResponseMessage PostMessage(HttpClient httpClient, string uri, object value)
        {
            if (value is HttpContent)
            {
                return GetResponseMessage(() => httpClient.PostAsync(uri, (HttpContent)value).GetResult());
            }
            else
            {
                return GetResponseMessage(() => httpClient.PostAsync(uri, new ObjectContent(value)).GetResult());
            }
        }

        Task<HttpResponseMessage> PostMessageAsync(HttpClient httpClient, string uri, object value)
        {
            if (value is HttpContent)
            {
                return GetResponseMessageAsync(() => httpClient.PostAsync(uri, (HttpContent)value));
            }
            else
            {
                return GetResponseMessageAsync(() => httpClient.PostAsync(uri, new ObjectContent(value)));
            }
        }

        #endregion

        #region put
        protected virtual TResult Put<TResult>(string uri, object value)
        {
            using (HttpContent httpContent = new ObjectStringContent(value))
            {
                HttpResponseMessage response = PutMessage(_httpClient, BuildUri(uri), value);
                return GetResult<TResult>(response);
            }
        }
        protected virtual void Put(string uri, object value)
        {
            using (HttpContent httpContent = new ObjectStringContent(value))
            {
                HttpResponseMessage response = PutMessage(_httpClient, BuildUri(uri), value);
                GetResult<string>(response);
            }
        }
        protected virtual async Task<TResult> PutAsync<TResult>(string uri, object value)
        {
            using (HttpContent httpContent = new ObjectStringContent(value))
            {
                HttpResponseMessage response = await PutMessageAsync(_httpClient, BuildUri(uri), value);
                return await GetResultAsync<TResult>(response);
            }
        }

        protected virtual async Task PutAsync(string uri, object value)
        {
            using (HttpContent httpContent = new ObjectStringContent(value))
            {
                HttpResponseMessage response = await PutMessageAsync(_httpClient, BuildUri(uri), value);
                await GetResultAsync<string>(response);
            }
        }

        HttpResponseMessage PutMessage(HttpClient httpClient, string uri, object value)
        {
            if (value is HttpContent)
            {
                return GetResponseMessage(() => httpClient.PutAsync(uri, (HttpContent)value).GetResult());
            }
            else
            {
                return GetResponseMessage(() => httpClient.PutAsync(uri, new ObjectContent(value)).GetResult());
            }
        }

        Task<HttpResponseMessage> PutMessageAsync(HttpClient httpClient, string uri, object value)
        {
            if (value is HttpContent)
            {
                return GetResponseMessageAsync(() => httpClient.PostAsync(uri, (HttpContent)value));
            }
            else
            {
                return GetResponseMessageAsync(() => httpClient.PostAsync(uri, new ObjectContent(value)));
            }
        }

        #endregion

        #region delete
        protected virtual TResult Delete<TResult>(string uri)
        {
            HttpResponseMessage response = GetResponseMessage(() => _httpClient.DeleteAsync(BuildUri(uri)).GetResult());
            return GetResult<TResult>(response);
        }
        protected virtual void Delete(string uri)
        {
            HttpResponseMessage response = GetResponseMessage(() => _httpClient.DeleteAsync(BuildUri(uri)).GetResult());
            GetResult<string>(response);
        }

        protected virtual async Task<TResult> DeleteAsync<TResult>(string uri)
        {
            HttpResponseMessage response = await GetResponseMessageAsync(() => _httpClient.DeleteAsync(BuildUri(uri)));
            return await GetResultAsync<TResult>(response);
        }
        protected virtual async Task DeleteAsync(string uri)
        {
            HttpResponseMessage response = await GetResponseMessageAsync(() => _httpClient.DeleteAsync(BuildUri(uri)));
            await GetResultAsync<string>(response);
        }
        #endregion

        HttpResponseMessage GetResponseMessage(Func<HttpResponseMessage> action)
        {
            try
            {
                return action();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Exception during SendAsync()");
                #region ErrorRequest
                ErrorRequestEventArgs errorArgs = new ErrorRequestEventArgs(this, ex);
                _globalFeignClientPipeline.GetServicePipeline(this.ServiceId)?.OnErrorRequest(this, errorArgs);
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

        async Task<HttpResponseMessage> GetResponseMessageAsync(Func<Task<HttpResponseMessage>> action)
        {
            try
            {
                return await action();
            }
            catch (Exception ex)
            {
                #region ErrorRequest
                ErrorRequestEventArgs errorArgs = new ErrorRequestEventArgs(this, ex);
                _globalFeignClientPipeline.GetServicePipeline(this.ServiceId)?.OnErrorRequest(this, errorArgs);
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

        void EnsureSuccess(HttpResponseMessage responseMessage)
        {
            if (!responseMessage.IsSuccessStatusCode)
            {
                string content = responseMessage.Content.ReadAsStringAsync().GetResult();
                _logger?.LogError($"request on \"{responseMessage.RequestMessage.RequestUri.ToString()}\" status code : " + responseMessage.StatusCode.GetHashCode() + " content : " + content);
            }
            responseMessage.EnsureSuccessStatusCode();
        }

        TResult GetResult<TResult>(HttpResponseMessage responseMessage)
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
            EnsureSuccess(responseMessage);
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

        async Task<TResult> GetResultAsync<TResult>(HttpResponseMessage responseMessage)
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
            EnsureSuccess(responseMessage);
#if NET45
            return (TResult)(object)Task.FromResult<object>(null);
#endif
#if NETSTANDARD
            return (TResult)(object)Task.CompletedTask;
#endif
            string text = await responseMessage.Content.ReadAsStringAsync();
            if (typeof(TResult) == typeof(string))
            {
                return (TResult)(object)text;
            }
            return Newtonsoft.Json.JsonConvert.DeserializeObject<TResult>(text);
        }

        #endregion



        #region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)。
                }

                // TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // TODO: 将大型字段设置为 null。
                _httpClient.Dispose();
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
