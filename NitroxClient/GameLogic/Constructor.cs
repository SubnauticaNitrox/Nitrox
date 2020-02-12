using System.Collections.Generic;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures;
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
        private readonly Vehicles vehicles;
        private readonly StorageSlots storageSlots;
        
        public MobileVehicleBay(IPacketSender packetSender, Vehicles vehicles, StorageSlots storageSlots)
        {
            this.packetSender = packetSender;
            this.vehicles = vehicles;
            this.storageSlots = storageSlots;
        }

        public void BeginCrafting(GameObject constructor, TechType techType, float duration)
        {
            NitroxId constructorId = NitroxEntity.GetId(constructor);

            Log.Debug("Building item from constructor with id: " + constructorId);

            Optional<object> opConstructedObject = TransientLocalObjectManager.Get(TransientObjectType.CONSTRUCTOR_INPUT_CRAFTED_GAMEOBJECT);

            if (opConstructedObject.IsPresent())
            {
                GameObject constructedObject = (GameObject)opConstructedObject.Get();

                List<InteractiveChildObjectIdentifier> childIdentifiers = VehicleChildObjectIdentifierHelper.ExtractInteractiveChildren(constructedObject);
                Vehicle vehicle = constructedObject.GetComponent<Vehicle>();
                NitroxId constructedObjectId = NitroxEntity.GetId(constructedObject);
                Vector3[] HSB = new Vector3[5];
                Vector3[] Colours = new Vector3[5];
                Vector4 tmpColour = Color.white;
                string name = "";
                
                if (!vehicle)
                { // Cylcops
                    GameObject target = NitroxEntity.RequireObjectFrom(constructedObjectId);
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
                ConstructorBeginCrafting beginCrafting = new ConstructorBeginCrafting(constructorId, constructedObjectId, techType.Model(), duration, childIdentifiers, constructedObject.transform.position, constructedObject.transform.rotation, 
                    name, HSB, Colours);
                vehicles.AddVehicle(VehicleModelFactory.BuildFrom(beginCrafting));
                packetSender.Send(beginCrafting);

                SpawnDefaultBatteries(constructedObject, childIdentifiers);
            }
            else
            {
                Log2.Instance.Log(NLogType.Error, "Could not send packet because there wasn't a corresponding constructed object!");
            }
        }

        // As the normal spawn is suppressed, spawn default batteries afterwards
        private void SpawnDefaultBatteries(GameObject constructedObject, List<InteractiveChildObjectIdentifier> childIdentifiers)
        {
            
            Optional<EnergyMixin> opEnergy = Optional<EnergyMixin>.OfNullable(constructedObject.GetComponent<EnergyMixin>());
            if (opEnergy.IsPresent())
            {
                EnergyMixin mixin = opEnergy.Get();                
                mixin.ReflectionSet("allowedToPlaySounds", false);
                mixin.SetBattery(mixin.defaultBattery, 1);
                mixin.ReflectionSet("allowedToPlaySounds", true);
            }

            foreach (InteractiveChildObjectIdentifier identifier in childIdentifiers)
            {
                Optional<GameObject> opChildGameObject = NitroxEntity.GetObjectFrom(identifier.Id);

                if (opChildGameObject.IsPresent())
                {
                    Optional<EnergyMixin> opEnergyMixin = Optional<EnergyMixin>.OfNullable(opChildGameObject.Get().GetComponent<EnergyMixin>());

                    if(opEnergyMixin.IsPresent())
                    {
                        
                        EnergyMixin mixin = opEnergyMixin.Get();
                        mixin.ReflectionSet("allowedToPlaySounds", false);
                        mixin.SetBattery(mixin.defaultBattery, 1);
                        mixin.ReflectionSet("allowedToPlaySounds", true);
                    }
                }
            }
        }
    }
}
