using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace KongStrong
{
    /// <summary>Config of one build piece.</summary>
    internal sealed class PieceSettings
    {
        public string Key;
        public bool Enabled = true;
        public string Ingredients;

        public void Bind(ConfigFile config)
        {
            Enabled = config.Bind(Key, "Enabled", Enabled, "Adds this piece to the hammer.").Value;
            Ingredients = config.Bind(Key, "Ingredients", Ingredients, "Comma separated list of ItemPrefab:Amount.").Value;
        }
    }

    /// <summary>Fridge, can pyramid, deposit machine and tap, all built from the wooden chest prefab.</summary>
    internal static class Pieces
    {
        public static readonly PieceSettings Fridge = new PieceSettings { Key = "Fridge", Ingredients = "FineWood:6,Bronze:2,FreezeGland:1" };
        public static readonly PieceSettings Pyramid = new PieceSettings { Key = "CanPyramid", Ingredients = "KongLeergut:10" };
        public static readonly PieceSettings Automat = new PieceSettings { Key = "DepositMachine", Ingredients = "FineWood:8,Bronze:3" };
        public static readonly PieceSettings Tap = new PieceSettings { Key = "Tap", Ingredients = "KongStrongXXL:1,Wood:6" };

        public static void Bind(ConfigFile config)
        {
            Fridge.Bind(config);
            Pyramid.Bind(config);
            Automat.Bind(config);
            Tap.Bind(config);
            KongDepositMachine.CoinsPerEmpty = config.Bind(Automat.Key, "CoinsPerEmpty", 1, "Coins paid for each empty.").Value;
            KongTap.Full = Mathf.Max(1, config.Bind(Tap.Key, "Servings", 38, "Drinks in one XXL canister on the tap.").Value);
        }

        public static void Register(Func<string, string, RequirementConfig[]> parse, Action<string, Action> attempt)
        {
            if (Fridge.Enabled)
            {
                attempt("piece_kong_fridge", () => AddFridge(parse));
            }

            if (Pyramid.Enabled)
            {
                attempt("piece_kong_pyramid", () => AddPyramid(parse));
            }

            if (Automat.Enabled)
            {
                attempt("piece_kong_automat", () => AddAutomat(parse));
            }

            if (Tap.Enabled)
            {
                attempt("piece_kong_tap", () => AddTap(parse));
            }
        }

        /// <summary>Clone of the wooden chest without its model; returns the model root ("New").</summary>
        private static GameObject FromChest(string name, bool keepContainer, out Transform model)
        {
            GameObject prefab = PrefabManager.Instance.CreateClonedPrefab(name, "piece_chest_wood");
            Transform snow = prefab.transform.Find("floor_2x2_snow");
            if (snow != null)
            {
                UnityEngine.Object.DestroyImmediate(snow.gameObject);
            }

            model = prefab.transform.Find("New");
            var children = new List<GameObject>();
            foreach (Transform child in model)
            {
                children.Add(child.gameObject);
            }

            children.ForEach(UnityEngine.Object.DestroyImmediate);
            LODGroup lod = model.GetComponent<LODGroup>();
            if (lod != null)
            {
                UnityEngine.Object.DestroyImmediate(lod);
            }

            if (!keepContainer)
            {
                UnityEngine.Object.DestroyImmediate(prefab.GetComponent<Container>());
            }

            return prefab;
        }

        private static void Add(GameObject prefab, string token, string icon, string category, string station, RequirementConfig[] requirements)
        {
            var config = new PieceConfig
            {
                Name = "$" + token,
                Description = "$" + token + "_desc",
                Icon = Icons.Get(icon),
                PieceTable = PieceTables.Hammer,
                Category = category,
                CraftingStation = station,
                Requirements = requirements,
            };
            PieceManager.Instance.AddPiece(new CustomPiece(prefab, false, config));
        }

        private static void AddFridge(Func<string, string, RequirementConfig[]> parse)
        {
            GameObject prefab = FromChest("piece_kong_fridge", true, out Transform model);
            Material material = Visuals.PieceMaterial("tex_fridge");
            Visuals.AddPart(model, "body", Models.FridgeBody(), material, Vector3.zero, Quaternion.identity);
            Mesh door = Models.FridgeDoor();
            var hinge = new Vector3(Models.FridgeWidth / 2, 0f, Models.FridgeDepth / 2);
            GameObject closed = Visuals.AddPart(model, "door_closed", door, material, hinge, Quaternion.identity, false);
            GameObject open = Visuals.AddPart(model, "door_open", door, material, hinge, Quaternion.Euler(0f, 100f, 0f), false);
            open.SetActive(false);

            Container container = prefab.GetComponent<Container>();
            container.m_name = "$piece_kong_fridge";
            container.m_width = 4;
            container.m_height = 4;
            container.m_open = open;
            container.m_closed = closed;
            Add(prefab, "piece_kong_fridge", "fridge", PieceCategories.Furniture, CraftingStations.Workbench,
                parse("piece_kong_fridge", Fridge.Ingredients));
        }

        private static void AddPyramid(Func<string, string, RequirementConfig[]> parse)
        {
            GameObject prefab = FromChest("piece_kong_pyramid", false, out Transform model);
            string[] textures = { "tex_can_sugarfree", "tex_can_classic", "tex_can_frost", "tex_can_toxic" };
            for (int row = 0; row < 4; row++)
            {
                Visuals.AddPart(model, "row" + row, Models.PyramidRow(row), Visuals.PieceMaterial(textures[row]),
                    Vector3.zero, Quaternion.identity, false);
            }

            var box = model.gameObject.AddComponent<BoxCollider>();
            box.center = new Vector3(0f, 0.48f, 0f);
            box.size = new Vector3(0.42f, 0.96f, 0.1f);
            prefab.GetComponent<Piece>().m_comfort = 1;
            Add(prefab, "piece_kong_pyramid", "pyramid", PieceCategories.Furniture, null,
                parse("piece_kong_pyramid", Pyramid.Ingredients));
        }

        private static void AddAutomat(Func<string, string, RequirementConfig[]> parse)
        {
            GameObject prefab = FromChest("piece_kong_automat", false, out Transform model);
            // German lettering for a German game, English (DEPOSIT / EMPTIES) for every other language; needs a restart to change.
            bool german = Localization.instance != null && Localization.instance.GetSelectedLanguage() == "German";
            Visuals.AddPart(model, "body", Models.Automat(), Visuals.PieceMaterial(german ? "tex_automat" : "tex_automat_en"),
                Vector3.zero, Quaternion.identity);
            prefab.AddComponent<KongDepositMachine>();
            Add(prefab, "piece_kong_automat", "automat", PieceCategories.Misc, CraftingStations.Workbench,
                parse("piece_kong_automat", Automat.Ingredients));
        }

        private static void AddTap(Func<string, string, RequirementConfig[]> parse)
        {
            GameObject prefab = FromChest("piece_kong_tap", false, out Transform model);
            Visuals.AddPart(model, "stand", Models.TapStand(), Visuals.PieceMaterial("tex_wood"), Vector3.zero, Quaternion.identity);
            Visuals.AddPart(model, "canister", Models.TapCanister(), Visuals.PieceMaterial("tex_xxl"), Vector3.zero, Quaternion.identity);
            prefab.AddComponent<KongTap>();

            // The canister on the tap is used up: tearing the tap down does not give it back.
            RequirementConfig[] requirements = parse("piece_kong_tap", Tap.Ingredients);
            foreach (RequirementConfig requirement in requirements)
            {
                if (requirement.Item == "KongStrongXXL")
                {
                    requirement.Recover = false;
                }
            }

            Add(prefab, "piece_kong_tap", "tap", PieceCategories.Misc, CraftingStations.Workbench, requirements);
        }
    }
}
