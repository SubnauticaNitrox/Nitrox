using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

internal static class VehicleSpeedBoost
{
    internal const float MULTIPLIER = 2f;

    internal static bool IsActive(SeaMoth seamoth)
    {
        Player player = Player.main;
        return IsActive(player && player.currentMountedVehicle == seamoth);
    }

    internal static bool IsActive(SubControl subControl)
    {
        Player player = Player.main;
        return IsActive(player && player.currentSub == subControl.sub && player.mode == Player.Mode.Piloting);
    }

    internal static bool IsActive(bool isLocalPilot)
    {
        AvatarInputHandler inputHandler = AvatarInputHandler.main;
        if (!isLocalPilot || !inputHandler || !inputHandler.IsEnabled())
        {
            return false;
        }

        return ShouldBoost(true, true, GameInput.GetButtonHeld(GameInput.Button.Sprint), GameInput.GetMoveDirection());
    }

    internal static bool ShouldBoost(bool isLocalPilot, bool inputEnabled, bool sprintHeld, Vector3 movementInput)
    {
        return isLocalPilot && inputEnabled && sprintHeld && movementInput.z > 0f;
    }

    internal static float ApplyTemporaryMultiplier(ref float forwardForce, bool boostActive)
    {
        float originalForce = forwardForce;
        if (boostActive)
        {
            forwardForce *= MULTIPLIER;
        }
        return originalForce;
    }

    internal static void Restore(ref float forwardForce, float originalForce)
    {
        forwardForce = originalForce;
    }
}
