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

	[Expand(true)]
	public HousingIconConfig HousingIconOptions = new();

	[Expand(true)]
	public HousingBannersConfig HousingBannersOptions = new();

	[Expand(true)]
	public HousingPanelConfig HousingPanelOptions = new();
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

	public float BannerScale = 1f;

	public float HoverScale = 1.25f;

	[DrawTicks]
	public BannerDisplayOptions DisplayOptions = BannerDisplayOptions.Vanilla;

	public override bool Equals(object? obj)
	{
		return obj is HousingBannersConfig config &&
			   ScaleOption == config.ScaleOption &&
			   BannerScale == config.BannerScale &&
			   HoverScale == config.HoverScale &&
			   DisplayOptions == config.DisplayOptions;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(ScaleOption, BannerScale, HoverScale, DisplayOptions);
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