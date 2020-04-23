using System;
using System.Collections.Generic;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.MultiplayerSession.ConnectionState;
using NitroxClient.Debuggers;
using NitroxClient.GameLogic;
using NitroxModel;
using NitroxModel.Helper;
using NitroxModel.Logger;
using NitroxModel.MultiplayerSession;
using NitroxModel.Packets;

namespace NitroxClient.Communication.MultiplayerSession
{
    public class MultiplayerSessionManager : IMultiplayerSession, IMultiplayerSessionConnectionContext
    {
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
            PlayerSettings = playerSettings;
            AuthenticationContext = authenticationContext;
            CurrentState.NegotiateReservation(this);
        }

        public void ProcessReservationResponsePacket(MultiplayerSessionReservation reservation)
        {
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

        public PacketSuppressor<T> Suppress<T>()
        {
            return new PacketSuppressor<T>(suppressedPacketsTypes);
        }

        public void UpdateConnectionState(IMultiplayerSessionConnectionState sessionConnectionState)
        {
            Validate.NotNull(sessionConnectionState);

            string fromStage = CurrentState == null ? "null" : CurrentState.CurrentStage.ToString();
            string username = AuthenticationContext == null ? "" : AuthenticationContext.Username;
            Log.Info($"Updating session stage from '{fromStage}' to '{sessionConnectionState.CurrentStage}' for '{username}'");

            CurrentState = sessionConnectionState;

            // Last connection state changed will not have any handlers
            ConnectionStateChanged?.Invoke(CurrentState);
        }

        public void ClearSessionState()
        {
            IpAddress = null;
            ServerPort = 11000;
            SessionPolicy = null;
            PlayerSettings = null;
            AuthenticationContext = null;
            Reservation = null;
        }
    }
}
