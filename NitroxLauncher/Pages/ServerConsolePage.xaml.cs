using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using NitroxLauncher.Events;

namespace NitroxLauncher.Pages
{
    public partial class ServerConsolePage : PageBase, INotifyPropertyChanged
    {
        private readonly List<string> commandLinesHistory = new List<string>();
        public static bool ServerWasStarted = false;
        private int commandHistoryIndex;
        private string commandInputText;
        private string serverOutput = string.Empty;

        public string ServerOutput
        {
            get => serverOutput;
            set
            {
                serverOutput = value;
                OnPropertyChanged();
                Dispatcher?.BeginInvoke(new Action(() => ConsoleWindowScrollView.ScrollToEnd()));
            }
        }

        public string CommandInputText
        {
            get => commandInputText;
            set
            {
                commandInputText = value;
                OnPropertyChanged();
            }
        }

        public int CommandHistoryIndex
        {
            get => commandHistoryIndex;
            set
            {
                commandHistoryIndex = Math.Min(Math.Max(value, 0), commandLinesHistory.Count);
                if (commandHistoryIndex >= commandLinesHistory.Count)
                {
                    // Out of bounds index means command history is disabled
                    CommandInputText = string.Empty;
                }
                else
                {
                    CommandInputText = commandLinesHistory[commandHistoryIndex];
                    // Move cursor at the end of the text
                    CommandInput.SelectionStart = CommandInputText.Length;
                    CommandInput.SelectionLength = 0;
                }

                OnPropertyChanged();
            }
        }
        public ServerConsolePage()
        {

            InitializeComponent();

            LauncherLogic.Instance.ServerStarted += ServerStarted;
            LauncherLogic.Instance.ServerDataReceived += ServerDataReceived;

            Loaded += (sender, args) =>
            {
                CommandInput.Focus();
            };

            ServerInfo.Visibility = Visibility.Hidden;
            ConsolePage.Visibility = Visibility.Visible;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     Raises this object's PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The property that has a new value.</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ServerStarted(object sender, ServerStartEventArgs e)
        {
            ServerOutput = string.Empty;
        }

        private void ServerDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (ServerWasStarted)
            {
                ServerStartedInit();
                ServerWasStarted = false;
            }
            // TODO: Change to virtualized textboxes per line.
            // This sucks for performance reasons. Every string concat in .NET will create a NEW string in memory.
            ServerOutput += e.Data + Environment.NewLine;
        }

        private async Task SendCommandInputToServerAsync()
        {
            await LauncherLogic.Instance.SendServerCommandAsync(CommandInputText);
            ServerOutput += CommandInputText + Environment.NewLine;

            // Deduplication of command history
            if (!string.IsNullOrWhiteSpace(CommandInputText) && CommandInputText != commandLinesHistory.LastOrDefault())
            {
                commandLinesHistory.Add(CommandInputText);
            }
            HideCommandHistory();
        }

        private void HideCommandHistory()
        {
            CommandHistoryIndex = commandLinesHistory.Count;
        }

        private async void CommandButton_OnClick(object sender, RoutedEventArgs e)
        {
            await SendCommandInputToServerAsync();
        }

        private async void StopButton_Click(object sender, RoutedEventArgs e)
        {
            // Suggest referencing NitroxServer.ConsoleCommands.ExitCommand.name, but the class is internal
            await LauncherLogic.Instance.SendServerCommandAsync("stop");
            ServerOutput += $"stop{Environment.NewLine}";

            commandLinesHistory.Add("stop");
            HideCommandHistory();
        }

        private async void CommandLine_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;

            switch (e.Key)
            {
                case Key.Enter:
                    await SendCommandInputToServerAsync();
                    break;
                case Key.Escape:
                    HideCommandHistory();
                    break;
                case Key.Up:
                    CommandHistoryIndex--;
                    break;
                case Key.Down:
                    CommandHistoryIndex++;
                    break;
                default:
                    e.Handled = false;
                    break;
            }
        }

