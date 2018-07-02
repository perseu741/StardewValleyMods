using Harmony;
using StardewValley;
using StardewValley.Locations;
using System;

namespace FishingAutomaton.Lib.HarmonyHacks
{
  [HarmonyPatch(typeof(MineShaft), "getFish", new Type[] { typeof(float), typeof(int), typeof(int), typeof(Farmer), typeof(double) })]
  class NoTrashLavaMineHack
  {
    public static ModConfig config;

    [HarmonyPostfix]
    static void CheckFish(ref StardewValley.Object __result, MineShaft __instance,
                          float millisecondsAfterNibble, int bait, int waterDepth, Farmer who, double baitPotency)
    {
      if ((bool)config?.noTrash) {
        while (__instance.getMineArea(-1) == 80 && (__result.ParentSheetIndex >= 167 && __result.ParentSheetIndex < 173)) {
          __result = __instance.getFish(millisecondsAfterNibble, bait, waterDepth, who, baitPotency);
        }
      }
    }
  }
}
