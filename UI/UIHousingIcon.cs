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

namespace RemoteNPCHousing.UI;
public class UIHousingIcon : UIElement
{
	public const int HousingClosed = 4;
	public const int HousingOpen = 5;
	public const int HousingHovered = 7;

	public bool IsOpen { get; set; } = false;

	public float Scale { get; set; } = 1f;

	public int SizeX { get; } = 22;
	public int SizeY { get; } = 24;

	protected Vector2 LastMouse { get; set; }
	public bool Dragging { get; protected set; }

	public bool Enabled { get; set; } = true;

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

	public override void Update(GameTime gameTime)
	{
		if (!Enabled) return;

		Width.Set(SizeX * Scale, 0f);
		Height.Set(SizeY * Scale, 0f);

		base.Update(gameTime);

		Drag();
	}

	protected override void DrawSelf(SpriteBatch spriteBatch)
	{
		if (!Enabled) return;

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

	public override void RightMouseDown(UIMouseEvent evt)
	{
		LastMouse = Main.MouseScreen;

		Main.LocalPlayer.mouseInterface = true;
		Dragging = true;

		base.RightMouseDown(evt);
	}

	public override void RightMouseUp(UIMouseEvent evt)
	{
		Dragging = false;

		base.RightMouseUp(evt);
	}

	protected virtual void Drag()
	{
		if (Dragging)
		{
			Vector2 delta = Main.MouseScreen - LastMouse;
			float newX = Left.Pixels + delta.X;
			float newY = Top.Pixels + delta.Y;
			newX = Math.Clamp(newX, 0, Parent.GetDimensions().Width);
			newY = Math.Clamp(newY, 0, Parent.GetDimensions().Height);
			Left.Set(newX, 0f);
			Top.Set(newY, 0f);
			LastMouse = Main.MouseScreen;
		}
	}

	public override void MouseOver(UIMouseEvent evt)
	{
		base.MouseOver(evt);
	}
}
