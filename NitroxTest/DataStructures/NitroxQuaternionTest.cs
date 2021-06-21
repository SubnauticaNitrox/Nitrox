using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NitroxModel.DataStructures.GameLogic
{
    [TestClass]
    public class NitroxQuaternionTest
    {
        private NitroxQuaternion defaultVal;
        private const float TOLERANCE = 0.0001f;

        [TestInitialize]
        public void Init()
        {
            defaultVal = new NitroxQuaternion(0.5682333f, -0.01828304f, -0.5182831f, 0.6388735f);
        }

        [TestMethod]
        public void TestEquality()
        {
            NitroxQuaternion other1 = new NitroxQuaternion(0.5682333f, -0.01828304f, -0.5182831f, 0.6388735f);
            NitroxQuaternion other2 = new NitroxQuaternion(0.5682343f, -0.01828314f, -0.5182841f, 0.6388745f);

            defaultVal.Equals(other1, TOLERANCE).Should().BeTrue();
            defaultVal.Equals(other2, TOLERANCE).Should().BeTrue(); //Tolerance to low to detect the difference

            (defaultVal == other1).Should().BeTrue();
            (defaultVal != other2).Should().BeTrue();
        }

        [TestMethod]
        public void TestMultiplication()
        {
            NitroxQuaternion result = defaultVal * new NitroxQuaternion(-10f, 0.5f, 0.004f, 256.1111f);
            NitroxQuaternion result2 = new NitroxQuaternion(-10f, 0.5f, 0.004f, 256.1111f) * defaultVal;
            NitroxQuaternion expectedResult = new NitroxQuaternion(139.4012f, 0.8175055f, -132.6342f, 169.3162f);
            NitroxQuaternion expectedResult2 = new NitroxQuaternion(138.8831f, -9.543612f, -132.8368f, 169.3162f);

            result.Equals(expectedResult, TOLERANCE).Should().BeTrue($"Expected: {expectedResult} - Found: {result}");
            result2.Equals(expectedResult2, TOLERANCE).Should().BeTrue($"Expected: {expectedResult2} - Found: {result2}");
        }

        [TestMethod]
        public void TestToEuler()
        {
            NitroxVector3 euler = defaultVal.ToEuler();
            NitroxVector3 euler2 = new NitroxQuaternion(0.3293858f, 0.9271265f, -0.09970073f, -0.1483285f).ToEuler();
            NitroxVector3 euler3 = new NitroxQuaternion(0.5f, 0.5f, -0.5f, 0.5f).ToEuler();
            NitroxVector3 euler4 = new NitroxQuaternion(-0.5f, 0.5f, -0.5f, -0.5f).ToEuler();
            NitroxVector3 expectedResult = new NitroxVector3(45f, 300f, 255f);
            NitroxVector3 expectedResult2 = new NitroxVector3(5f, 200f, 40f);
            NitroxVector3 expectedResult3 = new NitroxVector3(90f, 90f, 0f);
            NitroxVector3 expectedResult4 = new NitroxVector3(90f, 270f, 0f);

            euler.Equals(expectedResult, TOLERANCE).Should().BeTrue($"Expected: {expectedResult} - Found: {euler}");
            euler2.Equals(expectedResult2, TOLERANCE).Should().BeTrue($"Expected: {expectedResult2} - Found: {euler2}");
            euler3.Equals(expectedResult3, TOLERANCE).Should().BeTrue($"Expected: {expectedResult3} - Found: {euler3}");
            euler4.Equals(expectedResult4, TOLERANCE).Should().BeTrue($"Expected: {expectedResult4} - Found: {euler4}");
        }

        [TestMethod]
        public void TestFromEuler()
        {
            NitroxQuaternion result = NitroxQuaternion.FromEuler(new NitroxVector3(45f, 300f, 255f));
            NitroxQuaternion result2 = NitroxQuaternion.FromEuler(new NitroxVector3(45f, -60f, 615f));

            result.Equals(defaultVal, TOLERANCE).Should().BeTrue($"Expected: {defaultVal} - Found: {result}");
            result2.Equals(defaultVal, TOLERANCE).Should().BeTrue($"Expected: {defaultVal} - Found: {result2}");
        }

        [TestMethod]
        public void TestLookRotation()
        {
            NitroxQuaternion result = NitroxQuaternion.LookRotation(NitroxVector3.One, NitroxVector3.One);
            NitroxQuaternion result2 = NitroxQuaternion.LookRotation(new NitroxVector3(0, 0, 1), new NitroxVector3(0, 1, 0));
            NitroxQuaternion expectedResult = new NitroxQuaternion(-0.3250576f, 0.3250576f, 0f, 0.8880739f);
            NitroxQuaternion expectedResult2 = NitroxQuaternion.Identity;

            result.Equals(expectedResult, TOLERANCE).Should().BeTrue($"Expected: {expectedResult} - Found: {result}");
            result2.Equals(expectedResult2, TOLERANCE).Should().BeTrue($"Expected: {expectedResult2} - Found: {result2}");
        }
    }
}
