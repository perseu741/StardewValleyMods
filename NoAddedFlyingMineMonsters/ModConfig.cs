using StardewModdingAPI;

namespace NoAddedFlyingMineMonsters
{
  class ModConfig
  {
    // Test if notes go in the config file.
    public bool noRandomMonsters { get; set; } = true;
    public SButton noRandomMonstersButton { get; set; } = SButton.None;

    public bool loadHarmony { get; set; } = true;
  }
}
