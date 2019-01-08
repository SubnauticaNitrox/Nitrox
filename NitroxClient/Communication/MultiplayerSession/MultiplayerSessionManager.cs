using System;
using System.Collections.Generic;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.MultiplayerSession.ConnectionState;
using NitroxModel.Logger;
using NitroxModel.MultiplayerSession;
using NitroxModel.Packets;
using NitroxClient.GameLogic;

namespace NitroxClient.Communication.MultiplayerSession
{
    public class MultiplayerSessionManager : IMultiplayerSession, IMultiplayerSessionConnectionContext
    {
        private readonly HashSet<Type> suppressedPacketsTypes = new HashSet<Type>();

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

        public IClient Client { get; }
        public string IpAddress { get; private set; }
        public int ServerPort{ get; private set; }
        public MultiplayerSessionPolicy SessionPolicy { get; private set; }
        public PlayerSettings PlayerSettings { get; private set; }
        public AuthenticationContext AuthenticationContext { get; private set; }
        public event MultiplayerSessionConnectionStateChangedEventHandler ConnectionStateChanged;
        public IMultiplayerSessionConnectionState CurrentState { get; private set; }
        public MultiplayerSessionReservation Reservation { get; private set; }

        public void Connect(string ipAddress,int port)
        {
            IpAddress = ipAddress;
            ServerPort = port;
            CurrentState.NegotiateReservation(this);
        }

        public void ProcessSessionPolicy(MultiplayerSessionPolicy policy)
        {
            SessionPolicy = policy;
            NitroxConsole.DisableConsole = SessionPolicy.DisableConsole;
            if(SessionPolicy.NitroxVersionAllowed != typeof(NitroxModel.Extensions).Assembly.FullName)
            {
                Log.Warn($"Nitrox Model versions do not match.\n" +
                    $"    Server - {SessionPolicy.NitroxVersionAllowed}\n" +
                    $"    Client - {typeof(NitroxModel.Extensions).Assembly.FullName}");
                Log.InGame("The server is using a different version of Nitrox. Please contact the server admin to install the same version.");
                CurrentState.Disconnect(this);
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
            ServerPort = 11000;
            SessionPolicy = null;
            PlayerSettings = null;
            AuthenticationContext = null;
            Reservation = null;
        }
    }
}
