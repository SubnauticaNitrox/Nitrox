using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Prevents <see cref="Respawn.Start"/> from happening on non-simulated entities and sync its behaviour when it's simulated
/// </summary>
public sealed partial class Respawn_Start_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = AccessTools.EnumeratorMoveNext(Reflect.Method((Respawn t) => t.Start()));

    /*
     * UnityEngine.Object.Destroy(base.gameObject);
     * Respawn_Start_Patch.BroadcastRespawnerSpawnedEntity(this, gameObject);    [INSERTED LINE]
     */
    public static IEnumerable<CodeInstruction> Transpiler(MethodBase methodBase, IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions).MatchEndForward([
                                                new CodeMatch(OpCodes.Call),
                                                new CodeMatch(OpCodes.Callvirt),
                                                new CodeMatch(OpCodes.Callvirt),
                                                new CodeMatch(OpCodes.Ldloc_1),
                                                new CodeMatch(OpCodes.Call),
                                                new CodeMatch(OpCodes.Call),
                                            ])
                                            .Advance(1)
                                            .InsertAndAdvance(methodBase.Ldloc<Respawn>())
                                            .InsertAndAdvance(methodBase.Ldloc<GameObject>())
                                            .InsertAndAdvance(new CodeInstruction(OpCodes.Call, Reflect.Method(() => BroadcastRespawnerSpawnedEntity(default, default))))
                                            .InstructionEnumeration();
    }

    public static void BroadcastRespawnerSpawnedEntity(Respawn respawn, GameObject spawnedGameObject)
    {
        if (!respawn.TryGetNitroxId(out NitroxId respawnId))
        {
            respawnId = new();
        }
        // NitroxEntity Remove must happen before the same id is reassigned to spawnedGameObject
        if (respawn.TryGetNitroxEntity(out NitroxEntity respawnEntity))
        {
            respawnEntity.Remove();
        }
        NitroxEntity.SetNewId(spawnedGameObject, respawnId);

        // The entity will automatically replace the respawner entity on both server and remote clients
        Resolve<Items>().Dropped(spawnedGameObject);
    }
}
