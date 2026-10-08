using System.Collections.Generic;
using System.Linq;

namespace mt2_freecompany.Plugin;

// Delegate the attack to the native effect: normal targeting in the beast's room,
// Multistrike/Sweep and attack/kill triggers use the game's existing implementation.
public sealed class CardEffectBeastmasterCommand : CardEffectAttackWithUnit
{
    internal static bool Eligible(CharacterState unit)
    {
        if (unit == null || unit.IsDead || !unit.CheckAttackConditions()
            || !unit.HasStatusEffect(BeastmasterMarkers.StatusId)) return false;
        var card = unit.GetSpawnerCard();
        var room = unit.GetCurrentRoom();
        return card != null && BeastmasterTraining.IsCreature(card)
            && room != null && room.GetNumCharacters(Team.Type.Heroes) > 0;
    }

    public override bool TestEffect(CardEffectState effect, CardEffectParams parameters, ICoreGameManagers managers)
    {
        if (parameters.targets != null && parameters.targets.Count > 0)
            return parameters.targets.Count == 1 && Eligible(parameters.targets[0]);
        var units = new List<CharacterState>();
        managers.GetMonsterManager().AddCharactersToList(units);
        return units.Any(Eligible);
    }
}

// 2026-10-08-1944||codex-freecompany-fx||C:\Users\david\AppData\Roaming\Thunderstore Mod Manager\DataFolder\MonsterTrain2\profiles\Default\BepInEx\plugins\David-FreeCompany\src\code\CardEffectBeastmasterCommand.cs||Command valida una bestia Tamed viva con enemigos en su piso y usa ataque inmediato nativo, sin Dazed adicional

// 2026-10-08-1950||codex-freecompany-fx||C:\Users\david\AppData\Roaming\Thunderstore Mod Manager\DataFolder\MonsterTrain2\profiles\Default\BepInEx\plugins\David-FreeCompany\src\code\CardEffectBeastmasterCommand.cs||valida el piso capturado una sola vez para evitar referencia nula
