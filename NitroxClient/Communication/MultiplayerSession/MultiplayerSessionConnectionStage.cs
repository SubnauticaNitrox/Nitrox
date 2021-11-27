namespace NitroxClient.Communication.MultiplayerSession
{
    public enum MultiplayerSessionConnectionStage
    {
        DISCONNECTED,
        ESTABLISHING_SERVER_POLICY,
        AWAITING_RESERVATION_CREDENTIALS,
        AWAITING_SESSION_RESERVATION,
        SESSION_RESERVED,
        SESSION_RESERVATION_REJECTED,
        SESSION_JOINED
    }
}
