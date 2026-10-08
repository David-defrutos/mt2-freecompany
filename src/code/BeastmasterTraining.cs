using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HarmonyLib;
using Newtonsoft.Json.Linq;

namespace mt2_freecompany.Plugin;

internal static class BeastmasterTraining
{
    internal const string TrainingId = "bde5128c-6744-4365-8915-fc62f9f5369e";
    internal const string DiscountId = "639a283d-790f-4f1a-b0df-de28d8796027";
    private static HashSet<string>? creatureNames;

    // Exact card names, including the owning mod. No hard dependency on other clans.
    internal static bool IsCreature(CardState card)
    {
        if (card == null || card.GetCardType() != CardType.Monster) return false;
        if (creatureNames == null)
        {
            string path = Path.Combine(Path.GetDirectoryName(typeof(Plugin).Assembly.Location)!,
                "json", "champions", "champion_vesper_beastmaster.json");
            var entries = (JArray?)JObject.Parse(File.ReadAllText(path))["beastmaster_creatures"];
            creatureNames = new HashSet<string>(StringComparer.Ordinal);
            foreach (JObject entry in entries ?? new JArray())
            {
                string id = (string)entry["card"]!;
                string? mod = (string?)entry["mod"];
                if (string.IsNullOrEmpty(mod))
                {
                    creatureNames.Add(id);
                    creatureNames.Add("CardData_" + id);
                }
                else creatureNames.Add(mod + "-Card-" + id);
            }
        }
        return creatureNames.Contains(card.GetAssetName()) || creatureNames.Contains(card.GetCardDataID());
    }

    internal static int GetLevel(CharacterState character, ICoreGameManagers managers)
    {
        if (character == null || character.IsDead || character.HasStatusEffect("silenced") || character.HasStatusEffect("muted"))
            return 0;
        var card = character.GetSpawnerCard();
        for (int level = 3; level >= 1; level--)
        {
            string name = MyPluginInfo.PLUGIN_GUID + "-Upgrade-upg_Beastmaster" + level;
            var upgrade = managers.GetAllGameData().GetAllCardUpgradeData().FirstOrDefault(u => u != null && u.name == name);
            if (upgrade != null && (character.HasUpgrade(upgrade) || (card != null && card.HasUpgrade(upgrade))))
                return level;
        }
        return 0;
    }

    private static List<int> GetLevels(ICoreGameManagers managers)
    {
        var characters = new List<CharacterState>();
        managers.GetMonsterManager().AddCharactersToList(characters);
        return characters.Select(c => GetLevel(c, managers)).Where(l => l > 0).ToList();
    }

    internal static void Freeze(CardState card, ICoreGameManagers managers)
    {
        if (managers.GetSaveManager().PreviewMode || !IsCreature(card) || GetLevels(managers).Count == 0) return;
        Tame(card, managers);
        if (card.HasTrait(typeof(CardTraitFreeze))) return;
        var trait = new CardTraitData();
        trait.Setup("CardTraitFreeze");
        managers.GetCardManager().AddTemporaryTraitToCard(card, trait);
    }

    internal static void TrainHand(ICoreGameManagers managers)
    {
        var save = managers.GetSaveManager();
        if (save.PreviewMode) return;
        var levels = GetLevels(managers);
        if (levels.Count == 0) return;
        foreach (var card in managers.GetCardManager().GetHand().ToArray())
        {
            if (!IsCreature(card)) continue;
            Tame(card, managers);
            var oldDiscount = card.GetTemporaryCardStateModifiers().GetCardUpgrades().FirstOrDefault(u => u.GetCardUpgradeDataId() == DiscountId);
            var discount = new CardUpgradeState();
            discount.Setup();
            discount.SetCardUpgradeDataId(DiscountId);
            discount.SetIsUnique(true);
            discount.SetCostReduction(levels.Count + (oldDiscount?.GetCostReduction() ?? 0));
            if (oldDiscount != null) card.RemoveUpgrade(oldDiscount, card.GetTemporaryCardStateModifiers());
            // Temporary modifiers reset between battles, but survive ordinary pile changes.
            card.ApplyTemporaryUpgrade(discount, save);
            card.UpdateCardBodyText();
            managers.GetCardManager().RefreshCardInHand(card, cleanupTweens: false);
        }
    }

    internal static void Tame(CardState card, ICoreGameManagers managers)
    {
        var save = managers.GetSaveManager();
        if (save.PreviewMode || !IsCreature(card) || GetLevels(managers).Count == 0) return;
        if (card.GetCardStateModifiers().GetCardUpgrades().Any(u => u.GetCardUpgradeDataId() == TrainingId)) return;
        // Registered zero-stat marker. Keep older earned stat upgrades intact.
        var marker = new CardUpgradeState();
        marker.Setup();
        marker.SetCardUpgradeDataId(TrainingId);
        marker.SetIsUnique(true);
        card.ApplyPermanentUpgrade(marker, save, ignoreUpgradeAnimation: true);
    }

