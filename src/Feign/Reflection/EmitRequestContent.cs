using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Feign.Reflection
{
    class EmitRequestContent
    {
        public EmitRequestContent()
        {
            RequestContentIndex = -1;
        }
        public RequestParameterBaseAttribute RequestContent { get; set; }
        public ParameterInfo Content { get; set; }
        public int RequestContentIndex { get; set; }
    }
}
