using Nitrox.Launcher.Models.Attributes;
using Nitrox.Launcher.ViewModels;
using Nitrox.Launcher.Views.Abstract;

namespace Nitrox.Launcher.Views;

[ModalForViewModel(typeof(CreateServerViewModel))]
public partial class CreateServerModal : ModalBase
{
    public CreateServerModal()
    {
        InitializeComponent();
    }
}
