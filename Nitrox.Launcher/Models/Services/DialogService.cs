using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using Nitrox.Launcher.Models.Utils;
using Nitrox.Launcher.ViewModels;
using Nitrox.Launcher.ViewModels.Abstract;
using NitroxModel.Logger;

namespace Nitrox.Launcher.Models.Services;

internal sealed class DialogService(Func<Window> dialogOwnerProvider, IEnumerable<DialogService.Mapping> viewModelToWindowMap)
{
    private readonly Func<Window> dialogOwnerProvider = dialogOwnerProvider;
    private readonly Dictionary<Type, Mapping> viewModelToWindowMap = viewModelToWindowMap.ToDictionary(m => m.ViewModelType);
    private readonly Lock viewModelToWindowMapLocker = new();

    public async Task<TViewModel?> ShowAsync<TViewModel>(Action<TViewModel>? viewModelSetup = null) where TViewModel : ModalViewModelBase
    {
        try
        {
            Mapping mapping;
            lock (viewModelToWindowMapLocker)
            {
                if (!viewModelToWindowMap.TryGetValue(typeof(TViewModel), out mapping))
                {
                    throw new Exception($"No dialog known for {typeof(TViewModel).Name}");
                }
            }
            return await Dispatcher.UIThread.InvokeAsync(() =>
            {
                Window dialog = mapping.WindowFactory(typeof(TViewModel));
                viewModelSetup?.Invoke((TViewModel)dialog.DataContext);
                return dialog.ShowDialog<TViewModel>(dialogOwnerProvider());
            });
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"Failed to show dialog for ViewModel {typeof(TViewModel).FullName}");
            LauncherNotifier.Error(ex.Message);
            return null;
        }
    }

    public Task ShowErrorAsync(Exception exception, string? title = null, string? description = null) =>
        ShowAsync<DialogBoxViewModel>(model =>
        {
            model.Title = title ?? "Error";
            model.Description = string.IsNullOrWhiteSpace(description) ? exception.ToString() : $"{description}{Environment.NewLine}{exception}";
            model.ButtonOptions = ButtonOptions.OkClipboard;
        });

    public record Mapping(Type ViewModelType, Func<Type, Window> WindowFactory);
}
