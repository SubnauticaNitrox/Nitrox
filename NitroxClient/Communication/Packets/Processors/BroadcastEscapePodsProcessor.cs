using System.Collections.Generic;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Logger;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class BroadcastEscapePodsProcessor : ClientPacketProcessor<BroadcastEscapePods>
    {
        public static bool SURPRESS_ESCAPE_POD_AWAKE_METHOD;

        private readonly IPacketSender packetSender;
        private IMultiplayerSession multiplayerSession;
        private readonly Vector3 playerSpawnRelativeToEscapePodPosition = new Vector3(0.9f, 2.1f, 0);
        private readonly Dictionary<string, GameObject> escapePodsByGuid = new Dictionary<string, GameObject>();

        private string myEscapePodGuid;

        public BroadcastEscapePodsProcessor(IPacketSender packetSender, IMultiplayerSession multiplayerSession)
        {
            this.packetSender = packetSender;
            this.multiplayerSession = multiplayerSession;
        }

        public override void Process(BroadcastEscapePods packet)
        {
            if (myEscapePodGuid == null)
            {
                AssignPlayerToEscapePod(packet);
            }

            SyncEscapePodGuids(packet);
        }

        private void AssignPlayerToEscapePod(BroadcastEscapePods packet)
        {
            foreach (EscapePodModel model in packet.EscapePods)
            {
                if (model.AssignedPlayers.Contains(multiplayerSession.Reservation.PlayerId))
                {
                    EscapePod.main.transform.position = model.Location;
                    EscapePod.main.playerSpawn.position = model.Location + playerSpawnRelativeToEscapePodPosition;

                    Rigidbody rigidbody = EscapePod.main.GetComponent<Rigidbody>();

                    if (rigidbody != null)
                    {
                        Log.Debug("Freezing escape pod rigidbody");
                        rigidbody.constraints = RigidbodyConstraints.FreezeAll;
                    }
                    else
                    {
                        Log.Error("Escape pod did not have a rigid body!");
                    }

                    Player.main.transform.position = EscapePod.main.playerSpawn.position;
                    Player.main.transform.rotation = EscapePod.main.playerSpawn.rotation;

                    myEscapePodGuid = model.Guid;
                    break;
                }
            }
        }

        // Done in a coroutine because we want to wait until the escape pod has fully loaded
        // at the beginning of the game.
        private void SyncEscapePodGuids(BroadcastEscapePods packet)
        {
            foreach (EscapePodModel model in packet.EscapePods)
            {
                if (!escapePodsByGuid.ContainsKey(model.Guid))
                {
                    escapePodsByGuid[model.Guid] = CreateNewEscapePod(model);
                }
            }
        }

        private GameObject CreateNewEscapePod(EscapePodModel model)
        {
            SURPRESS_ESCAPE_POD_AWAKE_METHOD = true;

            GameObject escapePod;

            if (model.Guid == myEscapePodGuid)
            {
                escapePod = EscapePod.main.gameObject;
            }
            else
            {
                escapePod = Object.Instantiate(EscapePod.main.gameObject);
            }

            escapePod.transform.position = model.Location;

            StorageContainer storageContainer = escapePod.RequireComponentInChildren<StorageContainer>();

            using (packetSender.Suppress<ItemContainerRemove>())
            {
                storageContainer.container.Clear();
            }

            GuidHelper.SetNewGuid(storageContainer.gameObject, model.StorageContainerGuid);

            MedicalCabinet medicalCabinet = escapePod.RequireComponentInChildren<MedicalCabinet>();
            GuidHelper.SetNewGuid(medicalCabinet.gameObject, model.MedicalFabricatorGuid);

            Fabricator fabricator = escapePod.RequireComponentInChildren<Fabricator>();
            GuidHelper.SetNewGuid(fabricator.gameObject, model.FabricatorGuid);

            Radio radio = escapePod.RequireComponentInChildren<Radio>();
            GuidHelper.SetNewGuid(radio.gameObject, model.RadioGuid);

            return escapePod;
        }
    }
}
