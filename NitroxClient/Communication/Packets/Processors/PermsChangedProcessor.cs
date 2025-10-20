using Nitrox.Model.DataStructures.GameLogic;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using Nitrox.Model.Packets;
using Nitrox.Model.Subnautica.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class PermsChangedProcessor : ClientPacketProcessor<PermsChanged>
{
    private LocalPlayer localPlayer;

    public delegate void PermissionsChanged(Perms perms);
    public PermissionsChanged OnPermissionsChanged;

    public PermsChangedProcessor(LocalPlayer localPlayer)
    {
        this.localPlayer = localPlayer;
    }

    public override void Process(PermsChanged packet)
    {
        localPlayer.Permissions = packet.NewPerms;
        OnPermissionsChanged(packet.NewPerms);
    }
}
