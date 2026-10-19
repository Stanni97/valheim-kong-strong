using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Jotunn.Utils;
using UnityEngine;

namespace KongStrong
{
    /// <summary>Loads the PNGs embedded in the DLL (Assets/*.png), so the mod stays a single file.</summary>
    internal static class Icons
    {
        private static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();

        public static Sprite Get(string name)
        {
            if (Cache.TryGetValue(name, out Sprite cached))
            {
                return cached;
            }

            Texture2D texture = Texture(name);
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            Cache[name] = sprite;
            return sprite;
        }

        public static Texture2D Texture(string name)
        {
            string resource = $"KongStrong.{name}.png";
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource))
            {
                if (stream == null)
                {
                    throw new FileNotFoundException("Embedded resource missing: " + resource);
                }

                using (var buffer = new MemoryStream())
                {
                    stream.CopyTo(buffer);
                    return AssetUtils.LoadImage(buffer.ToArray());
                }
            }
        }
    }
}
