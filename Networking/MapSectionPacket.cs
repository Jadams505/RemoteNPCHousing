using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace RemoteNPCHousing.Networking;
internal class MapSectionPacket : Packet
{
	public const byte ID = 1;

	private readonly int _sectionX;
	private readonly int _sectionY;

	public MapSectionPacket(int sectionX, int sectionY) : base(ID)
	{
		_sectionX = sectionX;
		_sectionY = sectionY;
	}

	public override void Encode(ModPacket packet)
	{
		packet.Write(_sectionX);
		packet.Write(_sectionY);
	}

	public static void HandlePacket(BinaryReader reader, int whoSentIt)
	{
		int sectionX = reader.ReadInt32();
		int sectionY = reader.ReadInt32();

		if (Main.dedServ)
		{
			NetMessage.SendSection(whoSentIt, sectionX, sectionY);
		}
	}
}
