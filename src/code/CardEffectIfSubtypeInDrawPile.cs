using System.Collections;

namespace mt2_freecompany.Plugin
{
    /// <summary>
    /// GUARDA. No hace nada por si misma: su TestEffect dice si hay en la PILA DE ROBO una
    /// carta que invoque una unidad con el subtipo de `param_subtype`. Se pone DELANTE del
    /// efecto que se quiere condicionar, con `should_cancel_subsequent_effects_if_test_fails`.
    ///
    /// --------------------------------------------------------------------------------
    /// PARA QUE SIRVE
    ///
    /// Call of the Wild rescata un troll de la pila de robo con `CardEffectRecursion`.
    /// Cuando no hay ninguna carta que encaje, el juego **roba igual**: el TestEffect de
    /// `CardEffectRecursion` mira cuantas cartas hay en las pilas y si cabe en la mano,
    /// pero NO mira ni el subtipo ni el filtro (comprobado en el IL). Sin esta guarda, la
    /// habilidad te regala una carta cualquiera a coste 0 cuando el troll esta en la mano,
    /// en el descarte o todavia no ha entrado en el combate.
    ///
    /// --------------------------------------------------------------------------------
    /// 20-sep-2026: POR QUE ES UNA GUARDA Y NO UN ENVOLTORIO
    ///
    /// La primera version hacia lo mismo que `CardEffectGrantOnce`: instanciar
    /// `CardEffectRecursion` por reflexion y ejecutar su ApplyEffect. **El juego crashea.**
    /// Reproducido en partida: el log imprime
    ///
    ///     IfSubtypeInDrawPile: hay un '...Sub_Troll' en la pila de robo; ejecutando
    ///     CardEffectRecursion.
    ///
    /// y esa es la ultima linea antes del cierre.
    ///
    /// El motivo es que **no todos los efectos se pueden delegar**. El juego prepara el
    /// contexto de una carta ANTES de aplicar sus efectos, y para eso pregunta al efecto
    /// declarado en el JSON: `GetNumCardTargets`, `CheckForSingleCardTarget`,
    /// `CanApplyInPreviewMode`. `CardEffectRecursion` trabaja sobre cartas, asi que espera
    /// ese contexto preparado; nuestro envoltorio no declara objetivos de carta, asi que la
    /// instancia reflejada se encuentra sin nada con lo que trabajar.
    ///
    /// `CardEffectAddRunCard` y `CardEffectAddBattleCard` -lo que delega GrantOnce- no
    /// necesitan ese contexto, y por eso ahi si funciona. **Regla: delegar por reflexion
    /// vale para efectos autonomos; para los que operan sobre cartas objetivo, no.**
    /// --------------------------------------------------------------------------------
    ///
    /// Json:
    ///   { "id": "BeastmasterCallGate", "name": "@CardEffectIfSubtypeInDrawPile",
    ///     "param_subtype": "@Sub_Troll",
    ///     "target_mode": "self", "target_team": "monsters",
    ///     "should_test": true,
    ///     "should_fail_to_cast_if_test_fails": true,
    ///     "should_cancel_subsequent_effects_if_test_fails": true }
    ///
    /// Y detras, en la misma carta, el `CardEffectRecursion` de siempre.
    /// </summary>
    public sealed class CardEffectIfSubtypeInDrawPile : CardEffectBase
    {
        public override PropDescriptions CreateEditorInspectorDescriptions()
        {
            return new PropDescriptions();
        }

        /// <summary>
        /// Aqui SI se devuelve false a proposito: es justo lo contrario que en el resto de
        /// las clases del clan. El valor lo consumen `should_fail_to_cast_if_test_fails`
        /// (la habilidad no se puede activar) y
        /// `should_cancel_subsequent_effects_if_test_fails` (no se ejecuta el robo).
        /// </summary>
        public override bool TestEffect(CardEffectState cardEffectState, CardEffectParams cardEffectParams, ICoreGameManagers coreGameManagers)
        {
            var subtipo = cardEffectState.GetParamSubtype();
            if (subtipo == null || subtipo.IsNone)
            {
                Log("IfSubtypeInDrawPile: falta param_subtype.");
                return false;
            }

            var cardManager = coreGameManagers.GetCardManager();
            if (cardManager == null) return false;

            // shouldCopy a true: no queremos tocar la lista de verdad.
            foreach (var carta in cardManager.GetDrawPile(true))
            {
                if (carta == null) continue;

                var personaje = carta.GetSpawnCharacterData();
                if (personaje == null) continue;

                foreach (var st in personaje.GetSubtypes())
                {
                    if (st != null && st.Key == subtipo.Key) return true;
                }
            }
            return false;
        }

        /// <summary>
        /// La guarda no aplica nada. Si llega aqui es que el test paso; se deja la linea de
        /// log porque es lo unico que distingue "paso la guarda" de "no se llego a probar".
        /// </summary>
        public override IEnumerator ApplyEffect(CardEffectState cardEffectState, CardEffectParams cardEffectParams, ICoreGameManagers coreGameManagers, ISystemManagers sysManagers)
        {
            Log("IfSubtypeInDrawPile: guarda superada, sigue la cadena de efectos.");
            yield break;
        }

        private static void Log(string mensaje)
        {
            global::mt2_freecompany.Plugin.Plugin.Logger.LogInfo(mensaje);
        }
    }
}
