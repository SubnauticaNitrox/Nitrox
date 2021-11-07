using NitroxClient.GameLogic.Simulation;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.Logger;

namespace NitroxClient.GameLogic
{
    public class CameraControlManager
    {
        public static CameraControlManager Instance;
        private SimulationOwnership simulationOwnership;

        public NitroxId CurrentCameraId;
        public bool WaitingForLock;
        private bool hasControlOverCurrentCameraCache;
        
        public CameraControlManager()
        {
            // TornacTODO: Verify why player's location is not locked when controlling a camera
            Instance = this;
            simulationOwnership = NitroxServiceLocator.LocateService<SimulationOwnership>();
        }

        public void ControlCamera(MapRoomCamera mapRoomCamera)
        {
            CurrentCameraId = NitroxEntity.GetId(mapRoomCamera.gameObject);
            PlayerMovement.ControlledCamera = mapRoomCamera;
            PlayerMovement.ControllingCamera = false;
            hasControlOverCurrentCameraCache = false;
            WaitingForLock = true;
            ChangeCameraTitle("Trying to get control over this camera", "fff700");
            LockRequest<CameraControl> lockRequest = new(CurrentCameraId, SimulationLockType.EXCLUSIVE, ReceivedSimulationLockResponse, new CameraControl(CurrentCameraId));
            simulationOwnership.RequestSimulationLock(lockRequest);
        }

        public void FreeCamera()
        {
            PlayerMovement.ControllingCamera = false;
            PlayerMovement.ControlledCamera = null;
            hasControlOverCurrentCameraCache = false;
            WaitingForLock = false;
            simulationOwnership.RequestSimulationLock(CurrentCameraId, SimulationLockType.TRANSIENT);
            CurrentCameraId = null;
            ChangeCameraTitle(null);
        }

        public void ReceivedSimulationLockResponse(NitroxId id, bool lockAquired, CameraControl context)
        {
            WaitingForLock = false;

            // Maybe the client cycled to another camera before he received the lock
            if (id.Equals(CurrentCameraId))
            {
                if (lockAquired)
                {
                    // Now controlling camera
                    Log.Debug($"Now controlling camera [{id}]");
                    PlayerMovement.ControllingCamera = true;
                    hasControlOverCurrentCameraCache = true;
                    ChangeCameraTitle(null, "00ff00");
                }
                else
                {
                    // Camera is already controlled
                    Log.Debug($"Now spectating camera [{id}]");
                    ChangeCameraTitle("This camera is already being controlled by another player", "FFB200");
                    hasControlOverCurrentCameraCache = false;
                }
            }
            else
            {
                hasControlOverCurrentCameraCache = false;
            }
        }

        public bool HasControlOverCurrentCamera()
        {
            return hasControlOverCurrentCameraCache;
        }

        public void ChangeCameraTitle(string titleAddition, string colorCode = "ffffff")
        {
            uGUI_CameraDrone.main.UpdateCameraTitle();
            if (titleAddition != null)
            {
                uGUI_CameraDrone.main.textTitle.text += $" <color=#{colorCode}>{titleAddition}</color>";
            }
            else if(colorCode != "ffffff")
            {
                uGUI_CameraDrone.main.textTitle.text = $"<color=#{colorCode}>{uGUI_CameraDrone.main.textTitle.text}</color>";
            }
        }
    }
}
