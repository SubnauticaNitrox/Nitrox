namespace NitroxModel.Networking
{

    public class NitroxDeliveryMethod
    {
        public enum DeliveryMethod
        {
            UNRELIABLE_SEQUENCED,
            RELIABLE_ORDERED
        }

        public static LiteNetLib.DeliveryMethod ToLiteNetLib(DeliveryMethod deliveryMethod)
        {
            switch (deliveryMethod)
            {
                case DeliveryMethod.UNRELIABLE_SEQUENCED:
                    return LiteNetLib.DeliveryMethod.Sequenced;
                case DeliveryMethod.RELIABLE_ORDERED:
                    return LiteNetLib.DeliveryMethod.ReliableOrdered;
                default:
                    return LiteNetLib.DeliveryMethod.ReliableOrdered;
            }
        }
    }
}
