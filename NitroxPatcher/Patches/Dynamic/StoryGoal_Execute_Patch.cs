using System.Reflection;
using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxModel.Core;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures.GameLogic;
using Story;

namespace NitroxPatcher.Patches.Dynamic
{
    public class StoryGoal_Execute_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = typeof(StoryGoal).GetMethod("Execute", BindingFlags.Public | BindingFlags.Static);

        public static bool Prefix(string key, Story.GoalType goalType)
        {
            //To avoid duplicate events, we're skipping the code
            if (StoryGoalWhitelist.SimulatedByServer.Contains(key))
            {
                return false;
            }

            if (!StoryGoalManager.main.completedGoals.Contains(key))
            {
                StoryEventSend packet = new((StoryEventType) goalType, key);
                NitroxServiceLocator.LocateService<IPacketSender>().Send(packet);
            }

            return true;
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