        private void PART_VerticalScrollBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }

        private void ConsoleButton_Click(object sender, RoutedEventArgs e)
        {
            ServerInfo.Visibility = Visibility.Hidden;
            ConsolePage.Visibility = Visibility.Visible;
        }

        private void DetailsButton_Click(object sender, RoutedEventArgs e)
        {
            ServerInfo.Visibility = Visibility.Visible;
            ConsolePage.Visibility = Visibility.Hidden;
        }
        public static void ServerHasStarted()
        {
            ServerWasStarted = true;
        }
        private void ServerStartedInit()
        {
            Dispatcher.Invoke(() => 
            { 
                ServerSettings settings = ServerSettings.ReadConfigFile();
                ServerId.Text = "???";
                Connectivity.Text = "???";
                Port.Text = settings.port.ToString();
                IpAddress.Text = settings.ip_address?.ToString();
                ServerName.Text = settings.server_name;
                ServerPassword.Text = settings.server_password;
                AdminPassword.Text = settings.admin_password;
                GameMode.SelectedIndex = (int)settings.gamemode.Value;
                if (settings.ip_address == null)
                {
                    Task.Factory.StartNew(() => {
                        while(ServerSettings.GetLocalIPAddress() == null){ Task.Delay(100).Wait(); }
                        Dispatcher.Invoke(() =>
                        {
                            IpAddress.Text = ServerSettings.GetLocalIPAddress().ToString();
                            ServerId.Text = File.ReadAllText(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "PrivateServer.txt")) + IpAddress.Text.Split('.')[IpAddress.Text.Split('.').Length-1];
                        });
                    });
                    Task.Factory.StartNew(() => {
                        while (true)
                        {
                            int average_ping = Ping_all();
                            Dispatcher.Invoke(() =>
                            {
                                Connectivity.Text = average_ping.ToString() + " ms";
                            });
                            Task.Delay(500).Wait();
                        }
                    });
                }
            });
        }
        static string NetworkGateway()
        {
            string ip = "10.10.10.1";
            /*
            foreach (var I in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    if (ip.MapToIPv4().ToString().Contains("10.10.10."))
            */
            return ip;
        }
        public int TotalPing = 0;
        public int NumberOfPeers = 0;
        public int Ping_all()
        {
            TotalPing = 0;
            NumberOfPeers = 0;
            string gate_ip = NetworkGateway();
            if (gate_ip != null)
            {
                //Extracting and pinging all other ip's.
                string[] array = gate_ip.Split('.');

                for (int i = 2; i <= 255; i++)
                {
                    string ping_var = array[0] + "." + array[1] + "." + array[2] + "." + i;

                    //time in milliseconds           
                    Ping(ping_var, 1, 4000);

                }
                if (NumberOfPeers > 0)
                    return TotalPing / NumberOfPeers;
                else
                    return 0;
            }
            return 0;
        }

        public void Ping(string host, int attempts, int timeout)
        {
            for (int i = 0; i < attempts; i++)
            {
                Task.Factory.StartNew(() =>
                {
                    try
                    {
                        Ping ping = new Ping();
                        ping.PingCompleted += new PingCompletedEventHandler(PingCompleted);
                        ping.SendAsync(host, timeout, host);
                    }
                    catch
                    {
                        // Do nothing and let it try again until the attempts are exausted.
                        // Exceptions are thrown for normal ping failurs like address lookup
                        // failed.  For this reason we are supressing errors.
                    }
                });
            }
        }
        private void PingCompleted(object sender, PingCompletedEventArgs e)
        {
            string ip = (string)e.UserState;
            if (e.Reply != null && e.Reply.Status == IPStatus.Success)
            {
                Dispatcher.Invoke(() => 
                {
                    NumberOfPeers += 1;
                    TotalPing += (int)e.Reply.RoundtripTime;
                    bool hasUpdated = false;
                    List<object> copy = new List<object>();
                    foreach (var item in PlayerList.Items)
                    {
                        copy.Add(item);
                    }
                    foreach (var item in copy)
                    {
                        if (item.ToString().Contains(ip))
                        {
                            PlayerList.Items[copy.IndexOf(item)] = ip + "                      " + (int)e.Reply.RoundtripTime + " ms";
                            hasUpdated = true;
                        }
                    }
                    if(!hasUpdated)
                        PlayerList.Items.Add(ip + "                      " + (int)e.Reply.RoundtripTime + " ms");
                });
            }
            else
            {
                // MessageBox.Show(e.Reply.Status.ToString());
            }
        }
    }
    public class ServerSettings
    {
        public int? port { get; set; }
        public IPAddress? ip_address { get; set; }
        public string? server_name { get; set; }
        public string? server_password { get; set; }
        public string? admin_password { get; set; }
        public GameMode? gamemode { get; set; }
        public static IPAddress GetLocalIPAddress()
        {
            foreach (var ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    if(ip.MapToIPv4().ToString().Contains("10.10.10."))
                        return ip.MapToIPv4();
            return null;
        }
        public static ServerSettings ReadConfigFile()
        {
            ServerSettings settings = new ServerSettings();
            string[] Contents = File.ReadAllLines(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "server.cfg"));
            foreach (string l in Contents)
            {
                string line = l.Replace(" ", "");
                if (!line.StartsWith("#"))
                {
                    string var = line.Split('=')[0];
                    string val = line.Split('=')[1] ?? "";
                    GameMode gameMode = GameMode.None;
                    Enum.TryParse(val, out gameMode);
                    switch (var)
                    {
                        case "ServerPort":
                            if (settings.port == null)
                                settings.port = int.Parse(val);
                            break;
                        case "SaveName":
                            if (settings.server_name == null)
                                settings.server_name = val;
                            break;
                        case "ServerPassword":
                            if (settings.server_password == null)
                                settings.server_password = val;
                            break;
                        case "AdminPassword":
                            if (settings.admin_password == null)
                                settings.admin_password = val;
                            break;
                        case "GameMode":
                            if (settings.gamemode == null && gameMode != GameMode.None)
                                settings.gamemode = gameMode;
                            break;
                    }
                }
            }
            return settings;
        }
    }
    public enum GameMode
    {
        Survival = 0,
        Freedom = 1,
        Hardcore = 2,
        Creative = 3,
        None = 4
    }
}