    internal static IEnumerator TrainDeployed(ICoreGameManagers managers)
    {
        if (managers.GetSaveManager().PreviewMode || GetLevels(managers).Count == 0) yield break;
        var units = new List<CharacterState>();
        managers.GetMonsterManager().AddCharactersToList(units);
        foreach (var unit in units)
        {
            if (unit == null || unit.IsDead || unit.GetHP() <= 0) continue;
            var card = unit.GetSpawnerCard();
            if (card == null || !IsCreature(card)) continue;
            Tame(card, managers);
            BeastmasterMarkers.MarkTrained(unit);
        }
    }

    internal static void RecallTroll(ICoreGameManagers managers)
    {
        if (managers.GetSaveManager().PreviewMode || GetLevels(managers).Count == 0) return;
        var cards = managers.GetCardManager();
        string prefix = MyPluginInfo.PLUGIN_GUID + "-Card-Spawn";
        // Never pull a deployed or dead troll out of its standby/cemetery pile.
        foreach (var card in cards.GetHand().Concat(cards.GetDrawPile()).ToArray())
        {
            if (!card.GetAssetName().StartsWith(prefix, StringComparison.Ordinal) || !IsCreature(card)) continue;
            if (!cards.GetHand().Contains(card) && !cards.DrawSpecificCard(card, drawSource: HandUI.DrawSource.Deck)) return;
            Freeze(card, managers);
            return;
        }
    }

    private static IEnumerator BeforeTurn(IEnumerator original)
    {
        var managers = AllGameManagers.Instance?.GetCoreManagers();
        if (managers != null)
        {
            TrainHand(managers);
            yield return TrainDeployed(managers);
        }
        while (original.MoveNext()) yield return original.Current;
    }

    [HarmonyPatch(typeof(CombatManager), "RunMonsterTurn")]
    private static class StartTurnPatch
    {
        [HarmonyPostfix]
        private static void Postfix(ref IEnumerator __result) => __result = BeforeTurn(__result);
    }

    [HarmonyPatch(typeof(RelicManager), nameof(RelicManager.ApplyCardAddedToHandRelicEffects))]
    private static class HandEntryPatch
    {
        [HarmonyPostfix]
        private static void Postfix(CardState cardState)
        {
            var managers = AllGameManagers.Instance?.GetCoreManagers();
            if (managers != null) Freeze(cardState, managers);
        }
    }
}

public sealed class CardEffectBeastmasterRecall : CardEffectBase
{
    public override bool CanApplyInPreviewMode => false;
    public override PropDescriptions CreateEditorInspectorDescriptions() => new();
    public override bool TestEffect(CardEffectState state, CardEffectParams parameters, ICoreGameManagers managers) => true;
    public override IEnumerator ApplyEffect(CardEffectState state, CardEffectParams parameters, ICoreGameManagers managers, ISystemManagers systems)
    {
        BeastmasterTraining.RecallTroll(managers);
        yield break;
    }
}
// 2026-10-03-2242||codex-freecompany-fx||src/code/BeastmasterTraining.cs||Frozen en entrada de mano, entrenamiento permanente antes del robo por cada Vesper y recuperación segura del troll
// 2026-10-03-2244||codex-freecompany-fx||src/code/BeastmasterTraining.cs||recuperación con origen Deck válido en la API
// 2026-10-03-2248||codex-freecompany-fx||src/code/BeastmasterTraining.cs||identifica el troll por asset name estable, no por GUID interno

// Older builds saved unregistered names; translate them before native loading.
// Native LoadFromFile then restores metadata while retaining saved stat values.
[HarmonyPatch(typeof(CardUpgradeState), nameof(CardUpgradeState.LoadFromFile))]
internal static class BeastmasterSavedUpgradePatch
{
    internal static void Prefix(CardUpgradeState __instance, AllGameData allGameData)
    {
        string id = __instance.GetCardUpgradeDataId();
        string? registered = id == "mt2_freecompany.BeastmasterTraining" ? BeastmasterTraining.TrainingId
            : id == "mt2_freecompany.BeastmasterDiscount" ? BeastmasterTraining.DiscountId : null;
        if (registered != null && allGameData.FindCardUpgradeData(registered) != null)
            __instance.SetCardUpgradeDataId(registered);
    }
}

// 2026-10-04-0028||codex-freecompany-fx||src\code\BeastmasterTraining.cs||usa GUIDs reales de mejoras registradas y migra IDs antiguos antes de cargar guardados

// 2026-10-08-1936||codex-freecompany-fx||C:\Users\david\AppData\Roaming\Thunderstore Mod Manager\DataFolder\MonsterTrain2\profiles\Default\BepInEx\plugins\David-FreeCompany\src\code\BeastmasterTraining.cs||entrena criaturas vivas desplegadas en cualquier piso; mejora permanente una vez por carta y bono inmediato de ataque/vida por unidad; descuento solo en mano

// 2026-10-08-1944||codex-freecompany-fx||C:\Users\david\AppData\Roaming\Thunderstore Mod Manager\DataFolder\MonsterTrain2\profiles\Default\BepInEx\plugins\David-FreeCompany\src\code\BeastmasterTraining.cs||sustituye crecimiento de estadísticas por marcador permanente Tamed sin atributos; conserva Frozen y descuento Ember; respeta mejoras ya ganadas
