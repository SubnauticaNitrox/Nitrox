using System;

namespace Nitrox.Model.Subnautica.DataStructures.GameLogic;

/// <summary>
///     States that are reset when the server goes offline, so we don't need to persist these.
/// </summary>
[Serializable]
public class SessionSettings
{
    public bool FastHatch;
    public bool FastGrow;
}
