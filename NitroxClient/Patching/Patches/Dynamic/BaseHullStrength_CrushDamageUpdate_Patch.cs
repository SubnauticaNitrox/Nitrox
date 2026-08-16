using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Bases;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;

namespace NitroxClient.Patching.Patches.Dynamic;

internal sealed partial class BaseHullStrength_CrushDamageUpdate_Patch : NitroxPatch, IDynamicPatch
{
    private static SimulationOwnership simulationOwnership;
    private static LiveMixinManager liveMixinManager;
    private static Entities entities;
    internal static readonly MethodInfo TARGET_METHOD = Reflect.Method((BaseHullStrength t) => t.CrushDamageUpdate());

    public BaseHullStrength_CrushDamageUpdate_Patch(SimulationOwnership so, LiveMixinManager lmm, Entities e)
    {
        simulationOwnership = so ?? throw new ArgumentNullException(nameof(so));
        liveMixinManager = lmm ?? throw new ArgumentNullException(nameof(lmm));
        entities = e ?? throw new ArgumentNullException(nameof(e));
    }

    public static bool Prefix(BaseHullStrength __instance)
    {
        return __instance.TryGetNitroxId(out NitroxId baseId) && simulationOwnership.HasAnyLockType(baseId);
    }

    /*
     * }
     * ErrorMessage.AddMessage(Language.main.GetFormat<float>("BaseHullStrDamageDetected", this.totalStrength));
     * BroadcastChange(this, random);           <------ Inserted line
     */
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        // We add instructions right before the ret which is equivalent to inserting at last offset
        return new CodeMatcher(instructions).End()
                                            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
                                            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_0))
                                            .Insert(new CodeInstruction(OpCodes.Call, Reflect.Method(() => BroadcastChange(default, default))))
                                            .InstructionEnumeration();
    }

    public static void BroadcastChange(BaseHullStrength baseHullStrength, LiveMixin liveMixin)
    {
        if (!baseHullStrength.TryGetNitroxId(out NitroxId baseId))
        {
            return;
        }
        BaseCell baseCell = liveMixin.GetComponent<BaseCell>();
        Int3 relativeCell = baseCell.cell - baseHullStrength.baseComp.anchor;
        BaseLeakManager baseLeakManager = baseHullStrength.gameObject.EnsureComponent<BaseLeakManager>();
        if (!baseLeakManager.TryGetAbsoluteCellId(baseCell.cell, out NitroxId leakId))
        {
            leakId = new();
        }
        baseLeakManager.EnsureLeak(relativeCell, leakId, liveMixin.health, liveMixinManager);

        if (!liveMixinManager.IsRemoteHealthChanging)
        {
            BaseLeakEntity baseLeakEntity = new(liveMixin.health, relativeCell.ToDto(), leakId, baseId);
            entities.BroadcastEntitySpawnedByClient(baseLeakEntity, true);
        }
    }
}
