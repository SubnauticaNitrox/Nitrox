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
        private readonly Items items;

        public NitroxConsole(IPacketSender packetSender, Items items)
        {
            this.packetSender = packetSender;
            this.items = items;
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
                    DefaultSpawn(gameObject);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error while trying to spawn {techType} from devconsole");
            }
        }

        /// <summary>
        /// Spawns Seamoth, Exosuit or Cyclops
        /// </summary>
        private void SpawnVehicle(GameObject gameObject, TechType techType)
        {
            NitroxId id = NitroxEntity.GetIdOrGenerateNew(gameObject);

            VehicleWorldEntity vehicleEntity = Vehicles.BuildVehicleWorldEntity(gameObject, id, techType);
            
            packetSender.Send(new EntitySpawnedByClient(vehicleEntity));

            Log.Debug($"Spawning vehicle {techType} with id {id} at {gameObject.transform.position}");
        }

        private void DefaultSpawn(GameObject gameObject)
        {
            items.Dropped(gameObject);
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
