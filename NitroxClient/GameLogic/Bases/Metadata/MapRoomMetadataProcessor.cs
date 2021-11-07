using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Buildings.Metadata;
using NitroxModel.Logger;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxClient.GameLogic.Bases.Metadata
{
    public class MapRoomMetadataProcessor : GenericBasePieceMetadataProcessor<MapRoomMetadata>
    {
        public override void UpdateMetadata(NitroxId id, MapRoomMetadata metadata, bool initialSync)
        {
            if (!initialSync)
            {
                // Log.Debug("UpdateMetadata() but not initial sync");
                return;
            }
            // Log.Debug($"UpdateMetada({id}, {metadata})");

            // MRF_GO is the MapRoomFunctionality's GameObject which has the NitroxId provided in the metadata
            GameObject MRF_GO;
            if (!NitroxEntity.TryGetObjectFrom(metadata.MapRoomFunctionalityId, out MRF_GO))
            {
                Log.Error($"There was an error while processing MapRoomMetadata for [NitroxId: {metadata.MapRoomFunctionalityId}], the corresponding game object couldn't be found");
                return;
            }
            
            MapRoomCameraDocking cameraDocking1 = MRF_GO.FindChild("dockingPoint1").GetComponent<MapRoomCameraDocking>();
            MapRoomCameraDocking cameraDocking2 = MRF_GO.FindChild("dockingPoint2").GetComponent<MapRoomCameraDocking>();
            if (cameraDocking1.AliveOrNull() != null && cameraDocking2.AliveOrNull() != null)
            {
                Log.Debug($"Processing CameraDockingMetadata [{metadata}]");
                // Simulate Undock but don't do it else Postfix will happen
                if (!metadata.CameraDocked1)
                {
                    if (cameraDocking1.camera != null)
                    {
                        NitroxEntity.RemoveFrom(cameraDocking1.camera.gameObject);
                        UnityEngine.Object.Destroy(cameraDocking1.camera.gameObject);
                        cameraDocking1.camera = null;
                        cameraDocking1.cameraDocked = false;
                    }
                }
                else if (metadata.Camera1NitroxId != null)
                {
                    NitroxEntity.SetNewId(cameraDocking1.camera.gameObject, metadata.Camera1NitroxId);
                }
                if (!metadata.CameraDocked2)
                {
                    NitroxEntity.RemoveFrom(cameraDocking2.camera.gameObject);
                    UnityEngine.Object.Destroy(cameraDocking2.camera.gameObject);
                    cameraDocking2.camera = null;
                    cameraDocking2.cameraDocked = false;
                }
                else if (metadata.Camera2NitroxId != null)
                {
                    NitroxEntity.SetNewId(cameraDocking2.camera.gameObject, metadata.Camera2NitroxId);
                }
            }
            else
            {
                Log.Error($"MapRoomMetadata Processing failed for {MRF_GO.name}({MRF_GO.transform.position}). No MapRoomCameraDocking component was found on object's children.");
                return;
            }
            // Recover the state of the scanner
            uGUI_MapRoomScanner uGUI_MapRoomScanner = MRF_GO.transform.Find("screen/scannerUI").GetComponent<uGUI_MapRoomScanner>();
            if (uGUI_MapRoomScanner.AliveOrNull() != null && metadata.TypeToScan != null)
            {
                uGUI_MapRoomScanner.mapRoom.StartScanning(metadata.TypeToScan.ToUnity());
                uGUI_MapRoomScanner.UpdateGUIState();
            }
            else
            {
                Log.Error($"MapRoomMetadata Processing failed for {MRF_GO.name}({MRF_GO.transform.position}). No uGUI_MapRoomScanner component was found on object's children.");
            }
        }
    }
}
