using System;
using System.Collections.Generic;
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

namespace NitroxClient.GameLogic.Helper
{
    public static class VehicleHelper
    {
        public static bool IsVehicle(TechType techtype)
        {
            bool res = false;

            switch (techtype)
            {
                case TechType.Seamoth:
                case TechType.Exosuit:
                case TechType.Cyclops:
                    res = true;
                    break;
            }

            return res;
        }

        public static ConstructorBeginCrafting BuildFrom(GameObject gameObject, TechType techType, List<InteractiveChildObjectIdentifier> childIdentifiers, NitroxId constructorId, float duration)
        {
            if (IsVehicle(techType))
            {
                Optional<Vehicle> opvehicle = Optional.OfNullable(gameObject.GetComponent<Vehicle>());

                NitroxId constructedObjectId = NitroxEntity.GetId(gameObject);
                Vector3[] Colours = new Vector3[] { new Vector3(0f, 0f, 1f) };
                string name = string.Empty;
                float health = 200f;


                if (opvehicle.HasValue)
                { //Seamoth & Exosuit
                    Optional<LiveMixin> livemixin = Optional.OfNullable(opvehicle.Value.GetComponent<LiveMixin>());

                    if (livemixin.HasValue)
                    {
                        health = livemixin.Value.health;
                    }

                    name = opvehicle.Value.GetName();
                    Colours = opvehicle.Value.subName?.GetColors();
                }
                else
                { //Cyclops
                    try
                    {
                        GameObject target = NitroxEntity.RequireObjectFrom(constructedObjectId);
                        SubNameInput subNameInput = target.RequireComponentInChildren<SubNameInput>();
                        SubName subNameTarget = (SubName)subNameInput.ReflectionGet("target");

                        Colours = subNameTarget?.GetColors();
                        name = subNameTarget.GetName();
                        health = target.GetComponent<LiveMixin>().health;
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Error while trying to spawn a cyclops", ex);
                    }
                }

                return new ConstructorBeginCrafting(
                    constructorId,
                    constructedObjectId,
                    techType.Model(),
                    duration,
                    childIdentifiers,
                    gameObject.transform.position,
                    gameObject.transform.rotation,
                    name,
                    Colours,
                    Colours,
                    health
                );
            }
            else
            {
                Log.Error($"Impossible to build from a non-vehicle GameObject (Received {techType})");
            }

            return null;
        }

        public static void SpawnDefaultBatteries(GameObject constructedObject, List<InteractiveChildObjectIdentifier> childIdentifiers)
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
