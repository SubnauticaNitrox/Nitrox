using System;
using System.Collections.Generic;
using ProtoBufNet;
using NitroxModel.DataStructures.GameLogic;
using NitroxServer.GameLogic.Items;
using UnityEngine;
using System.Linq;

namespace NitroxServer.GameLogic
{
    [ProtoContract]
    public class EscapePodManager
    {
        [ProtoMember(1)]
        public List<EscapePodModel> SerializedEscapePods
        {
            get
            {
                lock (escapePods)
                {
                    return escapePods;
                }
            }
            set
            {
                escapePods = value;
            }
        }

        [ProtoMember(2)]
        public Dictionary<ushort, EscapePodModel> SerializedEscapePodsByPlayerId
        {
            get
            {
                lock (escapePodsByPlayerId)
                {
                    return new Dictionary<ushort, EscapePodModel>(escapePodsByPlayerId);
                }
            }
            set
            {
                escapePodsByPlayerId = value;
            }
        }

        [ProtoMember(3)]
        private EscapePodModel podNotFullYet;

        [ProtoIgnore]
        private const int PLAYERS_PER_ESCAPEPOD = 50;
        [ProtoIgnore]
        private const int ESCAPE_POD_X_OFFSET = 40;

        [ProtoIgnore]
        private List<EscapePodModel> escapePods = new List<EscapePodModel>();
        [ProtoIgnore]
        private Dictionary<ushort, EscapePodModel> escapePodsByPlayerId = new Dictionary<ushort, EscapePodModel>();
        [ProtoIgnore]
        private InventoryData inventoryData;

        public void InitEscapePodManager(InventoryData inventoryData)
        {
            this.inventoryData = inventoryData;
            podNotFullYet = CreateNewEscapePod();
        }
        public void SetInventoryData(InventoryData inventoryData)
        {
            this.inventoryData = inventoryData;
        }

        public void AssignPlayerToEscapePod(ushort playerId)
        {
            lock (escapePodsByPlayerId)
            {
                if (escapePodsByPlayerId.ContainsKey(playerId))
                {
                    return;
                }

                if (podNotFullYet.AssignedPlayers.Count == PLAYERS_PER_ESCAPEPOD)
                {
                    podNotFullYet = CreateNewEscapePod();
                }

                podNotFullYet.AssignedPlayers.Add(playerId);
                escapePodsByPlayerId[playerId] = podNotFullYet;
            }
        }

        public EscapePodModel[] GetEscapePods()
        {
            lock (escapePods)
            {
                return escapePods.ToArray();
            }
        }

        private EscapePodModel CreateNewEscapePod()
        {
            //Had to get the data from somewhere and because gameobjects are inaccessable form outside of the game Had to hardcode the information
            const string NUTRIENT_BLOCK_DATA = "AggBhgEIABAAGAAiCFVudGFnZ2VkMiQwYTJjZTc2Yi0yNTJlLTRmYzMtOGZhYy01NDA1N2ViOTAxNGY6JGEwNjE1N2NjLThkZTgtNGZlYy04NWE2LTc2YjJhZWUxZTI2M0IkM2Q0NjdiMDEtODExNC00OTExLWJmODgtYWVlMmI3M2JlMGE4SAFQAAIIBBkKFVVuaXR5RW5naW5lLlRyYW5zZm9ybRABOAoPDQAAAAAVAAAAAB0AAAAAEhQNAAAAABUAAAAAHQAAAAAlAACAPxoPDQAAgD8VAACAPx0AAIA/DgoKUGlja3VwYWJsZRABE8I+EAgAEAAYACAAKAEwATgAQAMUChBMYXJnZVdvcmxkRW50aXR5EAECEAALCgdFYXRhYmxlEAEFDQAAAAA=";
            const string FILTERED_WATER_DATA = "AggBhgEIABAAGAAiCFVudGFnZ2VkMiRkNDdmM2FiZi1kNzMxLTQ4MjktYWY0Yy1kNmNhMzhhODFiZjU6JDIyYjBjZTA4LTYxYzktNDQ0Mi1hODNkLWJhN2ZiOTlmMjZiMEIkM2Q0NjdiMDEtODExNC00OTExLWJmODgtYWVlMmI3M2JlMGE4SAFQAAIIBBkKFVVuaXR5RW5naW5lLlRyYW5zZm9ybRABOAoPDQAAAAAVAAAAAB0AAAAAEhQNAAAAABUAAAAAHQAAAAAlAACAPxoPDQAAgD8VAACAPx0AAIA/DgoKUGlja3VwYWJsZRABE8I+EAgAEAAYACAAKAEwATgAQAMUChBMYXJnZVdvcmxkRW50aXR5EAECEAALCgdFYXRhYmxlEAEFDQAAAAA=";
            const string FLARE_DATA = "AggBhgEIABAAGAgiCFVudGFnZ2VkMiQ1YzE4ODExNS03NWZhLTQ2NTMtYWYxMi02ZGYzMjFhM2E0NDk6JGY1YjZlYmVkLTlhZDktNGU0Ni1iMGI0LTBmOGQ2NDY3ZTA5MUIkM2Q0NjdiMDEtODExNC00OTExLWJmODgtYWVlMmI3M2JlMGE4SAFQAAIIBBkKFVVuaXR5RW5naW5lLlRyYW5zZm9ybRABOAoPDQAAAAAVAAAAAB0AAAAAEhQNAAAAABUAAAAAHQAAAAAlAACAPxoPDQAAgD8VAACAPx0AAIA/DgoKUGlja3VwYWJsZRABE8I+EAgAEAAYACAAKAEwATgAQAMJCgVGbGFyZRABDg0AAOFEEAAYACUAAAAAFAoQTGFyZ2VXb3JsZEVudGl0eRABAhAA";

            lock (escapePods)
            {
                int totalEscapePods = escapePods.Count;

                EscapePodModel escapePod = new EscapePodModel();
                escapePod.InitEscapePodModel("escapePod" + totalEscapePods,
                                             new Vector3(-112.2f + (ESCAPE_POD_X_OFFSET * totalEscapePods), 0.0f, -322.6f),
                                             "escapePodFab" + totalEscapePods,
                                             "escapePodMedFab" + totalEscapePods,
                                             "escapePodStorageFab" + totalEscapePods,
                                             "escapePodRadioFab" + totalEscapePods);
                

                for(int i = 0;i<2;i++)
                {
                    inventoryData.ItemAdded(new ItemData("escapePodStorageFab" + totalEscapePods, "nBar" + i, Convert.FromBase64String(NUTRIENT_BLOCK_DATA)));
                    inventoryData.ItemAdded(new ItemData("escapePodStorageFab" + totalEscapePods, "fWater" + i, Convert.FromBase64String(FILTERED_WATER_DATA)));
                    inventoryData.ItemAdded(new ItemData("escapePodStorageFab" + totalEscapePods, "Flare" + i, Convert.FromBase64String(FLARE_DATA)));
                }
                
                escapePods.Add(escapePod);
                
                return escapePod;
            }
        }
    }
}
