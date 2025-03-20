using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader.Config;

namespace RemoteNPCHousing.Configs;
public class OverlayMapConfig
{
	[Expand(true)]
	public HousingBannersConfig OverlayMapBannersOptions = new();

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
