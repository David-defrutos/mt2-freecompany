using System.Collections;
using System.Collections.Generic;

namespace mt2_freecompany.Plugin
{
    /// <summary>
    /// Pone en la MANO unas cuantas cartas AL AZAR y DISTINTAS de un pool, una sola vez por
    /// partida.
    ///
    /// --------------------------------------------------------------------------------
    /// PARA QUE SIRVE
    ///
    /// La oferta de la senda Beastmaster ponia los CINCO hechizos "Tame" en la mano. Con los
    /// cinco delante siempre se acaba cogiendo el mismo troll, asi que ahora se ofrecen solo
    /// unos pocos, sorteados, y la eleccion cambia en cada partida.
    ///
    /// POR QUE NO SIRVE EL EFECTO DEL JUEGO
    ///
    /// `CardEffectAddBattleCard` ya sabe sacar cartas de un pool y repartirlas a la mano
    /// —`additional_param_int` es la cantidad, `param_int` la pila destino (3 = mano) y
    /// `param_bool_2` salta las que ya esten en la mano—, pero **el sorteo es con
    /// reemplazo**: leido su IL, cada vuelta hace
    /// `RandomManager.Range(0, toProcessCards.Count, RngId.Battle)` y **no saca del sitio la
    /// carta elegida**. Con `param_bool_2` una repeticion no se vuelve a sortear: se
    /// **descarta**, asi que pedir 3 de un pool de 5 da tres cartas distintas solo el 48% de
    /// las veces y el resto te deja con dos o con una. Para "N distintas" hace falta mezclar
    /// y cortar, y eso no esta en ningun efecto del juego.
    ///
    /// COMO FUNCIONA
    ///
    ///   1. Guarda de "una vez por partida", la misma idea que `CardEffectGrantOnce`: si en
    ///      el mazo del run ya hay una carta que invoque una unidad con `param_subtype`, no
    ///      se ofrece nada. El mazo es el unico estado que sobrevive al combate y al guardado.
    ///   2. Mezcla los indices del pool con Fisher-Yates y `RandomManager.Range` sobre
    ///      `RngId.Battle`, que es el generador que usa el propio juego para esto: la partida
    ///      sigue siendo reproducible por semilla.
    ///   3. Recorta la cantidad al hueco que quede en la mano
    ///      (`GetMaxHandSize() - GetNumCardsInHand()`, lo mismo que hace el efecto del
    ///      juego) y reparte las `param_int` primeras con
    ///      `CardManager.AddNewCard(data, CardPile.HandPile,
    ///      false, false, null, true)`. Devuelve `CardState` y **no es corrutina** (hallazgo
    ///      del 21-sep con Requisition). Las cartas Tame ya son efimeras por su propio
    ///      `traits` en el JSON, asi que aqui no hay que aplicar ninguna mejora.
    ///
    /// Json (en el trigger del campeon):
    ///   { "id": "BeastmasterOffer", "name": "@CardEffectOfferRandomFromPool",
    ///     "target_mode": "pyre", "target_team": "monsters",
    ///     "param_card_pool": "@FreeCompanyTameOffersPool",
    ///     "param_subtype": "@Sub_Troll",
    ///     "param_int": 3 }
    ///
    /// `param_int` es cuantas ofrecer; se recorta al tamano del pool. `param_subtype` es la
    /// guarda: sin el, la oferta se repetiria en cada combate.
    /// --------------------------------------------------------------------------------
    /// </summary>
    public sealed class CardEffectOfferRandomFromPool : CardEffectBase
    {
        public override PropDescriptions CreateEditorInspectorDescriptions()
        {
            return new PropDescriptions();
        }

        /// <summary>
        /// Anade cartas de verdad: en una pasada de vista previa llenaria la mano sin que el
        /// jugador haya hecho nada. `CardEffectBase` lo devuelve true por defecto.
        /// </summary>
        public override bool CanApplyInPreviewMode => false;

        // Nunca falla: si fallase, cancelaria los efectos siguientes del trigger.
        public override bool TestEffect(CardEffectState cardEffectState, CardEffectParams cardEffectParams, ICoreGameManagers coreGameManagers)
        {
            return true;
        }

