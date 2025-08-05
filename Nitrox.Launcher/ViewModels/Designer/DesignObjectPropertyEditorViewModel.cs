using NitroxModel.Serialization;

namespace Nitrox.Launcher.ViewModels.Designer;

internal class DesignObjectPropertyEditorViewModel : ObjectPropertyEditorViewModel
{
    public DesignObjectPropertyEditorViewModel() : base(null!)
    {
        OwnerObject = new SubnauticaServerConfig();
    }
}
