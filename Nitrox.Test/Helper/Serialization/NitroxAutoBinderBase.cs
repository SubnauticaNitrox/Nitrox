using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AutoBogus;

namespace Nitrox.Test.Helper.Serialization;

public abstract class NitroxAutoBinderBase : AutoBinder
{
    private readonly Dictionary<Type, Queue<Type>> subtypesByBaseType;

    public NitroxAutoBinderBase(Dictionary<Type, Type[]> subtypesByBaseType)
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

            if (subtypesByBaseType[type].Count == 0)
            {
                return default;
            }

            Type subtype = subtypesByBaseType[type].Dequeue();
            subtypesByBaseType[type].Enqueue(subtype);

            // AutoBogus does not support a non-generic CreateInstance method
            TType instance = (TType)typeof(AutoBinder).GetMethod(nameof(base.CreateInstance))
                .MakeGenericMethod(subtype).Invoke(this, new object[] { context });

            return instance;
        }

        if (context.GenerateType == context.ParentType)
        {
            return default;
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
        typeof(NitroxAutoBinderBase).GetMethod(nameof(PopulateInstanceBase), BindingFlags.NonPublic | BindingFlags.Instance)
            .MakeGenericMethod(instance.GetType()).Invoke(this, new object[] { instance, context, members });
    }

    // Weird reflection bypass
    private void PopulateInstanceBase<TType>(object instance, AutoGenerateContext context, IEnumerable<MemberInfo> members)
    {
        base.PopulateInstance<TType>(instance, context, members);
    }

    /// <inheritdoc cref="AutoBinder.GetMembers"/>
    public abstract override Dictionary<string, MemberInfo> GetMembers(Type t);
}
