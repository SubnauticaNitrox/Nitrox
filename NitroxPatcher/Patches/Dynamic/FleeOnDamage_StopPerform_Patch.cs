using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.GameLogic.Spawning.Metadata.Extractor;
using NitroxClient.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.Helper;
using NitroxModel.DataStructures;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class FleeOnDamage_StopPerform_Patch : NitroxPatch, IDynamicPatch
{
    internal static readonly MethodInfo TARGET_METHOD = Reflect.Method((FleeOnDamage t) => t.StopPerform(default, default));

    /**
     * this.accumulatedDamage = 0f;
     * if (this.breakLeash)
     * {
     *      creature.leashPosition = base.transform.position;
     *      FleeOnDamage_StopPerform_Patch.BroadcastChange(creature);                          <======= [INSERTED LINE]
     * }

     **/
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions).MatchEndForward(new CodeMatch(OpCodes.Stfld, Reflect.Field((Creature t) => t.leashPosition)))
                                            .Advance(1)
                                            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_1))
                                            .Insert(new CodeInstruction(OpCodes.Call, Reflect.Method(() => BroadcastChange(default))))
                                            .InstructionEnumeration();
    }

    public static void BroadcastChange(Creature creature)
    {
        if (creature.TryGetNitroxId(out NitroxId creatureId))
        {
            StayAtLeashPositionMetadata metadata = Resolve<StayAtLeashPositionMetadataExtractor>().Extract(creature);
            Resolve<Entities>().BroadcastMetadataUpdate(creatureId, metadata);
        }
    }
}
