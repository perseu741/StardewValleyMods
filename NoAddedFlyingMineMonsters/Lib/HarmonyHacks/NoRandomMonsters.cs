using Harmony;
using StardewValley.Locations;

namespace NoAddedFlyingMineMonsters.Lib.HarmonyHacks
{
  [HarmonyPatch(typeof(MineShaft), "spawnFlyingMonsterOffScreen")]
  class NoRandomMonsters
  {
    public static ModConfig config = null;

    [HarmonyPrefix]
    static bool AllowMonsters()
    {
      if (config == null) return true;
      else return !config.noRandomMonsters;
    }
  }
}
