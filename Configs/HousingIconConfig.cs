using System;
using Terraria;
using Terraria.ModLoader.Config;
using static RemoteNPCHousing.Configs.ClientConfig;

namespace RemoteNPCHousing.Configs;

public enum IconDisplayOptions
{
	AlwaysShow = 0,
	NeverShow = 1,
}

public enum IconPositionOptions
{
	Vanilla = 0,
	Custom = 1,
}

public class HousingIconConfig
{
	[DrawTicks]
	[BackgroundColor(BG_Nest2_R, BG_Nest2_G, BG_Nest2_B)]
	public IconDisplayOptions DisplayOption = IconDisplayOptions.AlwaysShow;

	[DrawTicks]
	[BackgroundColor(BG_Nest2_R, BG_Nest2_G, BG_Nest2_B)]
	public IconPositionOptions PositionOption = IconPositionOptions.Vanilla;

	[Range(0, 1f)]
	[BackgroundColor(BG_Nest2_R, BG_Nest2_G, BG_Nest2_B)]
	public float HousingIconX = 0f;

	[Range(0, 1f)]
	[BackgroundColor(BG_Nest2_R, BG_Nest2_G, BG_Nest2_B)]
	public float HousingIconY = 0f;

	[BackgroundColor(BG_Nest2_R, BG_Nest2_G, BG_Nest2_B)]
	public bool LockPosition = true;

	[Range(0.7f, 2f)]
	[DrawTicks]
	[Increment(0.1f)]
	[BackgroundColor(BG_Nest2_R, BG_Nest2_G, BG_Nest2_B)]
	public float Scale = 1f; // vanilla is technically 0.9, but this is nicer

	[BackgroundColor(BG_Nest2_R, BG_Nest2_G, BG_Nest2_B)]
	public bool AllowHoverText = true;

	// values taken from Main.DrawPageIcons()
	public Vector2 DefaultPosition(int mapHeight)
	{
		// scale is not part of vanilla, but allows you to change scale in the config and it still be roughly vanilla positioned
		float yOffset = 174 - 32 * Scale + mapHeight;
		Vector2 defaultPos = new Vector2(Main.screenWidth - 162, (int)yOffset);
		defaultPos.X += 82 - 48;
		return defaultPos;
	}

	public override bool Equals(object? obj)
	{
		return obj is HousingIconConfig config &&
			   DisplayOption == config.DisplayOption &&
			   PositionOption == config.PositionOption &&
			   HousingIconX == config.HousingIconX &&
			   HousingIconY == config.HousingIconY &&
			   LockPosition == config.LockPosition &&
			   Scale == config.Scale &&
			   AllowHoverText == config.AllowHoverText;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(DisplayOption, PositionOption, HousingIconX, HousingIconY, LockPosition, Scale);
	}
}
