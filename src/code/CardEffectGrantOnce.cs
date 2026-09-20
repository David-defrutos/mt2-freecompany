using System;
using System.Collections;

namespace mt2_freecompany.Plugin
{
    /// <summary>
    /// Ejecuta OTRO efecto del juego base, pero SOLO SI la carta que da no esta ya en el
    /// mazo de la partida.
    ///
    /// --------------------------------------------------------------------------------
    /// PARA QUE SIRVE
    ///
    /// Dar una carta de verdad -no un token- al elegir una senda de campeon. El efecto que
    /// hace el trabajo es `CardEffectAddRunCard`, que mete la carta en el MAZO DEL RUN (no
    /// en el combate, que es lo que hace `CardEffectAddBattleCard`). Una vez ahi, la carta
    /// es tuya: conserva las mejoras permanentes, se puede mejorar en la tienda y en la
    /// forja, y recibe lo que reciba cualquier otra carta del mazo.
    ///
    /// EL PROBLEMA QUE RESUELVE
    ///
    /// No existe ningun disparador que salte UNA SOLA VEZ por partida. `CardUpgradeData`
    /// -lo que es una mejora de senda- tiene 37 campos y NINGUNO anade una carta al mazo
    /// (comprobado en Assembly-CSharp.dll el 20-sep-2026). Asi que el `AddRunCard` hay que
    /// colgarlo de un trigger que salta cada combate, y sin esta clase acabarias con un
    /// troll por combate.
    ///
    /// La marca de "ya se dio" NO se guarda en ningun sitio: se mira el propio mazo, que
    /// es el unico estado que sobrevive al combate y a guardar la partida.
    ///
    /// COMO FUNCIONA
    ///
    /// Igual que `CardEffectAllRooms`: no reimplementa nada. Mira el mazo, y si la carta no
    /// esta, instancia por reflexion el efecto del juego base que diga `param_str` y le pasa
    /// los mismos parametros. Todo lo demas -animacion, notificacion, guardado- lo hace el
    /// juego.
    ///
    /// Json:
    ///   { "id": "BeastmasterGrantTroll", "name": "@CardEffectGrantOnce",
    ///     "target_mode": "self", "target_team": "monsters",
    ///     "param_str": "CardEffectAddRunCard",
    ///     "param_card_pool": "@FreeCompanyTrollPool",
    ///     "param_int": 0 }
    ///
    /// `param_int` es la pila destino del `AddRunCard`: 0 = mazo, 1 = descarte,
    /// 3 = mano, 5 = encima de la pila de robo, 7 = al azar en la pila de robo.
    ///
    /// DOS FORMAS DE COMPROBAR SI "YA SE DIO"
    ///
    ///   a) por POOL (por defecto): basta con que una carta de `param_card_pool` este en el
    ///      mazo. Sirve cuando el pool que se comprueba y el que se da son el mismo.
    ///
    ///   b) por SUBTIPO (`param_subtype`): se mira si alguna carta del mazo invoca una
    ///      unidad con ese subtipo. Hace falta cuando los dos pools son DISTINTOS: en la
    ///      senda Beastmaster se ofrecen los cinco trolls y cada uno, al jugarse, se anade
    ///      a si mismo; la comprobacion tiene que ser "ya tengo UN troll", no "ya tengo
    ///      ESTE troll".
    ///
    /// Si se pone `param_subtype`, manda el (b) y el pool solo se usa para dar.
    /// --------------------------------------------------------------------------------
    /// </summary>
    public sealed class CardEffectGrantOnce : CardEffectBase
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
                Log("GrantOnce: falta param_card_pool.");
                yield break;
            }

            var save = coreGameManagers.GetSaveManager();
            if (save == null)
            {
                Log("GrantOnce: no hay SaveManager.");
                yield break;
            }

            var subtipo = cardEffectState.GetParamSubtype();
            bool porSubtipo = subtipo != null && !subtipo.IsNone;

            foreach (var enMazo in save.GetDeckState())
            {
                if (enMazo == null) continue;

                if (porSubtipo)
                {
                    // (b) ?hay ya en el mazo una carta que invoque algo con ese subtipo?
                    var personaje = enMazo.GetSpawnCharacterData();
                    if (personaje == null) continue;
                    foreach (var st in personaje.GetSubtypes())
                    {
                        if (st != null && st.Key == subtipo.Key)
                        {
                            Log($"GrantOnce: ya hay en el mazo una carta con el subtipo '{subtipo.Key}', no se da nada.");
                            yield break;
                        }
                    }
                }
                else
                {
                    // (a) ?esta ya alguna carta del pool en el mazo del run?
                    // CardPool.Contains(string) recorre su cardDataList comparando GetID(),
                    // y CardState.GetCardDataID() devuelve ese mismo id. Comprobado en el IL
                    // del juego: es la pareja correcta.
                    string id = enMazo.GetCardDataID();
                    if (!string.IsNullOrEmpty(id) && pool.Contains(id))
                    {
                        Log($"GrantOnce: '{id}' ya esta en el mazo, no se da otra.");
                        yield break;
                    }
                }
            }

            string nombre = cardEffectState.GetParamStr();
            if (string.IsNullOrEmpty(nombre))
            {
                Log("GrantOnce: falta param_str con el nombre del efecto interno.");
                yield break;
            }

            var tipo = typeof(CardEffectBase).Assembly.GetType(nombre);
            if (tipo == null)
            {
                Log($"GrantOnce: no existe la clase '{nombre}' en Assembly-CSharp.");
                yield break;
            }

            CardEffectBase? interno = Activator.CreateInstance(tipo) as CardEffectBase;
            if (interno == null)
            {
                Log($"GrantOnce: '{nombre}' no es un CardEffectBase.");
                yield break;
            }
            interno.Setup(cardEffectState);

            Log($"GrantOnce: la carta no estaba en el mazo; ejecutando {nombre}.");
            yield return interno.ApplyEffect(cardEffectState, cardEffectParams, coreGameManagers, sysManagers);
        }

        private static void Log(string mensaje)
        {
            global::mt2_freecompany.Plugin.Plugin.Logger.LogInfo(mensaje);
        }
    }
}
