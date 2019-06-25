using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Feign.Tests
{
    [CustomFeignClient("yun-platform-service-provider"
        , Fallback = typeof(TestServiceFallback)
        //, FallbackFactory = typeof(TestServiceFallbackFactory)
        //, Url = "http://localhost:8802/"
        //, Url = "http://10.1.5.90:8802/"
        )]
    [RequestMapping("/organizations")]
    public interface ITestService
    {

        //string Name { get; }

        [RequestMapping("/{id}/asdasdsad", Method = "POST")]
        Task PostValueAsync();

        [RequestMapping("/{id}", Method = "GET")]
        Task<QueryResult<JObject>> GetQueryResultValueAsync([PathVariable("id")]string id, [RequestQuery] TestServiceParam param);

        [RequestMapping("/{id}", Method = "GET")]
        QueryResult<JObject> GetQueryResultValue([PathVariable("id")]string id, [RequestQuery] TestServiceParam param);

        //[RequestMapping("/{id}", Method = "GET")]
        //Task<JObject> GetValueAsync([PathVariable("id")]string id);
        //[RequestMapping("/{id}", Method = "GET")]
        //Task<JObject> GetValueAsync([PathVariable]int id, [RequestParam] string test);
        //[GetMapping("/{id}")]
        //Task<JObject> GetValueAsync([PathVariable]int id, [RequestQuery] TestServiceParam param);
        [RequestMapping("/{id}")]
        void GetValueVoid([PathVariable]int id, [RequestParam] string test, [RequestQuery] TestServiceParam param);

        [RequestMapping("/{id}")]
        Task GetValueVoidAsync([PathVariable]int id, [RequestParam] string test, [RequestQuery] TestServiceParam param);

        [RequestMapping("/{id}", Method = "POST")]
        Task PostValueAsync([PathVariable]int id, [RequestParam] string test, [RequestBody] TestServiceParam param);

        [RequestMapping("/{id}")]
        void GetValueVoid([PathVariable]int id, [RequestParam] TestServiceParam queryParam, [RequestQuery] TestServiceParam param);

        //[GetMapping("/{id}")]
        //Task<JObject> GetValueAsync([PathVariable]int id, [RequestParam] string test, [RequestQuery] TestServiceParam param);

    }



}
