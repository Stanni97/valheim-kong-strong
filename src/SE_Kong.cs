using System.Collections.Generic;
using UnityEngine;

namespace KongStrong
{
    /// <summary>
    /// Buff of every Kong drink. Handles the caffeine tolerance when it starts and the sugar crash
    /// when it runs out. SEMan clones the effect with MemberwiseClone, so these fields survive.
    /// </summary>
    internal class SE_Kong : SE_Stats
    {
        internal static StatusEffect Crash;
        internal static StatusEffect HeartRacing;
        internal static ToleranceSettings Tolerance = new ToleranceSettings();

        public bool CausesCrash;

        public override void Setup(Character character)
        {
            // Must run before base.Setup, which applies the instant stamina and eitr.
            if (character != null && character == Player.m_localPlayer)
            {
                ApplyTolerance(character);
            }

            base.Setup(character);
        }

        public override void Stop()
        {
            base.Stop();

            // Only when the buff ran out, not when it was cleared by death or logout.
            if (CausesCrash && Crash != null && IsDone() && m_character != null && m_character == Player.m_localPlayer)
            {
                m_character.GetSEMan().AddStatusEffect(Crash.NameHash(), true);
            }
        }

        public static bool HasActiveDrink(Character character)
        {
            foreach (StatusEffect effect in character.GetSEMan().GetStatusEffects())
            {
                if (effect is SE_Kong)
                {
                    return true;
                }
            }

            return false;
        }

        private void ApplyTolerance(Character character)
        {
            if (!Tolerance.Enabled)
            {
                return;
            }

            int count = Caffeine.Register(Time.time, Tolerance.WindowMinutes * 60f);
            if (count < 2)
            {
                return;
            }

            float percent = count == 2 ? Tolerance.SecondDrinkPercent : Tolerance.FurtherDrinkPercent;
            Scale(Mathf.Clamp01(percent / 100f));
            character.Message(MessageHud.MessageType.TopLeft, "$msg_kong_tolerance");

            if (HeartRacing != null && Tolerance.HeartRacingFromDrink > 0 && count >= Tolerance.HeartRacingFromDrink)
            {
                character.GetSEMan().AddStatusEffect(HeartRacing.NameHash(), true);
            }
        }

        /// <summary>Weakens the bonuses of this instance; resistances stay as they are.</summary>
        private void Scale(float factor)
        {
            m_staminaUpFront *= factor;
            m_eitrUpFront *= factor;
            m_healthOverTime *= factor;
            m_staminaRegenMultiplier = ScaleMultiplier(m_staminaRegenMultiplier, factor);
            m_healthRegenMultiplier = ScaleMultiplier(m_healthRegenMultiplier, factor);
            m_eitrRegenMultiplier = ScaleMultiplier(m_eitrRegenMultiplier, factor);
            m_speedModifier *= factor;
            m_runStaminaDrainModifier *= factor;
            m_jumpStaminaUseModifier *= factor;
            m_addMaxCarryWeight *= factor;
        }

        private static float ScaleMultiplier(float multiplier, float factor)
        {
            return multiplier > 1f ? 1f + (multiplier - 1f) * factor : multiplier;
        }
    }

    internal sealed class ToleranceSettings
    {
        public bool Enabled = true;
        public float WindowMinutes = 15f;
        public int SecondDrinkPercent = 50;
        public int FurtherDrinkPercent = 25;
        public int HeartRacingFromDrink = 3;
    }

    /// <summary>When the local player drank Kong lately. Kept in memory only.</summary>
    internal static class Caffeine
    {
        private static readonly List<float> Drinks = new List<float>();

        /// <summary>Records a drink and returns how many were drunk within the window, this one included.</summary>
        public static int Register(float now, float window)
        {
            Drinks.RemoveAll(time => now - time > window || time > now);
            Drinks.Add(now);
            return Drinks.Count;
        }
    }
}
