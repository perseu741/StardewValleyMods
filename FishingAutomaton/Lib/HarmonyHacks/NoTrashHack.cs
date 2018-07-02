using Harmony;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace FishingAutomaton.Lib.HarmonyHacks
{
  [HarmonyPatch(typeof(GameLocation), "getFish", new Type[] { typeof(float), typeof(int), typeof(int), typeof(Farmer), typeof(double), typeof(string) })]
  class NoTrashHack
  {
    public static ModConfig config;
    public static Utils.Logger log;

    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> SkipTheTrash(ILGenerator generator, MethodBase methodBase, IEnumerable<CodeInstruction> instructions)
    {
      log.Info("Starting IL Harmony injection for skipping trash");
      List<CodeInstruction> allInst = instructions.ToList<CodeInstruction>();

      bool done = false;
      bool foundChanceVar = false;

      if (!config.noTrash) {
        log.Info("Config option is false, skipping Harmony injection");
        foreach (var ci in instructions) { yield return ci; }
      }
      else {
        log.Info("Harmony injecting IL into getFish(float, int, int, Farmer, double, string) in GameLocation.cs");
        for (int i = 0; i < allInst.Count; ++i) {
          CodeInstruction ci = allInst[i];

          log.Silly($"CI is |{ci}| with operand type |{ci?.operand?.GetType()}|");
          if (!done) {
            if (!foundChanceVar) {
              if (ci.opcode == OpCodes.Ldloc_S && ci.operand is LocalBuilder && (ci.operand as LocalBuilder).LocalIndex == 17) {
                log.Silly("Found local variable 'chance' load");
                foundChanceVar = true;
              }
            }
            else {
              // We've found chance, check for branch if greater than.
              if (ci.opcode == OpCodes.Bgt_Un) {
                // We're done!
                log.Debug("Found the branch after loading chance.  Removing branch");
                // We could do something here like adding a label jumping to self, or nop'ing the last four ops, but let's just
                // pop off the values as the BGT would have and move on.
                yield return new CodeInstruction(OpCodes.Pop);
                ci.opcode = OpCodes.Pop;
                done = true;
              }
              else {
                // we did not find the right location
                foundChanceVar = false;
              }
            }
          }
          log.Silly($"Sending on the code {ci.opcode}");
          yield return ci;
        }
      }
    }
  }
}
