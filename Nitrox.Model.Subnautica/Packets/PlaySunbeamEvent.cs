using System;
using Nitrox.Model.Subnautica.Extensions;
using NitroxModel.Networking.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public record PlaySunbeamEvent : Packet
{
    /// <summary>
    ///     Associates an understandable event name and the associated goal from <see cref="SunbeamGoals" />.
    /// </summary>
    public enum SunbeamEvent
    {
        STORYSTART,
        COUNTDOWN,
        GUNAIM
    }

    /// <summary>
    ///     An ordered list of the goals forming part of the whole Sunbeam story.
    /// </summary>
    /// <remarks>
    ///     If you modify this list, make sure to accordingly modify <see cref="SunbeamEvent" />.
    /// </remarks>
    [NonSerialized]
    public static readonly string[] SunbeamGoals =
    [
        SunbeamEvent.STORYSTART.ToSubnauticaStoryKey(), "OnPlayRadioSunbeamStart", "RadioSunbeam1", "OnPlayRadioSunbeam1", "RadioSunbeam2", "OnPlayRadioSunbeam2", "RadioSunbeam3", "OnPlayRadioSunbeam3", "RadioSunbeam4",
        SunbeamEvent.COUNTDOWN.ToSubnauticaStoryKey(), SunbeamEvent.GUNAIM.ToSubnauticaStoryKey(), "PrecursorGunAim", "SunbeamCheckPlayerRange", "PDASunbeamDestroyEventOutOfRange", "PDASunbeamDestroyEventInRange"
    ];

    public PlaySunbeamEvent(string eventKey)
    {
        EventKey = eventKey;
    }

    public string EventKey { get; }
}
