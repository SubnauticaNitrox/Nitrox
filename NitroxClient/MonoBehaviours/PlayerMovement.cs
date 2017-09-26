using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Helper;
using NitroxModel.Core;
using NitroxModel.DataStructures.ServerModel;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxClient.MonoBehaviours
{
    public class PlayerMovement : MonoBehaviour
    {
        public const float BROADCAST_INTERVAL = 0.05f;
        private PlayerLogic playerBroadcaster;
        private SimulationOwnership simulationOwnership;

        private float time;

        public void Awake()
        {
            playerBroadcaster = NitroxServiceLocator.LocateService<PlayerLogic>();
            simulationOwnership = NitroxServiceLocator.LocateService<SimulationOwnership>();
        }

        private Optional<VehicleModel> previousVehicle = Optional<VehicleModel>.Empty();

        public void Update()
        {
            time += Time.deltaTime;

            // Only do on a specific cadence to avoid hammering server
            if (time >= BROADCAST_INTERVAL)
            {
                time = 0;

                Vector3 currentPosition = Player.main.transform.position;
                Vector3 playerVelocity = Player.main.playerController.velocity;

                // IDEA: possibly only CameraRotation is of interest, because bodyrotation is extracted from that.
                // WARN: Using camera rotation may be dangerous, when the drone is used for instance (but then movement packets shouldn't be sent either so it's not even relevant...)

                Quaternion bodyRotation = MainCameraControl.main.viewModel.transform.rotation;
                Quaternion aimingRotation = Player.main.camRoot.GetAimingTransform().rotation;

                Optional<VehicleModel> opVehicle = GetVehicleModel();

                string subGuid = null;

                // TODO: Perhaps check if the sub is a cyclops or a base... Bases don't need movementupdates ;)
                SubRoot currentSub = Player.main.GetCurrentSub();

                // TODO: Check the logic here, with vehicle and currentSub potentially overlapping.

                if (currentSub != null)
                {
                    subGuid = GuidHelper.GetGuid(currentSub.gameObject);
                }

                // TODO: Patch this into the player enter/exit functions instead, and hook it into the simulationownershipframework (that is, claim control before letting the player enter, and also let go of it correctly). For now, this is just for testing:
                if (opVehicle.IsPresent())
                {
                    VehicleModel vehicle = opVehicle.Get();

                    // While it's highly unlikely (or maybe even impossible), ownership is not given up here if the player goes from one vehicle to the other.
                    // Doesn't matter for now, this is just temp testing code.
                    previousVehicle = opVehicle;

                    // If ownership was already claimed, the player should not even be able to enter the vehicle and reach the code here. This is just a final check.
                    if (!simulationOwnership.HasOwnership(vehicle.Guid))
                    {
                        simulationOwnership.TryToRequestOwnership(vehicle.Guid);
                        // Unset for now, until ownership is received.
                        opVehicle = Optional<VehicleModel>.Empty();
                    }
                }
                else if (previousVehicle.IsPresent())
                {
                    simulationOwnership.ReleaseOwnership(previousVehicle.Get().Guid);
                    previousVehicle = Optional<VehicleModel>.Empty();
                }
                playerBroadcaster.UpdateLocation(currentPosition, playerVelocity, bodyRotation, aimingRotation, opVehicle, Optional<string>.OfNullable(subGuid));
            }
        }

        private Optional<VehicleModel> GetVehicleModel()
        {
            Vehicle vehicle = Player.main.GetVehicle();
            SubRoot sub = Player.main.GetCurrentSub();

            string guid;
            Vector3 position;
            Quaternion rotation;
            Vector3 velocity;
            Vector3 angularVelocity;
            TechType techType;
            float steeringWheelYaw = 0f, steeringWheelPitch = 0f;
            bool appliedThrottle = false;

            if (vehicle != null)
            {
                guid = GuidHelper.GetGuid(vehicle.gameObject);
                position = vehicle.gameObject.transform.position;
                rotation = vehicle.gameObject.transform.rotation;
                techType = CraftData.GetTechType(vehicle.gameObject);

                Rigidbody rigidbody = vehicle.gameObject.GetComponent<Rigidbody>();

                velocity = rigidbody.velocity;
                angularVelocity = rigidbody.angularVelocity;

                // Required because vehicle is either a SeaMoth or an Exosuit, and these fields are defined privately in Vehicle.
                steeringWheelYaw = (float)vehicle.ReflectionGet<Vehicle, Vehicle>("steeringWheelYaw");
                steeringWheelPitch = (float)vehicle.ReflectionGet<Vehicle, Vehicle>("steeringWheelPitch");

                // Vehicles (or the SeaMoth at least) do not have special throttle animations. Instead, these animations are always playing because the player can't even see them (unlike the cyclops which has cameras).
                // So, we need to hack in and try to figure out when thrust needs to be applied.
                if (vehicle && AvatarInputHandler.main.IsEnabled())
                {
                    if (techType == TechType.Seamoth)
                    {
                        bool flag = vehicle.transform.position.y < Ocean.main.GetOceanLevel() && vehicle.transform.position.y < vehicle.worldForces.waterDepth && !vehicle.precursorOutOfWater;
                        appliedThrottle = flag && GameInput.GetMoveDirection().sqrMagnitude > .1f;
                    }
                    else if (techType == TechType.Exosuit)
                    {
                        Exosuit exosuit = vehicle as Exosuit;
                        if (exosuit)
                        {
                            appliedThrottle = (bool)exosuit.ReflectionGet("_jetsActive") && (float)exosuit.ReflectionGet("thrustPower") > 0f;
                        }
                    }
                }
            }
            else if (sub != null && Player.main.isPiloting)
            {
                guid = GuidHelper.GetGuid(sub.gameObject);
                position = sub.gameObject.transform.position;
                rotation = sub.gameObject.transform.rotation;
                Rigidbody rigidbody = sub.GetComponent<Rigidbody>();
                velocity = rigidbody.velocity;
                angularVelocity = rigidbody.angularVelocity;
                techType = TechType.Cyclops;

                SubControl subControl = sub.GetComponent<SubControl>();
                steeringWheelYaw = (float)subControl.ReflectionGet("steeringWheelYaw");
                steeringWheelPitch = (float)subControl.ReflectionGet("steeringWheelPitch");
                appliedThrottle = subControl.appliedThrottle && (bool)subControl.ReflectionGet("canAccel");
            }
            else
            {
                return Optional<VehicleModel>.Empty();
            }

            VehicleModel model = new VehicleModel(techType,
                                                  guid,
                                                  position,
                                                  rotation,
                                                  velocity,
                                                  angularVelocity,
                                                  steeringWheelYaw,
                                                  steeringWheelPitch,
                                                  appliedThrottle);

            return Optional<VehicleModel>.Of(model);
        }
    }
}
