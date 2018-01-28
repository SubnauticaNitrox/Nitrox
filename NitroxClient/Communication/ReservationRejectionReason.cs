namespace NitroxClient.Communication
{
    public enum ReservationRejectionReason
    {
        None,
        PlayerNameInUse,

        //These are all intended for future use. Maybe YAGNI, but this is where we should look to expand upon server reservations
        ServerFull,
        PasswordInvalid
    }
}