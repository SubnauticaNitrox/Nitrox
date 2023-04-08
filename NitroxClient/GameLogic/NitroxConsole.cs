using System;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using NitroxModel_Subnautica.Helper;
using UnityEngine;

namespace NitroxClient.GameLogic
{
    public class NitroxConsole
    {
        public static bool DisableConsole { get; set; } = true;

        private readonly IPacketSender packetSender;
        private readonly Items item;

        public NitroxConsole(IPacketSender packetSender, Items item)
        {
            this.packetSender = packetSender;
            this.item = item;
        }

        //List of things that can be spawned : https://subnauticacommands.com/items
        public void Spawn(GameObject gameObject)
        {
            TechType techType = CraftData.GetTechType(gameObject);

            try
            {
                if (VehicleHelper.IsVehicle(techType))
                {
                    SpawnVehicle(gameObject);
                }
                else
                {
                    SpawnItem(gameObject);
                    //TODO: Add support for no AI creature that need to be spawned as well
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error while trying to spawn {techType} from devconsole");
            }
        }

        /// <summary>
        /// Spawns a Seamoth or an Exosuit
        /// </summary>
        private void SpawnVehicle(GameObject gameObject)
        {
            TechType techType = CraftData.GetTechType(gameObject);

            NitroxId id = NitroxEntity.GetId(gameObject);

            VehicleWorldEntity vehicleEntity = new VehicleWorldEntity(null, DayNightCycle.main.timePassedAsFloat, gameObject.transform.ToLocalDto(), "", false, id, techType.ToDto(), null);
            VehicleChildEntityHelper.PopulateChildren(id, gameObject.GetFullHierarchyPath(), vehicleEntity.ChildEntities, gameObject);

            packetSender.Send(new EntitySpawnedByClient(vehicleEntity));

            Log.Debug($"Spawning vehicle {techType} with id {techType} at {gameObject.transform.position}");
        }

        /// <summary>
        /// Spawns a Pickupable item
        /// </summary>
        private void SpawnItem(GameObject gameObject)
        {
            Optional<Pickupable> opitem = Optional.OfNullable(gameObject.GetComponent<Pickupable>());

            if (opitem.HasValue)
            {
                Log.Debug($"Spawning item {opitem.Value.GetTechName()} at {gameObject.transform.position}");
                item.Dropped(gameObject, opitem.Value.GetTechType());
            }
        }
    }
}
