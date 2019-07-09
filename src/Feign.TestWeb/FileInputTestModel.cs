using Feign.Tests;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Feign.TestWeb
{
    public class FileInputTestModel
    {
        public IFormFile File { get; set; }
        public TestServiceParam Param { get; set; }
    }
}
