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
        private const float TOLERANCE = 0.000002f;

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

            (defaultVal == other1).Should().BeTrue();
            (defaultVal != other2).Should().BeTrue();
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

            float mult = 2f;

            NitroxMatrix4x4 result = defaultVal * other;
            NitroxMatrix4x4 result2 = other * defaultVal;

            NitroxMatrix4x4 result3 = defaultVal * mult;

            expectedResult.Equals(result, TOLERANCE).Should().BeTrue();
            expectedResult2.Equals(result2, TOLERANCE).Should().BeTrue();

        }

        [TestMethod]
        public void TestAddition()
        {
            NitroxMatrix4x4 other = new NitroxMatrix4x4(
                5f, 21f, 55f, 321f,
                4f, 50f, 3f, 50f,
                4.4f, 427f, 3f, 52f,
                23.4f, 5.6f, 1f, 7f);

            NitroxMatrix4x4 expectedResult = new NitroxMatrix4x4(
                5.5f, 23f, 60f, 577f,
                6.4f, 100f, 35f, 55f,
                8.4f, 469.7f, 16f, 564f,
                257.4f, 61.6f, 62f, 85f);

            NitroxMatrix4x4 result = other + defaultVal;

            defaultVal = new NitroxMatrix4x4(
                0.5f, 2f, 5f, 256f,
                2.4f, 50f, 32f, 5f,
                4f, 42.7f, 13f, 512f,
                234f, 56f, 61f, 78f);

            expectedResult.Equals(result, TOLERANCE).Should().BeTrue();

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

        [TestMethod]
        public void TestTRS()
        {
            NitroxVector3 position = new NitroxVector3(256, 32, -256);
            NitroxQuaternion rotation = NitroxQuaternion.FromEuler(new NitroxVector3(30, 25, 45));
            NitroxVector3 scale = new NitroxVector3(1, 1, 1);
            NitroxMatrix4x4 matrix = NitroxMatrix4x4.TRS(position, rotation, scale);

            NitroxVector3 position2 = new NitroxVector3(-256, 32, 256);
            NitroxQuaternion rotation2 = NitroxQuaternion.FromEuler(new NitroxVector3(90, 180, 45));
            NitroxVector3 scale2 = new NitroxVector3(3, 2, 1);
            NitroxMatrix4x4 matrix2 = NitroxMatrix4x4.TRS(position2, rotation2, scale2);

            NitroxMatrix4x4.DecomposeMatrix(ref matrix, out NitroxVector3 afterPosition, out NitroxQuaternion afterRotation, out NitroxVector3 afterScale);
            NitroxMatrix4x4.DecomposeMatrix(ref matrix2, out NitroxVector3 afterPosition2, out NitroxQuaternion afterRotation2, out NitroxVector3 afterScale2);

            NitroxMatrix4x4 result = matrix * matrix2;

            NitroxVector3 resultPosition = new NitroxVector3(349.5739f, -806.8391f, 772.1367f);
            NitroxQuaternion resultRotation = new NitroxQuaternion(0.1899641f, 0.9203625f, -0.2981839f, 0.1671313f);
            NitroxVector3 resultScale = new NitroxVector3(9, 6, 8);

            NitroxMatrix4x4.DecomposeMatrix(ref result, out NitroxVector3 afterPosition3, out NitroxQuaternion afterRotation3, out NitroxVector3 afterScale3);

            position.Equals(afterPosition, TOLERANCE).Should().BeTrue();
            rotation.Equals(afterRotation, TOLERANCE).Should().BeTrue($"Expected: {afterRotation} should be {rotation}");
            scale.Equals(afterScale, TOLERANCE).Should().BeTrue();

            position2.Equals(afterPosition2, TOLERANCE).Should().BeTrue();
            rotation2.Equals(afterRotation2, TOLERANCE).Should().BeTrue($"Expected: {afterRotation2} should be {rotation2}");
            scale2.Equals(afterScale2, TOLERANCE).Should().BeTrue();

            resultPosition.Equals(afterPosition3, TOLERANCE).Should().BeTrue();
            resultRotation.Equals(afterRotation3, TOLERANCE).Should().BeTrue($"Expected: {afterRotation3} should be {resultRotation}");
            resultScale.Equals(afterScale3, TOLERANCE).Should().BeTrue();
        }
    }
}
