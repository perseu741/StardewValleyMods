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
  class NoSeaweedHack
  {
    public static ModConfig config;
    public static Utils.Logger log;

    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> SkipTheSeaweed(ILGenerator generator, MethodBase methodBase, IEnumerable<CodeInstruction> instructions)
    {
      log.Info("Starting IL Harmony injection for skipping seaweed");
      List<CodeInstruction> allInst = instructions.ToList<CodeInstruction>();

      bool foundSetFish = false;
      bool foundGetFish = false;
      bool done = false;
      bool didInjection = false;

      Label branchTarget = generator.DefineLabel();
      Label innerLoopLabel = new Label();
      int branchTargetIndex = -1;
      LocalBuilder loadLocalFish = null;

      if (!config.noSeaweed) {
        log.Info("Config option is false, skipping Harmony injection");
        foreach (var ci in instructions) { yield return ci; }
      }
      else {
        log.Info("Harmony injecting IL into getFish(float, int, int, Farmer, double, string) in GameLocation.cs");
        for (int i = 0; i < allInst.Count; ++i) {
          CodeInstruction ci = allInst[i];

          log.Silly($"CI is |{ci}| with operand type |{ci?.operand?.GetType()}|");
          if (!done) {
            if (i == branchTargetIndex) {
              log.Silly("Reached index point of target");
              allInst[i].labels.Add(branchTarget);

              // This should be our last operation.
              done = true;
            }

            if (!foundSetFish) {
              // Check if this is a branch and save the jump point.
              if (ci.opcode == OpCodes.Br) {
                log.Silly($"Found a branch point to {ci.operand}, saving.");
                innerLoopLabel = (Label)ci.operand;
              }
              // Check if this opcode is setting the specific fish variable.
              if (ci.opcode == OpCodes.Stloc_S && ci.operand is LocalBuilder && ((LocalBuilder)ci.operand).LocalIndex == 13) {
                log.Silly("Found the specificFishData OpCode");
                foundSetFish = true;
              }
            }
            else {
              if (!foundGetFish) {
                // Setfish was true here, but get fish is false.  The next opcode must be get fish or we start over.
                if (ci.opcode == OpCodes.Ldloc_S && ci.operand is LocalBuilder && ((LocalBuilder)ci.operand).LocalIndex == 13) {
                  log.Silly("Found the loading specificFishData OPCode");
                  loadLocalFish = ci.operand as LocalBuilder;
                  foundGetFish = true;
                }
                else {
                  log.Silly("Did not find load specific after finding set.  Restarting.");
                  foundSetFish = false;
                }
              }
              else if (!didInjection) {

                log.Debug("We have found both opcodes, first find the end of our loop to inject label");
                // At this point, innerLoopLabel points to the end of the loop
                branchTargetIndex = FindEndOfLoopTarget(allInst, innerLoopLabel, i);

                log.Silly("Injecting codes");
                foreach (var newCode in newCodes) {
                  if (newCode.opcode == OpCodes.Beq) {
                    log.Silly("Found the branch, setting operand to new branch point.");
                    newCode.operand = branchTarget;
                  }
                  if (newCode.opcode == OpCodes.Ldloc_S) {
                    log.Silly("Found the load local opcode, setting operand to local builder");
                    newCode.operand = loadLocalFish;
                  }
                  log.Silly($"Injecting |{newCode}| with operand type |{newCode?.operand?.GetType()}|");
                  yield return newCode;
                }
                didInjection = true;
              }
            }
          }
          log.Silly($"Sending on the code {ci.opcode}");
          yield return ci;
        }
      }
    }

    private static int FindEndOfLoopTarget(List<CodeInstruction> insts, Label innerLoopLabel, int startAt)
    {
      log.Silly($"Finding next reference of label {innerLoopLabel.ToString()}");
      for (int i = startAt; i < insts.Count; ++i) {
        foreach (Label l in insts[i].labels) {
          if (l.Equals(innerLoopLabel)) {
            log.Silly("Found a label match");
            // This is kind of hackish, we know the loop increment is four opcodes back
            if ((new[] { OpCodes.Ldloc, OpCodes.Ldloc_0, OpCodes.Ldloc_1, OpCodes.Ldloc_2, OpCodes.Ldloc_3, OpCodes.Ldloc_S }).Contains(insts[i - 4].opcode)) {
              return i - 4;
            }
          }
        }
      }
      return -1;
    }

    private static readonly CodeInstruction[] newCodes = new CodeInstruction[] { 
      new CodeInstruction(OpCodes.Ldc_I4_1),
      new CodeInstruction(OpCodes.Ldelem_Ref),
      new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Convert), "ToInt32", new Type[] { typeof(string) })),
      new CodeInstruction(OpCodes.Ldc_I4_5),
      new CodeInstruction(OpCodes.Beq),
      new CodeInstruction(OpCodes.Ldloc_S)
    };
  }
}
