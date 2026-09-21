using System;
using System.Collections;
using System.Collections.Generic;

namespace mt2_freecompany.Plugin
{
    /// <summary>
    /// Aplica una mejora SOLO SI alguna unidad aliada, EN CUALQUIER PISO, lleva puesta una
    /// mejora concreta.
    ///
    /// --------------------------------------------------------------------------------
    /// POR QUE HACE FALTA
    ///
    /// "Si Vesper con la senda Beastmaster esta en juego, el troll entra mas fuerte" no se
    /// puede escribir en JSON por dos motivos a la vez:
    ///
    ///   1. El trigger `card_monster_played` (Rally) es DE PISO: solo salta si la unidad se
    ///      invoca en el piso de quien lleva el trigger. Con un troll de 4 pips, obligar a
    ///      que comparta piso con Vesper es obligar a jugar sin escolta.
    ///   2. No hay ninguna condicion "si existe un aliado con X" en los efectos del juego.
    ///      Los filtros (`target_subtype`, los de estado) filtran OBJETIVOS, no deciden si
    ///      el efecto se ejecuta.
    ///
    /// Con esta clase el trigger se mueve AL TROLL (`on_spawn`, que salta este donde este)
    /// y la condicion se comprueba mirando a todos los aliados del tren.
    ///
    /// COMO FUNCIONA
    ///
    /// `MonsterManager` lleva todas las unidades del jugador de todos los pisos. Se recorre
    /// buscando una que tenga puesta la mejora que diga `param_str`, y si aparece se delega
    /// en `CardEffectAddCardUpgradeToUnits` con los mismos parametros: objetivos, mejora a
    /// aplicar y `param_int_3` (0 temporal / 1 hasta morir / 2 permanente) son los suyos.
    ///
    /// --------------------------------------------------------------------------------
    /// 21-sep-2026: POR QUE MIRAR TAMBIEN LA CARTA
    ///
    /// La primera version solo preguntaba `CharacterState.HasUpgrade`. **Con las sendas de
    /// campeon eso es siempre false.** Leido el IL:
    ///
    ///   - `CharacterState.HasUpgrade(data)` es
    ///     `CardUpgradeHelper.GetUpgradesWithDataIdCount(PrimaryStateInformation.appliedCardUpgrades, data.GetID()) > 0`.
    ///   - Y el campo `appliedCardUpgrades` **solo lo escribe
    ///     `CharacterState.ApplyCardUpgrade`**: es el unico metodo de todo
    ///     Assembly-CSharp.dll que hace `stfld` sobre el (el resto son lecturas, el .ctor y
    ///     la copia de vista previa). Nada en el camino de invocacion lo rellena.
    ///
    /// Una mejora de senda se aplica a la CARTA del campeon, no a la unidad; al invocarla,
    /// las estadisticas ya vienen calculadas de la carta. Por eso Vesper Beastmaster estaba
    /// viva en el piso 2, el troll entraba en el piso 3, el efecto se ejecutaba —lo dice su
    /// propia linea de log— y decidia que no habia ningun aliado con la mejora.
    ///
    /// La via correcta es `CharacterState.GetSpawnerCard()` (devuelve el `CardState` que la
    /// invoco, puesto en `InitialSetup` desde `CharacterStateSetup.spawnerCard`) y sobre el
    /// `CardState.HasUpgrade(data)`, que recorre las mejoras **permanentes y temporales** de
    /// la carta comparando `CardUpgradeState.GetCardUpgradeDataId()` con `data.GetID()`.
    ///
    /// Se dejan las dos comprobaciones: la de la unidad sirve para mejoras que si se aplican
    /// a la unidad, y la de la carta para las sendas.
    /// --------------------------------------------------------------------------------
    ///
    /// OJO CON EL PREFIJO. Trainworks registra las mejoras como "{guid}-Upgrade-{id}", no
    /// con el id pelado del JSON. Es el mismo fallo que tuvo Requisition callado durante
    /// dias: ver la seccion 7.0.13 del diseno.
    ///
    /// Json (en el troll):
    ///   { "id": "TrollBeast1", "name": "@CardEffectIfAllyHasUpgrade",
    ///     "target_mode": "self", "target_team": "monsters",
    ///     "param_str": "upg_Beastmaster1",
    ///     "param_upgrade": "@TrollBuff1",
    ///     "param_int_3": 0 }
    /// --------------------------------------------------------------------------------
    /// </summary>
    public sealed class CardEffectIfAllyHasUpgrade : CardEffectBase
    {
        private const string PluginGuid = "mt2_freecompany.Plugin";

        private static string NombreMejora(string upgradeId)
            => $"{PluginGuid}-Upgrade-{upgradeId}";

        public override PropDescriptions CreateEditorInspectorDescriptions()
        {
            return new PropDescriptions();
        }

