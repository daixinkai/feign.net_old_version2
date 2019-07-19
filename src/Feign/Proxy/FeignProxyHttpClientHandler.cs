using Feign.Internal;
using Feign.Logging;
using Feign.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Feign.Proxy
{
    public class FeignProxyHttpClientHandler : HttpClientHandler
    {
        private readonly ILogger _logger;
        private FeignClientHttpProxy _feignClient;
        /// <summary>
        /// Initializes a new instance of the <see cref="FeignHttpClientHandler"/> class.
        /// </summary>
        public FeignProxyHttpClientHandler(FeignClientHttpProxy feignClient, ILogger logger)
        {
            _feignClient = feignClient;
            _logger = logger;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            FeignHttpRequestMessage feignRequest = request as FeignHttpRequestMessage;
            var current = request.RequestUri;
            try
            {

                #region BuildingRequest
                BuildingRequestEventArgs buildingArgs = new BuildingRequestEventArgs(_feignClient, request.Method.ToString(), request.RequestUri, new Dictionary<string, string>());
                _feignClient.OnBuildingRequest(buildingArgs);
                //request.Method = new HttpMethod(buildingArgs.Method);
                request.RequestUri = buildingArgs.RequestUri;
                if (buildingArgs.Headers != null && buildingArgs.Headers.Count > 0)
                {
                    foreach (var item in buildingArgs.Headers)
                    {
                        request.Headers.TryAddWithoutValidation(item.Key, item.Value);
                    }
                }
                #endregion
                request.RequestUri = LookupRequestUri(request.RequestUri);
                #region SendingRequest
                SendingRequestEventArgs sendingArgs = new SendingRequestEventArgs(_feignClient, feignRequest);
                _feignClient.OnSendingRequest(sendingArgs);
                if (sendingArgs.IsTerminated)
                {
                    throw new TerminatedRequestException();
                }
                request = sendingArgs.RequestMessage;
                if (request == null)
                {
                    _logger?.LogError($"SendingRequest is null;");
                    return new HttpResponseMessage(System.Net.HttpStatusCode.ExpectationFailed)
                    {
                        Content = new StringContent(""),
                        //Headers = new System.Net.Http.Headers.HttpResponseHeaders(),
                        RequestMessage = request
                    };
                }
                #endregion

                #region CannelRequest
                CancelRequestEventArgs cancelArgs = new CancelRequestEventArgs(_feignClient, cancellationToken);
                _feignClient.OnCancelRequest(cancelArgs);
                #endregion

                return await base.SendAsync(request, cancellationToken);
            }
            catch (Exception e)
            {
                if (!e.IsSkipLog())
                {
                    _logger?.LogError(e, "Exception during SendAsync()");
                }
                if (e is HttpRequestException)
                {
                    FeignHttpRequestException feignHttpRequestException = new FeignHttpRequestException(_feignClient, feignRequest, (HttpRequestException)e);
                    ExceptionDispatchInfo exceptionDispatchInfo = ExceptionDispatchInfo.Capture(feignHttpRequestException);
                    exceptionDispatchInfo.Throw();
                }
                throw;
            }
            finally
            {
                request.RequestUri = current;
            }
        }


        protected virtual Uri LookupRequestUri(Uri uri)
        {
            return uri;
        }

    }
}
