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
		// If I draw it here I have to account for UIScale or restart the spritebatch
		// TODO: move MapHousingSystem into this layer
		// TODO: figure out how to make this work
		//MapHousingSystem.Instance.Draw(ref text);
	}
}
