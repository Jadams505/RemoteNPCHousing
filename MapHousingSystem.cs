using Microsoft.Xna.Framework.Graphics;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;

namespace RemoteNPCHousing;
public class MapHousingSystem : ModSystem
{
	public static MapHousingSystem Instance => ModContent.GetInstance<MapHousingSystem>();

	public override void PostDrawFullscreenMap(ref string mouseText)
	{
		Main_DrawNPCHousesInUI(Main.instance);
		//Main_DrawInterface_38_MouseCarriedObject(Main.instance);
		HandleMouseNPC(Main.instance);
	}

	[UnsafeAccessor(UnsafeAccessorKind.Method, Name = "DrawNPCHousesInUI")]
	internal static extern void Main_DrawNPCHousesInUI(Main self);

	[UnsafeAccessor(UnsafeAccessorKind.Method, Name = "DrawInterface_38_MouseCarriedObject")]
	internal static extern void Main_DrawInterface_38_MouseCarriedObject(Main self);

	public static void HandleMouseNPC(Main main)
	{
		if (main.mouseNPCType <= -1) return;

		float scale = 1f;
		scale *= Main.cursorScale;
		if (main.mouseNPCIndex >= 0)
		{
			var npc = Main.npc[main.mouseNPCIndex];
			if (!npc.active || npc.type != main.mouseNPCType)
			{
				// this changes mouseNPCType = 0 and mouseNPCIndex = -1
				main.SetMouseNPC_ToHousingQuery(); 
			}
		}

		int npcHeadIndex = main.mouseNPCIndex >= 0 
			? TownNPCProfiles.GetHeadIndexSafe(Main.npc[main.mouseNPCIndex]) 
			: NPC.TypeToDefaultHeadIndex(main.mouseNPCType);
		Texture2D headTexture = TextureAssets.NpcHead[npcHeadIndex].Value;
		Main.spriteBatch.Draw(headTexture, new Vector2(Main.mouseX + 26f * scale - headTexture.Width * 0.5f * scale, Main.mouseY + 26f * scale - headTexture.Height * 0.5f * scale), null, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);

		// Under what circumstances is this true?
		// When a house query is chosen this will fall through
		if (PlayerInput.IgnoreMouseInterface) return;

		Main.LocalPlayer.mouseInterface = true;
		Main.mouseText = false;
		// mouseRightRelease is never true in the fullscreen map? Fixed with IL
		if (Main.mouseRight && Main.mouseRightRelease)
		{
			SoundEngine.PlaySound(SoundID.MenuTick);
			main.SetMouseNPC(-1, -1);
		}

		// When a house query is chosen mouseLeftRelease = true
		if (Main.mouseLeft && Main.mouseLeftRelease)
		{
			// Values taken from Main.DrawMap()
			float mapScale = Main.mapFullscreenScale; // num5
			Vector2 mapOffset = new(10, 10); // num7, num8
			Vector2 mapPos = Main.mapFullscreenPos * mapScale; // num24, num25
			Vector2 topLeftOfMap = (Main.ScreenSize / new Point(2, 2)).ToVector2() - mapPos;
			Vector2 zoomedTopLeftOfMap = topLeftOfMap + mapOffset * mapScale; // num, num2

			Point tilePos = ((Main.MouseScreen - zoomedTopLeftOfMap) / mapScale + mapOffset).ToPoint();

			if (main.mouseNPCType == 0)
			{
				int x = (int)((Main.mouseX + Main.screenPosition.X) / 16f);
				int y = (int)((Main.mouseY + Main.screenPosition.Y) / 16f);
				if (Main.LocalPlayer.gravDir == -1f)
					y = (int)((Main.screenPosition.Y + Main.screenHeight - Main.mouseY) / 16f);

				Main.NewText($"({tilePos.X}, {tilePos.Y})");

				if (WorldGen.MoveTownNPC(tilePos.X, tilePos.Y, -1))
				{
					Main.NewText(Lang.inter[39].Value, byte.MaxValue, 240, 20);
				}
			}

			else if (main.mouseNPCIndex >= 0)
			{
				int cachedIndex = main.mouseNPCIndex;
				if (WorldGen.MoveTownNPC(tilePos.X, tilePos.Y, cachedIndex))
				{
					main.SetMouseNPC(-1, -1); // does this need to be called first?
					WorldGen.moveRoom(tilePos.X, tilePos.Y, cachedIndex);
					SoundEngine.PlaySound(SoundID.MenuTick);
				}
			}
		}
	}
}
