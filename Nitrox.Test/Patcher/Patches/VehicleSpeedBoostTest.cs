using FluentAssertions;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

[TestClass]
public sealed class VehicleSpeedBoostTest
{
    [TestMethod]
    public void ActivatesForLocalPilotHoldingSprintAndMovingForward()
    {
        VehicleSpeedBoost.ShouldBoost(true, true, true, Vector3.forward).Should().BeTrue();
    }

    [TestMethod]
    public void RejectsIncompleteBoostInput()
    {
        VehicleSpeedBoost.ShouldBoost(true, true, false, Vector3.forward).Should().BeFalse();
        VehicleSpeedBoost.ShouldBoost(true, true, true, Vector3.zero).Should().BeFalse();
        VehicleSpeedBoost.ShouldBoost(true, true, true, Vector3.back).Should().BeFalse();
        VehicleSpeedBoost.ShouldBoost(true, true, true, Vector3.left).Should().BeFalse();
        VehicleSpeedBoost.ShouldBoost(true, true, true, Vector3.up).Should().BeFalse();
    }

    [TestMethod]
    public void RejectsDisabledInputAndNonLocalPilots()
    {
        VehicleSpeedBoost.ShouldBoost(true, false, true, Vector3.forward).Should().BeFalse();
        VehicleSpeedBoost.ShouldBoost(false, true, true, Vector3.forward).Should().BeFalse();
    }

    [TestMethod]
    public void AppliesDoubleForceAndRestoresOriginalValue()
    {
        const float originalForce = 12.5f;
        float forwardForce = originalForce;

        float savedForce = VehicleSpeedBoost.ApplyTemporaryMultiplier(ref forwardForce, true);

        savedForce.Should().Be(originalForce);
        forwardForce.Should().Be(originalForce * VehicleSpeedBoost.MULTIPLIER);

        VehicleSpeedBoost.Restore(ref forwardForce, savedForce);
        forwardForce.Should().Be(originalForce);
    }

    [TestMethod]
    public void InactiveBoostPreservesForce()
    {
        const float originalForce = 12.5f;
        float forwardForce = originalForce;

        float savedForce = VehicleSpeedBoost.ApplyTemporaryMultiplier(ref forwardForce, false);

        savedForce.Should().Be(originalForce);
        forwardForce.Should().Be(originalForce);
    }
}
