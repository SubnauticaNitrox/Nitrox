namespace NitroxModel.Networking
{

    public class NitroxDeliveryMethod
    {
        public enum DeliveryMethod
        {
            UnreliableSequenced,
            ReliableOrdered
        }

        public static LiteNetLib.DeliveryMethod ToLiteNetLib(DeliveryMethod deliveryMethod)
        {
            switch (deliveryMethod)
            {
                case DeliveryMethod.UnreliableSequenced:
                    return LiteNetLib.DeliveryMethod.Sequenced;
                case DeliveryMethod.ReliableOrdered:
                    return LiteNetLib.DeliveryMethod.ReliableOrdered;
                default:
                    return LiteNetLib.DeliveryMethod.ReliableOrdered;
            }
        }
    }
}
