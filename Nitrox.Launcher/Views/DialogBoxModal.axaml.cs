using Nitrox.Launcher.Models.Attributes;
using Nitrox.Launcher.ViewModels;
using Nitrox.Launcher.Views.Abstract;

namespace Nitrox.Launcher.Views;

[ModalForViewModel(typeof(DialogBoxViewModel))]
public partial class DialogBoxModal : ModalBase
{
    public DialogBoxModal()
    {
        InitializeComponent();
    }
}
