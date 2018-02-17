using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using NitroxModel.PlayerSlot;

namespace NitroxModel
{
    public static class Extensions
    {
        public static TAttribute GetAttribute<TAttribute>(this Enum value)
            where TAttribute : Attribute
        {
            Type type = value.GetType();
            string name = Enum.GetName(type, value);

            return type.GetField(name)
                .GetCustomAttributes(false)
                .OfType<TAttribute>()
                .SingleOrDefault();
        }

        public static bool HasStateFlag(this PlayerSlotReservationState currentState, PlayerSlotReservationState checkedState)
        {
            return (currentState & checkedState) == checkedState;
        }

        public static string Describe(this PlayerSlotReservationState currentState)
        {
            StringBuilder descriptionBuilder = new StringBuilder();

            foreach (string reservationStateName in Enum.GetNames(typeof(PlayerSlotReservationState)))
            {
                PlayerSlotReservationState reservationState = (PlayerSlotReservationState)Enum.Parse(typeof(PlayerSlotReservationState), reservationStateName);
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

        public static string PrefixWith<T>(this IEnumerable<T> items, string prefix)
        {
            StringBuilder sb = new StringBuilder();
            foreach (T i in items)
            {
                sb.Append(prefix);
                sb.Append(i);
            }

            return sb.ToString();
        }
    }
}
