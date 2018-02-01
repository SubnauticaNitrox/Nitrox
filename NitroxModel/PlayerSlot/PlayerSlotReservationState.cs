using System;
using System.ComponentModel;

namespace NitroxModel.PlayerSlot
{
    [Flags]
    public enum PlayerSlotReservationState
    {
        Reserved = 0,        
        Rejected = 1 << 0,

        [Description("The player name is already in use. Please try again with a different name.")]
        UniquePlayerNameConstraintViolated = 1 << 1,

        //These are all intended for future use. Maybe YAGNI, but this is where we should look to expand upon server reservations
        [Description("The server is currently at capacity. Please try again later.")]
        ServerPlayerCapacityReached = 1 << 2,

        [Description("The password that you provided for the server is incorrect.")]
        AuthenticationFailed = 1 << 3
    }
}