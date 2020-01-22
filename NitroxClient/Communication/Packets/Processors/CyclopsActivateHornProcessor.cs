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

        private readonly FieldInfo fieldInfo = typeof(FMOD_CustomEmitter).GetField("evt", BindingFlags.Instance | BindingFlags.NonPublic);

        public CyclopsActivateHornProcessor(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override void Process(CyclopsActivateHorn hornPacket)
        {
            GameObject cyclops = NitroxIdentifier.RequireObjectFrom(hornPacket.Id);
            CyclopsHornControl horn = cyclops.RequireComponentInChildren<CyclopsHornControl>();

            EventInstance eventInstance = (EventInstance)fieldInfo.GetValue(horn.hornSound);
            eventInstance.setProperty(EVENT_PROPERTY.MAXIMUM_DISTANCE, 150f);
            fieldInfo.SetValue(horn.hornSound, eventInstance);

            horn.OnHandClick(null);
        }
    }
}
