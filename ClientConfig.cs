using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader.Config;

namespace RemoteNPCHousing;
public class ClientConfig : ModConfig
{
	public override ConfigScope Mode => ConfigScope.ClientSide;
}
