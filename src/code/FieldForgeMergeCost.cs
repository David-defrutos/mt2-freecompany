using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace mt2_freecompany.Plugin;

// Retain the native modifier. Identify only this relic when calculating a merge;
// component discounts must not be baked into the new card and then applied again.
public sealed class RelicEffectFieldForgeCost : RelicEffectBase, ICardModifierRelicEffect
{
    private readonly RelicEffectModifyCardTypeCost native = new();
    internal int CostModifier { get; private set; }
    public override PropDescriptions CreateEditorInspectorDescriptions() => native.CreateEditorInspectorDescriptions();
    public override void Initialize(RelicState state, RelicData data, RelicEffectData effect)
    {
        base.Initialize(state, data, effect);
        CostModifier = effect.GetParamInt();
        native.Initialize(state, data, effect);
    }
    public bool ApplyCardStateModifiers(CardState card, RelicManager relics, SaveManager save, CardManager? cards)
        => native.ApplyCardStateModifiers(card, relics, save, cards);
    public bool GetTooltip(out string title, out string body) => native.GetTooltip(out title, out body);
}

[HarmonyPatch]
internal static class FieldForgeMergeCost
{
    private static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(EquipmentMergeRewardData), "GetStartingModifiersForChosenCards");
        yield return AccessTools.Method(typeof(CardEffectMergeEquipment), "OnMergeCardsChosen");
    }
    internal static int ComponentCost(CardState card, CardStatistics stats, MonsterManager? monsters,
        RelicManager? relics, RoomState? room)
    {
        int lift = 0;
        var manager = AllGameManagers.Instance?.GetRelicManager();
        if (card.GetCardType() == CardType.Equipment && manager != null)
            lift = -manager.GetRelicEffects(new List<RelicEffectFieldForgeCost>()).Sum(e => e.CostModifier);
        var modifiers = card.GetTemporaryCardStateModifiers();
        modifiers.IncrementAdditionalCost(lift);
        try { return card.GetCost(stats, monsters, relics, room); }
        finally { modifiers.IncrementAdditionalCost(-lift); }
    }
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var getCost = AccessTools.Method(typeof(CardState), nameof(CardState.GetCost));
        var replacement = AccessTools.Method(typeof(FieldForgeMergeCost), nameof(ComponentCost));
        int matches = 0;
        foreach (var instruction in instructions)
        {
            if (instruction.Calls(getCost))
            {
                instruction.opcode = OpCodes.Call; instruction.operand = replacement; ++matches;
            }
            yield return instruction;
        }
        if (matches != 1) throw new InvalidOperationException("Field Forge: merge cost call changed; expected one native GetCost call.");
    }
}

// 2026-10-26-2051||codex-freecompany-fx||C:\Users\david\AppData\Roaming\Thunderstore Mod Manager\DataFolder\MonsterTrain2\profiles\Default\BepInEx\plugins\David-FreeCompany\src\code\FieldForgeMergeCost.cs||delega descuento nativo y excluye solo Field Forge al hornear costes de fusión; conserva otros descuentos y clamping
