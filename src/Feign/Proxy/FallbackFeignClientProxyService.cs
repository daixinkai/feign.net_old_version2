using Feign.Cache;
using Feign.Discovery;
using Feign.Fallback;
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
    public abstract class FallbackFeignClientProxyService : FeignClientProxyService, IFallbackFeignClient
    {
        public FallbackFeignClientProxyService(IFeignOptions feignOptions, IServiceDiscovery serviceDiscovery, IServiceCacheProvider serviceCacheProvider, ILoggerFactory loggerFactory, object fallback) : base(feignOptions, serviceDiscovery, serviceCacheProvider, loggerFactory)
        {
            _fallback = fallback;
        }

        object _fallback;

        object IFallbackFeignClient.Fallback => _fallback;

        protected override bool IsResponseSuspendedRequest => false;

        #region HttpMethod

        #region define
        #region get
        internal static readonly MethodInfo HTTP_GET_GENERIC_METHOD_FALLBACK = typeof(FallbackFeignClientProxyService).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).Where(o => o.IsGenericMethod && o.GetParameters().Length == 2).FirstOrDefault(o => o.Name == "Get");
        internal static readonly MethodInfo HTTP_GET_ASYNC_GENERIC_METHOD_FALLBACK = typeof(FallbackFeignClientProxyService).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).Where(o => o.IsGenericMethod && o.GetParameters().Length == 2).FirstOrDefault(o => o.Name == "GetAsync");

        internal static readonly MethodInfo HTTP_GET_METHOD_FALLBACK = typeof(FallbackFeignClientProxyService).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).Where(o => !o.IsGenericMethod && o.GetParameters().Length == 2).FirstOrDefault(o => o.Name == "Get");
        internal static readonly MethodInfo HTTP_GET_ASYNC_METHOD_FALLBACK = typeof(FallbackFeignClientProxyService).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).Where(o => !o.IsGenericMethod && o.GetParameters().Length == 2).FirstOrDefault(o => o.Name == "GetAsync");
        #endregion

        #region post
        internal static readonly MethodInfo HTTP_POST_GENERIC_METHOD_FALLBACK = typeof(FallbackFeignClientProxyService).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).Where(o => o.IsGenericMethod && o.GetParameters().Length == 3).FirstOrDefault(o => o.Name == "Post");
        internal static readonly MethodInfo HTTP_POST_ASYNC_GENERIC_METHOD_FALLBACK = typeof(FallbackFeignClientProxyService).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).Where(o => o.IsGenericMethod && o.GetParameters().Length == 3).FirstOrDefault(o => o.Name == "PostAsync");

        internal static readonly MethodInfo HTTP_POST_METHOD_FALLBACK = typeof(FallbackFeignClientProxyService).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).Where(o => !o.IsGenericMethod && o.GetParameters().Length == 3).FirstOrDefault(o => o.Name == "Post");
        internal static readonly MethodInfo HTTP_POST_ASYNC_METHOD_FALLBACK = typeof(FallbackFeignClientProxyService).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).Where(o => !o.IsGenericMethod && o.GetParameters().Length == 3).FirstOrDefault(o => o.Name == "PostAsync");
        #endregion

        #region put
        internal static readonly MethodInfo HTTP_PUT_GENERIC_METHOD_FALLBACK = typeof(FallbackFeignClientProxyService).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).Where(o => o.IsGenericMethod && o.GetParameters().Length == 3).FirstOrDefault(o => o.Name == "Put");
        internal static readonly MethodInfo HTTP_PUT_ASYNC_GENERIC_METHOD_FALLBACK = typeof(FallbackFeignClientProxyService).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).Where(o => o.IsGenericMethod && o.GetParameters().Length == 3).FirstOrDefault(o => o.Name == "PutAsync");

        internal static readonly MethodInfo HTTP_PUT_METHOD_FALLBACK = typeof(FallbackFeignClientProxyService).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).Where(o => !o.IsGenericMethod && o.GetParameters().Length == 3).FirstOrDefault(o => o.Name == "Put");
        internal static readonly MethodInfo HTTP_PUT_ASYNC_METHOD_FALLBACK = typeof(FallbackFeignClientProxyService).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).Where(o => !o.IsGenericMethod && o.GetParameters().Length == 3).FirstOrDefault(o => o.Name == "PutAsync");
        #endregion

        #region delete
        internal static readonly MethodInfo HTTP_DELETE_GENERIC_METHOD_FALLBACK = typeof(FallbackFeignClientProxyService).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).Where(o => o.IsGenericMethod && o.GetParameters().Length == 2).FirstOrDefault(o => o.Name == "Delete");
        internal static readonly MethodInfo HTTP_DELETE_ASYNC_GENERIC_METHOD_FALLBACK = typeof(FallbackFeignClientProxyService).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).Where(o => o.IsGenericMethod && o.GetParameters().Length == 2).FirstOrDefault(o => o.Name == "DeleteAsync");

        internal static readonly MethodInfo HTTP_DELETE_METHOD_FALLBACK = typeof(FallbackFeignClientProxyService).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).Where(o => !o.IsGenericMethod && o.GetParameters().Length == 2).FirstOrDefault(o => o.Name == "Delete");
        internal static readonly MethodInfo HTTP_DELETE_ASYNC_METHOD_FALLBACK = typeof(FallbackFeignClientProxyService).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).Where(o => !o.IsGenericMethod && o.GetParameters().Length == 2).FirstOrDefault(o => o.Name == "DeleteAsync");
        #endregion

        #endregion


        #region get
        protected virtual TResult Get<TResult>(string uri, Func<TResult> fallback)
        {
            return Invoke(() => Get<TResult>(uri), fallback);
        }
        protected virtual void Get(string uri, Action fallback)
        {
            Invoke(() => Get(uri), fallback);
        }
        protected virtual Task<TResult> GetAsync<TResult>(string uri, Func<Task<TResult>> fallback)
        {
            return InvokeAsync(() => GetAsync<TResult>(uri), fallback);
        }
        protected virtual Task GetAsync(string uri, Func<Task> fallback)
        {
            return InvokeAsync(() => GetAsync(uri), fallback);
        }

        #endregion

        #region post
        protected virtual TResult Post<TResult>(string uri, object value, Func<TResult> fallback)
        {
            return Invoke(() => Post<TResult>(uri, value), fallback);
        }

        protected virtual void Post(string uri, object value, Action fallback)
        {
            Invoke(() => Post(uri, value), fallback);
        }

        protected virtual Task<TResult> PostAsync<TResult>(string uri, object value, Func<Task<TResult>> fallback)
        {
            return InvokeAsync(() => PostAsync<TResult>(uri, value), fallback);
        }
        protected virtual Task PostAsync(string uri, object value, Func<Task> fallback)
        {
            return InvokeAsync(() => PostAsync(uri, value), fallback);
        }
        #endregion

        #region put
        protected virtual TResult Put<TResult>(string uri, object value, Func<TResult> fallback)
        {
            return Invoke(() => Put<TResult>(uri, value), fallback);
        }

        protected virtual void Put(string uri, object value, Action fallback)
        {
            Invoke(() => Put(uri, value), fallback);
        }

        protected virtual Task<TResult> PutAsync<TResult>(string uri, object value, Func<Task<TResult>> fallback)
        {
            return InvokeAsync(() => PutAsync<TResult>(uri, value), fallback);
        }
        protected virtual Task PutAsync(string uri, object value, Func<Task> fallback)
        {
            return InvokeAsync(() => PutAsync(uri, value), fallback);
        }
        #endregion

        #region delete
        protected virtual TResult Delete<TResult>(string uri, Func<TResult> fallback)
        {
            return Invoke(() => Delete<TResult>(uri), fallback);
        }
        protected virtual void Delete(string uri, Action fallback)
        {
            Invoke(() => Delete(uri), fallback);
        }
        protected virtual Task<TResult> DeleteAsync<TResult>(string uri, Func<Task<TResult>> fallback)
        {
            return InvokeAsync(() => DeleteAsync<TResult>(uri), fallback);
        }
        protected virtual Task DeleteAsync(string uri, Func<Task> fallback)
        {
            return InvokeAsync(() => DeleteAsync(uri), fallback);
        }
        #endregion

        void Invoke(Action action, Action fallback)
        {
            try
            {
                action();
            }
            catch (SuspendedRequestException)
            {
                return;
            }
            catch (Exception)
            {
                if (fallback == null)
                {
                    throw;
                }
                fallback.Invoke();
            }
        }
        TResult Invoke<TResult>(Func<TResult> action, Func<TResult> fallback)
        {
            try
            {
                return action();
            }
            catch (SuspendedRequestException)
            {
                return default(TResult);
            }
            catch (Exception)
            {
                if (fallback == null)
                {
                    throw;
                }
                return fallback.Invoke();
            }
        }
        async Task InvokeAsync(Func<Task> action, Func<Task> fallback)
        {
            try
            {
                await action();
            }
            catch (SuspendedRequestException)
            {
                return;
            }
            catch (Exception)
            {
                if (fallback == null)
                {
                    throw;
                }
                await fallback.Invoke();
            }
        }
        async Task<TResult> InvokeAsync<TResult>(Func<Task<TResult>> action, Func<Task<TResult>> fallback)
        {
            try
            {
                return await action();
            }
            catch (SuspendedRequestException)
            {
                return default(TResult);
            }
            catch (Exception)
            {
                if (fallback == null)
                {
                    throw;
                }
                return await fallback.Invoke();
            }
        }

        #endregion

    }
}
