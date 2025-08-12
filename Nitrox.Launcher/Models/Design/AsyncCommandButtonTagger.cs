using System;
using System.Collections.Concurrent;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Reactive;
using CommunityToolkit.Mvvm.Input;

namespace Nitrox.Launcher.Models.Design;

/// <summary>
///     Listens for async command changes on buttons to add the chosen classname to, for use with styling.
/// </summary>
public class AsyncCommandButtonTagger : IDisposable
{
    public string ClassName { get; init; }
    private readonly ConcurrentDictionary<ICommand, BusyState> states = [];
    private readonly IDisposable commandChangeSubscription;

    public AsyncCommandButtonTagger(string className)
    {
        ClassName = className;
        commandChangeSubscription = Button.CommandProperty.Changed.Subscribe(new AnonymousObserver<AvaloniaPropertyChangedEventArgs<ICommand>>(ButtonCommandChangedOnNext));

        void ButtonCommandChangedOnNext(AvaloniaPropertyChangedEventArgs<ICommand> args)
        {
            if (args.Sender is not Button button)
            {
                return;
            }
            if (args.OldValue.Value is { } oldCommand && states.TryRemove(oldCommand, out BusyState oldState))
            {
                oldState.Dispose();
            }
            if (args.NewValue.Value is { } newCommand)
            {
                states.TryAdd(newCommand, new BusyState(ClassName, newCommand, button));
            }
        }
    }

    private class BusyState : IDisposable
    {
        public string ClassName { get; }
        private ICommand Command { get; }
        private Button Button { get; }

        public BusyState(string className, ICommand command, Button button)
        {
            ClassName = className;
            Command = command;
            Button = button;
            Command.CanExecuteChanged += CommandOnCanExecuteChanged;
        }

        public void Dispose()
        {
            Command.CanExecuteChanged -= CommandOnCanExecuteChanged;
            Button.Classes.Set(ClassName, false);
        }

        private void CommandOnCanExecuteChanged(object sender, EventArgs e)
        {
            if (sender is IAsyncRelayCommand asyncCommand)
            {
                Button.Classes.Set(ClassName, asyncCommand.IsRunning);
            }
        }
    }

    public void Dispose()
    {
        commandChangeSubscription.Dispose();
    }

    /// <summary>
    ///     Removes the busy states of buttons.
    /// </summary>
    public void Clear()
    {
        foreach ((ICommand _, BusyState value) in states)
        {
            value.Dispose();
        }
        states.Clear();
    }
}
