using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.HUD;
using NitroxClient.GameLogic.Simulation;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using NitroxModel.Logger;

namespace NitroxPatcher.Patches.Dynamic
{
    public class MapRoomScreen_OnHandClick_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method((MapRoomScreen t) => t.OnHandClick(default(GUIHand)));
        
        private static bool skipPrefix;
        private static NitroxId currentScreenId;
        private static SimulationOwnership simulationOwnership;

        public static bool Prefix(MapRoomScreen __instance, GUIHand guiHand)
        {
            simulationOwnership ??= Resolve<SimulationOwnership>();
            if (skipPrefix)
            {
                return true;
            }

            NitroxId id = NitroxEntity.GetId(__instance.gameObject);

            if (simulationOwnership.HasExclusiveLock(id))
            {
                Log.Debug($"Already have an exclusive lock on the room screen: {id}");
                return true;
            }

            // Simulate the camera search and don't try to lock if there are no cameras available
            __instance.currentIndex = __instance.NormalizeIndex(__instance.currentIndex);
            MapRoomCamera mapRoomCamera = __instance.FindCamera(1);
            if (mapRoomCamera)
            {
                HandInteraction<MapRoomScreen> context = new(__instance, guiHand);
                LockRequest<HandInteraction<MapRoomScreen>> lockRequest = new(id, SimulationLockType.EXCLUSIVE, ReceivedSimulationLockResponse, context);

                simulationOwnership.RequestSimulationLock(lockRequest);
            }

            return false;
        }

        public static void ReleaseScreen()
        {
            simulationOwnership.RequestSimulationLock(currentScreenId, SimulationLockType.TRANSIENT);
            currentScreenId = null;
        }

        private static void ReceivedSimulationLockResponse(NitroxId id, bool lockAquired, HandInteraction<MapRoomScreen> context)
        {
            MapRoomScreen mapRoomScreen = context.Target;

            if (lockAquired)
            {
                currentScreenId = id;
                skipPrefix = true;
                mapRoomScreen.OnHandClick(context.GuiHand);
                skipPrefix = false;
            }
            else
            {
                mapRoomScreen.gameObject.AddComponent<DenyOwnershipHand>();
                mapRoomScreen.isValidHandTarget = false;
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
