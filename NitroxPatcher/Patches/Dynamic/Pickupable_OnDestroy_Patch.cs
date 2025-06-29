using System.Reflection;
using NitroxClient.GameLogic.Bases;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class Pickupable_OnDestroy_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((Pickupable t) => t.OnDestroy());

    public static bool Prefix(Pickupable __instance)
    {
        // Stops Pickupable.OnDestroy from triggering the OnDestroy function if BuildingHandler is Resyncing
        // this stops the IItemsContainer.RemoveItem() method from being called which broadcasts a packet in some implementations 
        if (BuildingHandler.Main.Resyncing)
        {
            __instance.isDestroyed = true;
            return false;
        }

        return true;
    }
}
