using Feign.Cache;
using Feign.Discovery;
using Feign.Fallback;
using Feign.Internal;
using Feign.Logging;
using Feign.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Feign.Proxy
{
    public abstract class FallbackFeignClientHttpProxy : FeignClientHttpProxy, IFallbackFeignClient
    {
        public FallbackFeignClientHttpProxy(IFeignOptions feignOptions, IServiceDiscovery serviceDiscovery, IServiceCacheProvider serviceCacheProvider, ILoggerFactory loggerFactory, object fallback) : base(feignOptions, serviceDiscovery, serviceCacheProvider, loggerFactory)
        {
            _fallback = fallback;
        }

        object _fallback;

        object IFallbackFeignClient.Fallback => _fallback;

        protected override bool IsResponseTerminatedRequest => false;

        #region Send Request

        #region define
        internal static readonly MethodInfo HTTP_SEND_GENERIC_METHOD_FALLBACK = typeof(FallbackFeignClientHttpProxy).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).Where(o => o.IsGenericMethod && o.GetParameters().Length == 2).FirstOrDefault(o => o.Name == "Send");
        internal static readonly MethodInfo HTTP_SEND_ASYNC_GENERIC_METHOD_FALLBACK = typeof(FallbackFeignClientHttpProxy).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).Where(o => o.IsGenericMethod && o.GetParameters().Length == 2).FirstOrDefault(o => o.Name == "SendAsync");

        internal static readonly MethodInfo HTTP_SEND_METHOD_FALLBACK = typeof(FallbackFeignClientHttpProxy).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).Where(o => !o.IsGenericMethod && o.GetParameters().Length == 2).FirstOrDefault(o => o.Name == "Send");
        internal static readonly MethodInfo HTTP_SEND_ASYNC_METHOD_FALLBACK = typeof(FallbackFeignClientHttpProxy).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).Where(o => !o.IsGenericMethod && o.GetParameters().Length == 2).FirstOrDefault(o => o.Name == "SendAsync");
        #endregion

        protected async Task SendAsync(FeignClientRequest request, Func<Task> fallback)
        {
            try
            {
                await SendAsync(request);
            }
            catch (TerminatedRequestException)
            {
                return;
            }
            catch (Exception)
            {
                if (fallback == null)
                {
                    throw;
                }
                if (InvokeFallbackRequestPipeline(request, fallback))
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
            catch (TerminatedRequestException)
            {
                return default(TResult);
            }
            catch (Exception)
            {
                if (fallback == null)
                {
                    throw;
                }
                if (InvokeFallbackRequestPipeline(request, fallback))
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
            catch (TerminatedRequestException)
            {
                return;
            }
            catch (Exception)
            {
                if (fallback == null)
                {
                    throw;
                }
                if (InvokeFallbackRequestPipeline(request, fallback))
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
            catch (TerminatedRequestException)
            {
                return default(TResult);
            }
            catch (Exception)
            {
                if (fallback == null)
                {
                    throw;
                }
                if (InvokeFallbackRequestPipeline(request, fallback))
                {
                    throw;
                }
                return fallback.Invoke();
            }
        }
        #endregion

        protected internal virtual void OnFallbackRequest(FallbackRequestEventArgs e)
        {
            _serviceIdFeignClientPipeline?.OnFallbackRequest(this, e);
            _globalFeignClientPipeline?.OnFallbackRequest(this, e);
        }

        bool InvokeFallbackRequestPipeline(FeignClientRequest request, Delegate @delegate)
        {
            IFallbackProxy fallbackProxy = @delegate.Target as IFallbackProxy;
            FallbackRequestEventArgs eventArgs;
            if (fallbackProxy == null)
            {
                //可能因为method parameters length=0 , 故没有生成匿名调用类
                eventArgs = new FallbackRequestEventArgs(this, request, _fallback, null, @delegate.Method);
            }
            else
            {
                eventArgs = new FallbackRequestEventArgs(this, request, _fallback, fallbackProxy, null);
            }
            OnFallbackRequest(eventArgs);
            return eventArgs.IsTerminated;
        }

    }
}
