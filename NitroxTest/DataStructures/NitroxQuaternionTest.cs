using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxTest.DataStructures
{
    [TestClass]
    public class NitroxQuaternionTest
    {
        private NitroxQuaternion defaultVal;
        private const float TOLERANCE = 0.0000002f;

        [TestInitialize]
        public void Init()
        {
            defaultVal = new NitroxQuaternion(-0.5682333f, 0.018283f, 0.518283f, -0.6388735f);
        }

        [TestMethod]
        public void TestEquality()
        {
            NitroxQuaternion other1 = new NitroxQuaternion(-0.5682333f, 0.018283f, 0.518283f, -0.6388735f);
            NitroxQuaternion other2 = NitroxQuaternion.FromEuler(new NitroxVector3(0f, 32f, 65f));

            defaultVal.Equals(other1, TOLERANCE).Should().BeTrue();
            defaultVal.Equals(other2, TOLERANCE).Should().BeFalse();

            (defaultVal == other1).Should().BeTrue();
            (defaultVal != other2).Should().BeTrue();
        }

        [TestMethod]
        public void TestMultiplication()
        {
            NitroxQuaternion mult = NitroxQuaternion.FromEuler(new NitroxVector3(0f, 32f, 65f));

            NitroxQuaternion result = mult * defaultVal;

            NitroxQuaternion expectedResult = new NitroxQuaternion(-0.5229965f, 0.5234092f, 0.1701184f, -0.6508282f);

            expectedResult.Equals(result, TOLERANCE).Should().BeTrue();

        }
    }
}
