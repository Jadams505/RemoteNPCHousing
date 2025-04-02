using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static System.Collections.Specialized.BitVector32;

namespace RemoteNPCHousing.Networking;
internal class MoveRoomPacket : Packet
{
	public const int ID = 1;
	private readonly int _x;
	private readonly int _y;
	private readonly int _npcId;

	public MoveRoomPacket(int x, int y, int id) : base(ID)
	{
		_x = x;
		_y = y;
		_npcId = id;
	}

	public override void Encode(ModPacket packet)
	{
		packet.Write(_npcId);
		packet.Write(_x);
		packet.Write(_y);
	}

	public static void HandlePacket(BinaryReader reader, int whoSentId)
	{
		int id = reader.ReadInt32();
		int x = reader.ReadInt32();
		int y = reader.ReadInt32();
		
		if (Main.dedServ)
		{
			int sectionX = x * Main.sectionWidth;
			int sectionY = y * Main.sectionHeight;
			int whoAmi = whoSentId;
			if (sectionX < 0 || sectionY < 0 || sectionX >= Main.maxSectionsX || sectionY >= Main.maxSectionsY || Netplay.Clients[whoAmi].TileSections[sectionX, sectionY])
				return;

			Netplay.Clients[whoAmi].TileSections[sectionX, sectionY] = true;
			int number = sectionX * 200;
			int num = sectionY * 150;
			int num2 = 150;
			for (int i = num; i < num + 150; i += num2)
			{
				//NetMessage.CompressTileBlock(number, (int)number2, (short)number3, (short)number4, writer.BaseStream);
				//SendData(10, whoAmi, -1, null, number, i, 200f, num2);
			}
		}
		else
		{
			Main.instance.SetMouseNPC(-1, -1); // does this need to be called first?
			WorldGen.moveRoom(x, y, id);
			SoundEngine.PlaySound(SoundID.MenuTick);
		}
	}
}
