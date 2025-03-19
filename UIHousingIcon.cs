using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace RemoteNPCHousing;
public class UIHousingIcon : UIElement
{
	public static HousingIconConfig Config => ClientConfig.Instance.HousingIconOptions;
	public const int HousingClosed = 4;
	public const int HousingOpen = 5;
	public const int HousingHovered = 7;

	public bool IsOpen { get; set; } = false;

	public float Scale => Config.Scale;

	public int SizeX { get; } = 22;
	public int SizeY { get; } = 24;

	public UIHousingIcon(int x, int y)
	{
		Width.Set(SizeX * Scale, 0f);
		Height.Set(SizeY * Scale, 0f);
		Left.Set(x, 0f);
		Top.Set(y, 0f);

		// I have no idea why I have to do this, but if I put this in LeftClick
		// all input gets disabled for some reason
		OnLeftClick += (@event, element) =>
		{
			IsOpen = !IsOpen;
		};
	}

	public UIHousingIcon() : this(Config.HousingIconPosX,
		Config.HousingIconPosY)
	{
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);

		if (Config.UseDefaultPosition)
		{
			var defaultPos = HousingIconConfig.DefaultPosition();
			Left.Set(defaultPos.X, 0f);
			Top.Set(defaultPos.Y, 0f);
		}

		Width.Set(TextureAssets.EquipPage[HousingClosed].Size().X * Scale, 0f);
		Height.Set(TextureAssets.EquipPage[HousingClosed].Size().Y * Scale, 0f);
	}

	protected override void DrawSelf(SpriteBatch spriteBatch)
	{
		base.DrawSelf(spriteBatch);

		var dims = GetDimensions();
		if (IsMouseHovering)
		{
			Main.spriteBatch.Draw(TextureAssets.EquipPage[HousingHovered].Value, dims.Position(), null,
				Main.OurFavoriteColor, 0f, new Vector2(2f), Scale, SpriteEffects.None, 0f);
		}
		var texture = IsOpen ? TextureAssets.EquipPage[HousingOpen] : TextureAssets.EquipPage[HousingClosed];
		Main.spriteBatch.Draw(texture.Value, dims.Position(), null, Color.White, 0f, Vector2.Zero, Scale, SpriteEffects.None, 0f);
	}

	public override void MouseOver(UIMouseEvent evt)
	{
		base.MouseOver(evt);
	}
}
