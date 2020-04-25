using System;
using System.Collections.Generic;
using System.Reflection;

namespace NitroxModel.Helper
{
    public static class ReflectionCache
    {
        private static readonly Dictionary<ReflectionKey, Delegate> cache = new Dictionary<ReflectionKey, Delegate>();
        
        public static Func<TReturn> GetReturn<TReturn>(string methodName, object instance)
        {
            Type instanceType = instance.GetType();
            Type returnType = typeof(TReturn);
            Delegate del;
            if (cache.TryGetValue(new ReflectionKey(instanceType, returnType), out del))
            {
                return (Func<TReturn>)del;
            }
            
            MethodInfo methodInfo = instanceType.GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (methodInfo == null)
            {
                return null;
            }
            Func<TReturn> result = (Func<TReturn>) Delegate.CreateDelegate(typeof(Func<TReturn>), instance, methodInfo);
            cache[new ReflectionKey(instanceType, returnType)] = result;
            return result;
        }

        private struct ReflectionKey
        {
            public Type InstanceType { get; }
            public Type ResultType { get; }

            public ReflectionKey(Type instanceType, Type resultType)
            {
                InstanceType = instanceType;
                ResultType = resultType;
            }

            public bool Equals(ReflectionKey other)
            {
                return Equals(InstanceType, other.InstanceType) && Equals(ResultType, other.ResultType);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                return obj is ReflectionKey && Equals((ReflectionKey) obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((InstanceType != null ? InstanceType.GetHashCode() : 0) * 397) ^ (ResultType != null ? ResultType.GetHashCode() : 0);
                }
            }

            public static bool operator ==(ReflectionKey left, ReflectionKey right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(ReflectionKey left, ReflectionKey right)
            {
                return !left.Equals(right);
            }
        }
    }
}