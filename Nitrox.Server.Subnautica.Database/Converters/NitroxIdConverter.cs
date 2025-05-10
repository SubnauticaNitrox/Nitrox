extern alias JB;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NitroxModel.DataStructures;

namespace Nitrox.Server.Subnautica.Database.Converters;

// TODO: optimize - get guid from nitrox id without ToString
internal sealed class NitroxIdConverter() : ValueConverter<NitroxId, Guid>(static id => new Guid(id.ToString()), static guid => new NitroxId(guid));