        public override IEnumerator ApplyEffect(CardEffectState cardEffectState, CardEffectParams cardEffectParams, ICoreGameManagers coreGameManagers, ISystemManagers sysManagers)
        {
            var pool = cardEffectState.GetParamCardPool();
            if (pool == null)
            {
                Log("OfferRandomFromPool: falta param_card_pool.");
                yield break;
            }

            var cardManager = coreGameManagers.GetCardManager();
            if (cardManager == null)
            {
                Log("OfferRandomFromPool: no hay CardManager.");
                yield break;
            }

            // (1) guarda de una vez por partida, por subtipo en el mazo del run.
            var subtipo = cardEffectState.GetParamSubtype();
            if (subtipo != null && !subtipo.IsNone)
            {
                var save = coreGameManagers.GetSaveManager();
                if (save == null)
                {
                    Log("OfferRandomFromPool: no hay SaveManager.");
                    yield break;
                }
                foreach (var enMazo in save.GetDeckState())
                {
                    if (enMazo == null) continue;
                    var personaje = enMazo.GetSpawnCharacterData();
                    if (personaje == null) continue;
                    foreach (var st in personaje.GetSubtypes())
                    {
                        if (st != null && st.Key == subtipo.Key)
                        {
                            Log($"OfferRandomFromPool: ya hay en el mazo una carta con el subtipo '{subtipo.Key}', no se ofrece nada.");
                            yield break;
                        }
                    }
                }
            }

            int total = pool.GetNumCards();
            if (total <= 0)
            {
                Log("OfferRandomFromPool: el pool esta vacio.");
                yield break;
            }

            int cuantas = cardEffectState.GetParamInt();
            if (cuantas <= 0) cuantas = 1;
            if (cuantas > total) cuantas = total;

            // Tope de mano. `CardEffectAddBattleCard` lo hace con
            // Mathf.Min(GetMaxHandSize() - GetNumCardsInHand(), ...) y aqui hace falta
            // igual: el trigger es `pre_combat` y la mano puede venir ya servida.
            int hueco = cardManager.GetMaxHandSize() - cardManager.GetNumCardsInHand();
            if (hueco <= 0)
            {
                Log("OfferRandomFromPool: la mano esta llena, no se ofrece nada.");
                yield break;
            }
            if (cuantas > hueco)
            {
                Log($"OfferRandomFromPool: solo caben {hueco} cartas en la mano; se recorta la oferta.");
                cuantas = hueco;
            }

            // (2) Fisher-Yates sobre los indices, con el generador del juego.
            var indices = new List<int>(total);
            for (int i = 0; i < total; i++) indices.Add(i);
            for (int i = total - 1; i > 0; i--)
            {
                int j = RandomManager.Range(0, i + 1, RngId.Battle);
                int tmp = indices[i];
                indices[i] = indices[j];
                indices[j] = tmp;
            }

            // (3) reparto.
            var repartidas = new List<string>();
            for (int k = 0; k < cuantas; k++)
            {
                var data = pool.GetCardAtIndex(indices[k]);
                if (data == null) continue;
                cardManager.AddNewCard(
                    data,                 // cardData
                    CardPile.HandPile,    // targetPile
                    false,                // fromRelic
                    false,                // permanent
                    null,                 // addCardUpgradingInfo
                    true);                // animate
                repartidas.Add(data.GetID());
            }

            Log($"OfferRandomFromPool: ofrecidas {repartidas.Count} de {total} cartas del pool a la mano: {string.Join(", ", repartidas)}.");
        }

        private static void Log(string mensaje)
        {
            global::mt2_freecompany.Plugin.Plugin.Logger.LogInfo(mensaje);
        }
    }
}
// 2026-09-21-2225||claude-mt2-the-free-company2-vesper-beastmaster||src/code/CardEffectOfferRandomFromPool.cs||clase nueva: ofrece N cartas distintas al azar de un pool a la mano, una vez por partida (guarda por subtipo en el mazo). CardEffectAddBattleCard no sirve: sortea CON reemplazo y param_bool_2 descarta la repeticion en vez de volver a sortear
