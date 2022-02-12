using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel_Subnautica.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class RocketPreflightCompleteProcessor : ClientPacketProcessor<RocketPreflightComplete>
    {
        private readonly IPacketSender packetSender;

        public RocketPreflightCompleteProcessor(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override void Process(RocketPreflightComplete packet)
        {
            GameObject gameObjectRocket = NitroxEntity.RequireObjectFrom(packet.Id);

            switch (packet.FlightCheck)
            {
                //Preflights inside the cockpit, we do want to ignore TimeCapsule for now
                case PreflightCheck.LifeSupport:
                case PreflightCheck.PrimaryComputer:

                    CockpitSwitch[] cockpitSwitches = gameObjectRocket.GetComponentsInChildren<CockpitSwitch>(true);

                    foreach (CockpitSwitch cockpitSwitch in cockpitSwitches)
                    {
                        if (!cockpitSwitch.completed && cockpitSwitch.preflightCheck == packet.FlightCheck)
                        {
                            cockpitSwitch.animator.SetBool("Completed", true);
                            cockpitSwitch.completed = true;

                            CompletePreflightCheck(cockpitSwitch.preflightCheckSwitch);

                            if (cockpitSwitch.collision)
                            {
                                cockpitSwitch.collision.SetActive(false);
                            }
                        }
                    }

                    break;

                //CommunicationsArray, Hydraulics, AuxiliaryPowerUnit
                default:

                    ThrowSwitch[] throwSwitches = gameObjectRocket.GetComponentsInChildren<ThrowSwitch>(true);

                    foreach (ThrowSwitch throwSwitch in throwSwitches)
                    {
                        if (!throwSwitch.completed && throwSwitch.preflightCheck == packet.FlightCheck)
                        {
                            throwSwitch.animator?.SetTrigger("Throw");
                            throwSwitch.completed = true;
                            CompletePreflightCheck(throwSwitch.preflightCheckSwitch);
                            throwSwitch.cinematicTrigger.showIconOnHandHover = false;
                            throwSwitch.triggerCollider.enabled = false;
                            throwSwitch.lamp.GetComponent<SkinnedMeshRenderer>().material = throwSwitch.completeMat;
                        }
                    }

                    break;
            }

        }

        private void CompletePreflightCheck(PreflightCheckSwitch preflightCheckSwitch)
        {
            using (packetSender.Suppress<RocketPreflightComplete>())
            {
                preflightCheckSwitch.preflightCheckManager?.CompletePreflightCheck(preflightCheckSwitch.preflightCheck);   
            }
        }
    }
}
