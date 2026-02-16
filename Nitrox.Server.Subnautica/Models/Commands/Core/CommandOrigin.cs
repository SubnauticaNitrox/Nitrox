namespace Nitrox.Server.Subnautica.Models.Commands.Core;

[Flags]
internal enum CommandOrigin
{
    SERVER = 1 << 0,
    PLAYER = 1 << 1,
    ANY = SERVER | PLAYER,
    DEFAULT = ANY
}
