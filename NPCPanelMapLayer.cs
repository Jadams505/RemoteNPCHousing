using Terraria.Map;
using Terraria.ModLoader;

namespace RemoteNPCHousing;
public class NPCPanelMapLayer : ModMapLayer
{
	public override void Draw(ref MapOverlayDrawContext context, ref string text)
	{
		// draw is handled by MapHousingSystem.PostDrawFullscreenMap()
		// I want to draw above all icons on the map
		// Can be removed when the positioning PR gets merged
	}
}
