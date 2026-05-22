using System;
using System.Threading.Tasks;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.MonoBehaviours;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class MapRoomScanChangedProcessor : IClientPacketProcessor<MapRoomScanChanged>
{
    public Task Process(ClientProcessorContext context, MapRoomScanChanged packet)
    {
        if (!NitroxEntity.TryGetObjectFrom(packet.MapRoomId, out GameObject mapRoomObject))
        {
            Log.Warn($"Could not find MapRoomFunctionality for scanner room scan change: {packet.MapRoomId}");
            return Task.CompletedTask;
        }

        MapRoomFunctionality mapRoomFunctionality = mapRoomObject.GetComponent<MapRoomFunctionality>();
        if (!mapRoomFunctionality)
        {
            Log.Warn($"Object for scanner room scan change did not have MapRoomFunctionality: {mapRoomObject.GetFullHierarchyPath()}");
            return Task.CompletedTask;
        }

        if (!Enum.TryParse(packet.TechType, out TechType techType))
        {
            Log.Warn($"Could not parse scanner room scan TechType: {packet.TechType}");
            return Task.CompletedTask;
        }

        using (PacketSuppressor<MapRoomScanChanged>.Suppress())
        {
            mapRoomFunctionality.StartScanning(techType);
            RefreshMapRoomScannerUi(mapRoomFunctionality, techType);
        }

        return Task.CompletedTask;
    }

    private static void RefreshMapRoomScannerUi(MapRoomFunctionality mapRoomFunctionality, TechType techType)
    {
        if (!mapRoomFunctionality.screenRoot)
        {
            return;
        }

        uGUI_MapRoomScanner scannerUi = mapRoomFunctionality.screenRoot.GetComponentInChildren<uGUI_MapRoomScanner>(true);
        if (!scannerUi)
        {
            Log.Warn($"Could not find uGUI_MapRoomScanner for scanner room scan change: {mapRoomFunctionality.gameObject.GetFullHierarchyPath()}");
            return;
        }

        scannerUi.UpdateGUIState();

        if (techType != TechType.None)
        {
            scannerUi.StartAnimation();
        }
    }
}
