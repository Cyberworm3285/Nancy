using System;
using System.Collections.Generic;
using System.Text;

using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;

namespace Nancy
{
    public static class Injector
    {
        private static ConcurrentDictionary<Type, object> _singeltons = new ConcurrentDictionary<Type, object>();
        private static ConcurrentDictionary<Type, Lazy<object>> _ctors = new ConcurrentDictionary<Type, Lazy<object>>();

        public static void RegisterType<T>(Func<object> pseudoCtor)
        {
            Type t = typeof(T);
            if (_ctors.ContainsKey(t))
                throw new TypeAccessException("type already registered");

            _ctors.GetOrAdd(t, new Lazy<object>(pseudoCtor));
        }

        public static void RegisterType<T>()
            where T : new()
        {
            RegisterType<T>(() => new T());
        }

        public static T ResolveType<T>()
        {
            var t = typeof(T);
            if (!_ctors.ContainsKey(t))
                throw new TypeAccessException($"no ctor for type {t.Name} registered");
            if (!_ctors[t].IsValueCreated)
                _singeltons.GetOrAdd(t, _ctors[t].Value);

            return (T)_singeltons[t];
        }

        public static async Task<T> ResolveTypeAsync<T>()
        {
            var t = typeof(T);
            if (!_ctors.ContainsKey(t))
                throw new TypeAccessException($"no ctor for type {t.Name} registered");
            if (!_ctors[t].IsValueCreated)
            {
                var task = new Task<T>(() => (T)_singeltons.GetOrAdd(t, _ctors[t].Value));
                task.Start();
                return await task;
            }

            return (T)_singeltons[t];
        }

        public static async Task<T> RegisterAndResolveAsync<T>(Func<object> pseudoCtor)
        {
            RegisterType<T>(pseudoCtor);
            return await ResolveTypeAsync<T>();
        }

        public static async Task<T> RegisterAndResolveAsync<T>()
            where T : new()
        {
            RegisterType<T>();
            return await ResolveTypeAsync<T>();
        }

        public static T RegisterAndResolve<T>(Func<object> pseudoCtor)
        {
            RegisterType<T>(pseudoCtor);
            return  ResolveType<T>();
        }

        public static T RegisterAndResolve<T>()
            where T : new()
        {
            RegisterType<T>();
            return ResolveType<T>();
        }

        public static async Task<bool> NullifyAsync<T>(Func<bool> cancelTrigger)
        {
            var t = new Task<bool>(() =>
            {
                while (!cancelTrigger())
                {
                    if (_singeltons.TryRemove(typeof(T), out object dummy))
                        return true;
                }
                return false;
            });
            t.Start();
            return await t;
        }
    }
}
