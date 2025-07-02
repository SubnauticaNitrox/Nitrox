using Nitrox.Launcher.Models.Attributes;
using Nitrox.Launcher.ViewModels;
using Nitrox.Launcher.Views.Abstract;

namespace Nitrox.Launcher.Views;

[ModalForViewModel(typeof(ObjectPropertyEditorViewModel))]
public partial class ObjectPropertyEditorModal : ModalBase
{
    public ObjectPropertyEditorModal()
    {
        InitializeComponent();
    }
}
