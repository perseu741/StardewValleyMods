using Harmony;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace FishingAutomaton
{
  /// <summary>The mod entry point.</summary>
  public class ModEntry : Mod
  {
    private ModConfig config;
    private HarmonyInstance harmony;
    private Lib.Utils.Logger log;
    private Lib.FishForMe automaton;
    private List<PropertyInfo> configuredButtons = new List<PropertyInfo>();

    /// <summary>
    /// Entry point for the mod.  Sets up logging, Harmony, and configuration, and preps the mod to fish.
    /// </summary>
    /// <param name="helper"></param>
    public override void Entry(IModHelper helper)
    {
      Lib.Utils.Logger.Monitor = this.Monitor;
      log = Lib.Utils.Logger.GetLog();

      log.Silly("Creating main class entry");

      config = helper.ReadConfig<ModConfig>();

      // Do this to speed up processing in handleKeyPress
      log.Debug("Loading buttons that have configuration set");
      foreach (var fi in typeof(ModConfig).GetProperties()) {
        if (fi.Name.EndsWith("Button")) {
          if ((SButton)fi.GetValue(config) != SButton.None) {
            log.Debug($"Adding button {fi.Name} to list");
            configuredButtons.Add(fi);
          }
        }
      }

      if (config.loadHarmony) {
        log.Info("Loading Harmony and patching functions.  If something odd happens, check this mod first");

        Lib.HarmonyHacks.NoSeaweedHack.config = config;
        Lib.HarmonyHacks.NoSeaweedHack.log = log;
        Lib.HarmonyHacks.NoTrashHack.config = config;
        Lib.HarmonyHacks.NoTrashHack.log = log;
        Lib.HarmonyHacks.NoTrashLavaMineHack.config = config;

        // Only one patch file so have it patch everything instead of doing manual thing.
        HarmonyInstance.DEBUG = false;
        harmony = HarmonyInstance.Create(helper.ModRegistry.ModID);
        harmony.PatchAll(Assembly.GetExecutingAssembly());
      }

      log.Silly("Loading event handlers");
      GameEvents.UpdateTick += new EventHandler(OnUpdateTick);
      GameEvents.HalfSecondTick += new EventHandler(OnHalfSecondTick);
      InputEvents.ButtonPressed += new EventHandler<EventArgsInput>(HandleKeyPress);

      automaton = new Lib.FishForMe(helper, config);
      log.Trace("Finished init, ready for operation");
    }

    /// <summary>
    /// Process if a user presses any of our keys.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void HandleKeyPress(object sender, EventArgsInput e)
    {
      foreach (var fi in configuredButtons) {
        if ((SButton)fi.GetValue(config) == e.Button) {
          var pName = fi.Name.Remove(fi.Name.Length - 6);
          var v = typeof(ModConfig).GetProperty(pName);
          v?.SetValue(config, !(bool)v.GetValue(config));
          log.Debug($"Switched value of {pName} to {(bool)v?.GetValue(config)}");
        }
      }
    }

    /// <summary>
    /// Event handler for the update that happens appx 60 times a second.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnUpdateTick(object sender, EventArgs e)
    {
      if (!Context.IsWorldReady || Game1.player == null || !(Game1.player.CurrentTool is FishingRod))
        return;

      automaton.OnUpdate();
    }

    /// <summary>
    /// Does the mod need to do anything on half second?
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnHalfSecondTick(object sender, EventArgs e)
    {
      // Nothing at the moment, but send off to the automaton
      automaton.OnHalfSecondUpdate();
    }
  }
}