using System.Reflection;
using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxModel.Helper;
using NitroxModel.Packets;
using Story;

namespace NitroxPatcher.Patches.Dynamic
{
    public class StoryGoal_Execute_Patch : NitroxPatch, IDynamicPatch
    {
#if SUBNAUTICA
        public static readonly MethodInfo TARGET_METHOD = ReflectionHelper.GetMethodInfo(() => StoryGoal.Execute(default(string), default(Story.GoalType)));
#elif BELOWZERO
        public static readonly MethodInfo TARGET_METHOD = ReflectionHelper.GetMethodInfo(() => StoryGoal.Execute(default(string), default(Story.GoalType), default(bool), default(bool)));
#endif

        public static void Prefix(string key, Story.GoalType goalType)
        {
            if (!StoryGoalManager.main.completedGoals.Contains(key))
            {
                StoryEventSend packet = new((StoryEventSend.EventType)goalType, key);
                Resolve<IPacketSender>().Send(packet);
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
