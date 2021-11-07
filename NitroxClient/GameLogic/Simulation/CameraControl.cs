using NitroxModel.DataStructures;

namespace NitroxClient.GameLogic.Simulation
{
    public class CameraControl : LockRequestContext
    {
        public NitroxId CameraNitroxId { get; }

        public CameraControl(NitroxId cameraNitroxId)
        {
            CameraNitroxId = cameraNitroxId;
        }
    }
}
