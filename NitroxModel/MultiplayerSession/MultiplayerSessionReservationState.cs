using System;
using System.ComponentModel;
using System.Text;

namespace NitroxModel.MultiplayerSession
{
    [Flags]
    public enum MultiplayerSessionReservationState
    {
        Reserved = 0,
        Rejected = 1 << 0,

        [Description("The player name is already in use. Please try again with a different name.")]
        UniquePlayerNameConstraintViolated = 1 << 1,

        // These are all intended for future use. Maybe YAGNI, but this is where we should look to expand upon server reservations
        [Description("The server is currently at capacity. Please try again later.")]
        ServerPlayerCapacityReached = 1 << 2,

        [Description("The password that you provided for the server is incorrect.")]
        AuthenticationFailed = 1 << 3,

        [Description("The SteamID is already in use. Are you sure your not logged in?")]
        UniqueSteamIdConstraintViolated = 1 << 4
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
