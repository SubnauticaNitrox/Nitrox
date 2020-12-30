using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxModel.DataStructures;

namespace NitroxTest.DataStructures
{
    [TestClass]
    public class NitroxInt3Test
    {
        private NitroxInt3 int3;

        [TestInitialize]
        public void Setup()
        {
            int3 = new NitroxInt3(5, 10, 15);
        }

        [TestMethod]
        public void Equals()
        {
            NitroxInt3 other1 = new NitroxInt3(5, 10, 15);
            NitroxInt3 other2 = new NitroxInt3(15, 10, 5);

            int3.Equals(other1).Should().BeTrue();
            int3.Equals(other2).Should().BeFalse();
        }

        [TestMethod]
        public void Floor()
        {
            NitroxInt3.Floor(5.1f, 10.4f, 15.5f).Should().Be(int3);
        }

        [TestMethod]
        public void Ceil()
        {
            NitroxInt3.Ceil(4.1f, 9.4f, 14.5f).Should().Be(int3);
        }
    }
}
