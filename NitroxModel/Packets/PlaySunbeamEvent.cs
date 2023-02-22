using System;

namespace NitroxModel.Packets;

[Serializable]
public class PlaySunbeamEvent : Packet
{
    public string EventKey { get; }

    public PlaySunbeamEvent(string eventKey)
    {
        EventKey = eventKey;
    }

    /// <summary>
    /// Associates an understandable event name and the associated goal from <see cref="SunbeamGoals" />.
    /// </summary>
    public static class SunbeamEvent
    {
        public const string STORYSTART = "RadioSunbeamStart";
        public const string COUNTDOWN = "OnPlayRadioSunbeam4";
        public const string GUNAIM = "PrecursorGunAimCheck";
    }

    /// <summary>
    /// An ordered list of the goals forming part of the whole Sunbeam story.
    /// </summary>
    /// <remarks>
    /// If you modify this list, make sure to accordingly modify <see cref="SunbeamEvent"/>.
    /// </remarks>
    [NonSerialized]
    public static readonly string[] SunbeamGoals = new string[] { SunbeamEvent.STORYSTART, "OnPlayRadioSunbeamStart", "RadioSunbeam1", "OnPlayRadioSunbeam1", "RadioSunbeam2", "OnPlayRadioSunbeam2", "RadioSunbeam3", "OnPlayRadioSunbeam3", "RadioSunbeam4", SunbeamEvent.COUNTDOWN, SunbeamEvent.GUNAIM, "PrecursorGunAim", "SunbeamCheckPlayerRange", "PDASunbeamDestroyEventOutOfRange", "PDASunbeamDestroyEventInRange" };
}
