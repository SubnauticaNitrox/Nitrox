using Nitrox.Launcher.Models.Services;
using NitroxModel.Helper;

namespace Nitrox.Launcher.ViewModels.Designer;

internal class DesignServersViewModel() : ServersViewModel(KeyValueStore.Instance, null!, new ServerService(null!, KeyValueStore.Instance, null!), null!);
