namespace Nitrox.Model.Constants;

public static class NitroxConstants
{
    /// <summary>
    ///     The environment variable used for storing the home directory of the logged in operating system user.
    /// </summary>
    public const string HOST_HOME_ENV_VAR_NAME = "NITROX_HOST_HOME";
    public const string NITROX_CLIENT_DLL_NAME = "NitroxClient.dll";
    public const string LAUNCHER_APP_NAME = "Nitrox.Launcher";
    public const string PLAYER_NAME_VALID_REGEX = @"^[ a-zA-Z0-9._-]{3,25}$";
    public const string SAVE_FILE_ENDING = ".json";
}
