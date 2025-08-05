using Nitrox.Launcher.Models.Attributes;
using Nitrox.Launcher.ViewModels;
using Nitrox.Launcher.Views.Abstract;

namespace Nitrox.Launcher.Views;

[ModalForViewModel(typeof(BackupRestoreViewModel))]
public partial class BackupRestoreModal : ModalBase
{
    public BackupRestoreModal()
    {
        InitializeComponent();
    }
}
