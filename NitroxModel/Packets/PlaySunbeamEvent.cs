using System;
using System.Collections.Generic;

namespace NitroxModel.Packets;

[Serializable]
public class PlaySunbeamEvent : Packet
{
    public SunbeamEvent Event { get; }

    public PlaySunbeamEvent(SunbeamEvent @event)
    {
        Event = @event;
    }

    /// <summary>
    /// Represents the index of the associated goal in <see cref="SunbeamGoals" />
    /// </summary>
    public enum SunbeamEvent
    {
        STORYSTART = 0,
        COUNTDOWN = 9,
        GUNAIM = 10
    }

    [NonSerialized]
    public static readonly List<string> SunbeamGoals = new() { "RadioSunbeamStart", "OnPlayRadioSunbeamStart", "RadioSunbeam1", "OnPlayRadioSunbeam1", "RadioSunbeam2", "OnPlayRadioSunbeam2", "RadioSunbeam3", "OnPlayRadioSunbeam3", "RadioSunbeam4", "OnPlayRadioSunbeam4", "PrecursorGunAimCheck", "PrecursorGunAim", "SunbeamCheckPlayerRange", "PDASunbeamDestroyEventOutOfRange", "PDASunbeamDestroyEventInRange" };
}
