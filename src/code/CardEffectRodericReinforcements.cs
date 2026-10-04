using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;

namespace mt2_freecompany.Plugin;

// Record actual deaths, not every monster card in the consumed pile. The native
// replay rebuilds this set when loading a battle; SetupCards starts a fresh battle.
internal static class RodericReinforcements
{
    internal static readonly HashSet<CardState> DeadCards = new HashSet<CardState>();

    internal static List<CardState> Candidates(CardManager cards, int maximumCost, out bool revive)
    {
        bool Eligible(CardState card) => card != null && card.IsMonsterCard()
            && !card.IsChampionCard() && card.GetSpawnCharacterData() != null
            && card.GetCostWithoutAnyModifications() >= 0
            && card.GetCostWithoutAnyModifications() <= maximumCost
            && !cards.IsCardInStandByPile(card);
        var found = cards.GetDrawPile().Where(Eligible).Distinct().ToList();
        revive = found.Count == 0;
        if (revive)
            found = cards.GetExhaustedPile().Concat(cards.GetEatenPile())
                .Where(c => DeadCards.Contains(c) && Eligible(c)).Distinct().ToList();
        return found;
    }

    internal static void RecordReturn(CardState card, CharacterState character)
    {
        if (character != null && (character.IsDead || character.GetHP() <= 0)) DeadCards.Add(card);
    }

    [HarmonyPatch(typeof(CardManager), nameof(CardManager.SetupCards))]
    private static class BattleResetPatch
    {
        private static void Prefix() => DeadCards.Clear();
    }

    [HarmonyPatch(typeof(CardManager), nameof(CardManager.OnMonsterCardReturned))]
    private static class DeathReturnPatch
    {
        private static void Prefix(CardState cardState, CardPile cardPile, CharacterState characterState)
        {
            var all = AllGameManagers.Instance;
            if (all == null || !all.AreMainSceneOnlyManagersInitialized()
                || all.GetCoreManagers().GetSaveManager().PreviewMode) return;
            RecordReturn(cardState, characterState);
        }
    }
}

public sealed class CardEffectRodericReinforcements : CardEffectBase
{
    public override bool CanApplyInPreviewMode => false;
    public override PropDescriptions CreateEditorInspectorDescriptions() => new PropDescriptions();

    private static CharacterState? Owner(CardEffectParams parameters)
        => parameters.characterThatActivatedAbility ?? parameters.selfTarget;

    public override bool TestEffect(CardEffectState effect, CardEffectParams parameters, ICoreGameManagers managers)
    {
        var owner = Owner(parameters);
        if (owner == null || owner.IsDead) return false;
        var room = owner.GetCurrentRoom();
        // Deliberately allow capacity overflow; only physical unit slots matter.
        if (room == null || room.GetNumCharacters(Team.Type.Monsters) >= 7
            || room.GetRemainingSpawnPointCount(Team.Type.Monsters) <= 0
            || room.GetMonsterPoint(owner.GetSpawnPoint().GetIndexInRoom() + 1) == null) return false;
        int cost = Math.Max(1, Math.Min(3, effect.GetParamInt()));
        return RodericReinforcements.Candidates(managers.GetCardManager(), cost, out _).Count > 0;
    }

    public override IEnumerator ApplyEffect(CardEffectState effect, CardEffectParams parameters,
        ICoreGameManagers managers, ISystemManagers systemManagers)
    {
        if (managers.GetSaveManager().PreviewMode || !TestEffect(effect, parameters, managers)) yield break;
        var owner = Owner(parameters)!;
        var room = owner.GetCurrentRoom();
        var cards = managers.GetCardManager();
        int cost = Math.Max(1, Math.Min(3, effect.GetParamInt()));
        var candidates = RodericReinforcements.Candidates(cards, cost, out bool revive);
        var card = candidates[RandomManager.Range(0, candidates.Count, RngId.CardDraw)];
        CharacterState? spawned = null;
        var location = room.GetMonsterPoint(owner.GetSpawnPoint().GetIndexInRoom() + 1);
        yield return managers.GetMonsterManager().CreateMonsterState(card.GetSpawnCharacterData(), card,
            owner.GetCurrentRoomIndex(), unit => spawned = unit, SpawnMode.SelectedSlot, location,
            afterCharacterSetup: unit =>
            {
                if (!revive) return;
                unit.SetHealth(1, Math.Max(1, unit.GetMaxHP()));
                unit.AddStatusEffect("undying", 1, CharacterState.defaultAddStatusEffectParams,
                    allowModification: false, allowDualism: false);
            }, fromPlayedCard: parameters.playedCard);
        if (spawned == null) yield break;
        // Move the same CardState, preserving its upgrades and preventing duplicates.
        cards.GetDrawPile().Remove(card);
        cards.GetExhaustedPile().Remove(card);
        cards.GetEatenPile().Remove(card);
        RodericReinforcements.DeadCards.Remove(card);
        var others = new List<CharacterState>();
        room.AddCharactersToList(others, Team.Type.Heroes | Team.Type.Monsters);
        foreach (var unit in others)
            if (unit != spawned) managers.GetCombatManager().QueueTrigger(unit, CharacterTriggerData.Trigger.CardMonsterPlayed);
        yield return cards.DiscardCard(new CardManager.DiscardCardParams
        {
            discardCard = card, wasPlayed = true, characterSummoned = spawned
        });
        cards.SetCardIsDrawable(card, drawable: true);
    }
}

// 2026-10-04-0556||codex-freecompany-fx||src\code\CardEffectRodericReinforcements.cs||implementa Call to Arms con fallback Not Yet, coste base 1/2/3, siete unidades, muerte real y Undying 1 sin modificadores
