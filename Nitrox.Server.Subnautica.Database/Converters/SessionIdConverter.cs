extern alias JB;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NitroxModel.Networking;

namespace Nitrox.Server.Subnautica.Database.Converters;

internal sealed class SessionIdConverter() : ValueConverter<SessionId, ushort>(static id => id,
                                                                                 static id => id);
