using NitroxModel.Dto;

namespace Nitrox.Server.Subnautica.Extensions.Dto;

public static class ConnectedPlayerDtoExtensions
{
    public static ConnectedPlayerDto ToConnectedPlayerDto(this Database.Models.PlayerSession session) =>
        new()
        {
            Id = session.Player.Id,
            SessionId = session.Id,
            Name = session.Player.Name,
            Permissions = session.Player.Permissions,
        };
}
