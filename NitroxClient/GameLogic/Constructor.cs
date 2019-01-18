using System;
using System.Collections.Generic;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;
using NitroxModel.Packets;
using UnityEngine;
using static NitroxClient.GameLogic.Helper.TransientLocalObjectManager;

namespace NitroxClient.GameLogic
{
    public class MobileVehicleBay
    {
        private readonly IPacketSender packetSender;

        

        public MobileVehicleBay(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public void BeginCrafting(GameObject constructor, TechType techType, float duration)
        {
            string constructorGuid = GuidHelper.GetGuid(constructor);

            Log.Debug("Building item from constructor with uuid: " + constructorGuid);

            Optional<object> opConstructedObject = TransientLocalObjectManager.Get(TransientObjectType.CONSTRUCTOR_INPUT_CRAFTED_GAMEOBJECT);

            if (opConstructedObject.IsPresent())
            {
                GameObject constructedObject = (GameObject)opConstructedObject.Get();

                List<InteractiveChildObjectIdentifier> childIdentifiers = VehicleChildObjectIdentifierHelper.ExtractGuidsOfInteractiveChildren(constructedObject);

                // Need to hardcode Cyclops untill a way to get default colours is found.
                string name = "Cyclops";
                Vector3[] colours = new Vector3[]
                {
                    new Vector3(0f, 0f, 1f),
                    new Vector3(0f, 0f, 0f),
                    new Vector3(0f, 0f, 1f),
                    new Vector3(0.577f, 0.447f, 0.604f),
                    new Vector3(0.114f, 0.729f, 0.965f)
                };

               
                if (techType != TechType.Cyclops)
                {
                    Vehicle vehicle = constructedObject.GetComponent<Vehicle>();
                    name = vehicle.vehicleName;
                    colours = vehicle.vehicleColors;
                }

                string constructedObjectGuid = GuidHelper.GetGuid(constructedObject);
                ConstructorBeginCrafting beginCrafting = new ConstructorBeginCrafting(constructorGuid, constructedObjectGuid, techType, duration, childIdentifiers, constructedObject.transform.position, constructedObject.transform.rotation, name, colours);
                packetSender.Send(beginCrafting);
            }
            else
            {
                Log.Error("Could not send packet because there wasn't a corresponding constructed object!");
            }
        }
    }
}
