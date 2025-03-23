using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader.Config;
using tModPorter;
using static RemoteNPCHousing.Configs.ClientConfig;

namespace RemoteNPCHousing.Configs;
public enum PanelPositionOptions
{
	Vanilla = 0, // use what ever mH is already there
	MiniMapOpen = 1,
	MiniMapClosed = 2,
	CustomOffset = 3,
}

public class HousingPanelConfig
{
	[DrawTicks]
	[BackgroundColor(BG_Nest2_R, BG_Nest2_G, BG_Nest2_B)]
	public PanelPositionOptions PositionOption = PanelPositionOptions.MiniMapOpen;

	// What is a reasonable range for this? at least 256
	[Range(-500, 500)]
	[BackgroundColor(BG_Nest2_R, BG_Nest2_G, BG_Nest2_B)]
	public int VerticalOffset = 0;

	[BackgroundColor(BG_Nest2_R, BG_Nest2_G, BG_Nest2_B)]
	public bool SyncWithInventory = true; // if it was open in the map open it in the inventory (is this even possible/useful?)

	public int GetDefaultVerticalOffset(int old)
	{
		// TODO: there is some logic for handling if mH is off the screen, implement it here too
		int height = PositionOption switch
		{
			PanelPositionOptions.MiniMapOpen => 256,
			PanelPositionOptions.MiniMapClosed => 0,
			PanelPositionOptions.CustomOffset => VerticalOffset,
			PanelPositionOptions.Vanilla => old,
			_ => old,
		};
		return height;
	}

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
