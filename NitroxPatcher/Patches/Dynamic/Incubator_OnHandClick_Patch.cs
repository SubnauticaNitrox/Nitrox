using System;
using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic
{
    // The incubator class is attached to the main terminal window.  This is what the player clicks
    // to hatch the eggs using the enzymes.
    public class Incubator_OnHandClick_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(Incubator);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("OnHandClick", BindingFlags.Public | BindingFlags.Instance);

        public static void Prefix(Incubator __instance)
        {
            if (__instance.powered && !__instance.hatched && Inventory.main.container.Contains(TechType.HatchingEnzymes))
            {
                // the server only knows about the the main incubator platform which is the direct parent
                GameObject platform = __instance.gameObject.transform.parent.gameObject;
                NitroxId id = NitroxEntity.GetId(platform);
                IncubatorMetadata metadata = new IncubatorMetadata(true, true);

                Entities entities = NitroxServiceLocator.LocateService<Entities>();
                entities.BroadcastMetadataUpdate(id, metadata);
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
