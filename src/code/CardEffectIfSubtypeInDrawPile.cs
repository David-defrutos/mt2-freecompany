using System.Collections;
using System.Collections.Generic;
using System.Reflection;

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
    /// 21-sep-2026: AHORA MIRA TRES PILAS, NO SOLO LA DE ROBO
    ///
    /// **El nombre de la clase se ha quedado corto**: mira la MANO, la PILA DE ROBO y el
    /// DESCARTE, en ese orden. Se deja el nombre para no tocar el JSON de otros sitios.
    ///
    /// Mirando solo la pila de robo, Call of the Wild era injugable casi siempre. El troll
    /// entra en la pila de robo al domarlo (`CardPile.DeckPileRandom`, el 7), se roba en los
    /// primeros turnos, y la habilidad no esta disponible hasta el turno 5
    /// (`initial_cooldown: 4`): cuando por fin se puede usar, el troll ya esta en la mano, en
    /// juego o en el descarte, y la guarda decia no. Sintoma: **"No valid target"**.
    ///
    /// Y ese mensaje sale de aqui de verdad, no de un fallo: de los dos efectos de la
    /// habilidad, `CardEffectRecursion` **no puede validar la jugada** porque su
    /// `CanApplyInPreviewMode` es `cardSelectionMode == 4` (`RandomToRoom`) y el nuestro es
    /// `RandomToHand`, el 1. `CommonSelectionBehavior.PrunePossibleTargets` llama a
    /// `GameEffectHelper.TestEffect` con la comprobacion de vista previa activada, asi que
    /// ese efecto cuenta como test fallido. **La habilidad es seleccionable si y solo si esta
    /// guarda dice si**; si dice no, `possibleTargets` se queda vacio y sale
    /// `SelectionError.InvalidTarget`.
    /// --------------------------------------------------------------------------------
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

        // GetSubtypes() de CharacterData resuelve contra TODOS los subtipos registrados
        // (bucle anidado dentro de SubtypeManager) y construye una lista nueva por carta.
        // Este TestEffect lo llama la interfaz cada vez que refresca la mano, asi que se
        // leen las claves en crudo y se compara texto. Si el campo cambiara de nombre en
        // una version del juego, se cae al camino lento, que siempre funciona.
        private static readonly FieldInfo? ClavesDeSubtipo =
            typeof(CharacterData).GetField("subtypeKeys", BindingFlags.NonPublic | BindingFlags.Instance);

        /// <summary>
        /// Aqui SI se devuelve false a proposito: es justo lo contrario que en el resto de
        /// las clases del clan. El valor lo consumen `should_fail_to_cast_if_test_fails`
        /// (la habilidad no se puede activar) y
        /// `should_cancel_subsequent_effects_if_test_fails` (no se ejecuta el robo).
        /// </summary>
        // Se recuerda el ultimo resultado para escribir en el log SOLO cuando cambia: este
        // TestEffect lo llama la interfaz en cada refresco de mano y un log por llamada
        // inundaria el fichero. 21-sep: sin esta linea, un "No valid target" era mudo.
        private static bool? ultimoResultado;

        public override bool TestEffect(CardEffectState cardEffectState, CardEffectParams cardEffectParams, ICoreGameManagers coreGameManagers)
        {
            var subtipo = cardEffectState.GetParamSubtype();
            if (subtipo == null || subtipo.IsNone) return Recordar(false, "falta param_subtype");

            var cardManager = coreGameManagers.GetCardManager();
            if (cardManager == null) return Recordar(false, "no hay CardManager");

            string buscado = subtipo.Key;

            // shouldCopy a false: las pilas solo se recorren, no se tocan.
            int enMano = Contar(cardManager.GetHand(false), buscado);
            if (enMano > 0) return Recordar(true, $"{enMano} en la mano");

            int enRobo = Contar(cardManager.GetDrawPile(false), buscado);
            if (enRobo > 0) return Recordar(true, $"{enRobo} en la pila de robo");

            int enDescarte = Contar(cardManager.GetDiscardPile(false), buscado);
            if (enDescarte > 0) return Recordar(true, $"{enDescarte} en el descarte");

            return Recordar(false, "no hay ninguna en mano, robo ni descarte");
        }

        private static int Contar(List<CardState> pila, string buscado)
        {
            if (pila == null) return 0;
            int n = 0;
            foreach (var carta in pila)
            {
                if (carta == null) continue;

                var personaje = carta.GetSpawnCharacterData();
                if (personaje == null) continue;

                if (ClavesDeSubtipo?.GetValue(personaje) is List<string> claves)
                {
                    foreach (var clave in claves)
                    {
                        if (clave == buscado) { n++; break; }
                    }
                }
                else
                {
                    foreach (var st in personaje.GetSubtypes())
                    {
                        if (st != null && st.Key == buscado) { n++; break; }
                    }
                }
            }
            return n;
        }

        private static bool Recordar(bool resultado, string motivo)
        {
            if (ultimoResultado != resultado)
            {
                ultimoResultado = resultado;
                Log($"IfSubtypeInPiles: {(resultado ? "SI" : "NO")} se puede lanzar ({motivo}).");
            }
            return resultado;
        }

        /// <summary>
        /// La guarda no aplica nada. Si llega aqui es que el test paso; se deja la linea de
        /// log porque es lo unico que distingue "paso la guarda" de "no se llego a probar".
        /// </summary>
        public override IEnumerator ApplyEffect(CardEffectState cardEffectState, CardEffectParams cardEffectParams, ICoreGameManagers coreGameManagers, ISystemManagers sysManagers)
        {
            Log("IfSubtypeInPiles: guarda superada, sigue la cadena de efectos.");
            yield break;
        }

        private static void Log(string mensaje)
        {
            global::mt2_freecompany.Plugin.Plugin.Logger.LogInfo(mensaje);
        }
    }
}
// 2026-09-21-2310||claude-mt2-the-free-company2-vesper-beastmaster||src/code/CardEffectIfSubtypeInDrawPile.cs||la guarda mira ahora mano + pila de robo + descarte, no solo la pila de robo, y escribe en el log cuando su resultado cambia
