using FluentAssertions;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

[TestClass]
public sealed class UnderwaterMotor_UpdateMove_PatchTest
{
    [TestMethod]
    public void DisabledFeaturePreservesVanillaBehavior()
    {
        ShouldTrack(enhancementEnabled: false).Should().BeFalse();
    }

    [TestMethod]
    public void TracksOrdinaryAndSeaglideAscents()
    {
        ShouldTrack(currentMode: Player.MotorMode.Dive).Should().BeTrue();
        ShouldTrack(currentMode: Player.MotorMode.Seaglide).Should().BeTrue();
    }

    [TestMethod]
    public void ContinuesOnlyAnArmedAscentDuringRunHandoff()
    {
        ShouldTrack(currentMode: Player.MotorMode.Run, transitionArmed: true).Should().BeTrue();
        ShouldTrack(currentMode: Player.MotorMode.Run, transitionArmed: false).Should().BeFalse();
    }

    [TestMethod]
    public void AcceptsActualDirectOrLookDirectedUpwardMotorInput()
    {
        ShouldTrack(inputY: 1f).Should().BeTrue();
        ShouldTrack(inputY: 0f, inputZ: 1f, lookY: 0.5f).Should().BeTrue();
    }

    [TestMethod]
    public void RejectsVehiclesPassiveMovementAndFalling()
    {
        ShouldTrack(currentMode: Player.MotorMode.Vehicle).Should().BeFalse();
        ShouldTrack(inputY: 0f).Should().BeFalse();
        ShouldTrack(inputY: -1f).Should().BeFalse();
        ShouldTrack(inputY: 0f, inputZ: -1f, lookY: 0.5f).Should().BeFalse();
        ShouldTrack(upwardVelocity: UnderwaterMotor_UpdateMove_Patch.MINIMUM_UPWARD_SPEED).Should().BeFalse();
    }

    [TestMethod]
    public void RejectsNonSwimmingOrExternallyControlledStates()
    {
        ShouldTrack(isUnderwaterForSwimming: false).Should().BeFalse();
        ShouldTrack(inputEnabled: false).Should().BeFalse();
        ShouldTrack(isPiloting: true).Should().BeFalse();
        ShouldTrack(isInSub: true).Should().BeFalse();
        ShouldTrack(isActiveUnderwaterMotor: false).Should().BeFalse();
    }

    [TestMethod]
    public void TrackerCapturesPeakThenFreezesAtSurfaceBandEntry()
    {
        UnderwaterMotor_UpdateMove_Patch.SurfaceBreachMomentumTracker tracker = new();

        tracker.Track(3f, false).Should().BeApproximately(3f, 0.001f);
        tracker.Track(5f, false).Should().BeApproximately(5f, 0.001f);
        tracker.Track(4f, true).Should().BeApproximately(5f, 0.001f);
        tracker.Track(7f, true).Should().BeApproximately(5f, 0.001f);
        tracker.SurfaceBandEntered.Should().BeTrue();
    }

    [TestMethod]
    public void RestoredVelocityUsesFrozenBoundedMomentumAndNeverReducesCurrentVelocity()
    {
        CalculateVelocity(6f, 0.1f, 0f).Should().BeApproximately(3f, 0.001f);
        CalculateVelocity(6f, 0.1f, 0.5f).Should().BeApproximately(3f, 0.001f);
        CalculateVelocity(6f, 0.1f, 1f).Should().BeApproximately(6f, 0.001f);
        CalculateVelocity(6f, 0.1f, 2f).Should().BeApproximately(7.5f, 0.001f);
        CalculateVelocity(6f, 8f, 1f).Should().BeApproximately(8f, 0.001f);
        CalculateVelocity(6f, 0.1f, float.NaN).Should().BeApproximately(6f, 0.001f);
    }

    private static bool ShouldTrack(bool enhancementEnabled = true,
                                    Player.MotorMode currentMode = Player.MotorMode.Dive,
                                    bool transitionArmed = false,
                                    bool isUnderwaterForSwimming = true,
                                    bool inputEnabled = true,
                                    bool isPiloting = false,
                                    bool isInSub = false,
                                    bool isActiveUnderwaterMotor = true,
                                    float upwardVelocity = 4f,
                                    float inputY = 1f,
                                    float inputZ = 0f,
                                    float lookY = 0f)
    {
        return UnderwaterMotor_UpdateMove_Patch.ShouldTrackAscent(enhancementEnabled,
                                                                  currentMode,
                                                                  transitionArmed,
                                                                  isUnderwaterForSwimming,
                                                                  inputEnabled,
                                                                  isPiloting,
                                                                  isInSub,
                                                                  isActiveUnderwaterMotor,
                                                                  upwardVelocity,
                                                                  new Vector3(0f, inputY, inputZ),
                                                                  new Vector3(0f, lookY, 1f));
    }

    private static float CalculateVelocity(float preSurface, float current, float retention)
    {
        return UnderwaterMotor_UpdateMove_Patch.CalculateRestoredUpwardVelocity(preSurface, current, retention);
    }
}
