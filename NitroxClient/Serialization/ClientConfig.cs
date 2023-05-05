using System;
using System.IO;
using NitroxModel.Serialization;

namespace NitroxClient.Serialization;

public class ClientConfigSettings
{
    public static readonly string NitroxRoamingDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Nitrox");
}

/// <summary>
///     This is the client config for Nitrox
///     The file should be stored under %appdata%\Roaming\Nitrox\client.cfg
/// </summary>
[PropertyDescription("These are the settings for the Nitrox Client.")]
public class ClientConfig : NitroxConfig<ClientConfig>
{
    public override string FileName => "client.cfg";

    [PropertyDescription("Keybinds to open chat [Default primary is Y]")]
    public string OpenChatKeybindPrimary { get; set; } = "Y";

    public string OpenChatKeybindSecondary { get; set; } = "";

    [PropertyDescription("Keybinds to focus discord, ALT + [Default primary is F]")]
    public string FocusDiscordKeybindPrimary { get; set; } = "F";

    public string FocusDiscordKeybindSecondary { get; set; } = "";

    /// <summary>
    ///     This method initialises the client config
    ///     It checks for an existing file and returns back either a fresh instance, or an existing instance
    ///     This is ran once on game startup. Use <see cref="ClientConfig.Load" /> to retrieve instance
    /// </summary>
    /// <returns>Instance of ClientConfig</returns>
    public static ClientConfig InitClientConfig()
    {
        if (!Directory.Exists(ClientConfigSettings.NitroxRoamingDir))
        {
            Directory.CreateDirectory(ClientConfigSettings.NitroxRoamingDir);
        }

        ClientConfig cfg = null;
        try
        {
            if (File.Exists(Path.Combine(ClientConfigSettings.NitroxRoamingDir, "client.cfg")))
            {
                cfg = Load(ClientConfigSettings.NitroxRoamingDir);
            }
            else
            {
                cfg = new ClientConfig();
                cfg.Serialize(ClientConfigSettings.NitroxRoamingDir);
            }
        }
        catch (Exception ex)
        {
            Log.Error($"Unable to load client config: {ex.Message}");
        }

        Log.Info("Client Config Registered.");

        return cfg;
    }
}
