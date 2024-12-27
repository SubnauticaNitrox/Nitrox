namespace NitroxModel.Networking
{

    public class NitroxDeliveryMethod
    {
        public enum DeliveryMethod : byte
        {
            /// <summary>
            /// <inheritdoc cref="LiteNetLib.DeliveryMethod.Sequenced"/>
            /// </summary>
            UNRELIABLE_SEQUENCED = LiteNetLib.DeliveryMethod.Sequenced,
            /// <summary>
            /// <inheritdoc cref="LiteNetLib.DeliveryMethod.ReliableOrdered"/>
            /// </summary>
            RELIABLE_ORDERED = LiteNetLib.DeliveryMethod.ReliableOrdered,
            /// <summary>
            /// <inheritdoc cref="LiteNetLib.DeliveryMethod.ReliableSequenced"/>
            /// </summary>
            RELIABLE_ORDERED_LAST = LiteNetLib.DeliveryMethod.ReliableSequenced,
        }

        public static LiteNetLib.DeliveryMethod ToLiteNetLib(DeliveryMethod deliveryMethod)
        {
            switch (deliveryMethod)
            {
                case DeliveryMethod.UNRELIABLE_SEQUENCED:
                case DeliveryMethod.RELIABLE_ORDERED_LAST:
                case DeliveryMethod.RELIABLE_ORDERED:
                    return (LiteNetLib.DeliveryMethod)deliveryMethod;

                default:
                    return LiteNetLib.DeliveryMethod.ReliableOrdered;
            }
        }
    }
}
