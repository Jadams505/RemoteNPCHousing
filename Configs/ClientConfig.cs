using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace RemoteNPCHousing.Configs;
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