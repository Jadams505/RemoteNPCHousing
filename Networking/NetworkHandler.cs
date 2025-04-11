using log4net;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RemoteNPCHousing.Networking;
/// <summary>
/// The purpose of this class is to handle all the Packets that need 
/// to be sent between the server and the client
/// </summary>
internal class NetworkHandler
{
	private const int SEND_TO_ALL = -1;
	private const int SERVER = -1;

	private static Mod ModInstance => RemoteNPCHousing.Instance;
	private static ILog Logger => ModInstance.Logger;

	public static bool EnableLogging { get; set; } = false;

	public static void HandlePackets(BinaryReader reader, int whoSentIt)
	{
		byte id = reader.ReadByte();
		if (id == MapSectionPacket.ID)
		{
			MapSectionPacket.HandlePacket(reader, whoSentIt);
		}
	}

	/// <summary>
	/// Sends a packet to the server
	/// Can only be called by the client
	/// </summary>
	/// <param name="packet">The packet instance to send</param>
	/// <param name="whoSentIt">The client id of the player who is sending the packet</param>
	public static void SendToServer(Packet packet, int whoSentIt)
	{
		if (Main.netMode == NetmodeID.MultiplayerClient)
		{
			if (EnableLogging)
				Logger.Info($"Client {whoSentIt} sent a {packet.GetType().Name} to the server [{packet.ToString()}]");

			ModPacket data = GetPacket(packet);
			packet.Encode(data);
			data.Send(SERVER, whoSentIt);
		}
	}

	/// <summary>
	/// Sends a packet to a specific client
	/// Can only be called by the server
	/// </summary>
	/// <param name="packet">The packet instance to send</param>
	/// <param name="sendTo">The client id of the player to send the packet to</param>
	public static void SendToClient(Packet packet, int sendTo)
	{
		if (Main.dedServ)
		{
			if (EnableLogging)
				Logger.Info($"Server sent a {packet.GetType().Name} to client {sendTo} [{packet.ToString()}]");

			ModPacket data = GetPacket(packet);
			packet.Encode(data);
			data.Send(sendTo, SERVER);
		}
	}

	/// <summary>
	/// Sends a packet to all clients except the one the server got it from
	/// For a client to use this a packet must be sent to the server first
	/// Can only be called by the server
	/// </summary>
	/// <param name="packet">The packet intance to send</param>
	/// <param name="whoServerGotItFrom">The client id of the player who is notifying the server of a change
	/// that needs to be synced with the other clients</param>
	public static void SendToClients(Packet packet, int whoServerGotItFrom)
	{
		if (Main.dedServ)
		{
			if (EnableLogging)
				Logger.Info($"Server sent a {packet.GetType().Name} to every client except {whoServerGotItFrom} [{packet.ToString()}]");

			ModPacket data = GetPacket(packet);
			packet.Encode(data);
			data.Send(SEND_TO_ALL, whoServerGotItFrom);
		}
	}

	/// <summary>
	/// Sends a packet from sendTo to whoSentIt
	/// </summary>
	/// <param name="packet">The packet intance to send</param>
	/// <param name="sendTo">The id of player to send the packet to
	/// <param name="whoSentIt">The id of player who is sending the packet
	public static void Send(Packet packet, int sendTo, int whoSentIt)
	{
		if (Main.dedServ)
		{
			if (EnableLogging)
				Logger.Info($"{whoSentIt} sent a {packet.GetType().Name} to {sendTo} [{packet.ToString()}]");

			ModPacket data = GetPacket(packet);
			packet.Encode(data);
			data.Send(sendTo, whoSentIt);
		}
	}

	/// <summary>
	/// Gets a ModPacket instance and writes the packet id to it
	/// </summary>
	/// <param name="packetIn">The Packet instance with the id to write</param>
	private static ModPacket GetPacket(Packet packetIn)
	{
		ModPacket packetOut = ModInstance.GetPacket();
		packetOut.Write(packetIn.Id);
		return packetOut;
	}
}
