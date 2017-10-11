using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.Unity.Helper;
using NitroxModel.Helper;
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
                            motorMode.cyclopsMotorMode = motorPacket.Mode;
                            float num = motorMode.motorModeSpeeds[(int)motorMode.cyclopsMotorMode];
                            motorMode.subController.BaseForwardAccel = num;
                            motorMode.subController.BaseVerticalAccel = num;
                            motorMode.subController.NewSpeed((int)motorMode.cyclopsMotorMode);
                            motorMode.subRoot.BroadcastMessage("NewAlarmState", null, SendMessageOptions.DontRequireReceiver);
                            motorModeButton.image.sprite = motorModeButton.activeSprite;
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