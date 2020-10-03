using System;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours;
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
                            if (cockpitSwitch.preflightCheck == packet.FlightCheck && !cockpitSwitch.completed)
                            {
                                cockpitSwitch.animator?.SetTrigger("Completed");
                                cockpitSwitch.completed = true;

                                cockpitSwitch.preflightCheckSwitch?.CompletePreflightCheck();

                                if (cockpitSwitch.collision)
                                {
                                    cockpitSwitch.collision.SetActive(false);
                                }
                            }
                        }

                        break;

                    //CommunicationsArray, Hydraulics, LifeSupport
                    default:

                        ThrowSwitch[] throwSwitches = gameObjectRocket.GetComponentsInChildren<ThrowSwitch>(true);

                        foreach (ThrowSwitch throwSwitch in throwSwitches)
                        {
                            if (throwSwitch.preflightCheck == packet.FlightCheck && !throwSwitch.completed)
                            {
                                throwSwitch.animator?.SetTrigger("Throw");
                                throwSwitch.completed = true;
                                throwSwitch.preflightCheckSwitch?.CompletePreflightCheck();
                                throwSwitch.cinematicTrigger.showIconOnHandHover = false;
                                throwSwitch.triggerCollider.enabled = false;
                                throwSwitch.lamp.GetComponent<SkinnedMeshRenderer>().material = throwSwitch.completeMat;
                            }
                        }

                        break;
                }

            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occured while processing RocketPreflightComplete packet");
                Log.InGame("Error while processing a preflight complete packet :(");
            }
        }
    }
}
