using Nitrox.Model.DataStructures;
using Nitrox.Server.Subnautica.Models.Commands.ArgConverters.Core;

namespace Nitrox.Server.Subnautica.Models.Commands.ArgConverters;

/// <summary>
///     Converts a string to a NitroxId, if valid.
/// </summary>
internal sealed class StringToNitroxIdConverter : IArgConverter<string, NitroxId>
{
    public Task<ConvertResult> ConvertAsync(string nitroxId)
    {
        try 
        {
            NitroxId id = new(nitroxId);
            return Task.FromResult(ConvertResult.Ok(id));
        }
        catch (Exception)
        {
            return Task.FromResult(ConvertResult.Fail());
        }
    }
}
