﻿using Microsoft.Xna.Framework.Graphics;
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
	public override void Draw(ref MapOverlayDrawContext context, ref string text)
	{
		// find a better condition later
		if (false && Main.instance.mouseNPCType == -1) return;

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

			int roomateCount = 0;
			for(int j = i + 1; j < npcsWithBanners.Count; ++j)
			{
				var roomate = Main.npc[npcsWithBanners[j]];
				if (roomate.homeTileX == npc.homeTileX && roomate.homeTileY == npc.homeTileY)
					roomateCount++;
			}

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

			Vector2 position = new()
			{
				X = npc.homeTileX,
				Y = npcHomeY + 2 // what is wrong with the math
			};
			float normalScale = 1f;
			float hoverScale = 1.25f;
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
				string bannerText = Lang.GetNPCHouseBannerText(npc, bannerStyle);
				//MouseText(bannerText, 0, 0);
				text = bannerText;
				if (/*Main.mouseRightRelease && */Main.mouseRight)
				{
					//Main.mouseRightRelease = false;
					WorldGen.kickOut(npcsWithBanners[i]);
					SoundEngine.PlaySound(SoundID.MenuTick);
				}
			}
		}
	}
}
