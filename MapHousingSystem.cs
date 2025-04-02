using Microsoft.Xna.Framework.Graphics;
using RemoteNPCHousing.Configs;
using RemoteNPCHousing.Networking;
using RemoteNPCHousing.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.Map;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Gamepad;
using tModPorter;

namespace RemoteNPCHousing;
public class MapHousingSystem : ModSystem
{
	public static MapHousingSystem Instance => ModContent.GetInstance<MapHousingSystem>();

	// this map layer is used simply for compatability with visible
	private static NPCPanelMapLayer MapLayer => ModContent.GetInstance<NPCPanelMapLayer>();

	private GameTime _lastUpdateTime = null!;
	private UserInterface _interface = null!;
	private UIState _state = null!;
	private UIHousingIcon _housingToggle = null!;

	/// <summary>
	/// Determines the minimum requirement to be drawn. MapLayer.Hide() is called if this is not true.
	/// </summary>
	public bool CanUIBeEnabled => Main.mapFullscreen && ClientConfig.Instance.Enable;

	/// <summary>
	/// Determines if anything at all should be drawn based on the MapLayer visibility
	/// </summary>
	public bool IsUIEnabled => MapLayer.Visible && CanUIBeEnabled;

	/// <summary>
	/// Determines if the housing UI is open and housing banners should be drawn
	/// </summary>
	public bool IsHousingOpen => _housingToggle.IsOpen;

	private bool _mapFullscreenCache = false;

	public override void Load()
	{
		if (!Main.dedServ)
		{
			_interface = new UserInterface();
			_state = new UIState();
			_housingToggle = new UIConfiguredHousingIcon();
			_state.Append(_housingToggle);

			_interface.SetState(_state);
		}
	}

	public override void Unload()
	{
		_lastUpdateTime = null!;
		_interface = null!;
		_state = null!;
	}

	public override void UpdateUI(GameTime gameTime)
	{
		bool mapJustOpened = !_mapFullscreenCache && Main.mapFullscreen;
		_mapFullscreenCache = Main.mapFullscreen;
		if (mapJustOpened)
		{
			var panelConfig = ClientConfig.Instance.FullscreenMapOptions.HousingPanelOptions;
			_housingToggle.IsOpen = panelConfig.GetInitialDisplay(_housingToggle.IsOpen, Main.EquipPage == 1);
		}
		Main.LocalPlayer.GetModPlayer<MapPlayer>().TryInvokingQuery();

		if (IsUIEnabled)
		{
			_lastUpdateTime = gameTime;
			
			_interface?.Update(gameTime);
		}
	}

	// Main.mapFullscreen should always be true when this is called
	public void Draw(ref string mouseText)
	{
		bool mapJustOpened = !_mapFullscreenCache && Main.mapFullscreen;
		_mapFullscreenCache = Main.mapFullscreen;

		if (IsUIEnabled)
		{
			// prevents a 1 frame draw glitch. TODO: figure out the order of operations between Update and Draw to avoid redundancy
			if (mapJustOpened)
			{
				var panelConfig = ClientConfig.Instance.FullscreenMapOptions.HousingPanelOptions;
				_housingToggle.IsOpen = panelConfig.GetInitialDisplay(_housingToggle.IsOpen, Main.EquipPage == 1);
			}
			_interface?.Draw(Main.spriteBatch, _lastUpdateTime);
			if (_housingToggle.IsMouseHovering && UIConfiguredHousingIcon.Config.AllowHoverText && _housingToggle.Enabled)
			{
				mouseText = Lang.inter[80].Value; // Housing
			}
			if (IsHousingOpen)
			{
				ChangeMapHeightFromConfig(out int oldValue);
				// this is normally set by DrawInventory()
				// ss far as I know it is always 0.85f when calling DrawNPCHousesInUI() 
				// TODO: configure
				Main.inventoryScale = 0.85f; 
				Main_DrawNPCHousesInUI(Main.instance);
				Main_mH(Main.instance) = oldValue;

				HandleMouseNPC(Main.instance);
			}
		}
	}

