using RemoteNPCHousing.Configs;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Map;
using Terraria.ModLoader;
using Terraria.UI;

namespace RemoteNPCHousing;
public class NPCHousesMapLayer : ModMapLayer
{
	public static HousingBannersConfig Config => ClientConfig.Instance.GetRelevantConfig();

	public static bool ShouldDraw()
	{
		if (Config is null) return false;
		if (!ClientConfig.Instance.Enable) return false;
		if (!Config.Enable) return false;
		if (Config.DisplayOptions == BannerDisplayOptions.AlwaysShow) return true;
		if (Config.DisplayOptions == BannerDisplayOptions.NeverShow) return false;

		if (Config.DisplayOptions == BannerDisplayOptions.Vanilla)
		{
			if (Main.mapFullscreen) return MapHousingSystem.Instance.IsHousingOpen;
			if (Main.mapStyle == 1) return Main.EquipPage == 1;
			if (Main.mapStyle == 2) return Main.EquipPage == 1;
		}

		return false;
	}

	public static IEnumerable<int> NpcsWithBanners()
	{
		for (int i = 0; i < Main.maxNPCs; ++i)
		{
			var npc = Main.npc[i];
			if (npc.active && npc.townNPC && !npc.homeless && npc.homeTileX > 0 && npc.homeTileY > 0 && npc.type != 37)
				yield return i;
		}
	}

	internal readonly List<int> npcsWithBanners = [];
	internal readonly List<int> occupantBanners = [];

