// Documento generado el 2026-09-28-2313
using System;
using System.Collections.Generic;
using HarmonyLib;

namespace mt2_freecompany.Plugin
{
    /// <summary>Reproduce cues globales del juego sin redistribuir sus archivos de audio.</summary>
    [HarmonyPatch(typeof(CardManager), nameof(CardManager.PlayAnyCard))]
    internal static class FreeCompanyCardSounds
    {
        private static readonly HashSet<string> DamageCards = new(StringComparer.Ordinal)
        {
            "MagicMissile", "Cloudkill", "DelayedBlastFireball", "FingerOfDeath",
            "Fireball", "LightningBolt", "BountyHunterShot", "HedgeMageVolley",
            "ShieldbearerAbility", "TwoHeadedWrath", "GrimoireSpell1",
            "GrimoireSpell2", "GrimoireSpell3"
        };

        private static readonly HashSet<string> HealCards = new(StringComparer.Ordinal)
        {
            "CureWounds", "Resurrection", "BattleChaplainAbility",
            "ChirurgeonSurgery1", "ChirurgeonSurgery2", "ChirurgeonSurgery3"
        };

        private static readonly HashSet<string> DebuffCards = new(StringComparer.Ordinal)
        {
            "Banishment", "Fear", "Grease", "MassHoldPerson", "Silence", "Web",
            "DoubleShift"
        };

        private static readonly HashSet<string> MoveCards = new(StringComparer.Ordinal)
        {
            "DimensionDoor", "LongbowmanShiftAim"
        };

        private static readonly HashSet<string> GoldCards = new(StringComparer.Ordinal)
        {
            "SeverancePay", "CutpurseAbility"
        };

        [HarmonyPostfix]
        private static void AfterPlay(CardState cardState, bool fromDirectPlay, bool __result)
        {
            if (!__result || !fromDirectPlay || cardState == null)
                return;

            var prefix = MyPluginInfo.PLUGIN_GUID + "-Card-";
            var cardDataId = cardState.GetCardDataID();
            if (cardDataId == null || !cardDataId.StartsWith(prefix, StringComparison.Ordinal))
                return;

            var id = cardDataId.Substring(prefix.Length);
            string cue;
            if (DamageCards.Contains(id) || id.StartsWith("RodericSlam", StringComparison.Ordinal))
                cue = "Combat_Attack";
            else if (HealCards.Contains(id))
                cue = "Combat_Heal";
            else if (DebuffCards.Contains(id))
                cue = "Combat_Debuff";
            else if (MoveCards.Contains(id))
                cue = "Combat_Ascend";
            else if (GoldCards.Contains(id))
                cue = "Node_Coins";
            else if (cardState.IsMonsterCard())
                cue = "Combat_Spawn";
            else
                cue = "Combat_Buff";

            SoundManager.PlaySfxSignal.Dispatch(cue);
        }
    }
}
// 2026-09-28-2313||codex-freecompany-fx||src/code/FreeCompanyCardSounds.cs||asocia cartas del clan a cues globales originales al jugarlas con éxito
