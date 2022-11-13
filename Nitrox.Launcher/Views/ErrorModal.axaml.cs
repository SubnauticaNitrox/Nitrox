using System;
using Nitrox.Launcher.ViewModels;
using Nitrox.Launcher.Views.Abstract;
using ReactiveUI;

namespace Nitrox.Launcher.Views
{
    public partial class ErrorModal : ModalBase<ErrorViewModel>
    {
        public ErrorModal()
        {
            this.WhenActivated(d =>
            {
                d(ViewModel!.OkCommand.Subscribe(_ => Close()));
            });
            InitializeComponent();
        }
    }
}
