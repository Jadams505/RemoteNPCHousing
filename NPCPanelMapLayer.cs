using Terraria.Map;
using Terraria.ModLoader;

namespace RemoteNPCHousing;
public class NPCPanelMapLayer : ModMapLayer
{
	public override Position GetDefaultPosition()
	{
		return AfterLastVanillaLayer;
	}

	public override void Draw(ref MapOverlayDrawContext context, ref string text)
	{
		// TODO: move MapHousingSystem into this layer
		MapHousingSystem.Instance.Draw(ref text);
	}
}
