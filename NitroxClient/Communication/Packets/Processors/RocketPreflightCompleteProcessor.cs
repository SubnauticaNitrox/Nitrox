using System;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.Helper;
using NitroxModel.Logger;
using NitroxModel_Subnautica.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class RocketPreflightCompleteProcessor : ClientPacketProcessor<RocketPreflightComplete>
    {
        public override void Process(RocketPreflightComplete packet)
        {
            try
            {
                GameObject gameObjectRocket = NitroxEntity.RequireObjectFrom(packet.Id);

                switch (packet.FlightCheck)
                {
                    case PreflightCheck.AuxiliaryPowerUnit:
                    case PreflightCheck.PrimaryComputer:

                        CockpitSwitch[] cockpitSwitches = gameObjectRocket.GetComponentsInChildren<CockpitSwitch>(true);

                        foreach (CockpitSwitch cockpitSwitch in cockpitSwitches)
                        {
                            if (!cockpitSwitch.completed && cockpitSwitch.preflightCheck == packet.FlightCheck)
                            {
                                cockpitSwitch.animator.SetTrigger("Activate");
                                cockpitSwitch.completed = true;

                                if (cockpitSwitch.collision)
                                {
                                    cockpitSwitch.collision.SetActive(false);
                                }

                                cockpitSwitch.ReflectionCall("SystemReady", false, false);
                            }
                        }

                        break;

                    //CommunicationsArray, Hydraulics, LifeSupport
                    default:

                        ThrowSwitch[] throwSwitches = gameObjectRocket.GetComponentsInChildren<ThrowSwitch>(true);

                        foreach (ThrowSwitch throwSwitch in throwSwitches)
                        {
                            if (!throwSwitch.completed && throwSwitch.preflightCheck == packet.FlightCheck)
                            {
                                throwSwitch.animator.SetTrigger("Throw");
                                throwSwitch.completed = true;
                                throwSwitch.lamp.GetComponent<SkinnedMeshRenderer>().material = throwSwitch.completeMat;
                                throwSwitch.triggerCollider.enabled = false;
                                throwSwitch.cinematicTrigger.showIconOnHandHover = false;
                            }
                        }

                        break;
                }

            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occured while processing RocketPreflightComplete packet");
                Log.InGame("Error while processing a preflight complete packet :(");
                throw;
            }
        }
    }
}

