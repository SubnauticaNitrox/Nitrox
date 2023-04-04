using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using NitroxModel.Packets;
using NitroxPatcher.PatternMatching;
using Oculus.Platform;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using static System.Reflection.Emit.OpCodes;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Synchronizes entities that Spawn something when they are killed, e.g. Coral Disks.
/// </summary>
public class SpawnOnKill_OnKill_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((SpawnOnKill t) => t.OnKill());

    private static readonly InstructionsPattern OnKillPattern = new()
    {
        { Stloc_0, "CallbackInsertion" },
        Ldarg_0,
    };

    public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions) => instructions
    .Transform(OnKillPattern, (label, instruction) =>
    {
        switch (label)
        {
            case "CallbackInsertion":
                return new List<CodeInstruction>
                {
                    new CodeInstruction(Ldarg_0),
                    new CodeInstruction(Ldloc_0),
                    new CodeInstruction(Call, Reflect.Method(() => Callback(default, default))),
                };
        }
        return null;
    });

    public static void Callback(SpawnOnKill spawnOnKill, GameObject gameObject)
    {
        // get the ID of the destroyed object
        NitroxId destroyedEntityId = NitroxEntity.GetId(spawnOnKill.gameObject);
        // Says that the entity doesn't exist anymore  --  Without this, the gameobject is not updated. It will not be pickuppable after that.
        Resolve<IPacketSender>().Send(new EntityDestroyed(destroyedEntityId));
        // Give an ID to the new game object
        NitroxEntity.SetNewId(gameObject, destroyedEntityId);
        // Drop the item in the game
        Resolve<Items>().Dropped(gameObject, CraftData.GetTechType(gameObject));
    }

    public override void Patch(Harmony harmony)
    {
        PatchTranspiler(harmony, TARGET_METHOD);
    }
}
