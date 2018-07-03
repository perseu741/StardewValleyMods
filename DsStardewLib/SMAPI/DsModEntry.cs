using DsStardewLib.Utils;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace DsStardewLib.SMAPI
{
  public class DsModHelper<TConfig> where TConfig : class, new()
  {
    private Logger log;
    private TConfig config;
    private  List<PropertyInfo> configuredButtons = new List<PropertyInfo>();

    public Logger Log { get => log; private set => log = value; }
    public TConfig Config { get => config; private set => config = value; }

    /// <summary>
    /// Entry point for the mod.  Sets up logging, Harmony, and configuration, and preps the mod to fish.
    /// </summary>
    /// <param name="helper"></param>
    public void Init(IModHelper helper, IMonitor monitor)
    {
      Logger.Monitor = monitor;
      Log = Logger.GetLog();

      Log.Silly("Creating base class entry");

      Config = helper.ReadConfig<TConfig>();

      // Do this to speed up processing in handleKeyPress
      Log.Debug("Loading buttons that have configuration set");
      foreach (var fi in typeof(TConfig).GetProperties()) {
        if (fi.Name.EndsWith("Button")) {
          if ((SButton)fi.GetValue(Config) != SButton.None) {
            Log.Debug($"Adding button {fi.Name} to list");
            configuredButtons.Add(fi);
          }
        }
      }

      Log.Silly("Loading event handlers");
      InputEvents.ButtonPressed += new EventHandler<EventArgsInput>(HandleKeyPress);
      
      Log.Trace("Finished init, ready for operation");
    }

    /// <summary>
    /// Process if a user presses any of our keys.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void HandleKeyPress(object sender, EventArgsInput e)
    {
      foreach (var fi in configuredButtons) {
        if ((SButton)fi.GetValue(Config) == e.Button) {
          var pName = fi.Name.Remove(fi.Name.Length - 6);
          var v = typeof(TConfig).GetProperty(pName);
          v?.SetValue(Config, !(bool)v.GetValue(Config));
          Log.Debug($"Switched value of {pName} to {(bool)v?.GetValue(Config)}");
        }
      }
    }
  }
}
