using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using UnityEngine;

namespace KongStrong
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class KongStrongPlugin : BaseUnityPlugin
    {
        public const string PluginGUID = "felix.kongstrong";
        public const string PluginName = "Kong Strong Energy";
        // Keep in sync with <Version> in KongStrong.csproj.
        public const string PluginVersion = "1.1.0";

        // Shared by all drinks and the crash, so only one of them can be active at a time.
        private const string StatusCategory = "kongstrong";

        private readonly Drink[] _drinks =
        {
            new Drink
            {
                Key = "SugarFree", PrefabName = "KongStrongSugarFree",
                Duration = 300f, InstantStamina = 40f, StaminaRegenBonus = 50, SpeedBonus = 10,
                RunStaminaReduction = 25, JumpStaminaReduction = 25,
                Ingredients = "Honey:1,Raspberry:3", Amount = 2,
            },
            new Drink
            {
                Key = "Classic", PrefabName = "KongStrongClassic", CausesCrash = true,
                Duration = 180f, InstantStamina = 80f, StaminaRegenBonus = 75, SpeedBonus = 15,
                RunStaminaReduction = 25, JumpStaminaReduction = 25,
                Ingredients = "Honey:2,Raspberry:3", Amount = 2,
            },
            new Drink
            {
                Key = "Skyr", PrefabName = "KongSkyrEnergy",
                BasePrefabs = new[] { "MeadTasty", "MeadStaminaMinor" },
                Duration = 600f, InstantStamina = 20f, StaminaRegenBonus = 25, HealthRegenBonus = 25,
                CarryWeightBonus = 50f,
                Ingredients = "Honey:1,Blueberries:3,DeerMeat:1", Amount = 2,
            },
            new Drink
            {
                Key = "XXL", PrefabName = "KongStrongXXL",
                BasePrefabs = new[] { "MeadStaminaMedium", "MeadStaminaMinor" },
                Weight = 19f, StackSize = 1, ModelScale = 1.8f,
                Duration = 1200f, InstantStamina = 40f, StaminaRegenBonus = 50, SpeedBonus = 10,
                RunStaminaReduction = 25, JumpStaminaReduction = 25,
                Ingredients = "Honey:5,Raspberry:15", Amount = 1,
            },
        };

        private bool _crashEnabled;
        private float _crashDuration;
        private int _crashRegenPenalty;
        private int _crashSpeedPenalty;

        private void Awake()
        {
            foreach (Drink drink in _drinks)
            {
                drink.Bind(Config);
            }

            _crashEnabled = Config.Bind("SugarCrash", "Enabled", true,
                "Urban Classic is followed by a sugar crash when its buff runs out.").Value;
            _crashDuration = Config.Bind("SugarCrash", "DurationSeconds", 60f,
                "How long the sugar crash lasts, in seconds.").Value;
            _crashRegenPenalty = Config.Bind("SugarCrash", "StaminaRegenPenaltyPercent", 25,
                "Slower stamina regeneration during the crash, in percent.").Value;
            _crashSpeedPenalty = Config.Bind("SugarCrash", "MoveSpeedPenaltyPercent", 5,
                "Slower movement during the crash, in percent.").Value;

            AddLocalizations();
            PrefabManager.OnVanillaPrefabsAvailable += AddDrinks;
        }

        private Drink Get(string key) => _drinks.First(d => d.Key == key);

        private static string Minutes(Drink drink) => Mathf.Max(1, Mathf.RoundToInt(drink.Duration / 60f)).ToString();

        private void AddLocalizations()
        {
            Drink sugarFree = Get("SugarFree"), classic = Get("Classic"), skyr = Get("Skyr"), xxl = Get("XXL");
            CustomLocalization loc = LocalizationManager.Instance.GetLocalization();

            loc.AddTranslation("English", new Dictionary<string, string>
            {
                { "item_kong_sugarfree", "Kong Strong Sugar Free" },
                { "item_kong_sugarfree_desc", $"Ice-cold, blue and sugar free. More stamina and speed for {Minutes(sugarFree)} minutes." },
                { "se_kong_sugarfree", "Kong Strong" },
                { "se_kong_sugarfree_tooltip", "Caffeine is rushing through your veins." },
                { "se_kong_sugarfree_start", "KONG STRONG!" },

                { "item_kong_classic", "Kong Strong Urban Classic" },
                { "item_kong_classic_desc", $"The red can, full of sugar. A big kick for {Minutes(classic)} minutes, followed by a sugar crash." },
                { "se_kong_classic", "Sugar Rush" },
                { "se_kong_classic_tooltip", "Sugar and caffeine. This won't end well." },
                { "se_kong_classic_start", "KONG STRONG!" },

                { "item_kong_skyr", "Skyr Energy" },
                { "item_kong_skyr_desc", $"Protein drink with caffeine. Carry more and regenerate faster for {Minutes(skyr)} minutes." },
                { "se_kong_skyr", "Skyr Power" },
                { "se_kong_skyr_tooltip", "27 g of protein. Gains incoming." },
                { "se_kong_skyr_start", "PROTEIN!" },

                { "item_kong_xxl", "Kong Strong XXL (18.9 L)" },
                { "item_kong_xxl_desc", $"18.9 litres of Sugar Free in a canister. Lasts {Minutes(xxl)} minutes. Good luck carrying it." },
                { "se_kong_xxl", "Kong Strong XXL" },
                { "se_kong_xxl_tooltip", "You drank 18.9 litres. Your heart is doing 200 bpm." },
                { "se_kong_xxl_start", "KONG STRONG XXL!" },

                { "se_kong_stop", "The energy rush wears off." },
                { "se_kong_crash", "Sugar Crash" },
                { "se_kong_crash_tooltip", "The sugar is gone and you are tired." },
                { "se_kong_crash_start", "Sugar crash..." },
            });

            loc.AddTranslation("German", new Dictionary<string, string>
            {
                { "item_kong_sugarfree", "Kong Strong Sugar Free" },
                { "item_kong_sugarfree_desc", $"Eiskalt, blau und zuckerfrei. Mehr Ausdauer und Tempo für {Minutes(sugarFree)} Minuten." },
                { "se_kong_sugarfree", "Kong Strong" },
                { "se_kong_sugarfree_tooltip", "Koffein rauscht durch deine Adern." },
                { "se_kong_sugarfree_start", "KONG STRONG!" },

                { "item_kong_classic", "Kong Strong Urban Classic" },
                { "item_kong_classic_desc", $"Die rote Dose mit ordentlich Zucker. Ein starker Kick für {Minutes(classic)} Minuten, danach kommt der Zuckercrash." },
                { "se_kong_classic", "Zuckerschock" },
                { "se_kong_classic_tooltip", "Zucker und Koffein. Das geht nicht lange gut." },
                { "se_kong_classic_start", "KONG STRONG!" },

                { "item_kong_skyr", "Skyr Energy" },
                { "item_kong_skyr_desc", $"Protein-Drink mit Koffein. Mehr Tragkraft und schnellere Regeneration für {Minutes(skyr)} Minuten." },
                { "se_kong_skyr", "Skyr Power" },
                { "se_kong_skyr_tooltip", "27 g Protein. Die Gains kommen." },
                { "se_kong_skyr_start", "PROTEIN!" },

                { "item_kong_xxl", "Kong Strong XXL (18,9 L)" },
                { "item_kong_xxl_desc", $"18,9 Liter Sugar Free im Kanister. Hält {Minutes(xxl)} Minuten. Viel Spaß beim Tragen." },
                { "se_kong_xxl", "Kong Strong XXL" },
                { "se_kong_xxl_tooltip", "Du hast 18,9 Liter getrunken. Dein Herz schlägt mit 200 bpm." },
                { "se_kong_xxl_start", "KONG STRONG XXL!" },

                { "se_kong_stop", "Der Energiekick lässt nach." },
                { "se_kong_crash", "Zuckercrash" },
                { "se_kong_crash_tooltip", "Der Zucker ist alle und du bist müde." },
                { "se_kong_crash_start", "Zuckercrash..." },
            });
        }

        private void AddDrinks()
        {
            PrefabManager.OnVanillaPrefabsAvailable -= AddDrinks;

            bool crashUsed = _crashEnabled && _drinks.Any(d => d.Enabled && d.CausesCrash);
            if (crashUsed)
            {
                try
                {
                    SE_SugarRush.Crash = CreateCrash();
                    ItemManager.Instance.AddStatusEffect(new CustomStatusEffect(SE_SugarRush.Crash, false));
                }
                catch (Exception e)
                {
                    Logger.LogError($"Failed to add the sugar crash: {e}");
                    crashUsed = false;
                }
            }

            foreach (Drink drink in _drinks.Where(d => d.Enabled))
            {
                try
                {
                    AddDrink(drink, crashUsed && drink.CausesCrash);
                }
                catch (Exception e)
                {
                    Logger.LogError($"Failed to add {drink.PrefabName}: {e}");
                }
            }
        }

        private void AddDrink(Drink drink, bool crashAfter)
        {
            string basePrefab = drink.BasePrefabs.FirstOrDefault(n => PrefabManager.Instance.GetPrefab(n) != null);
            if (basePrefab == null)
            {
                Logger.LogError($"None of the base prefabs ({string.Join(", ", drink.BasePrefabs)}) exist, {drink.PrefabName} not added.");
                return;
            }

            Sprite icon = LoadIcon(drink.IconResource);
            StatusEffect baseEffect = PrefabManager.Instance.GetPrefab(basePrefab)
                .GetComponent<ItemDrop>().m_itemData.m_shared.m_consumeStatusEffect;

            SE_Stats effect = crashAfter
                ? ScriptableObject.CreateInstance<SE_SugarRush>()
                : ScriptableObject.CreateInstance<SE_Stats>();
            effect.name = "SE_" + drink.PrefabName;
            effect.m_name = $"$se_{drink.Token}";
            effect.m_tooltip = $"$se_{drink.Token}_tooltip";
            effect.m_category = StatusCategory;
            effect.m_icon = icon;
            effect.m_ttl = drink.Duration;
            effect.m_startMessageType = MessageHud.MessageType.Center;
            effect.m_startMessage = $"$se_{drink.Token}_start";
            if (!crashAfter)
            {
                effect.m_stopMessageType = MessageHud.MessageType.TopLeft;
                effect.m_stopMessage = "$se_kong_stop";
            }

            effect.m_staminaUpFront = drink.InstantStamina;
            effect.m_staminaRegenMultiplier = 1f + drink.StaminaRegenBonus / 100f;
            effect.m_healthRegenMultiplier = 1f + drink.HealthRegenBonus / 100f;
            effect.m_speedModifier = drink.SpeedBonus / 100f;
            effect.m_runStaminaDrainModifier = -drink.RunStaminaReduction / 100f;
            effect.m_jumpStaminaUseModifier = -drink.JumpStaminaReduction / 100f;
            effect.m_addMaxCarryWeight = drink.CarryWeightBonus;

            // Reuse the drinking sound and particles of the mead we cloned.
            if (baseEffect != null)
            {
                effect.m_startEffects = baseEffect.m_startEffects;
                effect.m_stopEffects = baseEffect.m_stopEffects;
            }

            ItemManager.Instance.AddStatusEffect(new CustomStatusEffect(effect, false));

            var itemConfig = new ItemConfig
            {
                Name = $"$item_{drink.Token}",
                Description = $"$item_{drink.Token}_desc",
                CraftingStation = CraftingStations.GetInternalName(drink.CraftingStation),
                Amount = Mathf.Max(1, drink.Amount),
                Requirements = ParseRequirements(drink.Ingredients),
                Icon = icon,
                Weight = drink.Weight,
                StackSize = drink.StackSize,
            };

            var item = new CustomItem(drink.PrefabName, basePrefab, itemConfig);
            item.ItemDrop.m_itemData.m_shared.m_consumeStatusEffect = effect;
            item.ItemPrefab.transform.localScale *= drink.ModelScale;
            ItemManager.Instance.AddItem(item);

            Logger.LogInfo($"Added {drink.PrefabName} (cloned from {basePrefab}).");
        }

        private SE_Stats CreateCrash()
        {
            var crash = ScriptableObject.CreateInstance<SE_Stats>();
            crash.name = "SE_KongSugarCrash";
            crash.m_name = "$se_kong_crash";
            crash.m_tooltip = "$se_kong_crash_tooltip";
            crash.m_category = StatusCategory;
            crash.m_icon = LoadIcon("KongStrong.crash.png");
            crash.m_ttl = _crashDuration;
            crash.m_startMessageType = MessageHud.MessageType.Center;
            crash.m_startMessage = "$se_kong_crash_start";
            crash.m_staminaRegenMultiplier = Mathf.Max(0f, 1f - _crashRegenPenalty / 100f);
            crash.m_speedModifier = -_crashSpeedPenalty / 100f;
            return crash;
        }

        private RequirementConfig[] ParseRequirements(string value)
        {
            var requirements = new List<RequirementConfig>();
            foreach (string entry in value.Split(','))
            {
                string[] parts = entry.Split(':');
                string prefab = parts[0].Trim();
                if (prefab.Length == 0)
                {
                    continue;
                }

                int amount = 1;
                if (parts.Length > 1 && !int.TryParse(parts[1].Trim(), out amount))
                {
                    Logger.LogWarning($"Invalid amount in ingredient '{entry}', using 1.");
                    amount = 1;
                }

                requirements.Add(new RequirementConfig(prefab, Mathf.Max(1, amount), 0, true));
            }

            return requirements.ToArray();
        }

        private static Sprite LoadIcon(string resource)
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource))
            using (var buffer = new MemoryStream())
            {
                stream.CopyTo(buffer);
                Texture2D texture = AssetUtils.LoadImage(buffer.ToArray());
                return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            }
        }
    }
}
