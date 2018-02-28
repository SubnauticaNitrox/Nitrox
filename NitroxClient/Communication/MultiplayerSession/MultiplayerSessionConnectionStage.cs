namespace NitroxClient.Communication.MultiplayerSession
{
    public enum MultiplayerSessionConnectionStage
    {
        Disconnected,
        EstablishingServerPolicy,
        AwaitingReservationCredentials,
        AwaitingSessionReservation,
        SessionReserved,
        SessionReservationRejected,
        SessionJoined
    }
}
