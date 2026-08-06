using FluentAssertions;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

[TestClass]
public sealed class PlayerController_SetMotorMode_PatchTest
{
    [TestMethod]
    public void DisabledFeaturePreservesVanillaBehavior()
    {
        ShouldRestore(enhancementEnabled: false).Should().BeFalse();
    }

    [TestMethod]
    public void AcceptsDirectOrLookDirectedUpwardIntent()
    {
        ShouldRestore(inputY: 1f).Should().BeTrue();
        ShouldRestore(inputY: 0f, inputZ: 1f, lookY: 0.5f).Should().BeTrue();
    }

    [TestMethod]
    public void RejectsPassiveOrDownwardMovement()
    {
        ShouldRestore(inputY: 0f).Should().BeFalse();
        ShouldRestore(inputY: -1f).Should().BeFalse();
        ShouldRestore(inputY: 0f, inputZ: -1f, lookY: 0.5f).Should().BeFalse();
    }

    [TestMethod]
    public void RejectsSeaglideVehicleAndOtherMotorTransitions()
    {
        ShouldRestore(currentMode: Player.MotorMode.Seaglide).Should().BeFalse();
        ShouldRestore(currentMode: Player.MotorMode.Vehicle).Should().BeFalse();
        ShouldRestore(currentMode: Player.MotorMode.Walk).Should().BeFalse();
        ShouldRestore(newMode: Player.MotorMode.Walk).Should().BeFalse();
    }

    [TestMethod]
    public void RejectsNonSurfaceOrExternallyControlledStates()
    {
        ShouldRestore(isUnderwaterForSwimming: false).Should().BeFalse();
        ShouldRestore(inputEnabled: false).Should().BeFalse();
        ShouldRestore(isPiloting: true).Should().BeFalse();
        ShouldRestore(isInSub: true).Should().BeFalse();
        ShouldRestore(isUsingUnderwaterMotor: false).Should().BeFalse();
        ShouldRestore(upwardVelocity: PlayerController_SetMotorMode_Patch.MINIMUM_UPWARD_SPEED).Should().BeFalse();
    }

    [TestMethod]
    public void RestoredVelocityUsesBoundedRetentionAndNeverReducesCurrentVelocity()
    {
        CalculateVelocity(6f, 3f, 0f).Should().BeApproximately(3f, 0.001f);
        CalculateVelocity(6f, 3f, 0.5f).Should().BeApproximately(3f, 0.001f);
        CalculateVelocity(6f, 3f, 1f).Should().BeApproximately(6f, 0.001f);
        CalculateVelocity(6f, 3f, 2f).Should().BeApproximately(7.5f, 0.001f);
        CalculateVelocity(6f, 8f, 1f).Should().BeApproximately(8f, 0.001f);
        CalculateVelocity(6f, 3f, float.NaN).Should().BeApproximately(6f, 0.001f);
    }

    private static bool ShouldRestore(bool enhancementEnabled = true,
                                      Player.MotorMode currentMode = Player.MotorMode.Dive,
                                      Player.MotorMode newMode = Player.MotorMode.Run,
                                      bool isUnderwaterForSwimming = true,
                                      bool inputEnabled = true,
                                      bool isPiloting = false,
                                      bool isInSub = false,
                                      bool isUsingUnderwaterMotor = true,
                                      float upwardVelocity = 4f,
                                      float inputY = 1f,
                                      float inputZ = 0f,
                                      float lookY = 0f)
    {
        return PlayerController_SetMotorMode_Patch.ShouldRestoreMomentum(enhancementEnabled,
                                                                        currentMode,
                                                                        newMode,
                                                                        isUnderwaterForSwimming,
                                                                        inputEnabled,
                                                                        isPiloting,
                                                                        isInSub,
                                                                        isUsingUnderwaterMotor,
                                                                        upwardVelocity,
                                                                        new Vector3(0f, inputY, inputZ),
                                                                        new Vector3(0f, lookY, 1f));
    }

    private static float CalculateVelocity(float preTransition, float postTransition, float retention)
    {
        return PlayerController_SetMotorMode_Patch.CalculateRestoredUpwardVelocity(preTransition, postTransition, retention);
    }
}
