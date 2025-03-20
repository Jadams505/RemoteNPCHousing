using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader.Config;

namespace RemoteNPCHousing.Configs;
public enum PanelPositionOptions
{
	MiniMapOpen,
	MiniMapClosed,
	CustomOffset
}

public class HousingPanelConfig
{
	[DrawTicks]
	public PanelPositionOptions PositionOption = PanelPositionOptions.MiniMapOpen;

	public int VerticalOffset = 0;

	public override bool Equals(object? obj)
	{
		return obj is HousingPanelConfig config &&
			   PositionOption == config.PositionOption &&
			   VerticalOffset == config.VerticalOffset;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(PositionOption, VerticalOffset);
	}
}
