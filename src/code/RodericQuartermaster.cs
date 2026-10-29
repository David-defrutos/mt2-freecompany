using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using HarmonyLib;

namespace mt2_freecompany.Plugin;

// Native AddNewCard and equipment standby return do the actual card operations.
// The missing pieces are per-Quartermaster turn limits and the first eligible death.
internal static class RodericQuartermaster
{
    private sealed class TurnUsage
    {
        internal int Turn = -1;
        internal bool Supplied;
        internal bool Salvaged;
    }

    private static ConditionalWeakTable<CharacterState, TurnUsage> usage = new();
    private static ConditionalWeakTable<CharacterState, object> salvagedUnits = new();

    internal static void Reset()
    {
        usage = new();
        salvagedUnits = new();
    }

    internal static CardUpgradeData? FindUpgrade(CharacterState unit, AllGameData data, out int level)
    {
        level = 0;
        if (unit == null) return null;
        var card = unit.GetSpawnerCard();
        if (card == null || card.GetAssetName() != MyPluginInfo.PLUGIN_GUID + "-Card-SpawnRoderic") return null;
        for (int tier = 3; tier >= 1; --tier)
        {
            string name = MyPluginInfo.PLUGIN_GUID + "-Upgrade-upg_Quarter" + tier;
            var upgrade = data.GetAllCardUpgradeData().FirstOrDefault(u => u != null && u.name == name);
            if (upgrade != null && (unit.HasUpgrade(upgrade) || card.HasUpgrade(upgrade)))
            {
                level = tier;
                return upgrade;
            }
        }
        return null;
    }

    private static IEnumerable<(CharacterState owner, int level)> Owners(ICoreGameManagers managers)
    {
        var characters = new List<CharacterState>();
        managers.GetMonsterManager().AddCharactersToList(characters);
        foreach (var unit in characters)
        {
            if (unit == null || unit.IsDestroyed || unit.IsDead || unit.GetHP() <= 0
                || unit.HasStatusEffect("silenced") || unit.HasStatusEffect("muted")) continue;
            if (FindUpgrade(unit, managers.GetAllGameData(), out int tier) != null)
                yield return (unit, tier);
        }
    }

    private static TurnUsage Usage(CharacterState owner, ICoreGameManagers managers)
    {
        var state = usage.GetValue(owner, _ => new TurnUsage());
        int turn = managers.GetCombatManager().GetTurnCount();
        if (state.Turn != turn)
        {
            state.Turn = turn;
            state.Supplied = false;
            state.Salvaged = false;
        }
        return state;
    }

    internal static void Supply(CharacterState summoned, ICoreGameManagers managers)
    {
        if (managers.GetSaveManager().PreviewMode || summoned == null || summoned.PreviewMode
            || summoned.IsDestroyed || summoned.IsDead || summoned.GetTeamType() != Team.Type.Monsters) return;
        foreach (var (owner, tier) in Owners(managers))
        {
            if (ReferenceEquals(owner, summoned)) continue;
            var state = Usage(owner, managers);
            if (state.Supplied) continue;
            var data = CardEffectRequisition.FindKitData(summoned, tier, managers);
            if (data == null) continue;
            var cards = managers.GetCardManager();
            // Match native return-to-hand overflow: preserve the card in the draw pile.
            var destination = cards.GetBelowHandSize() ? CardPile.HandPile : CardPile.DeckPile;
            // Reserve before native hand-entry callbacks, which may summon more units.
            state.Supplied = true;
            var card = cards.AddNewCard(data, destination, false, false, null, true);
            if (card == null) { state.Supplied = false; continue; }
            owner.ShowNotification((MyPluginInfo.PLUGIN_GUID + "_QuartermasterSupply").Localize(), PopupNotificationUI.Source.Trigger);
        }
    }

