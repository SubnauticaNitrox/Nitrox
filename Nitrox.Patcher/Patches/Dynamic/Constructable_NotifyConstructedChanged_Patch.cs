using System;
using System.Reflection;
using Harmony;
using Nitrox.Client.GameLogic;
using Nitrox.Client.GameLogic.Helper;
using Nitrox.Client.MonoBehaviours;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Logger;
using static Nitrox.Client.GameLogic.Helper.TransientLocalObjectManager;


namespace Nitrox.Patcher.Patches.Dynamic
{
    public class Constructable_NotifyConstructedChanged_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(Constructable);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("NotifyConstructedChanged", BindingFlags.NonPublic | BindingFlags.Instance);

        public static void Postfix(Constructable __instance)
        {
            if (!__instance._constructed && __instance.constructedAmount == 1f)
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
        }
        public override void Patch(HarmonyInstance harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
