using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Feign.Reflection
{
    internal interface IMethodBuilder
    {
        //void BuildMethod(MethodInfo method, MethodBuilder methodBuilder, FeignClientAttribute feignClientAttribute);
        void BuildMethod(TypeBuilder typeBuilder, Type serviceType, MethodInfo method, FeignClientAttribute feignClientAttribute);
    }
}
