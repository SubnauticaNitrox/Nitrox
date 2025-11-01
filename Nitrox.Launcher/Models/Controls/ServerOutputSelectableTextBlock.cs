using System;
using Avalonia.Controls;
using Avalonia.Input;

namespace Nitrox.Launcher.Models.Controls;

public class ServerOutputSelectableTextBlock : SelectableTextBlock
{
    public static event EventHandler<ServerOutputSelectableTextBlock>? SelectionStartedGlobal;

    public ServerOutputSelectableTextBlock()
    {
        SelectionStartedGlobal += OnOtherSelectionStarted;
    }

    ~ServerOutputSelectableTextBlock()
    {
        SelectionStartedGlobal -= OnOtherSelectionStarted;
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        SelectionStartedGlobal?.Invoke(this, this);
    }

    private void OnOtherSelectionStarted(object? sender, ServerOutputSelectableTextBlock selectedBlock)
    {
        if (!ReferenceEquals(this, selectedBlock))
        {
            ClearSelection();
        }
    }
    
    protected override Type StyleKeyOverride { get; } = typeof(SelectableTextBlock);
}
