﻿using Feign.Formatting;
using Feign.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Feign.Proxy
{
    partial class FeignClientHttpProxy
    {

        #region Define

        internal static readonly MethodInfo HTTP_SEND_GENERIC_METHOD = typeof(FeignClientHttpProxy).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).Where(o => o.IsGenericMethod).FirstOrDefault(o => o.Name == "Send");
        internal static readonly MethodInfo HTTP_SEND_ASYNC_GENERIC_METHOD = typeof(FeignClientHttpProxy).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).Where(o => o.IsGenericMethod).FirstOrDefault(o => o.Name == "SendAsync");

        internal static readonly MethodInfo HTTP_SEND_METHOD = typeof(FeignClientHttpProxy).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).Where(o => !o.IsGenericMethod).FirstOrDefault(o => o.Name == "Send");
        internal static readonly MethodInfo HTTP_SEND_ASYNC_METHOD = typeof(FeignClientHttpProxy).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).Where(o => !o.IsGenericMethod).FirstOrDefault(o => o.Name == "SendAsync");

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

        private HttpResponseMessage GetResponseMessage(FeignClientRequest request)
        {
            try
            {
                return SendAsyncInternal(request).GetResult();
            }
            catch (TerminatedRequestException)
            {
                if (IsResponseTerminatedRequest)
                {
                    return null;
                }
                throw;
            }
            catch (Exception ex)
            {
                #region ErrorRequest
                ErrorRequestEventArgs errorArgs = new ErrorRequestEventArgs(this, ex);
                OnErrorRequest(errorArgs);
                if (errorArgs.ExceptionHandled)
                {
                    return null;
                }
                #endregion
                throw;
            }
        }

        private async Task<HttpResponseMessage> GetResponseMessageAsync(FeignClientRequest request)
        {
            try
            {
                return await SendAsyncInternal(request);
            }
            catch (TerminatedRequestException)
            {
                if (IsResponseTerminatedRequest)
                {
                    return null;
                }
                throw;
            }
            catch (Exception ex)
            {
                #region ErrorRequest
                ErrorRequestEventArgs errorArgs = new ErrorRequestEventArgs(this, ex);
                OnErrorRequest(errorArgs);
                if (errorArgs.ExceptionHandled)
                {
                    return null;
                }
                #endregion
                throw;
            }
        }

        private void EnsureSuccess(FeignClientRequest request, HttpResponseMessage responseMessage)
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

        private async Task EnsureSuccessAsync(FeignClientRequest request, HttpResponseMessage responseMessage)
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

        private TResult GetResult<TResult>(FeignClientRequest request, HttpResponseMessage responseMessage)
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
            return mediaTypeFormatter.GetResult<TResult>(responseMessage.Content.ReadAsByteArrayAsync().GetResult(), FeignClientUtils.GetEncoding(responseMessage.Content.Headers.ContentType));
        }

        private async Task<TResult> GetResultAsync<TResult>(FeignClientRequest request, HttpResponseMessage responseMessage)
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
            return mediaTypeFormatter.GetResult<TResult>(await responseMessage.Content.ReadAsByteArrayAsync(), FeignClientUtils.GetEncoding(responseMessage.Content.Headers.ContentType));
        }



        private Task<HttpResponseMessage> SendAsyncInternal(FeignClientRequest request)
        {
            HttpMethod httpMethod = GetHttpMethod(request.HttpMethod);
            HttpRequestMessage httpRequestMessage = CreateRequestMessage(request, httpMethod, CreateUri(BuildUri(request.Uri)));
            // if support content
            if (httpMethod == HttpMethod.Post || httpMethod == HttpMethod.Put)
            {
                HttpContent httpContent = request.GetHttpContent();
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

        private string BuildUri(string uri)
        {
            if (uri.StartsWith("/"))
            {
                return BaseUrl + uri;
            }
            return BaseUrl + "/" + uri;
        }

    }
}
