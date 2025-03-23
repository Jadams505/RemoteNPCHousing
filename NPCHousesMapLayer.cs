using Microsoft.Xna.Framework.Graphics;
using RemoteNPCHousing.Configs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.Map;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Gamepad;

namespace RemoteNPCHousing;
public class NPCHousesMapLayer : ModMapLayer
{
	public static HousingBannersConfig? Config => ClientConfig.Instance.GetRelevantConfig();

	public static bool ShouldDraw()
	{
		if (Config is null) return false;
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

	// Re-implementation of Main.DrawNPCHousesInWorld()
	public override void Draw(ref MapOverlayDrawContext context, ref string text)
	{
		if (!ShouldDraw()) return;

		List<int> npcsWithBanners = [];
		List<int> occupantBanners = [];
		for (int i = 0; i < Main.maxNPCs; ++i)
		{
			var npc = Main.npc[i];
			if (npc.active && npc.townNPC && !npc.homeless && npc.homeTileX > 0 && npc.homeTileY > 0 && npc.type != 37)
				npcsWithBanners.Add(i);
		}

		npcsWithBanners.Sort((a, b) => -Main.npc[a].housingCategory.CompareTo(Main.npc[b].housingCategory));
		for (int i = 0; i < npcsWithBanners.Count; ++i)
		{
			var npc = Main.npc[npcsWithBanners[i]];

			int npcHomeY = npc.homeTileY; // num3
			int npcHomeX = npc.homeTileX;
			WorldGen.TownManager.AddOccupantsToList(npc.homeTileX, npc.homeTileY, occupantBanners);

			/*
			int roomateCount = 0;
			for (int j = i + 1; j < npcsWithBanners.Count; ++j)
			{
				var roomate = Main.npc[npcsWithBanners[j]];
				if (roomate.homeTileX == npc.homeTileX && roomate.homeTileY == npc.homeTileY)
					roomateCount++;
			}
			*/

			// move the banner up until it finds the top of the house
			// so the banner looks like it is hanging from the roof
			do
			{
				npcHomeY--;
				if (npcHomeY < 10) break;
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
			int fitTiles = Config!.BannerTiles;

			Vector2 position = new()
			{
				X = npc.homeTileX,
				// + 2 here seems to work the best with Alignment.Center
				// TODO: This should really be + 1 with Alignment.Top
				Y = npcHomeY + 2
			};

			if (Config.ScaleOption == BannerScaleOptions.UseTileValues)
			{
				position.Y += fitTiles / 4; // where did this 4 come from?
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
				if (Config.AllowHoverText)
				{
					string bannerText = Lang.GetNPCHouseBannerText(npc, bannerStyle);
					text = bannerText;
				}
				if (Main.mouseRightRelease && Main.mouseRight && Config.AllowClickActions)
				{
					// since this is called before PostDrawFullscreenMap() resetting this will
					// prevent clearing the housing query AND the current housing banner at the same time
					// which is the normal behavior.
					// It also prevents clearing multiple overlapping banners at the same time (although they get cleared in reverse order. TODO: Fix at some point)
					Main.mouseRightRelease = false;
					WorldGen.kickOut(npcsWithBanners[i]);
					SoundEngine.PlaySound(SoundID.MenuTick);
				}
			}
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
			float hoverFactor = Config!.HoverScale; // hover scale is used for both. I may regret this.
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
