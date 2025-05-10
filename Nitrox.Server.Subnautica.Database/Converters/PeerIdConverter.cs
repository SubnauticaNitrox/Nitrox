extern alias JB;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NitroxModel.Networking;

namespace Nitrox.Server.Subnautica.Database.Converters;

internal sealed class PeerIdIdConverter() : ValueConverter<PeerId, uint>(static id => id,
                                                                         static id => id);
