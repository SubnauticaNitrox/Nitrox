using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Bases.New;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Buildings.New;
using NitroxModel.DataStructures.GameLogic.Entities.Bases;
using NitroxModel.Helper;
using NitroxModel.Packets;
using NitroxPatcher.PatternMatching;
using UnityEngine;
using UWE;
using static System.Reflection.Emit.OpCodes;

namespace NitroxPatcher.Patches.Dynamic;

internal sealed class Builder_Patch : NitroxPatch, IDynamicPatch
{
    // TODO: Secure the system, for example: you can't build on a base on which another player built less than 3 seconds ago
    // idea for it: when an action is applied on a base from another player, you can't build on it for 2 seconds
    internal static MethodInfo TARGET_METHOD_TRYPLACE = Reflect.Method(() => Builder.TryPlace());
    internal static MethodInfo TARGET_METHOD_DECONSTRUCT = Reflect.Method((BaseDeconstructable t) => t.Deconstruct());
    internal static MethodInfo TARGET_METHOD_CONSTRUCT = Reflect.Method((Constructable t) => t.Construct());
    internal static MethodInfo TARGET_METHOD_DECONSTRUCT_ASYNC = AccessTools.EnumeratorMoveNext(Reflect.Method((Constructable t) => t.DeconstructAsync(default, default)));
    internal static MethodInfo TARGET_METHOD_DECONSTRUCTION_ALLOWED = typeof(BaseDeconstructable).GetMethod(nameof(BaseDeconstructable.DeconstructionAllowed), BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(string).MakeByRefType() }, null);
    internal static MethodInfo TARGET_METHOD_TOOL_CONSTRUCT = Reflect.Method((BuilderTool t) => t.Construct(default, default, default));

    private static BuildPieceIdentifier cachedPieceIdentifier;

    // Place ghost
    public static readonly InstructionsPattern AddInstructionPattern1 = new()
    {
        Ldloc_0,
        Ldc_I4_0,
        Ldc_I4_1,
        new() { OpCode = Callvirt, Operand = new(nameof(Constructable), nameof(Constructable.SetState)) },
        { Pop, "Insert1" }
    };

    public static readonly List<CodeInstruction> InstructionsToAdd1 = new()
    {
        new(Ldloc_0),
        new(Call, Reflect.Method(() => GhostCreated(default)))
    };

    public static readonly InstructionsPattern AddInstructionPattern2 = new()
    {
        { Ldloc_S, "Take" },
        Ldloc_3,
        Ldloc_S,
        Or,
        { new() { OpCode = Callvirt, Operand = new(nameof(Constructable), nameof(Constructable.SetIsInside)) }, "Insert2" }
    };

    public static readonly List<CodeInstruction> InstructionsToAdd2 = new()
    {
        new(Ldloc_S),
        new(Call, Reflect.Method(() => GhostCreated(default)))
    };


    public static IEnumerable<CodeInstruction> TranspilerTryplace(MethodBase original, IEnumerable<CodeInstruction> instructions) =>
        instructions.Transform(AddInstructionPattern1, (label, instruction) =>
        {
            if (label.Equals("Insert1"))
            {
                return InstructionsToAdd1;
            }
            return null;
        }).Transform(AddInstructionPattern2, (label, instruction) =>
        {
            switch (label)
            {
                case "Take":
                    InstructionsToAdd2[0].operand = instruction.operand;
                    break;
                case "Insert2":
                    return InstructionsToAdd2;
            }
            return null;
        });

    // Construction progress
    public static readonly InstructionsPattern ConstructionInstructionPattern = new()
    {
        Div,
        Stfld,
        Ldc_I4_0,
        Ret,
        Ldarg_0,
        { InstructionPattern.Call(nameof(Constructable), nameof(Constructable.UpdateMaterial)), "Insert" }
    };

    public static readonly List<CodeInstruction> ConstructionInstructionsToAdd = new()
    {
        new(Ldarg_0),
        new(Call, Reflect.Method(() => ConstructionAmountModified(default)))
    };

    public static IEnumerable<CodeInstruction> TranspilerConstruct(MethodBase original, IEnumerable<CodeInstruction> instructions) =>
        instructions.Transform(ConstructionInstructionPattern, (label, instruction) =>
        {
            if (label.Equals("Insert"))
            {
                return ConstructionInstructionsToAdd;
            }
            return null;
        });

    // Deconstruction progress
    public static readonly InstructionsPattern DeconstructionInstructionPattern = new()
    {
        Ldc_I4_0,
        Ret,
        Ldloc_1,
        { InstructionPattern.Call(nameof(Constructable), nameof(Constructable.UpdateMaterial)), "InsertDestruction" }
    };

    public static readonly List<CodeInstruction> DeconstructionInstructionsToAdd = new()
    {
        new(Ldloc_1),
        new(Call, Reflect.Method(() => ConstructionAmountModified(default)))
    };

    public static IEnumerable<CodeInstruction> TranspilerDeconstructAsync(MethodBase original, IEnumerable<CodeInstruction> instructions) =>
        instructions.Transform(DeconstructionInstructionPattern, (label, instruction) =>
        {
            if (label.Equals("InsertDestruction"))
            {
                return DeconstructionInstructionsToAdd;
            }
            return null;
        });

    // Piece Deconstruct
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

    public static IEnumerable<CodeInstruction> MakeBaseDeconstructInstructionsToAdd(bool destroyed)
    {
        yield return new(Ldarg_0);
        yield return new(Ldloc_2);
        yield return new(Ldloc_0);
        yield return new(destroyed ? Ldc_I4_1 : Ldc_I4_0);
        yield return new(Call, Reflect.Method(() => BaseDeconstructed(default, default, default, default)));
    }

    public static IEnumerable<CodeInstruction> TranspilerBaseDeconstruct(MethodBase original, IEnumerable<CodeInstruction> instructions) =>
        instructions.Transform(BaseDeconstructInstructionPattern1, (label, instruction) =>
        {
            if (label.Equals("Insert1"))
            {
                return MakeBaseDeconstructInstructionsToAdd(true);
            }
            return null;
        }).Transform(BaseDeconstructInstructionPattern2, (label, instruction) =>
        {
            if (label.Equals("Insert2"))
            {
                return MakeBaseDeconstructInstructionsToAdd(false);
            }
            return null;
        });

    //

    public static void GhostCreated(Constructable constructable)
    {
        GameObject ghostObject = constructable.gameObject;
        Log.Debug($"GhostCreated: {ghostObject.name} under: {(ghostObject.transform.parent ? ghostObject.transform.parent.name : "nowhere")}");

        NitroxId parentId = null;
        if (ghostObject.TryGetComponentInParent(out Base @base) &&
            NitroxEntity.TryGetEntityFrom(@base.gameObject, out NitroxEntity entity))
        {
            parentId = entity.Id;
        }

        // Assign a NitroxId to the ghost now
        NitroxId ghostId = new();
        NitroxEntity.SetNewId(ghostObject, ghostId);
        if (constructable is ConstructableBase constructableBase)
        {
            GhostEntity ghost = NitroxGhost.From(constructableBase);
            ghost.Id = ghostId;
            ghost.ParentId = parentId;
            Log.Debug($"Sending ghost: {ghost}");
            Resolve<IPacketSender>().Send(new PlaceGhost(ghost));
            return;
        }
        ModuleEntity module = NitroxModule.From(constructable);
        module.Id = ghostId;
        module.ParentId = parentId;
        Log.Debug($"Sending module: {module}");
        Resolve<IPacketSender>().Send(new PlaceModule(module));
    }

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
        if (amount == 1f && constructable is ConstructableBase constructableBase)
        {
            CoroutineHost.StartCoroutine(BroadcastObjectBuilt(constructableBase, entity));
            return;
        }
        // update as a normal module
        Resolve<IPacketSender>().Send(new ModifyConstructedAmount(entity.Id, amount));
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
        BuildManager.TryTransferIdFromGhostToModule(baseGhost, entity.Id, constructableBase);

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
            UpdateBase updateBase = new(parentEntity.Id, entity.Id, NitroxBase.From(parentBase), NitroxBuild.GetChildEntities(parentBase, parentEntity.Id));

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

            Resolve<IPacketSender>().Send(new PlaceBase(entity.Id, NitroxBuild.From(targetBase)));
        }
    }

    // TODO: Should maybe rename to PieceDeconstructed
    public static void BaseDeconstructed(BaseDeconstructable baseDeconstructable, ConstructableBase constructableBase, Base @base, bool destroyed)
    {
        Log.Debug("BaseDeconstructed");
        if (!NitroxEntity.TryGetEntityFrom(@base.gameObject, out NitroxEntity baseEntity))
        {
            Log.Error("Couldn't find NitroxEntity on a deconstructed base, which is really problematic");
            return;
        }

        GhostEntity ghostEntity = NitroxGhost.From(constructableBase);
        ghostEntity.Id = baseEntity.Id;
        if (destroyed)
        {
            // Base was destroyed and replaced with a simple ghost
            Log.Debug("Transferring id from base to the new ghost");
            NitroxEntity.SetNewId(constructableBase.gameObject, baseEntity.Id);

            Log.Debug("Base destroyed and replaced by a simple ghost");
            Resolve<IPacketSender>().Send(new BaseDeconstructed(baseEntity.Id, ghostEntity));
            return;
        }
        if (!baseDeconstructable.GetComponentInParent<BaseCell>())
        {
            Log.Error("Couldn't find a BaseCell parent to the BaseDeconstructable");
            return;
        }

        // If deconstruction was ordered by BuildingTester, then we simply take the provided id
        if (BuildingTester.Main.TempId != null)
        {
            // If it had an attached module, we'll also delete the NitroxEntity from the said module similarly to the code below
            if (NitroxEntity.TryGetObjectFrom(BuildingTester.Main.TempId, out GameObject moduleObject) &&
                moduleObject.TryGetComponent(out IBaseModule baseModule) &&
                constructableBase.moduleFace.HasValue && constructableBase.moduleFace.Value.Equals(baseModule.moduleFace))
            {
                GameObject.Destroy(moduleObject.GetComponent<NitroxEntity>());
            }
            else if (constructableBase.techType.Equals(TechType.BaseMoonpool) && @base.TryGetComponent(out MoonpoolManager moonpoolManager))
            {
                moonpoolManager.DeregisterMoonpool(constructableBase.transform);
            }

            NitroxEntity.SetNewId(constructableBase.gameObject, BuildingTester.Main.TempId);
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
            if (geometryObject != null && NitroxEntity.TryGetEntityFrom((geometryObject as MonoBehaviour).gameObject, out NitroxEntity moduleEntity))
            {
                pieceId = moduleEntity.Id;
                GameObject.Destroy(moduleEntity);
                Log.Debug($"Successfully transferred NitroxEntity from module geometry {moduleEntity.Id}");
            }
        }
        else if (constructableBase.techType.Equals(TechType.BaseMoonpool) && @base.TryGetComponent(out MoonpoolManager moonpoolManager))
        {
            pieceId = moonpoolManager.DeregisterMoonpool(constructableBase.transform); // pieceId can still be null
        }
        else if (constructableBase.techType.Equals(TechType.BaseMapRoom))
        {
            Int3 mapRoomFunctionalityCell = BuildManager.GetMapRoomFunctionalityCell(constructableBase.model.GetComponent<BaseGhost>());
            MapRoomFunctionality mapRoomFunctionality = @base.GetMapRoomFunctionalityForCell(mapRoomFunctionalityCell);
            if (mapRoomFunctionality && NitroxEntity.TryGetEntityFrom(mapRoomFunctionality.gameObject, out NitroxEntity mapRoomEntity))
            {
                pieceId = mapRoomEntity.Id;
            }
            else
            {
                Log.Error("Either couldn't find a MapRoomFunctionality associated with destroyed piece or couldn't find a NitroxEntity onto it.");
            }
        }
        // When a BaseWaterPark doesn't have a moduleFace, it means that there's still another WaterPark so we don't need to destroy its id and it won't be an error
        else if (!constructableBase.techType.Equals(TechType.BaseWaterPark))
        {
            Log.Error("Couldn't find the module's GameObject of IBaseModuleGeometry when transfering the NitroxEntity");
        }

        // Else, if it's a local client deconstruction, we generate a new one
        pieceId ??= new();
        NitroxEntity.SetNewId(constructableBase.gameObject, pieceId);
        ghostEntity.Id = pieceId;
        ghostEntity.ParentId = baseEntity.Id;

        PieceDeconstructed pieceDeconstructed = BuildingTester.Main.NewWaterPark == null ?
            new PieceDeconstructed(baseEntity.Id, pieceId, cachedPieceIdentifier, ghostEntity, NitroxBase.From(@base)) :
            new WaterParkDeconstructed(baseEntity.Id, pieceId, cachedPieceIdentifier, ghostEntity, NitroxBase.From(@base), BuildingTester.Main.NewWaterPark);
        Log.Debug($"Base is not empty, sending packet {pieceDeconstructed}");

        Resolve<IPacketSender>().Send(pieceDeconstructed);
        BuildingTester.Main.NewWaterPark = null;
    }

    public static void PostfixDeconstructionAllowed(BaseDeconstructable __instance, ref bool __result, ref string reason)
    {
        if (__result && __instance.deconstructedBase.TryGetComponent(out NitroxEntity parentId) &&
            BuildingTester.Main && BuildingTester.Main.BasesCooldown.ContainsKey(parentId.Id))
        {
            __result = false;
            // TODO: Add a cooldown so that the message is not actually spammed (or maybe we don't care)
            // TODO: Localize this string (same for below)
            reason = "You can't modify a base that was recently updated by another player";
        }
    }

    public static bool PrefixToolConstruct(Constructable c)
    {
        if (c.TryGetComponentInParent(out NitroxEntity parentId) &&
            BuildingTester.Main && BuildingTester.Main.BasesCooldown.ContainsKey(parentId.Id))
        {
            ErrorMessage.AddMessage("You can't modify a base that was recently updated by another player");
            return false;
        }
        return true;
    }

    public static void PrefixBaseDeconstruct(BaseDeconstructable __instance)
    {
        BuildManager.TryGetIdentifier(__instance, out cachedPieceIdentifier, null, __instance.face);
    }

    public override void Patch(Harmony harmony)
    {
        PatchTranspiler(harmony, TARGET_METHOD_TRYPLACE, nameof(TranspilerTryplace));
        PatchTranspiler(harmony, TARGET_METHOD_CONSTRUCT, nameof(TranspilerConstruct));
        PatchTranspiler(harmony, TARGET_METHOD_DECONSTRUCT_ASYNC, nameof(TranspilerDeconstructAsync));
        PatchPostfix(harmony, TARGET_METHOD_DECONSTRUCTION_ALLOWED, nameof(PostfixDeconstructionAllowed));
        PatchPrefix(harmony, TARGET_METHOD_TOOL_CONSTRUCT, nameof(PrefixToolConstruct));
        PatchMultiple(harmony, TARGET_METHOD_DECONSTRUCT, prefixMethod: nameof(PrefixBaseDeconstruct), transpilerMethod: nameof(TranspilerBaseDeconstruct));
    }
}
