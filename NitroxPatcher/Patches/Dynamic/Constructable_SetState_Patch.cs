using System;
using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic.Helper;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;
using static NitroxClient.GameLogic.Helper.TransientLocalObjectManager;

namespace NitroxPatcher.Patches.Dynamic
{
    public class Constructable_SetState_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(Constructable);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("SetState", BindingFlags.Public | BindingFlags.Instance);

        public static void Postfix(Constructable __instance)
        {
            /**
             * On base pieces we need to have special logic during deconstruction to transfer the id from the main 
             * object to the ghost.  This method will see if we have a stored LATEST_DECONSTRUCTED_BASE_PIECE_GUID 
             * (set from deconstructor patch or incoming deconstruction packet).  If we do, then we'll copy the id 
             * to the new ghost.
             */
            Optional<object> opId = TransientLocalObjectManager.Get(TransientObjectType.LATEST_DECONSTRUCTED_BASE_PIECE_GUID);

            if (opId.HasValue)
            {
                TransientLocalObjectManager.Add(TransientObjectType.LATEST_DECONSTRUCTED_BASE_PIECE_GUID, null);

                NitroxId id = (NitroxId)opId.Value;
                Log.Debug($"Setting ghost id via Constructable_SetState_Patch {id}");
                NitroxEntity.SetNewId(__instance.gameObject, id);
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}

