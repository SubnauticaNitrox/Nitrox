using System;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Nitrox.Launcher.Models;

namespace Nitrox.Launcher.ViewModels.Abstract;

internal abstract class ViewModelBase : ObservableValidator, IMessageReceiver
{
    protected ViewModelBase()
    {
        ThrowIfViewModelCtorWasEmptyWhileNonEmptyExists();
    }

    public virtual void Dispose() => WeakReferenceMessenger.Default.UnregisterAll(this);

    /// <summary>
    ///     This will check that DI did not call the empty ViewModel constructor if dependencies for another constructor aren't met.
    /// </summary>
    [Conditional("DEBUG")]
    private static void ThrowIfViewModelCtorWasEmptyWhileNonEmptyExists()
    {
        if (IsDesignMode)
        {
            return;
        }
        foreach (StackFrame stackFrame in new StackTrace(2, false).GetFrames())
        {
            if (stackFrame.GetMethod() is not { IsConstructor: true } method)
            {
                continue;
            }
            if (method.DeclaringType is not { IsAbstract: false } declaringType)
            {
                continue;
            }
            if (method.GetParameters().Length > 0 || declaringType.GetConstructors().Length == 1)
            {
                break;
            }
            throw new Exception($"Empty ViewModel constructor of '{declaringType.Name}' should only be used in design mode! Check that the DI-container has all the dependencies to call a different constructor.");
        }
    }
}
