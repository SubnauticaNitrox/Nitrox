using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxModel.Packets;

namespace NitroxTest.Model.Packets
{
    [TestClass]
    public class PacketsSerializableTest
    {
        private HashSet<Type> visitedTypes = new HashSet<Type>();

        public void IsSerializable(Type t)
        {
            if (visitedTypes.Contains(t))
            {
                return;
            }

            if (!t.IsSerializable && !t.IsInterface && !Packet.IsTypeSerializable(t))
            {
                Assert.Fail($"Type {t} is not serializable!");
            }

            visitedTypes.Add(t);

            // Recursively check all properties and fields, because IsSerializable only checks if the current type is a primitive or has the [Serializable] attribute.
            t.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public).Select(tt => tt.FieldType).ForEach(IsSerializable);
        }

        [TestMethod]
        public void AllPacketsAreSerializable()
        {
            typeof(Packet).Assembly.GetTypes()
                .Where(p => typeof(Packet).IsAssignableFrom(p) && p.IsClass && !p.IsAbstract)
                .ToList()
                .ForEach(IsSerializable);
        }
    }
}
