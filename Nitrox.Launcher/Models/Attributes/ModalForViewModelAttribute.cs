using System;
using Nitrox.Launcher.ViewModels.Abstract;

namespace Nitrox.Launcher.Models.Attributes;

/// <summary>
///     Marks a modal as supporting a ViewModel.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
internal sealed class ModalForViewModelAttribute : Attribute
{
    public Type ViewModelType { get; }

    public ModalForViewModelAttribute(Type viewModelType)
    {
        if (!typeof(ModalViewModelBase).IsAssignableFrom(viewModelType))
        {
            throw new Exception($"ViewModel for modals must inherit {nameof(ModalViewModelBase)}");
        }
        ViewModelType = viewModelType;
    }
}
