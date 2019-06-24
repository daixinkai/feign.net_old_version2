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

        #region Send Request

        #region define
        internal static readonly MethodInfo HTTP_SEND_GENERIC_METHOD_FALLBACK = typeof(FallbackFeignClientProxyService).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).Where(o => o.IsGenericMethod && o.GetParameters().Length == 2).FirstOrDefault(o => o.Name == "Send");
        internal static readonly MethodInfo HTTP_SEND_ASYNC_GENERIC_METHOD_FALLBACK = typeof(FallbackFeignClientProxyService).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).Where(o => o.IsGenericMethod && o.GetParameters().Length == 2).FirstOrDefault(o => o.Name == "SendAsync");

        internal static readonly MethodInfo HTTP_SEND_METHOD_FALLBACK = typeof(FallbackFeignClientProxyService).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).Where(o => !o.IsGenericMethod && o.GetParameters().Length == 2).FirstOrDefault(o => o.Name == "Send");
        internal static readonly MethodInfo HTTP_SEND_ASYNC_METHOD_FALLBACK = typeof(FallbackFeignClientProxyService).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).Where(o => !o.IsGenericMethod && o.GetParameters().Length == 2).FirstOrDefault(o => o.Name == "SendAsync");
        #endregion

        protected async Task SendAsync(FeignClientRequest request, Func<Task> fallback)
        {
            try
            {
                await SendAsync(request);
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
        protected async Task<TResult> SendAsync<TResult>(FeignClientRequest request, Func<Task<TResult>> fallback)
        {
            try
            {
                return await SendAsync<TResult>(request);
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
        protected void Send(FeignClientRequest request, Action fallback)
        {
            try
            {
                Send(request);
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
        protected TResult Send<TResult>(FeignClientRequest request, Func<TResult> fallback)
        {
            try
            {
                return Send<TResult>(request);
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
        #endregion

    }
}
