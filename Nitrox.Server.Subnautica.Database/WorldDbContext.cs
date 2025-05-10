using Microsoft.EntityFrameworkCore;
using Nitrox.Server.Subnautica.Database.Converters;
using Nitrox.Server.Subnautica.Database.Models;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Unity;
using NitroxModel.Networking;

namespace Nitrox.Server.Subnautica.Database;

public class WorldDbContext(DbContextOptions<WorldDbContext> options) : DbContext(options)
{
    public DbSet<Player> Players { get; set; }
    public DbSet<PlayContext> PlayContexts { get; set; }
    public DbSet<PlayerSession> PlayerSessions { get; set; }
    public DbSet<SurvivalContext> PlayerSurvivalStats { get; set; }
    public DbSet<StoryGoal> StoryGoals { get; set; }

    /// <summary>
    ///     Batch cells that have been parsed.
    /// </summary>
    public DbSet<BatchCell> BatchCells { get; set; }

    public WorldDbContext() : this(new DbContextOptions<WorldDbContext>())
    {
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder builder)
    {
        base.ConfigureConventions(builder);

        builder
            .Properties<NitroxId>()
            .HaveConversion<NitroxIdConverter>();
        builder
            .Properties<NitroxInt3>()
            .HaveConversion<NitroxInt3Converter>();
        builder
            .Properties<NitroxVector3>()
            .HaveConversion<NitroxVector3Converter>();
        builder
            .Properties<NitroxQuaternion>()
            .HaveConversion<NitroxQuaternionConverter>();
        builder
            .Properties<SessionId>()
            .HaveConversion<SessionIdConverter>();
        builder
            .Properties<PeerId>()
            .HaveConversion<PeerIdIdConverter>();
    }
}
