using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using Nitrox.Model.Core;

namespace Nitrox.Server.Subnautica.Models.Communication;

/// <summary>
///     Assigns session ids to player connections. Each session id is unique for the current server instance.
///     A session id will be reused when connections are lost. It will take at least 10 minutes before a session id is
///     reused to prevent impersonation and other bugs.
/// </summary>
internal sealed class SessionManager(ILogger<SessionManager> logger)
{
    private readonly ILogger<SessionManager> logger = logger;
    private readonly List<(TimeSpan ReturnedTimeStamp, SessionId Id)> returnedSessionIds = [];
    private readonly Dictionary<IPEndPoint, SessionId> sessionIdByEndpoint = [];
    private readonly Lock sessionLock = new();
    private readonly Dictionary<SessionId, Session> sessions = [];
    private readonly Stopwatch time = Stopwatch.StartNew();

    /// <summary>
    ///     The next session id. Returns a prior id if <see cref="returnedSessionIds" /> are available.
    /// </summary>
    private SessionId NextSessionId
    {
        get
        {
            lock (sessionLock)
            {
                SessionId id = 0;
                if (returnedSessionIds.Count > 0 && (time.Elapsed - returnedSessionIds[0].ReturnedTimeStamp).TotalMinutes >= SessionId.DELAY_REUSE_MINUTES)
                {
                    id = returnedSessionIds[0].Id;
                    returnedSessionIds.RemoveAt(0);
                }

                if (id == 0)
                {
                    id = ++field;
                }
                Debug.Assert(id > 0);
                return id;
            }
        }
    }

    public Session GetOrCreateSession(IPEndPoint endPoint)
    {
        Session session;
        lock (sessionLock)
        {
            if (!sessionIdByEndpoint.TryGetValue(endPoint, out SessionId id))
            {
                SessionId nextSessionId = NextSessionId;
                if (sessionIdByEndpoint.TryAdd(endPoint, nextSessionId))
                {
                    session = new Session(nextSessionId, endPoint);
                    sessions.Add(nextSessionId, session);
                }
            }

            session = sessions[id];
        }
        logger.ZLogTrace($"Created session #{session.Id}");
        return session;
    }

    public bool DeleteSessionAsync(SessionId sessionId)
    {
        lock (sessionLock)
        {
            if (!sessions.Remove(sessionId, out Session session))
            {
                return false;
            }
            sessionIdByEndpoint.Remove(session.EndPoint);
            returnedSessionIds.Add((time.Elapsed, session.Id));
        }
        logger.ZLogTrace($"Deleted session #{sessionId}");
        return true;
    }

    public record Session(SessionId Id, IPEndPoint EndPoint);
}
