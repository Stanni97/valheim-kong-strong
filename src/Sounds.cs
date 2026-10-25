using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Jotunn.Managers;
using UnityEngine;

namespace KongStrong
{
    /// <summary>Sounds embedded in the DLL (Assets/*.wav), played through Valheim's own SFX objects.</summary>
    internal static class Sounds
    {
        /// <summary>Raises the clip to near full scale, so the shout is as loud as the game's mixer allows.</summary>
        public static AudioClip Normalize(AudioClip clip, float boost = 1f)
        {
            var samples = new float[clip.samples * clip.channels];
            clip.GetData(samples, 0);
            float peak = 0f;
            foreach (float s in samples)
            {
                peak = Math.Max(peak, Math.Abs(s));
            }

            if (peak > 0.001f)
            {
                // Full scale first, then push it into a soft limiter: louder on average, no hard clipping.
                float gain = 0.98f / peak * Math.Max(1f, boost);
                float norm = (float)Math.Tanh(Math.Max(1f, boost));
                for (int i = 0; i < samples.Length; i++)
                {
                    samples[i] = boost <= 1f ? samples[i] * gain : (float)Math.Tanh(samples[i] * gain / 0.98f * 0.98f) / norm * 0.98f;
                }

                clip.SetData(samples, 0);
            }

            return clip;
        }

        /// <summary>Reads a 16 bit PCM WAV file from disk into an AudioClip.</summary>
        public static AudioClip LoadWavFile(string path)
        {
            return FromWav(Path.GetFileNameWithoutExtension(path), File.ReadAllBytes(path));
        }

        public static AudioClip FromWav(string name, byte[] data)
        {
            int channels = 1, rate = 22050, bits = 16, offset = 12, dataStart = -1, dataLength = 0;
            while (offset + 8 <= data.Length)
            {
                string id = System.Text.Encoding.ASCII.GetString(data, offset, 4);
                int size = BitConverter.ToInt32(data, offset + 4);
                if (id == "fmt ")
                {
                    channels = BitConverter.ToInt16(data, offset + 10);
                    rate = BitConverter.ToInt32(data, offset + 12);
                    bits = BitConverter.ToInt16(data, offset + 22);
                }
                else if (id == "data")
                {
                    dataStart = offset + 8;
                    dataLength = Math.Min(size, data.Length - dataStart);
                    break;
                }

                offset += 8 + size + (size & 1);
            }

            if (dataStart < 0 || bits != 16)
            {
                throw new InvalidDataException(name + ".wav is not 16 bit PCM");
            }

            int count = dataLength / 2;
            var samples = new float[count];
            for (int i = 0; i < count; i++)
            {
                samples[i] = BitConverter.ToInt16(data, dataStart + i * 2) / 32768f;
            }

            AudioClip clip = AudioClip.Create(name, count / channels, channels, rate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        /// <summary>
        /// Clones the first SFX prefab of the template effect list and lets it play the clip instead,
        /// so the sound goes through the game's mixer and volume settings.
        /// </summary>
        public static GameObject CreateSfx(string name, AudioClip clip, EffectList template, float volume, float delay)
        {
            GameObject source = template?.m_effectPrefabs?
                .Select(e => e.m_prefab)
                .FirstOrDefault(p => p != null && p.GetComponent<ZSFX>() != null);
            if (source == null)
            {
                return null;
            }

            GameObject sfx = PrefabManager.Instance.CreateClonedPrefab(name, source);
            ZSFX z = sfx.GetComponent<ZSFX>();
            z.m_audioClips = new[] { clip };
            z.m_minPitch = z.m_maxPitch = 1f;
            z.m_minVol = z.m_maxVol = volume;
            z.m_minDelay = z.m_maxDelay = delay;
            z.m_closedCaptionToken = "";
            z.m_secondaryCaptionToken = "";
            return sfx;
        }

        /// <summary>The template's effects plus one more prefab.</summary>
        public static EffectList With(EffectList template, GameObject extra)
        {
            var effects = (template?.m_effectPrefabs ?? new EffectList.EffectData[0]).ToList();
            if (extra != null)
            {
                effects.Add(new EffectList.EffectData { m_prefab = extra, m_enabled = true, m_variant = -1 });
            }

            return new EffectList { m_effectPrefabs = effects.ToArray() };
        }
    }
}
