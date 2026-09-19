using System.Collections;
using System.Collections.Generic;

namespace mt2_freecompany.Plugin
{
    /// <summary>
    /// Requisition: da a la unidad objetivo el siguiente escalon del equipo de SU ROL.
    ///
    /// No hacen falta estados marcadores: CharacterState.HasUpgrade dice por que escalon
    /// va la unidad. Las mejoras se buscan por nombre entre las de AllGameData.
    ///
    /// Json:
    ///   { "id": "RequisitionEffect", "name": "@CardEffectRequisition",
    ///     "target_mode": "drop_target_character", "target_team": "monsters" }
    ///
    /// --------------------------------------------------------------------------------
    /// 19-sep-2026: ARREGLADO EL FALLO QUE LO HACIA NO FUNCIONAR NUNCA.
    ///
    /// Trainworks NO registra las cosas con el id del JSON a secas: les pone delante el
    /// GUID del plugin. Comprobado en el codigo de Trainworks y en el log de arranque:
    ///
    ///   subtipos  SubtypeDataPipeline.cs mete en SubtypeData._subtype la cadena
    ///             "SubtytpesData_nameKey-{guid}-Subtype-{id}"   (la errata
    ///             "SubtytpesData" es de Trainworks, no nuestra)
    ///   mejoras   CardUpgradePipeline.cs hace data.name = "{guid}-Upgrade-{id}"
    ///
    /// Antes se comparaba contra "Sub_Warrior" y se buscaba "UpgBuckler" pelados, asi que
    /// NUNCA coincidia nada: la carta se jugaba, costaba su ember y no hacia absolutamente
    /// nada, sin error en el log ni aviso del validador. Ahora los nombres se componen.
    /// --------------------------------------------------------------------------------
    /// </summary>
    public sealed class CardEffectRequisition : CardEffectBase
    {
        // El GUID del plugin. Es el mismo de MyPluginInfo.PLUGIN_GUID; se deja literal
        // para que este fichero se entienda solo al leerlo.
        private const string PluginGuid = "mt2_freecompany.Plugin";

        // rol -> los tres escalones, en orden. Los ids son los de json/kits/*.json.
        private static readonly Dictionary<string, string[]> Escaleras = new()
        {
            ["Sub_Warrior"]   = ["UpgBuckler",     "UpgKiteShield",     "UpgBulwarkPlate"],
            ["Sub_Barbarian"] = ["UpgNotchedAxe",  "UpgTwinAxes",       "UpgReaversMaul"],
            ["Sub_Rogue"]     = ["UpgLockpicks",   "UpgPoisonedDagger", "UpgThiefsMantle"],
            ["Sub_Wizard"]    = ["UpgChalkCircle", "UpgGrimoire",       "UpgArchmagesStaff"],
            ["Sub_Cleric"]    = ["UpgBandages",    "UpgHolySymbol",     "UpgReliquary"],
            ["Sub_Ranger"]    = ["UpgShortbow",    "UpgLongbow",        "UpgGreatBow"],
        };

        // Como quedan los ids una vez registrados por Trainworks.
        private static string ClaveSubtipo(string rolId)
            => $"SubtytpesData_nameKey-{PluginGuid}-Subtype-{rolId}";

        private static string NombreMejora(string upgradeId)
            => $"{PluginGuid}-Upgrade-{upgradeId}";

        public override PropDescriptions CreateEditorInspectorDescriptions()
        {
            return new PropDescriptions();
        }

        // NUNCA falla: si fallase, cancelaria los efectos siguientes de la carta.
        // Ese es el fallo de Pebb, y aqui no lo queremos.
        public override bool TestEffect(CardEffectState cardEffectState, CardEffectParams cardEffectParams, ICoreGameManagers coreGameManagers)
        {
            return true;
        }

        public override IEnumerator ApplyEffect(CardEffectState cardEffectState, CardEffectParams cardEffectParams, ICoreGameManagers coreGameManagers, ISystemManagers sysManagers)
        {
            var gameData = coreGameManagers.GetAllGameData();

            foreach (CharacterState target in cardEffectParams.targets)
            {
                string[]? escalera = null;
                foreach (var par in Escaleras)
                {
                    if (TieneSubtipo(target, par.Key)) { escalera = par.Value; break; }
                }
                if (escalera == null)
                {
                    // Sin rol no hay escalera. Se avisa: el fallo de esta carta es MUDO
                    // por diseno (TestEffect siempre true), asi que sin esta linea no hay
                    // forma de distinguir "no aplica" de "esta rota".
                    Log($"Requisition: objetivo sin subtipo de rol. Subtipos vistos: {ListarSubtipos(target)}");
                    continue;
                }

                // escalon actual = el ultimo que ya lleva puesto
                int actual = 0;
                for (int i = 0; i < escalera.Length; i++)
                {
                    var data = BuscarMejora(gameData, escalera[i]);
                    if (data != null && target.HasUpgrade(data)) actual = i + 1;
                }
                if (actual >= escalera.Length)
                {
                    Log("Requisition: el objetivo ya esta en el escalon III.");
                    continue;
                }

                var siguiente = BuscarMejora(gameData, escalera[actual]);
                if (siguiente == null)
                {
                    Log($"Requisition: NO ENCUENTRO la mejora {NombreMejora(escalera[actual])}.");
                    continue;
                }

                // Fuera el anterior, dentro el nuevo.
                // OJO: RemoveCardUpgrade pide el CardUpgradeState QUE LLEVA PUESTO, no el
                // CardUpgradeData (eso NO compila: CS1503). El state se localiza entre los
                // aplicados comparando su GetSourceCardUpgradeData(), y se quita FUERA del
                // bucle, porque RemoveCardUpgrade toca la lista que se esta recorriendo.
                if (actual > 0)
                {
                    var anterior = BuscarMejora(gameData, escalera[actual - 1]);
                    if (anterior != null)
                    {
                        CardUpgradeState? puesta = null;
                        foreach (var aplicada in target.GetAppliedCardUpgrades())
                        {
                            if (aplicada != null && aplicada.GetSourceCardUpgradeData() == anterior)
                            {
                                puesta = aplicada;
                                break;
                            }
                        }
                        if (puesta != null) target.RemoveCardUpgrade(puesta);
                    }
                }

                var estado = new CardUpgradeState();
                estado.Setup(siguiente, false, false);
                target.ApplyCardUpgrade(estado);
                Log($"Requisition: aplicado {escalera[actual]} (escalon {actual + 1}).");
            }
            yield break;
        }

        /// <summary>
        /// Busca la mejora por su nombre registrado. NO se usa FindCardUpgradeData porque
        /// no esta comprobado contra que campo compara; recorrer la lista y mirar .name
        /// es lo que hace el propio registro de Trainworks y no deja lugar a dudas.
        /// </summary>
        private static CardUpgradeData? BuscarMejora(AllGameData gameData, string upgradeId)
        {
            string buscado = NombreMejora(upgradeId);
            foreach (var data in gameData.GetAllCardUpgradeData())
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
