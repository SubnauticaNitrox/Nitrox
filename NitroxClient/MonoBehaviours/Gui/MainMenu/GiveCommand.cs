using System;
using System.Collections.Generic;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxModel.Core;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxModel_Subnautica.Helper;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NitroxClient.MonoBehaviours.Gui.MainMenu
{
    /// <summary>
    /// ༼ つ ◕_◕ ༽つ
    /// </summary>
    public class GiveCommand : MonoBehaviour
    {
        public void Awake()
        {
            SceneManager.sceneLoaded += SceneManager_sceneLoaded;
            DontDestroyOnLoad(gameObject);
        }

        public void OnDestroy()
        {
            SceneManager.sceneLoaded -= SceneManager_sceneLoaded;
        }

        public void OnConsoleCommand_give(NotificationCenter.Notification n)
        {
            if (n?.data?.Count > 0)
            {
                TechType techType;

                if (!UWE.Utils.TryParseEnum((string)n.data[0], out techType))
                {
                    Log.InGame("Could not parse " + n.data[0] + " to TechType");
                    return;
                }

                // You'll find that the Cyclops gets a lot of special attention everywhere, including spawning
                if (techType == TechType.Cyclops)
                {
                    SpawnCyclops();
                }
                else
                {
                    // No idea what constitutes an allowed spawn
                    if (CraftData.IsAllowed(techType))
                    {
                        SpawnItem(new NitroxModel.DataStructures.TechType(techType.ToString()));
                    }
                    else
                    {
                        Log.InGame("TechType: " + techType + " is not allowed to be spawned!");
                    }
                }
            }
        }

        private void SpawnCyclops()
        {
            string guid = Guid.NewGuid().ToString();
            // Because why not
            Vector4 yellow = new Vector4(0.846f, 1f, 0.231f, 1);
            Vector3 yellowHSB = new Vector3(0.2f, 0.8f, 1);
            Vector4 green = new Vector4(0.003f, 0.795f, 0f, 1);
            Vector3 greenHSB = new Vector3(0.3f, 1, 0.8f);
            Vector3 black = new Vector3(0, 0, 0);
            Vector3 blackHSB = new Vector3(1, 1, 0);
            Vector3[] HSB = new Vector3[4] { yellowHSB, greenHSB, yellowHSB, blackHSB };
            Vector3[] Colours = new Vector3[4] { yellow, green, yellow, black };

            VehicleModel newVehicle = new VehicleModel(new NitroxModel.DataStructures.TechType(TechType.Cyclops.ToString()), //TechType.Cyclops,
                guid,
                MainCamera.camera.transform.position + MainCamera.camera.transform.forward * 20f,
                Quaternion.LookRotation(MainCamera.camera.transform.right),
                Optional<List<InteractiveChildObjectIdentifier>>.Empty(),
                Optional<string>.Empty(),
                "Nitrox",
                HSB,
                Colours);

            NitroxServiceLocator.LocateService<Vehicles>().CreateVehicle(newVehicle);
            NitroxServiceLocator.LocateService<Vehicles>().BroadcastCreatedVehicle(newVehicle);
        }

        /// <summary>
        /// Spawn anything but the <see cref="Cyclops"/> and send a <see cref="DroppedItem"/> packet to notify everyone else a new item exists
        /// </summary>
        /// <param name="techType"></param>
        private void SpawnItem(NitroxModel.DataStructures.TechType techType)
        {
            GameObject prefabForTechType = CraftData.GetPrefabForTechType(techType.Enum(), true);

            if (prefabForTechType == null)
            {
                Log.InGame("TechType: " + techType + " does not have a Prefab!");
                return;
            }

            LocalPlayer localPlayer = NitroxServiceLocator.LocateService<LocalPlayer>();
            string newGuid = Guid.NewGuid().ToString();
            Vector3 spawnLocation = localPlayer.Body.transform.position + localPlayer.Body.transform.forward * 8f;

            Entity entity = new Entity(spawnLocation,
                localPlayer.Body.transform.rotation,
                new Vector3(1, 1, 1),
                techType,
                0,
                null,
                true,
                newGuid);
            
            NitroxServiceLocator.LocateService<Entities>().Spawn(new List<Entity>() { entity });

            NitroxServiceLocator.LocateService<IPacketSender>().Send(new DroppedItem(newGuid,
                Optional<string>.Empty(),
                techType,
                spawnLocation,
                localPlayer.Body.transform.rotation,
                entity.SerializedGameObject));
        }

        private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            DevConsole.RegisterConsoleCommand(this, "give", false);
        }
    }
}
