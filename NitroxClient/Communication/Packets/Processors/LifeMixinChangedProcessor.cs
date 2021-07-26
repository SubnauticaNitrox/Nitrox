using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors
{
    class LifeMixinChangedProcessor : ClientPacketProcessor<LiveMixinHealthChanged>
    {
        private readonly IPacketSender packetSender;
        private readonly LiveMixinManager liveMixinManager;
        public LifeMixinChangedProcessor(IPacketSender packetSender, LiveMixinManager liveMixinManager)
        {
            this.packetSender = packetSender;
            this.liveMixinManager = liveMixinManager;

        }
        public override void Process(LiveMixinHealthChanged packet)
        {
            liveMixinManager.ProcessRemoteHealthChange(packet.Id, packet.LifeChanged, packet.DamageTakenData, packet.TotalHealth);
        }
    }
}
