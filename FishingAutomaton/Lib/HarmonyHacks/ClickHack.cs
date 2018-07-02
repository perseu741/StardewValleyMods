using Harmony;
using StardewValley;

namespace FishingAutomaton.Lib.HarmonyHacks
{
  /// <summary>
  /// Harmony class to patch the 'didPlayerJustClickAtAll' function
  /// </summary>
  [HarmonyPatch(typeof(Game1), "didPlayerJustClickAtAll")]
  class ClickAtAllHack
  {
    /// <summary>
    /// Whether the function should simulate the user clicking.
    /// </summary>
    public static bool simulateClick = false;
 
    /// <summary>
    /// A Harmony Postfix function that forces the didPlayerJustClickAtAll function
    /// to return true if the member variable <code>simulateclick</code> is set
    /// to true.
    /// </summary>
    /// <param name="__result">Harmony will inject this reference to the original return value</param>
    [HarmonyPostfix]
    static void YesPlayerDidClick(ref bool __result)
    {
      if (simulateClick) {
        __result = true;
      }
    }
  }

  /// <summary>
  /// Harmony class to patch the 'isOneOfTheseKeysDown' function.  This is specifically needed
  /// to simulate the bobber bar in the fishing minigame being controlled by an actual user.
  /// The check for the useTool button is to prevent other things (like that chat box) from
  /// also being activated at the same time as the fishing game.
  /// </summary>
  [HarmonyPatch(typeof(Game1), "isOneOfTheseKeysDown")]
  class IsButtonDownHack
  {
    /// <summary>
    /// Whether the function should simulate the user holding a button down.
    /// </summary>
    public static bool simulateDown = false;

    /// <summary>
    /// A Harmony Postfix function that forces the isOneOfTheseKeysDown function
    /// to return true if the member variable <code>simulateDown</code> is set
    /// to true and the key being checked is the use tool button.
    /// </summary>
    /// <param name="__result">Harmony will inject this reference to the original return value</param>
    [HarmonyPostfix]
    static void YesButtonIsDown(ref bool __result, InputButton[] keys)
    {
      if (simulateDown) {
        foreach (InputButton key in keys) {
          foreach (InputButton key2 in Game1.options.useToolButton) {
            if (key.key == key2.key) {
              __result = true;
              return;
            }
          }
        }
      }
    }
  }
}
