using Nitrox.Model.Core;
using NitroxClient.GameLogic;
using UnityEngine.InputSystem;

namespace NitroxClient.MonoBehaviours.Gui.Input.KeyBindings.Actions;

public sealed class VehicleHornKeyBindingAction : KeyBinding
{
    public VehicleHornKeyBindingAction() : base("Nitrox_Settings_Keybind_VehicleHorn", "h", "leftStickPress")
    {
    }

    public override void Execute(InputAction.CallbackContext _)
    {
        if (Multiplayer.Joined)
        {
            // KeyBindingManager owns these actions statically, outside the dependency injection container.
#pragma warning disable DIMA001
            NitroxServiceLocator.LocateService<VehicleHorns>().TryHonkCurrentVehicle();
#pragma warning restore DIMA001
        }
    }
}
