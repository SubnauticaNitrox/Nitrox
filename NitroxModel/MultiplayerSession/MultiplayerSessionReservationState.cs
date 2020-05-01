using System;
using System.ComponentModel;
using System.Text;

namespace NitroxModel.MultiplayerSession
{
    [Flags]
    public enum MultiplayerSessionReservationState
    {
        RESERVED = 0,
        REJECTED = 1 << 0,

        [Description("The player name is already in use. Please try again with a different name.")]
        UNIQUE_PLAYER_NAME_CONSTRAINT_VIOLATED = 1 << 1,

        // These are all intended for future use. Maybe YAGNI, but this is where we should look to expand upon server reservations
        [Description("The server is currently at capacity. Please try again later.")]
        SERVER_PLAYER_CAPACITY_REACHED = 1 << 2,

        [Description("The password that you provided for the server is incorrect.")]
        AUTHENTICATION_FAILED = 1 << 3,

        [Description("The server is using hardcore gamemode, player is dead.")]
        HARDCORE_PLAYER_DEAD = 1 << 4,
    }

    public static class MultiplayerSessionReservationStateExtensions
    {
        public static bool HasStateFlag(this MultiplayerSessionReservationState currentState, MultiplayerSessionReservationState checkedState)
        {
            return (currentState & checkedState) == checkedState;
        }

        public static string Describe(this MultiplayerSessionReservationState currentState)
        {
            StringBuilder descriptionBuilder = new StringBuilder();

            foreach (string reservationStateName in Enum.GetNames(typeof(MultiplayerSessionReservationState)))
            {
                MultiplayerSessionReservationState reservationState = (MultiplayerSessionReservationState)Enum.Parse(typeof(MultiplayerSessionReservationState), reservationStateName);
                if (currentState.HasStateFlag(reservationState))
                {
                    DescriptionAttribute descriptionAttribute = reservationState.GetAttribute<DescriptionAttribute>();

                    if (!string.IsNullOrEmpty(descriptionAttribute?.Description))
                    {
                        descriptionBuilder.AppendLine(descriptionAttribute.Description);
                    }
                }
            }

            return descriptionBuilder.ToString();
        }
    }
}
