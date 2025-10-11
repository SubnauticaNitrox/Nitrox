using System;

namespace Nitrox.Model.DataStructures.GameLogic;

/// <summary>
/// States that are temporary for one session so we don't need to persist them
/// </summary>
[Serializable]
public class SessionSettings
{
    public bool FastHatch;
    public bool FastGrow;
}
