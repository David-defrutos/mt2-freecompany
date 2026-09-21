using System;
using System.Collections;
using System.Collections.Generic;

namespace mt2_freecompany.Plugin
{
    /// <summary>
    /// Requisition: mete EN LA MANO la carta de equipo que le toca al rol de la unidad
    /// apuntada, en el escalon que diga param_int.
    ///
    /// Json (una entrada por escalon de la senda Quartermaster de Roderic):
    ///   { "id": "RodericKitI", "name": "@CardEffectRequisition",
    ///     "target_mode": "drop_target_character", "target_team": "monsters",
    ///     "param_int": 1 }
    ///
    /// --------------------------------------------------------------------------------
    /// POR QUE ESTA CLASE YA NO TOCA EL EQUIPO (20-sep-2026)
    ///
    /// Las dos versiones anteriores aplicaban la mejora a mano con ApplyCardUpgrade. Eso
    /// mete los numeros pero NO equipa: ni ranura, ni icono, ni tooltip. Comprobado en el
    /// IL de Assembly-CSharp: equipar es CharacterState.AddEquipment(cardState, ...), que
    /// mete la CardState en PrimaryStateInformation.equipment, aplica los upgrades de la
    /// carta con sourceEquipment puesto y llama a charUI.AddEquipment. Nada de eso pasa
    /// por ApplyCardUpgrade a pelo.
    ///
    /// Asi que ya no se equipa desde aqui: se DA LA CARTA. Cuando el jugador la suelta
    /// sobre quien quiera, actua CardEffectAttachEquipment del juego base y hace las
    /// cuatro cosas bien, gratis. Tambien resuelve lo de poder elegir el destinatario:
    /// apuntar solo decide QUE objeto sale; a quien se le pone lo decides al soltarlo.
    ///
    /// --------------------------------------------------------------------------------
    /// LO QUE HAY QUE SABER DE LAS DOS LLAMADAS
    ///
    ///   - Trainworks registra las cartas con el GUID delante: el nombre real de Buckler
    ///     es "mt2_freecompany.Plugin-Card-Buckler". Visto en el log de arranque
    ///     ([CardDataRegister] Register Card ...). Por eso los nombres se componen.
    ///
    ///   - CardManager.AddCard(cardData, targetPile, currentCardIndex, maxCardsToShowcase,
    ///     fromRelic, permanent, addCardUpgradingInfo, animate, animationTimeScale)
    ///     devuelve CardState, NO IEnumerator: no es corrutina y no hay que recorrerla.
    ///     Es la misma llamada con la que termina CardEffectAddBattleCard.
    ///
    ///   - addCardUpgradingInfo puede ir a null: ProcessAddCardUpgrades hace
    ///     "if (info == null) return;" en su primera instruccion. Comprobado en el IL.
    ///
    ///   - CardPile.HandPile vale 3, que es el mismo 3 que se pone en param_int cuando se
    ///     usa CardEffectAddBattleCard desde JSON.
    ///
    ///   - AddCard devuelve null y saca "cards drawn" si la mano esta llena y la carta no
    ///     es permanente. No es un fallo de esta clase; se registra en el log y ya.
    ///
    /// OJO: todo lo de arriba esta leido de MonsterTrain2.Api 2.1.20112166, que es la que
    /// hay en la cache de NuGet de este equipo. El csproj compila contra la 4.5.x. Si
    /// alguna firma se ha movido, lo dice Actions al compilar.
    /// --------------------------------------------------------------------------------
    /// </summary>
    public sealed class CardEffectRequisition : CardEffectBase
    {
        // El GUID del plugin. Es el mismo de MyPluginInfo.PLUGIN_GUID; se deja literal
        // para que este fichero se entienda solo al leerlo.
        private const string PluginGuid = "mt2_freecompany.Plugin";

        // rol -> las tres cartas de equipo, en orden. Son ids de CARTA (json/kits/*.json),
        // no de mejora: lo que se da es la carta, no el upgrade.
        private static readonly Dictionary<string, string[]> Escaleras = new()
        {
            ["Sub_Warrior"]   = ["Buckler",     "KiteShield",     "BulwarkPlate"],
            ["Sub_Barbarian"] = ["NotchedAxe",  "TwinAxes",       "ReaversMaul"],
            ["Sub_Rogue"]     = ["Lockpicks",   "PoisonedDagger", "ThiefsMantle"],
            ["Sub_Wizard"]    = ["ChalkCircle", "Grimoire",       "ArchmagesStaff"],
            ["Sub_Cleric"]    = ["Bandages",    "HolySymbol",     "Reliquary"],
            ["Sub_Ranger"]    = ["Shortbow",    "Longbow",        "GreatBow"],
        };

