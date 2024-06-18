using System;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Security.Cryptography;
using System.Text;
using Avalonia;
using ReactiveUI;

namespace Nitrox.Launcher.ViewModels.Abstract;

public abstract class RoutableViewModelBase : ViewModelBase, IRoutableViewModel, IActivatableViewModel
{
    private Vector lastScrollPosition;

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

    protected RoutableViewModelBase(IScreen screen)
    {
        HostScreen = screen;

        this.WhenActivated(disposables =>
        {
            if (MainWindow.DataContext is MainWindowViewModel vmMain)
            {
                vmMain.ScrollViewerOffset = lastScrollPosition;
            }

            // Deactivation
            Disposable
                .Create(this, vm =>
                {
                    lastScrollPosition = (MainWindow.DataContext as MainWindowViewModel)?.ScrollViewerOffset ?? Vector.Zero;
                })
                .DisposeWith(disposables);
        });
    }

    /// <summary>
    ///     Pass-through event from MVVM toolkit to ReactiveUI.
    /// </summary>
    public void RaisePropertyChanging(PropertyChangingEventArgs args) => OnPropertyChanging(args);

    /// <summary>
    ///     Pass-through event from MVVM toolkit to ReactiveUI.
    /// </summary>
    public void RaisePropertyChanged(PropertyChangedEventArgs args) => OnPropertyChanged(args);

    public ViewModelActivator Activator { get; } = new();
}
