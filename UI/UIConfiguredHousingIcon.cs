using RemoteNPCHousing.Configs;
using Terraria;
using Terraria.UI;

namespace RemoteNPCHousing.UI;
public class UIConfiguredHousingIcon : UIHousingIcon
{
	public static HousingIconConfig Config => ClientConfig.Instance.FullscreenMapOptions.HousingIconOptions;

	public UIConfiguredHousingIcon() : base(Config.HousingIconX,
		Config.HousingIconY)
	{
		IsOpen = Config.DisplayOption switch
		{
			IconDisplayOptions.AlwaysShow => true,
			IconDisplayOptions.NeverShow => false,
			_ => Enabled,
		};
	}

	public override void Update(GameTime gameTime)
	{
		Scale = Config.Scale;
		Enabled = Config.DisplayOption switch
		{
			IconDisplayOptions.AlwaysShow => true,
			IconDisplayOptions.NeverShow => false,
			_ => Enabled,
		};

		UpdatePosition();

		base.Update(gameTime);
	}

	protected override void Drag()
	{
		base.Drag();

		Config.HousingIconX = Left.Percent;
		Config.HousingIconY = Top.Percent;
	}

	public override void RightMouseDown(UIMouseEvent evt)
	{
		var parentDim = Parent.GetDimensions().ToRectangle();
		LastMousePercent = Main.MouseScreen / parentDim.Size();

		if (!Config.LockPosition)
		{
			Main.LocalPlayer.mouseInterface = true;
			Config.PositionOption = IconPositionOptions.Custom;
			Dragging = true;
		}
	}

	public void UpdatePosition()
	{
		if (Config.PositionOption == IconPositionOptions.Vanilla)
		{
			var panelConfig = ClientConfig.Instance.FullscreenMapOptions.HousingPanelOptions;
			var defaultPos = Config.DefaultPosition(panelConfig.GetDefaultVerticalOffset(MapHousingSystem.Main_mH(Main.instance)));
			var parentDim = Parent.GetDimensions().ToRectangle().Size();
			Left.Set(0f, defaultPos.X / parentDim.X);
			Top.Set(0f, defaultPos.Y / parentDim.Y);
		}
		else if (Config.PositionOption == IconPositionOptions.Custom)
		{
			Left.Set(0f, Config.HousingIconX);
			Top.Set(0f, Config.HousingIconY);
		}
	}
}
