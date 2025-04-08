using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace RemoteNPCHousing.Configs;

public class ServerConfig : ModConfig
{
	public static ServerConfig Instance => ModContent.GetInstance<ServerConfig>();

	public override ConfigScope Mode => ConfigScope.ServerSide;

	// Load map sections when querying an NPC, must be revealed
	[DefaultValue(true)]
	public bool LoadQueriedChunks;

	// Keep sections loaded that contain housed NPCs, must be revealed
	[DefaultValue(true)]
	public bool LoadNPCHomeChunks;
}

public enum LoadMapSectionOptions
{
	Load = 0,
	Reveal = 1,
	Nothing = 2,
	OnlyLoadAnyRevealedChunks = 3,
	OnlyLoadFullyRevealedChunks = 4,
	OnlyRevealLoadedChunks = 5
}