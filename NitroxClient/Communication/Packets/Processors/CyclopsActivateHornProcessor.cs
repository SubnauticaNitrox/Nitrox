using System.Reflection;
using FMOD.Studio;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel_Subnautica.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class CyclopsActivateHornProcessor : ClientPacketProcessor<CyclopsActivateHorn>
    {
        private readonly IPacketSender packetSender;

        public CyclopsActivateHornProcessor(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override void Process(CyclopsActivateHorn hornPacket)
        {
            GameObject cyclops = NitroxIdentifier.RequireObjectFrom(hornPacket.Id);
            CyclopsHornControl horn = cyclops.RequireComponentInChildren<CyclopsHornControl>();

            FieldInfo field = typeof(FMOD_CustomEmitter).GetField("evt", BindingFlags.Instance | BindingFlags.NonPublic);
            EventInstance eventInstance = (EventInstance)field.GetValue(horn.hornSound);
            eventInstance.setProperty(EVENT_PROPERTY.MAXIMUM_DISTANCE, 150f);
            field.SetValue(horn.hornSound, eventInstance);

            horn.OnHandClick(null);
        }
    }
}
