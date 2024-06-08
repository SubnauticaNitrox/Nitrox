using System;
using System.ComponentModel;
using System.Threading.Tasks;
using HanumanInstitute.MvvmDialogs;
using Nitrox.Launcher.ViewModels.Abstract;

namespace Nitrox.Launcher.Models.Extensions;

public static class DialogServiceExtensions
{
    public static async Task<T> ShowAsync<T, TExtra>(this IDialogService dialogService, Action<T, TExtra> setup = null, TExtra extraParameter = default) where T : ModalViewModelBase
    {
        ArgumentNullException.ThrowIfNull(dialogService);
        INotifyPropertyChanged owner = AppViewLocator.MainWindow.DataContext as INotifyPropertyChanged;
        ArgumentNullException.ThrowIfNull(owner);

        T viewModel = dialogService.CreateViewModel<T>();
        setup?.Invoke(viewModel, extraParameter);
        bool? result = await dialogService.ShowDialogAsync<T>(owner, viewModel);
        if (result == true)
        {
            return viewModel;
        }
        return default;
    }

    public static Task<T> ShowAsync<T>(this IDialogService dialogService, Action<T> setup = null) where T : ModalViewModelBase => dialogService.ShowAsync<T, Action<T>>((model, act) => act?.Invoke(model), setup);
}
