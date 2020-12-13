using System.Reflection;
using FMOD.Studio;
using Nitrox.Client.Communication.Abstract;
using Nitrox.Client.Communication.Packets.Processors.Abstract;
using Nitrox.Client.MonoBehaviours;
using Nitrox.Client.Unity.Helper;
using Nitrox.Model.Subnautica.Packets;
using UnityEngine;

namespace Nitrox.Client.Communication.Packets.Processors
{
    public class CyclopsActivateHornProcessor : ClientPacketProcessor<CyclopsActivateHorn>
    {
        private readonly IPacketSender packetSender;

        private readonly FieldInfo fieldInfo = typeof(FMOD_CustomEmitter).GetField("evt", BindingFlags.Instance | BindingFlags.NonPublic);

        public CyclopsActivateHornProcessor(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override void Process(CyclopsActivateHorn hornPacket)
        {
            GameObject cyclops = NitroxEntity.RequireObjectFrom(hornPacket.Id);
            CyclopsHornControl horn = cyclops.RequireComponentInChildren<CyclopsHornControl>();

            EventInstance eventInstance = (EventInstance)fieldInfo.GetValue(horn.hornSound);
            eventInstance.setProperty(EVENT_PROPERTY.MAXIMUM_DISTANCE, 150f);
            fieldInfo.SetValue(horn.hornSound, eventInstance);

            horn.OnHandClick(null);
        }
    }
}
