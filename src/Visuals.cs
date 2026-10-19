using System.Linq;
using Jotunn.Managers;
using UnityEngine;

namespace KongStrong
{
    /// <summary>Puts the code-built models and textures onto the cloned vanilla prefabs.</summary>
    internal static class Visuals
    {
        private static readonly string[] UnusedMaps =
        {
            "_BumpMap", "_MetallicGlossMap", "_OcclusionMap", "_EmissionMap", "_DetailAlbedoMap",
            "_DetailNormalMap", "_DetailMask", "_ParallaxMap", "_SpecGlossMap", "_MetallicTex",
        };

        private static readonly string[] UnusedKeywords =
        {
            "_NORMALMAP", "_METALLICGLOSSMAP", "_EMISSION", "_DETAIL_MULX2", "_PARALLAXMAP", "_SPECGLOSSMAP",
        };

        /// <summary>Opaque Standard material, cloned from the tasty mead so it matches the game's lighting.</summary>
        public static Material ItemMaterial(string texture, float metallic, float smoothness)
        {
            Material template = PrefabManager.Instance.GetPrefab("MeadTasty")?.GetComponentInChildren<MeshRenderer>(true)?.sharedMaterial;
            return Prepare(template != null ? new Material(template) : new Material(Shader.Find("Standard")), texture, metallic, smoothness);
        }

        /// <summary>Valheim's Custom/Piece material (wetness, snow, wear), cloned from the wooden chest.</summary>
        public static Material PieceMaterial(string texture)
        {
            Material template = PrefabManager.Instance.GetPrefab("piece_chest_wood")?.GetComponentInChildren<MeshRenderer>(true)?.sharedMaterial;
            return Prepare(template != null ? new Material(template) : new Material(Shader.Find("Standard")), texture, 0f, 0.2f);
        }

        private static Material Prepare(Material material, string texture, float metallic, float smoothness)
        {
            material.name = "kong_" + texture;
            material.mainTexture = Icons.Texture(texture);
            if (material.HasProperty("_Color"))
            {
                material.color = Color.white;
            }

            foreach (string map in UnusedMaps.Where(material.HasProperty))
            {
                material.SetTexture(map, null);
            }

            foreach (string keyword in UnusedKeywords)
            {
                material.DisableKeyword(keyword);
            }

            if (material.HasProperty("_Metallic"))
            {
                material.SetFloat("_Metallic", metallic);
            }

            if (material.HasProperty("_Glossiness"))
            {
                material.SetFloat("_Glossiness", smoothness);
            }

            material.renderQueue = 2000;
            return material;
        }

        /// <summary>
        /// Replaces the model of a cloned item: every mesh gets ours, the visible ones sit at the item origin,
        /// one box collider fits the new mesh, and the potion light goes off (or takes the glow colour).
        /// </summary>
        public static void ApplyItem(GameObject prefab, Mesh mesh, Material material, Color? glow = null)
        {
            MeshFilter visible = null;
            foreach (MeshFilter filter in prefab.GetComponentsInChildren<MeshFilter>(true))
            {
                filter.sharedMesh = mesh;
                MeshRenderer renderer = filter.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    renderer.sharedMaterials = new[] { material };
                }

                Transform t = filter.transform;
                bool equipped = t.GetComponentsInParent<Transform>(true).Any(p => p.name == "equipoffset");
                if (!equipped)
                {
                    t.localPosition = Vector3.zero;
                    t.localRotation = Quaternion.identity;
                    t.localScale = Vector3.one;
                    if (visible == null && t.gameObject.activeSelf)
                    {
                        visible = filter;
                    }
                }

                filter.GetComponent<LODGroup>()?.RecalculateBounds();
            }

            foreach (Collider collider in prefab.GetComponentsInChildren<Collider>(true))
            {
                Object.DestroyImmediate(collider);
            }

            if (visible != null)
            {
                var box = visible.gameObject.AddComponent<BoxCollider>();
                box.center = mesh.bounds.center;
                box.size = mesh.bounds.size;
            }

            foreach (Light light in prefab.GetComponentsInChildren<Light>(true))
            {
                if (glow == null)
                {
                    light.gameObject.SetActive(false);
                }
                else
                {
                    light.color = glow.Value;
                    light.intensity *= 0.6f;
                }
            }
        }

        /// <summary>Adds a model part below parent, optionally with a box collider fitting it.</summary>
        public static GameObject AddPart(Transform parent, string name, Mesh mesh, Material material, Vector3 position,
                                         Quaternion rotation, bool collider = true)
        {
            var part = new GameObject(name) { layer = parent.gameObject.layer };
            part.transform.SetParent(parent, false);
            part.transform.localPosition = position;
            part.transform.localRotation = rotation;
            part.AddComponent<MeshFilter>().sharedMesh = mesh;
            part.AddComponent<MeshRenderer>().sharedMaterial = material;
            if (collider)
            {
                var box = part.AddComponent<BoxCollider>();
                box.center = mesh.bounds.center;
                box.size = mesh.bounds.size;
            }

            return part;
        }
    }
}
