using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Feign.Request
{
    public class FilePathRequestFile : StreamRequestFile
    {
        public FilePathRequestFile(string filePath) : base(File.OpenRead(filePath), Path.GetFileName(filePath))
        {
        }
    }
}
