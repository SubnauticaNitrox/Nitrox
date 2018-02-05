namespace NitroxClient.Communication
{
    public enum MultiplayerSessionManagerStage
    {
        Disconnected,
        WaitingForRerservation,
        Failed,
        Reserved,
        ReservationRejected,
        Connected
    }
}
