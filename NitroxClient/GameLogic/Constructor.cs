using System.Collections.Generic;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.GameLogic.Spawning;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
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
        private readonly StorageSlots storageSlots;


        public MobileVehicleBay(IPacketSender packetSender, StorageSlots storageSlots)
        {
            this.packetSender = packetSender;
            this.storageSlots = storageSlots;
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
                Vector3[] HSB = new Vector3[5];
                Vector3[] Colours = new Vector3[5];
                Vector4 tmpColour = Color.white;
                string name = "";

                if (!vehicle)
                { // Cylcops
                    GameObject target = GuidHelper.RequireObjectFrom(constructedObjectGuid);
                    SubNameInput subNameInput = target.RequireComponentInChildren<SubNameInput>();
                    SubName subNameTarget = (SubName)subNameInput.ReflectionGet("target");

                    Colours = subNameTarget.GetColors();
                    HSB = subNameTarget.GetColors();
                    name = subNameTarget.GetName();
                }
                else if(vehicle)
                { // Seamoth & Prawn Suit
                    name = (string)vehicle.ReflectionCall("GetName", true);
                    HSB = vehicle.subName.GetColors();
                    Colours = vehicle.subName.GetColors();
                }
                ConstructorBeginCrafting beginCrafting = new ConstructorBeginCrafting(constructorGuid, constructedObjectGuid, techType.Model(), duration, childIdentifiers, constructedObject.transform.position, constructedObject.transform.rotation, name, HSB, Colours);
                packetSender.Send(beginCrafting);

                // Mark vehicle as controlled by nitrox (used for sending add/remove batteries aka storage slots)
                constructedObject.AddComponent<NitroxEntity>();
                BroadcastDefaultBatterySlots(constructedObject, childIdentifiers);
            }
            else
            {
                Log.Error("Could not send packet because there wasn't a corresponding constructed object!");
            }
        }

        // The server has no notice of the default batteries spawned for the vehicle. This info will be send to the server
        private void BroadcastDefaultBatterySlots(GameObject constructedObject, List<InteractiveChildObjectIdentifier> childIdentifiers)
        {
            
            Optional<EnergyMixin> opEnergy = Optional<EnergyMixin>.OfNullable(constructedObject.GetComponent<EnergyMixin>());
            if (opEnergy.IsPresent())
            {
                EnergyMixin mixin = opEnergy.Get();
                StorageSlot slot = (StorageSlot)mixin.ReflectionGet("batterySlot");
                foreach (InventoryItem item in slot)
                {
                    storageSlots.BroadcastItemAdd(item, constructedObject);
                }
            }
            foreach (InteractiveChildObjectIdentifier identifier in childIdentifiers)
            {
                Optional<GameObject> opChildGameObject = GuidHelper.GetObjectFrom(identifier.Guid);
                if (opChildGameObject.IsPresent())
                {
                                        
                    opChildGameObject.Get().AddComponent<NitroxEntity>();
                    Optional<EnergyMixin> opEnergyMixin = Optional<EnergyMixin>.OfNullable(opChildGameObject.Get().GetComponent<EnergyMixin>());
                    if(opEnergyMixin.IsPresent())
                    {
                        
                        EnergyMixin mixin = opEnergyMixin.Get();
                        StorageSlot slot = (StorageSlot)mixin.ReflectionGet("batterySlot");
                        foreach (InventoryItem item in slot)
                        {
                            storageSlots.BroadcastItemAdd(item, mixin.gameObject);
                        }
                    }
                }
            }
        }
    }
}
