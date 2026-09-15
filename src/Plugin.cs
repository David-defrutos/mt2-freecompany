using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using TrainworksReloaded.Core;
using TrainworksReloaded.Core.Extensions;

namespace mt2_freecompany.Plugin
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        internal static new ManualLogSource Logger = new(MyPluginInfo.PLUGIN_GUID);

        public void Awake()
        {
            Logger = base.Logger;

            var builder = Railhead.GetBuilder();
            builder.Configure(
                MyPluginInfo.PLUGIN_GUID,
                c =>
                {
                    // La lista de rutas JSON que carga el clan.
                    // Anadir aqui CADA fichero nuevo y recompilar: si no esta en
                    // esta lista, el juego no lo lee. Ver docs/64.
                    c.AddMergedJsonFile(
                        "json/plugin.json",
                        "json/champions/champion_roderic.json",
                        "json/champions/champion_vesper.json",
                        "json/cards/card_magic_missile.json",
                        "json/cards/card_shield_wall.json",
                        "json/units/unit_battle_chaplain.json",
                        "json/units/unit_berserker.json",
                        "json/units/unit_cutpurse.json",
                        "json/units/unit_fieldmedic.json",
                        "json/units/unit_hedgemage.json",
                        "json/units/unit_ironbound_champion.json",
                        "json/units/unit_longbowman.json",
                        "json/units/unit_master_of_coin.json",
                        "json/units/unit_shieldbearer.json",
                        "json/spells/spell_banishment.json",
                        "json/spells/spell_bless.json",
                        "json/spells/spell_cloudkill.json",
                        "json/spells/spell_cure_wounds.json",
                        "json/spells/spell_delayed_blast_fireball.json",
                        "json/spells/spell_dimension_door.json",
                        "json/spells/spell_fear.json",
                        "json/spells/spell_finger_of_death.json",
                        "json/spells/spell_fireball.json",
                        "json/spells/spell_grease.json",
                        "json/spells/spell_haste.json",
                        "json/spells/spell_invisibility.json",
                        "json/spells/spell_lightning_bolt.json",
                        "json/spells/spell_mass_hold_person.json",
                        "json/spells/spell_resurrection.json",
                        "json/spells/spell_shield.json",
                        "json/spells/spell_silence.json",
                        "json/spells/spell_web.json",
                        "json/rooms/room_caltrops.json",
                        "json/rooms/room_paymasters_tent.json",
                        "json/rooms/room_rope_and_grapple.json",
                        "json/rooms/room_the_pit.json",
                        "json/rooms/room_watchtower.json",
                        "json/kits/kit_barbarian.json",
                        "json/kits/kit_cleric.json",
                        "json/kits/kit_ranger.json",
                        "json/kits/kit_rogue.json",
                        "json/kits/kit_warrior.json",
                        "json/kits/kit_wizard.json",
                        "json/kits/spell_requisition.json"
                    );
                }
            );

            // --- apano de la pagina de mejoras del logbook (LogbookClanFit.cs) ---
            // No tiene nada que ver con el clan: es una correccion de interfaz para que
            // se vean todos los clanes instalados. Se apaga desde el config de BepInEx.
            var cfgActivo = Config.Bind(
                "LogbookFit", "Enabled", true,
                "Encaja los rombos de clan de la pagina de mejoras del logbook cuando no caben.");
            var cfgEscalaMinima = Config.Bind(
                "LogbookFit", "MinScale", 0.45f,
                "Hasta donde se deja encoger un rombo. 1 = tamano original.");
            var cfgColumnas = Config.Bind(
                "LogbookFit", "MaxColumns", 0,
                "Columnas a forzar en la rejilla. 0 = dejar las que ponga el juego.");
            var cfgTraza = Config.Bind(
                "LogbookFit", "Verbose", true,
                "Escribe en LogOutput.log lo que mide y lo que ajusta.");

            LogbookClanFit.Enabled = cfgActivo.Value;
            LogbookClanFit.MinScale = cfgEscalaMinima.Value;
            LogbookClanFit.MaxColumns = cfgColumnas.Value;
            LogbookClanFit.Verbose = cfgTraza.Value;

            new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll();

            Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        }
    }
}
