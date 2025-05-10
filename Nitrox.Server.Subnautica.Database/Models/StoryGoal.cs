using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Database.Models;

[Table("StoryGoals")]
public record StoryGoal
{
    public int Id { get; set; }

    /// <summary>
    ///     Key or name of the goal as known by Subnautica.
    /// </summary>
    public string GoalKey { get; set; }

    /// <summary>
    ///     Gets or sets the offset in seconds from initial Subnautica world time that this goal should trigger.
    /// </summary>
    public float ExecuteTime { get; set; }

    public Schedule.GoalCategory Category { get; set; }
    public GoalPhase Phase { get; set; }

    /// <summary>
    ///     The real-world time when the event last changed.
    /// </summary>
    [Required]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTimeOffset Changed { get; set; } = DateTimeOffset.Now; // TODO: VERIFY THIS WORKS

    public enum GoalPhase
    {
        NONE,
        RADIO_QUEUE,
        COMPLETED
    }
}
