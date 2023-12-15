using System;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;
using UWE;
using Terrain = NitroxClient.GameLogic.Terrain;

namespace NitroxClient.Communication.Packets.Processors;

public class PlayerTeleportedProcessor : ClientPacketProcessor<PlayerTeleported>
{
    public override void Process(PlayerTeleported packet)
    {
        Player.main.OnPlayerPositionCheat();

        if (packet.SubRootID.HasValue && NitroxEntity.TryGetComponentFrom(packet.SubRootID.Value, out SubRoot subRoot))
        {
            // Cyclops is using a local position system inside it's subroot
            if (subRoot.isCyclops)
            {
                // Reversing calculations from PlayerMovementBroadcaster.Update()
                Vector3 position = (subRoot.transform.rotation * packet.DestinationTo.ToUnity()) + subRoot.transform.position;

                Player.main.SetPosition(position);
                Player.main.SetCurrentSub(subRoot);
                return;
            }

            Player.main.SetCurrentSub(subRoot);
        }

        Vehicle currentVehicle = Player.main.currentMountedVehicle;
        // Check to make sure the player is in a vehicle
        if(currentVehicle != null)
        {
            Rigidbody vehicleRigidbody = currentVehicle.GetComponent<Rigidbody>();
            if (vehicleRigidbody != null)
            {
                if (!vehicleRigidbody.isKinematic) // let's do kinematic switching only if needed
                {
                    // Make the rigidbody kinematic
                    vehicleRigidbody.isKinematic = true;
                    Quaternion preservedRotation = currentVehicle.transform.rotation; // Preserving the current rotation
                    currentVehicle.teleporting = true;
                    // Teleport the vehicle by modifying its position
                    vehicleRigidbody.position = packet.DestinationTo.ToUnity();
                    currentVehicle.transform.rotation = preservedRotation; // Applying the preserved rotation after teleportation
                    currentVehicle.teleporting = false;
                    // Reset the RB kinematic to false
                    vehicleRigidbody.isKinematic = false;

                }
                else
                {
                    Quaternion preservedRotation = currentVehicle.transform.rotation;
                    currentVehicle.teleporting = true;
                    vehicleRigidbody.position = packet.DestinationTo.ToUnity();
                    currentVehicle.transform.rotation = preservedRotation;
                    currentVehicle.teleporting = false;
                }
                RunTerrainLoadCoroutine();
            }
            else
            {
                Log.Error("Tried to teleport vehicle, but was unable to acquire the RigidBody component");
            }
        }
        else
        {
            Player.main.SetPosition(packet.DestinationTo.ToUnity());
            RunTerrainLoadCoroutine();
        }
    }

    private static void RunTerrainLoadCoroutine()
    {
        // Freeze the player while he's loading its new position
        Player.main.cinematicModeActive = true;
        try
        {
            CoroutineHost.StartCoroutine(Terrain.WaitForWorldLoad());
        }
        catch (Exception e)
        {
            Player.main.cinematicModeActive = false;
            Log.Warn($"Something wrong happened while waiting for the terrain to load.\n{e}");
        }
    }
}


