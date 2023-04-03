using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Helper;
using NitroxModel.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic
{
    internal class SpawnOnKill_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method((SpawnOnKill t) => t.OnKill());

        public static void Prefix(SpawnOnKill __instance)
        {
            if (__instance != null)
            {
                NitroxId id = NitroxEntity.GetId(__instance.gameObject);
                Resolve<IPacketSender>().Send(new EntityDestroyed(id));
                Resolve<Items>().Created(__instance.prefabToSpawn);
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
