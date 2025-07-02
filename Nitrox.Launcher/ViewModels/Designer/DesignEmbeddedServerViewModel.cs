using System;
using Nitrox.Launcher.Models.Design;

namespace Nitrox.Launcher.ViewModels.Designer;

internal class DesignEmbeddedServerViewModel : EmbeddedServerViewModel
{
    public DesignEmbeddedServerViewModel() : base(new ServerEntry { Name = "Design server" })
    {
        ServerOutput.AddRange(
        [
            new OutputLine
            {
                Type = OutputLineType.INFO_LOG,
                LogText = "Server output line 1",
                Timestamp = DateTime.Now.ToString("hh:mm:ss.fff")
            }
        ]);
        ServerCommand = "quit";
    }
}
