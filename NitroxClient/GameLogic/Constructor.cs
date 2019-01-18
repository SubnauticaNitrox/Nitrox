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

                Vehicle vehicle = constructedObject.GetComponent<Vehicle>();
                string name = vehicle.subName.GetName();
  
                Vector3[] colours = vehicle.subName.GetColors(); // This doesnt work for the Prawn Suit Not sure why...so untill i find a better way ive just hard coded the default values
                if(techType == TechType.Exosuit)
                {
                    colours = new Vector3[] {

                        new Vector3(0f, 0f, 1f),
                        new Vector3(0f, 0f, 0f),
                        new Vector3(0f, 0f, 1f),
                        new Vector3(0.577f, 0.447f, 0.604f),
                        new Vector3(0.114f, 0.729f, 0.965f)
                    };
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
