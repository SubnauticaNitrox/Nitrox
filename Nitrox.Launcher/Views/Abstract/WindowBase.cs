using System;
using System.Threading.Tasks;
using Avalonia.ReactiveUI;
using Nitrox.Launcher.ViewModels.Abstract;
using ReactiveUI;

namespace Nitrox.Launcher.Views.Abstract;

public class WindowBase<TViewModal> : ReactiveWindow<TViewModal> where TViewModal : class
{
    /// <summary>
    ///     Registers a modal that will be owned by this window where the input and output data of the modal is the same <see cref="TModalViewModel"/>.
    /// </summary>
    protected void RegisterModal<TModal, TModalViewModel>(Func<Interaction<TModalViewModel, TModalViewModel?>> interactionGetter) where TModal : ModalBase<TModalViewModel>, new() where TModalViewModel : ModalViewModelBase
    {
        this.WhenActivated(d => d(interactionGetter().RegisterHandler(DefaultShowDialogAsync<TModal, TModalViewModel>)));
    }

    private async Task DefaultShowDialogAsync<TModal, TModalViewModel>(InteractionContext<TModalViewModel, TModalViewModel?> interaction) where TModal : ModalBase<TModalViewModel>, new() where TModalViewModel : ModalViewModelBase
    {
        TModal dialog = new() { DataContext = interaction.Input };
        TModalViewModel? result = await dialog.ShowDialog<TModalViewModel?>(this);
        interaction.SetOutput(result);
    }
}
