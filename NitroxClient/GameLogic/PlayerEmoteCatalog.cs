using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace NitroxClient.GameLogic;

public enum PlayerEmoteGroup
{
    Yes,
    Affirmative,
    Thanks,
    TeamUp,
    LetsGo,
    ShowOff,
    Attention,
    SorryOrCeasefire,
    No
}

public sealed class PlayerEmoteDefinition
{
    public PlayerEmoteGroup Group { get; }
    public string LanguageKey { get; }
    public IReadOnlyList<byte> SoundIndices { get; }

    public PlayerEmoteDefinition(PlayerEmoteGroup group, string languageKey, params byte[] soundIndices)
    {
        Group = group;
        LanguageKey = languageKey;
        SoundIndices = Array.AsReadOnly(soundIndices);
    }
}

public static class PlayerEmoteCatalog
{
    private static readonly IReadOnlyList<PlayerEmoteDefinition> orderedDefinitions = Array.AsReadOnly(new[]
    {
        new PlayerEmoteDefinition(PlayerEmoteGroup.Yes, "Nitrox_EmoteWheel_Yes", 20, 21, 22, 23),
        new PlayerEmoteDefinition(PlayerEmoteGroup.Affirmative, "Nitrox_EmoteWheel_Affirmative", 12, 17),
        new PlayerEmoteDefinition(PlayerEmoteGroup.Thanks, "Nitrox_EmoteWheel_Thanks", 0, 13),
        new PlayerEmoteDefinition(PlayerEmoteGroup.TeamUp, "Nitrox_EmoteWheel_TeamUp", 14, 15, 16),
        new PlayerEmoteDefinition(PlayerEmoteGroup.LetsGo, "Nitrox_EmoteWheel_LetsGo", 8, 9, 19),
        new PlayerEmoteDefinition(PlayerEmoteGroup.ShowOff, "Nitrox_EmoteWheel_ShowOff", 1, 7, 18),
        new PlayerEmoteDefinition(PlayerEmoteGroup.Attention, "Nitrox_EmoteWheel_Attention", 3, 6),
        new PlayerEmoteDefinition(PlayerEmoteGroup.SorryOrCeasefire, "Nitrox_EmoteWheel_SorryOrCeasefire", 2, 4, 5),
        new PlayerEmoteDefinition(PlayerEmoteGroup.No, "Nitrox_EmoteWheel_No", 10, 11)
    });

    private static readonly IReadOnlyDictionary<PlayerEmoteGroup, PlayerEmoteDefinition> definitionsByGroup =
        new ReadOnlyDictionary<PlayerEmoteGroup, PlayerEmoteDefinition>(orderedDefinitions.ToDictionary(definition => definition.Group));

    public static IReadOnlyList<PlayerEmoteDefinition> OrderedDefinitions => orderedDefinitions;

    public static PlayerEmoteDefinition Get(PlayerEmoteGroup group) => definitionsByGroup[group];
}
