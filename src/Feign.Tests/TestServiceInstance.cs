using Feign.Discovery;
using System;
using System.Collections.Generic;
using System.Text;

namespace Feign.Tests
{
    class TestServiceInstance : IServiceInstance
    {
        public string ServiceId { get; set; }

        public string Host { get; set; }

        public int Port { get; set; }

        public Uri Uri { get; set; }
    }
}
