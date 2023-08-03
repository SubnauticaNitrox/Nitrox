using HarmonyLib;
using NitroxClient.GameLogic.Bases;
using NitroxModel.DataStructures.GameLogic.Bases;
using NitroxModel.Helper;
using NitroxPatcher.PatternMatching;
using System.Collections.Generic;
using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.GameLogic.Entities.Bases;
using NitroxModel.DataStructures;
using NitroxModel.Packets;
using UnityEngine;
using NitroxClient.GameLogic.Spawning.Bases;
using static System.Reflection.Emit.OpCodes;
using static NitroxClient.GameLogic.Bases.BuildingHandler;

namespace NitroxPatcher.Patches.Dynamic;

internal class BaseDeconstructable_Deconstruct_Patch : NitroxPatch, IDynamicPatch
{
    internal static MethodInfo TARGET_METHOD = Reflect.Method((BaseDeconstructable t) => t.Deconstruct());

    private static TemporaryBuildData Temp => BuildingHandler.Main.Temp;
    private static BuildPieceIdentifier cachedPieceIdentifier;

    public static readonly InstructionsPattern BaseDeconstructInstructionPattern1 = new()
    {
        Callvirt,
        Call,
        Ldloc_3,
        { new() { OpCode = Callvirt, Operand = new(nameof(BaseGhost), nameof(BaseGhost.ClearTargetBase)) }, "Insert1" }
    };
    public static readonly InstructionsPattern BaseDeconstructInstructionPattern2 = new()
    {
        Ldloc_0,
        new() { OpCode = Callvirt, Operand = new(nameof(Base), nameof(Base.FixCorridorLinks)) },
        Ldloc_0,
        { new() { OpCode = Callvirt, Operand = new(nameof(Base), nameof(Base.RebuildGeometry)) }, "Insert2" },
    };

    public static IEnumerable<CodeInstruction> InstructionsToAdd(bool destroyed)
    {
        yield return new(Ldarg_0);
        yield return new(Ldloc_2);
        yield return new(Ldloc_0);
        yield return new(destroyed ? Ldc_I4_1 : Ldc_I4_0);
        yield return new(Call, Reflect.Method(() => PieceDeconstructed(default, default, default, default)));
    }

