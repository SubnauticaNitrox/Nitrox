using System;
using Nitrox.Launcher.ViewModels;
using Nitrox.Launcher.Views.Abstract;
using ReactiveUI;

namespace Nitrox.Launcher.Views
{
    public partial class CreateServerModal : ModalBase<CreateServerViewModel>
    {
        public CreateServerModal()
        {
            this.WhenActivated(d =>
            {
                d(ViewModel!.BackCommand.Subscribe(Close));
                d(ViewModel!.CreateServerCommand.Subscribe(Close));
            });
            InitializeComponent();
        }
    }
}
