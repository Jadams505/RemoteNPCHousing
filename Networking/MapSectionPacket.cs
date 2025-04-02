using System.IO;
using Terraria;
using Terraria.ModLoader;

namespace RemoteNPCHousing.Networking;

internal class MapSectionPacket : Packet
{
	public const byte ID = 1;

	private readonly int _sectionX;
	private readonly int _sectionY;
	private readonly byte _radius;

	public static MapSectionPacket FromTile(int x, int y, byte radius = 0) => 
		new MapSectionPacket(Netplay.GetSectionX(x), Netplay.GetSectionY(y), radius);

	public static MapSectionPacket FromSection(int sectionX, int sectionY, byte radius = 0) =>
		new MapSectionPacket(sectionX, sectionY, radius);

	private MapSectionPacket(int sectionX, int sectionY, byte radius = 0) : base(ID)
	{
		_sectionX = sectionX;
		_sectionY = sectionY;
		_radius = radius;
	}

	public override void Encode(ModPacket packet)
	{
		packet.Write(_sectionX);
		packet.Write(_sectionY);
		packet.Write(_radius);
	}

	public static void HandlePacket(BinaryReader reader, int whoSentIt)
	{
		int sectionX = reader.ReadInt32();
		int sectionY = reader.ReadInt32();
		byte radius = reader.ReadByte();

		if (Main.dedServ)
		{
			for (int i = -radius; i <= radius; ++i)
			{
				for (int j = -radius; j <= radius; ++j)
				{
					NetMessage.SendSection(whoSentIt, sectionX + i, sectionY + j);
				}
			}
		}
	}
}
