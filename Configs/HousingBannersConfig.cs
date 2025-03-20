using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader.Config;

namespace RemoteNPCHousing.Configs;

public enum BannerDisplayOptions
{
	Vanilla, // show if housing panel is open
	AlwaysShow,
	NeverShow
}

public enum BannerScaleOptions
{
	UseScaleValues,
	ScaleToFit
}

public class HousingBannersConfig
{
	[DrawTicks]
	public BannerScaleOptions ScaleOption = BannerScaleOptions.UseScaleValues;

	[Range(0.25f, 2f)]
	[Increment(0.25f)]
	public float BannerScale = 1f;

	[Range(0.25f, 4f)]
	[Increment(0.25f)]
	public float HoverScale = 1.25f;

	[DrawTicks]
	public BannerDisplayOptions DisplayOptions = BannerDisplayOptions.Vanilla;

	// Used with the ScaleToFit option
	// This is how many map tiles the image should take up
	// so that it can be scaled to fit these dimensions
	// TODO: Dynamically determine so that it always fits, then add a multiplier or something
	[Range(1, 16)]
	public int BannerTiles = 4;

	public bool AllowHoverText = true;

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
