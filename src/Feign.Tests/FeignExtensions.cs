using Feign.Tests;
using System;
using System.Collections.Generic;
using System.Text;

namespace Feign.Tests
{
    public static class FeignExtensions
    {
        public static void ReceivingQueryResult(this IGlobalFeignClientPipelineBuilder globalFeignClient)
        {
            globalFeignClient.ReceivingResponse += (sender, e) =>
            {
                if (!typeof(QueryResult).IsAssignableFrom(e.ResultType))
                {
                    return;
                }
                if (e.ResultType == typeof(QueryResult))
                {
                    e.Result = new QueryResult()
                    {
                        StatusCode = e.ResponseMessage.StatusCode
                    };
                    return;
                }

                if (e.ResultType.IsGenericType && e.ResultType.GetGenericTypeDefinition() == typeof(QueryResult<>))
                {
                    QueryResult queryResult;
                    if (e.ResponseMessage.IsSuccessStatusCode)
                    {
                        string json = e.ResponseMessage.Content.ReadAsStringAsync().Result;
                        object data = Newtonsoft.Json.JsonConvert.DeserializeObject(json, e.ResultType.GetGenericArguments()[0]);
                        var constructor = e.ResultType.GetConstructor(new Type[] { typeof(object) });
                        queryResult = (QueryResult)constructor.Invoke(new object[] { data });
                    }
                    else
                    {
                        queryResult = (QueryResult)e.ResultType.GetConstructor(Type.EmptyTypes).Invoke(new object[0]);
                    }

                    queryResult.StatusCode = e.ResponseMessage.StatusCode;

                    e.Result = queryResult;

                }

            };
        }
    }
}
