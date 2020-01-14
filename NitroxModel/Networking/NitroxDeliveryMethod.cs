using Lidgren.Network;

namespace NitroxModel.Networking
{

    public class NitroxDeliveryMethod
    {
        public enum DeliveryMethod
        {
            UnreliableSequenced,
            ReliableOrdered
        }

        public static NetDeliveryMethod ToLidgren(DeliveryMethod deliveryMethod)
        {
            switch (deliveryMethod)
            {
                case DeliveryMethod.UnreliableSequenced:
                    return NetDeliveryMethod.UnreliableSequenced;
                case DeliveryMethod.ReliableOrdered:
                    return NetDeliveryMethod.ReliableOrdered;
                default:
                    return NetDeliveryMethod.ReliableOrdered;
            }
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
