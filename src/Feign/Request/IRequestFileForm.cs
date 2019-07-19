using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Feign.Request
{
    public interface IRequestFileForm
    {
        IEnumerable<IRequestFile> RequestFiles { get; }
    }
}
