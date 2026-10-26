using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace mt2_freecompany.Plugin;

// Keep the original attachment, source GUID and cooldown. Native upgrade
// application/removal owns stat changes, Sniper and units entering/leaving the floor.
public sealed class CardEffectWatchtowerToggle : CardEffectBase
{
    public override bool CanApplyInPreviewMode => true;
    public override PropDescriptions CreateEditorInspectorDescriptions() => new();
    internal static bool IsWatchtowerUpgrade(CardUpgradeData u) => u != null
        && (u.name.EndsWith("WatchtowerRoomUpgrade", StringComparison.Ordinal)
            || u.name.EndsWith("WatchtowerRoomOffUpgrade", StringComparison.Ordinal));
    internal static TrainRoomAttachmentState? Find(RoomState room)
    {
        foreach (var type in new[] { TrainRoomAttachmentType.Primary, TrainRoomAttachmentType.Primary_Embedded })
            if (room.TryGetFirstTrainRoomAttachment(out var a, type) && a.UpgradeStates.Any(x => IsWatchtowerUpgrade(x.Item2))) return a;
        return null;
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
        var originalEffect = a.CardState?.GetEffectStates().FirstOrDefault(x => x.GetCardEffect() is CardEffectAttachTrainRoomAttachment);
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
