using System.Collections.Generic;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Logger;
using NitroxModel.Packets;
using UnityEngine;
using NitroxModel.DataStructures.Util;

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

            DamageEscapePod(model.Damaged, model.RadioDamaged);

            return escapePod;
        }

        public void DamageEscapePod(bool damage, bool radio)
        {
            if (damage)
            {
                EscapePod.main.ShowDamagedEffects();

                EscapePod.main.lightingController.SnapToState(2);
                uGUI_EscapePod.main.SetHeader(Language.main.Get("IntroEscapePod3Header"), new Color32(243, 201, 63, byte.MaxValue), 2f);
                uGUI_EscapePod.main.SetContent(Language.main.Get("IntroEscapePod3Content"), new Color32(233, 63, 27, byte.MaxValue));
                uGUI_EscapePod.main.SetPower(Language.main.Get("IntroEscapePod3Power"), new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
            }
            else
            {
                EscapePod.main.liveMixin.health = EscapePod.main.liveMixin.maxHealth;
                EscapePod.main.animator.SetFloat("lifepod_damage", 1.0f);
            }

            if (radio)
            {
                EscapePod.main.DamageRadio();
            }
        }

        public void OnRepair(string guid)
        {
            if (escapePodsByGuid.ContainsKey(guid))
            {
                EscapePod pod = escapePodsByGuid[guid].GetComponent<EscapePod>();
                pod.liveMixin.health = pod.liveMixin.maxHealth;
                pod.animator.SetFloat("lifepod_damage", 1.0f);
                pod.fixPanelGoal.Trigger();
                pod.fixPanelPowerUp.Play();
            } else
            {
                Log.Warn("No escape pod to be repaired by guid " + guid);
            }
        }

        public void OnRepairedByMe(EscapePod pod)
        {
            string guid = "";
            foreach(KeyValuePair<string, GameObject> dict in escapePodsByGuid)
            {
                if(dict.Value.GetGuid() == pod.gameObject.GetGuid())
                {
                    guid = dict.Key; // we're looking for serverside guid here
                    break;
                }
            }

            if (!guid.Equals(""))
            {
                EscapePodRepair repair = new EscapePodRepair(guid);
                packetSender.Send(repair);
            } else
            {
                Log.Warn("Couldn't find escape pod guid on repair");
            }
        }

        public void OnRadioRepair(string guid)
        {
            Optional<GameObject> radObj = GuidHelper.GetObjectFrom(guid);
            if (radObj.IsPresent())
            {
                Radio radio = radObj.Get().GetComponent<Radio>();

                // StoryGoalManager.main.PulsePendingMessages();
                radio.liveMixin.health = radio.liveMixin.maxHealth;
                radio.repairNotification.Play();
            }
        }

        public void OnRadioRepairedByMe(Radio radio)
        {
            // todo: can this apply to non-escape pod radios?

            string guid = GuidHelper.GetGuid(radio.gameObject);
            
            EscapePodRadioRepair repair = new EscapePodRadioRepair(guid);
            packetSender.Send(repair);
        }
    }
}
