using System;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Nitrox.Launcher.ViewModels;
using ReactiveUI;

namespace Nitrox.Launcher.Views
{
    public partial class CreateServerModal : ReactiveWindow<CreateServerViewModel>
    {
        public CreateServerModal()
        {
            this.WhenActivated(d =>
            {
                d(ViewModel!.CreateServerCommand.Subscribe(Close));
            });
            AvaloniaXamlLoader.Load(this);
        }
    }
}
