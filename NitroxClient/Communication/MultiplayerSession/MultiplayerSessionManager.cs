using System;
using System.Collections.Generic;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.MultiplayerSession.ConnectionState;
using NitroxModel.Logger;
using NitroxModel.MultiplayerSession;
using NitroxModel.Packets;

namespace NitroxClient.Communication.MultiplayerSession
{
    public class MultiplayerSessionManager : IMultiplayerSession, IMultiplayerSessionConnectionContext
    {
        private readonly HashSet<Type> suppressedPacketsTypes = new HashSet<Type>();
        private readonly IClient client;
        private string ipAddress;
        private MultiplayerSessionPolicy sessionPolicy;
        private PlayerSettings playerSettings;
        private AuthenticationContext authenticationContext;

        IClient IMultiplayerSessionState.Client => client;
        string IMultiplayerSessionState.IpAddress => ipAddress;
        MultiplayerSessionPolicy IMultiplayerSessionState.SessionPolicy => sessionPolicy;
        PlayerSettings IMultiplayerSessionState.PlayerSettings => playerSettings;
        AuthenticationContext IMultiplayerSessionState.AuthenticationContext => authenticationContext;

        public event MultiplayerSessionConnectionStateChangedEventHandler ConnectionStateChanged;
        public IMultiplayerSessionConnectionState CurrentState { get; private set; }
        public MultiplayerSessionReservation Reservation { get; private set; }

        public MultiplayerSessionManager(IClient client)
        {
            Log.Info("Initializing MultiplayerSessionManager...");
            this.client = client;
            CurrentState = new Disconnected();
        }

        // Testing entry point
        internal MultiplayerSessionManager(IClient client, IMultiplayerSessionConnectionState initialState)
        {
            this.client = client;
            CurrentState = initialState;
        }

        public void Connect(string ipAddress)
        {
            this.ipAddress = ipAddress;
            CurrentState.NegotiateReservation(this);
        }

        public void ProcessSessionPolicy(MultiplayerSessionPolicy policy)
        {
            sessionPolicy = policy;
            CurrentState.NegotiateReservation(this);
        }

        public void RequestSessionReservation(PlayerSettings playerSettings, AuthenticationContext authenticationContext)
        {
            this.playerSettings = playerSettings;
            this.authenticationContext = authenticationContext;
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
            CurrentState.Disconnect(this);
        }

        public void Send(Packet packet)
        {
            if (!suppressedPacketsTypes.Contains(packet.GetType()))
            {
                client.Send(packet);
            }
        }

        public PacketSuppressor<T> Suppress<T>()
        {
            return new PacketSuppressor<T>(suppressedPacketsTypes);
        }

        public void UpdateConnectionState(IMultiplayerSessionConnectionState sessionConnectionState)
        {
            CurrentState = sessionConnectionState;
            ConnectionStateChanged?.Invoke(CurrentState);
        }

        public void ClearSessionState()
        {
            ipAddress = null;
            sessionPolicy = null;
            playerSettings = null;
            authenticationContext = null;
            Reservation = null;
        }
    }
}
