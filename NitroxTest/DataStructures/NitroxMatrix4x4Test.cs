using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NitroxModel.DataStructures.GameLogic
{
    [TestClass]
    public class NitroxMatrix4x4Test
    {
        private NitroxMatrix4x4 defaultVal;
        private const float TOLERANCE = 0.00009f;

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
            NitroxVector3 position = new NitroxVector3(256f, 32f, -256f);
            NitroxQuaternion rotation = NitroxQuaternion.FromEuler(30f, 25f, 45f);
            NitroxVector3 scale = new NitroxVector3(1f, 2f, 3f);
            NitroxMatrix4x4 matrix = NitroxMatrix4x4.TRS(position, rotation, scale);

            NitroxVector3 position2 = new NitroxVector3(-256f, 32f, 256f);
            NitroxQuaternion rotation2 = NitroxQuaternion.FromEuler(90f, 180f, 45f);
            NitroxVector3 scale2 = new NitroxVector3(3f, 2f, 1f);
            NitroxMatrix4x4 matrix2 = NitroxMatrix4x4.TRS(position2, rotation2, scale2);

            NitroxMatrix4x4 matrix3 = matrix * matrix2;

            NitroxMatrix4x4.DecomposeMatrix(ref matrix, out NitroxVector3 afterPosition, out NitroxQuaternion afterRotation, out NitroxVector3 afterScale);
            NitroxMatrix4x4.DecomposeMatrix(ref matrix2, out NitroxVector3 afterPosition2, out NitroxQuaternion afterRotation2, out NitroxVector3 afterScale2);
            NitroxMatrix4x4.DecomposeMatrix(ref matrix3, out NitroxVector3 afterPosition3, out NitroxQuaternion afterRotation3, out NitroxVector3 afterScale3);

            NitroxVector3 position3 = new NitroxVector3(303.3243f, -469.5755f, 380.89752f); //Actual Values from Unity: [303.3242f, -469.5755f, 380.8975f], but seems plausible anyway
            NitroxQuaternion rotation3 = new NitroxQuaternion(-0.2975527f, 0.8491729f, 0.2783289f, 0.3360072f);
            NitroxVector3 scale3 = new NitroxVector3(6.708203f, 4.472136f, 2f);


            position.Equals(afterPosition, TOLERANCE).Should().BeTrue($"Expected: {position}; Found: {afterPosition}");
            rotation.Equals(afterRotation, TOLERANCE).Should().BeTrue($"Expected: {rotation}; Found: {afterRotation}");
            scale.Equals(afterScale, TOLERANCE).Should().BeTrue($"Expected: {scale}; Found: {afterScale}");

            position2.Equals(afterPosition2, TOLERANCE).Should().BeTrue($"Expected: {position2}; Found: {afterPosition2}");
            rotation2.Equals(afterRotation2, TOLERANCE).Should().BeTrue($"Expected: {rotation2}; Found: {afterRotation2}");
            scale2.Equals(afterScale2, TOLERANCE).Should().BeTrue($"Expected: {scale2}; Found: {afterScale2}");

            position3.Equals(afterPosition3, TOLERANCE).Should().BeTrue($"Expected: {position3}; Found: {afterPosition3}");
            rotation3.Equals(afterRotation3, TOLERANCE).Should().BeTrue($"Expected: {rotation3}; Found: {afterRotation3}");
            scale3.Equals(afterScale3, TOLERANCE).Should().BeTrue($"Expected: {scale3}; Found: {afterScale3}");
        }
    }
}
