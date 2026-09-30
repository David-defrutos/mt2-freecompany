using System.Collections;

namespace mt2_freecompany.Plugin;

public sealed class RelicEffectFirstUnitDiscountEachTurn : RelicEffectBase,
    ITurnPhaseStartOfPlayerTurnBeforeDrawRelicEffect,
    ITurnPhaseEndOfTurnRelicEffect,
    IOnCardAddedToHandRelicEffect,
    ICardPlayedRelicEffect
{
    private readonly CardUpgradeState discount = new();
    private bool available = true;

    public override PropDescriptions CreateEditorInspectorDescriptions() => new();

    public override void Initialize(RelicState relicState, RelicData relicData, RelicEffectData relicEffectData)
    {
        base.Initialize(relicState, relicData, relicEffectData);
        discount.Setup();
        discount.SetIsUnique(true);
        discount.SetExcludeFromClones(true);
        discount.SetCardUpgradeDataId("mt2_freecompany.MarchingOrdersDiscount");
        discount.SetRemoveOnDiscard(true);
        int reduction = System.Math.Max(0, relicEffectData.GetParamInt());
        discount.SetCostReduction(reduction);
        discount.SetXCostReduction(reduction);
        available = true;
    }

    public bool TestEffectTurnPhaseTiming(RelicEffectParams relicEffectParams, ICoreGameManagers coreGameManagers) => true;

    public IEnumerator ApplyEffectTurnPhaseTiming(RelicEffectParams relicEffectParams, ICoreGameManagers coreGameManagers)
    {
        available = true;
        foreach (CardState card in coreGameManagers.GetCardManager().GetHand())
            ApplyDiscount(card);
        yield break;
    }

    public bool OnCardAdded(CardAddedToHandRelicEffectParams relicEffectParams, ICoreGameManagers coreGameManagers)
    {
        return available && ApplyDiscount(relicEffectParams.cardState);
    }

    public bool TestEffectOnCardPlayed(CardPlayedRelicEffectParams relicEffectParams, ICoreGameManagers coreGameManagers)
    {
        // Even a zero-cost unit consumes the first-unit benefit. Spells and
        // automatic summons do not pass this card-play test.
        return available && IsUnit(relicEffectParams.cardState);
    }

    public IEnumerator ApplyEffectOnCardPlayed(CardPlayedRelicEffectParams relicEffectParams, ICoreGameManagers coreGameManagers)
    {
        available = false;
        foreach (CardState card in coreGameManagers.GetCardManager().GetHand())
        {
            if (!card.GetTemporaryCardStateModifiers().HasUpgrade(discount))
                continue;
            card.GetTemporaryCardStateModifiers().RemoveUpgrade(discount);
            card.UpdateCardBodyText();
        }
        // Keep the played card's paid-cost context intact. Its temporary
        // upgrade is removed on discard and is never inherited by clones.
        NotifyRelicTriggered(coreGameManagers.GetRelicManager(), null, "", false, 0f);
        yield break;
    }

    public bool TestEffectEndOfTurn(EndOfTurnRelicEffectParams relicEffectParams, ICoreGameManagers coreGameManagers) => true;

    public IEnumerator ApplyEffectEndOfTurn(EndOfTurnRelicEffectParams relicEffectParams, ICoreGameManagers coreGameManagers)
    {
        available = true;
        yield break;
    }

    private static bool IsUnit(CardState? card) => card != null && card.GetCardType() == CardType.Monster;

    private bool ApplyDiscount(CardState? card)
    {
        if (!IsUnit(card) || card!.GetTemporaryCardStateModifiers().HasUpgrade(discount))
            return false;
        card.GetTemporaryCardStateModifiers().AddUpgrade(discount);
        card.UpdateCardBodyText();
        return true;
    }
}
// 2026-09-30-2208||codex-freecompany-fx||src/code/RelicEffectFirstUnitDiscountEachTurn.cs||implementa descuento temporal de 1 Ember a la primera carta de unidad de cada turno; limpia la mano al consumirlo y reinicia antes del robo
// 2026-09-30-2215||codex-freecompany-fx||src/code/RelicEffectFirstUnitDiscountEachTurn.cs||implementa CreateEditorInspectorDescriptions requerido por la API de RelicEffectBase; corrige CS0534 de Actions
