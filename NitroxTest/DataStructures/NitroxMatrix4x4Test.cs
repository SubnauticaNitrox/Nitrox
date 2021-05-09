using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxTest.DataStructures
{
    [TestClass]
    public class NitroxMatrix4x4Test
    {
        private NitroxMatrix4x4 defaultVal;
        private const float TOLERANCE = 0.0000002f;

        [TestInitialize]
        public void Init()
        {
            defaultVal = new NitroxMatrix4x4(
                0.5f, 2f, 5f, 256f,
                2.4f, 50f, 32f, 5f,
                4f, 42.7f, 13f, 512f,
                234f, 56f, 61f, 78f);
        }

        [TestMethod]
        public void TestEquality()
        {
            NitroxMatrix4x4 other1 = new NitroxMatrix4x4(
                0.5f, 2f, 5f, 256f,
                2.4f, 50f, 32f, 5f,
                4f, 42.7f, 13f, 512f,
                234f, 56f, 61f, 78f);

            NitroxMatrix4x4 other2 = new NitroxMatrix4x4(
                5f, 21f, 55f, 321,
                4f, 50f, 3f, 50f,
                4.4f, 427f, 3f, 52f,
                23.4f, 5.6f, 1f, 7f);


            defaultVal.Equals(other1, TOLERANCE).Should().BeTrue();
            defaultVal.Equals(other2, TOLERANCE).Should().BeFalse();
        }

        [TestMethod]
        public void TestMultiplication()
        {
            NitroxMatrix4x4 other = new NitroxMatrix4x4(
                5f, 21f, 55f, 321f,
                4f, 50f, 3f, 50f,
                4.4f, 427f, 3f, 52f,
                23.4f, 5.6f, 1f, 7f);

            NitroxMatrix4x4 expectedResult = new NitroxMatrix4x4(
                6022.9f, 3679.1f, 304.5f, 2312.5f,
                469.8f, 16242.4f, 383f, 4969.4f,
                12228.8f, 10637.2f, 899.1f, 7679f,
                3487.6f, 34197.8f, 13299f, 81632f);

            NitroxMatrix4x4 expectedResult2 = new NitroxMatrix4x4(
                75386.9f, 21384.5f, 20993f, 54583f,
                11834f, 5436.1f, 4709f, 6710f,
                13207f, 24398.9f, 16897f, 8853.4f,
                1667.14f, 761.5f, 736.2f, 7076.4f);

            NitroxMatrix4x4 result = defaultVal * other;
            NitroxMatrix4x4 result2 = other * defaultVal;

            expectedResult.Equals(result, TOLERANCE).Should().BeTrue();
            expectedResult2.Equals(result2, TOLERANCE).Should().BeTrue();

        }

        [TestMethod]
        public void TestSubtraction()
        {
            NitroxMatrix4x4 other = new NitroxMatrix4x4(
                5f, 21f, 55f, 321f,
                4f, 50f, 3f, 50f,
                4.4f, 427f, 3f, 52f,
                23.4f, 5.6f, 1f, 7f);

            NitroxMatrix4x4 expectedResult = new NitroxMatrix4x4(
                4.5f, 19f, 50f, 65f,
                1.6f, 0f, -29f, 45f,
                0.4f, 384.3f, -10f, -460f,
                -210.6f, -50.4f, -60f, -71f);

            NitroxMatrix4x4 result = other - defaultVal;

            expectedResult.Equals(result, TOLERANCE).Should().BeTrue();

        }
    }
}
