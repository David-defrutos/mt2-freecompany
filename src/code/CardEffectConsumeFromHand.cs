using System.Collections;
using System.Collections.Generic;

namespace mt2_freecompany.Plugin
{
    /// <summary>
    /// Consume de la MANO todas las cartas que esten en `param_card_pool`, menos la que se
    /// acaba de jugar.
    ///
    /// --------------------------------------------------------------------------------
    /// PARA QUE SIRVE
    ///
    /// La oferta de la senda Beastmaster pone cinco hechizos "Tame" en la mano y solo uno
    /// se queda: al jugar uno, los otros cuatro sobran. Son efimeros, asi que se irian
    /// solos al acabar el turno, pero hasta entonces ocupan mano y confunden.
    ///
    /// POR QUE HACE FALTA C#
    ///
    /// El unico descarte que hay en JSON es `CardEffectDiscardHand`, y su `param_int` es un
    /// modo (Discard = 0, Consume = 1), no un filtro: se lleva la mano ENTERA, incluidas
    /// las cartas de verdad del jugador. No existe ningun efecto del juego base que
    /// descarte solo las cartas de un pool.
    ///
    /// COMO FUNCIONA
    ///
    /// `CardManager.GetHand(true)` da una copia de la mano —importante, porque consumir
    /// toca la lista original—; de ahi se filtra con la pareja que ya usa GrantOnce:
    /// `CardPool.Contains(string)` contra `CardState.GetCardDataID()`. Lo que consume es
    /// `CardManager.ConsumeCardWithoutPlaying`, que **devuelve IEnumerator**: es una
    /// corrutina y hay que recorrerla (hallazgo del 20-sep con ApplyCardUpgrade).
    ///
    /// `param_bool` a true incluye **la carta que se acaba de jugar**. Hace falta porque el
    /// juego no la esta sacando de la mano por si mismo: se queda ahi y se puede volver a
    /// jugar. Comprobado en partida el 20-sep-2026, y el trait `CardTraitEphemeral`
    /// -"Purges when played, discarded, or at end of turn"- tampoco la quita. Cuando la
    /// llamada llega, la carta jugada SIGUE en la mano: se comprobo contando, porque el
    /// efecto consumia 4 de 5 saltandose la jugada por referencia.
    ///
    /// Json:
    ///   { "id": "TameConsumeOthers", "name": "@CardEffectConsumeFromHand",
    ///     "target_mode": "self", "target_team": "monsters",
    ///     "param_bool": true,
    ///     "param_card_pool": "@FreeCompanyTameOffersPool" }
    /// --------------------------------------------------------------------------------
    /// </summary>
    public sealed class CardEffectConsumeFromHand : CardEffectBase
    {
        public override PropDescriptions CreateEditorInspectorDescriptions()
        {
            return new PropDescriptions();
        }

        /// <summary>
        /// `CardEffectBase` lo devuelve **true** por defecto, y este efecto consume cartas
        /// de verdad: en una pasada de vista previa se las llevaria por delante sin que el
        /// jugador haya jugado nada. Se apaga a proposito.
        /// </summary>
        public override bool CanApplyInPreviewMode => false;

        // Nunca falla: si fallase, cancelaria los efectos siguientes de la carta.
        public override bool TestEffect(CardEffectState cardEffectState, CardEffectParams cardEffectParams, ICoreGameManagers coreGameManagers)
        {
            return true;
        }

        public override IEnumerator ApplyEffect(CardEffectState cardEffectState, CardEffectParams cardEffectParams, ICoreGameManagers coreGameManagers, ISystemManagers sysManagers)
        {
            var pool = cardEffectState.GetParamCardPool();
            if (pool == null)
            {
                Log("ConsumeFromHand: falta param_card_pool.");
                yield break;
            }

            var cardManager = coreGameManagers.GetCardManager();
            if (cardManager == null)
            {
                Log("ConsumeFromHand: no hay CardManager.");
                yield break;
            }

            // param_bool: consumir tambien la carta que se acaba de jugar.
            bool incluirJugada = cardEffectState.GetParamBool();
            var jugada = cardEffectParams.playedCard;
            // GetHand(true) ya devuelve una copia; no hace falta duplicarla otra vez.
            var mano = cardManager.GetHand(true);

            int consumidas = 0;
            foreach (var carta in mano)
            {
                if (carta == null) continue;
                if (carta == jugada && !incluirJugada) continue;

                string id = carta.GetCardDataID();
                if (string.IsNullOrEmpty(id) || !pool.Contains(id)) continue;

                // GetHand(true) da una copia: para cuando llega el turno de esta carta, la
                // mano de verdad ya ha cambiado (la hemos tocado nosotros, o el juego ha
                // purgado la jugada por su trait Ephemeral). Se vuelve a preguntar.
                if (!cardManager.IsCardInHand(carta)) continue;

                var parametros = new CardManager.DiscardCardParams
                {
                    discardCard = carta,
                    wasPlayed = false,
                };

                var consumir = cardManager.ConsumeCardWithoutPlaying(parametros);
                while (consumir.MoveNext()) yield return consumir.Current;
                consumidas++;
            }

            Log($"ConsumeFromHand: consumidas {consumidas} cartas de la mano (jugada incluida: {incluirJugada}).");
        }

        private static void Log(string mensaje)
        {
            global::mt2_freecompany.Plugin.Plugin.Logger.LogInfo(mensaje);
        }
    }
}
