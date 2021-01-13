using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grapevine;
using Newtonsoft.Json;
using NitroxModel.MultiplayerSession;
using NitroxPublic.API.CommonResponses;
using NitroxServer.Serialization;

namespace NitroxServer.PublicAPI.Routes
{
    [RestResource]
    public class ServerRelated
    {
        #region Server Status
        [RestRoute("Get", "/server/status")]
        public async Task getServerStatus(IHttpContext ctx)
        {
            ServerStatusResponse response = new ServerStatusResponse();
            Server srv = Server.Instance;

            if (!srv.IsRunning)
            {
                response.Status = "OFFLINE";
            }
            else
            {
                response.Status = "ONLINE";
                response.PlayersOnline = srv.GetNitroxServer().GetPlayerCount();
                response.MaxPlayers = srv.GetServerConfig().MaxConnections;
            }
                

            await ctx.Response.SendResponseAsync(JsonConvert.SerializeObject(response, Formatting.Indented));
        }
        [RestRoute("Get", "/server/ping")]
        public async Task pingServer(IHttpContext ctx)
        {
            if(Server.Instance.IsRunning)
            {
                ctx.Response.StatusCode = 200;
                await ctx.Response.SendResponseAsync("{}");
            } else
            {
                ctx.Response.StatusCode = 400;
                await ctx.Response.SendResponseAsync("{}");
            }
        }
        #endregion
        #region Server Control

        [RestRoute("Delete", "/server/stop")]
        public async Task stopServer(IHttpContext ctx)
        {
            Server srv = Server.Instance;
            if(ctx.Request.Headers.Get("Auth") != srv.GetServerConfig().AdminPassword)
            {
                ctx.Response.StatusCode = 401;
                await ctx.Response.SendResponseAsync(JsonConvert.SerializeObject(new UnauthorizedResponse("Please pass your ADMIN PASSWORD through the Auth header to access this endpoint."), Formatting.Indented));
            } else
            {
                await ctx.Response.SendResponseAsync(JsonConvert.SerializeObject(new ServerStoppedResponse(), Formatting.Indented));
                srv.Stop();

            }
        }
        [RestRoute("POST", "/server/pause")]
        public async Task pauseServer(IHttpContext ctx)
        {
            Server srv = Server.Instance;
            if (ctx.Request.Headers.Get("Auth") != srv.GetServerConfig().AdminPassword)
            {
                ctx.Response.StatusCode = 401;
                await ctx.Response.SendResponseAsync(JsonConvert.SerializeObject(new UnauthorizedResponse("Please pass your ADMIN PASSWORD through the Auth header to access this endpoint."), Formatting.Indented));
            }
            else
            {
                if (srv.Paused)
                {
                    ctx.Response.StatusCode = 500;
                    await ctx.Response.SendResponseAsync(JsonConvert.SerializeObject(new ServerAlreadyResponse("The server is already paused!"), Formatting.Indented));
                    return;
                }
                await ctx.Response.SendResponseAsync(JsonConvert.SerializeObject(new ServerPausedResponse(), Formatting.Indented));
                srv.PauseServer();
            }
        }
        [RestRoute("POST", "/server/resume")]
        public async Task resumeServer(IHttpContext ctx)
        {
            Server srv = Server.Instance;
            if (ctx.Request.Headers.Get("Auth") != srv.GetServerConfig().AdminPassword)
            {
                ctx.Response.StatusCode = 401;
                await ctx.Response.SendResponseAsync(JsonConvert.SerializeObject(new UnauthorizedResponse("Please pass your ADMIN PASSWORD through the Auth header to access this endpoint."), Formatting.Indented));
            }
            else
            {
                if (!srv.Paused)
                {
                    ctx.Response.StatusCode = 500;
                    await ctx.Response.SendResponseAsync(JsonConvert.SerializeObject(new ServerAlreadyResponse("The server isn't paused!"), Formatting.Indented));
                    return;
                }
                await ctx.Response.SendResponseAsync(JsonConvert.SerializeObject(new ServerResumedResponse(), Formatting.Indented));
                srv.ResumeServer();
            }
        }
        #endregion
        #region Player Management
        [RestRoute("Get", "/server/players")]
        public async Task getPlayers(IHttpContext ctx)
        {
            Server srv = Server.Instance;
            if (ctx.Request.Headers.Get("Auth") != srv.GetServerConfig().AdminPassword)
            {
                ctx.Response.StatusCode = 401;
                await ctx.Response.SendResponseAsync(JsonConvert.SerializeObject(new UnauthorizedResponse("Please pass your ADMIN PASSWORD through the Auth header to access this endpoint."), Formatting.Indented));
            }
            else
            {
                IEnumerable<Player> players = srv.GetNitroxServer().GetPlayerManager().GetAllPlayers();
                List<PlayerContext> playerInfos = new List<PlayerContext>();
                foreach (Player player in players)
                {
                    playerInfos.Add(player.PlayerContext);
                }
                await ctx.Response.SendResponseAsync(JsonConvert.SerializeObject(playerInfos, Formatting.Indented));
            }
        }
        [RestRoute("Get", "/server/players/count")]
        public async Task getPlayerCount(IHttpContext ctx)
        {
            Server srv = Server.Instance;
            PlayerCountResponse res = new PlayerCountResponse();
            res.Count = srv.GetNitroxServer().GetPlayerManager().GetAllPlayers().Count();
            res.Max = srv.GetServerConfig().MaxConnections;
            await ctx.Response.SendResponseAsync(JsonConvert.SerializeObject(res, Formatting.Indented));
        }
        #endregion
    }
}

