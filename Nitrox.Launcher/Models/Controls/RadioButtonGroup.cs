using System;
using Avalonia;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;

namespace Nitrox.Launcher.Models.Controls;

public class RadioButtonGroup : ItemsControl
{
    public static readonly DirectProperty<RadioButtonGroup, Type> EnumProperty = AvaloniaProperty.RegisterDirect<RadioButtonGroup, Type>(nameof(Enum), o => o.Enum, (o, v) => o.Enum = v);
    public static readonly StyledProperty<object> SelectedItemProperty = AvaloniaProperty.Register<RadioButtonGroup, object>(nameof(SelectedItem));

    public static readonly DirectProperty<RadioButtonGroup, RelayCommand<Button>> ItemClickCommandProperty = AvaloniaProperty.RegisterDirect<RadioButtonGroup, RelayCommand<Button>>(nameof(ItemClickCommand), o => o.ItemClickCommand, (o, v) => o.ItemClickCommand = v);

    private Type @enum;
    private RelayCommand<Button> itemClickCommand;

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

    public RelayCommand<Button> ItemClickCommand
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
        itemClickCommand = new RelayCommand<Button>(param => SelectedItem = param.Tag);
    }

    protected override Type StyleKeyOverride { get; } = typeof(ItemsControl);
}
