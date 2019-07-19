using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Feign.Request
{
    public class StreamRequestFile : IRequestFile
    {
        public StreamRequestFile(Stream stream, string fileName)
        {
            Stream = stream;
            FileName = fileName;
        }

        public Stream Stream { get; }
        public string Name { get; set; }
        public string FileName { get; }
        public string MediaType { get; set; }

        HttpContent IRequestFile.GetHttpContent()
        {
            StreamContent streamContent = new StreamContent(Stream);
            streamContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            {
                FileName = FileName,
                Name = Name
            };            
            streamContent.Headers.ContentType = MediaTypeHeaderValue.Parse(MediaType ?? "application/octet-stream");
            return streamContent;
        }
    }
}
