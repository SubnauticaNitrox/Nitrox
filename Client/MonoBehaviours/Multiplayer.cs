using NitroxClient.Communication;
using NitroxModel.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NitroxClient.MonoBehaviours
{
    public class Multiplayer : MonoBehaviour
    {
        public static MultiplayerClient client;
        public static bool isActive = false;
        
        public void Awake()
        {
            DevConsole.RegisterConsoleCommand(this, "mplayer", false);
        }

        public void Update()
        {

        }
        
        public void OnConsoleCommand_mplayer(NotificationCenter.Notification n)
        {
            if (n != null && n.data != null && n.data.Count > 0)
            {
                string playerId = (string)n.data[0];
                client = new MultiplayerClient(playerId);
                InitMonoBehaviours();
                isActive = true;
            }
        }
        
        public void InitMonoBehaviours()
        {
            this.gameObject.AddComponent<Chat>();
            this.gameObject.AddComponent<PlayerMovement>();
            this.gameObject.AddComponent<PlayerItemPickup>();
            this.gameObject.AddComponent<PlayerDroppedItem>();
            this.gameObject.AddComponent<ItemBuilt>();
        }
    }
}
