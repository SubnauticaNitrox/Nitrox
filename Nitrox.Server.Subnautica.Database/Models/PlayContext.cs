using System.ComponentModel.DataAnnotations.Schema;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Unity;

namespace Nitrox.Server.Subnautica.Database.Models;

/// <summary>
///     Play session data. This record is removed when the respective player disconnects.
/// </summary>
[Table("PlayerContext")]
public record PlayContext
{
    [ForeignKey(nameof(Session.Id))]
    public PlayerSession Session { get; set; }

    public NitroxVector3 Position { get; set; }

    /// <summary>
    ///     Synchronization id of the player object in the Subnautica world.
    /// </summary>
    public NitroxId NitroxId { get; set; }

    /// <summary>
    ///     Synchronization id of the structure the player has entered.
    /// </summary>
    /// <remarks>
    ///     Subnautica uses <c>SubRoot</c> terminology for any interior the player can enter (like cyclops, seamoth or player
    ///     bases).
    /// </remarks>
    public NitroxId SubRootId { get; set; }
}
