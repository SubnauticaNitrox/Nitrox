namespace NitroxModel.DataStructures.GameLogic;

public enum Perms : byte
{
    NONE,
    PLAYER,
    MODERATOR,
    ADMIN,
    /// <summary>
    ///     Owner of the server. This is the permission used when using the server console.
    /// </summary>
    SUPERADMIN,
    DEFAULT = PLAYER
}
