using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AutoBogus;
using NitroxModel.DataStructures.Util;

namespace Nitrox.Test.Helper.Serialization;

public abstract class NitroxAutoBinderBase : AutoBinder
{
    private readonly PropertyInfo typeStackProperty = typeof(AutoGenerateContext).GetProperty("TypesStack", BindingFlags.NonPublic | BindingFlags.Instance);
    private readonly MethodInfo createInstanceMethod = typeof(AutoBinder).GetMethod(nameof(AutoBinder.CreateInstance));
    private readonly MethodInfo populateInstanceBaseMethod = typeof(NitroxAutoBinderBase).GetMethod(nameof(PopulateInstanceBase), BindingFlags.NonPublic | BindingFlags.Instance);

    private readonly Dictionary<Type, Queue<Type>> subtypesByBaseType;

    protected NitroxAutoBinderBase(Dictionary<Type, Type[]> subtypesByBaseType)
    {
        this.subtypesByBaseType = subtypesByBaseType.ToDictionary(pair => pair.Key, pair => new Queue<Type>(pair.Value));
    }

    /// <remarks>
    /// Populates abstract members with a manually generated object (see <see cref="NitroxAutoBinderBase(Dictionary{Type,Type[]})"/>) which is implementing the abstract class.
    /// </remarks>
    /// <inheritdoc cref="AutoBinder.CreateInstance{TType}"/>
    public override TType CreateInstance<TType>(AutoGenerateContext context)
    {
        Type type = typeof(TType);

        if (type.IsAbstract && type != typeof(Type)) // System.Type is abstract but that is not important to us
        {
            if (!subtypesByBaseType.ContainsKey(type))
            {
                throw new Exception($"{type} has no registered implementing classes.");
            }

            Type subtype;

            // Optionals are for some reason double created. The second creation has an Optional<t> at the second position of the type stack.
            Type[] typeStack = ((Stack<Type>)typeStackProperty.GetValue(context)).ToArray();
            if (typeStack.Length > 1 && typeStack[1].IsGenericType && typeStack[1].GetGenericTypeDefinition() == typeof(Optional<>))
            {
                subtype = subtypesByBaseType[type].Peek();
            }
            else
            {
                subtype = subtypesByBaseType[type].Dequeue();
                subtypesByBaseType[type].Enqueue(subtype);
            }

            // AutoBogus does not support a non-generic CreateInstance method
            TType instance = (TType)createInstanceMethod.MakeGenericMethod(subtype).Invoke(this, new object[] { context });

            return instance;
        }

        return base.CreateInstance<TType>(context);
    }

    public override void PopulateInstance<TType>(object instance, AutoGenerateContext context, IEnumerable<MemberInfo> members = null)
    {
        Type type = typeof(TType);

        if (type.IsAbstract)
        {
            return;
        }

        if (instance == null)
        {
            return;
        }

        // Avoids type mismatch that would only populate inherited members
        populateInstanceBaseMethod.MakeGenericMethod(instance.GetType()).Invoke(this, new[] { instance, context, members });
    }

    // Weird reflection bypass
    private void PopulateInstanceBase<TType>(object instance, AutoGenerateContext context, IEnumerable<MemberInfo> members)
    {
        base.PopulateInstance<TType>(instance, context, members);
    }

    /// <inheritdoc cref="AutoBinder.GetMembers"/>
    public abstract override Dictionary<string, MemberInfo> GetMembers(Type t);

    public static Dictionary<Type, Type[]> GetAllAbstractMembers(List<Type> types, IEnumerable<Type> blacklistedTypes)
    {
        Dictionary<Type, Type[]> subtypesByBaseType = types.Where(type => type.IsAbstract && !type.IsSealed && !type.ContainsGenericParameters && !blacklistedTypes.Contains(type))
                                                           .ToDictionary(type => type, type => types.Where(t => type.IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface).ToArray());
        return subtypesByBaseType;
    }
}
