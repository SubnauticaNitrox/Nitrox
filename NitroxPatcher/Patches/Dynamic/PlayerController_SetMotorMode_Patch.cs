using System.Reflection;
using NitroxClient.GameLogic.Settings;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Optionally preserves upward momentum that vanilla halves while handing an ordinary swimmer
/// from the underwater motor to the ground motor at the ocean surface.
/// </summary>
public sealed partial class PlayerController_SetMotorMode_Patch : NitroxPatch, IDynamicPatch
{
    internal const float VANILLA_MOMENTUM_RETENTION = 0.5f;
    internal const float DEFAULT_MOMENTUM_RETENTION = 1f;
    internal const float MAX_MOMENTUM_RETENTION = 1.25f;
    internal const float MINIMUM_UPWARD_SPEED = 0.1f;
    internal const float MINIMUM_UPWARD_INPUT = 0.1f;
    internal const float MINIMUM_UPWARD_LOOK = 0.15f;

    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((PlayerController t) => t.SetMotorMode(default));

    public static void Prefix(PlayerController __instance, Player.MotorMode motorMode, out SurfaceBreachState __state)
    {
        __state = default;

        Player player = Player.main;
        AvatarInputHandler inputHandler = AvatarInputHandler.main;
        Vector3 movementInput = inputHandler && inputHandler.IsEnabled() ? GameInput.GetMoveDirection() : Vector3.zero;
        Vector3 forwardDirection = __instance.forwardReference ? __instance.forwardReference.forward : Vector3.zero;

        if (!player || !ShouldRestoreMomentum(NitroxPrefs.EnableEnhancedSurfaceBreaches.Value,
                                              player.motorMode,
                                              motorMode,
                                              player.IsUnderwaterForSwimming(),
                                              inputHandler && inputHandler.IsEnabled(),
                                              player.isPiloting,
                                              player.GetCurrentSub(),
                                              __instance.activeController is UnderwaterMotor,
                                              __instance.velocity.y,
                                              movementInput,
                                              forwardDirection))
        {
            return;
        }

        __state = new SurfaceBreachState(player.rigidBody, __instance.velocity.y);
    }

    public static void Postfix(PlayerController __instance, SurfaceBreachState __state)
    {
        if (!__state.Rigidbody)
        {
            return;
        }

        Vector3 rigidbodyVelocity = __state.Rigidbody.velocity;
        float restoredUpwardVelocity = CalculateRestoredUpwardVelocity(__state.UpwardVelocity,
                                                                       rigidbodyVelocity.y,
                                                                       NitroxPrefs.SurfaceBreachMomentumRetention.Value);
        if (restoredUpwardVelocity <= rigidbodyVelocity.y)
        {
            return;
        }

        rigidbodyVelocity.y = restoredUpwardVelocity;
        __state.Rigidbody.velocity = rigidbodyVelocity;

        Vector3 controllerVelocity = __instance.velocity;
        controllerVelocity.y = UnityEngine.Mathf.Max(controllerVelocity.y, restoredUpwardVelocity);
        __instance.velocity = controllerVelocity;
    }

    internal static bool ShouldRestoreMomentum(bool enhancementEnabled, Player.MotorMode currentMode, Player.MotorMode newMode,
                                               bool isUnderwaterForSwimming, bool inputEnabled, bool isPiloting, bool isInSub,
                                               bool isUsingUnderwaterMotor, float upwardVelocity, Vector3 movementInput, Vector3 forwardDirection)
    {
        if (!enhancementEnabled || currentMode != Player.MotorMode.Dive || newMode != Player.MotorMode.Run ||
            !isUnderwaterForSwimming || !inputEnabled || isPiloting || isInSub || !isUsingUnderwaterMotor ||
            upwardVelocity <= MINIMUM_UPWARD_SPEED)
        {
            return false;
        }

        return movementInput.y > MINIMUM_UPWARD_INPUT ||
               movementInput.z > MINIMUM_UPWARD_INPUT && forwardDirection.y > MINIMUM_UPWARD_LOOK;
    }

    internal static float CalculateRestoredUpwardVelocity(float preTransitionUpwardVelocity, float postTransitionUpwardVelocity, float configuredRetention)
    {
        float retention = float.IsNaN(configuredRetention)
                              ? DEFAULT_MOMENTUM_RETENTION
                              : UnityEngine.Mathf.Clamp(configuredRetention, VANILLA_MOMENTUM_RETENTION, MAX_MOMENTUM_RETENTION);
        return UnityEngine.Mathf.Max(postTransitionUpwardVelocity, preTransitionUpwardVelocity * retention);
    }

    public readonly struct SurfaceBreachState
    {
        public readonly Rigidbody Rigidbody;
        public readonly float UpwardVelocity;

        public SurfaceBreachState(Rigidbody rigidbody, float upwardVelocity)
        {
            Rigidbody = rigidbody;
            UpwardVelocity = upwardVelocity;
        }
    }
}
