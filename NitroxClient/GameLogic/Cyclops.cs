using NitroxClient.Communication;
using NitroxModel.DataStructures;
using NitroxModel.Packets;
using System;

namespace NitroxClient.GameLogic
{
    public class Cyclops
    {
        private PacketSender packetSender;

        public Cyclops(PacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public void ToggleInternalLight(String guid, bool isOn)
        {
            CyclopsToggleInternalLighting packet = new CyclopsToggleInternalLighting(packetSender.PlayerId, guid, isOn);
            packetSender.Send(packet);
        }

        public void ToggleFloodLights(String guid, bool isOn)
        {
            CyclopsToggleFloodLights packet = new CyclopsToggleFloodLights(packetSender.PlayerId, guid, isOn);
            packetSender.Send(packet);
        }

        public void BeginSilentRunning(String guid)
        {
            CyclopsBeginSilentRunning beginSilentRunning = new CyclopsBeginSilentRunning(packetSender.PlayerId, guid);
            packetSender.Send(beginSilentRunning);
        }

        public void ActivateHorn(String guid)
        {
            CyclopsActivateHorn packet = new CyclopsActivateHorn(packetSender.PlayerId, guid);
            packetSender.Send(packet);
        }

        public void ActivateShield(String guid)
        {
            CyclopsActivateShield packet = new CyclopsActivateShield(packetSender.PlayerId, guid);
            packetSender.Send(packet);
        }

        public void ChangeName(String guid, string name)
        {
            CyclopsChangeName packet = new CyclopsChangeName(packetSender.PlayerId, guid, name);
            packetSender.Send(packet);
        }

        public void ChangeColor(String guid, int index, Vector3 hsb, Color color)
        {
            CyclopsChangeColor packet = new CyclopsChangeColor(packetSender.PlayerId, index, guid, hsb, color);
            packetSender.Send(packet);
        }
    }
}
