using System;

namespace Feign
{
    [AttributeUsage(AttributeTargets.Interface, Inherited = false, AllowMultiple = false)]
    public class FeignClientAttribute : Attribute
    {
        public FeignClientAttribute(string name)
        {
            Name = name;
        }
        public virtual string Name { get; set; }
        public virtual string Url { get; set; }
        public virtual Type Fallback { get; set; }
        public virtual Type FallbackFactory { get; set; }
    }
}
