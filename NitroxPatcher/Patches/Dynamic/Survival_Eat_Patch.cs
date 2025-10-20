using System.Reflection;
using NitroxClient.Communication.Abstract;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Packets;
using Nitrox.Model.Subnautica.Packets;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Let the server know when the player successfully eats an item.
/// </summary>
public sealed partial class Survival_Eat_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((Survival t) => t.Eat(default(GameObject)));

    public static void Postfix(bool __result, GameObject useObj)
    {
        if (__result && useObj.TryGetIdOrWarn(out NitroxId id))
        {
            Resolve<IPacketSender>().Send(new EntityDestroyed(id));
        }
    }
}
