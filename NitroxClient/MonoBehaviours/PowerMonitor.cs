using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using NitroxClient.Communication.Packets.Processors;
using NitroxClient.GameLogic;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Logger;
using UnityEngine;

namespace NitroxClient.MonoBehaviours
{
    /**
     * We have two types of power events that can happen:
     * 
     * 1) Passive - not caused by a player, such as a docked vehicle recharging
     * 2) Active - caused by a player, such as driving a vehicle forward
     * 
     * Passive events 'should' be automatically synced between players (ignoring some
     * edge cases such as clock skew and floating point arithmetic) Active events 
     * need to be sent over the wire.  
     */
    public class PowerMonitor : MonoBehaviour
    {
        private const int POWER_POSITIVE_THRESHOLD_TO_TRIGGER_IMMEDIATE_PACKET = 2;
        private const int POWER_NEGATIVE_THRESHOLD_TO_TRIGGER_IMMEDIATE_PACKET = -2;

        /**
         * Since the code base does not differentiate between active and passive 
         * sources we have no choice but to evalaute the caller.  We choose to 
         * check against a fixed dictionary of sources so we can error if a new  
         * one is added later.
         */
        private static Dictionary<MethodBase, bool> isActiveFlagByWhiteListedEnergyInterfaceCallers = new Dictionary<MethodBase, bool>()
        {
            { typeof(Vehicle).GetMethod("ConsumeEnergy", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(float) }, null), true },
            { typeof(Vehicle).GetMethod("AddEnergy", BindingFlags.NonPublic | BindingFlags.Instance), false },
            { typeof(Vehicle).GetMethod("ReplenishOxygen", BindingFlags.NonPublic | BindingFlags.Instance), false },
            { typeof(Vehicle).GetMethod("UpdateEnergyRecharge", BindingFlags.NonPublic | BindingFlags.Instance), false },
            { typeof(PropulsionCannon).GetMethod("Update", BindingFlags.NonPublic | BindingFlags.Instance), false },
            { typeof(PropulsionCannon).GetMethod("OnShoot", BindingFlags.Public | BindingFlags.Instance), false },
            { typeof(PowerLevelChangedProcessor).GetMethod("Process", BindingFlags.Public | BindingFlags.Instance), false },
        };

        private Power powerBoardcaster;
        private float runningDelta = 0;
        private float elapsedTime = 0;
        private float interpolationPeriod = 4.00f;

        public void ChargeChanged(float amount, GameObject gameObject)
        {
            if (CameFromActivePowerEvent())
            {
                runningDelta += amount;
            }
        }

        public void Awake()
        {
            powerBoardcaster = NitroxServiceLocator.LocateService<Power>();
        }

        /**
         * We allow small bursts of power to Build up so we don't flood with packets.
         * The buffer will be flushed upon reaching the timeout period or the threshold.
         */
        public void Update()
        {
            elapsedTime += Time.deltaTime;

            if (elapsedTime >= interpolationPeriod ||
                runningDelta > POWER_POSITIVE_THRESHOLD_TO_TRIGGER_IMMEDIATE_PACKET ||
                runningDelta < POWER_NEGATIVE_THRESHOLD_TO_TRIGGER_IMMEDIATE_PACKET)
            {
                elapsedTime = 0;

                if (runningDelta != 0)
                {
                    NitroxId id = NitroxEntity.GetId(gameObject);
                    powerBoardcaster.ChargeChanged(id, runningDelta, PowerType.ENERGY_INTERFACE);
                    runningDelta = 0;
                }
            }
        }

        private bool CameFromActivePowerEvent()
        {
            StackFrame stackFrame = new StackFrame(5, true);
            MethodBase method = stackFrame.GetMethod();

            bool isMethodActive;
            if (isActiveFlagByWhiteListedEnergyInterfaceCallers.TryGetValue(method, out isMethodActive))
            {
                return isMethodActive;
            }

            Log.Error("Could not find a whitelisted power method for " + method + " (from " + method.DeclaringType + ") - it might be newly introduced!");
            Log.Error(new StackTrace().ToString());

            return true;
        }
    }
}
