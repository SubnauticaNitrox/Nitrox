using System;
using System.Reflection;
using NitroxClient.GameLogic;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using static NitroxClient.GameLogic.Helper.TransientLocalObjectManager;


namespace NitroxPatcher.Patches.Dynamic
{
    public sealed partial class Constructable_NotifyConstructedChanged_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = Reflect.Method((Constructable t) => t.NotifyConstructedChanged(default(bool)));

        public static void Postfix(Constructable __instance)
        {
            if (!__instance._constructed && Math.Abs(__instance.constructedAmount - 1f) < 0.0002)
            {
                Optional<object> opId = Get(TransientObjectType.LATEST_DECONSTRUCTED_BASE_PIECE_GUID);

                NitroxId id;

                // Check to see if they are trying to deconstruct a base piece.  If so, we will need to use the
                // id in LATEST_DECONSTRUCTED_BASE_PIECE_GUID because base pieces get destroyed and recreated with
                // a ghost (furniture just uses the same game object).
                if (opId.HasValue)
                {
                    // base piece, get id before ghost appeared
                    id = (NitroxId)opId.Value;
                    Log.Debug($"Deconstructing base piece with id: {id}");
                }
                else
                {
                    // furniture, just use the same object to get the id
                    if (!__instance.TryGetIdOrWarn(out id))
                    {
                        return;
                    }
                    Log.Debug($"Deconstructing furniture with id: {id}");
                }

                NitroxServiceLocator.LocateService<Building>().DeconstructionBegin(id);
            }
        }
    }
}
