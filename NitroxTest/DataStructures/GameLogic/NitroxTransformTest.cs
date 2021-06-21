using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxModel.Logger;

namespace NitroxModel.DataStructures.GameLogic
{
    [TestClass]
    public class NitroxTransformTest
    {
        private const float TOLERANCE = 0.000005f;

        private static NitroxTransform root;
        private static NitroxTransform child1;
        private static NitroxTransform child2;
        private static NitroxTransform grandchild1;
        private static NitroxTransform grandchild2;

        [ClassInitialize]
        public static void Setup(TestContext testContext)
        {
            root = new NitroxTransform(new NitroxVector3(1, 1, 1), NitroxQuaternion.FromEuler(0, 0, 0), new NitroxVector3(1, 1, 1), null);
            child1 = new NitroxTransform(new NitroxVector3(5, 3, -6), NitroxQuaternion.FromEuler(30, 0, 10), new NitroxVector3(2, 2, 2), null);
            child2 = new NitroxTransform(new NitroxVector3(11, 0, -0.5f), NitroxQuaternion.FromEuler(180, 30, 80), new NitroxVector3(0.5f, 0.5f, 0.5f), null);
            grandchild1 = new NitroxTransform(new NitroxVector3(13, 8, 15), NitroxQuaternion.FromEuler(-50, 5, 10), new NitroxVector3(1, 1, 1), null);
            grandchild2 = new NitroxTransform(new NitroxVector3(3, 18, -5), NitroxQuaternion.FromEuler(-5, 15, 1), new NitroxVector3(10, 10, 10), null);

            child1.SetParent(root, false);
            child2.SetParent(root, false);
            grandchild1.SetParent(child1, false);
            grandchild2.SetParent(child2, false);
        }

        [TestMethod]
        public void PositionTest()
        {
            NitroxVector3 rootResult = new NitroxVector3(1, 1, 1);
            NitroxVector3 child1Result = new NitroxVector3(6, 4, -5);
            NitroxVector3 child2Result = new NitroxVector3(12, 1, 0.5f);
            NitroxVector3 grandchild1Result = new NitroxVector3(28.82663f, 6.55587f, 31.11665f);
            NitroxVector3 grandchild2Result = new NitroxVector3(5.79976f, -2.040047f, 6.966463f);

            root.Position.Equals(rootResult, TOLERANCE).Should().BeTrue($"Expected: {rootResult} Found: {root.Position}");
            child1.Position.Equals(child1Result, TOLERANCE).Should().BeTrue($"Expected: {child1Result} Found: {child1.Position}");
            child2.Position.Equals(child2Result, TOLERANCE).Should().BeTrue($"Expected: {child2Result} Found: {child2.Position}");
            grandchild1.Position.Equals(grandchild1Result, TOLERANCE).Should().BeTrue($"Expected: {grandchild1Result} Found: {grandchild1.Position}");
            grandchild2.Position.Equals(grandchild2Result, TOLERANCE).Should().BeTrue($"Expected: {grandchild2Result} Found: {grandchild2.Position}");
        }

        [TestMethod]
        public void RotationTest()
        {
            NitroxQuaternion rootResult = NitroxQuaternion.FromEuler(0, 0, 0);
            NitroxQuaternion child1Result = NitroxQuaternion.FromEuler(30, 0, 10);
            NitroxQuaternion child2Result = NitroxQuaternion.FromEuler(8.537737e-07f, 210, 260);
            NitroxQuaternion grandchild1Result = NitroxQuaternion.FromEuler(340.0263f, 355.2486f, 21.87437f);
            NitroxQuaternion grandchild2Result = NitroxQuaternion.FromEuler(15.60783f, 212.4433f, 261.9936f);

            Log.Debug("Rot: " + grandchild2.Rotation.ToEuler());

            root.Rotation.Equals(rootResult, TOLERANCE).Should().BeTrue($"Expected: {rootResult} Found: {root.Rotation}");
            child1.Rotation.Equals(child1Result, TOLERANCE).Should().BeTrue($"Expected: {child1Result} Found: {child1.Rotation}");
            child2.Rotation.Equals(child2Result, TOLERANCE).Should().BeTrue($"Expected: {child2Result} Found: {child2.Rotation}");
            grandchild1.Rotation.Equals(grandchild1Result, TOLERANCE).Should().BeTrue($"Expected: {grandchild1Result} Found: {grandchild1.Rotation}");
            grandchild2.Rotation.Equals(grandchild2Result, TOLERANCE).Should().BeTrue($"Expected: {grandchild2Result} Found: {grandchild2.Rotation}");
        }
    }
}