        // System.Random a proposito: el csproj NO referencia UnityEngine.Modules, asi que
        // UnityEngine.Random no esta disponible. Ver el comentario del propio csproj.
        private static readonly Random Azar = new();

        // Como quedan los ids una vez registrados por Trainworks.
        private static string ClaveSubtipo(string rolId)
            => $"SubtytpesData_nameKey-{PluginGuid}-Subtype-{rolId}";

        private static string NombreCarta(string cardId)
            => $"{PluginGuid}-Card-{cardId}";

        public override PropDescriptions CreateEditorInspectorDescriptions()
        {
            return new PropDescriptions();
        }

        /// <summary>
        /// Solo falla en modo vista previa. Fuera de la vista previa nunca, para no
        /// cancelar los efectos siguientes de la carta.
        ///
        /// 21-sep-2026, LOS DOS INTENTOS:
        ///
        ///   1) `CanApplyInPreviewMode => false`. ROMPIO EL APUNTADO: la carta decia
        ///      "no valid target" sobre CUALQUIER unidad, incluido Roderic. La causa esta
        ///      en el IL de GameEffectHelper.TestEffect, que hace
        ///          ok = efecto.TestEffect(...);
        ///          if (esPreview) ok = ok && efecto.CanApplyInPreviewMode;
        ///      o sea que ese flag en false no solo evita ejecutar: invalida el objetivo.
        ///      Para un efecto SIN objetivo (CardEffectAddBattleCard, CardEffectDraw) da
        ///      igual y por eso ellos lo usan; para uno con drop_target no se puede.
        ///      Sintoma en el log: ni una linea de esta clase, ApplyEffect no llegaba a
        ///      correr nunca.
        ///
        ///   2) Esto: la guarda que usa el juego base en un efecto que SI apunta.
        ///      CardEffectGrantEquipmentFromPool.TestEffect empieza exactamente asi:
        ///          var save = coreGameManagers.GetSaveManager();
        ///          if (save != null && save.PreviewMode) return false;
        ///      Copiado tal cual, que es el unico patron comprobado del juego para
        ///      "efecto con objetivo que no se debe simular".
        /// </summary>
        public override bool TestEffect(CardEffectState cardEffectState, CardEffectParams cardEffectParams, ICoreGameManagers coreGameManagers)
        {
            var save = coreGameManagers.GetSaveManager();
            if (save != null && save.PreviewMode) return false;
            return true;
        }

        /// <summary>
        /// No se puede activar con la mano llena.
        ///
        /// 21-sep-2026: CardEffectBase lo devuelve TRUE por defecto, asi que la habilidad
        /// se activaba, se comia su recarga de dos turnos y no daba nada, porque AddNewCard
        /// devuelve null cuando la mano esta llena y la carta no es permanente.
        ///
        /// El mismo GameEffectHelper.TestEffect que rompio el apuntado con
        /// CanApplyInPreviewMode hace tambien:
        ///     if (!cardManager.GetBelowHandSize()) ok = ok && efecto.CanPlayWhenHandFull;
        /// o sea que este flag SOLO se mira cuando la mano esta llena. Con la mano a medias
        /// no interviene, asi que aqui no hay el riesgo que tenia el otro: no puede
        /// invalidar objetivos salvo en el caso en que precisamente queremos invalidarlos.
        ///
        /// CardEffectAddBattleCard lo trae como campo configurable justo para esto.
        /// </summary>
        public override bool CanPlayWhenHandFull => false;

