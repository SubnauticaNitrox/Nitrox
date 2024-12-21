
namespace NitroxModel.Packets;
public class PlayerInfectionLevelChange : Packet
{
    public ushort PlayerID { get; }
    public int InfectionLevel { get; }
    public PlayerInfectionLevelChange(int infectionLevel, ushort playerID)
    {
        InfectionLevel = infectionLevel;
        PlayerID = playerID;
    }
}
