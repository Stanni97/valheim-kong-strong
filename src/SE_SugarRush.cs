namespace KongStrong
{
    /// <summary>
    /// Buff of the sugared drink: when it runs out, the sugar crash debuff follows.
    /// </summary>
    internal class SE_SugarRush : SE_Stats
    {
        internal static StatusEffect Crash;

        public override void Stop()
        {
            base.Stop();

            // Only when the buff ran out, not when it was cleared by death or logout.
            if (Crash != null && IsDone() && m_character != null && m_character == Player.m_localPlayer)
            {
                m_character.GetSEMan().AddStatusEffect(Crash.NameHash(), true);
            }
        }
    }
}
