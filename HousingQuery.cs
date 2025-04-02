using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace RemoteNPCHousing;

public record HousingQuery(int X, int Y, int MouseNPCType, int MouseNPCIndex)
{
	private uint _startTime;
	private readonly uint _waitTime = 60;

	public int Radius { get; } = 2;

	public bool Fulfilled => TilesLoaded();

	public bool Expired => Main.GameUpdateCount - _startTime > _waitTime;

	public bool Sent { get; private set; }

	public void SendToServer()
	{
		MapHousingSystem.LoadTileSections(X, Y, Radius);
		_startTime = Main.GameUpdateCount;
		Sent = true;
	}

	public bool Invoke()
	{
		if (Expired || Fulfilled)
		{
			FullilledCallback();
			return true;
		}
		return false;
	}

	public void FullilledCallback()
	{
		RemoteNPCHousing.DebugText($"Took {Main.GameUpdateCount - _startTime} ticks to fullfil.");

		if (Main.instance.mouseNPCType != MouseNPCType)
		{
			RemoteNPCHousing.DebugText("mouseNPCTypes do not match housing query aborted.");
			return;
		}

		if (Main.instance.mouseNPCIndex != MouseNPCIndex)
		{
			RemoteNPCHousing.DebugText("mouseNPCIndexes do not match housing query aborted.");
			return;
		}

		if (MouseNPCType == 0)
		{
			if (WorldGen.MoveTownNPC(X, Y, -1))
			{
				Main.NewText(Lang.inter[39].Value, byte.MaxValue, 240, 20);
			}
		}

		else if (MouseNPCIndex >= 0)
		{
			if (WorldGen.MoveTownNPC(X, Y, MouseNPCIndex))
			{
				Main.instance.SetMouseNPC(-1, -1); // does this need to be called first?
				WorldGen.moveRoom(X, Y, MouseNPCIndex);
				SoundEngine.PlaySound(SoundID.MenuTick);
			}
		}
	}

	public bool TileLoaded(int x, int y)
	{
		if (!WorldGen.InWorld(x, y)) return true;
		return Main.sectionManager.TileLoaded(x, y); ;
	}

	public bool TilesLoaded()
	{
		int sectionX = Netplay.GetSectionX(X);
		int sectionY = Netplay.GetSectionY(Y);

		for (int i = -Radius; i <= Radius; ++i)
		{
			for (int j = -Radius; j <= Radius; ++j)
			{
				int x = (sectionX + i) * Main.sectionWidth;
				int y = (sectionY + j) * Main.sectionHeight;
				bool loaded = TileLoaded(x, y);
				if (!loaded) return false;
			}
		}

		return true;
	}
}