    internal static CardPile Salvage(CharacterState dead, CardState equipment, CardPile nativeReturn,
        ICoreGameManagers managers)
    {
        if (managers.GetSaveManager().PreviewMode || dead == null || dead.PreviewMode
            || dead.GetTeamType() != Team.Type.Monsters || (!dead.IsDead && dead.GetHP() > 0)
            || nativeReturn == CardPile.KeepInStandBy || nativeReturn == CardPile.None
            || !dead.GetEquipment().Contains(equipment)) return nativeReturn;

        // One death claims the whole set. Repeated native condition checks must not
        // spend another charge; merged equipment remains the exact same CardState.
        if (!salvagedUnits.TryGetValue(dead, out _))
        {
            foreach (var (owner, _) in Owners(managers))
            {
                var state = Usage(owner, managers);
                if (state.Salvaged) continue;
                state.Salvaged = true;
                salvagedUnits.Add(dead, new object());
                owner.ShowNotification((MyPluginInfo.PLUGIN_GUID + "_QuartermasterSalvage").Localize(), PopupNotificationUI.Source.Trigger);
                break;
            }
            if (!salvagedUnits.TryGetValue(dead, out _)) return nativeReturn;
        }
        return managers.GetCardManager().GetBelowHandSize() ? CardPile.HandPile : CardPile.DeckPile;
    }

    private static ICoreGameManagers? LiveManagers()
    {
        var all = AllGameManagers.Instance;
        return all != null && all.AreMainSceneOnlyManagersInitialized() ? all.GetCoreManagers() : null;
    }

    [HarmonyPatch(typeof(CharacterState), nameof(CharacterState.OnSpawn))]
    private static class SupplyPatch
    {
        private static void Postfix(CharacterState __instance, ref IEnumerator __result)
            => __result = BeforeSpawn(__instance, __result);

        private static IEnumerator BeforeSpawn(CharacterState unit, IEnumerator original)
        {
            // Run before Summon triggers so nested summons cannot take the first slot.
            var managers = LiveManagers();
            if (managers != null) Supply(unit, managers);
            while (original.MoveNext()) yield return original.Current;
        }
    }

    [HarmonyPatch(typeof(CardManager), nameof(CardManager.CheckEquipmentRemoveFromStandByCondition))]
    private static class SalvagePatch
    {
        private static void Postfix(CharacterState character, CardState equipmentCard, ref CardPile __result)
        {
            var managers = LiveManagers();
            if (managers != null) __result = Salvage(character, equipmentCard, __result, managers);
        }
    }

    [HarmonyPatch(typeof(CardManager), nameof(CardManager.SetupCards))]
    private static class BattleResetPatch
    {
        private static void Prefix()
        {
            // Preview setup must not reset the real battle's per-turn limits.
            var managers = LiveManagers();
            if (managers == null || !managers.GetSaveManager().PreviewMode) Reset();
        }
    }

    [HarmonyPatch(typeof(CharacterTooltipHelper), nameof(CharacterTooltipHelper.GetAdditionalTooltipContents))]
    private static class RulesTooltipPatch
    {
        private const string TooltipId = "mt2_freecompany.QuartermasterRules";
        private static void Postfix(CharacterState characterState, SaveManager saveManager,
            ref IEnumerable<(TooltipContent content, bool allowDuplicates)> __result)
            => __result = AppendRules(__result, characterState, saveManager);

        internal static IEnumerable<(TooltipContent content, bool allowDuplicates)> AppendRules(
            IEnumerable<(TooltipContent content, bool allowDuplicates)> original, CharacterState unit, SaveManager save)
        {
            bool shown = false;
            foreach (var item in original)
            {
                if (item.content.tooltipId == TooltipId) shown = true;
                yield return item;
            }
            if (shown || unit == null || save == null) yield break;
            var data = save.GetAllGameData();
            if (data == null) yield break;
            var upgrade = FindUpgrade(unit, data, out _);
            if (upgrade == null) yield break;
            var state = new CardUpgradeState();
            state.Setup(upgrade);
            string body = state.GetUpgradeDescriptionKey().Localize(new CardEffectLocalizationContext(upgrade, null, unit.GetSpawnerCard()));
            yield return (new TooltipContent(state.GetUpgradeTitle(), body,
                TooltipDesigner.TooltipDesignType.Keyword, TooltipId), false);
        }
    }
}

// 2026-10-29-1402||codex-freecompany-fx||src/code/RodericQuartermaster.cs||implementa Suministro y Recuperacion pasivos por turno; usa alta nativa de cartas y retorno de mismos equipos desde standby; tooltip localizado

// 2026-10-29-1406||codex-freecompany-fx||src/code/RodericQuartermaster.cs||guarda nula estable de carta y evita resetear limites reales desde inicializacion de preview
