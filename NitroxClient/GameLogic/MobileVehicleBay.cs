using System.Collections.Generic;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxModel_Subnautica.Helper;
using UnityEngine;
using static NitroxClient.GameLogic.Helper.TransientLocalObjectManager;

namespace NitroxClient.GameLogic
{
    public class MobileVehicleBay
    {
        private readonly IPacketSender packetSender;
        private readonly Vehicles vehicles;

        public MobileVehicleBay(IPacketSender packetSender, Vehicles vehicles)
        {
            this.packetSender = packetSender;
            this.vehicles = vehicles;
        }

        public void BeginCrafting(GameObject constructor, TechType techType, float duration)
        {
            NitroxId constructorId = NitroxEntity.GetId(constructor);

            Log.Debug("Building item from constructor with id: " + constructorId);

            Optional<object> opConstructedObject = TransientLocalObjectManager.Get(TransientObjectType.CONSTRUCTOR_INPUT_CRAFTED_GAMEOBJECT);

            if (opConstructedObject.HasValue)
            {
                GameObject constructedObject = (GameObject)opConstructedObject.Value;
                List<InteractiveChildObjectIdentifier> childIdentifiers = VehicleChildObjectIdentifierHelper.ExtractInteractiveChildren(constructedObject);

                VehicleModel vehicleModel = vehicles.BuildVehicleModelFrom(constructedObject, techType);
                vehicles.AddVehicle(vehicleModel);

                ConstructorBeginCrafting beginCrafting = VehicleConstructionFactory.BuildFrom(vehicleModel, constructorId, duration);
                packetSender.Send(beginCrafting);

                vehicles.SpawnDefaultBatteries(constructedObject, childIdentifiers);
            }
            else
            {
                Log.Error("Could not send packet because there wasn't a corresponding constructed object!");
            }
        }
    }
}
