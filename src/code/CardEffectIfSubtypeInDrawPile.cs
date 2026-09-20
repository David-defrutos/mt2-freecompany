using System;
using System.Collections;

namespace mt2_freecompany.Plugin
{
    /// <summary>
    /// Ejecuta OTRO efecto del juego base, pero SOLO SI hay en la PILA DE ROBO una carta que
    /// invoque una unidad con el subtipo de `param_subtype`.
    ///
    /// --------------------------------------------------------------------------------
    /// PARA QUE SIRVE
    ///
    /// Call of the Wild rescata un troll de la pila de robo con `CardEffectRecursion`. El
    /// problema: **cuando no hay ninguna carta que encaje, el juego roba igual**. No es un
    /// fallo nuestro, es como esta hecho: `CardEffectRecursion.TestEffect` mira cuantas
    /// cartas hay en las pilas y si cabe en la mano, pero **no mira ni el subtipo ni el
    /// filtro** (comprobado en el IL el 20-sep-2026). Por eso `should_fail_to_cast_if_test_fails`
    /// no sirve para esto: el test pasa aunque no haya ningun troll.
    ///
    /// Resultado sin esta guarda: si el troll esta en la mano, en el descarte o todavia no
    /// ha entrado en el combate, la habilidad te regala **una carta cualquiera a coste 0**.
    ///
    /// COMO FUNCIONA
    ///
    /// Igual que `CardEffectGrantOnce`: no reimplementa nada. Mira `CardManager.GetDrawPile`
    /// —con `shouldCopy` a true, que no queremos tocar la lista de verdad— y compara por
    /// `CardState.GetSpawnCharacterData().GetSubtypes()`, que es el mismo camino que usa
    /// `PickRandomCards` del juego para filtrar. Si hay, instancia por reflexion el efecto
    /// que diga `param_str` y le pasa los mismos parametros.
    ///
    /// Json:
    ///   { "id": "BeastmasterCallDraw", "name": "@CardEffectIfSubtypeInDrawPile",
    ///     "param_str": "CardEffectRecursion",
    ///     "param_subtype": "@Sub_Troll",
    ///     "target_mode": "draw_pile",
    ///     "target_card_selection_mode": "random_to_hand",
    ///     "target_subtype": "@Sub_Troll",
    ///     "param_card_filter": "@TrollOnlyMask",
    ///     "param_upgrade": "@TrollFreeSummon",
    ///     "param_int": 1 }
    ///
    /// `param_subtype` es la guarda; `target_subtype` es el filtro que usa el efecto de
    /// dentro. Son campos distintos y los dos hacen falta.
    /// --------------------------------------------------------------------------------
    /// </summary>
    public sealed class CardEffectIfSubtypeInDrawPile : CardEffectBase
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
            var subtipo = cardEffectState.GetParamSubtype();
            if (subtipo == null || subtipo.IsNone)
            {
                Log("IfSubtypeInDrawPile: falta param_subtype.");
                yield break;
            }

            var cardManager = coreGameManagers.GetCardManager();
            if (cardManager == null)
            {
                Log("IfSubtypeInDrawPile: no hay CardManager.");
                yield break;
            }

            bool hay = false;
            foreach (var carta in cardManager.GetDrawPile(true))
            {
                if (carta == null) continue;

                var personaje = carta.GetSpawnCharacterData();
                if (personaje == null) continue;

                foreach (var st in personaje.GetSubtypes())
                {
                    if (st != null && st.Key == subtipo.Key) { hay = true; break; }
                }
                if (hay) break;
            }

            if (!hay)
            {
                Log($"IfSubtypeInDrawPile: no hay ninguna carta con el subtipo '{subtipo.Key}' en la pila de robo; no se hace nada.");
                yield break;
            }

            string nombre = cardEffectState.GetParamStr();
            if (string.IsNullOrEmpty(nombre))
            {
                Log("IfSubtypeInDrawPile: falta param_str con el nombre del efecto interno.");
                yield break;
            }

            var tipo = typeof(CardEffectBase).Assembly.GetType(nombre);
            if (tipo == null)
            {
                Log($"IfSubtypeInDrawPile: no existe la clase '{nombre}' en Assembly-CSharp.");
                yield break;
            }

            CardEffectBase? interno = Activator.CreateInstance(tipo) as CardEffectBase;
            if (interno == null)
            {
                Log($"IfSubtypeInDrawPile: '{nombre}' no es un CardEffectBase.");
                yield break;
            }
            interno.Setup(cardEffectState);

            Log($"IfSubtypeInDrawPile: hay un '{subtipo.Key}' en la pila de robo; ejecutando {nombre}.");

            // Corrutina: se recorre a mano, no con un "yield return" pelado.
            var ejecutar = interno.ApplyEffect(cardEffectState, cardEffectParams, coreGameManagers, sysManagers);
            while (ejecutar.MoveNext()) yield return ejecutar.Current;
        }

        private static void Log(string mensaje)
        {
            global::mt2_freecompany.Plugin.Plugin.Logger.LogInfo(mensaje);
        }
    }
}
