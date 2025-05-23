﻿using System.ComponentModel;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.UI;

namespace RemoteNPCHousing.Configs;
public class ClientConfig : ModConfig
{
	public static ClientConfig Instance => ModContent.GetInstance<ClientConfig>();
	internal static Color BackgroundColor = UICommon.DefaultUIBlue;
	internal const int BG_Nest1_R = 58, BG_Nest1_G = 75, BG_Nest1_B = 137;
	internal const int BG_Nest2_R = 44, BG_Nest2_G = 56, BG_Nest2_B = 103;
	internal const int BG_Nest3_R = 29, BG_Nest3_G = 38, BG_Nest3_B = 68;

	public override ConfigScope Mode => ConfigScope.ClientSide;

	[DefaultValue(true)]
	public bool Enable;

	[Expand(false)]
	public FullscreenMapConfig FullscreenMapOptions = new();

	[Expand(false)]
	public MiniMapConfig MiniMapOptions = new();

	[Expand(false)]
	public OverlayMapConfig OverlayMapOptions = new();

	[DefaultValue(false)]
	public bool DebugMode;

	// When doing a pylon teleport from the fullscreen map, but having mapStyle == 0
	// The fullscreen map is still being drawn, but Main.fullscreen is false
	// This acts as a default config so that it is not null in these cases.
	private readonly HousingBannersConfig DefaultBannerConfig = new()
	{
		Enable = false
	};

	public HousingBannersConfig GetRelevantConfig()
	{
		if (Main.mapFullscreen) return FullscreenMapOptions.FullscreenBannersOptions;
		if (Main.mapStyle == 1) return MiniMapOptions.MiniMapBannersOptions;
		if (Main.mapStyle == 2) return OverlayMapOptions.OverlayMapBannersOptions;
		return DefaultBannerConfig;
	}
}