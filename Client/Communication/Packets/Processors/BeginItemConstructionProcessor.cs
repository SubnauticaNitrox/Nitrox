using NitroxClient.Communication.Packets.Processors;
using NitroxClient.Communication.Packets.Processors.Base;
using NitroxClient.MonoBehaviours.Overrides;
using NitroxModel.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class BeginItemConstructionProcessor : GenericPacketProcessor<BeginItemConstruction>
    {
        public override void Process(BeginItemConstruction construction)
        {
            Console.WriteLine("Processing construction " + construction.ItemPosition + " " + construction.PlayerId);
            TechType techType;
            UWE.Utils.TryParseEnum<TechType>(construction.TechType, out techType);
            GameObject techPrefab = TechTree.main.GetGamePrefab(techType);
            Console.WriteLine("Built item is of tech " + techType.ToString());
            ConstructItem(ApiHelper.Vector3(construction.ItemPosition), ApiHelper.Quaternion(construction.Rotation), techType);
        }
        
        public void ConstructItem(Vector3 position, Quaternion rotation, TechType techType)
        {
            GameObject buildPrefab = CraftData.GetBuildPrefab(techType);
            MultiplayerBuilder.overridePosition = position;
            MultiplayerBuilder.overrideQuaternion = rotation;
            MultiplayerBuilder.placePosition = position;
            MultiplayerBuilder.placeRotation = rotation;
            MultiplayerBuilder.Begin(buildPrefab);
            MultiplayerBuilder.TryPlace();
        }
        
    }
}
