using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace mt2_freecompany.Plugin;

public sealed class CardEffectRemoveRandomDebuff : CardEffectBase
{
    public override bool CanApplyInPreviewMode => true;
    public override PropDescriptions CreateEditorInspectorDescriptions() => new PropDescriptions();
    public override bool TestEffect(CardEffectState effect, CardEffectParams parameters, ICoreGameManagers managers)
        => parameters.targets != null && parameters.targets.Count > 0;

    public override IEnumerator ApplyEffect(CardEffectState effect, CardEffectParams parameters,
        ICoreGameManagers managers, ISystemManagers systems)
    {
        // Random cleansing must not leak simulated results or consume RNG on hover.
        if (managers.GetSaveManager().PreviewMode) yield break;
        foreach (var target in parameters.targets)
        {
            var statuses = new List<CharacterState.StatusEffectStack>();
            target.GetStatusEffects(ref statuses);
            var eligible = statuses.Where(s => s.Count > 0 && !s.State.IsHidden()
                && s.State.GetDisplayCategory() == StatusEffectData.DisplayCategory.Negative).ToList();
            if (eligible.Count == 0) continue;
            var chosen = eligible[RandomManager.Range(0, eligible.Count, RngId.Battle)];
            target.RemoveStatusEffect(chosen.State.GetStatusId(), chosen.Count,
                new CharacterState.RemoveStatusEffectParams
                {
                    showNotification = true,
                    sourceCardState = parameters.playedCard,
                    sourceRelicState = parameters.sourceRelic
                }, false);
            target.GetCharacterUI().ShowEffectVFX(target, effect.GetAppliedVFX(), parameters.appliedVfxId);
        }
        yield break;
    }
}

// 2026-10-09-1828||codex-freecompany-fx||C:\Users\david\AppData\Roaming\Thunderstore Mod Manager\DataFolder\MonsterTrain2\profiles\Default\BepInEx\plugins\David-FreeCompany\src\code\CardEffectRemoveRandomDebuff.cs||elimina una categoría negativa completa al azar con RNG Battle; protege preview, estados positivos y ocultos

// 2026-10-09-1828||codex-freecompany-fx||C:\Users\david\AppData\Roaming\Thunderstore Mod Manager\DataFolder\MonsterTrain2\profiles\Default\BepInEx\plugins\David-FreeCompany\src\code\CardEffectRemoveRandomDebuff.cs||usa cuarto argumento posicional false compatible con API instalada de RemoveStatusEffect
