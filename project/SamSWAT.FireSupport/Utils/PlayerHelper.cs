using Comfort.Common;
using EFT;
using UnityToolkit.Extensions;

namespace SamSWAT.FireSupport.ArysReloaded.Utils;

internal static class PlayerHelper
{
	public static bool IsMainPlayerAlive()
	{
		GameWorld gameWorld = Singleton<GameWorld>.Instance;
		if (gameWorld == null)
		{
			return false;
		}
		
		Player player = gameWorld.MainPlayer;
		return player.OrNull()?.ActiveHealthController.IsAlive == true;
	}
}