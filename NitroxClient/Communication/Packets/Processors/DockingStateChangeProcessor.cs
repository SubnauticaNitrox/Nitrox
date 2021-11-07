using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.Logger;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class DockingStateChangeProcessor : ClientPacketProcessor<DockingStateChange>
    {
        public override void Process(DockingStateChange dockingStateChange)
        {
            if (NitroxEntity.TryGetObjectFrom(dockingStateChange.NitroxId, out GameObject gameObject))
            {
                if (gameObject.TryGetComponent(out MapRoomCameraDocking mapRoomCameraDocking))
                {
                    if (!dockingStateChange.Docked)
                    {
                        // Need to manually apply modifications that doesn't happen automatically
                        mapRoomCameraDocking.camera.SetDocked(null);
                        mapRoomCameraDocking.camera = null;
                        mapRoomCameraDocking.cameraDocked = false;
                    }
                }
            }
            else
            {
                Log.Warn($"Couldn't find MapRoomCameraDocking [NitroxId:{dockingStateChange.NitroxId}], a NitroxId desync may have occurred");
            }
        }
    }
}
