using System;
using System.Collections.Generic;
using System.Linq.Expressions;
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

        public static Func<object, TOut> InstanceMethod<TOut>(string methodName, object instance)
        {
            Type declaringType = instance.GetType();
            Delegate del;
            if (cache.TryGetValue(new ReflectionKey(declaringType, methodName), out del))
            {
                return (Func<object, TOut>)del;
            }

            Func<object, TOut> method = CreateInstancedDelegate<TOut>(methodName, instance);
            cache[new ReflectionKey(declaringType, methodName)] = method;
            return method;
        }

        private static Func<object, TOut> CreateInstancedDelegate<TOut>(string methodName, object instance)
        {
            Type instanceType = instance.GetType();
            MethodInfo method = instanceType.GetMethod(methodName, searchForEverytingUsefulFlags);
            if (method == null)
            {
                return null;
            }

            ParameterExpression instanceParam = Expression.Parameter(typeof(object), "instance");
            MethodCallExpression body = Expression.Call(Expression.Convert(instanceParam, instanceType), method);
            return Expression.Lambda<Func<object, TOut>>(body, instanceParam).Compile();
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
