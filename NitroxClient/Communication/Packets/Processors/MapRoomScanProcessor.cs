using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class MapRoomScanProcessor : ClientPacketProcessor<MapRoomScan>
    {
        public override void Process(MapRoomScan mapRoomScan)
        {
            if (NitroxEntity.TryGetObjectFrom(mapRoomScan.NitroxId, out GameObject gameObject))
            {
                if (gameObject.TryGetComponent(out uGUI_MapRoomScanner uGUI_MapRoomScanner))
                {
                    // ScanAction is either to scan or to cancel
                    if (mapRoomScan.ScanAction)
                    {
                        uGUI_MapRoomScanner.mapRoom.StartScanning(mapRoomScan.NitroxTechType.ToUnity());
                        uGUI_MapRoomScanner.UpdateGUIState();
                    }
                    else
                    {
                        uGUI_MapRoomScanner.mapRoom.StartScanning(TechType.None);
                        uGUI_MapRoomScanner.UpdateGUIState();
                    }
                }
            }
            else
            {
                Log.Warn($"Couldn't find MapRoomFunctionality [NitroxId:{mapRoomScan.NitroxId}], a NitroxId desync may have occurred");
            }
        }
    }
}
