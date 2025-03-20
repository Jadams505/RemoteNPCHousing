using RemoteNPCHousing.Configs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.UI;

namespace RemoteNPCHousing.UI;
public class UIConfiguredHousingIcon : UIHousingIcon
{
	public static HousingIconConfig Config => ClientConfig.Instance.FullscreenMapOptions.HousingIconOptions;

	public UIConfiguredHousingIcon() : base(Config.HousingIconPosX,
		Config.HousingIconPosY)
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

		Config.HousingIconPosX = (int)Left.Pixels;
		Config.HousingIconPosY = (int)Top.Pixels;
	}

	public override void RightMouseDown(UIMouseEvent evt)
	{
		LastMouse = Main.MouseScreen;

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
			var defaultPos = Config.DefaultPosition();
			Left.Set(defaultPos.X, 0f);
			Top.Set(defaultPos.Y, 0f);
		}
		else if (Config.PositionOption == IconPositionOptions.Custom)
		{
			Left.Set(Config.HousingIconPosX, 0f);
			Top.Set(Config.HousingIconPosY, 0f);
		}
	}
}