	// Re-implementation of Main.DrawNPCHousesInWorld()
	public override void Draw(ref MapOverlayDrawContext context, ref string text)
	{
		npcsWithBanners.Clear();
		occupantBanners.Clear();
		npcsWithBanners.AddRange(NpcsWithBanners());

		var config = Config;
		int bannerOnTop = -1;

		npcsWithBanners.Sort((a, b) => -Main.npc[a].housingCategory.CompareTo(Main.npc[b].housingCategory));
		for (int i = 0; i < npcsWithBanners.Count; ++i)
		{
			var npc = Main.npc[npcsWithBanners[i]];

			int npcHomeY = npc.homeTileY; // num3
			int npcHomeX = npc.homeTileX;
			WorldGen.TownManager.AddOccupantsToList(npc.homeTileX, npc.homeTileY, occupantBanners);

			// roommates is a unused feature in vanilla which allows banners to show up on top of each other
			// The Roommates mod (https://steamcommunity.com/sharedfiles/filedetails/?id=3168223097) exposes this feature so this is necessary
			// for compatibility with that mod
			int roomateCount = 0;
			for (int num5 = npcsWithBanners.Count - 1; num5 > i; num5--)
			{
				var roomate = Main.npc[npcsWithBanners[num5]];
				if (roomate.homeTileX == npcHomeX && roomate.homeTileY == npcHomeY)
					roomateCount++;
			}

			bool anyUnloadedTiles = false;
			// move the banner up until it finds the top of the house
			// so the banner looks like it is hanging from the roof
			do
			{
				npcHomeY--;
				if (npcHomeY < 10) break;

				// if any tiles are unloaded then the banner placement will not be correct
				if (!Main.sectionManager.TileLoaded(npcHomeX, npcHomeY))
				{
					anyUnloadedTiles = true;
					break;
				}
			} while (!Main.tile[npcHomeX, npcHomeY].HasTile || !Main.tileSolid[Main.tile[npcHomeX, npcHomeY].TileType]);

			var bannerTexture = TextureAssets.HouseBanner.Value;
			int bannerStyle = occupantBanners.Contains(npc.type) ? 1 : 0;
			SpriteFrame bannerFrame = new(2, 2)
			{
				CurrentColumn = (byte)bannerStyle,
				CurrentRow = (byte)Math.Clamp(npc.housingCategory, 0, 1),
				PaddingX = 2,
				PaddingY = 2,
			};

			// used for ScaleToFit. This is how many map tiles the image should take up
			// so that it can be scaled to fit these dimensions
			int fitTiles = config.BannerTiles;

			Vector2 position = new()
			{
				X = npc.homeTileX,
				// + 2 here seems to work the best with Alignment.Center
				// TODO: This should really be + 1 with Alignment.Top
				Y = npcHomeY + 2
			};

			// defaulting to using the actual home position will prevent banners from being at the top of the map
			// TODO: Should this be before/after UseTileValues?
			if (anyUnloadedTiles)
			{
				position.Y = npc.homeTileY;
			}

			if (config.ScaleOption == BannerScaleOptions.UseTileValues)
			{
				position.Y += fitTiles / 4; // where did this 4 come from?
				position.Y += roomateCount * 11.5f * fitTiles / 16f; // why 11.5?
			}
			else
			{
				position.Y += roomateCount * .95f;
			}

			var drawSize = bannerFrame.GetSourceRectangle(bannerTexture).Size();

			DetermineScale(fitTiles, drawSize, out var normalScale, out var hoverScale);
			var bannerResult = context.Draw(
				texture: bannerTexture,
				position: position,
				color: Color.White,
				frame: bannerFrame,
				scaleIfNotSelected: normalScale,
				scaleIfSelected: hoverScale,
				alignment: Alignment.Center
			);

			int headIndex = TownNPCProfiles.GetHeadIndexSafe(npc);
			var headTexture = TextureAssets.NpcHead[headIndex].Value;
			var headFrame = new SpriteFrame(1, 1);
			float headMaxDim = Math.Max(headTexture.Width, headTexture.Height);
			float scale = headMaxDim > 24 ? 24f / headMaxDim : 1f;

			var headResult = context.Draw(
				texture: headTexture,
				position: position,
				color: Color.White,
				frame: headFrame,
				scaleIfNotSelected: scale * (bannerResult.IsMouseOver ? hoverScale : normalScale),
				scaleIfSelected: scale * (bannerResult.IsMouseOver ? hoverScale : normalScale),
				alignment: Alignment.Center
			);

			// the banner is always bigger than the head so I don't have to handle both hovers
			if (bannerResult.IsMouseOver)
			{
				if (config.AllowHoverText)
				{
					string bannerText = Lang.GetNPCHouseBannerText(npc, bannerStyle);
					text = bannerText;
				}

				bannerOnTop = npcsWithBanners[i];
			}
		}

		// the click action should only happen on the banner that is on top
		if (bannerOnTop != -1 && Main.mouseRightRelease && Main.mouseRight && config.AllowClickActions)
		{
			// since this is called before PostDrawFullscreenMap() resetting this will
			// prevent clearing the housing query AND the current housing banner at the same time
			// which is the normal behavior.
			Main.mouseRightRelease = false;
			WorldGen.kickOut(bannerOnTop);
			SoundEngine.PlaySound(SoundID.MenuTick);
		}
	}

	private static void DetermineScale(int fitTiles, Vector2 drawSize, out float normalScale, out float hoverScale)
	{
		normalScale = 1f;
		hoverScale = 1f;
		if (Config is null) return;

		if (Main.mapFullscreen)
		{
			DetermineScale(Main.mapFullscreenScale, out normalScale, out hoverScale);
		}
		else if (Main.mapStyle == 1)
		{
			DetermineScale(Main.mapMinimapScale, out normalScale, out hoverScale);
		}
		else if (Main.mapStyle == 2)
		{
			DetermineScale(Main.mapOverlayScale, out normalScale, out hoverScale);
		}

		void DetermineScale(float scale, out float normalScale, out float hoverScale)
		{
			float hoverFactor = Config.HoverScale; // hover scale is used for both. I may regret this.
			normalScale = Config.ScaleOption switch
			{
				BannerScaleOptions.UseScaleValues => Config.BannerScale,
				BannerScaleOptions.UseTileValues => scale * fitTiles / drawSize.Y,
				_ => Config.BannerScale,
			};
			hoverScale = Config.ScaleOption switch
			{
				BannerScaleOptions.UseScaleValues => Config.HoverScale,
				BannerScaleOptions.UseTileValues => hoverFactor * scale * fitTiles / drawSize.Y,
				_ => Config.HoverScale,
			};
		}

	}
}
