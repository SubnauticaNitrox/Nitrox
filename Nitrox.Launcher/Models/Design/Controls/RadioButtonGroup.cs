using System;
using System.Reactive;
using Avalonia;
using Avalonia.Controls;
using ReactiveUI;

namespace Nitrox.Launcher.Models.Design.Controls;

public class RadioButtonGroup : ItemsControl
{
    public static readonly DirectProperty<RadioButtonGroup, Type> EnumProperty = AvaloniaProperty.RegisterDirect<RadioButtonGroup, Type>(nameof(Enum), o => o.Enum, (o, v) => o.Enum = v);
    public static readonly StyledProperty<object> SelectedItemProperty = AvaloniaProperty.Register<RadioButtonGroup, object>(nameof(SelectedItem));

    public static readonly DirectProperty<RadioButtonGroup, ReactiveCommand<Button, Unit>> ItemClickCommandProperty = AvaloniaProperty.RegisterDirect<RadioButtonGroup, ReactiveCommand<Button, Unit>>(nameof(ItemClickCommand), o => o.ItemClickCommand, (o, v) => o.ItemClickCommand = v);

    private Type @enum;
    private ReactiveCommand<Button, Unit> itemClickCommand;

    public Type Enum
    {
        get => @enum;
        set
        {
            if (value is not { IsEnum: true })
            {
                return;
            }

            ItemsSource = System.Enum.GetValues(value);
            SetAndRaise(EnumProperty, ref @enum, value);
        }
    }

    public ReactiveCommand<Button, Unit> ItemClickCommand
    {
        get => itemClickCommand;
        private set => SetAndRaise(ItemClickCommandProperty, ref itemClickCommand, value);
    }

    public object SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    public RadioButtonGroup()
    {
        itemClickCommand = ReactiveCommand.Create<Button>(param => SelectedItem = param.Tag);
    }

    protected override Type StyleKeyOverride { get; } = typeof(ItemsControl);
}
