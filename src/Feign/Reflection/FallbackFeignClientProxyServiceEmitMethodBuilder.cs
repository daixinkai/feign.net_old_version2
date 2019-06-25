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
    class FallbackFeignClientProxyServiceEmitMethodBuilder : FeignClientProxyServiceEmitMethodBuilder
    {
        public FallbackFeignClientProxyServiceEmitMethodBuilder(DynamicAssembly dynamicAssembly)
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
                httpClientMethod = async ? FallbackFeignClientProxyService.HTTP_SEND_ASYNC_GENERIC_METHOD_FALLBACK : FallbackFeignClientProxyService.HTTP_SEND_GENERIC_METHOD_FALLBACK;
            }
            else
            {
                httpClientMethod = async ? FallbackFeignClientProxyService.HTTP_SEND_ASYNC_METHOD_FALLBACK : FallbackFeignClientProxyService.HTTP_SEND_METHOD_FALLBACK;
            }
            if (isGeneric)
            {
                return httpClientMethod.MakeGenericMethod(returnType);
            }
            return httpClientMethod;
        }

        protected override void BuildMethod(TypeBuilder typeBuilder, Type serviceType, MethodInfo method, FeignClientAttribute feignClientAttribute, RequestMappingBaseAttribute requestMapping)
        {
            MethodBuilder methodBuilder = CreateMethodBuilder(typeBuilder, method);
            ILGenerator iLGenerator = methodBuilder.GetILGenerator();
            if (requestMapping == null)
            {
                iLGenerator.Emit(OpCodes.Newobj, typeof(NotSupportedException).GetConstructor(Type.EmptyTypes));
                iLGenerator.Emit(OpCodes.Throw);
                return;
            }

            string uri = requestMapping.Value ?? "";
            LocalBuilder local_Uri = iLGenerator.DeclareLocal(typeof(string));
            LocalBuilder local_OldValue = iLGenerator.DeclareLocal(typeof(string));

            iLGenerator.Emit(OpCodes.Ldstr, uri);
            iLGenerator.Emit(OpCodes.Stloc, local_Uri);

            var invokeMethod = GetInvokeMethod(method, requestMapping);
            EmitRequestContent emitRequestContent = EmitParameter(iLGenerator, method, local_Uri, local_OldValue);
            if (emitRequestContent.RequestContent != null && !SupportRequestContent(invokeMethod, requestMapping))
            {
                throw new NotSupportedException("不支持RequestBody或者RequestForm");
            }
            iLGenerator.Emit(OpCodes.Ldarg_0);  //this
            //baseUrl
            EmitBaseUrl(iLGenerator);
            //mapping uri
            if (requestMapping.Value == null)
            {
                iLGenerator.Emit(OpCodes.Ldnull);
            }
            else
            {
                iLGenerator.Emit(OpCodes.Ldstr, requestMapping.Value);
            }
            iLGenerator.Emit(OpCodes.Ldloc, local_Uri); //uri
            iLGenerator.Emit(OpCodes.Ldstr, requestMapping.GetMethod()); //method
            EmitContentType(iLGenerator, serviceType, requestMapping, emitRequestContent); // contentType
            //content
            if (emitRequestContent.Content != null)
            {
                iLGenerator.Emit(OpCodes.Ldarg_S, emitRequestContent.RequestContentIndex);
            }
            else
            {
                iLGenerator.Emit(OpCodes.Ldnull);
            }
            iLGenerator.Emit(OpCodes.Newobj, typeof(FeignClientRequest).GetConstructors()[0]);
            // fallback
            LocalBuilder fallbackDelegate = DefineFallbackDelegate(typeBuilder, methodBuilder, iLGenerator, method);
            iLGenerator.Emit(OpCodes.Ldloc, fallbackDelegate);
            iLGenerator.Emit(OpCodes.Call, invokeMethod);
            iLGenerator.Emit(OpCodes.Ret);
        }


        LocalBuilder DefineFallbackDelegate(TypeBuilder typeBuilder, MethodBuilder methodBuilder, ILGenerator iLGenerator, MethodInfo method)
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
                //nedd
                //var anonymousMethodClassTypeBuild = AnonymousMethodClassBuilder.BuildType(_dynamicAssembly,typeBuilder, methodBuilder, method.GetParameters());
                //var anonymousMethodClassTypeBuild = AnonymousMethodClassBuilder.BuildType(_dynamicAssembly.ModuleBuilder, typeBuilder, method);
                var anonymousMethodClassTypeBuild = FallbackAnonymousMethodClassBuilder.BuildType(_dynamicAssembly.ModuleBuilder, typeBuilder, method);
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