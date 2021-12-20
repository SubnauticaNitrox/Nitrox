using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxModel.DataStructures.Unity;

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
            NitroxQuaternion other2 = new NitroxQuaternion(-0.5682333f, 0.01828304f, 0.5182831f, -0.6388735f);
            NitroxQuaternion other3 = new NitroxQuaternion(0.5682343f, -0.01828314f, -0.5182841f, 0.6388745f);

            defaultVal.Equals(other1, TOLERANCE).Should().BeTrue();
            defaultVal.Equals(other2, TOLERANCE).Should().BeTrue();
            defaultVal.Equals(other3, TOLERANCE).Should().BeTrue(); //Tolerance to low to detect the difference

            (defaultVal == other1).Should().BeTrue();
            (defaultVal == other2).Should().BeTrue();
            (defaultVal != other3).Should().BeTrue();
        }

        [TestMethod]
        public void TestMultiplication()
        {
            NitroxQuaternion result1 = defaultVal * new NitroxQuaternion(-10f, 0.5f, 0.004f, 256.1111f);
            NitroxQuaternion result2 = new NitroxQuaternion(-10f, 0.5f, 0.004f, 256.1111f) * defaultVal;
            NitroxQuaternion expectedResult1 = new NitroxQuaternion(139.4012f, 0.8175055f, -132.6342f, 169.3162f);
            NitroxQuaternion expectedResult2 = new NitroxQuaternion(138.8831f, -9.543612f, -132.8368f, 169.3162f);

            result1.Equals(expectedResult1, TOLERANCE).Should().BeTrue($"Expected: {expectedResult1} - Found: {result1}");
            result2.Equals(expectedResult2, TOLERANCE).Should().BeTrue($"Expected: {expectedResult2} - Found: {result2}");
        }

        [TestMethod]
        public void TestToEuler()
        {
            NitroxVector3 euler1 = defaultVal.ToEuler();
            NitroxVector3 euler2 = new NitroxQuaternion(0.5f, 0.5f, -0.5f, 0.5f).ToEuler();
            NitroxVector3 euler3 = new NitroxQuaternion(-0.5f, 0.5f, -0.5f, -0.5f).ToEuler();
            NitroxVector3 expectedResult1 = new NitroxVector3(45f, 300f, 255f);
            NitroxVector3 expectedResult1Other = new NitroxVector3(104.5108f, 50.75358f, 316.9205f); //defaultVal can be interpreted as both euler :shrug:
            NitroxVector3 expectedResult2 = new NitroxVector3(90f, 90f, 0f);
            NitroxVector3 expectedResult3 = new NitroxVector3(90f, 270f, 0f);

            (euler1.Equals(expectedResult1, TOLERANCE) || euler1.Equals(expectedResult1Other, TOLERANCE)).Should().BeTrue($"Expected: {expectedResult1} or {expectedResult1Other}- Found: {euler1}");
            euler2.Equals(expectedResult2, TOLERANCE).Should().BeTrue($"Expected: {expectedResult2} - Found: {euler2}");
            euler3.Equals(expectedResult3, TOLERANCE).Should().BeTrue($"Expected: {expectedResult3} - Found: {euler3}");
        }

        [TestMethod]
        public void TestFromEuler()
        {
            NitroxQuaternion result1 = NitroxQuaternion.FromEuler(new NitroxVector3(45f, 300f, 255f));
            NitroxQuaternion result2 = NitroxQuaternion.FromEuler(new NitroxVector3(45f, -60f, 615f));
            NitroxQuaternion result3 = NitroxQuaternion.FromEuler(new NitroxVector3(400f, 10f, -0.07f));
            NitroxQuaternion result4 = NitroxQuaternion.FromEuler(new NitroxVector3(360f, 0f, -720));
            NitroxQuaternion expectedResult3 = new NitroxQuaternion(-0.3406684f, -0.08210776f, 0.03038081f, -0.9360985f);

            result1.Equals(defaultVal, TOLERANCE).Should().BeTrue($"Expected: {defaultVal} - Found: {result1}");
            result2.Equals(defaultVal, TOLERANCE).Should().BeTrue($"Expected: {defaultVal} - Found: {result2}");
            result3.Equals(expectedResult3, TOLERANCE).Should().BeTrue($"Expected: {expectedResult3} - Found: {result3}");
            result4.Equals(NitroxQuaternion.Identity, TOLERANCE).Should().BeTrue($"Expected: {NitroxQuaternion.Identity} - Found: {result4}");
        }
    }
}
