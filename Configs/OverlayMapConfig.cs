using System;
using Terraria.ModLoader.Config;
using static RemoteNPCHousing.Configs.ClientConfig;

namespace RemoteNPCHousing.Configs;
public class OverlayMapConfig
{
	[Expand(true)]
	[BackgroundColor(BG_Nest1_R, BG_Nest1_G, BG_Nest1_B)]
	public HousingBannersConfig OverlayMapBannersOptions = new()
	{
		DisplayOptions = BannerDisplayOptions.NeverShow, // disable in overlay by default
	};

	public override bool Equals(object? obj)
	{
		return obj is OverlayMapConfig config &&
			   OverlayMapBannersOptions.Equals(config.OverlayMapBannersOptions);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(OverlayMapBannersOptions);
	}
}
