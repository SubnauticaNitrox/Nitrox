using System.Reflection;
using HarmonyLib;
using NitroxClient.MonoBehaviours;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic
{
    class Base_CopyFrom_Patch : NitroxPatch, IDynamicPatch
    {
        public readonly MethodInfo TARGET_METHOD = Reflect.Method((Base t) => t.CopyFrom(default(Base), default(Int3.Bounds), default(Int3)));

        public static void Prefix(Base __instance, Base sourceBase)
        {
            NitroxEntity entity = sourceBase.GetComponent<NitroxEntity>();

            // The game will clone the base when doing things like rebuilding gemometry or placing a new
            // piece.  The copy is normally between a base ghost and a base - and vise versa.  When building
            // a face piece, such as a window, this will clone a ghost base to stage the change which is later
            // integrated into the real base.  For now, prevent guid copies to these staging ghost bases; however,
            // there is still a pending edge case when a base converts to a BaseGhost for deconstruction.
            if (entity != null && __instance.gameObject.name != "BaseGhost")
            {
                Log.Debug("Transfering base id : " + entity.Id + " from " + sourceBase.name + " to " + __instance.name);
                NitroxEntity.SetNewId(__instance.gameObject, entity.Id);
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
