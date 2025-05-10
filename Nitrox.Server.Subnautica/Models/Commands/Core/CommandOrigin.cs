using System;

namespace Nitrox.Server.Subnautica.Models.Commands.Core;

[Flags]
public enum CommandOrigin
{
    SERVER = 1 << 0,
    PLAYER = 1 << 1,
    ANY = SERVER | PLAYER,
    DEFAULT = ANY
}
