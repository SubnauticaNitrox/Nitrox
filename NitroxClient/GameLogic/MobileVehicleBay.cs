using NitroxClient.Communication.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.GameLogic;

public class MobileVehicleBay
{
    public static bool TransmitLocalSpawns { get; set; } = true;
    public static GameObject MostRecentlyCrafted { get; set; }

    private readonly IPacketSender packetSender;
    private readonly Vehicles vehicles;

    public MobileVehicleBay(IPacketSender packetSender, Vehicles vehicles)
    {
        this.packetSender = packetSender;
        this.vehicles = vehicles;
    }

    public void BeginCrafting(ConstructorInput constructor, GameObject constructedObject, TechType techType, float duration)
    {
        MostRecentlyCrafted = constructedObject;

        // Sometimes build templates, such as the cyclops, are already tagged with IDs.  Remove any that exist to retag.
        // TODO: this seems to happen because various patches execute when the cyclops template loads (on game load).
        // This will leave vehicles with NitroxEntity but an empty NitroxId.  We need to chase these down and only call
        // the code paths when the owner has a simulation lock.
        Vehicles.RemoveNitroxEntitiesTagging(constructedObject);

        if (!TransmitLocalSpawns)
        {
            return;
        }

        constructor.constructor.TryGetIdOrWarn(out NitroxId constructorId);

        NitroxId constructedObjectId = NitroxEntity.GenerateNewId(constructedObject);

        VehicleWorldEntity vehicleEntity = Vehicles.BuildVehicleWorldEntity(constructedObject, constructedObjectId, techType, constructorId);

        packetSender.Send(new EntitySpawnedByClient(vehicleEntity));

        constructor.StartCoroutine(vehicles.UpdateVehiclePositionAfterSpawn(constructedObjectId, techType, constructedObject, duration + 10.0f));
    }
}
