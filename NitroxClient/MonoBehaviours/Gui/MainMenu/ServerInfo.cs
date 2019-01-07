using System;
using System.Net;
using System.Text.RegularExpressions;
using NitroxModel.Logger;
using UnityEngine;
using System.Threading;
using System.Linq;

namespace NitroxClient.MonoBehaviours.Gui.MainMenu
{
    public class ServerInfo : IComparable<ServerInfo>
    {
        private bool _isValid = true;     // lets stay positive unless we know its not
        public bool IsValid { get { return _isValid; } set { } }

        // simple parse
        public readonly string Name;      // title
        public readonly string Address;   // domain or ip. directly from serverStr

        // LAZY LOAD considering network latency
        public string Host { get { if(_host == null) LazyLoad(); return _host; } }
        public string Ip { get { if (_ip == null) LazyLoad(); return _ip; } }
        public int Port { get { if (_port == 0) LazyLoad(); return _port; } }
        public int Ping { get { if (_ping == -1) UpdateLatency(); return _ping; }  set { _ping = value; } }

        // Internals for getter/setter
        private string _host = null;   // only domain name. no port
        private string _ip = null;     // parsed ip address
        private int _port = 0;
        private int _ping = -1;        // do not use 0 here. because localhost = 0 and it is corrent


        public ServerInfo(string serverStr)
        {
            string[] lineData = serverStr.Split('|');
            if(lineData.Length == 1)
            {
                Name = "";
                Address = lineData[0].Trim();
            }
            else
            {
                Name = lineData[0];
                Address = lineData[1].Trim();
            }
        }

        public int CompareTo(ServerInfo other)
        {
            if(Ping.Equals(other.Ping))
            {
                return Name.CompareTo(other.Name);
            }
            else
            {
                return Ping.CompareTo(other.Ping);
            }
        }
        
        private void LazyLoad()
        {
            /* Possible Valid Address Formats:
             *      12.34.56.78
             *      12.34.56.78:12
             *      fe80::12:34
             *      [fe80::12:34]
             *      [fe80::12:34]:12
             *      sub.非英文字符.domain.com
             *      sub.非英文字符.domain.com:123
             * Solution:
             *      1. split to host and port
             *      2. check result of IPAddress.TryParse(host, out ipaddr)
             */
            if (!Address.Contains('.') && !Address.Contains(':'))
            {
                _isValid = false;
                _host = Address;    // fill them to avoid null ref if used elsewhere
                _ip = Address;
                return;
            }

            string tmpAddressStr = Address;

            // UrlBuilder throws exception on naked ipv6 ( address not qouted with [ ] )
            // so we qoute it before throwing it in UrlBuilder
            if (Address.Count(f => f == ':') > 1 && Regex.IsMatch(Address, @"^[0-9a-zA-Z:\.]+$")) // seems like naked ipv6
            {
                tmpAddressStr = $"[{tmpAddressStr}]";
            }

            // Now we use UrlBuilder to have everything parsed well
            try
            {
                UriBuilder uri = new UriBuilder("http://" + tmpAddressStr);

                _port = uri.Port == 80 ? 11000 : uri.Port;    // lets hope nobody really bind game server on 80

                IPAddress tmpIPAddr = null;
                if(IPAddress.TryParse(uri.Host, out tmpIPAddr))
                {
                    _ip = tmpIPAddr.ToString(); // shorten ip string
                    _host = _ip;
                }
                else
                {
                    _host = uri.Host;
                    _ip = Dns.GetHostEntry(_host).AddressList[0].ToString();
                }
                return;
            }
            catch (Exception e)
            {
                _isValid = false;
                if (_host == null) _host = Address;
                if (_ip   == null) _ip   = Address;
                Log.Error($"Failed to parse server {Address}. Error Message: {e.Message}");
            }
            
        }


        public void UpdateLatency()
        {
            Ping ping = null;
            try
            {
                ping = new Ping(Ip);
                while (!ping.isDone)
                {
                    Thread.Sleep(50);
                }
                _ping = ping.time;

            }
            catch (Exception)
            {
                _ping = 9999;
            }
        }
    }
    
}
