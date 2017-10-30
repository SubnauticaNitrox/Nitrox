using NitroxClient.Communication;
using NitroxModel.Packets;
using System;
using UnityEngine;

namespace NitroxClient.GameLogic
{
    public class Cyclops
    {
        private readonly PacketSender packetSender;

        public Cyclops(PacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public void ToggleInternalLight(string guid, bool isOn)
        {
            CyclopsToggleInternalLighting packet = new CyclopsToggleInternalLighting(packetSender.PlayerId, guid, isOn);
            packetSender.Send(packet);
        }

        public void ToggleFloodLights(string guid, bool isOn)
        {
            CyclopsToggleFloodLights packet = new CyclopsToggleFloodLights(packetSender.PlayerId, guid, isOn);
            packetSender.Send(packet);
        }

        public void BeginSilentRunning(string guid)
        {
            CyclopsBeginSilentRunning packet = new CyclopsBeginSilentRunning(packetSender.PlayerId, guid);
            packetSender.Send(packet);
        }

        public void ChangeEngineMode(string guid, CyclopsMotorMode.CyclopsMotorModes mode)
        {
            CyclopsChangeEngineMode packet = new CyclopsChangeEngineMode(packetSender.PlayerId, guid, mode);
            packetSender.Send(packet);
        }

        public void ToggleEngineState(string guid, bool isOn, bool isStarting)
        {
            CyclopsToggleEngineState packet = new CyclopsToggleEngineState(packetSender.PlayerId, guid, isOn, isStarting);
            packetSender.Send(packet);
        }

        public void ActivateHorn(string guid)
        {
            CyclopsActivateHorn packet = new CyclopsActivateHorn(packetSender.PlayerId, guid);
            packetSender.Send(packet);
        }

        public void ActivateShield(string guid)
        {
            CyclopsActivateShield packet = new CyclopsActivateShield(packetSender.PlayerId, guid);
            packetSender.Send(packet);
        }

        public void ChangeName(string guid, string name)
        {
            CyclopsChangeName packet = new CyclopsChangeName(packetSender.PlayerId, guid, name);
            packetSender.Send(packet);
        }

        public void ChangeColor(string guid, int index, Vector3 hsb, Color color)
        {
            CyclopsChangeColor packet = new CyclopsChangeColor(packetSender.PlayerId, index, guid, hsb, color);
            packetSender.Send(packet);
        }
    }
}
