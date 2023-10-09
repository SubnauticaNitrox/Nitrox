using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Bases;
using NitroxClient.GameLogic.Spawning.Bases;
using NitroxClient.Helpers;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Bases;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.GameLogic.Entities.Bases;
using NitroxModel.Helper;
using NitroxModel.Packets;
using NitroxPatcher.PatternMatching;
using UnityEngine;
using UWE;
using static System.Reflection.Emit.OpCodes;
using static NitroxClient.GameLogic.Bases.BuildingHandler;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class Constructable_Construct_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((Constructable t) => t.Construct());

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
        if (!constructable.TryGetNitroxId(out NitroxId entityId))
        {
            Log.ErrorOnce($"[{nameof(ConstructionAmountModified)}] Couldn't find a NitroxEntity on {constructable.name}");
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
            Resolve<ThrottledPacketSender>().RemovePendingPackets(entityId);
            if (constructable is ConstructableBase constructableBase)
            {
                CoroutineHost.StartCoroutine(BroadcastObjectBuilt(constructableBase, entityId));
                return;
            }
            IEnumerator postSpawner = BuildingPostSpawner.ApplyPostSpawner(constructable.gameObject, entityId);
    
            // Can be null if no post spawner is set for the constructable's techtype
            if (postSpawner != null)
            {
                CoroutineHost.StartCoroutine(postSpawner);
            }
            // To avoid any unrequired throttled packet to be sent we clean the pending throttled packets for this object
            Resolve<IPacketSender>().Send(new ModifyConstructedAmount(entityId, 1f));
            return;
        }

        // update as a normal module
        Resolve<ThrottledPacketSender>().SendThrottled(new ModifyConstructedAmount(entityId, amount),
            (packet) => { return packet.GhostId; }, 0.1f);
    }

    public static IEnumerator BroadcastObjectBuilt(ConstructableBase constructableBase, NitroxId entityId)
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
        BuildUtils.TryTransferIdFromGhostToModule(baseGhost, entityId, constructableBase, out GameObject moduleObject);

        // Have a delay for baseGhost to be actually destroyed
        yield return null;

        if (parentBase)
        {
            // update existing base
            if (!parentBase.TryGetNitroxId(out NitroxId parentId))
            {
                BuildingHandler.Main.FailedOperations++;
                Log.Error($"[{nameof(BroadcastObjectBuilt)}] Parent base doesn't have a NitroxEntity, which is not normal");
                yield break;
            }
            MoonpoolManager moonpoolManager = parentBase.gameObject.EnsureComponent<MoonpoolManager>();

            GlobalRootEntity builtPiece = null;
            if (moduleObject)
            {
                if (moduleObject.TryGetComponent(out IBaseModule builtModule))
                {
                    builtPiece = InteriorPieceEntitySpawner.From(builtModule);
                }
                else if (moduleObject.GetComponent<VehicleDockingBay>())
                {
                    builtPiece = moonpoolManager.LatestRegisteredMoonpool;
                }
                else if (moduleObject.TryGetComponent(out MapRoomFunctionality mapRoomFunctionality))
                {
                    builtPiece = BuildUtils.CreateMapRoomEntityFrom(mapRoomFunctionality, parentBase, entityId, parentId);
                }
            }

            SendUpdateBase(parentBase, parentId, entityId, builtPiece, moonpoolManager);
        }
        else
        {
            // Must happen before NitroxEntity.SetNewId because else, if a moonpool was marked with the same id, this id be will unlinked from the base object
            if (baseGhost.targetBase.TryGetComponent(out MoonpoolManager moonpoolManager))
            {
                moonpoolManager.LateAssignNitroxEntity(entityId);
                moonpoolManager.OnPostRebuildGeometry(baseGhost.targetBase);
            }
            // create a new base
            NitroxEntity.SetNewId(baseGhost.targetBase.gameObject, entityId);
            BuildingHandler.Main.EnsureTracker(entityId).ResetToId();

            Resolve<IPacketSender>().Send(new PlaceBase(entityId, BuildEntitySpawner.From(targetBase)));
        }

        if (moduleObject)
        {
            yield return BuildingPostSpawner.ApplyPostSpawner(moduleObject, entityId);
        }
    }

    private static void SendUpdateBase(Base @base, NitroxId baseId, NitroxId pieceId, GlobalRootEntity builtPieceEntity, MoonpoolManager moonpoolManager)
    {
        List<Entity> childEntities = BuildUtils.GetChildEntities(@base, baseId);

        // We get InteriorPieceEntity children from the base and make up a dictionary with their updated data (their BaseFace)
        Dictionary<NitroxId, NitroxBaseFace> updatedChildren = childEntities.OfType<InteriorPieceEntity>()
            .ToDictionary(entity => entity.Id, entity => entity.BaseFace);
        // Same for MapRooms
        Dictionary<NitroxId, NitroxInt3> updatedMapRooms = childEntities.OfType<MapRoomEntity>()
            .ToDictionary(entity => entity.Id, entity => entity.Cell);

        BuildingHandler.Main.EnsureTracker(baseId).LocalOperations++;
        int operationId = BuildingHandler.Main.GetCurrentOperationIdOrDefault(baseId);

        UpdateBase updateBase = new(baseId, pieceId, BuildEntitySpawner.GetBaseData(@base), builtPieceEntity, updatedChildren, moonpoolManager.GetMoonpoolsUpdate(), updatedMapRooms, Temp.ChildrenTransfer, operationId);

        // TODO: (for server-side) Find a way to optimize this (maybe by copying BaseGhost.Finish() => Base.CopyFrom)
        Resolve<IPacketSender>().Send(updateBase);
    }
}
