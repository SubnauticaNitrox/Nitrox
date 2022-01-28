using System.Reflection;
using HarmonyLib;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using static NitroxClient.GameLogic.Helper.TransientLocalObjectManager;

namespace NitroxPatcher.Patches.Dynamic
{
    public class Constructable_SetState_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = Reflect.Method((Constructable t) => t.SetState(default(bool), default(bool)));

        public static void Postfix(Constructable __instance)
        {
            /**
             * On base pieces we need to have special logic during deconstruction to transfer the id from the main 
             * object to the ghost.  This method will see if we have a stored LATEST_DECONSTRUCTED_BASE_PIECE_GUID 
             * (set from deconstructor patch or incoming deconstruction packet).  If we do, then we'll copy the id 
             * to the new ghost.
             */
            Optional<object> opId = Get(TransientObjectType.LATEST_DECONSTRUCTED_BASE_PIECE_GUID);

            if (opId.HasValue)
            {
                Remove(TransientObjectType.LATEST_DECONSTRUCTED_BASE_PIECE_GUID);

                NitroxId id = (NitroxId)opId.Value;
                // TornacTODO: Check this gameobject to see why it can't be removed its nitrox entity
                Log.Debug($"Setting ghost id via Constructable_SetState_Patch {id} of GameObject: {__instance.gameObject}");
                NitroxEntity.SetNewId(__instance.gameObject, id);
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
