namespace NitroxModel.DataStructures.GameLogic
{
    public enum Perms : byte
    {
        PLAYER = 0x01,
        MODERATOR = 0x02,
        ADMIN = 0x03,
        CONSOLE = 0x04,
        DEBUG = 0x05,
        ANY = CONSOLE | DEBUG
    }
}
