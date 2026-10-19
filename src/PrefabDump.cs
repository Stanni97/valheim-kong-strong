using System;
using System.IO;
using System.Linq;
using System.Text;
using BepInEx;
using BepInEx.Logging;
using Jotunn.Managers;
using UnityEngine;

namespace KongStrong
{
    /// <summary>Development aid: logs the structure of vanilla prefabs (Debug.DumpPrefabs in the config).</summary>
    internal static class PrefabDump
    {
        /// <summary>Writes the icons of the given items and pieces as PNGs to BepInEx/cache/KongStrongDump.</summary>
        public static void ExportIcons(ManualLogSource log, string[] names)
        {
            string folder = Path.Combine(Paths.CachePath, "KongStrongDump");
            Directory.CreateDirectory(folder);
            foreach (string name in names)
            {
                try
                {
                    GameObject prefab = PrefabManager.Instance.GetPrefab(name);
                    Sprite icon = prefab == null ? null
                        : prefab.GetComponent<ItemDrop>()?.m_itemData.m_shared.m_icons?.FirstOrDefault()
                          ?? prefab.GetComponent<Piece>()?.m_icon;
                    if (icon == null)
                    {
                        log.LogInfo($"=== ICON {name}: none");
                        continue;
                    }

                    Texture2D readable = Readable(icon);
                    File.WriteAllBytes(Path.Combine(folder, name + ".png"), readable.EncodeToPNG());
                    log.LogInfo($"=== ICON {name}: {readable.width}x{readable.height} -> {folder}");
                }
                catch (Exception e)
                {
                    log.LogWarning($"=== ICON {name}: {e.Message}");
                }
            }
        }

        /// <summary>Copies the sprite out of its (possibly unreadable) atlas through a render texture.</summary>
        private static Texture2D Readable(Sprite sprite)
        {
            Texture2D source = sprite.texture;
            Rect rect = sprite.textureRect;
            RenderTexture rt = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            RenderTexture previous = RenderTexture.active;
            try
            {
                Graphics.Blit(source, rt);
                RenderTexture.active = rt;
                var result = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGBA32, false);
                result.ReadPixels(rect, 0, 0);
                result.Apply();
                return result;
            }
            finally
            {
                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(rt);
            }
        }

        public static void Dump(ManualLogSource log, string[] names)
        {
            foreach (string name in names)
            {
                GameObject prefab = PrefabManager.Instance.GetPrefab(name);
                var text = new StringBuilder();
                text.AppendLine($"=== DUMP {name}" + (prefab == null ? " NOT FOUND" : ""));
                if (prefab != null)
                {
                    Walk(prefab.transform, "", text);
                }

                log.LogInfo(text.ToString());
            }
        }

        private static void Walk(Transform t, string indent, StringBuilder text)
        {
            text.AppendLine($"{indent}[{t.name}] active={t.gameObject.activeSelf} layer={LayerMask.LayerToName(t.gameObject.layer)} " +
                            $"pos={t.localPosition:F3} rot={t.localEulerAngles:F1} scale={t.localScale:F3}");
            foreach (Component c in t.GetComponents<Component>())
            {
                if (c == null || c is Transform)
                {
                    continue;
                }

                text.AppendLine($"{indent}  - {c.GetType().Name}{Describe(c)}");
            }

            foreach (Transform child in t)
            {
                Walk(child, indent + "  ", text);
            }
        }

        private static string Describe(Component c)
        {
            switch (c)
            {
                case BoxCollider box: return $" center={box.center:F3} size={box.size:F3} trigger={box.isTrigger}";
                case CapsuleCollider cap: return $" center={cap.center:F3} r={cap.radius:F3} h={cap.height:F3} dir={cap.direction}";
                case SphereCollider sph: return $" center={sph.center:F3} r={sph.radius:F3}";
                case MeshCollider mc: return $" mesh={(mc.sharedMesh ? mc.sharedMesh.name : "-")} convex={mc.convex}";
                case MeshFilter mf: return mf.sharedMesh ? $" mesh={mf.sharedMesh.name} verts={mf.sharedMesh.vertexCount} bounds={mf.sharedMesh.bounds}" : " mesh=-";
                case Renderer r:
                    return " materials=[" + string.Join("; ", r.sharedMaterials.Select(m => m == null ? "null"
                        : $"{m.name} shader={m.shader.name} tex={(m.HasProperty("_MainTex") && m.mainTexture ? m.mainTexture.name : "-")}" +
                          $" color={(m.HasProperty("_Color") ? m.color.ToString() : "-")} queue={m.renderQueue}" +
                          $" props={string.Join(",", m.GetTexturePropertyNames())}")) + "]";
                case Container con: return $" name={con.m_name} size={con.m_width}x{con.m_height} open={(con.m_open ? con.m_open.name : "-")} closed={(con.m_closed ? con.m_closed.name : "-")}";
                case WearNTear w: return $" new={(w.m_new ? w.m_new.name : "-")} worn={(w.m_worn ? w.m_worn.name : "-")} broken={(w.m_broken ? w.m_broken.name : "-")} hp={w.m_health} fragments={w.m_fragmentRoots?.Length}";
                case Piece p: return $" name={p.m_name} comfort={p.m_comfort} category={p.m_category}";
                case ItemDrop i: return $" type={i.m_itemData.m_shared.m_itemType} food={i.m_itemData.m_shared.m_food} weight={i.m_itemData.m_shared.m_weight}";
                case CraftingStation cs: return $" name={cs.m_name}";
                default: return "";
            }
        }
    }
}
