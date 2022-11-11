using System;
using System.Reactive.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;

namespace Nitrox.Launcher.ViewModels.Abstract;

public abstract class RoutableViewModelBase : ReactiveObject, IRoutableViewModel
{
    /// <summary>
    ///     Gets the unique URL for the view.
    /// </summary>
    public string UrlPathSegment
    {
        get
        {
            using MD5 md5 = MD5.Create();
            byte[] inputBytes = Encoding.ASCII.GetBytes(GetType().Name);
            byte[] hashBytes = md5.ComputeHash(inputBytes);
            return Convert.ToHexString(hashBytes);
        }
    }

    public IScreen HostScreen { get; }
    protected MainWindowViewModel MainViewModel => (MainWindowViewModel)HostScreen;

    protected RoutableViewModelBase(IScreen hostScreen)
    {
        HostScreen = hostScreen;
    }

    /// <summary>
    ///     Shows the dialog (interaction) and returns the result when it closes.
    /// </summary>
    protected async Task<TModalViewModel?> ShowDialogAsync<TModalViewModel>(Interaction<TModalViewModel, TModalViewModel?> interaction) where TModalViewModel : IModalViewModel, new()
    {
        return await interaction.Handle(new());
    }
}
