using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using HarmonyLib;
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
        public const string PluginVersion = "1.2.0";

        // Shared by all drinks, the crash and heart racing, so only one of them can be active at a time.
        private const string StatusCategory = "kongstrong";
        private const string EmptyPrefab = "KongLeergut";

        private static readonly HashSet<string> ReturnsEmpty = new HashSet<string>();
        private static bool _emptiesEnabled = true;

        private readonly Drink[] _drinks =
        {
            new Drink
            {
                Key = "SugarFree", PrefabName = "KongStrongSugarFree",
                Duration = 300f, InstantStamina = 40f, StaminaRegenBonus = 50, SpeedBonus = 10,
                RunStaminaReduction = 25, JumpStaminaReduction = 25,
                Ingredients = "Blueberries:3,Thistle:1,MushroomYellow:1", Amount = 2,
                Previous = { ["Ingredients"] = new[] { "Honey:1,Raspberry:3" }, ["CraftingStation"] = new[] { "Workbench" } },
            },
            new Drink
            {
                Key = "Classic", PrefabName = "KongStrongClassic", CausesCrash = true,
                Duration = 180f, InstantStamina = 80f, StaminaRegenBonus = 75, SpeedBonus = 15,
                RunStaminaReduction = 25, JumpStaminaReduction = 25,
                Ingredients = "Honey:2,Raspberry:3,Bloodbag:1,MushroomYellow:1", Amount = 2,
                Previous = { ["Ingredients"] = new[] { "Honey:2,Raspberry:3" }, ["CraftingStation"] = new[] { "Workbench" } },
            },
            new Drink
            {
                Key = "Skyr", PrefabName = "KongSkyrEnergy",
                BasePrefabs = new[] { "MeadTasty", "MeadStaminaMinor" },
                Duration = 600f, InstantStamina = 20f, StaminaRegenBonus = 25, HealthRegenBonus = 25,
                CarryWeightBonus = 50f,
                Ingredients = "ChickenEgg:1,Honey:1,Blueberries:2", Amount = 2,
                Previous = { ["Ingredients"] = new[] { "Honey:1,Blueberries:3,DeerMeat:1" } },
            },
            new Drink
            {
                Key = "XXL", PrefabName = "KongStrongXXL",
                BasePrefabs = new[] { "MeadStaminaMedium", "MeadStaminaMinor" },
                Weight = 19f, StackSize = 1, ModelScale = 1.8f, ReturnsEmpty = false,
                FireResistance = HitData.DamageModifier.Resistant,
                Duration = 1200f, InstantStamina = 40f, StaminaRegenBonus = 50, SpeedBonus = 10,
                RunStaminaReduction = 25, JumpStaminaReduction = 25,
                Ingredients = "Vineberry:6,MushroomSmokePuff:3,MushroomYellow:3", Amount = 1,
                Previous = { ["Ingredients"] = new[] { "Honey:5,Raspberry:15" }, ["CraftingStation"] = new[] { "Workbench" } },
            },
            new Drink
            {
                Key = "Frost", PrefabName = "KongStrongFrost",
                FrostResistance = HitData.DamageModifier.Resistant,
                Duration = 600f, InstantStamina = 30f, StaminaRegenBonus = 25, SpeedBonus = 5,
                RunStaminaReduction = 15, JumpStaminaReduction = 15,
                Ingredients = "Blueberries:3,FreezeGland:1,MushroomYellow:1", Amount = 2,
            },
            new Drink
            {
                Key = "Toxic", PrefabName = "KongStrongToxic",
                PoisonResistance = HitData.DamageModifier.Resistant,
                Duration = 600f, InstantStamina = 30f, StaminaRegenBonus = 25, SpeedBonus = 5,
                RunStaminaReduction = 15, JumpStaminaReduction = 15,
                Ingredients = "Blueberries:2,Ooze:1,Thistle:1,MushroomYellow:1", Amount = 2,
            },
            new Drink
            {
                Key = "Eitr", PrefabName = "KongStrongEitr",
                Duration = 600f, InstantStamina = 20f, InstantEitr = 25f, StaminaRegenBonus = 25, EitrRegenBonus = 50,
                Ingredients = "Sap:1,MushroomMagecap:1,MushroomYellow:1,Blueberries:2", Amount = 2,
            },
            new Drink
            {
                Key = "MetBombe", PrefabName = "KongMetBombe", Fermented = true,
                BasePrefabs = new[] { "MeadTasty", "MeadStaminaMinor" }, FermenterBase = "MeadBaseTasty",
                Weight = 1f, StackSize = 10, ReturnsEmpty = false,
                Duration = 300f, HealOverTime = 50f, StaminaRegenBonus = 50, SpeedBonus = 10,
                RunStaminaReduction = 25, JumpStaminaReduction = 25,
                Ingredients = "Honey:5,KongStrongSugarFree:2,Raspberry:5", Amount = 1, FermenterYield = 6,
            },
        };

        private readonly SkyrCupSettings _skyrCup = new SkyrCupSettings();

        private bool _crashEnabled;
        private float _crashDuration;
        private int _crashRegenPenalty;
        private int _crashSpeedPenalty;

        private bool _heartEnabled;
        private float _heartDuration;
        private int _heartHealthPenalty;
        private int _heartStaminaPenalty;

        private bool _dumpPrefabs;
        private bool _shout;
        private float _shoutVolume;
        private string _shoutFile;
        private GameObject _shoutSfx;

        /// <summary>Prefab name of the empty an item leaves behind, or null.</summary>
        internal static string EmptyFor(string itemToken)
        {
            return _emptiesEnabled && ReturnsEmpty.Contains(itemToken) ? EmptyPrefab : null;
        }

        private void Awake()
        {
            foreach (Drink drink in _drinks)
            {
                drink.Bind(Config);
            }

            _skyrCup.Bind(Config);
            BindGeneral();

            Texts.Register(Get, _skyrCup);
            Harmony.CreateAndPatchAll(typeof(Patches), PluginGUID);
            PrefabManager.OnVanillaPrefabsAvailable += AddContent;
        }

        private void BindGeneral()
        {
            Patches.TooAwakeToSleep = Config.Bind("General", "TooAwakeToSleep", true,
                "Beds refuse to let you sleep while a Kong buff is active.").Value;
            _emptiesEnabled = Config.Bind("General", "Empties", true,
                "Drinks leave an empty (Leergut) behind, for the deposit machine and the can pyramid.").Value;

            _crashEnabled = Config.Bind("SugarCrash", "Enabled", true,
                "Urban Classic is followed by a sugar crash when its buff runs out.").Value;
            _crashDuration = Config.Bind("SugarCrash", "DurationSeconds", 60f,
                "How long the sugar crash lasts, in seconds.").Value;
            _crashRegenPenalty = Config.Bind("SugarCrash", "StaminaRegenPenaltyPercent", 25,
                "Slower stamina regeneration during the crash, in percent.").Value;
            _crashSpeedPenalty = Config.Bind("SugarCrash", "MoveSpeedPenaltyPercent", 5,
                "Slower movement during the crash, in percent.").Value;

            ToleranceSettings t = SE_Kong.Tolerance;
            t.Enabled = Config.Bind("CaffeineTolerance", "Enabled", true,
                "Drinks taken shortly after each other work less.").Value;
            t.WindowMinutes = Config.Bind("CaffeineTolerance", "WindowMinutes", 15f,
                "Drinks within this many minutes count as drunk in a row.").Value;
            t.SecondDrinkPercent = Config.Bind("CaffeineTolerance", "SecondDrinkEffectPercent", 50,
                "Strength of the second drink in a row, in percent.").Value;
            t.FurtherDrinkPercent = Config.Bind("CaffeineTolerance", "FurtherDrinkEffectPercent", 25,
                "Strength of the third and every further drink in a row, in percent.").Value;
            t.HeartRacingFromDrink = Config.Bind("CaffeineTolerance", "HeartRacingFromDrink", 3,
                "From this drink in a row on you get heart racing. 0 turns it off.").Value;

            _heartEnabled = t.HeartRacingFromDrink > 0;
            _heartDuration = Config.Bind("HeartRacing", "DurationSeconds", 120f,
                "How long heart racing lasts, in seconds.").Value;
            _heartHealthPenalty = Config.Bind("HeartRacing", "HealthRegenPenaltyPercent", 50,
                "Slower health regeneration during heart racing, in percent.").Value;
            _heartStaminaPenalty = Config.Bind("HeartRacing", "StaminaRegenPenaltyPercent", 25,
                "Slower stamina regeneration during heart racing, in percent.").Value;

            _shout = Config.Bind("Sound", "Shout", false,
                "Every Kong drink is followed by a shouted \"D K!\".").Value;
            SE_Kong.PowerAnimation = Config.Bind("General", "PowerAnimation", true,
                "Drinking Kong raises your arms like activating a Forsaken power, with Eikthyr's glow.").Value;
            _shoutVolume = Config.Bind("Sound", "ShoutVolume", 0.8f,
                new BepInEx.Configuration.ConfigDescription("Volume of the shout.", new BepInEx.Configuration.AcceptableValueRange<float>(0f, 1f))).Value;
            _shoutFile = Config.Bind("Sound", "CustomShoutFile", "KongStrong/shout.wav",
                "Your own 16 bit PCM WAV instead of the built-in shout, relative to BepInEx/config or absolute. " +
                "Only played on this PC; the mod never distributes it.").Value;

            _dumpPrefabs = Config.Bind("Debug", "DumpPrefabs", false,
                "Logs the structure of some vanilla prefabs once at startup (development aid).").Value;
        }

        private Drink Get(string key) => _drinks.First(d => d.Key == key);

        private void AddContent()
        {
            PrefabManager.OnVanillaPrefabsAvailable -= AddContent;

            if (_dumpPrefabs)
            {
                PrefabDump.Dump(Logger, new[]
                {
                    "MeadStaminaMinor", "MeadStaminaMedium", "MeadTasty", "MeadBaseTasty", "QueensJam", "Coins",
                    "piece_chest_wood", "piece_chest", "piece_workbench", "piece_table", "piece_banner01",
                    "piece_bench01", "barrell", "fermenter", "piece_cauldron",
                });
                PrefabDump.ExportIcons(Logger, new[]
                {
                    "MeadStaminaMinor", "MeadTasty", "MeadBaseTasty", "MeadFrostResist", "MeadPoisonResist", "MeadEitrMinor",
                    "QueensJam", "CarrotSoup", "Coins", "Blueberries", "Honey", "Raspberry", "FreezeGland", "Bronze",
                    "piece_chest_wood", "fermenter", "piece_cauldron",
                });
            }

            Try("the sugar crash", () =>
            {
                if (_crashEnabled && _drinks.Any(d => d.Enabled && d.CausesCrash))
                {
                    SE_Kong.Crash = AddDebuff("SE_KongSugarCrash", "se_kong_crash", "crash", _crashDuration,
                        1f - _crashRegenPenalty / 100f, 1f, -_crashSpeedPenalty / 100f);
                }
            });

            Try("heart racing", () =>
            {
                if (_heartEnabled && SE_Kong.Tolerance.Enabled)
                {
                    SE_Kong.HeartRacing = AddDebuff("SE_KongHeartRacing", "se_kong_heartracing", "heartracing", _heartDuration,
                        1f - _heartStaminaPenalty / 100f, 1f - _heartHealthPenalty / 100f, 0f);
                }
            });

            Try("the power effect", () =>
            {
                // Visuals of Eikthyr's power, without its sound.
                StatusEffect eikthyr = PrefabManager.Cache.GetPrefab<StatusEffect>("GP_Eikthyr");
                EffectList.EffectData[] visuals = eikthyr?.m_startEffects?.m_effectPrefabs?
                    .Where(e => e.m_prefab != null && e.m_prefab.GetComponentInChildren<ZSFX>(true) == null).ToArray();
                if (visuals != null && visuals.Length > 0)
                {
                    SE_Kong.PowerEffects = new EffectList { m_effectPrefabs = visuals };
                    Logger.LogInfo("Power effect: " + string.Join(", ", visuals.Select(v => v.m_prefab.name)));
                }
            });

            Try(EmptyPrefab, AddEmpty);

            foreach (Drink drink in _drinks.Where(d => d.Enabled))
            {
                Try(drink.PrefabName, () => AddDrink(drink));
            }

            if (_skyrCup.Enabled)
            {
                Try("KongSkyrCup", AddSkyrCup);
            }
        }

        private void Try(string what, Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Logger.LogError($"Failed to add {what}: {e}");
            }
        }

        private StatusEffect AddDebuff(string name, string token, string icon, float duration,
                                       float staminaRegen, float healthRegen, float speed)
        {
            var effect = ScriptableObject.CreateInstance<SE_Stats>();
            effect.name = name;
            effect.m_name = "$" + token;
            effect.m_tooltip = effect.m_name + "_tooltip";
            effect.m_category = StatusCategory;
            effect.m_icon = Icons.Get(icon);
            effect.m_ttl = duration;
            effect.m_startMessageType = MessageHud.MessageType.Center;
            effect.m_startMessage = effect.m_name + "_start";
            effect.m_staminaRegenMultiplier = Mathf.Max(0f, staminaRegen);
            effect.m_healthRegenMultiplier = Mathf.Max(0f, healthRegen);
            effect.m_speedModifier = speed;
            ItemManager.Instance.AddStatusEffect(new CustomStatusEffect(effect, false));
            return effect;
        }

        private void AddDrink(Drink drink)
        {
            string basePrefab = drink.BasePrefabs.FirstOrDefault(n => PrefabManager.Instance.GetPrefab(n) != null);
            if (basePrefab == null)
            {
                Logger.LogError($"None of the base prefabs ({string.Join(", ", drink.BasePrefabs)}) exist, {drink.PrefabName} not added.");
                return;
            }

            Sprite icon = Icons.Get(drink.Key.ToLowerInvariant());
            StatusEffect baseEffect = PrefabManager.Instance.GetPrefab(basePrefab)
                .GetComponent<ItemDrop>().m_itemData.m_shared.m_consumeStatusEffect;

            SE_Kong effect = CreateEffect(drink, icon, baseEffect);
            ItemManager.Instance.AddStatusEffect(new CustomStatusEffect(effect, false));

            var itemConfig = new ItemConfig
            {
                Name = $"$item_{drink.Token}",
                Description = $"$item_{drink.Token}_desc",
                Icon = icon,
                Weight = drink.Weight,
                StackSize = drink.StackSize,
            };
            if (!drink.Fermented)
            {
                itemConfig.CraftingStation = CraftingStations.GetInternalName(drink.CraftingStation);
                itemConfig.Amount = Mathf.Max(1, drink.Amount);
                itemConfig.Requirements = ParseRequirements(drink.PrefabName, drink.Ingredients);
            }

            var item = new CustomItem(drink.PrefabName, basePrefab, itemConfig);
            item.ItemDrop.m_itemData.m_shared.m_consumeStatusEffect = effect;
            item.ItemPrefab.transform.localScale *= drink.ModelScale;
            ItemManager.Instance.AddItem(item);

            if (drink.ReturnsEmpty)
            {
                ReturnsEmpty.Add(itemConfig.Name);
            }

            if (drink.Fermented)
            {
                AddFermenterBase(drink);
            }

            Logger.LogInfo($"Added {drink.PrefabName} (cloned from {basePrefab}).");
        }

        private SE_Kong CreateEffect(Drink drink, Sprite icon, StatusEffect baseEffect)
        {
            var effect = ScriptableObject.CreateInstance<SE_Kong>();
            bool crashAfter = drink.CausesCrash && SE_Kong.Crash != null;
            effect.CausesCrash = crashAfter;
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
            effect.m_eitrUpFront = drink.InstantEitr;
            if (drink.HealOverTime > 0f)
            {
                effect.m_healthOverTime = drink.HealOverTime;
                effect.m_healthOverTimeDuration = 10f;
                effect.m_healthOverTimeInterval = 1f;
            }

            effect.m_staminaRegenMultiplier = 1f + drink.StaminaRegenBonus / 100f;
            effect.m_healthRegenMultiplier = 1f + drink.HealthRegenBonus / 100f;
            effect.m_eitrRegenMultiplier = 1f + drink.EitrRegenBonus / 100f;
            effect.m_speedModifier = drink.SpeedBonus / 100f;
            effect.m_runStaminaDrainModifier = -drink.RunStaminaReduction / 100f;
            effect.m_jumpStaminaUseModifier = -drink.JumpStaminaReduction / 100f;
            effect.m_addMaxCarryWeight = drink.CarryWeightBonus;

            var mods = new List<HitData.DamageModPair>();
            AddResistance(mods, HitData.DamageType.Fire, drink.FireResistance);
            AddResistance(mods, HitData.DamageType.Frost, drink.FrostResistance);
            AddResistance(mods, HitData.DamageType.Poison, drink.PoisonResistance);
            effect.m_mods = mods;

            // Reuse the drinking sound and particles of the mead we cloned, plus the shout.
            if (baseEffect != null)
            {
                if (_shout && _shoutSfx == null)
                {
                    Try("the shout", () => _shoutSfx = Sounds.CreateSfx("sfx_kong_dk", LoadShout(),
                        baseEffect.m_startEffects, _shoutVolume, 0.35f));
                }

                effect.m_startEffects = _shout ? Sounds.With(baseEffect.m_startEffects, _shoutSfx) : baseEffect.m_startEffects;
                effect.m_stopEffects = baseEffect.m_stopEffects;
            }

            return effect;
        }

        private AudioClip LoadShout()
        {
            string path = string.IsNullOrEmpty(_shoutFile) ? null
                : System.IO.Path.IsPathRooted(_shoutFile) ? _shoutFile : System.IO.Path.Combine(BepInEx.Paths.ConfigPath, _shoutFile);
            if (path != null && System.IO.File.Exists(path))
            {
                try
                {
                    AudioClip custom = Sounds.LoadWavFile(path);
                    Logger.LogInfo($"Using your own shout from {path}.");
                    return custom;
                }
                catch (Exception e)
                {
                    Logger.LogWarning($"Could not read {path} ({e.Message}), using the built-in shout.");
                }
            }

            return Sounds.LoadWav("dk");
        }

        private static void AddResistance(List<HitData.DamageModPair> mods, HitData.DamageType type, HitData.DamageModifier modifier)
        {
            if (modifier != HitData.DamageModifier.Normal)
            {
                mods.Add(new HitData.DamageModPair { m_type = type, m_modifier = modifier });
            }
        }

        private void AddFermenterBase(Drink drink)
        {
            var baseConfig = new ItemConfig
            {
                Name = $"$item_{drink.Token}_base",
                Description = $"$item_{drink.Token}_base_desc",
                Icon = Icons.Get(drink.Key.ToLowerInvariant() + "_base"),
                CraftingStation = CraftingStations.GetInternalName(drink.CraftingStation),
                Amount = Mathf.Max(1, drink.Amount),
                Requirements = ParseRequirements(drink.BasePrefabName, drink.Ingredients),
            };
            ItemManager.Instance.AddItem(new CustomItem(drink.BasePrefabName, drink.FermenterBase, baseConfig));

            ItemManager.Instance.AddItemConversion(new CustomItemConversion(new FermenterConversionConfig
            {
                FromItem = drink.BasePrefabName,
                ToItem = drink.PrefabName,
                ProducedItems = Mathf.Max(1, drink.FermenterYield),
            }));
        }

        private void AddEmpty()
        {
            var config = new ItemConfig
            {
                Name = "$item_kong_empty",
                Description = "$item_kong_empty_desc",
                Icon = Icons.Get("empty"),
                Weight = 0.1f,
                StackSize = 50,
            };
            var item = new CustomItem(EmptyPrefab, "MeadStaminaMinor", config);
            ItemDrop.ItemData.SharedData shared = item.ItemDrop.m_itemData.m_shared;
            shared.m_itemType = ItemDrop.ItemData.ItemType.Material;
            shared.m_consumeStatusEffect = null;
            shared.m_food = shared.m_foodStamina = shared.m_foodEitr = 0f;
            ItemManager.Instance.AddItem(item);
        }

        private void AddSkyrCup()
        {
            var config = new ItemConfig
            {
                Name = "$item_kong_skyrcup",
                Description = "$item_kong_skyrcup_desc",
                Icon = Icons.Get("skyrcup"),
                CraftingStation = CraftingStations.GetInternalName(_skyrCup.CraftingStation),
                Amount = Mathf.Max(1, _skyrCup.Amount),
                Requirements = ParseRequirements("KongSkyrCup", _skyrCup.Ingredients),
                StackSize = 10,
            };
            var item = new CustomItem("KongSkyrCup", "QueensJam", config);
            ItemDrop.ItemData.SharedData shared = item.ItemDrop.m_itemData.m_shared;
            shared.m_itemType = ItemDrop.ItemData.ItemType.Consumable;
            shared.m_consumeStatusEffect = null;
            shared.m_food = _skyrCup.Health;
            shared.m_foodStamina = _skyrCup.Stamina;
            shared.m_foodEitr = _skyrCup.Eitr;
            shared.m_foodBurnTime = _skyrCup.Duration;
            shared.m_foodRegen = _skyrCup.Regen;
            ItemManager.Instance.AddItem(item);
            Logger.LogInfo("Added KongSkyrCup (cloned from QueensJam).");
        }

        private RequirementConfig[] ParseRequirements(string owner, string value)
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

                // Kept anyway, it might be an item another mod registers later.
                if (PrefabManager.Instance.GetPrefab(prefab) == null)
                {
                    Logger.LogWarning($"{owner}: ingredient '{prefab}' was not found, check the prefab name.");
                }

                requirements.Add(new RequirementConfig(prefab, Mathf.Max(1, amount), 0, true));
            }

            return requirements.ToArray();
        }
    }
}
