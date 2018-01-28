namespace NitroxClient.Communication
{
    public enum ClientBridgeState
    {
        Disconnected,
        WaitingForRerservation,
        Failed,
        Connected,
        ReservationRejected
    }
}
