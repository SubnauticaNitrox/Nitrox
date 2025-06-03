using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Nitrox.Launcher.ViewModels;
using Nitrox.Launcher.Views.Abstract;
using NitroxModel.Logger;

namespace Nitrox.Launcher.Views;

public partial class TroubleshootingView : RoutableViewBase<TroubleshootingViewModel>
{
    private UdpClient server;
    private static readonly IPEndPoint port = new(IPAddress.Any, 11000);
    private CancellationTokenSource canceller = new();
    private readonly Button portForwardCheckBtn;
    private readonly TextBlock portForwardStatusText;
    private readonly Ellipse portForwardStatusCircle;
    private static readonly IBrush yellowBrush = new SolidColorBrush(Colors.Yellow);
    private static readonly IBrush redBrush = new SolidColorBrush(Colors.Red);
    private static readonly IBrush greenBrush = new SolidColorBrush(Colors.Green);

    public TroubleshootingView()
    {
        InitializeComponent();
        portForwardCheckBtn = this.FindControl<Button>("PortForwardCheckButton");
        portForwardStatusText = this.FindControl<TextBlock>("PortForwardStatusText");
        portForwardStatusCircle = this.FindControl<Ellipse>("PortForwardStatusCircle");
    }

    private void CleanupServer()
    {
        server?.Dispose();
        server = null;
    }

    public void EnablePortChecking(object? sender, RoutedEventArgs e)
    {
        server = new UdpClient(port);
        Task.Run(() =>
        {
            // WTF: this isn't running after it is cancelled once.
            IPEndPoint clientEndpoint = new IPEndPoint(IPAddress.Any, 0);
            try
            {
                byte[] bytesin = server.Receive(ref clientEndpoint);
            }
            catch (Exception) {
                Dispatcher.UIThread.Post(() => {
                    portForwardCheckBtn.Classes.Clear();
                    portForwardCheckBtn.Classes.Add("primary");
                    portForwardCheckBtn.Content = "Start";
                    portForwardStatusText.Text = "Failed";
                    portForwardStatusText.Foreground = redBrush;
                    portForwardStatusCircle.Fill = redBrush;
                });
                CleanupServer();
                return; 
            }
            CleanupServer();
            Dispatcher.UIThread.Post(() =>
            {
                portForwardCheckBtn.Classes.Clear();
                portForwardCheckBtn.Classes.Add("primary");
                portForwardCheckBtn.Content = "Start";
                portForwardStatusCircle.Fill = greenBrush;
                portForwardStatusText.Text = "Succeeded";
                portForwardStatusText.Foreground = greenBrush;
            });
        }, canceller.Token);
    }

    public void CancelPortChecking(object? sender, RoutedEventArgs e)
    {
        canceller.Cancel();
        CleanupServer();
        canceller = new CancellationTokenSource();
    }

    public void OnPortForwardClick(object? sender, RoutedEventArgs e)
    {
        if (server == null)
        {
            portForwardCheckBtn.Classes.Clear();
            portForwardCheckBtn.Classes.Add("abort");
            portForwardCheckBtn.Content = "Stop";
            portForwardStatusText.Text = "Running...";
            portForwardStatusText.Foreground = yellowBrush;
            portForwardStatusCircle.Fill = yellowBrush;
            EnablePortChecking(sender, e);
        }
        else
        {
            CancelPortChecking(sender, e);
        }
    }
}