	public static void ChangeMapHeightFromConfig(out int old)
	{
		var config = ClientConfig.Instance.FullscreenMapOptions.HousingPanelOptions;

		old = Main_mH(Main.instance);
		int height = config.GetDefaultVerticalOffset(old);
		Main_mH(Main.instance) = height; 
	}

	public override void PreDrawMapIconOverlay(IReadOnlyList<IMapLayer> layers, MapOverlayDrawContext mapOverlayDrawContext)
	{
		foreach (var layer in layers)
		{
			if (layer == ModContent.GetInstance<NPCHousesMapLayer>() && !NPCHousesMapLayer.ShouldDraw()) layer.Hide();
			if (layer == ModContent.GetInstance<NPCPanelMapLayer>() && !CanUIBeEnabled) layer.Hide();
		}
	}

	public override void PostDrawFullscreenMap(ref string mouseText)
	{
		Draw(ref mouseText);
	}

	[UnsafeAccessor(UnsafeAccessorKind.Method, Name = "DrawNPCHousesInUI")]
	internal static extern void Main_DrawNPCHousesInUI(Main self);

	[UnsafeAccessor(UnsafeAccessorKind.StaticField, Name = "mH")]
	internal static extern ref int Main_mH(Main self);

	// Re-implementation of Main.DrawInterface_38_MouseCarriedObject()
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
			float mapScale = Main.mapFullscreenScale / Main.UIScale; // num5
			Vector2 mapOffset = new(10, 10); // num7, num8
			Vector2 mapPos = Main.mapFullscreenPos * mapScale; // num24, num25
			Vector2 topLeftOfMap = (Main.ScreenSize / new Point(2, 2)).ToVector2() - mapPos;
			Vector2 innerTopLeftOfMap = topLeftOfMap + mapOffset * mapScale; // num, num2

			Point tilePos = ((Main.MouseScreen - innerTopLeftOfMap) / mapScale + mapOffset).ToPoint();

			if (tilePos.X < 0 || tilePos.X >= Main.maxTilesX) return;
			if (tilePos.Y < 0 || tilePos.Y >= Main.maxTilesY) return;

			HousingQuery query = new(tilePos.X, tilePos.Y, Main.instance.mouseNPCType, Main.instance.mouseNPCIndex);

			// In single player there is no need to have any delay in the query since all map sections are loaded
			if (Main.netMode == NetmodeID.SinglePlayer)
			{
				query.FullilledCallback();
				return;
			}
			
			SendHousingQuery(query);
		}
	}

	public static void SendHousingQuery(HousingQuery query)
	{
		if (!Main.Map.IsRevealed(query.X, query.Y))
		{
			string text = "This tile is not revealed!";
			Main.NewText(text, byte.MaxValue, 240, 20);
			return;
		}

		if (!Main.sectionManager.TileLoaded(query.X, query.Y) && !ServerConfig.Instance.LoadLoadQueriedChunks)
		{
			// TODO: localize
			string text = "This tile is too far away and not loaded!";
			Main.NewText(text, byte.MaxValue, 240, 20);
			return;
		}

		Main.LocalPlayer.GetModPlayer<MapPlayer>().CurrentQuery = query;
		query.SendToServer();
	}

	public static void LoadTileSections(int tileX, int tileY, int radius)
	{
		int sectionX = Netplay.GetSectionX(tileX);
		int sectionY = Netplay.GetSectionY(tileY);

		for (int i = -radius; i <= radius; ++i)
		{
			for (int j = -radius; j <= radius; ++j)
			{
				int x = (sectionX + i) * Main.sectionWidth;
				int y = (sectionY + j) * Main.sectionHeight;
				LoadTileSection(x, y);
			}
		}
	}

	public static void LoadTileSection(int tileX, int tileY)
	{
		if (!WorldGen.InWorld(tileX, tileY)) return;

		NetworkHandler.SendToServer(MapSectionPacket.FromTile(tileX, tileY), Main.LocalPlayer.whoAmI);
	}
}
