using System;
using System.Collections.Generic;
using System.Reflection;

namespace NitroxModel.Helper
{
    public static class ReflectionCache
    {
        private static readonly Dictionary<ReflectionKey, Delegate> cache = new Dictionary<ReflectionKey, Delegate>();

        private static readonly BindingFlags searchForEverytingUsefulFlags = BindingFlags.Instance |
                                                                             BindingFlags.Public |
                                                                             BindingFlags.NonPublic |
                                                                             BindingFlags.SetField |
                                                                             BindingFlags.SetProperty |
                                                                             BindingFlags.GetField |
                                                                             BindingFlags.GetProperty |
                                                                             BindingFlags.InvokeMethod;

        public static Func<TIn, TReturn> InstanceMethod<TIn, TReturn>(string methodName, TIn instance)
        {
            Type instanceType = instance.GetType();
            Delegate del;
            if (cache.TryGetValue(new ReflectionKey(instanceType, methodName), out del))
            {
                return (Func<TIn, TReturn>)del;
            }

            MethodInfo methodInfo = instanceType.GetMethod(methodName, searchForEverytingUsefulFlags);
            if (methodInfo == null)
            {
                cache[new ReflectionKey(instanceType, methodName)] = null; // Cache that this type doesn't have the method
                return null;
            }
            
            Func<TIn, TReturn> result = (Func<TIn, TReturn>)Delegate.CreateDelegate(typeof(Func<TIn, TReturn>), null, methodInfo);
            cache[new ReflectionKey(instanceType, methodName)] = result;
            return result;
        }

        private struct ReflectionKey
        {
            public Type InstanceType { get; }
            public string MethodName { get; }

            public ReflectionKey(Type instanceType, string methodName)
            {
                InstanceType = instanceType;
                MethodName = methodName;
            }

            public bool Equals(ReflectionKey other)
            {
                return InstanceType == other.InstanceType && MethodName == other.MethodName;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                {
                    return false;
                }
                return obj is ReflectionKey && Equals((ReflectionKey)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((InstanceType != null ? InstanceType.GetHashCode() : 0) * 397) ^ (MethodName != null ? MethodName.GetHashCode() : 0);
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
