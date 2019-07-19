using Feign.Request;
using System;
using System.Collections.Generic;
using System.Text;

namespace Feign.Tests
{
    public class TestServiceUploadFileParam : TestServiceParam, IRequestFileForm
    {
        public IRequestFile File { get; set; }
        IEnumerable<IRequestFile> IRequestFileForm.RequestFiles => new[] { File };
    }
}
