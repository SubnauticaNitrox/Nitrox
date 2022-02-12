using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Called whenever a Cyclops or Seamoth is spawned. Nitrox already has <see cref="Vehicles.CreateVehicle(NitroxModel.DataStructures.GameLogic.VehicleModel)"/> to
/// spawn vehicles. This patch is only meant to block the method from executing, causing two vehicles to be spawned instead of one
/// </summary>
public class SubConsoleCommand_OnConsoleCommand_sub_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((SubConsoleCommand t) => t.OnConsoleCommand_sub(default));

    public static bool Prefix()
    {
        ErrorMessage.AddMessage(Language.main.Get("Nitrox_CommandNotAvailable"));
        return false;
    }

    public override void Patch(Harmony harmony)
    {
        PatchPrefix(harmony, TARGET_METHOD);
    }
}
