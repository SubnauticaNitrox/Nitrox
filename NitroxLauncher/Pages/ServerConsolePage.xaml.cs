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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using NitroxLauncher.Events;
using LibZeroTier;
using System.Runtime.InteropServices;

namespace NitroxLauncher.Pages
{
    public partial class ServerConsolePage : PageBase, INotifyPropertyChanged
    {
        private readonly List<string> commandLinesHistory = new List<string>();
        public static bool IsServerRunning;
        private static bool isPrivateServer;
        private static string SERVER_DETAILS_PATH = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "private_server");
        private bool isEmbed;
        public bool IsEmbed
        {
            get => isEmbed;
            set
            {
                isEmbed = value;
                if (!isEmbed)
                {
                    Dispatcher?.Invoke(() =>
                    {
                        ConsoleButton.Visibility = Visibility.Hidden;
                        ServerInfo.Visibility = Visibility.Visible;
                        ConsolePage.Visibility = Visibility.Hidden;
                    });
                }
                Dispatcher?.Invoke(() => { ServerStartedInit(); });
            }
        }
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
        private void ServerStartedInit()
        {
            Task.Factory.StartNew(() =>
            {
                while (!File.Exists(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "server.cfg"))){ }
                Dispatcher.Invoke(() =>
                {
                    ServerSettings settings = ServerSettings.ReadConfigFile();
                    ServerId.Text = "";
                    Connectivity.Text = "";
                    Port.Text = settings.port.ToString();
                    IpAddress.Text = settings.ip_address?.ToString();
                    ServerName.Text = settings.server_name;
                    ServerPassword.Text = settings.server_password;
                    AdminPassword.Text = settings.admin_password;
                    GameMode.SelectedIndex = (int)settings.gamemode.Value;
                    if (settings.ip_address == null)
                    {
                        Task.Factory.StartNew(() =>
                        {
                            while (ServerSettings.GetLocalIPAddress() == null) { Task.Delay(250).Wait(); }
                            Dispatcher.Invoke(() =>
                            {
                                isPrivateServer = File.ReadAllLines(SERVER_DETAILS_PATH)?[1] == "Activated";
                                IpAddress.Text = ServerSettings.GetLocalIPAddress().ToString();
                                if (isPrivateServer)
                                    ServerId.Text = File.ReadAllLines(SERVER_DETAILS_PATH)[0] + IpAddress.Text.Split('.')[IpAddress.Text.Split('.').Length - 1];
                                else
                                {
                                    string externalip = new WebClient().DownloadString("http://icanhazip.com");
                                    ServerId.Text = "Local IP :  " + IpAddress.Text + "                         Public IP :  " + externalip;
                                }
                            });
                        });
                        Task.Factory.StartNew(() =>
                        {
                            IsServerRunning = LauncherLogic.Instance.ServerRunning;
                            while (IsServerRunning)
                            {
                                isPrivateServer = File.ReadAllLines(SERVER_DETAILS_PATH)?[1] == "Activated";
                                int average_ping = Ping_all();
                                Dispatcher.Invoke(() =>
                                {
                                    Connectivity.Text = average_ping.ToString() + " ms";
                                    switch (average_ping)
                                    {
                                        case <= 50:
                                            SignalBar1.Fill = new SolidColorBrush() { Color = Colors.LimeGreen };
                                            SignalBar2.Fill = new SolidColorBrush() { Color = Colors.LimeGreen };
                                            SignalBar3.Fill = new SolidColorBrush() { Color = Colors.LimeGreen };
                                            SignalBar4.Fill = new SolidColorBrush() { Color = Colors.LimeGreen };
                                            SignalBar5.Fill = new SolidColorBrush() { Color = Colors.LimeGreen };
                                            break;
                                        case <= 100:
                                            SignalBar1.Fill = new SolidColorBrush() { Color = Colors.Lime };
                                            SignalBar2.Fill = new SolidColorBrush() { Color = Colors.Lime };
                                            SignalBar3.Fill = new SolidColorBrush() { Color = Colors.Lime };
                                            SignalBar4.Fill = new SolidColorBrush() { Color = Colors.Lime };
                                            SignalBar5.Fill = new SolidColorBrush() { Color = Colors.Gray };
                                            break;
                                        case <= 150:
                                            SignalBar1.Fill = new SolidColorBrush() { Color = Colors.Yellow };
                                            SignalBar2.Fill = new SolidColorBrush() { Color = Colors.Yellow };
                                            SignalBar3.Fill = new SolidColorBrush() { Color = Colors.Yellow };
                                            SignalBar4.Fill = new SolidColorBrush() { Color = Colors.Gray };
                                            SignalBar5.Fill = new SolidColorBrush() { Color = Colors.Gray };
                                            break;
                                        case <= 300:
                                            SignalBar1.Fill = new SolidColorBrush() { Color = Colors.Orange };
                                            SignalBar2.Fill = new SolidColorBrush() { Color = Colors.Orange };
                                            SignalBar3.Fill = new SolidColorBrush() { Color = Colors.Gray };
                                            SignalBar4.Fill = new SolidColorBrush() { Color = Colors.Gray };
                                            SignalBar5.Fill = new SolidColorBrush() { Color = Colors.Gray };
                                            break;
                                        default:
                                            SignalBar1.Fill = new SolidColorBrush() { Color = Colors.Red };
                                            SignalBar2.Fill = new SolidColorBrush() { Color = Colors.Gray };
                                            SignalBar3.Fill = new SolidColorBrush() { Color = Colors.Gray };
                                            SignalBar4.Fill = new SolidColorBrush() { Color = Colors.Gray };
                                            SignalBar5.Fill = new SolidColorBrush() { Color = Colors.Gray };
                                            break;
                                    }
                                });
                                if (!IsServerRunning)
                                    return;
                                Task.Delay(500).Wait();
                            }
                        });
                    }
                });
            });
        }
        private List<KeyValuePair<string, int>> listOfPeers = new List<KeyValuePair<string, int>>();
        private ZeroTierAPI privateNet = new ZeroTierAPI(new API_Settings() { Web_API_Key = "AOVr7MaXugibaWm8kmRXOegCH84NBRnv" }, null);
        public int Ping_all()
        {
            if(File.Exists(SERVER_DETAILS_PATH))
            {
                if (isPrivateServer)
                {
                    //Extracting and pinging all other ip's.
                    foreach (var i in privateNet.GetPeers(File.ReadAllLines(SERVER_DETAILS_PATH)[0]))
                        if (i.Value)
                            Ping(i.Key.ToString(), 1, 4000).Wait();
                    if (listOfPeers.Count > 0)
                        return new Func<int>(() => { int total_ping = 0; foreach (KeyValuePair<string, int> item in listOfPeers) { total_ping += item.Value; } return total_ping; })() / listOfPeers.Count;
                    else
                        return 0;
                }
                else
                {
                    IPAddress gate_way = GetGatewayForDestination(ServerSettings.GetLocalIPAddress());
                    string MasterIp = gate_way.MapToIPv4().ToString();
                    string[] IpArray = MasterIp.Split('.');
                    string Ip = string.Join(".", IpArray[0], IpArray[1], IpArray[2]);
                    for (var i = 2; i < 255; i++)
                        Ping(Ip + "." + i.ToString(), 1, 4000).Wait();
                    return 0;
                }
            }
            return 0;
        }

        [DllImport("iphlpapi.dll", CharSet = CharSet.Auto)]
        private static extern int GetBestInterface(UInt32 destAddr, out UInt32 bestIfIndex);

        public static IPAddress GetGatewayForDestination(IPAddress destinationAddress)
        {
            UInt32 destaddr = BitConverter.ToUInt32(destinationAddress.GetAddressBytes(), 0);

            uint interfaceIndex;
            int result = GetBestInterface(destaddr, out interfaceIndex);
            if (result != 0)
                throw new Win32Exception(result);

            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                var niprops = ni.GetIPProperties();
                if (niprops == null)
                    continue;

                var gateway = niprops.GatewayAddresses?.FirstOrDefault()?.Address;
                if (gateway == null)
                    continue;

                if (ni.Supports(NetworkInterfaceComponent.IPv4))
                {
                    var v4props = niprops.GetIPv4Properties();
                    if (v4props == null)
                        continue;

                    if (v4props.Index == interfaceIndex)
                        return gateway;
                }

                if (ni.Supports(NetworkInterfaceComponent.IPv6))
                {
                    var v6props = niprops.GetIPv6Properties();
                    if (v6props == null)
                        continue;

                    if (v6props.Index == interfaceIndex)
                        return gateway;
                }
            }

            return null;
        }
        public async Task Ping(string host, int attempts, int timeout)
        {
            for (int i = 0; i < attempts; i++)
            {
                await Task.Factory.StartNew(() =>
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
        private List<object> tmpList = new List<object>();
        private void PingCompleted(object sender, PingCompletedEventArgs e)
        {
            if (isPrivateServer || (e.Reply?.Status == IPStatus.Success))
            {
                string ip = (string)e.UserState;
                int ping = (int)e.Reply.RoundtripTime;
                Dispatcher.Invoke(() =>
                {

                if (e.Reply.Status != IPStatus.Success)
                    ping = 11010;
                int CurrentIndex = IndexOfKey(listOfPeers, ip);
                if (CurrentIndex >= 0)
                {
                    listOfPeers[CurrentIndex] = new KeyValuePair<string, int>(ip, ping);
                }
                else
                {
                    listOfPeers.Add(new KeyValuePair<string, int>(ip, ping));
                }
                    if (!isPrivateServer)
                    {
                        tmpList.Add(formatIp(ip) + Space + ping + " ms");
                        if (ip.EndsWith("254"))
                        {
                            PlayerList.Items.Clear();
                            foreach (object obj in tmpList)
                                PlayerList.Items.Add(obj);
                            tmpList.Clear();
                        }
                    }
                    else
                    {
                        string content = formatIp(ip) + Space + ping + " ms";
                        int len = PlayerList.Items.Count;
                        for (var i = 0; i < len; i++)
                            if (PlayerList.Items[i].ToString().Contains(ip))
                                PlayerList.Items[PlayerList.Items.IndexOf(PlayerList.Items[i])] = content;
                        if (!PlayerList.Items.Contains(content))
                            PlayerList.Items.Add(content);
                    }
                });
            }
        }
        public string formatIp(string ip)
        {
            while (ip.Length < 14)
                ip += " ";
            return ip;
        }
        public string Space => "                      ";
        public int IndexOfKey(List<KeyValuePair<string, int>> list, string key)
        {
            int index = 0;
            foreach(var i in list)
            {
                if (i.Key == key)
                    return index;
                index++;
            }
            return -1;
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
            foreach (IPAddress ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    if(ip.MapToIPv4().ToString().Contains("10.10.10."))
                        return ip.MapToIPv4();
            foreach (var netI in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (netI.NetworkInterfaceType != NetworkInterfaceType.Wireless80211 && (netI.NetworkInterfaceType != NetworkInterfaceType.Ethernet || netI.OperationalStatus != OperationalStatus.Up))
                    continue;
                foreach (var uniIpAddrInfo in netI.GetIPProperties().UnicastAddresses.Where(x => netI.GetIPProperties().GatewayAddresses.Count > 0))
                    if (uniIpAddrInfo.Address.AddressFamily == AddressFamily.InterNetwork && uniIpAddrInfo.AddressPreferredLifetime != uint.MaxValue)
                        return uniIpAddrInfo.Address.MapToIPv4();
            }
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
