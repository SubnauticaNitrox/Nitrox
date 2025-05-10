using NitroxClient.GameLogic;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Networking.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class PermsChangedProcessor : IClientPacketProcessor<PermsChanged>
{
    private LocalPlayer localPlayer;

    public delegate void PermissionsChanged(Perms perms);
    public PermissionsChanged OnPermissionsChanged;

    public PermsChangedProcessor(LocalPlayer localPlayer)
    {
        this.localPlayer = localPlayer;
    }

    public Task Process(IPacketProcessContext context, PermsChanged packet)
    {
        localPlayer.Permissions = packet.NewPerms;
        OnPermissionsChanged(packet.NewPerms);

        return Task.CompletedTask;
    }
}
