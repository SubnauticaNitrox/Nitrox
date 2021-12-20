using System.Reflection;
using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxModel.Helper;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using Story;

namespace NitroxPatcher.Patches.Dynamic
{
    public class StoryGoal_Execute_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method(() => StoryGoal.Execute(default(string), default(Story.GoalType)));

        public static void Prefix(string key, Story.GoalType goalType)
        {
            if (!StoryGoalManager.main.completedGoals.Contains(key))
            {
                StoryEventSend packet = new(goalType.ToDto(), key);
                Resolve<IPacketSender>().Send(packet);
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
