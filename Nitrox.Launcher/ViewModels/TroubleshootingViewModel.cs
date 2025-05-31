using Nitrox.Launcher.ViewModels.Abstract;
using NitroxModel.Helper;

namespace Nitrox.Launcher.ViewModels;
public partial class TroubleshootingViewModel : RoutableViewModelBase
{
    private readonly IKeyValueStore keyValueStore;

    public TroubleshootingViewModel()
    {
    }
    public TroubleshootingViewModel(IKeyValueStore keyValueStore)
    {
        this.keyValueStore = keyValueStore;
    }
}
