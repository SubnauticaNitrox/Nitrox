using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Simulation;
using NitroxClient.MonoBehaviours.Gui.HUD;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using NitroxModel.Packets;

namespace NitroxPatcher.Patches.Dynamic
{
    /// <summary>
    /// Synchronizes interactions with StoryHandTarget objects such as PDAs.
    /// Ensures only one player can interact with an object at a time by requesting a simulation lock.
    /// On a successful interaction, broadcasts the object's destruction to all clients.
    /// </summary>
    public sealed partial class StoryHandTarget_OnHandClick_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method((StoryHandTarget t) => t.OnHandClick(default(GUIHand)));

        // If true, then a lock is acquired and the original method is invoked.
        private static bool skipPrefix;

        public static bool Prefix(StoryHandTarget __instance, GUIHand hand)
        {
            if (!__instance.TryGetIdOrWarn(out NitroxId id) || Resolve<SimulationOwnership>().HasExclusiveLock(id) || skipPrefix)
            {
                return true;
            }

            HandInteraction<StoryHandTarget> context = new(__instance, hand);
            LockRequest<HandInteraction<StoryHandTarget>> lockRequest = new(id, SimulationLockType.EXCLUSIVE, ReceivedSimulationLockResponse, context);
            Resolve<SimulationOwnership>().RequestSimulationLock(lockRequest);

            return false;
        }

        private static void ReceivedSimulationLockResponse(NitroxId id, bool lockAcquired, HandInteraction<StoryHandTarget> context)
        {
            StoryHandTarget storyHandTarget = context.Target;

            if (lockAcquired)
            {
                skipPrefix = true;
                storyHandTarget.OnHandClick(context.GuiHand);
                skipPrefix = false;
                
                NitroxServiceLocator.LocateService<IPacketSender>().Send(new EntityDestroyed(id));
            }
            else
            {
                storyHandTarget.gameObject.AddComponent<DenyOwnershipHand>();
                storyHandTarget.isValidHandTarget = false;
            }
        }
    }
}
