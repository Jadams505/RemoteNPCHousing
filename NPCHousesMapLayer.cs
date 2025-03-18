using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
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
		if (Main.instance.mouseNPCType == -1) return;

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

			int npcHomeY = npc.homeTileY - 1; // num3
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
			SpriteFrame frame = new(2, 2)
			{
				PaddingX = 2,
				PaddingY = 2,
			};
			if (occupantBanners.Contains(npc.type)) frame = frame with { CurrentColumn = 1 };
			if (npc.housingCategory > 0) frame = frame with { CurrentRow = 1 };

			Vector2 position = new()
			{
				X = npc.homeTileX,
				Y = npcHomeY + 2 // what is wrong with the math
			};
			context.Draw(
				texture: bannerTexture,
				position: position,
				color: Color.White,
				frame: frame,
				scaleIfNotSelected: 1f,
				scaleIfSelected: 1.25f,
				alignment: Alignment.Center
			);
			
		}
	}
}
