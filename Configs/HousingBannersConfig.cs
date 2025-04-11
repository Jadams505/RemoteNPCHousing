using System;
using Terraria.ModLoader.Config;
using static RemoteNPCHousing.Configs.ClientConfig;

namespace RemoteNPCHousing.Configs;

public enum BannerDisplayOptions
{
	Vanilla = 0, // show if housing panel is open
	AlwaysShow = 1,
	NeverShow = 2
}

public enum BannerScaleOptions
{
	UseScaleValues = 0,
	UseTileValues = 1,
}

public class HousingBannersConfig
{
	[DrawTicks]
	[BackgroundColor(BG_Nest2_R, BG_Nest2_G, BG_Nest2_B)]
	public BannerDisplayOptions DisplayOptions = BannerDisplayOptions.Vanilla;

	[DrawTicks]
	[BackgroundColor(BG_Nest2_R, BG_Nest2_G, BG_Nest2_B)]
	public BannerScaleOptions ScaleOption = BannerScaleOptions.UseScaleValues;

	[Range(0.25f, 2f)]
	[Increment(0.25f)]
	[DrawTicks]
	[BackgroundColor(BG_Nest2_R, BG_Nest2_G, BG_Nest2_B)]
	public float BannerScale = 1f;

	// Used with the UseTileValues option
	// This is how many map tiles the image should take up
	// so that it can be scaled to fit these dimensions
	// TODO: Dynamically determine so that it always fits, then add a multiplier or something
	[Range(1, 16)]
	[DrawTicks]
	[BackgroundColor(BG_Nest2_R, BG_Nest2_G, BG_Nest2_B)]
	public int BannerTiles = 2;

	[Range(0.25f, 4f)]
	[Increment(0.25f)]
	[DrawTicks]
	[BackgroundColor(BG_Nest2_R, BG_Nest2_G, BG_Nest2_B)]
	public float HoverScale = 1.25f;

	[BackgroundColor(BG_Nest2_R, BG_Nest2_G, BG_Nest2_B)]
	public bool AllowHoverText = true;

	[BackgroundColor(BG_Nest2_R, BG_Nest2_G, BG_Nest2_B)]
	public bool AllowClickActions = true;

	public override bool Equals(object? obj)
	{
		return obj is HousingBannersConfig config &&
			   ScaleOption == config.ScaleOption &&
			   BannerScale == config.BannerScale &&
			   HoverScale == config.HoverScale &&
			   DisplayOptions == config.DisplayOptions &&
			   BannerTiles == config.BannerTiles &&
			   AllowHoverText == config.AllowHoverText &&
			   AllowClickActions == config.AllowClickActions;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(ScaleOption, BannerScale, HoverScale, DisplayOptions, BannerTiles);
	}
}
