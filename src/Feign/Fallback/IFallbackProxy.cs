using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Feign.Fallback
{
    public interface IFallbackProxy
    {
        string MethodName { get; }
        Type ReturnType { get; }
        IDictionary<string, object> GetParameters();
        Type[] GetParameterTypes();
    }
}
