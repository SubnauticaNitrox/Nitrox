using System.Threading.Tasks;
using NitroxClient.Communication;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.MonoBehaviours;
using Nitrox.Model.Subnautica.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

public sealed class MapRoomCameraLightChangedProcessor : IClientPacketProcessor<MapRoomCameraLightChanged>
{
    public Task Process(ClientProcessorContext context, MapRoomCameraLightChanged packet)
    {
        if (!NitroxEntity.TryGetObjectFrom(packet.CameraId, out GameObject cameraObject))
        {
            Log.Warn($"Could not find scanner room camera for light update: {packet.CameraId}");
            return Task.CompletedTask;
        }

        MapRoomCamera camera = cameraObject.GetComponent<MapRoomCamera>();
        if (!camera)
        {
            Log.Warn($"Light update target did not have MapRoomCamera: {cameraObject.GetFullHierarchyPath()}");
            return Task.CompletedTask;
        }

        using (PacketSuppressor<MapRoomCameraLightChanged>.Suppress())
        {
            camera.lightState = packet.LightState;
            camera.lightsParent.SetActive(packet.LightState);
        }

        return Task.CompletedTask;
    }
}
