using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader.Config;

namespace RemoteNPCHousing.Configs;
public class FullscreenMapConfig
{
	[Expand(true)]
	public HousingBannersConfig FullscreenBannersOptions = new();

	[Expand(false)]
	public HousingIconConfig HousingIconOptions = new();

	[Expand(false)]
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
