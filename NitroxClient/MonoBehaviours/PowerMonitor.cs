using NitroxClient.Communication.Packets.Processors;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Helper.GameLogic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
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
        private static readonly int POWER_POSITIVE_THRESHOLD_TO_TRIGGER_IMMEDIATE_PACKET = 2;
        private static readonly int POWER_NEGATIVE_THRESHOLD_TO_TRIGGER_IMMEDIATE_PACKET = -2;

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

        private float runningDelta = 0;
        private float elapsedTime = 0;
        public float interpolationPeriod = 4.00f;

        public void ChargeChanged(float amount, GameObject gameObject)
        {
            if (CameFromActivePowerEvent())
            {
                runningDelta += amount;
            }
        }

        /**
         * We allow small bursts of power to build up so we don't flood with packets.
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
                    String guid = GuidHelper.GetGuid(this.gameObject);
                    Multiplayer.Logic.Power.ChargeChanged(guid, runningDelta, PowerType.ENERGY_INTERFACE);
                    runningDelta = 0;
                }
            }
        }

        private bool CameFromActivePowerEvent()
        {
            StackFrame stackFrame = new StackFrame(5, true);
            MethodBase method = stackFrame.GetMethod();

            if (isActiveFlagByWhiteListedEnergyInterfaceCallers.ContainsKey(method))
            {
                return isActiveFlagByWhiteListedEnergyInterfaceCallers[method];
            }
            else
            {
                Console.WriteLine("Could not find a whitelisted power method for " + method + " (from " + method.DeclaringType + ") - it might be newly introduced!");
                Console.WriteLine(new StackTrace());
            }
            return true;
        }
    }
}
