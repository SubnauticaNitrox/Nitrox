using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.Packets;
using NitroxModel.DataStructures.GameLogic;
using System;
using UnityEngine;
using System.Collections.Generic;
using NitroxClient.GameLogic.Helper;
using NitroxModel.Helper;
using System.Collections;

namespace NitroxClient.Communication.Packets.Processors
{
    public class BroadcastEscapePodsProcessor : ClientPacketProcessor<BroadcastEscapePods>
    {
        public static bool SURPRESS_ESCAPE_POD_AWAKE_METHOD = false;

        private String myEscapePodGuid;
        private PacketSender packetSender;
        private Vector3 playerSpawnRelativeToEscapePodPosition = new Vector3(0.9f, 2.1f, 0);
        private Dictionary<String, GameObject> escapePodsByGuid = new Dictionary<String, GameObject>();

        public BroadcastEscapePodsProcessor(PacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override void Process(BroadcastEscapePods packet)
        {
            if (myEscapePodGuid == null)
            {
                AssignPlayerToEscapePod(packet);
            }
            
            Player.main.StartCoroutine(SyncEscapePodGuids(packet));
        }

        private void AssignPlayerToEscapePod(BroadcastEscapePods packet)
        {
            foreach (EscapePodModel model in packet.EscapePods)
            {
                if (model.AssignedPlayers.Contains(packetSender.PlayerId))
                {
                    EscapePod.main.transform.position = model.Location;
                    EscapePod.main.playerSpawn.position = model.Location + playerSpawnRelativeToEscapePodPosition;

                    Rigidbody rigidbody = EscapePod.main.GetComponent<Rigidbody>();

                    if(rigidbody != null)
                    {
                        Console.WriteLine("Freezing rigidbody");
                        rigidbody.constraints = RigidbodyConstraints.FreezeAll;
                    }
                    else
                    {
                        Console.WriteLine("Escape pod did not have a rigid body!");
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
        private IEnumerator SyncEscapePodGuids(BroadcastEscapePods packet)
        {
            yield return new WaitUntil(() => EscapePod.main.gameObject);

            foreach (EscapePodModel model in packet.EscapePods)
            {
                if(!escapePodsByGuid.ContainsKey(model.Guid))
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
                escapePod = UnityEngine.Object.Instantiate(EscapePod.main.gameObject);
            }

            escapePod.transform.position = model.Location;

            StorageContainer storageContainer = escapePod.GetComponentInChildren<StorageContainer>();
            Validate.NotNull(storageContainer, "StorageContainer can not be null");
            GuidHelper.SetNewGuid(storageContainer.gameObject, model.StorageContainerGuid);

            MedicalCabinet medicalCabinet = escapePod.GetComponentInChildren<MedicalCabinet>();
            Validate.NotNull(medicalCabinet, "medicalCabinet can not be null");
            GuidHelper.SetNewGuid(medicalCabinet.gameObject, model.MedicalFabricatorGuid);

            Fabricator fabricator = escapePod.GetComponentInChildren<Fabricator>();
            Validate.NotNull(fabricator, "fabricator can not be null");
            GuidHelper.SetNewGuid(fabricator.gameObject, model.FabricatorGuid);

            Radio radio = escapePod.GetComponentInChildren<Radio>();
            Validate.NotNull(radio, "radio can not be null");
            GuidHelper.SetNewGuid(radio.gameObject, model.RadioGuid);

            return escapePod;
        }
    }
}
