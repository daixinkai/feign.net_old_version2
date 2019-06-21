using Feign.Proxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Feign.Reflection
{
    public class FeignClientTypeBuilder
    {
        public FeignClientTypeBuilder() : this(new DynamicAssembly())
        {
        }

#if DEBUG&&NET45
        public
#else
        internal
#endif
        FeignClientTypeBuilder(DynamicAssembly dynamicAssembly)
        {
            _guid = Guid.NewGuid().ToString("N").ToUpper();
            _suffix = "_Proxy_" + _guid;
            _dynamicAssembly = dynamicAssembly;
            _methodBuilder = new FeignClientProxyServiceEmitMethodBuilder();
            _fallbackMethodBuilder = new FallbackFeignClientProxyServiceEmitMethodBuilder(_dynamicAssembly);
        }

        string _guid;
        string _suffix;

        FeignClientProxyServiceEmitMethodBuilder _methodBuilder;

        FallbackFeignClientProxyServiceEmitMethodBuilder _fallbackMethodBuilder;

        DynamicAssembly _dynamicAssembly;

        public Type BuildType(Type interfaceType)
        {
            if (!NeedBuildType(interfaceType))
            {
                return null;
            }
            FeignClientAttribute feignClientAttribute = interfaceType.GetCustomAttribute<FeignClientAttribute>();

            IMethodBuilder methodBuilder;

            Type parentType;
            if (feignClientAttribute.Fallback != null)
            {
                methodBuilder = _fallbackMethodBuilder;
                parentType = typeof(FallbackFeignClientProxyService<,>);
                parentType = parentType.MakeGenericType(interfaceType, feignClientAttribute.Fallback);
            }
            else if (feignClientAttribute.FallbackFactory != null)
            {
                methodBuilder = _fallbackMethodBuilder;
                parentType = typeof(FallbackFactoryFeignClientProxyService<,>);
                parentType = parentType.MakeGenericType(interfaceType, feignClientAttribute.FallbackFactory);
            }
            else
            {
                methodBuilder = _methodBuilder;
                parentType = typeof(FeignClientProxyService<>);
                parentType = parentType.MakeGenericType(interfaceType);
            }
            parentType = GetParentType(parentType);
            TypeBuilder typeBuilder = CreateTypeBuilder(GetTypeFullName(interfaceType), parentType);

            BuildConstructor(typeBuilder, parentType);

            BuildServiceIdProperty(typeBuilder, interfaceType);
            BuildBaseUriProperty(typeBuilder, interfaceType);
            BuildUrlProperty(typeBuilder, interfaceType);
            typeBuilder.AddInterfaceImplementation(interfaceType);
            foreach (var method in interfaceType.GetMethods())
            {
                methodBuilder.BuildMethod(typeBuilder, parentType, method, feignClientAttribute);
            }
            var typeInfo = typeBuilder.CreateTypeInfo();
            Type type = typeInfo.AsType();

            return type;
        }

        public virtual Type GetParentType(Type parentType)
        {
            return parentType;
        }

        public virtual ConstructorInfo GetConstructor(Type parentType)
        {
            return parentType.GetConstructors()[0];
        }

        string GetTypeFullName(Type interfaceType)
        {
            return interfaceType.FullName + _suffix;
            //return interfaceType.Assembly.GetName().ToString() + "_" + interfaceType.FullName;
        }



        void BuildConstructor(TypeBuilder typeBuilder, Type parentType)
        {
            ConstructorInfo baseConstructorInfo = GetConstructor(parentType);
            var parameterTypes = baseConstructorInfo.GetParameters().Select(s => s.ParameterType).ToArray();

            ConstructorBuilder constructorBuilder = typeBuilder.DefineConstructor(
               MethodAttributes.Public,
               CallingConventions.Standard,
               parameterTypes);

            ILGenerator constructorIlGenerator = constructorBuilder.GetILGenerator();
            constructorIlGenerator.Emit(OpCodes.Ldarg_0);
            for (int i = 1; i <= baseConstructorInfo.GetParameters().Length; i++)
            {
                constructorIlGenerator.Emit(OpCodes.Ldarg_S, i);
            }
            constructorIlGenerator.Emit(OpCodes.Call, baseConstructorInfo);
            constructorIlGenerator.Emit(OpCodes.Ret);
        }

        void BuildReadOnlyProperty(TypeBuilder typeBuilder, Type interfaceType, string propertyName, string propertyValue)
        {
            PropertyBuilder propertyBuilder = typeBuilder.DefineProperty(propertyName, PropertyAttributes.None, typeof(string), Type.EmptyTypes);

            //if (property.CanRead)
            //{
            MethodBuilder propertyGet = typeBuilder.DefineMethod("get_" + propertyName, MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Virtual, typeof(string), Type.EmptyTypes);
            ILGenerator iLGenerator = propertyGet.GetILGenerator();
            iLGenerator.Emit(OpCodes.Ldstr, propertyValue);
            iLGenerator.Emit(OpCodes.Ret);
            propertyBuilder.SetGetMethod(propertyGet);
            //}
            //if (property.CanWrite)
            //{
            //    MethodBuilder propertySet = typeBuilder.DefineMethod("set_" + propertyName, MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Virtual, typeof(string), Type.EmptyTypes);
            //    ILGenerator iLGenerator = propertySet.GetILGenerator();
            //    iLGenerator.Emit(OpCodes.Ldstr, interfaceType.GetCustomAttribute<FeignClientAttribute>().Name);
            //    iLGenerator.Emit(OpCodes.Ret);
            //    propertyBuilder.SetSetMethod(propertySet);
            //}

        }

        void BuildServiceIdProperty(TypeBuilder typeBuilder, Type interfaceType)
        {
            BuildReadOnlyProperty(typeBuilder, interfaceType, "ServiceId", interfaceType.GetCustomAttribute<FeignClientAttribute>().Name);
        }

        void BuildBaseUriProperty(TypeBuilder typeBuilder, Type interfaceType)
        {
            BuildReadOnlyProperty(typeBuilder, interfaceType, "BaseUri", interfaceType.GetCustomAttribute<RequestMappingAttribute>().Value);
        }

        void BuildUrlProperty(TypeBuilder typeBuilder, Type interfaceType)
        {
            if (interfaceType.GetCustomAttribute<FeignClientAttribute>().Url != null)
            {
                BuildReadOnlyProperty(typeBuilder, interfaceType, "Url", interfaceType.GetCustomAttribute<FeignClientAttribute>().Url);
            }
        }

        internal static bool NeedBuildType(Type type)
        {
            return type.IsInterface && type.IsDefined(typeof(FeignClientAttribute));
        }

        private TypeBuilder CreateTypeBuilder(string typeName, Type parentType)
        {
            return _dynamicAssembly.ModuleBuilder.DefineType(typeName,
                          TypeAttributes.Public |
                          TypeAttributes.Class |
                          TypeAttributes.AutoClass |
                          TypeAttributes.AnsiClass |
                          TypeAttributes.BeforeFieldInit |
                          TypeAttributes.AutoLayout,
                          parentType);
        }

        public void FinishBuild()
        {
        }

#if DEBUG&&NET45
        public void Save()
        {
            FinishBuild();
            _dynamicAssembly.AssemblyBuilder.Save(_dynamicAssembly.AssemblyName);
        }
#endif

    }
}
