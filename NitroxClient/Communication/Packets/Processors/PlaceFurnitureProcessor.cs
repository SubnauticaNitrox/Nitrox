using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours.Overrides;
using NitroxModel.Helper;
using NitroxClient.GameLogic.Helper;
using NitroxModel.Packets;
using System.Reflection;
using UnityEngine;
using NitroxClient.Unity.Helper;

namespace NitroxClient.Communication.Packets.Processors
{
    public class PlaceFurnitureProcessor : ClientPacketProcessor<PlaceFurniture>
    {
        public override void Process(PlaceFurniture packet)
        {
            GameObject buildPrefab = CraftData.GetBuildPrefab(packet.TechType);
            MultiplayerBuilder.overridePosition = packet.ItemPosition;
            MultiplayerBuilder.overrideQuaternion = packet.Rotation;
            MultiplayerBuilder.overrideTransform = new GameObject().transform;
            MultiplayerBuilder.overrideTransform.position = packet.CameraPosition;
            MultiplayerBuilder.overrideTransform.rotation = packet.CameraRotation;
            MultiplayerBuilder.placePosition = packet.ItemPosition;
            MultiplayerBuilder.placeRotation = packet.Rotation;
            MultiplayerBuilder.Begin(buildPrefab);

            SubRoot subRoot = null;

            if (packet.SubGuid.IsPresent())
            {
                GameObject sub = GuidHelper.RequireObjectFrom(packet.SubGuid.Get());
                subRoot = sub.GetComponent<SubRoot>();
            }

            GameObject gameObject = MultiplayerBuilder.TryPlaceFurniture(subRoot);
            GuidHelper.SetNewGuid(gameObject, packet.Guid);

            Constructable constructable = gameObject.RequireComponentInParent<Constructable>();

            /**
             * Manually call start to initialize the object as we may need to interact with it within the same frame.
             */
            MethodInfo startCrafting = typeof(Constructable).GetMethod("Start", BindingFlags.NonPublic | BindingFlags.Instance);
            Validate.NotNull(startCrafting);
            startCrafting.Invoke(constructable, new object[] { });
        }        
    }
}
