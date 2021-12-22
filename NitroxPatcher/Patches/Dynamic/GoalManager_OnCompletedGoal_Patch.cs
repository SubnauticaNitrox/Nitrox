using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.Helper;
using NitroxModel.Packets;

namespace NitroxPatcher.Patches.Dynamic
{
    public class GoalManager_OnCompletedGoal_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method(() => GoalManager.main.OnCompleteGoal(default(GoalType), default(string)));

        public static void Postfix(GoalType goalType, string goalIdentifier)
        {            
            // Only send the completed goal if it was successfully added to the completed goals
            if (GoalManager.main.completedGoalNames.Contains(goalIdentifier))
            {                
                Resolve<IPacketSender>().Send(new GoalCompleted(goalIdentifier));
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
