global using Microsoft.Xna.Framework;
using MonoMod.Cil;
using RemoteNPCHousing.Networking;
using System.IO;
using Terraria;
using Terraria.ModLoader;

namespace RemoteNPCHousing
{
	// Please read https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Modding-Guide#mod-skeleton-contents for more information about the various files in a mod.
	public class RemoteNPCHousing : Mod
	{
		public static RemoteNPCHousing Instance => ModContent.GetInstance<RemoteNPCHousing>();

		public override void Load()
		{
			IL_Main.DoDraw += IL_Main_DoDraw;
		}

		public override void HandlePacket(BinaryReader reader, int whoAmI)
		{
			NetworkHandler.HandlePackets(reader, whoAmI);
		}

		private void IL_Main_DoDraw(ILContext il)
		{
			// MonoModHooks.DumpIL(this, il);
			var cursor = new ILCursor(il);

			bool foundMapDraw = cursor.TryGotoNext(MoveType.After,
				x => x.MatchCall(typeof(TimeLogger), nameof(TimeLogger.MapDrawTime)),
				x => x.MatchCall(typeof(TimeLogger), nameof(TimeLogger.EndDrawFrame))
			);

			if (!foundMapDraw)
			{
				Logger.Warn($"{nameof(foundMapDraw)} failed. Aborting IL edits");
				return;
			}

			/*
				IL_15f7: ldsfld System.Boolean Terraria.Main::mouseLeft
				IL_15fc: brfalse.s IL_1608
				IL_1601: ldc.i4.0
				IL_1602: stsfld System.Boolean Terraria.Main::mouseLeftRelease
				IL_1607: ret
				IL_1608: ldc.i4.1
				IL_1609: stsfld System.Boolean Terraria.Main::mouseLeftRelease
				IL_160e: ret
			 */
			bool foundResetMouseLeft = cursor.TryGotoNext(MoveType.Before,
				x => x.MatchLdsfld(typeof(Main), nameof(Main.mouseLeft)),
				x => x.MatchBrfalse(out _),
				x => x.MatchLdcI4(out _),
				x => x.MatchStsfld(typeof(Main), nameof(Main.mouseLeftRelease)),
				x => x.MatchRet(),
				x => x.MatchLdcI4(out _),
				x => x.MatchStsfld(typeof(Main), nameof(Main.mouseLeftRelease)),
				x => x.MatchRet()
			);

			if (!foundResetMouseLeft)
			{
				Logger.Warn($"{nameof(foundResetMouseLeft)} failed. Aborting IL edits");
				return;
			}

			// mouseRightRelease is not reset when using the fullscreen map
			// presumably since vanilla doesn't use the RMB for anything
			cursor.EmitDelegate(ResetMouseRight);
		}

		private static void ResetMouseRight()
		{
			Main.mouseRightRelease = !Main.mouseRight;
		}
	}
}
