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
        public readonly IRestServer RawServer;

        public APIServer()
        {
            Port = 3000;
            RawServer = RestServerBuilder.UseDefaults().Build();
            RawServer.UseCorsPolicy();
            RawServer.Prefixes.Clear();
            RawServer.Prefixes.Add($"http://localhost:{Port}/");
            WebHeaderCollection defaultHeaders = new WebHeaderCollection
            {
                { "Content-Type", "application/json" }
            };
            RawServer.ApplyGlobalResponseHeaders(defaultHeaders);
            Instance = this;
        }

        public void Start()
        {
            RawServer.Start();
        }

        public void Stop()
        {
            RawServer.Stop();
        }
    }
}
