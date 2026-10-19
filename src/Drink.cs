using System.Collections.Generic;
using System.Linq;
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

        // Fermented drinks are not crafted themselves: the recipe makes a base for the fermenter.
        public bool Fermented;
        public string FermenterBase = "MeadBaseTasty";
        public int FermenterYield = 6;

        /// <summary>Defaults of older versions per config key, see <see cref="ConfigBinder"/>.</summary>
        public Dictionary<string, string[]> Previous = new Dictionary<string, string[]>();

        public bool Enabled = true;
        public float Duration = 300f;
        public float InstantStamina;
        public float InstantEitr;
        public float HealOverTime;
        public int StaminaRegenBonus;
        public int HealthRegenBonus;
        public int EitrRegenBonus;
        public int SpeedBonus;
        public int RunStaminaReduction;
        public int JumpStaminaReduction;
        public float CarryWeightBonus;
        public HitData.DamageModifier FireResistance = HitData.DamageModifier.Normal;
        public HitData.DamageModifier FrostResistance = HitData.DamageModifier.Normal;
        public HitData.DamageModifier PoisonResistance = HitData.DamageModifier.Normal;
        public bool ReturnsEmpty = true;
        public string CraftingStation = nameof(CraftingStations.Cauldron);
        public string Ingredients = "";
        public int Amount = 1;

        public string Token => "kong_" + Key.ToLowerInvariant();
        public string IconResource => $"KongStrong.{Key.ToLowerInvariant()}.png";
        public string BasePrefabName => PrefabName + "Base";

        public void Bind(ConfigFile config)
        {
            Enabled = B(config, "Enabled", Enabled, "Adds this drink to the game.");
            Duration = B(config, "DurationSeconds", Duration, "How long the buff lasts, in seconds.");
            InstantStamina = B(config, "InstantStamina", InstantStamina, "Stamina restored immediately when drinking.");
            InstantEitr = B(config, "InstantEitr", InstantEitr, "Eitr restored immediately when drinking.");
            HealOverTime = B(config, "HealOverTime", HealOverTime, "Health restored over the first 10 seconds.");
            StaminaRegenBonus = B(config, "StaminaRegenBonusPercent", StaminaRegenBonus, "Faster stamina regeneration, in percent.");
            HealthRegenBonus = B(config, "HealthRegenBonusPercent", HealthRegenBonus, "Faster health regeneration, in percent.");
            EitrRegenBonus = B(config, "EitrRegenBonusPercent", EitrRegenBonus, "Faster eitr regeneration, in percent.");
            SpeedBonus = B(config, "MoveSpeedBonusPercent", SpeedBonus, "Faster movement, in percent.");
            RunStaminaReduction = B(config, "RunStaminaReductionPercent", RunStaminaReduction, "Less stamina used while running, in percent.");
            JumpStaminaReduction = B(config, "JumpStaminaReductionPercent", JumpStaminaReduction, "Less stamina used per jump, in percent.");
            CarryWeightBonus = B(config, "CarryWeightBonus", CarryWeightBonus, "Extra carry weight.");
            FireResistance = B(config, "FireResistance", FireResistance,
                "Fire damage while the buff is active. Normal = no change, Resistant = half damage.");
            FrostResistance = B(config, "FrostResistance", FrostResistance,
                "Frost damage while the buff is active. Resistant or better also prevents freezing.");
            PoisonResistance = B(config, "PoisonResistance", PoisonResistance,
                "Poison damage while the buff is active.");
            ReturnsEmpty = B(config, "ReturnsEmpty", ReturnsEmpty, "Drinking leaves an empty can or bottle (Leergut) in the inventory.");
            CraftingStation = ConfigBinder.Bind(config, Key, "CraftingStation", CraftingStation,
                new ConfigDescription(Fermented ? "Where the fermenter base is crafted." : "Where the drink is crafted.",
                    CraftingStations.GetAcceptableValueList()), Old("CraftingStation"));
            Ingredients = ConfigBinder.Bind(config, Key, "Ingredients", Ingredients,
                "Comma separated list of ItemPrefab:Amount" + (Fermented ? " for the fermenter base." : "."), Old("Ingredients"));
            Amount = B(config, "Amount", Amount, Fermented ? "Fermenter bases produced per craft." : "Drinks produced per craft.");
            if (Fermented)
            {
                FermenterYield = B(config, "FermenterYield", FermenterYield, "Drinks one base turns into in the fermenter.");
            }
        }

        private T B<T>(ConfigFile config, string key, T value, string description)
        {
            return ConfigBinder.Bind(config, Key, key, value, description);
        }

        private IEnumerable<string> Old(string key)
        {
            return Previous.TryGetValue(key, out string[] values) ? values : Enumerable.Empty<string>();
        }
    }
}
