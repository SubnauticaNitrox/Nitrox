namespace NitroxClient.Communication
{
    public enum MultiplayerSessionConnectionStage
    {
        Disconnected,
        EstablishingServerPolicy,
        AwaitingReservationRequest,
        AwaitingReservationResponse,
        SessionReserved,
        SessionReservationRejected,
        SessionJoined,
        Failed
    }
}