        public override IEnumerator ApplyEffect(CardEffectState cardEffectState, CardEffectParams cardEffectParams, ICoreGameManagers coreGameManagers, ISystemManagers sysManagers)
        {
            int escalon = cardEffectState.GetParamInt();
            if (escalon < 1 || escalon > 3)
            {
                Log($"Requisition: param_int={escalon} fuera de 1..3; se usa 1.");
                escalon = 1;
            }

            // Cinturon y tirantes: la guarda de verdad esta en TestEffect, pero si algun
            // camino conduce ApplyEffect igualmente en vista previa, aqui no se da carta.
            // Dar una carta no se puede simular: o se da o no se da.
            var saveManager = coreGameManagers.GetSaveManager();
            if (saveManager != null && saveManager.PreviewMode) yield break;

            var gameData = coreGameManagers.GetAllGameData();
            var cardManager = coreGameManagers.GetCardManager();
            if (gameData == null || cardManager == null)
            {
                Log("Requisition: falta AllGameData o CardManager.");
                yield break;
            }

            // drop_target_character deja el objetivo en targets[0].
            CharacterState? objetivo = null;
            var objetivos = cardEffectParams.targets;
            if (objetivos != null && objetivos.Count > 0) objetivo = objetivos[0];
            if (objetivo == null)
            {
                Log("Requisition: no hay objetivo; revisa target_mode en el JSON.");
                yield break;
            }

            string? cartaId = null;
            foreach (var par in Escaleras)
            {
                if (TieneSubtipo(objetivo, par.Key)) { cartaId = par.Value[escalon - 1]; break; }
            }

            if (cartaId == null)
            {
                // Sin rol no hay escalera: sale un objeto al azar, del MISMO escalon.
                var roles = new List<string[]>(Escaleras.Values);
                cartaId = roles[Azar.Next(roles.Count)][escalon - 1];
                Log($"Requisition: objetivo sin subtipo de rol (vistos: {ListarSubtipos(objetivo)}); al azar sale {cartaId}.");
            }

            var data = BuscarCarta(gameData, cartaId);
            if (data == null)
            {
                Log($"Requisition: NO ENCUENTRO la carta {NombreCarta(cartaId)}.");
                yield break;
            }

            // `CardManager.AddCard` NO EXISTE: el metodo publico es `AddNewCard`, y los de
            // 8-10 parametros que se le parecen son `AddNewCardWithSpecialPlacement`
            // (con indice y numero de cartas a lucir) y el privado `AddCardImpl`.
            // Comprobado en los metadatos del juego el 20-sep-2026:
            //   AddNewCard(CardData, CardPile, bool fromRelic, bool permanent,
            //              AddCardUpgradingInfo, bool animate) -> CardState
            // Devuelve CardState, NO IEnumerator: aqui no hay corrutina que recorrer.
            // addCardUpgradingInfo a null: el coste 1 y el ephemeral van en el propio JSON
            // de la carta, no hace falta parchearlos al darla.
            var creada = cardManager.AddNewCard(
                data,                 // cardData
                CardPile.HandPile,    // targetPile
                false,                // fromRelic
                false,                // permanent
                null,                 // addCardUpgradingInfo
                true);                // animate

            if (creada == null) Log($"Requisition: AddNewCard devolvio null para {cartaId} (mano llena?).");
            else Log($"Requisition: a la mano {cartaId} (escalon {escalon}).");

            yield break;
        }

        /// <summary>
        /// Busca la CardData por su nombre registrado. Se recorre GetAllCardData() mirando
        /// .name en vez de usar FindCardData(id), que compara contra otro campo.
        /// </summary>
        private static CardData? BuscarCarta(AllGameData gameData, string cardId)
        {
            string buscado = NombreCarta(cardId);
            foreach (var data in gameData.GetAllCardData())
            {
                if (data != null && data.name == buscado) return data;
            }
            return null;
        }

        private static bool TieneSubtipo(CharacterState target, string rolId)
        {
            string buscado = ClaveSubtipo(rolId);
            foreach (var st in target.GetSubtypes())
            {
                if (st != null && st.Key == buscado) return true;
            }
            return false;
        }

        private static string ListarSubtipos(CharacterState target)
        {
            var partes = new List<string>();
            foreach (var st in target.GetSubtypes())
            {
                if (st != null) partes.Add(st.Key);
            }
            return partes.Count == 0 ? "(ninguno)" : string.Join(", ", partes);
        }

        private static void Log(string mensaje)
        {
            global::mt2_freecompany.Plugin.Plugin.Logger.LogInfo(mensaje);
        }
    }
}
// 2026-09-20-2308||claude-mt2-the-free-company2-roderic-quartermaster||src/code/CardEffectRequisition.cs||reescrita: da la carta de kit a la mano en vez de equipar
// 2026-09-21-0005||claude-mt2-the-free-company2-roderic-quartermaster||src/code/CardEffectRequisition.cs||anadido CanApplyInPreviewMode => false
// 2026-09-21-2017||claude-mt2-the-free-company2-roderic-quartermaster||src/code/CardEffectRequisition.cs||revertido CanApplyInPreviewMode (rompia el apuntado) y puesta la guarda SaveManager.PreviewMode en TestEffect y en ApplyEffect
// 2026-09-21-2041||claude-mt2-the-free-company2-roderic-quartermaster||src/code/CardEffectRequisition.cs||anadido CanPlayWhenHandFull => false: con la mano llena la habilidad se gastaba la recarga sin dar nada
