using System.ComponentModel.DataAnnotations.Schema;

namespace Nitrox.Server.Subnautica.Database.Models;

/// <summary>
///     Data used when player is in survival mode.
/// </summary>
[Table("PlayerSurvivalContext")]
public record SurvivalContext
{
    [ForeignKey(nameof(Models.Player.Id))]
    public Player Player { get; set; }

    public float Oxygen { get; set; }
    public float MaxOxygen { get; set; }
    public float Health { get; set; }
    public float Food { get; set; }
    public float Water { get; set; }
    public float InfectionAmount { get; set; }
}
