using System.Collections.Generic;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
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

        public void getObjectAttributes(Vehicle vehicle, string name, Vector3[] hsb, Vector3[] colours, Vector4 tmpColour, string guid)
        {
            if(!vehicle)
            { // Cylcops
                GameObject target = GuidHelper.RequireObjectFrom(guid);
                SubNameInput subNameInput = target.RequireComponentInChildren<SubNameInput>();
                SubName subNameTarget = (SubName)subNameInput.ReflectionGet("target");
                name = subNameTarget.GetName();
                hsb = subNameTarget.GetColors();

                for (int i = 0; i < subNameTarget.GetColors().Length; i++)
                {
                    colours[i] = tmpColour;
                }
            }
            else
            { // Seamoth & Prawn Suit
                name = vehicle.vehicleName;
                hsb = vehicle.vehicleColors;
                for (int i = 0; i < vehicle.vehicleColors.Length; i++)
                {
                    colours[i] = tmpColour;
                }
            }
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
                string constructedObjectGuid = GuidHelper.GetGuid(constructedObject);

                //Initialize some default values to serialize.
                Vector3[] HSB = new Vector3[]
                {
                    new Vector3(0f, 0f, 1f),
                    new Vector3(0f, 0f, 0f),
                    new Vector3(0f, 0f, 1f),
                    new Vector3(0.577f, 0.447f, 0.604f),
                    new Vector3(0.114f, 0.729f, 0.965f)
                };

                Vector3[] Colours = new Vector3[5];
                Vector4 tmpColour = Color.white;
                string name = "Cyclops";

                getObjectAttributes(vehicle, name, HSB, Colours, tmpColour, constructedObjectGuid);
                ConstructorBeginCrafting beginCrafting = new ConstructorBeginCrafting(constructorGuid, constructedObjectGuid, techType, duration, childIdentifiers, constructedObject.transform.position, constructedObject.transform.rotation, name, HSB, Colours);
                packetSender.Send(beginCrafting);
            }
            else
            {
                Log.Error("Could not send packet because there wasn't a corresponding constructed object!");
            }
        }
    }
}
