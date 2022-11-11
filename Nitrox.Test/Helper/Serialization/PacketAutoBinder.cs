using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Nitrox.Test.Helper.Serialization;
internal class PacketAutoBinder : NitroxAutoBinderBase
{
    public PacketAutoBinder(Dictionary<Type, Type[]> subtypesByBaseType) : base(subtypesByBaseType) { }

    public override Dictionary<string, MemberInfo> GetMembers(Type t)
    {
        return t.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(member => member.MemberType == MemberTypes.Field)
            .GroupBy(member => member.Name).ToDictionary(k => k.Key, g => g.First());
    }
}
