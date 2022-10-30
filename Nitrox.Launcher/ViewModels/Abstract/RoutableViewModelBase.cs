using System;
using System.Security.Cryptography;
using System.Text;
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

    protected RoutableViewModelBase(IScreen hostScreen)
    {
        HostScreen = hostScreen;
    }
}
