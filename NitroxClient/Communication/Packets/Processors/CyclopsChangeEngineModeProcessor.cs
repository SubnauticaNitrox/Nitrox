using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.Helper;
using NitroxModel.Helper.GameLogic;
using NitroxModel.Helper.Unity;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class CyclopsChangeEngineModeProcessor : ClientPacketProcessor<CyclopsChangeEngineMode>
    {
        private PacketSender packetSender;

        public CyclopsChangeEngineModeProcessor(PacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override void Process(CyclopsChangeEngineMode motorPacket)
        {
            GameObject cyclops = GuidHelper.RequireObjectFrom(motorPacket.Guid);
            CyclopsHelmHUDManager hud = cyclops.RequireComponentInChildren<CyclopsHelmHUDManager>();
            CyclopsMotorMode motorMode = cyclops.RequireComponentInChildren<CyclopsMotorMode>();
            CyclopsMotorModeButton[] motorModeButtons = cyclops.GetComponentsInChildren<CyclopsMotorModeButton>();

            Validate.IsTrue(motorModeButtons.Length > 0, "Cyclops does not have any motormode buttons!");

            if (motorPacket.Mode != motorMode.cyclopsMotorMode)
            {
                foreach (CyclopsMotorModeButton motorModeButton in motorModeButtons)
                {
                    if (Player.main.currentSub == (SubRoot)motorModeButton.ReflectionGet("subRoot"))
                    {
                        motorModeButton.SetCyclopsMotorMode((CyclopsMotorMode.CyclopsMotorModes)motorPacket.Mode);
                    }
                    else
                    {
                        if (motorPacket.Mode == motorModeButton.motorModeIndex)
                        {
                            motorModeButton.image.sprite = motorModeButton.activeSprite;
                            motorModeButton.SendMessageUpwards("ChangeCyclopsMotorMode", motorModeButton.motorModeIndex, SendMessageOptions.RequireReceiver);
                        }
                        else
                        {
                            motorModeButton.image.sprite = motorModeButton.inactiveSprite;
                        }
                    }
                }

            }
        }
    }
}