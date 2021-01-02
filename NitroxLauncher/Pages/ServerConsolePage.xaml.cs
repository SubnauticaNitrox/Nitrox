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
        public static bool isPrivateServer;
        public static string SERVER_DETAILS_PATH = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "private_server");
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
                            IsServerRunning = LauncherLogic.Instance.ServerRunning;
                            while (IsServerRunning)
                            {
                                int average_ping = Ping_all();
                                string ip = GetLocalIPAddress().ToString();
                                string serverid = File.ReadAllLines(SERVER_DETAILS_PATH)[0] + ip.Split('.')[ip.Split('.').Length - 1];
                                string externalip = "Local IP :  " + ip + "                         Public IP :  " + new WebClient().DownloadString("http://icanhazip.com");
                                string conectivity = average_ping.ToString() + " ms";
                                Dispatcher.Invoke(() =>
                                {
                                    IpAddress.Text = ip;
                                    if (isPrivateServer)
                                        ServerId.Text = serverid;
                                    else
                                    {
                                        ServerId.Text = externalip;
                                    }
                                    Connectivity.Text = conectivity;
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
        public static ZeroTierAPI PrivateNet = new ZeroTierAPI(new API_Settings() { Web_API_Key = "AOVr7MaXugibaWm8kmRXOegCH84NBRnv" }, null);
        public int Ping_all()
        {
            if (isPrivateServer)
            {
                //Extracting and pinging all other ip's.
                foreach (var i in PrivateNet.GetPeers(File.ReadAllLines(SERVER_DETAILS_PATH)[0]))
                    if (i.Value)
                        Ping(i.Key.ToString(), 1, 2000).Wait();
                if (listOfPeers.Count > 0)
                    return new Func<int>(() => { int total_ping = 0; foreach (KeyValuePair<string, int> item in listOfPeers) { total_ping += item.Value; } return total_ping; })() / listOfPeers.Count;
                else
                    return 0;
            }
            else
            {
                IPAddress gate_way = GetGatewayForDestination(GetLocalIPAddress()).Key;
                string MasterIp = gate_way.MapToIPv4().ToString();
                string[] IpArray = MasterIp.Split('.');
                string Ip = string.Join(".", IpArray[0], IpArray[1], IpArray[2]);
                for (var i = 2; i < 255; i++)
                {
                    Ping(Ip + "." + i.ToString(), 1, 1000).Wait();
                }
                int failCount = 0;
                while (ResultsCounted != 0 && failCount < 10) { if (ResultsCounted == -1 || ResultsCounted == -2) { failCount++; } else { failCount = 0; } }
                ResultsCounted = 0;
                Dispatcher.Invoke(() =>
                {
                    PlayerList.Items.Clear();
                    foreach (object obj in tmpList)
                        PlayerList.Items.Add(obj);
                    tmpList.Clear();
                });
                return new Func<int>(() => { int total_ping = 0; foreach (KeyValuePair<string, int> item in listOfPeers) { total_ping += item.Value; } return total_ping; })() / listOfPeers.Count;
            }
        }
        private int ResultsCounted = 0;
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
                        ResultsCounted -= 1;
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
        private int LastNumber = 0;
        private void PingCompleted(object sender, PingCompletedEventArgs e)
        {
            ResultsCounted += 1;
            string ip = (string)e.UserState;
            if (isPrivateServer || (e.Reply?.Status == IPStatus.Success))
            {
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
                        tmpList.Add(formatIp(ip, ping, "ms"));
                        LastNumber = int.Parse(ip.Split('.')[ip.Split('.').Length - 1]);
                    }
                    else
                    {
                        string content = formatIp(ip, ping, "ms");
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
        public string formatIp(string ip, int ping, string ms)
        {
            while(ip.Length < 50)
                ip += " ";
            ip += ping.ToString() + " " + ms;
            Debug.WriteLine(ip);
            return ip;
        }
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
        public static IPAddress GetLocalIPAddress()
        {
            if (isPrivateServer)
            {
                return GetGatewayForDestination(PrivateNet.GetPeers(File.ReadAllLines(SERVER_DETAILS_PATH)[0])[0].Key).Value;
            }
            else
            {
                foreach (var netI in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (netI.NetworkInterfaceType != NetworkInterfaceType.Wireless80211 && (netI.NetworkInterfaceType != NetworkInterfaceType.Ethernet || netI.OperationalStatus != OperationalStatus.Up))
                        continue;
                    foreach (var uniIpAddrInfo in netI.GetIPProperties().UnicastAddresses.Where(x => netI.GetIPProperties().GatewayAddresses.Count > 0))
                        if (uniIpAddrInfo.Address.AddressFamily == AddressFamily.InterNetwork && uniIpAddrInfo.AddressPreferredLifetime != uint.MaxValue)
                            return uniIpAddrInfo.Address.MapToIPv4();
                }
            }
            return null;
        }
        [DllImport("iphlpapi.dll", CharSet = CharSet.Auto)]
        private static extern int GetBestInterface(UInt32 destAddr, out UInt32 bestIfIndex);

        public static KeyValuePair<IPAddress, IPAddress> GetGatewayForDestination(IPAddress destinationAddress)
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
                        return new KeyValuePair<IPAddress, IPAddress>(gateway, ni.GetIPProperties().UnicastAddresses.FirstOrDefault(ip => ip.Address.AddressFamily == AddressFamily.InterNetwork).Address.MapToIPv4());
                }

                if (ni.Supports(NetworkInterfaceComponent.IPv6))
                {
                    var v6props = niprops.GetIPv6Properties();
                    if (v6props == null)
                        continue;

                    if (v6props.Index == interfaceIndex)
                        return new KeyValuePair<IPAddress, IPAddress>(null, null);
                }
            }

            return new KeyValuePair<IPAddress, IPAddress>(null, null);
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
