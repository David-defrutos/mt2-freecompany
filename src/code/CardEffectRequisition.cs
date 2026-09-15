using System.Collections;
using System.Collections.Generic;

namespace mt2_freecompany.Plugin
{
    /// <summary>
    /// Requisition: da a la unidad objetivo el siguiente escalon del equipo de SU ROL.
    ///
    /// No hacen falta estados marcadores: CharacterState.HasUpgrade dice por que escalon
    /// va la unidad. Las mejoras se buscan por id con AllGameData.FindCardUpgradeData.
    ///
    /// Json:
    ///   { "id": "RequisitionEffect", "name": "@CardEffectRequisition",
    ///     "target_mode": "drop_target_character", "target_team": "monsters" }
    /// </summary>
    public sealed class CardEffectRequisition : CardEffectBase
    {
        // AllGameData NO tiene .Instance: se llega por ICoreGameManagers.GetAllGameData(),
        // que ya llega como parametro de ApplyEffect. Ver docs/43-api-csharp.md.
        // rol -> los tres escalones, en orden. Los ids son los de json/kits/*.json.
        private static readonly Dictionary<string, string[]> Escaleras = new()
        {
            ["Sub_Warrior"]   = ["UpgBuckler",     "UpgKiteShield",     "UpgBulwarkPlate"],
            ["Sub_Barbarian"] = ["UpgNotchedAxe",  "UpgTwinAxes",       "UpgReaversMaul"],
            ["Sub_Rogue"]     = ["UpgLockpicks",   "UpgPoisonedDagger", "UpgThiefsMantle"],
            ["Sub_Wizard"]    = ["UpgChalkCircle", "UpgGrimoire",       "UpgArchmagesStaff"],
            ["Sub_Cleric"]    = ["UpgBandages",    "UpgHolySymbol",     "UpgReliquary"],
            ["Sub_Ranger"]    = ["UpgShortbow",    "UpgLongbow",        "UpgGreatBow"],
        };

        public override PropDescriptions CreateEditorInspectorDescriptions()
        {
            return new PropDescriptions();
        }

        // NUNCA falla: si fallase, cancelaria los efectos siguientes de la carta.
        // Ese es el fallo de Pebb, y aqui no lo queremos.
        public override bool TestEffect(CardEffectState cardEffectState, CardEffectParams cardEffectParams, ICoreGameManagers coreGameManagers)
        {
            return true;
        }

        public override IEnumerator ApplyEffect(CardEffectState cardEffectState, CardEffectParams cardEffectParams, ICoreGameManagers coreGameManagers, ISystemManagers sysManagers)
        {
            foreach (CharacterState target in cardEffectParams.targets)
            {
                string[]? escalera = null;
                foreach (var par in Escaleras)
                {
                    if (TieneSubtipo(target, par.Key)) { escalera = par.Value; break; }
                }
                if (escalera == null)
                    continue;   // no es de ningun rol: la carta no hace nada con el

                // escalon actual = el ultimo que ya lleva puesto
                int actual = 0;
                for (int i = 0; i < escalera.Length; i++)
                {
                    var data = coreGameManagers.GetAllGameData().FindCardUpgradeData(escalera[i]);
                    if (data != null && target.HasUpgrade(data)) actual = i + 1;
                }
                if (actual >= escalera.Length)
                    continue;   // ya esta al maximo

                var siguiente = coreGameManagers.GetAllGameData().FindCardUpgradeData(escalera[actual]);
                if (siguiente == null)
                    continue;

                // fuera el anterior, dentro el nuevo
                if (actual > 0)
                {
                    var anterior = coreGameManagers.GetAllGameData().FindCardUpgradeData(escalera[actual - 1]);
                    if (anterior != null) target.RemoveCardUpgrade(anterior);
                }

                var estado = new CardUpgradeState();
                estado.Setup(siguiente, false, false);
                target.ApplyCardUpgrade(estado);
            }
            yield break;
        }

        private static bool TieneSubtipo(CharacterState target, string subtipoId)
        {
            foreach (var st in target.GetSubtypes())
            {
                if (st != null && st.Key == subtipoId) return true;
            }
            return false;
        }
    }
}
