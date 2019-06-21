using System;
using System.Collections.Generic;
using System.Text;

namespace Feign.Tests
{
    [AttributeUsage(AttributeTargets.Interface, Inherited = false, AllowMultiple = false)]
    public class CustomFeignClientAttribute : FeignClientAttribute
    {
        public CustomFeignClientAttribute(string name) : base(name)
        {
            _url = "http://10.1.5.90:8802/";
        }

        string _url;

        public override string Url { get => _url; set => _url = value; }

    }
}
