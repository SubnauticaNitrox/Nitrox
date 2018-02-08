namespace NitroxClient.Communication
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
