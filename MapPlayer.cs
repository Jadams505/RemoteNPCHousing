using RemoteNPCHousing.Configs;
using RemoteNPCHousing.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RemoteNPCHousing;
public class MapPlayer : ModPlayer
{
	// A HousingQuery is cached on the player in multiplayer so that when all the maps sections load it
	// can be fullfilled. This will always be null in singleplayer.
	internal HousingQuery? CurrentQuery { get; set; }

	internal bool TryInvokingQuery()
	{
		if (CurrentQuery is null) return true;
		if (CurrentQuery.Invoke())
		{
			CurrentQuery = null;
			return true;
		}
		return false;
	}

	// TODO: Is this the best hook for this?
	public override void PreUpdate()
	{
		// Tiles are not constantly synced in multiplayer, so in order to know where to place the banner
		// you have to ask the server. An alternative would be to just use npcHome, but that's kind of ugly
		if (Main.netMode == NetmodeID.MultiplayerClient && Main.GameUpdateCount % 60 == 0 && ServerConfig.Instance.LoadNPCHomeChunks)
		{
			foreach (var npcId in NPCHousesMapLayer.NpcsWithBanners())
			{
				var npc = Main.npc[npcId];
				int homeX = npc.homeTileX;
				int homeY = npc.homeTileY;

				int sectionX = Netplay.GetSectionX(homeX);
				int sectionY = Netplay.GetSectionY(homeY);

				for (int i = sectionY; i >= 0 && i < Main.maxSectionsY; --i)
				{
					if (!Main.sectionManager.SectionLoaded(sectionX, i))
					{
						NetworkHandler.SendToServer(MapSectionPacket.FromSection(sectionX, i), Main.LocalPlayer.whoAmI);
					}
				}
			}
		}
	}
}