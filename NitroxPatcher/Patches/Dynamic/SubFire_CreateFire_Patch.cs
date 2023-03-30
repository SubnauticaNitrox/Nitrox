using System;
using System.Reflection;
using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic
{
    /// <summary>
    ///     Hook onto <see cref="SubFire.CreateFire(SubFire.RoomFire)" />. We do this on the fire creation method because
    ///     unlike <see cref="SubRoot.OnTakeDamage(DamageInfo)" />, fires
    ///     can be created outside of <see cref="SubFire.OnTakeDamage(DamageInfo)" />
    /// </summary>
    internal class SubFire_CreateFire_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method((SubFire t) => t.CreateFire(default(SubFire.RoomFire)));

        public static bool Prefix(SubFire __instance, SubFire.RoomFire startInRoom, out bool __state)
        {
            __state = NitroxServiceLocator.LocateService<SimulationOwnership>().HasAnyLockType(NitroxEntity.GetId(__instance.subRoot.gameObject));

            // Block any new fires if this player is not the owner
            return __state;
        }

        public static void Postfix(SubFire __instance, SubFire.RoomFire startInRoom, bool __state)
        {
            // Spent way too much time trying to get around a bug in dnspy that doesn't allow me to propery edit this method, so I'm going with the hacky solution.
            // Every time a Fire is created, the whole list of SubFire.availableNodes is cleared, then populated with any transforms that have 0 childCount. 
            // Next, it chooses a random index, then spawns a fire in that node.
            // We can easily find where it is because it'll be the only Transform in SubFire.availableNodes with a childCount > 0
            if (__state)
            {
                foreach (Transform transform in __instance.availableNodes)
                {
                    if (transform.childCount > 0)
                    {
                        int nodeIndex = Array.IndexOf(__instance.roomFires[startInRoom.roomLinks.room].spawnNodes, transform);
                        Fire fire = transform.GetComponentInChildren<Fire>();

                        NitroxId subRootId = NitroxEntity.GetId(fire.fireSubRoot.gameObject);
                        NitroxId fireId = NitroxEntity.GetId(fire.gameObject);

                        CyclopsFireEntity cyclopsFireEntity = new((int)startInRoom.roomLinks.room, nodeIndex, fireId, TechType.None.ToDto(), null, subRootId, new());
                        Resolve<IPacketSender>().Send(new EntitySpawnedByClient(cyclopsFireEntity));

                        return;
                    }
                }
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchMultiple(harmony, TARGET_METHOD, prefix:true, postfix:true);
        }
    }
}
