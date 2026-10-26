using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;

namespace mt2_freecompany.Plugin;

// Keep the original attachment, source GUID and cooldown. Native upgrade
// application/removal owns stat changes, Sniper and units entering/leaving the floor.
public sealed class CardEffectWatchtowerToggle : CardEffectBase
{
    public override bool CanApplyInPreviewMode => true;
    public override PropDescriptions CreateEditorInspectorDescriptions() => new();
    internal static bool IsWatchtowerUpgrade(CardUpgradeData? u) => u != null
        && (u.name.EndsWith("WatchtowerRoomUpgrade", StringComparison.Ordinal)
            || u.name.EndsWith("WatchtowerRoomOffUpgrade", StringComparison.Ordinal));
    internal static TrainRoomAttachmentState? Find(RoomState room)
    {
        foreach (var type in new[] { TrainRoomAttachmentType.Primary, TrainRoomAttachmentType.Primary_Embedded })
            if (room.TryGetFirstTrainRoomAttachment(out var a, type) && a.UpgradeStates.Any(x => IsWatchtowerUpgrade(x.Item2))) return a;
        return null;
    }
    private static bool Owns(TrainRoomAttachmentState? source, CardUpgradeState state)
        => source != null && source.UpgradeStates.Any(x => IsWatchtowerUpgrade(x.Item2)
            && x.Item1.GetCardUpgradeDataId() == state.GetCardUpgradeDataId());
    private static CardUpgradeState WithoutSniper(CardUpgradeState source)
    {
        var copy = new CardUpgradeState(); copy.Setup(source);
        copy.GetStatusEffectUpgrades().RemoveAll(s => s.statusId == "sniper");
        return copy;
    }
    internal static CardUpgradeState PrepareApplication(CharacterState unit, CardUpgradeState state,
        TrainRoomAttachmentState? source)
        => Owns(source, state) && unit.HasStatusEffect("sniper")
            ? WithoutSniper(state) : state;
    internal static CardUpgradeState PrepareRemoval(CharacterState unit, CardUpgradeState state,
        TrainRoomAttachmentState? source)
    {
        if (!Owns(source, state)) return state;
        var applied = unit.GetAppliedCardUpgrades().FirstOrDefault(x => x.GetCardUpgradeDataId() == state.GetCardUpgradeDataId());
        if (applied == null) return state;
        // Sniper is nonstackable. Use the per-unit copy that actually granted it;
        // also retain Sniper granted by another equipment/upgrade while active.
        bool otherSource = unit.GetAppliedCardUpgrades().Any(x => !ReferenceEquals(x, applied)
            && x.GetStatusEffectUpgrades().Any(s => s.statusId == "sniper" && s.count > 0));
        return otherSource ? WithoutSniper(applied) : applied;
    }
    [HarmonyPatch(typeof(CharacterState), nameof(CharacterState.ApplyCardUpgrade))]
    private static class ApplySniperPatch
    {
        private static void Prefix(CharacterState __instance, ref CardUpgradeState cardUpgradeState,
            TrainRoomAttachmentState sourceTrainRoomAttachment)
            => cardUpgradeState = PrepareApplication(__instance, cardUpgradeState, sourceTrainRoomAttachment);
    }
    [HarmonyPatch(typeof(CharacterState), nameof(CharacterState.RemoveCardUpgrade))]
    private static class RemoveSniperPatch
    {
        private static void Prefix(CharacterState __instance, ref CardUpgradeState cardUpgradeState,
            TrainRoomAttachmentState sourceTrainRoomAttachment)
            => cardUpgradeState = PrepareRemoval(__instance, cardUpgradeState, sourceTrainRoomAttachment);
    }
    public override bool TestEffect(CardEffectState e, CardEffectParams p, ICoreGameManagers g)
    {
        var room = p.roomThatActivatedAbility ?? g.GetRoomManager().GetRoom(p.selectedRoom);
        return room != null && Find(room) != null;
    }
    public override IEnumerator ApplyEffect(CardEffectState e, CardEffectParams p,
        ICoreGameManagers g, ISystemManagers systems)
    {
        if (g.GetSaveManager().PreviewMode) yield break;
        var room = p.roomThatActivatedAbility ?? g.GetRoomManager().GetRoom(p.selectedRoom);
        if (room == null) yield break;
        var a = Find(room);
        if (a == null || a.UpgradeStates is not IList<(CardUpgradeState, CardUpgradeData)> upgrades) yield break;
        var originalEffect = a.CardState?.GetEffectStates().FirstOrDefault(x =>
            x.GetCardEffect() is CardEffectAttachTrainRoomAttachment && IsWatchtowerUpgrade(x.GetParamCardUpgradeData()));
        var on = originalEffect?.GetParamCardUpgradeData();
        var off = e.GetParamCardUpgradeData();
        if (on == null || off == null) yield break;
        int index = -1;
        for (int i = 0; i < upgrades.Count; ++i)
            if (IsWatchtowerUpgrade(upgrades[i].Item2)) { index = i; break; }
        if (index < 0) yield break;
        var old = upgrades[index];
        var nextData = old.Item2 == on ? off : on;
        var next = new CardUpgradeState(); next.Setup(nextData);
        var impacted = a.CharactersImpacted.ToList();
        foreach (var unit in impacted)
            yield return unit.RemoveCardUpgrade(old.Item1, a.Guid, false, a);
        upgrades[index] = (next, nextData);
        foreach (var unit in impacted)
            yield return unit.ApplyCardUpgrade(next, false, a.Guid, false, false, null, null, a);
        yield return room.UpdateTrainRoomAttachments();
    }
}

// 2026-10-26-2051||codex-freecompany-fx||C:\Users\david\AppData\Roaming\Thunderstore Mod Manager\DataFolder\MonsterTrain2\profiles\Default\BepInEx\plugins\David-FreeCompany\src\code\CardEffectWatchtowerToggle.cs||interruptor Watchtower sobre misma sala: cambia mejora nativa sin acumular ni reiniciar cooldown

// 2026-10-26-2103||codex-freecompany-fx||C:\Users\david\AppData\Roaming\Thunderstore Mod Manager\DataFolder\MonsterTrain2\profiles\Default\BepInEx\plugins\David-FreeCompany\src\code\CardEffectWatchtowerToggle.cs||protege Sniper no acumulable de otras fuentes mediante copia nativa por unidad y retirada de la copia real

// 2026-10-26-2211||codex-freecompany-fx||C:\Users\david\AppData\Roaming\Thunderstore Mod Manager\DataFolder\MonsterTrain2\profiles\Default\BepInEx\plugins\David-FreeCompany\src\code\CardEffectWatchtowerToggle.cs||revisión: selecciona el efecto Watchtower específico en salas fusionadas, no el primer attachment de otra sala

// 2026-10-26-2211||codex-freecompany-fx||C:\Users\david\AppData\Roaming\Thunderstore Mod Manager\DataFolder\MonsterTrain2\profiles\Default\BepInEx\plugins\David-FreeCompany\src\code\CardEffectWatchtowerToggle.cs||anota referencia opcional del efecto nativo al filtrar mejoras de sala
