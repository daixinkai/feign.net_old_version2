using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Feign.Formatting
{
    public sealed class ConverterCollection : IEnumerable<IConverter>
    {

#if NETSTANDARD
        System.Collections.Concurrent.ConcurrentDictionary<(Type, Type), IConverter> _map = new System.Collections.Concurrent.ConcurrentDictionary<(Type, Type), IConverter>();
#endif

#if NET45
        System.Collections.Concurrent.ConcurrentDictionary<Tuple<Type, Type>, IConverter> _map = new System.Collections.Concurrent.ConcurrentDictionary<Tuple<Type, Type>, IConverter>();
#endif


        public IEnumerator<IConverter> GetEnumerator()
        {
            return _map.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        public void AddConverter<TSource, TResult>(IConverter<TSource, TResult> converter)
        {
#if NETSTANDARD
            var key = (typeof(TSource), typeof(TResult));
#endif
#if NET45
            var key = Tuple.Create(typeof(TSource), typeof(TResult));
#endif
            if (_map.ContainsKey(key))
            {
                _map[key] = converter;
            }
            else
            {
                _map.TryAdd(key, converter);
            }
        }

        public IConverter<TSource, TResult> FindConverter<TSource, TResult>()
        {
#if NETSTANDARD
            var key = (typeof(TSource), typeof(TResult));
#endif
#if NET45
            var key = Tuple.Create(typeof(TSource), typeof(TResult));
#endif
            IConverter converter;
            _map.TryGetValue(key, out converter);
            return converter == null ? null : (IConverter<TSource, TResult>)converter;
        }

    }
}
