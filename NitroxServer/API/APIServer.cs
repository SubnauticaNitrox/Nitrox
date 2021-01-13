using System.Net;
using Grapevine;

namespace NitroxServer.API
{
    /**
     * <summary>
     * The central method for all APIServers, always runs on an ajacent port to the Nitrox Server allowing multiple instances to be live.
     * It can be disabled in-game or in-console using the /api command
     * </summary>
     */
    public class APIServer
    {
        public int Port;
        public static APIServer Instance { get; private set; }
        private readonly IRestServer rawServer;

        public APIServer()
        {
            Port = 3000;
            rawServer = RestServerBuilder.UseDefaults().Build();
            rawServer.UseCorsPolicy();
            rawServer.Prefixes.Clear();
            rawServer.Prefixes.Add($"http://localhost:{Port}/");
            WebHeaderCollection defaultHeaders = new WebHeaderCollection
            {
                { "Content-Type", "application/json" }
            };
            rawServer.ApplyGlobalResponseHeaders(defaultHeaders);
            Instance = this;
            rawServer.Start();
        }

        public void Stop()
        {
            rawServer.Stop();
        }
    }
}
