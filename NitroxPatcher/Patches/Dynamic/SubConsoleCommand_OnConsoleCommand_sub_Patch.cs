using System;
using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;

namespace NitroxPatcher.Patches.Dynamic
{
    /// <summary>
    /// Called whenever a Cyclops or Seamoth is spawned. Nitrox already has <see cref="Vehicles.CreateVehicle(NitroxModel.DataStructures.GameLogic.VehicleModel)"/> to
    /// spawn vehicles. This patch is only meant to block the method from executing, causing two vehicles to be spawned instead of one
    /// </summary>
    class SubConsoleCommand_OnConsoleCommand_sub_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(SubConsoleCommand);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("OnConsoleCommand_sub", BindingFlags.NonPublic | BindingFlags.Instance);

        public static bool Prefix(SubConsoleCommand __instance, NotificationCenter.Notification n)
        {
            return false;
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
