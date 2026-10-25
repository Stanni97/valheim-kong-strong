using System;
using System.Collections;
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
    // On a server with this mod its gameplay values apply to everyone; without a server each player's own config counts.
    [SynchronizationMode(AdminOnlyStrictness.IfOnServer)]
    internal class KongStrongPlugin : BaseUnityPlugin
    {
        public const string PluginGUID = "felix.kongstrong";
        public const string PluginName = "Kong Strong Energy";
        // Keep in sync with <Version> in KongStrong.csproj.
        public const string PluginVersion = "1.3.0";

        // Shared by all drinks, the crash and heart racing, so only one of them can be active at a time.
        private const string StatusCategory = "kongstrong";
        private const string EmptyPrefab = "KongLeergut";

        private static readonly HashSet<string> ReturnsEmpty = new HashSet<string>();
        private static bool _emptiesEnabled = true;

        private readonly Drink[] _drinks =
        {
            new Drink
            {
                Key = "SugarFree", PrefabName = "KongStrongSugarFree", Texture = "tex_can_sugarfree",
                Duration = 300f, InstantStamina = 40f, StaminaRegenBonus = 50, SpeedBonus = 10,
                RunStaminaReduction = 25, JumpStaminaReduction = 25,
                Ingredients = "Blueberries:3,Thistle:1,MushroomYellow:1", Amount = 2,
                Previous = { ["Ingredients"] = new[] { "Honey:1,Raspberry:3" }, ["CraftingStation"] = new[] { "Workbench" } },
            },
            new Drink
            {
                Key = "Classic", PrefabName = "KongStrongClassic", CausesCrash = true, Texture = "tex_can_classic",
                Duration = 180f, InstantStamina = 80f, StaminaRegenBonus = 75, SpeedBonus = 15,
                RunStaminaReduction = 25, JumpStaminaReduction = 25,
                Ingredients = "Honey:2,Raspberry:3,Bloodbag:1,MushroomYellow:1", Amount = 2,
                Previous = { ["Ingredients"] = new[] { "Honey:2,Raspberry:3" }, ["CraftingStation"] = new[] { "Workbench" } },
            },
            new Drink
            {
                Key = "Skyr", PrefabName = "KongSkyrEnergy", Model = "skyr", Shout = false, Texture = "tex_skyr", Metallic = 0f, Smoothness = 0.35f,
                BasePrefabs = new[] { "MeadTasty", "MeadStaminaMinor" },
                Duration = 600f, InstantStamina = 20f, StaminaRegenBonus = 25, HealthRegenBonus = 25,
                CarryWeightBonus = 50f,
                Ingredients = "ChickenEgg:1,Honey:1,Blueberries:2", Amount = 2,
                Previous = { ["Ingredients"] = new[] { "Honey:1,Blueberries:3,DeerMeat:1" } },
            },
            new Drink
            {
                Key = "XXL", PrefabName = "KongStrongXXL", Model = "canister", Texture = "tex_xxl", Metallic = 0f, Smoothness = 0.4f,
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
                Key = "Frost", PrefabName = "KongStrongFrost", Texture = "tex_can_frost", Glow = new Color(0.6f, 0.85f, 1f),
                FrostResistance = HitData.DamageModifier.Resistant,
                Duration = 600f, InstantStamina = 30f, StaminaRegenBonus = 25, SpeedBonus = 5,
                RunStaminaReduction = 15, JumpStaminaReduction = 15,
                Ingredients = "Blueberries:3,FreezeGland:1,MushroomYellow:1", Amount = 2,
            },
            new Drink
            {
                Key = "Toxic", PrefabName = "KongStrongToxic", Texture = "tex_can_toxic", Glow = new Color(0.55f, 1f, 0.45f),
                PoisonResistance = HitData.DamageModifier.Resistant,
                Duration = 600f, InstantStamina = 30f, StaminaRegenBonus = 25, SpeedBonus = 5,
                RunStaminaReduction = 15, JumpStaminaReduction = 15,
                Ingredients = "Blueberries:2,Ooze:1,Thistle:1,MushroomYellow:1", Amount = 2,
            },
            new Drink
            {
                Key = "Eitr", PrefabName = "KongStrongEitr", Texture = "tex_can_eitr", Glow = new Color(0.8f, 0.55f, 1f),
                Duration = 600f, InstantStamina = 20f, InstantEitr = 25f, StaminaRegenBonus = 25, EitrRegenBonus = 50,
                Ingredients = "Sap:1,MushroomMagecap:1,MushroomYellow:1,Blueberries:2", Amount = 2,
            },
            new Drink
            {
                Key = "MetBombe", PrefabName = "KongMetBombe", Fermented = true, Shout = false, Model = "meadbottle", Texture = "tex_metbombe",
                Metallic = 0.1f, Smoothness = 0.7f,
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

        private float _heartDuration;
        private int _heartHealthPenalty;
        private int _heartStaminaPenalty;

        private bool _dumpPrefabs;
        private bool _customModels;
        private readonly Dictionary<string, Mesh> _meshes = new Dictionary<string, Mesh>();
        private readonly Dictionary<string, SE_Kong> _effects = new Dictionary<string, SE_Kong>();
        private bool _shout;
        private float _shoutVolume;
        private string _shoutFile;
        private float _shoutBoost;
        private GameObject _shoutSfx;
        private bool _shoutLoaded;

        // The shout plays from these entries in the drinks' start effects; they are on while a shout is available.
        private readonly List<EffectList.EffectData> _shoutEntries = new List<EffectList.EffectData>();
        private AudioClip _localShout;
        private AudioClip _serverShout;
        private const int MaxShoutBytes = 4 * 1024 * 1024;

        private readonly Dictionary<string, CustomItem> _items = new Dictionary<string, CustomItem>();
        private bool _added;
        private bool _dirty;

        /// <summary>Prefab name of the empty an item leaves behind, or null.</summary>
        internal static string EmptyFor(string itemToken)
        {
            return _emptiesEnabled && ReturnsEmpty.Contains(itemToken) ? EmptyPrefab : null;
        }

        private void Awake()
        {
            ServerSync.Log = Logger;
            ConfigBinder.Begin(Config);
            BindAll();
            ConfigBinder.Finish(Config, Logger);

            Texts.Register(Get);
            Harmony.CreateAndPatchAll(typeof(Patches), PluginGUID);
            PrefabManager.OnVanillaPrefabsAvailable += AddContent;

            // Fires for every value the server sends, when it restores the own values on leaving, and on a config reload.
            Config.SettingChanged += (sender, args) => _dirty = true;

            // The server's own shout, if it has one, is sent to every player who joins.
            CustomRPC shoutRpc = NetworkManager.Instance.AddRPC("KongStrong_ServerShout", (sender, package) => Nothing(), ReceiveServerShout);
            SynchronizationManager.Instance.AddInitialSynchronization(shoutRpc, ServerShoutPackage);
        }

        private void BindAll()
        {
            foreach (Drink drink in _drinks)
            {
                drink.Bind(Config);
            }

            _skyrCup.Bind(Config);
            Pieces.Bind(Config);
            BindGeneral();
        }

        private void Update()
        {
            // Back in the main menu: your own shout again.
            if (_serverShout != null && ZNet.instance == null)
            {
                _serverShout = null;
                ApplyShout();
            }

            if (_dirty && _added)
            {
                _dirty = false;
                try
                {
                    Refresh();
                }
                catch (Exception e)
                {
                    Logger.LogError($"Could not apply the changed config: {e}");
                }
            }
        }

        /// <summary>Reads the config again and updates recipes, buffs, food, the pieces and the texts in place.</summary>
        private void Refresh()
        {
            BindAll();
            SE_Kong.CrashEnabled = _crashEnabled;

            Fermenter fermenter = PrefabManager.Instance.GetPrefab("fermenter")?.GetComponent<Fermenter>();
            foreach (Drink drink in _drinks)
            {
                if (_effects.TryGetValue(drink.Key, out SE_Kong effect))
                {
                    ApplyStats(effect, drink);
                }

                ServerSync.SetRecipe(RecipeOf(drink.Fermented ? drink.BasePrefabName : drink.PrefabName), drink.Enabled,
                    drink.CraftingStation, drink.Amount, drink.Ingredients);
                if (drink.Fermented)
                {
                    Fermenter.ItemConversion conversion =
                        fermenter?.m_conversion.Find(c => c.m_from != null && c.m_from.name == drink.BasePrefabName);
                    if (conversion != null)
                    {
                        conversion.m_producedItems = Mathf.Max(1, drink.FermenterYield);
                    }
                }
            }

            SetDebuff(SE_Kong.Crash, _crashDuration, 1f - _crashRegenPenalty / 100f, 1f, -_crashSpeedPenalty / 100f);
            SetDebuff(SE_Kong.HeartRacing, _heartDuration, 1f - _heartStaminaPenalty / 100f, 1f - _heartHealthPenalty / 100f, 0f);

            ServerSync.SetRecipe(RecipeOf("KongSkyrCup"), _skyrCup.Enabled, _skyrCup.CraftingStation, _skyrCup.Amount, _skyrCup.Ingredients);
            if (_items.TryGetValue("KongSkyrCup", out CustomItem cup))
            {
                SetSkyrFood(cup.ItemDrop.m_itemData.m_shared);
            }

            Pieces.Apply();
            Texts.Refresh(Get);
            ApplyShout();
        }

        private Recipe RecipeOf(string prefab) => _items.TryGetValue(prefab, out CustomItem item) ? item.Recipe?.Recipe : null;

        private void Track(CustomItem item, string station, string ingredients)
        {
            _items[item.ItemPrefab.name] = item;
            ServerSync.Remember(item.Recipe?.Recipe, station, ingredients);
        }

        private static IEnumerator Nothing()
        {
            yield break;
        }

        private string ShoutPath()
        {
            return string.IsNullOrEmpty(_shoutFile) ? null
                : System.IO.Path.IsPathRooted(_shoutFile) ? _shoutFile : System.IO.Path.Combine(BepInEx.Paths.ConfigPath, _shoutFile);
        }

        /// <summary>On the server: its shout file for a joining player, or nothing.</summary>
        private ZPackage ServerShoutPackage()
        {
            byte[] data = null;
            string path = ShoutPath();
            try
            {
                if (path != null && System.IO.File.Exists(path))
                {
                    data = System.IO.File.ReadAllBytes(path);
                    if (data.Length > MaxShoutBytes)
                    {
                        Logger.LogWarning($"{path} is larger than {MaxShoutBytes / 1024 / 1024} MB and is not sent to the players.");
                        data = null;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.LogWarning($"Could not read {path}: {e.Message}");
                data = null;
            }

            var package = new ZPackage();
            package.Write(data != null);
            if (data != null)
            {
                package.Write(data);
            }

            return package;
        }

        /// <summary>On a player: the server's shout replaces the own one while connected. Without one, the own one stays.</summary>
        private IEnumerator ReceiveServerShout(long sender, ZPackage package)
        {
            _serverShout = null;
            if (package.ReadBool() && !GUIManager.IsHeadless())
            {
                try
                {
                    _serverShout = Sounds.Normalize(Sounds.FromWav("server_shout", package.ReadByteArray()), _shoutBoost);
                    Logger.LogInfo("Using the shout of the server while connected.");
                }
                catch (Exception e)
                {
                    Logger.LogWarning($"The server's shout could not be read: {e.Message}");
                }
            }

            ApplyShout();
            yield break;
        }

        /// <summary>The server's shout if there is one, else your own file; [Sound] Shout and ShoutVolume are your own choice.</summary>
        private void ApplyShout()
        {
            AudioClip clip = _serverShout != null ? _serverShout : _localShout;
            if (_shoutSfx != null)
            {
                ZSFX sfx = _shoutSfx.GetComponent<ZSFX>();
                sfx.m_audioClips = clip != null ? new[] { clip } : new AudioClip[0];
                sfx.m_minVol = sfx.m_maxVol = _shoutVolume;
            }

            foreach (EffectList.EffectData entry in _shoutEntries)
            {
                entry.m_enabled = _shout && clip != null && _shoutSfx != null;
            }
        }

        private void BindGeneral()
        {
            Patches.TooAwakeToSleep = Config.Bind("General", "TooAwakeToSleep", true,
                ServerSync.Desc("Beds refuse to let you sleep while a Kong buff is active.")).Value;
            _emptiesEnabled = Config.Bind("General", "Empties", true,
                ServerSync.Desc("Drinks leave an empty (Leergut) behind, for the deposit machine and the can pyramid.")).Value;

            _crashEnabled = Config.Bind("SugarCrash", "Enabled", true,
                ServerSync.Desc("Urban Classic is followed by a sugar crash when its buff runs out.")).Value;
            _crashDuration = Config.Bind("SugarCrash", "DurationSeconds", 60f,
                ServerSync.Desc("How long the sugar crash lasts, in seconds.")).Value;
            _crashRegenPenalty = Config.Bind("SugarCrash", "StaminaRegenPenaltyPercent", 25,
                ServerSync.Desc("Slower stamina regeneration during the crash, in percent.")).Value;
            _crashSpeedPenalty = Config.Bind("SugarCrash", "MoveSpeedPenaltyPercent", 5,
                ServerSync.Desc("Slower movement during the crash, in percent.")).Value;

            ToleranceSettings t = SE_Kong.Tolerance;
            t.Enabled = Config.Bind("CaffeineTolerance", "Enabled", true,
                ServerSync.Desc("Drinks taken shortly after each other work less.")).Value;
            t.WindowMinutes = Config.Bind("CaffeineTolerance", "WindowMinutes", 15f,
                ServerSync.Desc("Drinks within this many minutes count as drunk in a row.")).Value;
            t.SecondDrinkPercent = Config.Bind("CaffeineTolerance", "SecondDrinkEffectPercent", 50,
                ServerSync.Desc("Strength of the second drink in a row, in percent.")).Value;
            t.FurtherDrinkPercent = Config.Bind("CaffeineTolerance", "FurtherDrinkEffectPercent", 25,
                ServerSync.Desc("Strength of the third and every further drink in a row, in percent.")).Value;
            t.HeartRacingFromDrink = Config.Bind("CaffeineTolerance", "HeartRacingFromDrink", 3,
                ServerSync.Desc("From this drink in a row on you get heart racing. 0 turns it off.")).Value;

            _heartDuration = Config.Bind("HeartRacing", "DurationSeconds", 120f,
                ServerSync.Desc("How long heart racing lasts, in seconds.")).Value;
            _heartHealthPenalty = Config.Bind("HeartRacing", "HealthRegenPenaltyPercent", 50,
                ServerSync.Desc("Slower health regeneration during heart racing, in percent.")).Value;
            _heartStaminaPenalty = Config.Bind("HeartRacing", "StaminaRegenPenaltyPercent", 25,
                ServerSync.Desc("Slower stamina regeneration during heart racing, in percent.")).Value;

            // 1.2.0 shipped with the shout off and 0.8; values still at that default move to the new one once (see ConfigBinder).
            _shout = ConfigBinder.Bind(Config, "Sound", "Shout", true,
                "Every Kong Strong drink (cans, XXL, tap) is followed by a shouted \"D K!\". Your own choice, also on a server.",
                new[] { false }, synced: false);
            _customModels = Config.Bind("General", "CustomModels", true,
                "Cans, bottles and the canister get their own models. Off: they look like the vanilla meads.").Value;
            SE_Kong.PowerAnimation = Config.Bind("General", "PowerAnimation", true,
                "Drinking Kong raises your arms like activating a Forsaken power, with Eikthyr's glow.").Value;
            _shoutVolume = ConfigBinder.Bind(Config, "Sound", "ShoutVolume", 1f,
                new BepInEx.Configuration.ConfigDescription("Volume of the shout (the clip itself is raised to full level).",
                    new BepInEx.Configuration.AcceptableValueRange<float>(0f, 1f)), new[] { 0.8f });
            _shoutFile = Config.Bind("Sound", "CustomShoutFile", "KongStrong/shout.wav",
                "Your own 16 bit PCM WAV for the shout, relative to BepInEx/config or absolute. " +
                "The mod ships no sound; without this file nothing is played. If the server has this file, players hear the server's shout.").Value;
            _shoutBoost = Config.Bind("Sound", "ShoutBoost", 2.5f,
                new BepInEx.Configuration.ConfigDescription("Makes the shout louder than full scale allows by soft limiting (1 = off).",
                    new BepInEx.Configuration.AcceptableValueRange<float>(1f, 8f))).Value;

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

            // Always added, so the server can switch them on and off; the settings decide whether they are used.
            Try("the sugar crash", () => SE_Kong.Crash = AddDebuff("SE_KongSugarCrash", "se_kong_crash", "crash"));
            Try("heart racing", () => SE_Kong.HeartRacing = AddDebuff("SE_KongHeartRacing", "se_kong_heartracing", "heartracing"));

            Try("the power effect", AddPowerEffect);

            Try(EmptyPrefab, AddEmpty);

            // Everything is added; Enabled only switches recipes and pieces, so the server can change it while you play.
            foreach (Drink drink in _drinks)
            {
                Try(drink.PrefabName, () => AddDrink(drink));
            }

            Try("KongSkyrCup", AddSkyrCup);

            KongTap.Effect = _effects.TryGetValue("SugarFree", out SE_Kong tap) ? tap
                : _effects.TryGetValue("XXL", out SE_Kong xxl) ? xxl : null;
            Pieces.Register(ParseRequirements, Try);

            _added = true;
            _dirty = true;   // applies Enabled and everything else on the next frame
        }

        private Mesh MeshFor(string model)
        {
            if (!_meshes.TryGetValue(model, out Mesh mesh))
            {
                switch (model)
                {
                    case "skyr": mesh = Models.SkyrBottle(); break;
                    case "canister": mesh = Models.Canister(); break;
                    case "meadbottle": mesh = Models.MeadBottle(); break;
                    case "barrel": mesh = Models.Barrel(); break;
                    case "cup": mesh = Models.Cup(); break;
                    case "crushed": mesh = Models.CrushedCan(); break;
                    default: mesh = Models.Can(); break;
                }

                _meshes[model] = mesh;
            }

            return mesh;
        }

        /// <summary>Own model for a cloned item, and its placeable piece (since 1.0 meads can be put down) gets our name.</summary>
        private void Dress(CustomItem item, string model, string texture, float metallic, float smoothness, Color? glow, Sprite icon)
        {
            Piece piece = item.ItemPrefab.GetComponent<Piece>();
            if (piece != null)
            {
                piece.m_name = item.ItemDrop.m_itemData.m_shared.m_name;
                piece.m_description = item.ItemDrop.m_itemData.m_shared.m_description;
                piece.m_icon = icon;
            }

            if (_customModels)
            {
                Visuals.ApplyItem(item.ItemPrefab, MeshFor(model), Visuals.ItemMaterial(texture, metallic, smoothness), glow);
            }
        }

        /// <summary>The visual part of Eikthyr's power effect; effects containing a sound are copied without it.</summary>
        private void AddPowerEffect()
        {
            StatusEffect eikthyr = PrefabManager.Cache.GetPrefab<StatusEffect>("GP_Eikthyr")
                                   ?? ObjectDB.instance?.GetStatusEffect("GP_Eikthyr".GetStableHashCode());
            if (eikthyr == null)
            {
                Logger.LogWarning("GP_Eikthyr not found, drinking plays the power animation without the glow.");
                return;
            }

            var visuals = new List<EffectList.EffectData>();
            foreach (EffectList.EffectData data in eikthyr.m_startEffects?.m_effectPrefabs ?? new EffectList.EffectData[0])
            {
                if (data?.m_prefab == null || data.m_prefab.GetComponent<ZSFX>() != null)
                {
                    continue;                                   // a pure sound
                }

                GameObject prefab = data.m_prefab;
                if (prefab.GetComponentInChildren<ZSFX>(true) != null || prefab.GetComponentInChildren<AudioSource>(true) != null)
                {
                    prefab = PrefabManager.Instance.CreateClonedPrefab("kong_" + data.m_prefab.name, data.m_prefab);
                    foreach (ZSFX sfx in prefab.GetComponentsInChildren<ZSFX>(true))
                    {
                        DestroyImmediate(sfx);
                    }

                    foreach (AudioSource audio in prefab.GetComponentsInChildren<AudioSource>(true))
                    {
                        DestroyImmediate(audio);
                    }
                }

                visuals.Add(new EffectList.EffectData
                {
                    m_prefab = prefab, m_enabled = data.m_enabled, m_variant = data.m_variant, m_attach = data.m_attach,
                    m_follow = data.m_follow, m_inheritParentRotation = data.m_inheritParentRotation,
                    m_inheritParentScale = data.m_inheritParentScale, m_multiplyParentVisualScale = data.m_multiplyParentVisualScale,
                    m_randomRotation = data.m_randomRotation, m_scale = data.m_scale, m_childTransform = data.m_childTransform,
                });
            }

            Logger.LogInfo($"Power effect from GP_Eikthyr: {string.Join(", ", visuals.Select(v => v.m_prefab.name))} " +
                           $"(of {eikthyr.m_startEffects?.m_effectPrefabs?.Length ?? 0} effects)");
            if (visuals.Count > 0)
            {
                SE_Kong.PowerEffects = new EffectList { m_effectPrefabs = visuals.ToArray() };
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

        private StatusEffect AddDebuff(string name, string token, string icon)
        {
            var effect = ScriptableObject.CreateInstance<SE_Stats>();
            effect.name = name;
            effect.m_name = "$" + token;
            effect.m_tooltip = effect.m_name + "_tooltip";
            effect.m_category = StatusCategory;
            effect.m_icon = Icons.Get(icon);
            effect.m_startMessageType = MessageHud.MessageType.Center;
            effect.m_startMessage = effect.m_name + "_start";
            ItemManager.Instance.AddStatusEffect(new CustomStatusEffect(effect, false));
            return effect;
        }

        private static void SetDebuff(StatusEffect status, float duration, float staminaRegen, float healthRegen, float speed)
        {
            if (status is SE_Stats effect)
            {
                effect.m_ttl = duration;
                effect.m_staminaRegenMultiplier = Mathf.Max(0f, staminaRegen);
                effect.m_healthRegenMultiplier = Mathf.Max(0f, healthRegen);
                effect.m_speedModifier = speed;
            }
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
            _effects[drink.Key] = effect;

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
            if (!_customModels)
            {
                item.ItemPrefab.transform.localScale *= drink.ModelScale;
            }

            Dress(item, drink.Model, drink.Texture, drink.Metallic, drink.Smoothness, drink.Glow, icon);
            ItemManager.Instance.AddItem(item);
            Track(item, drink.CraftingStation, drink.Ingredients);

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
            effect.name = "SE_" + drink.PrefabName;
            effect.m_name = $"$se_{drink.Token}";
            effect.m_tooltip = $"$se_{drink.Token}_tooltip";
            effect.m_category = StatusCategory;
            effect.m_icon = icon;
            effect.m_startMessageType = MessageHud.MessageType.Center;
            effect.m_startMessage = $"$se_{drink.Token}_start";
            ApplyStats(effect, drink);

            // Reuse the drinking sound and particles of the mead we cloned, plus the shout.
            if (baseEffect != null)
            {
                effect.m_startEffects = baseEffect.m_startEffects;
                effect.m_stopEffects = baseEffect.m_stopEffects;

                // A dedicated server plays no sound, so it has no shout.
                if (drink.Shout && !GUIManager.IsHeadless())
                {
                    if (!_shoutLoaded)
                    {
                        _shoutLoaded = true;
                        Try("the shout", () =>
                        {
                            _localShout = LoadShout();
                            _shoutSfx = Sounds.CreateSfx("sfx_kong_dk", _localShout, baseEffect.m_startEffects, _shoutVolume, 0.35f);
                        });
                    }

                    if (_shoutSfx != null)
                    {
                        var entry = new EffectList.EffectData { m_prefab = _shoutSfx, m_enabled = false, m_variant = -1 };
                        _shoutEntries.Add(entry);
                        effect.m_startEffects = new EffectList
                        {
                            m_effectPrefabs = (baseEffect.m_startEffects?.m_effectPrefabs ?? new EffectList.EffectData[0])
                                .Concat(new[] { entry }).ToArray(),
                        };
                    }
                }
            }

            return effect;
        }

        /// <summary>SEMan copies the effect when it starts, so a changed value counts from the next drink.</summary>
        private void ApplyStats(SE_Kong effect, Drink drink)
        {
            bool crashAfter = drink.CausesCrash && _crashEnabled;
            effect.CausesCrash = drink.CausesCrash;
            effect.m_ttl = drink.Duration;
            effect.m_stopMessageType = MessageHud.MessageType.TopLeft;
            effect.m_stopMessage = crashAfter ? "" : "$se_kong_stop";

            effect.m_staminaUpFront = drink.InstantStamina;
            effect.m_eitrUpFront = drink.InstantEitr;
            effect.m_healthOverTime = Mathf.Max(0f, drink.HealOverTime);
            effect.m_healthOverTimeDuration = 10f;
            effect.m_healthOverTimeInterval = 1f;

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
        }

        private AudioClip LoadShout()
        {
            string path = ShoutPath();
            if (path != null && System.IO.File.Exists(path))
            {
                try
                {
                    AudioClip custom = Sounds.LoadWavFile(path);
                    Logger.LogInfo($"Using your shout from {path} (boost x{_shoutBoost}).");
                    return Sounds.Normalize(custom, _shoutBoost);
                }
                catch (Exception e)
                {
                    Logger.LogWarning($"Could not read {path}: {e.Message}");
                }
            }
            else
            {
                Logger.LogInfo($"No shout file at {path}, so no \"D K!\". The mod ships none.");
            }

            return null;
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
            var baseItem = new CustomItem(drink.BasePrefabName, drink.FermenterBase, baseConfig);
            Dress(baseItem, "barrel", "tex_barrel", 0f, 0.25f, null, baseConfig.Icon);
            ItemManager.Instance.AddItem(baseItem);
            Track(baseItem, drink.CraftingStation, drink.Ingredients);

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
            Dress(item, "can", "tex_empty", 0.6f, 0.5f, null, config.Icon);
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
            SetSkyrFood(shared);
            Dress(item, "cup", "tex_skyrcup", 0f, 0.3f, null, config.Icon);
            ItemManager.Instance.AddItem(item);
            Track(item, _skyrCup.CraftingStation, _skyrCup.Ingredients);
            Logger.LogInfo("Added KongSkyrCup (cloned from QueensJam).");
        }

        private void SetSkyrFood(ItemDrop.ItemData.SharedData shared)
        {
            shared.m_food = _skyrCup.Health;
            shared.m_foodStamina = _skyrCup.Stamina;
            shared.m_foodEitr = _skyrCup.Eitr;
            shared.m_foodBurnTime = _skyrCup.Duration;
            shared.m_foodRegen = _skyrCup.Regen;
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
