using System.Reflection;
using NitroxClient.GameLogic.Settings;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Preserves intentional upward momentum through the vanilla surface-attenuation band for
/// ordinary swimming and Seaglide movement. Vehicles continue to use their own physics.
/// </summary>
public sealed partial class UnderwaterMotor_UpdateMove_Patch : NitroxPatch, IDynamicPatch
{
    internal const float MIN_MOMENTUM_RETENTION = 0.5f;
    internal const float DEFAULT_MOMENTUM_RETENTION = 1f;
    internal const float MAX_MOMENTUM_RETENTION = 1.25f;
    internal const float MINIMUM_UPWARD_SPEED = 0.01f;
    internal const float MINIMUM_UPWARD_INPUT = 0.1f;
    internal const float MINIMUM_UPWARD_LOOK = 0.15f;
    internal const float SURFACE_BAND_DEPTH = 0.5f;

    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((UnderwaterMotor t) => t.UpdateMove());

    private static UnderwaterMotor trackedMotor;
    private static SurfaceBreachMomentumTracker momentumTracker;

    public static void Prefix(UnderwaterMotor __instance, out SurfaceBreachState __state)
    {
        __state = default;

        Player player = Player.main;
        PlayerController playerController = player ? player.playerController : null;
        Rigidbody rigidbody = player ? player.rigidBody : null;
        AvatarInputHandler inputHandler = AvatarInputHandler.main;
        bool inputEnabled = inputHandler && inputHandler.IsEnabled();
        Vector3 forwardDirection = playerController && playerController.forwardReference
                                       ? playerController.forwardReference.forward
                                       : Vector3.zero;
        bool transitionArmed = trackedMotor == __instance && momentumTracker.IsTracking;

        if (!player || !playerController || !rigidbody ||
            !ShouldTrackAscent(NitroxPrefs.EnableEnhancedSurfaceBreaches.Value,
                               player.motorMode,
                               transitionArmed,
                               player.IsUnderwaterForSwimming(),
                               inputEnabled,
                               player.isPiloting,
                               player.GetCurrentSub(),
                               playerController.activeController == __instance,
                               rigidbody.velocity.y,
                               __instance.movementInputDirection,
                               forwardDirection))
        {
            ResetTracker(__instance);
            return;
        }

        if (trackedMotor != __instance)
        {
            trackedMotor = __instance;
            momentumTracker.Reset();
        }

        bool isInSurfaceBand = __instance.transform.position.y >= player.GetWaterLevel() - SURFACE_BAND_DEPTH;
        float preSurfaceUpwardVelocity = momentumTracker.Track(rigidbody.velocity.y, isInSurfaceBand);
        if (isInSurfaceBand)
        {
            __state = new SurfaceBreachState(rigidbody, playerController, preSurfaceUpwardVelocity);
        }
    }

    public static void Postfix(ref Vector3 __result, SurfaceBreachState __state)
    {
        if (!__state.Rigidbody || !__state.PlayerController)
        {
            return;
        }

        Vector3 rigidbodyVelocity = __state.Rigidbody.velocity;
        float restoredUpwardVelocity = CalculateRestoredUpwardVelocity(__state.PreSurfaceUpwardVelocity,
                                                                       rigidbodyVelocity.y,
                                                                       NitroxPrefs.SurfaceBreachMomentumRetention.Value);
        if (restoredUpwardVelocity > rigidbodyVelocity.y)
        {
            rigidbodyVelocity.y = restoredUpwardVelocity;
            __state.Rigidbody.velocity = rigidbodyVelocity;
        }

        __result.y = UnityEngine.Mathf.Max(__result.y, restoredUpwardVelocity);
        Vector3 controllerVelocity = __state.PlayerController.velocity;
        controllerVelocity.y = UnityEngine.Mathf.Max(controllerVelocity.y, restoredUpwardVelocity);
        __state.PlayerController.velocity = controllerVelocity;
    }

    internal static bool ShouldTrackAscent(bool enhancementEnabled, Player.MotorMode currentMode, bool transitionArmed,
                                           bool isUnderwaterForSwimming, bool inputEnabled, bool isPiloting, bool isInSub,
                                           bool isActiveUnderwaterMotor, float upwardVelocity, Vector3 movementInput, Vector3 forwardDirection)
    {
        bool supportedMode = currentMode == Player.MotorMode.Dive || currentMode == Player.MotorMode.Seaglide ||
                             transitionArmed && currentMode == Player.MotorMode.Run;
        if (!enhancementEnabled || !supportedMode || !isUnderwaterForSwimming || !inputEnabled || isPiloting || isInSub ||
            !isActiveUnderwaterMotor || upwardVelocity <= MINIMUM_UPWARD_SPEED)
        {
            return false;
        }

        return movementInput.y > MINIMUM_UPWARD_INPUT ||
               movementInput.z > MINIMUM_UPWARD_INPUT && forwardDirection.y > MINIMUM_UPWARD_LOOK;
    }

    internal static float CalculateRestoredUpwardVelocity(float preSurfaceUpwardVelocity, float currentUpwardVelocity, float configuredRetention)
    {
        float retention = float.IsNaN(configuredRetention)
                              ? DEFAULT_MOMENTUM_RETENTION
                              : UnityEngine.Mathf.Clamp(configuredRetention, MIN_MOMENTUM_RETENTION, MAX_MOMENTUM_RETENTION);
        return UnityEngine.Mathf.Max(currentUpwardVelocity, preSurfaceUpwardVelocity * retention);
    }

    private static void ResetTracker(UnderwaterMotor motor)
    {
        if (trackedMotor == motor)
        {
            trackedMotor = null;
            momentumTracker.Reset();
        }
    }

    public readonly struct SurfaceBreachState
    {
        public readonly Rigidbody Rigidbody;
        public readonly PlayerController PlayerController;
        public readonly float PreSurfaceUpwardVelocity;

        public SurfaceBreachState(Rigidbody rigidbody, PlayerController playerController, float preSurfaceUpwardVelocity)
        {
            Rigidbody = rigidbody;
            PlayerController = playerController;
            PreSurfaceUpwardVelocity = preSurfaceUpwardVelocity;
        }
    }

    internal struct SurfaceBreachMomentumTracker
    {
        public bool IsTracking { get; private set; }
        public bool SurfaceBandEntered { get; private set; }
        public float PreSurfaceUpwardVelocity { get; private set; }

        public float Track(float upwardVelocity, bool isInSurfaceBand)
        {
            if (!IsTracking)
            {
                IsTracking = true;
                PreSurfaceUpwardVelocity = upwardVelocity;
            }

            if (!SurfaceBandEntered)
            {
                PreSurfaceUpwardVelocity = UnityEngine.Mathf.Max(PreSurfaceUpwardVelocity, upwardVelocity);
                SurfaceBandEntered = isInSurfaceBand;
            }

            return PreSurfaceUpwardVelocity;
        }

        public void Reset()
        {
            this = default;
        }
    }
}
