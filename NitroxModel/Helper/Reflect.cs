using System;
using System.Linq.Expressions;
using System.Reflection;

namespace NitroxModel.Helper;

/// <summary>
///     Utility class for reflection API.
/// </summary>
/// <remarks>
///     This class should be used when requiring <see cref="MethodInfo" /> or <see cref="MemberInfo" /> like information from code. This will ensure that compilation only succeeds
///     when reflection is used properly.
/// </remarks>
public static class Reflect
{
    private static readonly BindingFlags BINDING_FLAGS_ALL = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

    public static ConstructorInfo Constructor(Expression<Action> expression)
    {
        return (ConstructorInfo)GetMemberInfo(expression);
    }

    /// <summary>
    ///     Given a lambda expression that calls a method, returns the method info.
    ///     If method has parameters then anything can be supplied, the actual method won't be called.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="expression">The expression.</param>
    /// <returns></returns>
    public static MethodInfo Method(Expression<Action> expression)
    {
        return (MethodInfo)GetMemberInfo(expression);
    }

    /// <summary>
    ///     Given a lambda expression that calls a method, returns the method info.
    ///     If method has parameters then anything can be supplied, the actual method won't be called.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="expression">The expression.</param>
    /// <returns></returns>
    public static MethodInfo Method<T>(Expression<Action<T>> expression)
    {
        return (MethodInfo)GetMemberInfo(expression, typeof(T));
    }

    /// <summary>
    ///     Given a lambda expression that calls a method, returns the method info.
    ///     If method has parameters then anything can be supplied, the actual method won't be called.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="expression">The expression.</param>
    /// <returns></returns>
    public static MethodInfo Method<T, TResult>(Expression<Func<T, TResult>> expression)
    {
        return (MethodInfo)GetMemberInfo(expression, typeof(T));
    }

    public static FieldInfo Field<T>(Expression<Func<T>> expression)
    {
        return (FieldInfo)GetMemberInfo(expression);
    }

    public static FieldInfo Field<T>(Expression<Func<T, object>> expression)
    {
        return (FieldInfo)GetMemberInfo(expression);
    }

    public static PropertyInfo Property<T>(Expression<Func<T>> expression)
    {
        return (PropertyInfo)GetMemberInfo(expression);
    }

    public static PropertyInfo Property<T>(Expression<Func<T, object>> expression)
    {
        return (PropertyInfo)GetMemberInfo(expression);
    }

    private static MemberInfo GetMemberInfo(LambdaExpression expression, Type implementingType = null)
    {
        Expression currentExpression = expression.Body;
        while (true)
        {
            switch (currentExpression.NodeType)
            {
                case ExpressionType.MemberAccess:
                    // If it cannot be unwrapped further, return this member.
                    MemberExpression exp = (MemberExpression)currentExpression;
                    if (exp.Expression is null or ParameterExpression)
                    {
                        return exp.Member;
                    }
                    currentExpression = exp.Expression;
                    break;
                case ExpressionType.UnaryPlus:
                    currentExpression = ((UnaryExpression)currentExpression).Operand;
                    break;
                case ExpressionType.New:
                    return ((NewExpression)currentExpression).Constructor;
                case ExpressionType.Call:
                    MethodInfo method = ((MethodCallExpression)currentExpression).Method;
                    if (implementingType == null)
                    {
                        return method;
                    }
                    if (implementingType == method.ReflectedType)
                    {
                        return method;
                    }
                    // If method target is an interface, lookup the implementation in the implementingType.
                    if (method.ReflectedType?.IsInterface ?? false)
                    {
                        InterfaceMapping interfaceMap = implementingType.GetInterfaceMap(method.ReflectedType);
                        int i = Array.IndexOf(interfaceMap.InterfaceMethods, method);
                        return interfaceMap.TargetMethods[i];
                    }

                    // Expression does not know which type the MethodInfo belongs to if it's virtual; MethodInfo of base type/interface is returned instead.
                    ParameterInfo[] parameters = method.GetParameters();
                    Type[] args = new Type[parameters.Length];
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        args[i] = parameters[i].ParameterType;
                    }
                    return implementingType.GetMethod(method.Name, BINDING_FLAGS_ALL, null, args, null) ?? throw new Exception($"Unable to find method {method} on type {implementingType.FullName}");
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                    currentExpression = ((UnaryExpression)currentExpression).Operand;
                    break;
                case ExpressionType.Invoke:
                    currentExpression = ((InvocationExpression)currentExpression).Expression;
                    break;
                default:
                    throw new ArgumentException($"Lambda expression '{expression}' does not target a member");
            }
        }
    }

    /// <summary>
    ///     Used to supplement ref/out/in parameters when using the <see cref="Reflect" /> API.
    /// </summary>
    public struct Ref<T>
    {
        public static T Field;
    }
}
