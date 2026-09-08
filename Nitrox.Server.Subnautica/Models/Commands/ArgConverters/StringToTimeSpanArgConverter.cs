using System.Text.RegularExpressions;
using Nitrox.Server.Subnautica.Models.Commands.ArgConverters.Core;

namespace Nitrox.Server.Subnautica.Models.Commands.ArgConverters;

internal sealed partial class StringToTimeSpanArgConverter : IArgConverter<string, TimeSpan>
{
    [GeneratedRegex(@"^(\d+\.?\d*)(s|m|h|d|w)$", RegexOptions.IgnoreCase)]
    private static partial Regex DurationRegex();

    public Task<ConvertResult> ConvertAsync(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Task.FromResult(ConvertResult.Fail());
        }
        TimeSpan? result = null;
        Match match = DurationRegex().Match(value);
        if (!match.Success)
        {
            // If an int/float, we require a unit postfix like (d for days or s for seconds).
            if (!float.TryParse(value, out _) && TimeSpan.TryParse(value, out TimeSpan parsed))
            {
                return Task.FromResult(ConvertResult.Ok(parsed));
            }
        }
        else
        {
            float amount = float.Parse(match.Groups[1].Value);
            result = match.Groups[2].Value.ToLowerInvariant() switch
            {
                "s" => TimeSpan.FromSeconds(amount),
                "m" => TimeSpan.FromMinutes(amount),
                "h" => TimeSpan.FromHours(amount),
                "d" => TimeSpan.FromDays(amount),
                "w" => TimeSpan.FromDays(amount * 7),
                _ => null
            };
        }

        return Task.FromResult(result == null ? ConvertResult.Fail($"Invalid {nameof(TimeSpan)} '{value}'. Use a number followed by s/m/h/d/w (e.g. 7d)") : ConvertResult.Ok(result));
    }
}