        // Nunca falla: si fallase, cancelaria los efectos siguientes del trigger, y aqui
        // van tres de estos en fila (uno por escalon de senda).
        public override bool TestEffect(CardEffectState cardEffectState, CardEffectParams cardEffectParams, ICoreGameManagers coreGameManagers)
        {
            return true;
        }

        public override IEnumerator ApplyEffect(CardEffectState cardEffectState, CardEffectParams cardEffectParams, ICoreGameManagers coreGameManagers, ISystemManagers sysManagers)
        {
            string idBuscado = cardEffectState.GetParamStr();
            if (string.IsNullOrEmpty(idBuscado))
            {
                Log("IfAllyHasUpgrade: falta param_str con el id de la mejora a buscar.");
                yield break;
            }

            var mejoraBuscada = BuscarMejora(coreGameManagers.GetAllGameData(), idBuscado);
            if (mejoraBuscada == null)
            {
                Log($"IfAllyHasUpgrade: NO ENCUENTRO la mejora {NombreMejora(idBuscado)}.");
                yield break;
            }

            var aliados = new List<CharacterState>();
            coreGameManagers.GetMonsterManager().AddCharactersToList(aliados);

            // DOS SITIOS donde puede estar puesta la mejora, y hay que mirar los dos.
            // Ver el bloque de comentario de arriba: las sendas de campeon viven en la
            // CARTA, no en la unidad.
            bool hay = false;
            foreach (var aliado in aliados)
            {
                if (aliado == null || aliado.IsDead) continue;

                // (a) mejora aplicada SOBRE LA UNIDAD, con CharacterState.ApplyCardUpgrade.
                if (aliado.HasUpgrade(mejoraBuscada))
                {
                    Log($"IfAllyHasUpgrade: '{aliado.GetName()}' lleva '{idBuscado}' en la unidad.");
                    hay = true;
                    break;
                }

                // (b) mejora aplicada SOBRE LA CARTA que la invoco. Es el caso de las
                // sendas de campeon: la del arbol se pone en la carta del campeon y nunca
                // pasa por appliedCardUpgrades.
                var carta = aliado.GetSpawnerCard();
                if (carta != null && carta.HasUpgrade(mejoraBuscada))
                {
                    Log($"IfAllyHasUpgrade: '{aliado.GetName()}' lleva '{idBuscado}' en la carta que la invoco.");
                    hay = true;
                    break;
                }
            }
            if (!hay)
            {
                Log($"IfAllyHasUpgrade: ningun aliado vivo lleva '{idBuscado}' (ni en la unidad ni en su carta); no se aplica nada. Aliados vivos: {ContarVivos(aliados)}.");
                yield break;
            }

            var interno = new CardEffectAddCardUpgradeToUnits();
            interno.Setup(cardEffectState);
            Log($"IfAllyHasUpgrade: hay un aliado con '{idBuscado}'; aplicando la mejora.");
            // Corrutina: se recorre a mano en vez de con un "yield return" pelado, que
            // depende de que quien conduzca este efecto entienda enumeradores anidados.
            // 20-sep-2026: ApplyCardUpgrade/RemoveCardUpgrade devuelven IEnumerator y
            // Requisition no hacia nada justo por esto. Ver CardEffectRequisition.cs.
            var ejecutar = interno.ApplyEffect(cardEffectState, cardEffectParams, coreGameManagers, sysManagers);
            while (ejecutar.MoveNext()) yield return ejecutar.Current;
        }

        private static string ContarVivos(List<CharacterState> aliados)
        {
            int n = 0;
            var nombres = new List<string>();
            foreach (var a in aliados)
            {
                if (a == null || a.IsDead) continue;
                n++;
                if (nombres.Count < 6) nombres.Add(a.GetName() + (a.GetSpawnerCard() == null ? " (sin carta)" : ""));
            }
            return n + " -> " + string.Join(", ", nombres);
        }

        private static CardUpgradeData? BuscarMejora(AllGameData gameData, string upgradeId)
        {
            string buscado = NombreMejora(upgradeId);
            foreach (var data in gameData.GetAllCardUpgradeData())
            {
                if (data != null && data.name == buscado) return data;
            }
            return null;
        }

        private static void Log(string mensaje)
        {
            global::mt2_freecompany.Plugin.Plugin.Logger.LogInfo(mensaje);
        }
    }
}
// 2026-09-21-2050||claude-mt2-the-free-company2-vesper-beastmaster||src/code/CardEffectIfAllyHasUpgrade.cs||anadida la comprobacion (b): GetSpawnerCard().HasUpgrade, porque las mejoras de senda de campeon no pasan por appliedCardUpgrades y CharacterState.HasUpgrade era siempre false
