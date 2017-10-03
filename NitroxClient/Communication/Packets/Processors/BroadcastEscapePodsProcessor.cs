using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Helper.GameLogic;
using NitroxModel.Helper.Unity;
using NitroxModel.Packets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

                    if (rigidbody != null)
                    {
                        Console.WriteLine("Freezing escape pod rigidbody");
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
                escapePod = UnityEngine.Object.Instantiate(EscapePod.main.gameObject);
            }

            escapePod.transform.position = model.Location;

            StorageContainer storageContainer = escapePod.RequireComponentInChildren<StorageContainer>();
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
