using Feign.Cache;
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
    public abstract class FallbackFeignClientProxyService : FeignClientProxyService
    {
        public FallbackFeignClientProxyService(IFeignOptions feignOptions, IServiceDiscovery serviceDiscovery, IServiceCacheProvider serviceCacheProvider, ILoggerFactory loggerFactory) : base(feignOptions, serviceDiscovery, serviceCacheProvider, loggerFactory)
        {
        }
        #region HttpMethod

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

        #region get
        protected virtual TResult Get<TResult>(string uri, Func<TResult> fallback)
        {
            try
            {
                return Get<TResult>(uri);
            }
            catch (Exception)
            {
                if (fallback != null)
                {
                    return fallback.Invoke();
                }
                throw;
            }
        }
        protected virtual void Get(string uri, Action fallback)
        {
            try
            {
                Get(uri);
            }
            catch (Exception)
            {
                if (fallback != null)
                {
                    fallback.Invoke();
                    return;
                }
                throw;
            }
        }
        protected virtual async Task<TResult> GetAsync<TResult>(string uri, Func<Task<TResult>> fallback)
        {
            try
            {
                return await GetAsync<TResult>(uri);
            }
            catch (Exception)
            {
                if (fallback != null)
                {
                    return await fallback.Invoke();
                }
                throw;
            }
        }
        protected virtual async Task GetAsync(string uri, Func<Task> fallback)
        {
            try
            {
                await GetAsync(uri);
            }
            catch (Exception)
            {
                if (fallback != null)
                {
                    await fallback.Invoke();
                    return;
                }
                throw;
            }
        }

        #endregion

        #region post
        protected virtual TResult Post<TResult>(string uri, object value, Func<TResult> fallback)
        {
            try
            {
                return Post<TResult>(uri, value);
            }
            catch (Exception)
            {
                if (fallback != null)
                {
                    return fallback.Invoke();
                }
                throw;
            }
        }

        protected virtual void Post(string uri, object value, Action fallback)
        {
            try
            {
                Post(uri, value);
            }
            catch (Exception)
            {
                if (fallback != null)
                {
                    fallback.Invoke();
                    return;
                }
                throw;
            }
        }

        protected virtual async Task<TResult> PostAsync<TResult>(string uri, object value, Func<Task<TResult>> fallback)
        {
            try
            {
                return await PostAsync<TResult>(uri, value);
            }
            catch (Exception)
            {
                if (fallback != null)
                {
                    return await fallback.Invoke();
                }
                throw;
            }
        }
        protected virtual async Task PostAsync(string uri, object value, Func<Task> fallback)
        {
            try
            {
                await PostAsync(uri, value);
            }
            catch (Exception)
            {
                if (fallback != null)
                {
                    await fallback.Invoke();
                    return;
                }
                throw;
            }
        }
        #endregion

        #region put
        protected virtual TResult Put<TResult>(string uri, object value, Func<TResult> fallback)
        {
            try
            {
                return Put<TResult>(uri, value);
            }
            catch (Exception)
            {
                if (fallback != null)
                {
                    return fallback.Invoke();
                }
                throw;
            }
        }
        protected virtual void Put(string uri, object value, Action fallback)
        {
            try
            {
                Put(uri, value);
            }
            catch (Exception)
            {
                if (fallback != null)
                {
                    fallback.Invoke();
                    return;
                }
                throw;
            }
        }
        protected virtual async Task<TResult> PutAsync<TResult>(string uri, object value, Func<Task<TResult>> fallback)
        {
            try
            {
                return await PutAsync<TResult>(uri, value);
            }
            catch (Exception)
            {
                if (fallback != null)
                {
                    return await fallback.Invoke();
                }
                throw;
            }
        }
        protected virtual async Task PutAsync(string uri, object value, Func<Task> fallback)
        {
            try
            {
                await PutAsync(uri, value);
            }
            catch (Exception)
            {
                if (fallback != null)
                {
                    await fallback.Invoke();
                    return;
                }
                throw;
            }
        }
        #endregion

        #region delete
        protected virtual TResult Delete<TResult>(string uri, Func<TResult> fallback)
        {
            try
            {
                return Delete<TResult>(uri);
            }
            catch (Exception)
            {
                if (fallback != null)
                {
                    return fallback.Invoke();
                }
                throw;
            }
        }
        protected virtual void Delete(string uri, Action fallback)
        {
            try
            {
                Delete(uri);
            }
            catch (Exception)
            {
                if (fallback != null)
                {
                    fallback.Invoke();
                    return;
                }
                throw;
            }
        }
        protected virtual async Task<TResult> DeleteAsync<TResult>(string uri, Func<Task<TResult>> fallback)
        {
            try
            {
                return await DeleteAsync<TResult>(uri);
            }
            catch (Exception)
            {
                if (fallback != null)
                {
                    return await fallback.Invoke();
                }
                throw;
            }
        }
        protected virtual async Task DeleteAsync(string uri, Func<Task> fallback)
        {
            try
            {
                await DeleteAsync(uri);
            }
            catch (Exception)
            {
                if (fallback != null)
                {
                    await fallback.Invoke();
                    return;
                }
                throw;
            }
        }
        #endregion

        #endregion
        
    }
}
