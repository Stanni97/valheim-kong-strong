using BepInEx.Configuration;
using Jotunn.Configs;

namespace KongStrong
{
    /// <summary>The Skyr cup: real food for one of the three food slots.</summary>
    internal sealed class SkyrCupSettings
    {
        private const string Section = "SkyrCup";

        public bool Enabled = true;
        public float Health = 50f;
        public float Stamina = 20f;
        public float Eitr;
        public float Duration = 1800f;
        public float Regen = 3f;
        public string CraftingStation = nameof(CraftingStations.Cauldron);
        public string Ingredients = "ChickenEgg:2,Cloudberry:2,Honey:1";
        public int Amount = 2;

        public void Bind(ConfigFile config)
        {
            Enabled = config.Bind(Section, "Enabled", Enabled, ServerSync.Desc("Can be made. Off: the recipe is gone.")).Value;
            Health = config.Bind(Section, "Health", Health, ServerSync.Desc("Health the food gives.")).Value;
            Stamina = config.Bind(Section, "Stamina", Stamina, ServerSync.Desc("Stamina the food gives.")).Value;
            Eitr = config.Bind(Section, "Eitr", Eitr, ServerSync.Desc("Eitr the food gives.")).Value;
            Duration = config.Bind(Section, "DurationSeconds", Duration, ServerSync.Desc("How long the food lasts, in seconds.")).Value;
            Regen = config.Bind(Section, "HealthRegen", Regen, ServerSync.Desc("Health regeneration per tick.")).Value;
            CraftingStation = config.Bind(Section, "CraftingStation", CraftingStation,
                ServerSync.Desc("Where the cup is made.", CraftingStations.GetAcceptableValueList())).Value;
            Ingredients = config.Bind(Section, "Ingredients", Ingredients, ServerSync.Desc("Comma separated list of ItemPrefab:Amount.")).Value;
            Amount = config.Bind(Section, "Amount", Amount, ServerSync.Desc("Cups produced per craft.")).Value;
        }
    }
}
