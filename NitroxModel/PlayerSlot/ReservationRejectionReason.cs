using System.ComponentModel;

namespace NitroxModel.PlayerSlot
{
    public enum ReservationRejectionReason
    {
        [Description("None")]
        None,

        [Description("The player name is already in use. Please try again with a different name.")]
        PlayerNameInUse,

        //These are all intended for future use. Maybe YAGNI, but this is where we should look to expand upon server reservations
        [Description("The server is currently at capacity. Please try again later.")]
        ServerFull,

        [Description("The password that you provided for the server is incorrect.")]
        PasswordInvalid
    }
}