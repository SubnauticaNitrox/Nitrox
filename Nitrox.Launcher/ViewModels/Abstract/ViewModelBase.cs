using System.Reactive.Linq;
using System.Threading.Tasks;
using ReactiveUI;

namespace Nitrox.Launcher.ViewModels.Abstract;

public class ViewModelBase : ReactiveObject
{
    /// <summary>
    ///     Shows the dialog (interaction) and returns the result when it closes.
    /// </summary>
    protected async Task<TModalViewModel?> ShowDialogAsync<TModalViewModel>(Interaction<TModalViewModel, TModalViewModel?> interaction) where TModalViewModel : IModalViewModel, new()
    {
        return await interaction.Handle(new());
    }
}
