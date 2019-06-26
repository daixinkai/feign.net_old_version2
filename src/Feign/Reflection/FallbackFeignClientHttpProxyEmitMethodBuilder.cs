using Feign.Internal;
using Feign.Proxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Feign.Reflection
{
    class FallbackFeignClientHttpProxyEmitMethodBuilder : FeignClientHttpProxyEmitMethodBuilder
    {
        public FallbackFeignClientHttpProxyEmitMethodBuilder(DynamicAssembly dynamicAssembly)
        {
            _dynamicAssembly = dynamicAssembly;
        }

        DynamicAssembly _dynamicAssembly;


        protected override MethodInfo GetInvokeMethod(RequestMappingBaseAttribute requestMapping, Type returnType, bool async)
        {
            MethodInfo httpClientMethod;
            bool isGeneric = !(returnType == null || returnType == typeof(void) || returnType == typeof(Task));
            if (isGeneric)
            {
                httpClientMethod = async ? FallbackFeignClientHttpProxy.HTTP_SEND_ASYNC_GENERIC_METHOD_FALLBACK : FallbackFeignClientHttpProxy.HTTP_SEND_GENERIC_METHOD_FALLBACK;
            }
            else
            {
                httpClientMethod = async ? FallbackFeignClientHttpProxy.HTTP_SEND_ASYNC_METHOD_FALLBACK : FallbackFeignClientHttpProxy.HTTP_SEND_METHOD_FALLBACK;
            }
            if (isGeneric)
            {
                return httpClientMethod.MakeGenericMethod(returnType);
            }
            return httpClientMethod;
        }

        protected override void EmitCallMethod(TypeBuilder typeBuilder, MethodBuilder methodBuilder, ILGenerator iLGenerator, Type serviceType, MethodInfo method, RequestMappingBaseAttribute requestMapping, LocalBuilder uri, EmitRequestContent emitRequestContent)
        {
            var invokeMethod = GetInvokeMethod(method, requestMapping);
            if (emitRequestContent.RequestContent != null && !SupportRequestContent(invokeMethod, requestMapping))
            {
                throw new NotSupportedException("不支持RequestBody或者RequestForm");
            }
            LocalBuilder feignClientRequest = DefineFeignClientRequest(typeBuilder, serviceType, iLGenerator, uri, requestMapping, emitRequestContent, method);
            // fallback
            LocalBuilder fallbackDelegate = DefineFallbackDelegate(typeBuilder, methodBuilder, iLGenerator, serviceType, method);
            iLGenerator.Emit(OpCodes.Ldarg_0);  //this
            iLGenerator.Emit(OpCodes.Ldloc, feignClientRequest);
            iLGenerator.Emit(OpCodes.Ldloc, fallbackDelegate);
            iLGenerator.Emit(OpCodes.Call, invokeMethod);
            iLGenerator.Emit(OpCodes.Ret);
        }

        LocalBuilder DefineFallbackDelegate(TypeBuilder typeBuilder, MethodBuilder methodBuilder, ILGenerator iLGenerator, Type serviceType, MethodInfo method)
        {
            Type delegateType;
            if (method.ReturnType == null || method.ReturnType == typeof(void))
            {
                delegateType = typeof(Action);
            }
            else
            {
                delegateType = typeof(Func<>).MakeGenericType(method.ReturnType);
            }

            int bindingFlagsValue = 0;
            foreach (BindingFlags item in Enum.GetValues(typeof(BindingFlags)))
            {
                bindingFlagsValue += item.GetHashCode();
            }
            var delegateConstructor = delegateType.GetConstructors((BindingFlags)bindingFlagsValue)[0];
            LocalBuilder invokeDelegate = iLGenerator.DeclareLocal(delegateType);
            // if has parameters
            if (method.GetParameters().Length > 0)
            {
                var anonymousMethodClassTypeBuild = FallbackProxyAnonymousMethodClassBuilder.BuildType(_dynamicAssembly.ModuleBuilder, serviceType, method);
                // new anonymousMethodClass
                LocalBuilder anonymousMethodClass = iLGenerator.DeclareLocal(anonymousMethodClassTypeBuild.Item1);
                //field
                iLGenerator.Emit(OpCodes.Ldarg_0); //this

                iLGenerator.Emit(OpCodes.Call, typeBuilder.BaseType.GetProperty("Fallback").GetMethod); //.Fallback
                for (int i = 1; i <= method.GetParameters().Length; i++)
                {
                    iLGenerator.Emit(OpCodes.Ldarg_S, i);
                }

                iLGenerator.Emit(OpCodes.Newobj, anonymousMethodClassTypeBuild.Item2);
                iLGenerator.Emit(OpCodes.Stloc, anonymousMethodClass);
                iLGenerator.Emit(OpCodes.Ldloc, anonymousMethodClass);
                iLGenerator.Emit(OpCodes.Ldftn, anonymousMethodClassTypeBuild.Item3);
            }
            else
            {
                iLGenerator.Emit(OpCodes.Ldarg_0); //this
                iLGenerator.Emit(OpCodes.Call, typeBuilder.BaseType.GetProperty("Fallback").GetMethod); //.Fallback
                iLGenerator.Emit(OpCodes.Ldftn, method);
            }

            iLGenerator.Emit(OpCodes.Newobj, delegateConstructor);
            iLGenerator.Emit(OpCodes.Stloc, invokeDelegate);

            return invokeDelegate;
        }


    }

}