using Avalonia.ReactiveUI;

namespace Nitrox.Launcher.Views.Abstract;

public abstract class WindowBase<TViewModal> : ReactiveWindow<TViewModal> where TViewModal : class
{
    protected override void OnInitialized() => this.ApplyOsWindowStyling();
}
