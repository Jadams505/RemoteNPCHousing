using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader.Config;
using static RemoteNPCHousing.Configs.ClientConfig;

namespace RemoteNPCHousing.Configs;
public class FullscreenMapConfig
{
	[Expand(true)]
	[BackgroundColor(BG_Nest1_R, BG_Nest1_G, BG_Nest1_B)]
	public HousingBannersConfig FullscreenBannersOptions = new();

	[Expand(true)]
	[BackgroundColor(BG_Nest1_R, BG_Nest1_G, BG_Nest1_B)]
	public HousingIconConfig HousingIconOptions = new();

	[Expand(true)]
	[BackgroundColor(BG_Nest1_R, BG_Nest1_G, BG_Nest1_B)]
	public HousingPanelConfig HousingPanelOptions = new();

	public override bool Equals(object? obj)
	{
		return obj is FullscreenMapConfig config &&
			   FullscreenBannersOptions.Equals(config.FullscreenBannersOptions) &&
			   HousingIconOptions.Equals(config.HousingIconOptions) &&
			   HousingPanelOptions.Equals(config.HousingPanelOptions);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(FullscreenBannersOptions, HousingIconOptions, HousingPanelOptions);
	}
}
