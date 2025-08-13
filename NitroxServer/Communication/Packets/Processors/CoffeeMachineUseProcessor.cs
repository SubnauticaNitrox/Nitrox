using NitroxModel.DataStructures.Unity;
using NitroxModel.GameLogic.FMOD;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;

namespace NitroxServer.Communication.Packets.Processors;
public class CoffeeMachineUseProcessor : AuthenticatedPacketProcessor<CoffeeMachineUse>
{
    private readonly PlayerManager playerManager;
    private readonly float machineRange; // To modify, edit the SoundWhitelist_Subnautica.csv file
    public CoffeeMachineUseProcessor(PlayerManager playerManager, FMODWhitelist soundWhitelist)
    {
        this.playerManager = playerManager;
        soundWhitelist.TryGetSoundData("event:/sub/base/make_coffee", out SoundData coffeeMachineSoundData);
        machineRange = coffeeMachineSoundData.Radius;
    }

    public override void Process(CoffeeMachineUse packet, Player sendingPlayer)
    {
        foreach (Player player in playerManager.GetConnectedPlayers())
        {
            if (player != sendingPlayer && NitroxVector3.Distance(player.Position, sendingPlayer.Position) < machineRange && player.SubRootId.Equals(sendingPlayer.SubRootId))
            {
                player.SendPacket(packet);
            }
        }
    }
}
