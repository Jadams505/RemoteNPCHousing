using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace RemoteNPCHousing;
public class ClientConfig : ModConfig
{
	public static ClientConfig Instance => ModContent.GetInstance<ClientConfig>();

	public override ConfigScope Mode => ConfigScope.ClientSide;

	public bool Enable = true;

	[Expand(false)]
	public FullscreenMapConfig FullscreenMapOptions = new();

	[Expand(false)]
	public MiniMapConfig MiniMapOptions = new();

	[Expand(false)]
	public OverlayMapConfig OverlayMapOptions = new();

	public HousingBannersConfig? GetRelevantConfig()
	{
		if (Main.mapFullscreen) return FullscreenMapOptions.FullscreenBannersOptions;
		if (Main.mapStyle == 1) return MiniMapOptions.MiniMapBannersOptions;
		if (Main.mapStyle == 2) return OverlayMapOptions.OverlayMapBannersOptions;
		return null;
	}
}

public enum IconDisplayOptions
{
	AlwaysShow,
	NeverShow,
}

public enum IconPositionOptions
{
	Vanilla,
	Custom
}

public class HousingIconConfig
{
	[DrawTicks]
	public IconDisplayOptions DisplayOption = IconDisplayOptions.AlwaysShow;

	[DrawTicks]
	public IconPositionOptions PositionOption = IconPositionOptions.Vanilla;

	[Range(0, 10000)]
	public int HousingIconPosX = 0;

	[Range(0, 10000)]
	public int HousingIconPosY = 0;

	public bool LockPosition = true;

	[Range(0.7f, 2f)]
	[DrawTicks]
	[Increment(0.1f)]
	public float Scale = 1f; // vanilla is technically 0.9, but this is nicer

	public Vector2 Position => new(HousingIconPosX, HousingIconPosY);

	// values taken from Main.DrawPageIcons()
	public Vector2 DefaultPosition()
	{
		// scale is not part of vanilla, but allows you to change scale in the config and it still be roughly vanilla positioned
		float yOffset = 174 - (32 * Scale) + MapHousingSystem.Main_mH(Main.instance);
		Vector2 defaultPos = new Vector2(Main.screenWidth - 162, (int)yOffset);
		defaultPos.X += 82 - 48;
		return defaultPos;
	}

	public override bool Equals(object? obj)
	{
		return obj is HousingIconConfig config &&
			   DisplayOption == config.DisplayOption &&
			   PositionOption == config.PositionOption &&
			   HousingIconPosX == config.HousingIconPosX &&
			   HousingIconPosY == config.HousingIconPosY &&
			   LockPosition == config.LockPosition &&
			   Scale == config.Scale;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(DisplayOption, PositionOption, HousingIconPosX, HousingIconPosY, LockPosition, Scale);
	}
}

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
	[Range(1, 8)]
	public int BannerTiles = 4;

	public override bool Equals(object? obj)
	{
		return obj is HousingBannersConfig config &&
			   ScaleOption == config.ScaleOption &&
			   BannerScale == config.BannerScale &&
			   HoverScale == config.HoverScale &&
			   DisplayOptions == config.DisplayOptions &&
			   BannerTiles == config.BannerTiles;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(ScaleOption, BannerScale, HoverScale, DisplayOptions, BannerTiles);
	}
}

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

public class MiniMapConfig
{
	[Expand(true)]
	public HousingBannersConfig MiniMapBannersOptions = new();

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