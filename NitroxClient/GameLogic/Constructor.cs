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
using NitroxModel_Subnautica.DataStructures;
using NitroxModel_Subnautica.Helper;
using UnityEngine;
using static NitroxClient.GameLogic.Helper.TransientLocalObjectManager;
using Color = UnityEngine.Color;
using Vector3 = UnityEngine.Vector3;
using Vector4 = UnityEngine.Vector4;

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

            Optional<object> opConstructedObject = Get(TransientObjectType.CONSTRUCTOR_INPUT_CRAFTED_GAMEOBJECT);

            if (opConstructedObject.HasValue)
            {
                GameObject constructedObject = (GameObject)opConstructedObject.Value;

                List<InteractiveChildObjectIdentifier> childIdentifiers = VehicleChildObjectIdentifierHelper.ExtractInteractiveChildren(constructedObject);
                Vehicle vehicle = constructedObject.GetComponent<Vehicle>();
                NitroxId constructedObjectId = NitroxEntity.GetId(constructedObject);
                Vector3[] hsb = new Vector3[5];
                Vector3[] colours = new Vector3[5];
                string name = "";
                float health = 1;

                if (!vehicle)
                {
                    // Cyclops
                    GameObject target = NitroxEntity.RequireObjectFrom(constructedObjectId);
                    SubNameInput subNameInput = target.RequireComponentInChildren<SubNameInput>();
                    SubName subNameTarget = (SubName)subNameInput.ReflectionGet("target");

                    colours = subNameTarget.GetColors();
                    hsb = subNameTarget.GetColors();
                    name = subNameTarget.GetName();
                    health = target.GetComponent<LiveMixin>().health;
                }
                else if (vehicle)
                {
                    // Seamoth & Prawn Suit
                    health = vehicle.GetComponent<LiveMixin>().health;
                    name = (string)vehicle.ReflectionCall("GetName", true);
                    hsb = vehicle.subName.GetColors();
                    colours = vehicle.subName.GetColors();
                }
                ConstructorBeginCrafting beginCrafting = new ConstructorBeginCrafting(constructorId,
                                                                                      constructedObjectId,
                                                                                      techType.ToDto(),
                                                                                      duration,
                                                                                      childIdentifiers,
                                                                                      constructedObject.transform.position.ToDto(),
                                                                                      constructedObject.transform.rotation.ToDto(),
                                                                                      name,
                                                                                      hsb.ToDto(),
                                                                                      colours.ToDto(),
                                                                                      health);
                vehicles.AddVehicle(VehicleModelFactory.BuildFrom(beginCrafting));
                packetSender.Send(beginCrafting);

                SpawnDefaultBatteries(constructedObject, childIdentifiers);
            }
            else
            {
                Log.Error("Could not send packet because there wasn't a corresponding constructed object!");
            }
        }

        // As the normal spawn is suppressed, spawn default batteries afterwards
        private void SpawnDefaultBatteries(GameObject constructedObject, List<InteractiveChildObjectIdentifier> childIdentifiers)
        {
            Optional<EnergyMixin> opEnergy = Optional.OfNullable(constructedObject.GetComponent<EnergyMixin>());
            if (opEnergy.HasValue)
            {
                EnergyMixin mixin = opEnergy.Value;
                mixin.ReflectionSet("allowedToPlaySounds", false);
                mixin.SetBattery(mixin.defaultBattery, 1);
                mixin.ReflectionSet("allowedToPlaySounds", true);
            }

            foreach (InteractiveChildObjectIdentifier identifier in childIdentifiers)
            {
                Optional<GameObject> opChildGameObject = NitroxEntity.GetObjectFrom(identifier.Id);

                if (opChildGameObject.HasValue)
                {
                    Optional<EnergyMixin> opEnergyMixin = Optional.OfNullable(opChildGameObject.Value.GetComponent<EnergyMixin>());

                    if (opEnergyMixin.HasValue)
                    {
                        EnergyMixin mixin = opEnergyMixin.Value;
                        mixin.ReflectionSet("allowedToPlaySounds", false);
                        mixin.SetBattery(mixin.defaultBattery, 1);
                        mixin.ReflectionSet("allowedToPlaySounds", true);
                    }
                }
            }
        }
    }
}
