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
    /// Json:
    ///   { "id": "TameConsumeOthers", "name": "@CardEffectConsumeFromHand",
    ///     "target_mode": "self", "target_team": "monsters",
    ///     "param_card_pool": "@FreeCompanyTameOffersPool" }
    /// --------------------------------------------------------------------------------
    /// </summary>
    public sealed class CardEffectConsumeFromHand : CardEffectBase
    {
        public override PropDescriptions CreateEditorInspectorDescriptions()
        {
            return new PropDescriptions();
        }

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

            var jugada = cardEffectParams.playedCard;
            var mano = new List<CardState>(cardManager.GetHand(true));

            int consumidas = 0;
            foreach (var carta in mano)
            {
                if (carta == null || carta == jugada) continue;

                string id = carta.GetCardDataID();
                if (string.IsNullOrEmpty(id) || !pool.Contains(id)) continue;

                var parametros = new CardManager.DiscardCardParams
                {
                    discardCard = carta,
                    wasPlayed = false,
                };

                var consumir = cardManager.ConsumeCardWithoutPlaying(parametros);
                while (consumir.MoveNext()) yield return consumir.Current;
                consumidas++;
            }

            Log($"ConsumeFromHand: consumidas {consumidas} cartas de la mano.");
        }

        private static void Log(string mensaje)
        {
            global::mt2_freecompany.Plugin.Plugin.Logger.LogInfo(mensaje);
        }
    }
}
