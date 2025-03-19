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

	[Expand(false)]
	public HousingIconConfig HousingIconOptions = new();
}

public class HousingIconConfig
{
	public bool UseDefaultPosition = true;

	public bool LockPosition = true;

	[Range(0.2f, 5f)]
	[DrawTicks]
	[Increment(0.1f)]
	public float Scale = 0.9f;

	[Range(0, 10000)]
	public int HousingIconPosX = 0;

	[Range(0, 10000)]
	public int HousingIconPosY = 0;

	public Vector2 Position => new(HousingIconPosX, HousingIconPosY);

	// values taken from Main.DrawPageIcons()
	public static Vector2 DefaultPosition()
	{
		int yOffset = 174 - 32 + MapHousingSystem.Main_mH(Main.instance);
		Vector2 defaultPos = new Vector2(Main.screenWidth - 162, yOffset);
		defaultPos.X += 82 - 48;
		return defaultPos;
	}

	public override bool Equals(object? obj)
	{
		return obj is HousingIcon icon &&
			   UseDefaultPosition == icon.UseDefaultPosition &&
			   LockPosition == icon.LockPosition &&
			   HousingIconPosX == icon.HousingIconPosX &&
			   HousingIconPosY == icon.HousingIconPosY;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(UseDefaultPosition, LockPosition, HousingIconPosX, HousingIconPosY);
	}
}

