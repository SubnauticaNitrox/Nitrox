using System;
using System.Reactive;
using System.Reflection;
using Avalonia;
using Avalonia.Input;
using Nitrox.Launcher.ViewModels.Abstract;
using ReactiveUI;
using ReactiveUI.Validation.Helpers;

namespace Nitrox.Launcher.ViewModels;

public class ErrorViewModel : ReactiveValidationObject, IModalViewModel
{
    public ReactiveCommand<Unit, Unit> OkCommand { get; } = ReactiveCommand.Create(() => { });
    public ReactiveCommand<Unit, Unit> CopyToClipboardCommand { get; }
    public string ErrorTitle { get; set; }
    public string? ErrorText { get; set; }
    public KeyGesture OkHotkey { get; } = new(Key.Return);
    public KeyGesture CopyToClipboardHotkey { get; } = new(Key.C, KeyModifiers.Control);

    public ErrorViewModel(Exception exception)
    {
        ErrorTitle = GetTitleFromException(exception);
        ErrorText = exception.ToString();
        CopyToClipboardCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            if (!string.IsNullOrWhiteSpace(ErrorText))
            {
                await Application.Current?.Clipboard?.SetTextAsync(ErrorText)!;
            }
        });
    }

    private string GetTitleFromException(in Exception exception)
    {
        string title = exception switch
                       {
                           TargetInvocationException e => e.InnerException?.Message,
                           _ => exception.Message
                       } ??
                       exception.Message;
        return $"Error: {title}";
    }
}
