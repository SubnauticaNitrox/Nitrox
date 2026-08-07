namespace NitroxClient.GameLogic.Emotes;

internal enum EmoteWheelReleaseKind
{
    None,
    PlayRecent,
    PlaySelected
}

internal readonly struct EmoteWheelRelease
{
    public EmoteWheelReleaseKind Kind { get; }
    public PlayerEmoteGroup SelectedGroup { get; }

    private EmoteWheelRelease(EmoteWheelReleaseKind kind, PlayerEmoteGroup selectedGroup = default)
    {
        Kind = kind;
        SelectedGroup = selectedGroup;
    }

    public static EmoteWheelRelease None => new(EmoteWheelReleaseKind.None);
    public static EmoteWheelRelease PlayRecent => new(EmoteWheelReleaseKind.PlayRecent);
    public static EmoteWheelRelease PlaySelected(PlayerEmoteGroup group) => new(EmoteWheelReleaseKind.PlaySelected, group);
}

internal sealed class EmoteWheelInputState
{
    public const float HOLD_THRESHOLD_SECONDS = 0.25f;

    private float pressedAt;
    private PlayerEmoteGroup? selectedGroup;

    public bool IsArmed { get; private set; }
    public bool IsOpen { get; private set; }

    public bool Begin(float currentTime)
    {
        if (IsArmed || IsOpen)
        {
            return false;
        }

        pressedAt = currentTime;
        selectedGroup = null;
        IsArmed = true;
        return true;
    }

    public bool TryOpen(float currentTime)
    {
        if (!IsArmed || currentTime - pressedAt < HOLD_THRESHOLD_SECONDS)
        {
            return false;
        }

        IsArmed = false;
        IsOpen = true;
        return true;
    }

    public void SetSelection(PlayerEmoteGroup? group)
    {
        if (IsOpen)
        {
            selectedGroup = group;
        }
    }

    public EmoteWheelRelease Release(float currentTime)
    {
        if (IsArmed)
        {
            bool isTap = currentTime - pressedAt < HOLD_THRESHOLD_SECONDS;
            Reset();
            return isTap ? EmoteWheelRelease.PlayRecent : EmoteWheelRelease.None;
        }

        if (!IsOpen)
        {
            return EmoteWheelRelease.None;
        }

        PlayerEmoteGroup? releasedGroup = selectedGroup;
        Reset();
        return releasedGroup.HasValue
                   ? EmoteWheelRelease.PlaySelected(releasedGroup.Value)
                   : EmoteWheelRelease.None;
    }

    public void Cancel() => Reset();

    private void Reset()
    {
        IsArmed = false;
        IsOpen = false;
        selectedGroup = null;
    }
}
