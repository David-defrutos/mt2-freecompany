using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using ShinyShoe;

namespace mt2_freecompany.Plugin;

internal static class BeastmasterMarkers
{
    internal const string StatusId = "mt2_freecompany.plugin_tamed";

    internal static CardUpgradeState? HandMarker(CardState? card)
    {
        // Deck/shop screens render these same UI components without combat managers.
        var all = AllGameManagers.Instance;
        if (card == null || all == null || !all.AreMainSceneOnlyManagersInitialized()) return null;
        var cardManager = all.GetCardManager();
        if (cardManager == null || !cardManager.IsCardInHand(card)
            || !BeastmasterTraining.IsCreature(card)) return null;
        var managers = all.GetCoreManagers();
        var data = managers.GetAllGameData()?.GetAllCardUpgradeData()
            ?.FirstOrDefault(u => u != null && u.name == MyPluginInfo.PLUGIN_GUID + "-Upgrade-TamableMarker");
        if (data == null) return null;
        var marker = new CardUpgradeState();
        marker.Setup(data);
        return marker;
    }

    internal static void MarkTrained(CharacterState unit)
    {
        var all = AllGameManagers.Instance;
        if (all == null || !all.AreMainSceneOnlyManagersInitialized()) return;
        var managers = all.GetCoreManagers();
        var card = unit.GetSpawnerCard();
        if (managers == null || managers.GetSaveManager().PreviewMode || card == null
            || unit.HasStatusEffect(StatusId)) return;
        if (!card.GetCardStateModifiers().GetCardUpgrades()
            .Any(u => u.GetCardUpgradeDataId() == BeastmasterTraining.TrainingId)) return;
        unit.AddStatusEffect(StatusId, 1, CharacterState.defaultAddStatusEffectParams,
            allowModification: false, allowDualism: false, fromPermanentUpgrade: true,
            skipDefaultStatusEffectNotifs: true);
    }
}

// This state exists only in a copied UI list. It is never attached to the card,
// so it cannot occupy an upgrade slot or affect relics that count upgrades.
[HarmonyPatch(typeof(CardUpgradeDisplayContainer), nameof(CardUpgradeDisplayContainer.DisplayUpgrades))]
internal static class BeastmasterHandIconPatch
{
    private static void Prefix(CardUpgradeDisplayContainer __instance,
        ref List<CardUpgradeState> temporaryUpgrades, bool ____isRegionRunDisplay)
    {
        if (____isRegionRunDisplay) return;
        var cardUI = __instance.GetComponentInParent<CardUI>();
        var marker = BeastmasterMarkers.HandMarker(cardUI?.GetCardState());
        if (marker == null) return;
        temporaryUpgrades = new List<CardUpgradeState>(temporaryUpgrades);
        temporaryUpgrades.Add(marker);
    }
}

[HarmonyPatch(typeof(CardTooltipContainer), "AddTooltipsForCardState")]
internal static class BeastmasterHandTooltipPatch
{
    private static void Postfix(CardTooltipContainer __instance, CardState cardState, SaveManager saveManager)
    {
        var marker = BeastmasterMarkers.HandMarker(cardState);
        if (marker != null) __instance.AddTooltipsCardUpgrade(cardState, marker, saveManager);
    }
}

[HarmonyPatch(typeof(CharacterState), nameof(CharacterState.OnSpawn))]
internal static class BeastmasterTamedSpawnPatch
{
    private static void Postfix(CharacterState __instance, ref IEnumerator __result)
        => __result = AfterSpawn(__instance, __result);

    private static IEnumerator AfterSpawn(CharacterState unit, IEnumerator original)
    {
        while (original.MoveNext()) yield return original.Current;
        BeastmasterMarkers.MarkTrained(unit);
    }
}

public sealed class StatusEffectFreeCompanyTamedState : StatusEffectState
{
    public override bool TestTrigger(InputTriggerParams input, OutputTriggerParams output,
        ICoreGameManagers core) => false;
}

// 2026-10-03-2350||codex-freecompany-fx||src\code\BeastmasterMarkers.cs||añade indicadores visuales sin modificar mejoras reales y marca criaturas entrenadas al desplegar

// 2026-10-04-0016||codex-freecompany-fx||src\code\BeastmasterMarkers.cs||evita accesos a gestores de combate desde tienda/mazo y protege inicialización de marcadores

// Summon is intentionally hidden in native character tooltips. The training
// mechanic runs in code, so show the actual champion path description explicitly.
[HarmonyPatch(typeof(CharacterTooltipHelper), nameof(CharacterTooltipHelper.GetAdditionalTooltipContents))]
internal static class BeastmasterChampionTooltipPatch
{
    private const string TooltipId = "mt2_freecompany.BeastmasterRules";

    private static void Postfix(CharacterState characterState, SaveManager saveManager,
        ref IEnumerable<(TooltipContent content, bool allowDuplicates)> __result)
        => __result = AppendRules(__result, characterState, saveManager);

    internal static IEnumerable<(TooltipContent content, bool allowDuplicates)> AppendRules(
        IEnumerable<(TooltipContent content, bool allowDuplicates)> original,
        CharacterState character, SaveManager save)
    {
        bool alreadyShown = false;
        foreach (var item in original)
        {
            if (item.content.tooltipId == TooltipId) alreadyShown = true;
            yield return item;
        }
        if (alreadyShown || character == null || save == null) yield break;
        var data = save.GetAllGameData();
        if (data == null) yield break;
        var card = character.GetSpawnerCard();
        for (int level = 3; level >= 1; level--)
        {
            string name = MyPluginInfo.PLUGIN_GUID + "-Upgrade-upg_Beastmaster" + level;
            var upgrade = data.GetAllCardUpgradeData().FirstOrDefault(u => u != null && u.name == name);
            if (upgrade == null || !(character.HasUpgrade(upgrade) || (card != null && card.HasUpgrade(upgrade))))
                continue;
            var state = new CardUpgradeState();
            state.Setup(upgrade);
            string body = state.GetUpgradeDescriptionKey().Localize(new CardEffectLocalizationContext(upgrade, null, card));
            yield return (new TooltipContent(state.GetUpgradeTitle(), body,
                TooltipDesigner.TooltipDesignType.Keyword, TooltipId), false);
            yield break;
        }
    }
}

// 2026-10-04-0044||codex-freecompany-fx||src\code\BeastmasterMarkers.cs||muestra reglas localizadas de Beastmaster según senda real en tooltip de Vesper, sin añadir triggers
