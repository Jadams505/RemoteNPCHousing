using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader.Config;

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

public enum InitialStateOptions // state when map is opened
{
	Enabled = 0,
	Disabled = 1,
	Remember = 2,
	Vanilla = 3, // match the state of vanilla housing
	VanillaIfEnabled = 4, // if vanilla housing is open when opening the map, then start opened
	VanillaIfDisabled = 5, // if vanilla housing is closed when opening the map, then start closed
}

public class HousingIconConfig
{
	[DrawTicks]
	public IconDisplayOptions DisplayOption = IconDisplayOptions.AlwaysShow;

	[DrawTicks]
	public InitialStateOptions InitialStateOption = InitialStateOptions.VanillaIfEnabled;

	[DrawTicks]
	public IconPositionOptions PositionOption = IconPositionOptions.Vanilla;

	[Range(0, 1f)]
	public float HousingIconX = 0f;

	[Range(0, 1f)]
	public float HousingIconY = 0f;

	public bool LockPosition = true;

	[Range(0.7f, 2f)]
	[DrawTicks]
	[Increment(0.1f)]
	public float Scale = 1f; // vanilla is technically 0.9, but this is nicer

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

	public bool InitialState(bool lastState, bool vanillaState) => InitialStateOption switch
	{
		InitialStateOptions.Enabled => true,
		InitialStateOptions.Disabled => false,
		InitialStateOptions.Remember => lastState,
		InitialStateOptions.Vanilla => vanillaState,
		InitialStateOptions.VanillaIfEnabled => vanillaState ? vanillaState : lastState,
		InitialStateOptions.VanillaIfDisabled => vanillaState ? lastState : vanillaState,
		_ => lastState,
	};

	public override bool Equals(object? obj)
	{
		return obj is HousingIconConfig config &&
			   DisplayOption == config.DisplayOption &&
			   InitialStateOption == config.InitialStateOption &&
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
