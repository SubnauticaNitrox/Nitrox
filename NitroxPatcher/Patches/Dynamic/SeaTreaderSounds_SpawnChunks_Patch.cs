using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Prevents SeaTreaders from spawning chunks when non simulated, else broadcasts this event.
/// </summary>
public sealed partial class SeaTreaderSounds_SpawnChunks_Patch : NitroxPatch, IDynamicPatch
{
    internal static readonly MethodInfo TARGET_METHOD = Reflect.Method((SeaTreaderSounds t) => t.SpawnChunks(default));

    public static bool Prefix(SeaTreaderSounds __instance)
    {
        if (!__instance.treader.TryGetNitroxId(out NitroxId creatureId) ||
            Resolve<SimulationOwnership>().HasAnyLockType(creatureId))
        {
            return true;
        }

        return false;
    }

    /*
     * MODIFIED:
     * Vector3 vector2 = new Vector3(vector.x, 0f, vector.y);
     * transform.position = transform.TransformPoint(vector2);
     * SeaTreaderSounds_SpawnChunks_Patch.BroadcastSeaTreaderSpawnedChunk(this, transform); <--- INSERTED LINE
     */
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions).MatchEndForward([
                                                new CodeMatch(OpCodes.Ldloc_3),
                                                new CodeMatch(OpCodes.Ldloc_3),
                                                new CodeMatch(OpCodes.Ldloc_S),
                                                new CodeMatch(OpCodes.Callvirt),
                                                new CodeMatch(OpCodes.Callvirt),
                                            ])
                                            .Advance(1)
                                            .InsertAndAdvance([
                                                new CodeInstruction(OpCodes.Ldarg_0),
                                                new CodeInstruction(OpCodes.Ldloc_3),
                                                new CodeInstruction(OpCodes.Call, Reflect.Method(() => BroadcastSeaTreaderSpawnedChunk(default, default)))
                                            ])
                                            .InstructionEnumeration();
    }

    public static void BroadcastSeaTreaderSpawnedChunk(SeaTreaderSounds seaTreaderSounds, Transform chunkTransform)
    {
        if (seaTreaderSounds.treader.TryGetNitroxId(out NitroxId seaTreaderId))
        {
            NitroxId chunkId = NitroxEntity.GenerateNewId(chunkTransform.gameObject);
            Resolve<IPacketSender>().Send(new SeaTreaderSpawnedChunk(seaTreaderId, chunkId, chunkTransform.position.ToDto(), chunkTransform.rotation.ToDto()));
        }
    }
}
