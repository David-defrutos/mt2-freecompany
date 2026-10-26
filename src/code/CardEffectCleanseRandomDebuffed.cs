using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace mt2_freecompany.Plugin;

// The native status filter is AND, and random targeting selects before applying
// the effect. Select eligible allies first, then let the native cleanse do its job.
public sealed class CardEffectCleanseRandomDebuffed : CardEffectBase
{
    public override bool CanApplyInPreviewMode => true;
    public override PropDescriptions CreateEditorInspectorDescriptions() => new();
    public override bool TestEffect(CardEffectState effect, CardEffectParams p, ICoreGameManagers g) => true;

    internal static bool Eligible(CharacterState unit) => unit != null && unit.IsAlive && !unit.IsDestroyed
        && !unit.HasStatusEffect("untouchable")
        && unit.GetNumberUniqueStatusEffectsInCategory(StatusEffectData.DisplayCategory.Negative, true) > 0;

    public override IEnumerator ApplyEffect(CardEffectState effect, CardEffectParams p,
        ICoreGameManagers g, ISystemManagers systems)
    {
        if (g.GetSaveManager().PreviewMode) yield break;
        var candidates = new List<CharacterState>();
        if (effect.GetParamBool2()) g.GetMonsterManager().AddCharactersToList(candidates);
        else candidates.AddRange(p.targets);
        candidates = candidates.Where(u => Eligible(u) && !u.PreviewMode).Distinct().ToList();
        if (candidates.Count == 0) yield break;
        var chosen = candidates[RandomManager.Range(0, candidates.Count, RngId.Battle)];
        var original = p.targets;
        p.targets = new List<CharacterState> { chosen };
        try
        {
            yield return new CardEffectRemoveAllStatusEffects().ApplyEffect(effect, p, g, systems);
        }
        finally { p.targets = original; }
    }
}

// 2026-10-26-2051||codex-freecompany-fx||C:\Users\david\AppData\Roaming\Thunderstore Mod Manager\DataFolder\MonsterTrain2\profiles\Default\BepInEx\plugins\David-FreeCompany\src\code\CardEffectCleanseRandomDebuffed.cs||selecciona aliado con cualquier aflicción visible antes del azar y delega limpieza nativa; sin RNG en preview
