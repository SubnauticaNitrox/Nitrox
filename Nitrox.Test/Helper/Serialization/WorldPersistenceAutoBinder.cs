using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace Nitrox.Test.Helper.Serialization;

internal class WorldPersistenceAutoBinder : NitroxAutoBinderBase
{
    public WorldPersistenceAutoBinder(Dictionary<Type, Type[]> subtypesByBaseType) : base(subtypesByBaseType) { }

    public override Dictionary<string, MemberInfo> GetMembers(Type t)
    {
        return t.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(member => Attribute.IsDefined(member, typeof(DataMemberAttribute)))
                .GroupBy(member => member.Name).ToDictionary(k => k.Key, g => g.First());
    }
}
