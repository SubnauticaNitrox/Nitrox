using System;
using Avalonia.Controls;
using Avalonia.Input;
using Nitrox.Launcher.ViewModels;
using Nitrox.Launcher.Views.Abstract;

namespace Nitrox.Launcher.Views;

public partial class ManageServerView : RoutableViewBase<ManageServerViewModel>
{
    public ManageServerView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }
    
    private void InputElement_OnKeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Up:
            case Key.Down:
                if (sender is TextBox textBox)
                {
                    string previousText = textBox.Text;
                    if (int.TryParse(textBox.Text, out int val))
                    {
                        val += e.Key == Key.Up ? 1 : -1;
                    }
                    textBox.Text = Math.Clamp(val, 0, int.MaxValue).ToString();
                    if (textBox.Text.Length > textBox.MaxLength)
                    {
                        textBox.Text = previousText;
                    }
                }
                break;
        }
    }

    private void ScrollViewer_OnPointerWheelChanged(object sender, PointerWheelEventArgs e)
    {
        // Change scroll wheel input to move scroll viewer left and right
        ScrollViewer scrollViewer = (ScrollViewer)sender;
        if (e.Delta.Y < 0)
        {
            scrollViewer.LineRight();
        }
        else
        {
            scrollViewer.LineLeft();
        }
        e.Handled = true;
    }
    
    private void OnLoaded(object sender, EventArgs e)
    {
        mainScrollViewer.ScrollToHome();
    }
}
