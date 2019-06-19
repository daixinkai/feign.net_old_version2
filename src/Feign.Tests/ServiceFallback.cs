using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Feign.Tests
{
    class ServiceFallback
    {
        public ServiceFallback(TestServiceFallback testServiceFallback1, TestServiceFallback testServiceFallback2)
        {
            _testService1 = testServiceFallback1;
            _testService2 = testServiceFallback2;
        }
        TestServiceFallback _testService2;
        TestServiceFallback _testService1;
        public Task<JObject> GetValueAsync([PathVariable("id")] string id, string text, TestServiceParam param)
        {
            Lazy<Task<JObject>> lazy = new Lazy<Task<JObject>>();
            Func<Task<JObject>> fallback = new Func<Task<JObject>>(delegate
            {
                return _testService2.GetValueAsync(int.Parse(id), text, param);
            });

            return GetValueAsync(id, fallback);
        }
        public Task<JObject> GetValueAsync([PathVariable("id")] string id, string text)
        {
            Lazy<Task<JObject>> lazy = new Lazy<Task<JObject>>();
            Func<Task<JObject>> fallback = new Func<Task<JObject>>(delegate
            {
                return GetValueAsyncStatic(id, text);
            });

            return GetValueAsync(id, fallback);
        }

        public static Task<JObject> GetValueAsyncStatic([PathVariable("id")] string id, string text)
        {
            return null;
        }


        public Task<JObject> GetValueAsync([PathVariable("id")] string id, Func<Task<JObject>> fallback)
        {
            return null;
        }

    }
}
