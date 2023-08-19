namespace Nitrox.Launcher.Models.Messages;

/// <summary>
///     Sent when a save is deleted outside of the Servers view (i.e. server manage view or via file explorer).
/// </summary>
public class SaveDeletedMessage
{
    public string SaveName { get; set; }

    public SaveDeletedMessage(string saveName)
    {
        SaveName = saveName;
    }
}
