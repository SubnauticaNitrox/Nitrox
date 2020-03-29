using System;
using System.Reflection;
using Harmony;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Helper;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
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

        public static bool Prefix(Constructable __instance, bool value)
        {
            // check to see if they are trying to update constructed value from true to false.  This means that they
            // are trying to deconstruct an object.
            if (__instance.constructed && value == false)
            {
                Optional<object> opId = TransientLocalObjectManager.Get(TransientObjectType.LATEST_DECONSTRUCTED_BASE_PIECE_GUID);

                NitroxId id;

                // Check to see if they are trying to deconstruct a base piece.  If so, we will need to use the 
                // id in LATEST_DECONSTRUCTED_BASE_PIECE_GUID because base pieces get destroyed and recreated with
                // a ghost (furniture just uses the same game object).
                if (opId.HasValue)
                {
                    // base piece, get id before ghost appeared
                    id = (NitroxId)opId.Value;
                    Log.Info("Deconstructing base piece with id: " + id);
                }
                else
                {
                    // furniture, just use the same object to get the id
                    id = NitroxEntity.GetId(__instance.gameObject);
                    Log.Info("Deconstructing furniture with id: " + id);
                }

                NitroxServiceLocator.LocateService<Building>().DeconstructionBegin(id);
            }

            return true;
        }

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
                Log.Info("Setting ghost id via Constructable_SetState_Patch " + id);
                NitroxEntity.SetNewId(__instance.gameObject, id);
            }
        }
        
        public override void Patch(HarmonyInstance harmony)
        {
            PatchMultiple(harmony, TARGET_METHOD, true, true, false);
        }
    }
}

