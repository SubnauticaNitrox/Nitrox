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
        private readonly INitroxLogger log;

        public MultiplayerSessionManager(INitroxLogger logger, IClient client)
        {
            log = logger;
            log.Info("Initializing MultiplayerSessionManager...");
            Client = client;
            CurrentState = new Disconnected();
        }

        // Testing entry point
        internal MultiplayerSessionManager(IClient client, IMultiplayerSessionConnectionState initialState)
        {
            log = NoLogger.Default;
            Client = client;
            CurrentState = initialState;
        }

        public IClient Client { get; }
        public string IpAddress { get; private set; }
        public MultiplayerSessionPolicy SessionPolicy { get; private set; }
        public PlayerSettings PlayerSettings { get; private set; }
        public AuthenticationContext AuthenticationContext { get; private set; }
        public event MultiplayerSessionConnectionStateChangedEventHandler ConnectionStateChanged;
        public IMultiplayerSessionConnectionState CurrentState { get; private set; }
        public MultiplayerSessionReservation Reservation { get; private set; }

        public void Connect(string ipAddress)
        {
            IpAddress = ipAddress;
            CurrentState.NegotiateReservation(this);
        }

        public void ProcessSessionPolicy(MultiplayerSessionPolicy policy)
        {
            SessionPolicy = policy;
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
            CurrentState.Disconnect(this);
        }

        public void Send(Packet packet)
        {
            if (!suppressedPacketsTypes.Contains(packet.GetType()))
            {
                Client.Send(packet);
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
            IpAddress = null;
            SessionPolicy = null;
            PlayerSettings = null;
            AuthenticationContext = null;
            Reservation = null;
        }
    }
}
