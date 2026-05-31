using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class PermsChangedProcessor(LocalPlayer localPlayer) : IClientPacketProcessor<PermsChanged>
{
    public delegate void PermissionsChanged(Perms perms);

    private readonly LocalPlayer localPlayer = localPlayer;
    public PermissionsChanged OnPermissionsChanged;

    public Task Process(ClientProcessorContext context, PermsChanged packet)
    {
        localPlayer.Permissions = packet.NewPerms;
        OnPermissionsChanged(packet.NewPerms);
        return Task.CompletedTask;
    }
}
