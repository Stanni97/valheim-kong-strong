using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KongStrong
{
    /// <summary>One ring of a lathe profile: radius, height and texture row (v) in metres.</summary>
    internal readonly struct Ring
    {
        public readonly float R, Y, V;

        public Ring(float r, float y, float v)
        {
            R = r;
            Y = y;
            V = v;
        }
    }

    /// <summary>
    /// Meshes built in code, so the mod needs no Unity project or asset bundle and stays a single DLL.
    /// The texture layouts match tools/make_textures.py.
    /// </summary>
    internal static class Models
    {
        // ------------------------------------------------------------------ generators

        /// <summary>
        /// Turns a profile (bottom to top) around the Y axis. u runs once around (within uMin..uMax),
        /// v comes from the profile. Two rings at the same spot make a hard edge.
        /// shape scales the radius per angle (rounded square for the canister), depth squashes Z.
        /// </summary>
        public static Mesh Lathe(string name, Ring[] profile, int segments = 32, Func<float, float> shape = null,
                                 float depth = 1f, float uMin = 0f, float uMax = 1f)
        {
            Vector2 Section(float a)
            {
                float m = shape?.Invoke(a) ?? 1f;
                return new Vector2(Mathf.Cos(a) * m, Mathf.Sin(a) * m * depth);
            }

            int rings = profile.Length;
            var ringNormals = new Vector2[rings];
            for (int i = 0; i < rings; i++)
            {
                Vector2 Point(int k) => new Vector2(profile[k].R, profile[k].Y);
                bool samePrev = i > 0 && Point(i) == Point(i - 1);
                bool sameNext = i < rings - 1 && Point(i) == Point(i + 1);
                Vector2 tangent;
                if (i == 0 || samePrev)
                {
                    tangent = Point(Math.Min(i + 1 + (sameNext ? 1 : 0), rings - 1)) - Point(i);
                }
                else if (i == rings - 1 || sameNext)
                {
                    tangent = Point(i) - Point(i - 1);
                }
                else
                {
                    tangent = Point(i + 1) - Point(i - 1);
                }

                ringNormals[i] = new Vector2(tangent.y, -tangent.x).normalized;
            }

            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();
            var triangles = new List<int>();
            const float eps = 0.001f;
            for (int i = 0; i < rings; i++)
            {
                for (int j = 0; j <= segments; j++)
                {
                    float u = (float)j / segments;
                    float a = u * Mathf.PI * 2f;
                    Vector2 p = Section(a);
                    Vector2 dp = Section(a + eps) - Section(a - eps);
                    Vector2 outward = new Vector2(dp.y, -dp.x).normalized;
                    vertices.Add(new Vector3(p.x * profile[i].R, profile[i].Y, p.y * profile[i].R));
                    normals.Add(new Vector3(outward.x * ringNormals[i].x, ringNormals[i].y, outward.y * ringNormals[i].x).normalized);
                    uvs.Add(new Vector2(uMin + (uMax - uMin) * u, profile[i].V));
                }
            }

            int cols = segments + 1;
            for (int i = 0; i < rings - 1; i++)
            {
                for (int j = 0; j < segments; j++)
                {
                    int a = i * cols + j, b = (i + 1) * cols + j, c = a + 1, d = b + 1;
                    triangles.AddRange(new[] { a, b, c, b, d, c });
                }
            }

            return Build(name, vertices, normals, uvs, triangles);
        }

        /// <summary>A box; each face gets its own rectangle of the texture (front is +Z).</summary>
        public static Mesh Box(string name, Vector3 center, Vector3 size, Rect front, Rect back, Rect left, Rect right, Rect top, Rect bottom)
        {
            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();
            var triangles = new List<int>();
            Vector3 h = size * 0.5f;

            void Face(Vector3 normal, Vector3 up, float width, float height, float distance, Rect uv)
            {
                Vector3 right = Vector3.Cross(up, -normal);    // screen right for someone looking at the face
                Vector3 c = center + normal * distance;
                int start = vertices.Count;
                vertices.Add(c - right * width / 2 - up * height / 2);
                vertices.Add(c + right * width / 2 - up * height / 2);
                vertices.Add(c + right * width / 2 + up * height / 2);
                vertices.Add(c - right * width / 2 + up * height / 2);
                uvs.Add(new Vector2(uv.xMin, uv.yMin));
                uvs.Add(new Vector2(uv.xMax, uv.yMin));
                uvs.Add(new Vector2(uv.xMax, uv.yMax));
                uvs.Add(new Vector2(uv.xMin, uv.yMax));
                for (int k = 0; k < 4; k++)
                {
                    normals.Add(normal);
                }

                triangles.AddRange(new[] { start, start + 3, start + 2, start, start + 2, start + 1 });
            }

            Face(Vector3.forward, Vector3.up, size.x, size.y, h.z, front);
            Face(Vector3.back, Vector3.up, size.x, size.y, h.z, back);
            Face(Vector3.left, Vector3.up, size.z, size.y, h.x, left);
            Face(Vector3.right, Vector3.up, size.z, size.y, h.x, right);
            Face(Vector3.up, Vector3.forward, size.x, size.z, h.y, top);
            Face(Vector3.down, Vector3.back, size.x, size.z, h.y, bottom);
            return Build(name, vertices, normals, uvs, triangles);
        }

        public static Mesh Box(string name, Vector3 center, Vector3 size, Rect all)
        {
            return Box(name, center, size, all, all, all, all, all, all);
        }

        public static Mesh Combine(string name, params (Mesh mesh, Matrix4x4 matrix)[] parts)
        {
            CombineInstance[] combine = parts.Select(p => new CombineInstance { mesh = p.mesh, transform = p.matrix }).ToArray();
            var mesh = new Mesh { name = name };
            mesh.CombineMeshes(combine, true, true);
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Mesh Build(string name, List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs, List<int> triangles)
        {
            var mesh = new Mesh { name = name };
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Matrix4x4 At(float x, float y, float z, float yaw = 0f, float pitch = 0f)
        {
            return Matrix4x4.TRS(new Vector3(x, y, z), Quaternion.Euler(pitch, yaw, 0f), Vector3.one);
        }

        /// <summary>Rounded square cross-section (superellipse) for the canister.</summary>
        private static float RoundedSquare(float a)
        {
            const float n = 4f;
            return 1f / Mathf.Pow(Mathf.Pow(Mathf.Abs(Mathf.Cos(a)), n) + Mathf.Pow(Mathf.Abs(Mathf.Sin(a)), n), 1f / n);
        }

        // ------------------------------------------------------------------ items

        /// <summary>500 ml can, 24 cm (Valheim items are a bit oversized). Label v 0.08-0.88.</summary>
        public static Mesh Can() => Lathe("kong_can", new[]
        {
            new Ring(0.000f, 0.006f, 0.00f), new Ring(0.040f, 0.000f, 0.03f), new Ring(0.050f, 0.010f, 0.06f),
            new Ring(0.050f, 0.018f, 0.08f), new Ring(0.050f, 0.208f, 0.88f), new Ring(0.046f, 0.226f, 0.92f),
            new Ring(0.042f, 0.236f, 0.95f), new Ring(0.043f, 0.240f, 0.96f), new Ring(0.038f, 0.236f, 0.98f),
            new Ring(0.000f, 0.232f, 1.00f),
        });

        /// <summary>The empty: a squashed, dented can.</summary>
        public static Mesh CrushedCan() => Lathe("kong_can_crushed", new[]
        {
            new Ring(0.000f, 0.006f, 0.00f), new Ring(0.044f, 0.000f, 0.03f), new Ring(0.054f, 0.010f, 0.06f),
            new Ring(0.058f, 0.040f, 0.30f), new Ring(0.050f, 0.075f, 0.50f), new Ring(0.060f, 0.110f, 0.70f),
            new Ring(0.050f, 0.140f, 0.92f), new Ring(0.043f, 0.150f, 0.96f), new Ring(0.000f, 0.146f, 1.00f),
        }, 24, a => 1f + 0.07f * Mathf.Sin(a * 3f) + 0.04f * Mathf.Sin(a * 7f + 1f));

        /// <summary>Skyr Energy bottle with a waist and a ribbed white cap.</summary>
        public static Mesh SkyrBottle() => Lathe("kong_skyr", new[]
        {
            new Ring(0.000f, 0.004f, 0.00f), new Ring(0.050f, 0.000f, 0.02f), new Ring(0.055f, 0.012f, 0.05f),
            new Ring(0.055f, 0.080f, 0.30f), new Ring(0.048f, 0.095f, 0.36f), new Ring(0.055f, 0.110f, 0.42f),
            new Ring(0.055f, 0.150f, 0.58f), new Ring(0.055f, 0.158f, 0.61f), new Ring(0.050f, 0.185f, 0.72f),
            new Ring(0.036f, 0.205f, 0.82f), new Ring(0.024f, 0.215f, 0.86f), new Ring(0.024f, 0.222f, 0.90f),
            new Ring(0.029f, 0.222f, 0.90f), new Ring(0.029f, 0.255f, 0.98f), new Ring(0.000f, 0.258f, 1.00f),
        });

        /// <summary>Mead style bottle for the mead bomb. Label v 0.2-0.62, cork v 0.92-1.</summary>
        public static Mesh MeadBottle() => Lathe("kong_meadbottle", new[]
        {
            new Ring(0.000f, 0.005f, 0.00f), new Ring(0.058f, 0.000f, 0.03f), new Ring(0.065f, 0.015f, 0.08f),
            new Ring(0.065f, 0.050f, 0.20f), new Ring(0.065f, 0.170f, 0.62f), new Ring(0.060f, 0.195f, 0.70f),
            new Ring(0.035f, 0.225f, 0.80f), new Ring(0.022f, 0.250f, 0.88f), new Ring(0.022f, 0.265f, 0.92f),
            new Ring(0.026f, 0.265f, 0.93f), new Ring(0.026f, 0.295f, 0.99f), new Ring(0.000f, 0.298f, 1.00f),
        });

        /// <summary>Small keg for the mead bomb base.</summary>
        public static Mesh Barrel() => Lathe("kong_barrel", new[]
        {
            new Ring(0.000f, 0.004f, 0.00f), new Ring(0.105f, 0.000f, 0.02f), new Ring(0.110f, 0.010f, 0.03f),
            new Ring(0.125f, 0.070f, 0.20f), new Ring(0.132f, 0.140f, 0.50f), new Ring(0.125f, 0.210f, 0.80f),
            new Ring(0.110f, 0.270f, 0.97f), new Ring(0.100f, 0.274f, 0.985f), new Ring(0.000f, 0.270f, 1.00f),
        }, 20);

        /// <summary>Skyr cup with a foil lid. Blue v 0.05-0.6, white v 0.6-0.95.</summary>
        public static Mesh Cup() => Lathe("kong_cup", new[]
        {
            new Ring(0.000f, 0.003f, 0.00f), new Ring(0.050f, 0.000f, 0.03f), new Ring(0.050f, 0.006f, 0.05f),
            new Ring(0.063f, 0.112f, 0.95f), new Ring(0.067f, 0.118f, 0.955f), new Ring(0.066f, 0.120f, 0.97f),
            new Ring(0.000f, 0.118f, 1.00f),
        });

        /// <summary>18.9 litre canister: rounded square body, white cap, handle on top. Front is +Z.</summary>
        public static Mesh Canister()
        {
            Mesh body = Lathe("kong_canister_body", new[]
            {
                new Ring(0.000f, 0.004f, 0.00f), new Ring(0.150f, 0.000f, 0.02f), new Ring(0.170f, 0.020f, 0.05f),
                new Ring(0.170f, 0.430f, 0.80f), new Ring(0.150f, 0.470f, 0.86f), new Ring(0.060f, 0.490f, 0.86f),
                new Ring(0.000f, 0.492f, 0.86f),
            }, 40, RoundedSquare, 0.7f);
            Mesh cap = Lathe("kong_canister_cap", new[]
            {
                new Ring(0.000f, 0.470f, 0.90f), new Ring(0.035f, 0.470f, 0.90f), new Ring(0.035f, 0.530f, 0.97f),
                new Ring(0.000f, 0.532f, 1.00f),
            }, 16, null, 1f, 0.62f, 0.98f);
            var yellow = new Rect(0.45f, 0.40f, 0.05f, 0.05f);
            Mesh post = Box("kong_canister_post", new Vector3(0f, 0.035f, 0f), new Vector3(0.03f, 0.07f, 0.045f), yellow);
            Mesh bar = Box("kong_canister_bar", Vector3.zero, new Vector3(0.15f, 0.03f, 0.045f), yellow);
            return Combine("kong_canister", (body, Matrix4x4.identity), (cap, At(-0.085f, 0f, 0f)),
                (post, At(0.02f, 0.47f, 0f)), (post, At(0.15f, 0.47f, 0f)), (bar, At(0.085f, 0.555f, 0f)));
        }

        // ------------------------------------------------------------------ pieces

        // fridge atlas: door x 0-0.5 full height, side x 0.5-1 top half, top x 0.5-0.75 bottom half, inside 0.75-1
        private static readonly Rect FridgeDoorUv = new Rect(0f, 0f, 0.5f, 1f);
        private static readonly Rect FridgeSide = new Rect(0.5f, 0.5f, 0.5f, 0.5f);
        private static readonly Rect FridgeTop = new Rect(0.5f, 0f, 0.25f, 0.5f);
        private static readonly Rect FridgeInside = new Rect(0.75f, 0f, 0.25f, 0.5f);

        public const float FridgeWidth = 0.9f, FridgeHeight = 1.6f, FridgeDepth = 0.75f, DoorThickness = 0.06f;

        public static Mesh FridgeBody() => Box("kong_fridge_body", new Vector3(0f, FridgeHeight / 2, 0f),
            new Vector3(FridgeWidth, FridgeHeight, FridgeDepth), FridgeInside, FridgeSide, FridgeSide, FridgeSide, FridgeTop, FridgeTop);

        /// <summary>The door with its hinge at the origin; it extends towards -X (screen left to right seen from the front).</summary>
        public static Mesh FridgeDoor() => Box("kong_fridge_door", new Vector3(-FridgeWidth / 2, FridgeHeight / 2, DoorThickness / 2),
            new Vector3(FridgeWidth, FridgeHeight, DoorThickness), FridgeDoorUv, FridgeSide, FridgeSide, FridgeSide, FridgeTop, FridgeTop);

        // deposit machine atlas (1024 x 1024, see make_textures.py): front x 0-0.5, side and back x 0.5-0.92 (both full
        // height, same aspect as the 1.0 x 1.9 x 0.8 m body), plain grey x 0.94-1 for top and bottom
        public static Mesh Automat()
        {
            var front = new Rect(0f, 0f, 0.5f, 1f);
            var side = new Rect(0.5f, 0f, 0.42f, 1f);
            var flat = new Rect(0.96f, 0.3f, 0.03f, 0.3f);
            return Box("kong_automat", new Vector3(0f, 0.95f, 0f), new Vector3(1.0f, 1.9f, 0.8f), front, side, side, side, flat, flat);
        }

        /// <summary>Cans of one row of the pyramid (rows 0-3 have 4, 3, 2, 1 cans), 10 empties in total.</summary>
        public static Mesh PyramidRow(int row)
        {
            Mesh can = Can();
            int count = 4 - row;
            const float spacing = 0.104f;
            var parts = new List<(Mesh, Matrix4x4)>();
            for (int i = 0; i < count; i++)
            {
                float x = (i - (count - 1) / 2f) * spacing;
                parts.Add((can, At(x, row * 0.241f, 0f, 37f * (i + row))));
            }

            return Combine("kong_pyramid_row" + row, parts.ToArray());
        }

        /// <summary>The wooden stand of the tap; the canister sits on top at 0.62 m.</summary>
        public static Mesh TapStand()
        {
            var all = new Rect(0f, 0f, 1f, 1f);
            Mesh plate = Box("kong_tap_plate", Vector3.zero, new Vector3(0.48f, 0.04f, 0.36f), all);
            Mesh leg = Box("kong_tap_leg", Vector3.zero, new Vector3(0.05f, 0.6f, 0.05f), all);
            return Combine("kong_tap_stand", (plate, At(0f, 0.60f, 0f)),
                (leg, At(0.2f, 0.3f, 0.14f)), (leg, At(-0.2f, 0.3f, 0.14f)), (leg, At(0.2f, 0.3f, -0.14f)), (leg, At(-0.2f, 0.3f, -0.14f)));
        }

        /// <summary>Canister plus a little spigot at the front.</summary>
        public static Mesh TapCanister()
        {
            Mesh spigot = Lathe("kong_spigot", new[]
            {
                new Ring(0.000f, 0.000f, 0.90f), new Ring(0.016f, 0.000f, 0.90f), new Ring(0.016f, 0.090f, 0.97f),
                new Ring(0.000f, 0.092f, 1.00f),
            }, 12, null, 1f, 0.62f, 0.98f);
            return Combine("kong_tap_canister", (Canister(), At(0f, 0.62f, 0f)), (spigot, At(0f, 0.68f, 0.10f, 0f, 90f)));
        }
    }
}
