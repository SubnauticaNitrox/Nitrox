using System.Collections.Generic;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Logger;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.GameLogic
{
    public class EscapePodManager
    {
        private readonly IPacketSender packetSender;
        private readonly IMultiplayerSession multiplayerSession;

        public static bool SURPRESS_ESCAPE_POD_AWAKE_METHOD;
        private readonly Vector3 playerSpawnRelativeToEscapePodPosition = new Vector3(0.9f, 2.1f, 0);
        private readonly Dictionary<string, GameObject> escapePodsByGuid = new Dictionary<string, GameObject>();

        public string MyEscapePodGuid;

        public EscapePodManager(IPacketSender packetSender, IMultiplayerSession multiplayerSession)
        {
            this.packetSender = packetSender;
            this.multiplayerSession = multiplayerSession;
        }

        public void AssignPlayerToEscapePod(EscapePodModel escapePod)
        {
            EscapePod.main.transform.position = escapePod.Location;
            EscapePod.main.playerSpawn.position = escapePod.Location + playerSpawnRelativeToEscapePodPosition;

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

            MyEscapePodGuid = escapePod.Guid;
        }

        public void AddNewEscapePod(EscapePodModel escapePod)
        {
            if (!escapePodsByGuid.ContainsKey(escapePod.Guid))
            {
                escapePodsByGuid[escapePod.Guid] = CreateNewEscapePod(escapePod);
            }
        }

        public void SyncEscapePodGuids(List<EscapePodModel> escapePods)
        {
            foreach (EscapePodModel model in escapePods)
            {
                if (!escapePodsByGuid.ContainsKey(model.Guid))
                {
                    escapePodsByGuid[model.Guid] = CreateNewEscapePod(model);
                }
            }
        }

        public GameObject CreateNewEscapePod(EscapePodModel model)
        {
            SURPRESS_ESCAPE_POD_AWAKE_METHOD = true;

            GameObject escapePod;

            if (model.Guid == MyEscapePodGuid)
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
