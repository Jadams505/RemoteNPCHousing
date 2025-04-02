using Microsoft.Xna.Framework.Graphics;
using RemoteNPCHousing.Configs;
using RemoteNPCHousing.Networking;
using RemoteNPCHousing.UI;
using System;
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
			Main.LocalPlayer.GetModPlayer<MapPlayer>().CurrentQuery = query;
			query.SendToServer();

			//if (main.mouseNPCType == 0)
			//{
			//	if (MoveTownNPCWithSectionCheck(tilePos.X, tilePos.Y, -1))
			//	{
			//		Main.NewText(Lang.inter[39].Value, byte.MaxValue, 240, 20);
			//	}
			//}

			//else if (main.mouseNPCIndex >= 0)
			//{
			//	int cachedIndex = main.mouseNPCIndex;
			//	if (MoveTownNPCWithSectionCheck(tilePos.X, tilePos.Y, cachedIndex))
			//	{
			//		main.SetMouseNPC(-1, -1); // does this need to be called first?
			//		WorldGen.moveRoom(tilePos.X, tilePos.Y, cachedIndex);
			//		SoundEngine.PlaySound(SoundID.MenuTick);
			//	}
			//}
		}
	}

	public static bool MoveTownNPCWithSectionCheck(int x, int y, int npcIndex)
	{
		if (!Main.sectionManager.TileLoaded(x, y))
		{
			if (!ServerConfig.Instance.LoadLoadQueriedChunks)
			{
				// TODO: localize
				string text = "This tile is too far away and not loaded!";
				Main.NewText(text, byte.MaxValue, 240, 20);
				return false;
			}

			LoadTileSections(x, y, radius: 2);
		}

		if (!Main.Map.IsRevealed(x, y))
		{
			string text = "This tile is not revealed!";
			Main.NewText(text, byte.MaxValue, 240, 20);
			return false;
		}

		return WorldGen.MoveTownNPC(x, y, npcIndex);
	}

	public static void RevealTileSections(int tileX, int tileY, int radius, bool requireLoaded = false)
	{
		for (int i = -radius; i <= radius; ++i)
		{
			for (int j = -radius; j <= radius; ++j)
			{
				int x = (tileX + radius) * Main.sectionWidth;
				int y = (tileY + radius) * Main.sectionHeight;

				if (requireLoaded && !Main.sectionManager.TileLoaded(x, y)) continue;
				RevealTileSection(x, y);
			}
		}
	}

	public static void RevealTileSection(int tileX, int tileY)
	{
		int x = Netplay.GetSectionX(tileX) * Main.sectionWidth;
		int y = Netplay.GetSectionY(tileY) * Main.sectionHeight;

		for (int i = 0; i < Main.sectionWidth; ++i)
		{
			for (int j = 0; j < Main.sectionHeight; ++j)
			{
				int innerX = x + i;
				int innerY = y + j;
				if (innerX < 0 || innerX > Main.Map.MaxWidth)
					continue;
				if (innerY < 0 || innerY > Main.Map.MaxHeight)
					continue;
				Main.Map.UpdateLighting(innerX, innerY, byte.MaxValue);
			}
		}
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

	public static bool AnyTileRevealedInTileSection(int tileX, int tileY)
	{
		int x = Netplay.GetSectionX(tileX) * Main.sectionWidth;
		int y = Netplay.GetSectionY(tileY) * Main.sectionHeight;

		for (int i = 0; i < Main.sectionWidth; ++i)
		{
			for (int j = 0; j < Main.sectionHeight; ++j)
			{
				int innerX = x + i;
				int innerY = y + j;
				if (innerX < 0 || innerX > Main.Map.MaxWidth) 
					continue;
				if (innerY < 0 || innerY > Main.Map.MaxHeight) 
					continue;
				if (Main.Map.IsRevealed(innerX, innerY)) return true;
			}
		}
		return false;
	}

	public static bool AllTilesRevealedInTileSection(int tileX, int tileY)
	{
		int x = Netplay.GetSectionX(tileX) * Main.sectionWidth;
		int y = Netplay.GetSectionY(tileY) * Main.sectionHeight;

		for (int i = 0; i < Main.sectionWidth; ++i)
		{
			for (int j = 0; j < Main.sectionHeight; ++j)
			{
				int innerX = x + i;
				int innerY = y + j;
				if (innerX < 0 || innerX > Main.Map.MaxWidth)
					continue;
				if (innerY < 0 || innerY > Main.Map.MaxHeight)
					continue;
				if (!Main.Map.IsRevealed(innerX, innerY)) return false;
			}
		}
		return true;
	}
}

public record HousingQuery(int X, int Y, int MouseNPCType, int MouseNPCIndex)
{
	public int Radius { get; } = 2;
	public Rectangle RequestedArea => new()
	{
		X = Math.Max((Netplay.GetSectionX(X) - Radius), 0) * Main.sectionWidth,
		Y = Math.Max((Netplay.GetSectionY(Y) - Radius), 0) * Main.sectionHeight,
		Width = Main.sectionWidth * (2 * Radius + 1),
		Height = Main.sectionHeight * (2 * Radius + 1),
	};

	private uint _startTime;
	private uint _waitTime = 60;

	public void SendToServer()
	{
		MapHousingSystem.LoadTileSections(X, Y, Radius);
		_startTime = Main.GameUpdateCount;
		Sent = true;
	}

	public bool Invoke()
	{
		if (Expired || Fulfilled)
		{
			FullilledCallback();
			return true;
		}
		return false;
	}

	public void FullilledCallback()
	{
		Main.NewText($"Took {Main.GameUpdateCount - _startTime} ticks to fullfil");

		if (Main.instance.mouseNPCType != MouseNPCType)
		{
			Main.NewText("mouseNPCTypes do not match housing query aborted");
			return;
		}

		if (Main.instance.mouseNPCIndex != MouseNPCIndex)
		{
			Main.NewText("mouseNPCIndexes do not match housing query aborted");
			return;
		}

		if (MouseNPCType == 0)
		{
			if (WorldGen.MoveTownNPC(X, Y, -1))
			{
				Main.NewText(Lang.inter[39].Value, byte.MaxValue, 240, 20);
			}
		}

		else if (MouseNPCIndex >= 0)
		{
			if (WorldGen.MoveTownNPC(X, Y, MouseNPCIndex))
			{
				Main.instance.SetMouseNPC(-1, -1); // does this need to be called first?
				WorldGen.moveRoom(X, Y, MouseNPCIndex);
				SoundEngine.PlaySound(SoundID.MenuTick);
			}
		}
	}

	public bool TileLoaded(int x, int y)
	{
		if (!WorldGen.InWorld(x, y)) return true;
		return Main.sectionManager.TileLoaded(x, y); ;
	}

	public bool TilesLoaded()
	{
		int sectionX = Netplay.GetSectionX(X);
		int sectionY = Netplay.GetSectionY(Y);

		for (int i = -Radius; i <= Radius; ++i)
		{
			for (int j = -Radius; j <= Radius; ++j)
			{
				int x = (sectionX + i) * Main.sectionWidth;
				int y = (sectionY + j) * Main.sectionHeight;
				bool loaded = TileLoaded(x, y);
				if (!loaded) return false;
			}
		}

		return true;
	}

	public bool Fulfilled => false && TilesLoaded();

	public bool Expired => Main.GameUpdateCount - _startTime > _waitTime;

	public bool Sent { get; private set; }
}
