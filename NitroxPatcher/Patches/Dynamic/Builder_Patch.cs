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
            SavedGhost ghost = NitroxGhost.From(constructableBase);
            ghost.NitroxId = ghostId;
            Log.Debug($"Sending ghost: {ghost}");
            Resolve<IPacketSender>().Send(new PlaceGhost(parentId, ghost));
            return;
        }
        SavedModule module = NitroxModule.From(constructable);
        module.NitroxId = ghostId;
        Log.Debug($"Sending module: {module}");
        Resolve<IPacketSender>().Send(new PlaceModule(parentId, module));
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
        if (amount == 1.0f && constructable is ConstructableBase constructableBase)
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
            parentBase = constructableBase.GetComponentInParent<Base>();
        }

        // Have a delay for baseGhost to be actually destroyed
        yield return null;

        if (parentBase)
        {
            // update existing base
            // TODO: (for server-side) Find a way to optimize this (maybe by copying BaseGhost.Finish())
            if (!NitroxEntity.TryGetEntityFrom(parentBase.gameObject, out NitroxEntity parentEntity))
            {
                // TODO: Probably add a resync here
                Log.Error("Parent base doesn't have a NitroxEntity, which is not normal");
                yield break;
            }

            Resolve<IPacketSender>().Send(new UpdateBase(parentEntity.Id, entity.Id, NitroxBuild.From(parentBase)));
        }
        else
        {
            // create a new base   
            NitroxEntity.SetNewId(baseGhost.targetBase.gameObject, entity.Id);

            Resolve<IPacketSender>().Send(new PlaceBase(entity.Id, NitroxBuild.From(targetBase)));
        }
    }

    public static void BaseDeconstructed(BaseDeconstructable baseDeconstructable, ConstructableBase constructableBase, Base @base, bool destroyed)
    {
        Log.Debug("HEHEHEHA");
        if (!NitroxEntity.TryGetEntityFrom(@base.gameObject, out NitroxEntity baseEntity))
        {
            Log.Error("Couldn't find NitroxEntity on a destructed base, which is really problematic");
            return;
        }

        if (destroyed)
        {
            // Base was destroyed and replaced with a simple ghost
            Log.Debug("Transferring id from base to the new ghost");
            NitroxEntity.SetNewId(constructableBase.gameObject, baseEntity.Id);

            Log.Debug("Base destroyed and replaced by a simple ghost");
            Resolve<IPacketSender>().Send(new BaseDeconstructed(baseEntity.Id, NitroxGhost.From(constructableBase)));
            return;
        }
        if (!baseDeconstructable.TryGetComponentInParent(out BaseCell baseCell))
        {
            Log.Error("Couldn't find a BaseCell parent to the BaseDeconstructable");
            return;
        }

        // If deconstruction was ordered by BuildingTester, then we simply take the provided id
        if (BuildingTester.Main.TempId != null)
        {
            NitroxEntity.SetNewId(constructableBase.gameObject, BuildingTester.Main.TempId);
            // We don't need to go any further
            return;
        }
        // Else, if it's a local client deconstruction, we generate a new one
        NitroxId pieceId = new();
        NitroxEntity.SetNewId(constructableBase.gameObject, pieceId);
        
        PieceDeconstructed pieceDeconstructed = new(baseEntity.Id, pieceId, cachedPieceIdentifier, NitroxGhost.From(constructableBase));
        Log.Debug($"Base is not empty, sending packet {pieceDeconstructed}");

        Resolve<IPacketSender>().Send(pieceDeconstructed);
    }

    public static void PostfixDeconstructionAllowed(BaseDeconstructable __instance, ref bool __result, ref string reason)
    {
        if (__result && __instance.TryGetComponentInParent(out NitroxEntity parentId) &&
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
