using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Bases.EntityUtils;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.GameLogic.Entities.Bases;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using NitroxModel.Packets;
using NitroxPatcher.PatternMatching;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using NitroxClient.Unity.Helper;
using static System.Reflection.Emit.OpCodes;

namespace NitroxPatcher.Patches.Dynamic;

internal class Builder_TryPlace_Patch : NitroxPatch, IDynamicPatch
{
    internal static MethodInfo TARGET_METHOD = Reflect.Method(() => Builder.TryPlace());

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

    public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions) =>
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

    public static void GhostCreated(Constructable constructable)
    {
        GameObject ghostObject = constructable.gameObject;
        Log.Debug($"GhostCreated: {ghostObject.name} under: {(ghostObject.transform.parent ? ghostObject.transform.parent.name : "nowhere")}");

        NitroxId parentId = null;
        if (ghostObject.TryGetComponentInParent(out SubRoot subRoot) && (subRoot.isBase || subRoot.isCyclops) &&
            NitroxEntity.TryGetEntityFrom(subRoot.gameObject, out NitroxEntity entity))
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

    public override void Patch(Harmony harmony)
    {
        PatchTranspiler(harmony, TARGET_METHOD);
    }
}
