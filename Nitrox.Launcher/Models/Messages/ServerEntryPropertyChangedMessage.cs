namespace Nitrox.Launcher.Models.Messages;

public class ServerEntryPropertyChangedMessage
{
    public string PropertyName { get; set; }

    public ServerEntryPropertyChangedMessage(string propertyName)
    {
        PropertyName = propertyName;
    }
}
