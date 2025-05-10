namespace NitroxModel.Dto;

// TODO: DO STUFF WITH THIS!
public record JoiningPlayerDto
{
    /// <summary>
    ///     ID of the player as it's known in the database.
    /// </summary>
    public ushort PlayerId { get; init; }

    /// <summary>
    ///     Password that the client must know to access the same player object when reconnecting. This key is first generated
    ///     and assigned when a new player is joining.
    /// </summary>
    public string OwnershipKey { get; init; }
}
