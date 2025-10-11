using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Packets;
using Nitrox.Model.Subnautica.DataStructures;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Broadcasts creature death consequences:<br/>
/// - Converted to a cooked item<br/>
/// - Converted to a corpse, which is destroyed on next cell load so we broadcast it accordingly<br/>
/// </summary>
public sealed partial class CreatureDeath_OnKillAsync_Patch : NitroxPatch, IDynamicPatch
{
    internal static readonly MethodInfo TARGET_METHOD = AccessTools.EnumeratorMoveNext(Reflect.Method((CreatureDeath t) => t.OnKillAsync()));

    private static bool IsRemotelyCalled => Resolve<LiveMixinManager>().IsRemoteHealthChanging;

    /*
     * 1st injection:
     * gameObject.GetComponent<Rigidbody>().angularDrag = base.gameObject.GetComponent<Rigidbody>().angularDrag * 3f;
     * UnityEngine.Object.Destroy(base.gameObject);
     * CreatureDeath_OnKillAsync_Patch.BroadcastCookedSpawned(this, gameObject); <---- INSERTED LINE
     * result = null;
     * 
     * 2nd injection:
     * this.SyncFixedUpdatingState();
     * CreatureDeath_OnKillAsync_Patch.BroadcastRemoveCorpse(this);  <---- INSERTED LINE
     */
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        // First injection
        return new CodeMatcher(instructions).MatchStartForward([
                                                new CodeMatch(OpCodes.Ldarg_0),
                                                new CodeMatch(OpCodes.Ldnull),
                                                new CodeMatch(OpCodes.Stfld),
                                                new CodeMatch(OpCodes.Br),
                                            ])
                                            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_1)) // this (CreatureDeath)
                                            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_2)) // gameObject (GameObject)
                                            .Insert(new CodeInstruction(OpCodes.Call, Reflect.Method(() => BroadcastCookedSpawned(default, default))))
                                            // Second injection
                                            .MatchEndForward([
                                                new CodeMatch(OpCodes.Ldloc_1),
                                                new CodeMatch(OpCodes.Call, Reflect.Method((CreatureDeath t) => t.SyncFixedUpdatingState())),
                                            ])
                                            .Advance(1)
                                            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_1)) // this (CreatureDeath)
                                            .Insert(new CodeInstruction(OpCodes.Call, Reflect.Method(() => BroadcastRemoveCorpse(default))))
                                            .InstructionEnumeration();
    }

    public static void BroadcastCookedSpawned(CreatureDeath creatureDeath, GameObject gameObject)
    {
        if (creatureDeath.TryGetNitroxId(out NitroxId creatureId))
        {
            NitroxEntity.SetNewId(gameObject, creatureId);
        }

        if (!IsRemotelyCalled)
        {
            TechType processed = TechData.GetProcessed(CraftData.GetTechType(creatureDeath.gameObject));
            Resolve<Items>().Dropped(gameObject, processed);
        }
    }

    public static void BroadcastRemoveCorpse(CreatureDeath creatureDeath)
    {
        // This case is expected when CreatureDeath.Start happens (calling this) after a metadata processor has already called this
        if (!creatureDeath.TryGetNitroxId(out NitroxId creatureId))
        {
            return;
        }

        TechType processed = TechData.GetProcessed(CraftData.GetTechType(creatureDeath.gameObject));

        // We only need to avoid the case in which there's cooked food spawning instead
        // This check corresponds to the one in CreatureDeath.OnKillAsync
        if (processed != TechType.None && creatureDeath.lastDamageWasHeat)
        {
            return;
        }

        Resolve<SimulationOwnership>().StopSimulatingEntity(creatureId);
        EntityPositionBroadcaster.RemoveEntityMovementControl(creatureDeath.gameObject, creatureId);

        if (!IsRemotelyCalled)
        {
            Resolve<IPacketSender>().Send(new RemoveCreatureCorpse(creatureId, creatureDeath.transform.localPosition.ToDto(), creatureDeath.transform.localRotation.ToDto()));
        }
    }
}
