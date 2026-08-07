using Nitrox.Model.Core;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours.Gui.InGame;
using UnityEngine.InputSystem;

namespace NitroxClient.MonoBehaviours.Gui.Input.KeyBindings.Actions;

public sealed class VehicleHornKeyBindingAction : KeyBinding
{
    public VehicleHornKeyBindingAction() : base("Nitrox_Settings_Keybind_VehicleHorn", "h", "leftStickPress")
    {
    }

    public override void OnStarted(InputAction.CallbackContext _)
    {
        if (Multiplayer.Joined)
        {
            // KeyBindingManager owns these actions statically, outside the dependency injection container.
#pragma warning disable DIMA001
            VehicleHorns vehicleHorns = NitroxServiceLocator.LocateService<VehicleHorns>();
            if (!vehicleHorns.TryHonkCurrentVehicle())
            {
                EmoteWheelManager.BeginHold();
            }
#pragma warning restore DIMA001
        }
    }

    public override void OnCanceled(InputAction.CallbackContext _)
    {
        EmoteWheelManager.EndHold();
    }
}
