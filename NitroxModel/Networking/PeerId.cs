namespace NitroxModel.Networking;

/// <summary>
///     Gets the connection id. Is 0 for server. Starts from 1 if player.
/// </summary>
/// <remarks>
///     This matches the ID in the database.
/// </remarks>
public readonly record struct PeerId
{
    public const uint SERVER_ID = 0;

    private readonly uint id;

    private PeerId(uint id)
    {
        this.id = id;
    }

    public bool IsServer => id == SERVER_ID;

    public bool IsPlayer => id != SERVER_ID;

    public static implicit operator uint(PeerId id) => id.id;

    public static implicit operator PeerId(uint id) => new(id);
}