    public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions) =>
        instructions.Transform(BaseDeconstructInstructionPattern1, (label, instruction) =>
        {
            if (label.Equals("Insert1"))
            {
                return InstructionsToAdd(true);
            }
            return null;
        }).Transform(BaseDeconstructInstructionPattern2, (label, instruction) =>
        {
            if (label.Equals("Insert2"))
            {
                return InstructionsToAdd(false);
            }
            return null;
        });

    public static void Prefix(BaseDeconstructable __instance)
    {
        BuildUtils.TryGetIdentifier(__instance, out cachedPieceIdentifier, null, __instance.face);
    }

    public static void PieceDeconstructed(BaseDeconstructable baseDeconstructable, ConstructableBase constructableBase, Base @base, bool destroyed)
    {
        Log.Debug("PieceDeconstructed");
        if (!@base.TryGetNitroxId(out NitroxId baseId))
        {
            Log.Error("Couldn't find NitroxEntity on a deconstructed base, which is really problematic");
            return;
        }

        GhostEntity ghostEntity = GhostEntitySpawner.From(constructableBase);
        ghostEntity.Id = baseId;
        if (destroyed)
        {
            // Base was destroyed and replaced with a simple ghost
            Log.Debug("Transferring id from base to the new ghost");
            NitroxEntity.SetNewId(constructableBase.gameObject, baseId);

            Log.Debug("Base destroyed and replaced by a simple ghost");
            Resolve<IPacketSender>().Send(new BaseDeconstructed(baseId, ghostEntity));
            return;
        }
        if (!baseDeconstructable.GetComponentInParent<BaseCell>())
        {
            Log.Error("Couldn't find a BaseCell parent to the BaseDeconstructable");
            return;
        }

        // If deconstruction was ordered by BuildingHandler, then we simply take the provided id
        if (Temp.Id != null)
        {
            // If it had an attached module, we'll also delete the NitroxEntity from the said module similarly to the code below
            if (NitroxEntity.TryGetObjectFrom(Temp.Id, out GameObject moduleObject) &&
                moduleObject.TryGetComponent(out IBaseModule baseModule) &&
                constructableBase.moduleFace.HasValue && constructableBase.moduleFace.Value.Equals(baseModule.moduleFace))
            {
                Object.Destroy(moduleObject.GetComponent<NitroxEntity>());
            }
            else if (constructableBase.techType.Equals(TechType.BaseMoonpool) && @base.TryGetComponent(out MoonpoolManager moonpoolManager))
            {
                moonpoolManager.DeregisterMoonpool(constructableBase.transform);
            }

            NitroxEntity.SetNewId(constructableBase.gameObject, Temp.Id);
            // We don't need to go any further
            return;
        }

        NitroxId pieceId = null;
        // If the destructed piece has an attached module, we'll transfer the NitroxEntity from it
        if (constructableBase.moduleFace.HasValue)
        {
            Base.Face moduleFace = constructableBase.moduleFace.Value;
            moduleFace.cell += @base.GetAnchor();
            IBaseModule geometryObject = @base.GetModule(moduleFace);
            if (geometryObject != null && (geometryObject as MonoBehaviour).TryGetNitroxEntity(out NitroxEntity moduleEntity))
            {
                pieceId = moduleEntity.Id;
                Object.Destroy(moduleEntity);
                Log.Debug($"Successfully transferred NitroxEntity from module geometry {moduleEntity.Id}");
            }
        }
        else
        {
            switch (constructableBase.techType)
            {
                case TechType.BaseMoonpool:
                    if (@base.TryGetComponent(out MoonpoolManager moonpoolManager))
                    {
                        pieceId = moonpoolManager.DeregisterMoonpool(constructableBase.transform); // pieceId can still be null
                    }
                    break;
                case TechType.BaseMapRoom:
                    Int3 mapRoomFunctionalityCell = BuildUtils.GetMapRoomFunctionalityCell(constructableBase.model.GetComponent<BaseGhost>());
                    MapRoomFunctionality mapRoomFunctionality = @base.GetMapRoomFunctionalityForCell(mapRoomFunctionalityCell);
                    if (mapRoomFunctionality && mapRoomFunctionality.TryGetNitroxId(out NitroxId mapRoomId))
                    {
                        pieceId = mapRoomId;
                    }
                    else
                    {
                        Log.Error("Either couldn't find a MapRoomFunctionality associated with destroyed piece or couldn't find a NitroxEntity onto it.");
                    }
                    break;
                case TechType.BaseWaterPark:
                    // When a BaseWaterPark doesn't have a moduleFace, it means that there's still another WaterPark so we don't need to destroy its id and it won't be an error
                    break;
                default:
                    if (baseDeconstructable.GetComponent<IBaseModuleGeometry>() != null)
                    {
                        Log.Error("Couldn't find the module's GameObject of IBaseModuleGeometry when transfering the NitroxEntity");
                    }
                    break;
            }
        }

        // Else, if it's a local client deconstruction, we generate a new one
        pieceId ??= new();
        NitroxEntity.SetNewId(constructableBase.gameObject, pieceId);
        ghostEntity.Id = pieceId;
        ghostEntity.ParentId = baseId;

        BuildingHandler.Main.EnsureTracker(baseId).LocalOperations++;
        int operationId = BuildingHandler.Main.GetCurrentOperationIdOrDefault(baseId);

        PieceDeconstructed pieceDeconstructed = Temp.NewWaterPark == null ?
            new PieceDeconstructed(baseId, pieceId, cachedPieceIdentifier, ghostEntity, BuildEntitySpawner.GetBaseData(@base), operationId) :
            new WaterParkDeconstructed(baseId, pieceId, cachedPieceIdentifier, ghostEntity, BuildEntitySpawner.GetBaseData(@base), Temp.NewWaterPark, Temp.MovedChildrenIds, Temp.Transfer, operationId);
        Log.Debug($"Base is not empty, sending packet {pieceDeconstructed}");

        Resolve<IPacketSender>().Send(pieceDeconstructed);
        BuildingHandler.Main.Temp.Reset();
    }

    public override void Patch(Harmony harmony)
    {
        PatchMultiple(harmony, TARGET_METHOD, prefix: true, transpiler: true);
    }
}
