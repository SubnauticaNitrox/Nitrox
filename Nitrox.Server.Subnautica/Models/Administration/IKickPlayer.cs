using Nitrox.Model.Core;
using Nitrox.Server.Subnautica.Models.Administration.Core;

namespace Nitrox.Server.Subnautica.Models.Administration;

internal interface IKickPlayer : IAdminFeature<IKickPlayer>
{
    Task<bool> KickPlayer(SessionId sessionId, string reason = "");
}
