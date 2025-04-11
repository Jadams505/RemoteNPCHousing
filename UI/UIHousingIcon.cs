using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
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

	protected Vector2 LastMousePercent { get; set; }
	public bool Dragging { get; protected set; }

	public bool Enabled { get; set; } = true;

	public UIHousingIcon(float x, float y)
	{
		Width.Set(SizeX * Scale, 0f);
		Height.Set(SizeY * Scale, 0f);
		Left.Set(0f, x);
		Top.Set(0f, y);

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

		var parentDim = Parent.GetDimensions();
		var selfDim = GetDimensions();

		Left.Set(0f, Math.Clamp(Left.Percent, 0, 1f - (selfDim.Width / parentDim.Width)));
		Top.Set(0f, Math.Clamp(Top.Percent, 0, 1f - (selfDim.Height / parentDim.Height)));

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
		LastMousePercent = Main.MouseScreen / Parent.GetDimensions().ToRectangle().Size();

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
			var parentDim = Parent.GetDimensions().ToRectangle();
			var selfDim = GetDimensions().ToRectangle();
			Vector2 currentMousePercent = Main.MouseScreen / parentDim.Size();
			Vector2 delta = currentMousePercent - LastMousePercent;
			float newX = Left.Percent + delta.X;
			float newY = Top.Percent + delta.Y;
			newX = Math.Clamp(newX, 0, 1f - ((float)selfDim.Width / parentDim.Width));
			newY = Math.Clamp(newY, 0, 1f - ((float)selfDim.Height / parentDim.Height));
			Left.Set(0, newX);
			Top.Set(0, newY);
			LastMousePercent = currentMousePercent;
		}
	}

	public override void MouseOver(UIMouseEvent evt)
	{
		base.MouseOver(evt);
	}
}
