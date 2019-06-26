using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Feign.Tests
{
    class TestFeignClientRequestMetaData
    {
        public Type ServiceType { get; set; }
        public MethodInfo Method { get; set; }
        public Type[] ParameterTypes { get; set; }
        public Type ReturnType { get; set; }
    }
}
