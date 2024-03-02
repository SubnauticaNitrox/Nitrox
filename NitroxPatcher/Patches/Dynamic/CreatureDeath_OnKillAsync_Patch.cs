using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Spawning.Metadata;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Broadcasts creature death consequences:<br/>
/// - Converted to a cooked item<br/>
/// - Dead but still has its corpse floating in the water<br/>
/// - Eatable decomposition metadata
/// </summary>
public sealed partial class CreatureDeath_OnKillAsync_Patch : NitroxPatch, IDynamicPatch
{
    internal static readonly MethodInfo TARGET_METHOD = AccessTools.EnumeratorMoveNext(Reflect.Method((CreatureDeath t) => t.OnKillAsync()));

    /*
     * 1st injection:
     * gameObject.GetComponent<Rigidbody>().angularDrag = base.gameObject.GetComponent<Rigidbody>().angularDrag * 3f;
     * UnityEngine.Object.Destroy(base.gameObject);
     * result = null;
     * CreatureDeath_OnKillAsync_Patch.BroadcastCookedSpawned(this, gameObject, cookedData); <---- INSERTED LINE
     * 
     * 2nd injection:
     * base.Invoke("RemoveCorpse", this.removeCorpseAfterSeconds);
     * CreatureDeath_OnKillAsync_Patch.BroadcastRemoveCorpse(this); <---- INSERTED LINE
     * 
     * 3rd injection:
     * this.eatable.SetDecomposes(true);
     * CreatureDeath_OnKillAsync_Patch.BroadcastCookedSpawned(this.eatable); <---- INSERTED LINE
     */
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        // First injection
        return new CodeMatcher(instructions).MatchEndForward([
                                                new CodeMatch(OpCodes.Ldarg_0),
                                                new CodeMatch(OpCodes.Ldnull),
                                                new CodeMatch(OpCodes.Stfld),
                                                new CodeMatch(OpCodes.Br),
                                            ])
                                            .Advance(1)
                                            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
                                            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_2))
                                            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_3))
                                            .Insert(new CodeInstruction(OpCodes.Call, Reflect.Method(() => BroadcastCookedSpawned(default, default, default))))
                                            // Second injection
                                            .MatchEndForward([
                                                new CodeMatch(OpCodes.Ldloc_1),
                                                new CodeMatch(OpCodes.Ldfld, Reflect.Field((CreatureDeath t) => t.removeCorpseAfterSeconds)),
                                                new CodeMatch(OpCodes.Call),
                                            ])
                                            .Advance(1)
                                            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
                                            .Insert(new CodeInstruction(OpCodes.Call, Reflect.Method(() => BroadcastRemoveCorpse(default))))
                                            // Third injection
                                            .MatchEndForward([
                                                new CodeMatch(OpCodes.Ldloc_1),
                                                new CodeMatch(OpCodes.Ldfld, Reflect.Field((CreatureDeath t) => t.eatable)),
                                                new CodeMatch(OpCodes.Ldc_I4_1),
                                                new CodeMatch(OpCodes.Callvirt),
                                            ])
                                            .Advance(1)
                                            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_1))
                                            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldfld, Reflect.Field((CreatureDeath t) => t.eatable)))
                                            .Insert(new CodeInstruction(OpCodes.Call, Reflect.Method(() => BroadcastEatableMetadata(default))))
                                            .InstructionEnumeration();
    }

    public static void BroadcastCookedSpawned(CreatureDeath creatureDeath, GameObject gameObject, TechType cookedTechType)
    {
        if (creatureDeath.TryGetNitroxId(out NitroxId creatureId))
        {
            NitroxEntity.SetNewId(gameObject, creatureId);
        }

        Resolve<Items>().Dropped(gameObject, cookedTechType);
    }

    public static void BroadcastRemoveCorpse(CreatureDeath creatureDeath)
    {
        if (creatureDeath.TryGetNitroxId(out NitroxId creatureId))
        {
            Resolve<SimulationOwnership>().StopSimulatingEntity(creatureId);
            EntityPositionBroadcaster.RemoveEntityMovementControl(creatureDeath.gameObject, creatureId);
            Resolve<IPacketSender>().Send(new RemoveCreatureCorpse(creatureId, creatureDeath.transform.localPosition.ToDto(), creatureDeath.transform.localRotation.ToDto()));
        }
    }

    public static void BroadcastEatableMetadata(Eatable eatable)
    {
        if (!eatable.TryGetNitroxId(out NitroxId eatableId))
        {
            return;
        }

        Optional<EntityMetadata> metadata = Resolve<EntityMetadataManager>().Extract(eatable);
        if (metadata.HasValue)
        {
            Resolve<Entities>().BroadcastMetadataUpdate(eatableId, metadata.Value);
        }
    }
}
