using System;
using System.Reflection;
using Harmony;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Packets;

namespace NitroxPatcher.Patches.Dynamic
{
    public class Constructable_Construct_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(Constructable);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("Construct");

        private static Base lastTargetBase;
        private static Int3 lastTargetBaseOffset;

        public static bool Prefix(Constructable __instance, bool __result)
        {

            if (NitroxServiceLocator.LocateService<Building>().remoteEventActive)
            {
                if (__instance.constructed)
                {
                    __result = false;
                }
                else
                {
                    System.Reflection.MethodInfo updateMaterial = typeof(Constructable).GetMethod("UpdateMaterial", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    Validate.NotNull(updateMaterial);
                    updateMaterial.Invoke(__instance, new object[] { });
                    if (__instance.constructedAmount >= 1f)
                    {
                        __instance.SetState(true, true);
                    }
                    __result = true;
                }
                return false;
            }
            return true;
        }

        public static void Postfix(Constructable __instance, bool __result)
        {
            if (__result && __instance.constructedAmount >= 1.0f)
            {

                //Check if we raised the event by using our own BuilderTool or if it came as post Event of a Remote-Action or Init-Action
                if (NitroxServiceLocator.LocateService<Building>().lastHoveredConstructable != null && NitroxServiceLocator.LocateService<Building>().lastHoveredConstructable == __instance && NitroxServiceLocator.LocateService<Building>().currentlyHandlingBuilderTool && !NitroxServiceLocator.LocateService<Building>().remoteEventActive)
                {

                    if (__result && __instance.constructedAmount < 1f)
                    {
                        NitroxId id = NitroxEntity.GetId(__instance.gameObject);

#if TRACE && BUILDING
                    NitroxModel.Logger.Log.Debug("Constructable_Construct_Post - sending notify for self constructing object - id: " + id + " amount: " + instance.constructedAmount);
#endif
                        ConstructionAmountChanged amountChanged = new ConstructionAmountChanged(id, __instance.constructedAmount);
                        NitroxServiceLocator.LocateService<IPacketSender>().Send(amountChanged);
                    }
                }

                if (__result && __instance.constructedAmount == 1f && NitroxServiceLocator.LocateService<Building>().remoteEventActive)
                {

#if TRACE && BUILDING
                NitroxModel.Logger.Log.Debug("Constructable_Construct_Post - finished construct remote");
#endif

                }
                
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchMultiple(harmony, TARGET_METHOD, true, true, false);
        }
    }
}
