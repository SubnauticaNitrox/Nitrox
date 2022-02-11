using System;
using System.Collections.Generic;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.MultiplayerSession.ConnectionState;
using NitroxClient.GameLogic;
using NitroxModel;
using NitroxModel.Helper;
using NitroxModel.MultiplayerSession;
using NitroxModel.Packets;
using NitroxModel.Serialization;

namespace NitroxClient.Communication.MultiplayerSession
{
    public class MultiplayerSessionManager : IMultiplayerSession, IMultiplayerSessionConnectionContext
    {
        internal readonly HashSet<Type> suppressedPacketsTypes = new();

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

        public void Connect(string ipAddress, int port)
        {
            IpAddress = ipAddress;
            ServerPort = port;
            CurrentState.NegotiateReservation(this);
        }

        public void ProcessSessionPolicy(MultiplayerSessionPolicy policy)
        {
            SessionPolicy = policy;
            NitroxConsole.DisableConsole = SessionPolicy.DisableConsole;
            Version localVersion = typeof(Extensions).Assembly.GetName().Version;
            localVersion = new Version(localVersion.Major, localVersion.Minor);
            switch (localVersion.CompareTo(SessionPolicy.NitroxVersionAllowed))
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

            CurrentState.NegotiateReservation(this);
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
            CurrentState.NegotiateReservation(this);
        }

        public void ProcessReservationResponsePacket(MultiplayerSessionReservation reservation)
        {
            if (reservation.ReservationState == MultiplayerSessionReservationState.ENQUEUED_IN_JOIN_QUEUE)
            {
                Log.InGame(Language.main.Get("Nitrox_Waiting"));
                return;
            }

            Reservation = reservation;
            CurrentState.NegotiateReservation(this);
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

        public bool Send(Packet packet)
        {
            Type packetType = packet.GetType();
            if (!suppressedPacketsTypes.Contains(packetType))
            {
                Client.Send(packet);
                return true;
            }
            return false;
        }

        public bool IsPacketSuppressed(Type packetType) => suppressedPacketsTypes.Contains(packetType);

        public PacketSuppressor<T> Suppress<T>()
        {
            return new PacketSuppressor<T>(suppressedPacketsTypes);
        }

        public SoundPacketSuppressor SuppressSounds()
        {
            return new SoundPacketSuppressor(suppressedPacketsTypes);
        }

        public PacketUnsuppressor<T> Unsuppress<T>()
        {
            return new PacketUnsuppressor<T> (suppressedPacketsTypes);
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
