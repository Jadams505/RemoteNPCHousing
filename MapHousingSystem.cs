using Microsoft.Xna.Framework.Graphics;
using RemoteNPCHousing.Configs;
using RemoteNPCHousing.UI;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Gamepad;
using tModPorter;

namespace RemoteNPCHousing;
public class MapHousingSystem : ModSystem
{
	public static MapHousingSystem Instance => ModContent.GetInstance<MapHousingSystem>();

	private GameTime _lastUpdateTime = null!;
	private UserInterface _interface = null!;
	private UIState _state = null!;
	private UIHousingIcon _housingToggle = null!;

	/// <summary>
	/// Determines if anything at all should be drawn
	/// </summary>
	public bool IsUIEnabled { get; set; } = true;

	/// <summary>
	/// Determines if the housing UI is open and housing banners should be drawn
	/// </summary>
	public bool IsHousingOpen => _housingToggle.IsOpen;

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
		IsUIEnabled = Main.mapFullscreen;
		if (IsUIEnabled)
		{
			_lastUpdateTime = gameTime;
			_interface?.Update(gameTime);
		}
	}

	public void Draw(ref string mouseText)
	{
		if (IsUIEnabled)
		{
			_interface?.Draw(Main.spriteBatch, _lastUpdateTime);
			if (_housingToggle.IsMouseHovering && UIConfiguredHousingIcon.Config.AllowHoverText)
			{
				mouseText = Lang.inter[80].Value; // Housing
			}
			if (IsHousingOpen)
			{
				ChangeMapHeightFromConfig(out int oldValue);
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
			float mapScale = Main.mapFullscreenScale; // num5
			Vector2 mapOffset = new(10, 10); // num7, num8
			Vector2 mapPos = Main.mapFullscreenPos * mapScale; // num24, num25
			Vector2 topLeftOfMap = (Main.ScreenSize / new Point(2, 2)).ToVector2() - mapPos;
			Vector2 zoomedTopLeftOfMap = topLeftOfMap + mapOffset * mapScale; // num, num2

			Point tilePos = ((Main.MouseScreen - zoomedTopLeftOfMap) / mapScale + mapOffset).ToPoint();

			if (main.mouseNPCType == 0)
			{
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
