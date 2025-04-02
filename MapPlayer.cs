using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace RemoteNPCHousing;
public class MapPlayer : ModPlayer
{
	internal HousingQuery? CurrentQuery { get; set; }

	internal bool TryInvokingQuery()
	{
		if (CurrentQuery is null) return true;
		if (CurrentQuery.Invoke())
		{
			CurrentQuery = null;
			return true;
		}
		return false;
	}

	public override void Unload()
	{
		CurrentQuery = null;
	}
}