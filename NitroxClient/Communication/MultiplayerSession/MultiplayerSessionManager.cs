using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.MultiplayerSession.ConnectionState;
using NitroxClient.GameLogic;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using NitroxModel.MultiplayerSession;
using NitroxModel.Packets;
using NitroxModel.Serialization;

namespace NitroxClient.Communication.MultiplayerSession
{
    public class MultiplayerSessionManager : IMultiplayerSession, IMultiplayerSessionConnectionContext
    {
        private static readonly Task initSerializerTask;

        static MultiplayerSessionManager()
        {
            initSerializerTask = Task.Run(Packet.InitSerializer);
        }

        private readonly HashSet<Type> suppressedPacketsTypes = new HashSet<Type>();

        public IClient Client { get; }
        public string IpAddress { get; private set; }
        public int ServerPort { get; private set; }
        public MultiplayerSessionPolicy SessionPolicy { get; private set; }
        public PlayerSettings PlayerSettings { get; private set; }
        public AuthenticationContext AuthenticationContext { get; private set; }
        public IMultiplayerSessionConnectionState CurrentState { get; private set; }
        public MultiplayerSessionReservation Reservation { get; private set; }

        public MultiplayerSessionManager(IClient client)
        {
            Log.Info("Initializing MultiplayerSessionManager...");
            Client = client;
            CurrentState = new Disconnected();
        }

        // Testing entry point
        internal MultiplayerSessionManager(IClient client, IMultiplayerSessionConnectionState initialState)
        {
            Client = client;
            CurrentState = initialState;
        }

        public event MultiplayerSessionConnectionStateChangedEventHandler ConnectionStateChanged;

        public async Task ConnectAsync(string ipAddress, int port)
        {
            IpAddress = ipAddress;
            ServerPort = port;
            await initSerializerTask;
            await CurrentState.NegotiateReservationAsync(this);
        }

        public void ProcessSessionPolicy(MultiplayerSessionPolicy policy)
        {
            SessionPolicy = policy;
            NitroxConsole.DisableConsole = SessionPolicy.DisableConsole;
            Version localVersion = NitroxEnvironment.Version;
            NitroxVersion nitroxVersion = new(localVersion.Major, localVersion.Minor);
            switch (nitroxVersion.CompareTo(SessionPolicy.NitroxVersionAllowed))
            {
                case -1:
                    Log.InGame($"Your Nitrox installation is out of date. Server: {SessionPolicy.NitroxVersionAllowed}, Yours: {localVersion}.");
                    CurrentState.Disconnect(this);
                    return;
                case 1:
                    Log.InGame($"The server runs an older version of Nitrox. Ask the server admin to upgrade or downgrade your Nitrox installation. Server: {SessionPolicy.NitroxVersionAllowed}, Yours: {localVersion}.");
                    CurrentState.Disconnect(this);
                    return;
            }

            CurrentState.NegotiateReservationAsync(this);
        }

        public void RequestSessionReservation(PlayerSettings playerSettings, AuthenticationContext authenticationContext)
        {
            // If a reservation has already been sent (in which case the client is enqueued in the join queue)
            if (CurrentState.CurrentStage == MultiplayerSessionConnectionStage.AWAITING_SESSION_RESERVATION)
            {
                Log.InGame(Language.main.Get("Nitrox_Waiting"));
                return;
            }

            PlayerSettings = playerSettings;
            AuthenticationContext = authenticationContext;
            CurrentState.NegotiateReservationAsync(this);
        }

        public void ProcessReservationResponsePacket(MultiplayerSessionReservation reservation)
        {
            if (reservation.ReservationState == MultiplayerSessionReservationState.ENQUEUED_IN_JOIN_QUEUE)
            {
                Log.InGame(Language.main.Get("Nitrox_Waiting"));
                return;
            }

            Reservation = reservation;
            CurrentState.NegotiateReservationAsync(this);
        }

        public void JoinSession()
        {
            CurrentState.JoinSession(this);
        }

        public void Disconnect()
        {
            if (CurrentState.CurrentStage != MultiplayerSessionConnectionStage.DISCONNECTED)
            {
                CurrentState.Disconnect(this);
            }
        }

        public void Send<T>(T packet) where T : Packet
        {
            Client.Send(packet);
        }
        
        public bool SendIfGameCode<T>(T packet) where T : Packet
        {
            static bool HasOriginModAndSentByInjectedCode(in StackFrame[] frames)
            {
                // Root caller is last stack frame, so start from the end.
                int i = frames.Length - 1;
                // Ignore Unity interjecting itself into iterators.
                if (frames.ElementAtOrDefault(i)?.GetMethod().Name == "InvokeMoveNext" && frames.ElementAtOrDefault(i - 1)?.GetMethod().Name == "MoveNext")
                {
                    i -= 2;
                }
                if (!frames.ElementAtOrDefault(i)?.GetMethod().DeclaringType?.Assembly.GetName().Name.StartsWith("NitroxClient", StringComparison.OrdinalIgnoreCase) ?? false)
                {
                    return false;
                }
                // Call stack is started by Nitrox, check if packet was sent via injected code. If so, suppress packet.
                while (--i >= 0)
                {
                    if (frames[i].GetMethod().DeclaringType?.Name.EndsWith("_Patch") ?? false)
                    {
                        return true;
                    }
                }
                return false;
            }
            
            StackFrame[] frames = new StackTrace(1, false).GetFrames() ?? Array.Empty<StackFrame>();
            if (HasOriginModAndSentByInjectedCode(in frames))
            {
                return false;
            }
            if (suppressedPacketsTypes.Contains(typeof(T)))
            {
                return false;
            }
            
            Client.Send(packet);
            return true;
        }
        
        public bool IsPacketSuppressed(Type packetType) => suppressedPacketsTypes.Contains(packetType);

        public PacketSuppressor<T> Suppress<T>()
        {
            return new PacketSuppressor<T>(suppressedPacketsTypes);
        }

        public void UpdateConnectionState(IMultiplayerSessionConnectionState sessionConnectionState)
        {
            Validate.NotNull(sessionConnectionState);

            string fromStage = CurrentState == null ? "null" : CurrentState.CurrentStage.ToString();
            string username = AuthenticationContext == null ? "" : AuthenticationContext.Username;
            Log.Debug($"Updating session stage from '{fromStage}' to '{sessionConnectionState.CurrentStage}' for '{username}'");

            CurrentState = sessionConnectionState;

            // Last connection state changed will not have any handlers
            ConnectionStateChanged?.Invoke(CurrentState);

            if (sessionConnectionState.CurrentStage == MultiplayerSessionConnectionStage.SESSION_RESERVED)
            {
                Log.PlayerName = username;
            }
        }

        public void ClearSessionState()
        {
            IpAddress = null;
            ServerPort = ServerList.DEFAULT_PORT;
            SessionPolicy = null;
            PlayerSettings = null;
            AuthenticationContext = null;
            Reservation = null;
        }
    }
}
