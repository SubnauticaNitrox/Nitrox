using System;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Text;
using ReactiveUI;
using ReactiveUI.Validation.Abstractions;
using ReactiveUI.Validation.Contexts;

namespace Nitrox.Launcher.ViewModels.Abstract;

public abstract class RoutableViewModelBase : ViewModelBase, IRoutableViewModel, IValidatableViewModel
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
    protected RoutingState Router => MainViewModel.Router;

    protected RoutableViewModelBase(IScreen hostScreen)
    {
        HostScreen = hostScreen;
    }

    /// <summary>
    ///     Pass-through event from MVVM toolkit to ReactiveUI.
    /// </summary>
    public void RaisePropertyChanging(PropertyChangingEventArgs args) => OnPropertyChanging(args);

    /// <summary>
    ///     Pass-through event from MVVM toolkit to ReactiveUI.
    /// </summary>
    public void RaisePropertyChanged(PropertyChangedEventArgs args) => OnPropertyChanged(args);

    public ValidationContext ValidationContext { get; }
}
