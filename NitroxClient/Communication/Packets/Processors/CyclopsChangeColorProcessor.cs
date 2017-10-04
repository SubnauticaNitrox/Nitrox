using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Helper.GameLogic;
using NitroxModel.Helper.Unity;
using NitroxModel.Packets;
using System;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
	public class CyclopsChangeColorProcessor : ClientPacketProcessor<CyclopsChangeColor>
	{
		private PacketSender packetSender;

		public CyclopsChangeColorProcessor(PacketSender packetSender)
		{
			this.packetSender = packetSender;
		}

		public override void Process(CyclopsChangeColor colorPacket)
		{
			GameObject opCyclops = GuidHelper.RequireObjectFrom(colorPacket.Guid);
			CyclopsNameScreenProxy screenProxy = opCyclops.RequireComponentInChildren<CyclopsNameScreenProxy>();
			SubName subname = (SubName)screenProxy.subNameInput.ReflectionGet("target");

			if (subname != null)
			{
				subname.SetColor(colorPacket.Index, colorPacket.HSB, colorPacket.Color);
				screenProxy.subNameInput.ReflectionCall("SetColor", false, false, new object[] { colorPacket.Index, colorPacket.Color });
				screenProxy.subNameInput.SetSelected(colorPacket.Index);
			}
			else
			{
				Console.WriteLine("Could not find SubName in SubNameInput to change color");
			}
		}

	}
}