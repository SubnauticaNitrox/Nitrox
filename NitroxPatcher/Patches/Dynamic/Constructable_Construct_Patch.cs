using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Bases.EntityUtils;
using NitroxClient.GameLogic.Bases;
using NitroxClient.GameLogic.Spawning.Bases.PostSpawners;
using NitroxClient.Helpers;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.GameLogic.Entities.Bases;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using NitroxModel.Packets;
using NitroxPatcher.PatternMatching;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UWE;
using UnityEngine;
using static System.Reflection.Emit.OpCodes;
using static NitroxClient.GameLogic.Bases.BuildingHandler;

namespace NitroxPatcher.Patches.Dynamic;

internal class Constructable_Construct_Patch : NitroxPatch, IDynamicPatch
{
    internal static MethodInfo TARGET_METHOD = Reflect.Method((Constructable t) => t.Construct());

    private static TemporaryBuildData Temp => BuildingHandler.Main.Temp;

    public static readonly InstructionsPattern InstructionsPattern = new()
    {
        Div,
        Stfld,
        Ldc_I4_0,
        Ret,
        Ldarg_0,
        { InstructionPattern.Call(nameof(Constructable), nameof(Constructable.UpdateMaterial)), "Insert" }
    };

    public static readonly List<CodeInstruction> InstructionsToAdd = new()
    {
        new(Ldarg_0),
        new(Call, Reflect.Method(() => ConstructionAmountModified(default)))
    };

    public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions) =>
        instructions.Transform(InstructionsPattern, (label, instruction) =>
        {
            if (label.Equals("Insert"))
            {
                return InstructionsToAdd;
            }
            return null;
        });

    public static void ConstructionAmountModified(Constructable constructable)
    {
        // We only manage the amount change, not the deconstruction/construction action
        if (!NitroxEntity.TryGetEntityFrom(constructable.gameObject, out NitroxEntity entity))
        {
            // TODO: Maybe delete the ghost
            Log.Error($"Couldn't find a NitroxEntity on {constructable.name}");
            return;
        }
        float amount = NitroxModel.Helper.Mathf.Clamp01(constructable.constructedAmount);
        /*
         * Different cases:
         * - Normal module (only Constructable), just let it go to 1.0f normally
         * - ConstructableBase:
         *   - if it's already in a base, simply update the current base
         *   - else, create a new base
         */
        if (amount == 1f)
        {
            if (constructable is ConstructableBase constructableBase)
            {
                CoroutineHost.StartCoroutine(BroadcastObjectBuilt(constructableBase, entity));
                return;
            }
            CoroutineHost.StartCoroutine(EntityPostSpawner.ApplyPostSpawner(constructable.gameObject, entity.Id));
        }
        // update as a normal module

        // TODO: A latest packet is always sent even after the base is fully built, we don't want that
        // When #2021 is merged, just use ThrottledPacketSender.RemovePendingPackets
        Resolve<ThrottledPacketSender>().SendThrottled(new ModifyConstructedAmount(entity.Id, amount),
            (packet) => { return packet.GhostId; }, 0.1f);
    }

    public static IEnumerator BroadcastObjectBuilt(ConstructableBase constructableBase, NitroxEntity entity)
    {
        BaseGhost baseGhost = constructableBase.model.GetComponent<BaseGhost>();
        constructableBase.SetState(true, true);
        if (!baseGhost.targetBase)
        {
            Log.Error("Something wrong happened, couldn't find base after finishing building ghost");
            yield break;
        }
        Base targetBase = baseGhost.targetBase;
        Base parentBase = null;
        if (constructableBase.tr.parent)
        {
            parentBase = constructableBase.GetComponentInParent<Base>(true);
        }

        // If a module was spawned we need to transfer the ghost id to it for further recognition
        BuildUtils.TryTransferIdFromGhostToModule(baseGhost, entity.Id, constructableBase, out GameObject moduleObject);

        // Have a delay for baseGhost to be actually destroyed
        yield return null;

        if (parentBase)
        {
            // update existing base
            if (!NitroxEntity.TryGetEntityFrom(parentBase.gameObject, out NitroxEntity parentEntity))
            {
                // TODO: Probably add a resync here
                Log.Error("Parent base doesn't have a NitroxEntity, which is not normal");
                yield break;
            }
            MoonpoolManager moonpoolManager = parentBase.gameObject.EnsureComponent<MoonpoolManager>();

            GlobalRootEntity builtPiece = null;
            if (moduleObject)
            {
                if (moduleObject.TryGetComponent(out IBaseModule builtModule))
                {
                    builtPiece = NitroxInteriorPiece.From(builtModule);
                }
                else if (moduleObject.GetComponent<VehicleDockingBay>())
                {
                    builtPiece = moonpoolManager.LatestRegisteredMoonpool;
                }
                else if (moduleObject.TryGetComponent(out MapRoomFunctionality mapRoomFunctionality))
                {
                    builtPiece = NitroxBuild.GetMapRoomEntityFrom(mapRoomFunctionality, parentBase, entity.Id, parentEntity.Id);
                }
            }

            List<Entity> childEntities = NitroxBuild.GetChildEntities(parentBase, parentEntity.Id);

            // We get InteriorPieceEntity children from the base and make up a dictionary with their updated data (their BaseFace)
            Dictionary<NitroxId, NitroxBaseFace> updatedChildren = childEntities.OfType<InteriorPieceEntity>()
                .ToDictionary(entity => entity.Id, entity => entity.BaseFace);
            // Same for MapRooms
            Dictionary<NitroxId, NitroxInt3> updatedMapRooms = childEntities.OfType<MapRoomEntity>()
                .ToDictionary(entity => entity.Id, entity => entity.Cell);

            BuildingHandler.Main.EnsureTracker(parentEntity.Id).LocalOperations++;
            int operationId = BuildingHandler.Main.GetCurrentOperationIdOrDefault(parentEntity.Id);

            UpdateBase updateBase = new(parentEntity.Id, entity.Id, NitroxBase.From(parentBase), builtPiece, updatedChildren, moonpoolManager.GetMoonpoolsUpdate(), updatedMapRooms, Temp.ChildrenTransfer, operationId);
            Log.Debug($"Sending UpdateBase packet: {updateBase}");

            // TODO: (for server-side) Find a way to optimize this (maybe by copying BaseGhost.Finish() => Base.CopyFrom)
            Resolve<IPacketSender>().Send(updateBase);
        }
        else
        {
            // Must happen before NitroxEntity.SetNewId because else, if a moonpool was marked with the same id, this id be will unlinked from the base object
            if (baseGhost.targetBase.TryGetComponent(out MoonpoolManager moonpoolManager))
            {
                moonpoolManager.LateAssignNitroxEntity(entity.Id);
                moonpoolManager.OnPostRebuildGeometry(baseGhost.targetBase);
            }
            // create a new base
            NitroxEntity.SetNewId(baseGhost.targetBase.gameObject, entity.Id);
            BuildingHandler.Main.EnsureTracker(entity.Id).ResetToId();

            Resolve<IPacketSender>().Send(new PlaceBase(entity.Id, NitroxBuild.From(targetBase)));
        }

        if (moduleObject)
        {
            yield return EntityPostSpawner.ApplyPostSpawner(moduleObject, entity.Id);
        }
    }

    public override void Patch(Harmony harmony)
    {
        PatchTranspiler(harmony, TARGET_METHOD);
    }
}
