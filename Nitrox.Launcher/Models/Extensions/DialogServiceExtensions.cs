using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Threading;
using HanumanInstitute.MvvmDialogs;
using Nitrox.Launcher.Models.Utils;
using Nitrox.Launcher.ViewModels;
using Nitrox.Launcher.ViewModels.Abstract;
using NitroxModel.Logger;

namespace Nitrox.Launcher.Models.Extensions;

public static class DialogServiceExtensions
{
    public static async Task<T> ShowAsync<T, TExtra>(this IDialogService dialogService, Action<T, TExtra> setup = null, TExtra extraParameter = default) where T : ModalViewModelBase
    {
        try
        {
            ArgumentNullException.ThrowIfNull(dialogService);
            // DataContext must be accessed on the UI thread, or it'll throw error.
            INotifyPropertyChanged owner = await Dispatcher.UIThread.InvokeAsync(() => AppViewLocator.MainWindow?.DataContext as INotifyPropertyChanged);
            if (owner == null)
            {
                throw new InvalidOperationException($"Expected {nameof(AppViewLocator.MainWindow)}.{nameof(AppViewLocator.MainWindow.DataContext)} to not be null");
            }

            T viewModel = dialogService.CreateViewModel<T>();
            setup?.Invoke(viewModel, extraParameter);
            bool? result = await dialogService.ShowDialogAsync<T>(owner, viewModel);
            if (result == true)
            {
                return viewModel;
            }
            return default;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to show dialog");
            LauncherNotifier.Error(ex.Message);
            return default;
        }
    }

    public static Task<T> ShowAsync<T>(this IDialogService dialogService, Action<T> setup = null) where T : ModalViewModelBase => dialogService.ShowAsync<T, Action<T>>((model, act) => act?.Invoke(model), setup);

    public static Task ShowErrorAsync(this IDialogService dialogService, Exception exception, string title = null, string description = null) =>
        dialogService.ShowAsync<DialogBoxViewModel>(model =>
        {
            model.Title = title ?? "Error";
            model.Description = string.IsNullOrWhiteSpace(description) ? exception.ToString() : $"{description}{Environment.NewLine}{exception}";
            model.DescriptionForeground = new SolidColorBrush(Colors.Red);
            model.ButtonOptions = ButtonOptions.OkClipboard;
        });
}
