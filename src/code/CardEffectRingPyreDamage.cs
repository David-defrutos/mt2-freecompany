using System;
using System.Collections;

namespace mt2_freecompany.Plugin
{
    /// <summary>
    /// Dano a la PIRA que depende del Anillo en el que estas: cuanto antes, mas duele.
    ///
    ///     dano = param_int * (param_int_2 - anillo)
    ///
    /// Con param_int = 5 y param_int_2 = 11 (Finiquito / Severance Pay), y los 10 Anillos
    /// del juego, sale:
    ///
    ///     Anillo  1 -> 5 * 10 = 50        Anillo  6 -> 5 * 5 = 25
    ///     Anillo  2 -> 5 *  9 = 45        Anillo  7 -> 5 * 4 = 20
    ///     Anillo  3 -> 5 *  8 = 40        Anillo  8 -> 5 * 3 = 15
    ///     Anillo  4 -> 5 *  7 = 35        Anillo  9 -> 5 * 2 = 10
    ///     Anillo  5 -> 5 *  6 = 30        Anillo 10 -> 5 * 1 =  5
    ///
    /// Json:
    ///   { "id": "SeverancePayPyreDamage", "name": "@CardEffectRingPyreDamage",
    ///     "target_mode": "self", "target_team": "monsters",
    ///     "param_int": 5, "param_int_2": 11 }
    ///
    /// Los dos numeros van en el JSON a proposito: se retocan sin recompilar.
    ///
    /// --------------------------------------------------------------------------------
    /// LO QUE ESTA COMPROBADO EN EL IL (22-sep-2026, MonsterTrain2.Api 2.1.20112166)
    ///
    ///   - El Anillo: SaveManager.GetDisplayDistance(). Devuelve el numero que ve el
    ///     jugador: internamente es GetCurrentDistance() + 1, salvo en una secuencia de
    ///     juego concreta (sequence == 2) donde resta uno antes. O sea que el primer
    ///     Anillo jugable da 1, como pide el diseno.
    ///
    ///   - La pira: PlayerManager.AdjustTowerHP(addHP, triggerDamageTakenRelicEffects).
    ///     OJO: la sobrecarga de UN parametro, AdjustTowerHP(addHP), NO es publica; la de
    ///     dos si. Devuelve void: no es corrutina, no hay que recorrerla. Dano = addHP
    ///     negativo. Con el segundo parametro a true saltan las reliquias que reaccionan
    ///     al dano en la pira, como con cualquier otro dano del juego.
    ///
    ///   - param_int -> GetParamInt(); param_int_2 -> GetAdditionalParamInt(). Es la
    ///     misma pareja que usa CardEffectAddBattleCard para "pila" y "numero de cartas".
    ///
    /// --------------------------------------------------------------------------------
    /// ES LETAL, A PROPOSITO (22-sep-2026)
    ///
    /// La primera version dejaba la pira a 1 si el dano la iba a matar. Se quito por
    /// decision de David: ese recorte convertia el coste en cero justo cuando mas
    /// importa. Con cuatro copias en el Anillo 1 el jugador se las comeria las cuatro,
    /// quedaria a 1 de vida y se llevaria 600 de oro sin ningun riesgo real. El coste
    /// tiene que poder matarte para que la decision exista.
    ///
    /// Lo que NO esta comprobado: SaveManager.AdjustTowerHP solo recorta la vida entre 0
    /// y el maximo y la guarda; no lanza la derrota. El fin de partida lo detecta otra
    /// parte del juego. Si la pira llega a 0 por esta carta, confirmar en partida que la
    /// run termina en ese momento y no en el siguiente golpe.
    /// --------------------------------------------------------------------------------
    /// </summary>
    public sealed class CardEffectRingPyreDamage : CardEffectBase
    {
        private const int PorAnilloPorDefecto = 5;
        private const int TopePorDefecto = 11;

        public override PropDescriptions CreateEditorInspectorDescriptions()
        {
            return new PropDescriptions();
        }

        // Nunca falla: si fallase, cancelaria los efectos siguientes de la carta, y el
        // siguiente es el que da el oro.
        public override bool TestEffect(CardEffectState cardEffectState, CardEffectParams cardEffectParams, ICoreGameManagers coreGameManagers)
        {
            return true;
        }

        // No se sobreescribe CanApplyInPreviewMode (lecciones del 21-sep con Requisition).
        // La guarda de vista previa va dentro de ApplyEffect: danar la pira no se simula.

        public override IEnumerator ApplyEffect(CardEffectState cardEffectState, CardEffectParams cardEffectParams, ICoreGameManagers coreGameManagers, ISystemManagers sysManagers)
        {
            var save = coreGameManagers.GetSaveManager();
            if (save == null)
            {
                Log("RingPyreDamage: no hay SaveManager.");
                yield break;
            }
            if (save.PreviewMode) yield break;

            var jugador = coreGameManagers.GetPlayerManager();
            if (jugador == null)
            {
                Log("RingPyreDamage: no hay PlayerManager.");
                yield break;
            }

            int porAnillo = cardEffectState.GetParamInt();
            if (porAnillo <= 0) porAnillo = PorAnilloPorDefecto;

            int tope = cardEffectState.GetAdditionalParamInt();
            if (tope <= 0) tope = TopePorDefecto;

            int anillo = save.GetDisplayDistance();
            int dano = porAnillo * (tope - anillo);
            if (dano <= 0)
            {
                Log($"RingPyreDamage: anillo {anillo}, dano {dano} -> no se aplica nada.");
                yield break;
            }

            jugador.AdjustTowerHP(-dano, true);
            Log($"RingPyreDamage: anillo {anillo}, {porAnillo} x ({tope} - {anillo}) = {dano} de dano a la pira.");
        }

        private static void Log(string mensaje)
        {
            global::mt2_freecompany.Plugin.Plugin.Logger.LogInfo(mensaje);
        }
    }
}
// 2026-09-22-2219||claude-mt2-the-free-company2-roderic-quartermaster||src/code/CardEffectRingPyreDamage.cs||fichero nuevo: dano a la pira = param_int x (param_int_2 - anillo), no letal, para Severance Pay
// 2026-09-22-2236||claude-mt2-the-free-company2-roderic-quartermaster||src/code/CardEffectRingPyreDamage.cs||quitado el recorte no letal: el dano de Severance Pay puede matar la pira
