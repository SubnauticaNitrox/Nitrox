using System;
using Nitrox.Launcher.Models;
using Nitrox.Launcher.ViewModels;
using Nitrox.Launcher.Views.Abstract;

namespace Nitrox.Launcher.Views;

public partial class ManageServerView : RoutableViewBase<ManageServerViewModel>
{
    public ManageServerView()
    {
        InitializeComponent();

        //PlayerPermsComboBox.ItemsSource = Enum.GetValues(typeof(PlayerPermissions));
    }
}
