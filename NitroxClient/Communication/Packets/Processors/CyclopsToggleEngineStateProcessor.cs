using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.Helper;
using NitroxModel.Helper.GameLogic;
using NitroxModel.Helper.Unity;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class CyclopsToggleEngineStateProcessor : ClientPacketProcessor<CyclopsToggleEngineState>
    {
        private PacketSender packetSender;

        public CyclopsToggleEngineStateProcessor(PacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override void Process(CyclopsToggleEngineState enginePacket)
        {
            GameObject cyclops = GuidHelper.RequireObjectFrom(enginePacket.Guid);
            CyclopsEngineChangeState engineState = cyclops.RequireComponentInChildren<CyclopsEngineChangeState>();
            CyclopsMotorMode motorMode = cyclops.RequireComponentInChildren<CyclopsMotorMode>();
            CyclopsHelmHUDManager hudManager = cyclops.RequireComponentInChildren<CyclopsHelmHUDManager>();

            if (enginePacket.IsOn == engineState.motorMode.engineOn)
            {
                if ((enginePacket.IsStarting != (bool)engineState.ReflectionGet("startEngine")) != enginePacket.IsOn)
                {
                    if (Player.main.currentSub != engineState.subRoot)
                    {
                        engineState.ReflectionSet("startEngine", !enginePacket.IsOn);
                        engineState.ReflectionSet("invalidButton", true);
                        engineState.Invoke("ResetInvalidButton", 2.5f);
                        engineState.subRoot.BroadcastMessage("InvokeChangeEngineState", !enginePacket.IsOn, SendMessageOptions.RequireReceiver);
                    }
                    else
                    {
                        engineState.ReflectionSet("invalidButton", false);
                        using (packetSender.Suppress<CyclopsToggleInternalLighting>())
                        {
                            engineState.OnClick();
                        }
                    }
                }
            }
        }
    }
}
