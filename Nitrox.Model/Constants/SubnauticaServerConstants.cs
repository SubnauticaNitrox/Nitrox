namespace Nitrox.Model.Constants;

public static class SubnauticaServerConstants
{
    public const int DEFAULT_PORT = 11000;

    /// <summary>
    ///     Default seed in development so starting spawn (escape pod) stays the same.
    /// </summary>
    /// <remarks>
    ///     This seed causes first escape pod spawn to be reasonably close to [-112, 0, -320] which we determined is optimal start position.
    /// </remarks>
    public const string DEFAULT_DEVELOPMENT_SEED = "95311395";
}
