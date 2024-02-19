using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.Helper;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class CreatureDeath_SpawnRespawner_Patch : NitroxPatch, IDynamicPatch
{
    internal static readonly MethodInfo TARGET_METHOD = Reflect.Method((CreatureDeath t) => t.SpawnRespawner());

    public static bool Prefix(CreatureDeath __instance)
    {
        if (__instance.TryGetNitroxId(out NitroxId creatureId) &&
            Resolve<SimulationOwnership>().HasAnyLockType(creatureId))
        {
            return true;
        }
        return false;
    }

    /*
     * this.hasSpawnedRespawner = true;
     * CreatureDeath_SpawnRespawner_Patch.BroadcastRespawnSpawned(this);    [INSERTED LINE]
     */
    public static IEnumerable<CodeInstruction> Transpiler(MethodBase methodBase, IEnumerable<CodeInstruction> instructions)
    {
        // We add instructions right before the ret which is equivalent to inserting at last offset
        return new CodeMatcher(instructions).End()
                                            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_3))
                                            .Insert(new CodeInstruction(OpCodes.Call, Reflect.Method(() => BroadcastSpawnedRespawner(default))))
                                            .InstructionEnumeration();
    }

    public static void BroadcastSpawnedRespawner(Respawn respawn)
    {
        int cellLevel = respawn.TryGetComponent(out LargeWorldEntity largeWorldEntity) ? (int)largeWorldEntity.cellLevel : 0;
        string classId = respawn.GetComponent<UniqueIdentifier>().ClassId;
        NitroxId respawnId = NitroxEntity.GenerateNewId(respawn.gameObject);

        NitroxId parentId = null;
        if (respawn.transform.parent)
        {
            respawn.transform.parent.TryGetNitroxId(out parentId);
        }

        CreatureRespawnEntity creatureSpawner = new(respawn.transform.ToWorldDto(), cellLevel, classId, false, respawnId, NitroxTechType.None, null, parentId, [],
                                                    respawn.spawnTime, respawn.techType.ToDto(), respawn.addComponents);

        Resolve<Entities>().BroadcastEntitySpawnedByClient(creatureSpawner, true);

        // We don't need this object as the respawner only works when we load its cell
        // and it won't activate right now so we'll just delete the entity locally
        GameObject.Destroy(respawn.gameObject);
    }
}
