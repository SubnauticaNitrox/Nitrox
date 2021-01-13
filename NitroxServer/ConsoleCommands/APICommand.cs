using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using NitroxModel.DataStructures.GameLogic;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.ConsoleCommands.Abstract.Type;

namespace NitroxServer.ConsoleCommands
{
    public class APICommand : Command
    {
        private string GetIPAddress()
        {
            string address = "";
            WebRequest request = WebRequest.Create("http://checkip.dyndns.org/");
            using (WebResponse response = request.GetResponse())
            using (StreamReader stream = new StreamReader(response.GetResponseStream()))
            {
                address = stream.ReadToEnd();
            }

            int first = address.IndexOf("Address: ") + 9;
            int last = address.LastIndexOf("</body>");
            address = address.Substring(first, last - first);

            return address;
        }

        public APICommand() : base("api", Perms.ADMIN, "Manage the NPAPI connected to this server. Valid actions: start, stop, enable, disable")
        {
            AddParameter(new TypeString("action", true));
        }
        protected override void Execute(CallArgs args)
        {
            string action = args.Get(0);
            switch (action)
            {
                case "start":
                    if (Server.Instance.API.RawServer.IsListening)
                    {
                        SendMessage(args.Sender, "The NAPI is already active and listening!");
                    } else
                    {
                        Server.Instance.API.Start();
                        SendMessage(args.Sender, $"The NAPI is now listening on http://localhost:3000/, you can forward the port TCP 3000 manually if you want outside connections to be able to use NPAPI");
                    }
                    break;
                case "stop":
                    if (!Server.Instance.API.RawServer.IsListening)
                    {
                        SendMessage(args.Sender, "The NAPI is already inactive!");
                        break;
                    } else
                    {
                        Server.Instance.API.Stop();
                        SendMessage(args.Sender, $"The NAPI is now inactive and has stopped listening.");
                    }
                    break;
                case "enable":
                    if(Server.Instance.GetServerConfig().EnablePublicAPI)
                    {
                        SendMessage(args.Sender, "NAPI is already enabled.");
                        break;
                    }
                    Server.Instance.GetServerConfig().EnablePublicAPI = true;
                    Server.Instance.Save();
                    SendMessage(args.Sender, $"The NAPI will now automatically start when the server is started. You can stop NAPI using /api stop");
                    break;
                case "disable":
                    if (!Server.Instance.GetServerConfig().EnablePublicAPI)
                    {
                        SendMessage(args.Sender, "NAPI is already disabled.");
                        break;
                    }
                    Server.Instance.GetServerConfig().EnablePublicAPI = false;
                    Server.Instance.Save();
                    SendMessage(args.Sender, $"The NAPI will not be started when the server is started. You can manually start NAPI using /api start");
                    break;
                default:
                    break;
            } 
        }
    }
}
