using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.GameLogic.Spawning.Metadata.Extractor;
using NitroxClient.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.DataStructures;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class AvoidEscapePod_StopPerform_Patch : NitroxPatch, IDynamicPatch
{
    internal static readonly MethodInfo TARGET_METHOD = Reflect.Method((AvoidEscapePod t) => t.StopPerform(default(Creature), default(float)));

    /**
     * if (EscapePod.main != null)
     * {
     *      Vector3 position = EscapePod.main.transform.position;
     *      if (Vector3.Distance(position, base.transform.position) > Vector3.Distance(position, creature.leashPosition))
     *      {
     *          creature.leashPosition = base.transform.position;
     *          AvoidEscapePod_StopPerform_Patch.BroadcastChange(creature);                          <======= [INSERTED LINE]
     *      }
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
