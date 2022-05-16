using System.Collections.Generic;
using NitroxClient.Communication.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxClient.MonoBehaviours.Overrides;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxClient.GameLogic
{
    public class EscapePodManager
    {
        /*
         * When creating additional escape pods (multiple users with multiple pods)
         * we want to supress the escape pod's awake method so it doesn't override
         * EscapePod.main to the new escape pod.
         */
        public static bool SURPRESS_ESCAPE_POD_AWAKE_METHOD;

        private readonly IPacketSender packetSender;
        private readonly IMultiplayerSession multiplayerSession;

        private readonly Vector3 playerSpawnRelativeToEscapePodPosition = new Vector3(0.9f, 2.1f, 0);
        private readonly Dictionary<NitroxId, GameObject> escapePodsById = new Dictionary<NitroxId, GameObject>();

        public NitroxId MyEscapePodId;

        public EscapePodManager(IPacketSender packetSender, IMultiplayerSession multiplayerSession)
        {
            this.packetSender = packetSender;
            this.multiplayerSession = multiplayerSession;
        }

        public void AssignPlayerToEscapePod(EscapePodModel escapePod, bool firstTimeSpawning)
        {
            Validate.NotNull(escapePod, "Escape pod can not be null");
            NitroxEntity.SetNewId(EscapePod.main.gameObject, escapePod.Id);
            EscapePod.main.transform.position = escapePod.Location.ToUnity();
            EscapePod.main.playerSpawn.position = escapePod.Location.ToUnity() + playerSpawnRelativeToEscapePodPosition; // This Might not correctly handle rotated EscapePods

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
            if (firstTimeSpawning)
            {
                Player.main.currentEscapePod = EscapePod.main;
            }
            Player.main.escapePod.Update(true); // Tells the game to update various EscapePod features
            MyEscapePodId = escapePod.Id;
        }

        public void AddNewEscapePod(EscapePodModel escapePod)
        {
            if (!escapePodsById.ContainsKey(escapePod.Id))
            {
                escapePodsById[escapePod.Id] = CreateNewEscapePod(escapePod);
            }
        }

        public void SyncEscapePodIds(List<EscapePodModel> escapePods)
        {
            foreach (EscapePodModel model in escapePods)
            {
                if (!escapePodsById.ContainsKey(model.Id))
                {
                    escapePodsById[model.Id] = CreateNewEscapePod(model);
                }
            }
        }

        public GameObject CreateNewEscapePod(EscapePodModel model)
        {
            SURPRESS_ESCAPE_POD_AWAKE_METHOD = true;

            GameObject escapePod;

            if (model.Id == MyEscapePodId)
            {
                escapePod = EscapePod.main.gameObject;
            }
            else
            {
                escapePod = Object.Instantiate(EscapePod.main.gameObject);
                NitroxEntity.SetNewId(escapePod, model.Id);
            }

            escapePod.transform.position = model.Location.ToUnity();

            StorageContainer storageContainer = escapePod.RequireComponentInChildren<StorageContainer>();

            using (packetSender.Suppress<ItemContainerRemove>())
            {
                storageContainer.container.Clear();
            }

            NitroxEntity.SetNewId(storageContainer.gameObject, model.StorageContainerId);

            MedicalCabinet medicalCabinet = escapePod.RequireComponentInChildren<MedicalCabinet>();
            NitroxEntity.SetNewId(medicalCabinet.gameObject, model.MedicalFabricatorId);

            Fabricator fabricator = escapePod.RequireComponentInChildren<Fabricator>();
            NitroxEntity.SetNewId(fabricator.gameObject, model.FabricatorId);

            Radio radio = escapePod.RequireComponentInChildren<Radio>();
            NitroxEntity.SetNewId(radio.gameObject, model.RadioId);

            DamageEscapePod(model.Damaged, model.RadioDamaged);
            FixStartMethods(escapePod);

            // Start() isn't executed for the EscapePod, why? Idk, maybe because it's a scene...
            MultiplayerCinematicReference reference = escapePod.AddComponent<MultiplayerCinematicReference>();
            foreach (PlayerCinematicController controller in escapePod.GetComponentsInChildren<PlayerCinematicController>())
            {
                reference.AddController(controller);
            }

            SURPRESS_ESCAPE_POD_AWAKE_METHOD = false;

            return escapePod;
        }

        /// <summary>
        /// Start() isn't executed for the EscapePod and children (Why? Idk, maybe because it's a scene...) so we call the components here where we have patches in Start.
        /// </summary>
        private static void FixStartMethods(GameObject escapePod)
        {
            foreach (FMOD_CustomEmitter customEmitter in escapePod.GetComponentsInChildren<FMOD_CustomEmitter>())
            {
                customEmitter.Start();
            }

            foreach (FMOD_StudioEventEmitter studioEventEmitter in escapePod.GetComponentsInChildren<FMOD_StudioEventEmitter>())
            {
                studioEventEmitter.Start();
            }
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

        public void OnRepair(NitroxId id)
        {
            if (escapePodsById.ContainsKey(id))
            {
                EscapePod pod = escapePodsById[id].GetComponent<EscapePod>();
                pod.liveMixin.health = pod.liveMixin.maxHealth;
                pod.animator.SetFloat("lifepod_damage", 1.0f);
                pod.fixPanelGoal.Trigger();
                pod.fixPanelPowerUp.Play();
            }
            else
            {
                Log.Warn("No escape pod to be repaired by id " + id);
            }
        }

        public void OnRepairedByMe(EscapePod pod)
        {
            NitroxId id = null;

            foreach (KeyValuePair<NitroxId, GameObject> dict in escapePodsById)
            {
                if (NitroxEntity.GetId(dict.Value) == NitroxEntity.GetId(pod.gameObject))
                {
                    id = dict.Key; // we're looking for serverside id here
                    break;
                }
            }

            if (id != null)
            {
                EscapePodRepair repair = new EscapePodRepair(id);
                packetSender.Send(repair);
            }
            else
            {
                Log.Warn("Couldn't find escape pod id on repair");
            }
        }

        public void OnRadioRepair(NitroxId id)
        {
            Optional<GameObject> radObj = NitroxEntity.GetObjectFrom(id);
            if (radObj.HasValue)
            {
                Radio radio = radObj.Value.GetComponent<Radio>();

                // StoryGoalManager.main.PulsePendingMessages();
                radio.liveMixin.health = radio.liveMixin.maxHealth;
                radio.repairNotification.Play();
            }
        }

        public void OnRadioRepairedByMe(Radio radio)
        {
            // todo: can this apply to non-escape pod radios?

            NitroxId id = NitroxEntity.GetId(radio.gameObject);

            EscapePodRadioRepair repair = new EscapePodRadioRepair(id);
            packetSender.Send(repair);
        }
    }
}
