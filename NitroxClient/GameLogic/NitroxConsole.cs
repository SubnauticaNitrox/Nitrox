using System;
using NitroxClient.Communication.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.Packets;
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
            TechType techType = GetObjectTechType(gameObject);

            try
            {
                if (VehicleHelper.IsVehicle(techType))
                {
                    SpawnVehicle(gameObject, techType);
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
        /// Spawns Seamoth, Exosuit and Cyclops
        /// </summary>
        private void SpawnVehicle(GameObject gameObject, TechType techType)
        {
            NitroxId id = NitroxEntity.GetIdOrGenerateNew(gameObject);

            VehicleWorldEntity vehicleEntity = Vehicles.MakeVehicleEntity(gameObject, id, techType);
            
            packetSender.Send(new EntitySpawnedByClient(vehicleEntity));

            Log.Debug($"Spawning vehicle {techType} with id {id} at {gameObject.transform.position}");
        }

        /// <summary>
        /// Spawns a Pickupable item
        /// </summary>
        private void SpawnItem(GameObject gameObject)
        {
            if (gameObject.TryGetComponent(out Pickupable pickupable))
            {
                Log.Debug($"Spawning item {pickupable.GetTechName()} at {gameObject.transform.position}");
                item.Dropped(gameObject, pickupable.GetTechType());
            }
        }

        private static TechType GetObjectTechType(GameObject gameObject)
        {
            TechType techType = CraftData.GetTechType(gameObject);
            if (techType != TechType.None)
            {
                return techType;
            }

            // Cyclops' GameObject doesn't have a way to give its a TechType so we detect it differently
            if (gameObject.TryGetComponent(out SubRoot subRoot) && subRoot.isCyclops)
            {
                return TechType.Cyclops;
            }

            return TechType.None;
        }
    }
}
