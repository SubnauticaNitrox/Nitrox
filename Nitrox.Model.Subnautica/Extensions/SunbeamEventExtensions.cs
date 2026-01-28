using System;
using static Nitrox.Model.Subnautica.Packets.PlaySunbeamEvent;
using static Nitrox.Model.Subnautica.Packets.PlaySunbeamEvent.SunbeamEvent;

namespace Nitrox.Model.Subnautica.Extensions;

public static class SunbeamEventExtensions
{
    public static string ToStoryKey(this SunbeamEvent storyEvent) =>
        storyEvent switch
        {
            STORYSTART => "RadioSunbeamStart",
            GUNAIM => "PrecursorGunAimCheck",
            COUNTDOWN => "OnPlayRadioSunbeam4",
            _ => throw new ArgumentOutOfRangeException(nameof(storyEvent), $"Unknown {nameof(SunbeamEvent)} with number {(int)storyEvent}.")
        };
}
