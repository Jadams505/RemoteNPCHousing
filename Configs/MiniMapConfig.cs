using System;
using Terraria.ModLoader.Config;
using static RemoteNPCHousing.Configs.ClientConfig;

namespace RemoteNPCHousing.Configs;
public class MiniMapConfig
{
	[Expand(true)]
	[BackgroundColor(BG_Nest1_R, BG_Nest1_G, BG_Nest1_B)]
	public HousingBannersConfig MiniMapBannersOptions = new()
	{
		Enable = false, // disable in minimap by default
	};

	public override bool Equals(object? obj)
	{
		return obj is MiniMapConfig config &&
			   MiniMapBannersOptions.Equals(config.MiniMapBannersOptions);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(MiniMapBannersOptions);
	}
}
