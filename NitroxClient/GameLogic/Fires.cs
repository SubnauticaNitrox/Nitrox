using System.Collections.Generic;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;

namespace NitroxClient.GameLogic
{
    public class Fires
    {
        private readonly IPacketSender packetSender;

        /// <summary>
        /// Used to reduce the <see cref="FireDoused"/> packet spam as fires are being doused. A packet is only sent after
        /// the douse amount surpasses <see cref="FIRE_DOUSE_AMOUNT_TRIGGER"/>
        /// </summary>
        private readonly Dictionary<string, float> fireDouseAmount = new Dictionary<string, float>();

        /// <summary>
        /// Each extinguisher hit is from 0.15 to 0.25. 5 is a bit less than half a second of full extinguishing
        /// </summary>
        private const float FIRE_DOUSE_AMOUNT_TRIGGER = 5f;

        public Fires(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public void OnCreate(Fire fire, SubFire.RoomFire room)
        {
            Optional<string> subRootGuid = Optional<string>.OfNullable(GuidHelper.GetGuid(fire.fireSubRoot.gameObject));
            Optional<CyclopsRooms> startInRoom = Optional<CyclopsRooms>.OfNullable(room.roomLinks.room);

            FireCreated packet = new FireCreated(GuidHelper.GetGuid(fire.gameObject), subRootGuid, startInRoom);
            packetSender.Send(packet);
        }

        public void OnDouse(Fire fire, float douseAmount)
        {
            string fireGuid = GuidHelper.GetGuid(fire.gameObject);

            // Temporary packet limiter
            if (!fireDouseAmount.ContainsKey(fireGuid))
            {
                fireDouseAmount.Add(fireGuid, douseAmount);
            }
            else
            {
                float summedDouseAmount = fireDouseAmount[fireGuid] + douseAmount;

                if (summedDouseAmount > FIRE_DOUSE_AMOUNT_TRIGGER)
                {
                    // It is significantly faster to keep the key as a 0 value than to remove it and re-add it later.
                    fireDouseAmount[fireGuid] = 0;

                    FireDoused packet = new FireDoused(fireGuid, douseAmount);
                    packetSender.Send(packet);
                }
            }
        }
    }
}
