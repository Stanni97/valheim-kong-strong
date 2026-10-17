using BepInEx.Configuration;
using Jotunn.Configs;

namespace KongStrong
{
    /// <summary>
    /// One drink variant. Field initialisers are the defaults; <see cref="Bind"/> replaces them
    /// with the values from the config file.
    /// </summary>
    internal sealed class Drink
    {
        public string Key;                 // config section, localization and icon name
        public string PrefabName;
        public string[] BasePrefabs = { "MeadStaminaMinor", "MeadStaminaMedium", "MeadTasty" };
        public float Weight = 0.5f;
        public int StackSize = 20;
        public float ModelScale = 1f;
        public bool CausesCrash;

        public bool Enabled = true;
        public float Duration = 300f;
        public float InstantStamina;
        public int StaminaRegenBonus;
        public int HealthRegenBonus;
        public int SpeedBonus;
        public int RunStaminaReduction;
        public int JumpStaminaReduction;
        public float CarryWeightBonus;
        public string CraftingStation = nameof(CraftingStations.Workbench);
        public string Ingredients = "";
        public int Amount = 1;

        public string Token => "kong_" + Key.ToLowerInvariant();
        public string IconResource => $"KongStrong.{Key.ToLowerInvariant()}.png";

        public void Bind(ConfigFile config)
        {
            string s = Key;
            Enabled = config.Bind(s, "Enabled", Enabled, "Adds this drink to the game.").Value;
            Duration = config.Bind(s, "DurationSeconds", Duration, "How long the buff lasts, in seconds.").Value;
            InstantStamina = config.Bind(s, "InstantStamina", InstantStamina, "Stamina restored immediately when drinking.").Value;
            StaminaRegenBonus = config.Bind(s, "StaminaRegenBonusPercent", StaminaRegenBonus, "Faster stamina regeneration, in percent.").Value;
            HealthRegenBonus = config.Bind(s, "HealthRegenBonusPercent", HealthRegenBonus, "Faster health regeneration, in percent.").Value;
            SpeedBonus = config.Bind(s, "MoveSpeedBonusPercent", SpeedBonus, "Faster movement, in percent.").Value;
            RunStaminaReduction = config.Bind(s, "RunStaminaReductionPercent", RunStaminaReduction, "Less stamina used while running, in percent.").Value;
            JumpStaminaReduction = config.Bind(s, "JumpStaminaReductionPercent", JumpStaminaReduction, "Less stamina used per jump, in percent.").Value;
            CarryWeightBonus = config.Bind(s, "CarryWeightBonus", CarryWeightBonus, "Extra carry weight.").Value;
            CraftingStation = config.Bind(s, "CraftingStation", CraftingStation,
                new ConfigDescription("Where the drink is crafted.", CraftingStations.GetAcceptableValueList())).Value;
            Ingredients = config.Bind(s, "Ingredients", Ingredients, "Comma separated list of ItemPrefab:Amount.").Value;
            Amount = config.Bind(s, "Amount", Amount, "Drinks produced per craft.").Value;
        }
    }
}
