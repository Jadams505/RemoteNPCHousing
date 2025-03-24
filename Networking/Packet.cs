using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace RemoteNPCHousing.Networking;

/// <summary>
/// The purpose of this class is to write data to a buffer that can be sent
/// between the server and client
/// </summary>
internal abstract class Packet
{
	public byte Id { get; }

	protected Packet(byte id)
	{
		Id = id;
	}

	/// <summary>
	/// Used to add information to the packet
	/// </summary>
	/// <param name="packet">The ModPacket instance to write data to</param>
	public abstract void Encode(ModPacket packet);
}
