using BepInEx;
using BepInEx.Logging;
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
                        "json/champions/champion_roderic_wall.json",
                        "json/champions/champion_roderic_quartermaster.json",
                        "json/champions/champion_roderic_plunder.json",
                        "json/champions/champion_vesper.json",
                        "json/champions/champion_vesper_grimoire.json",
                        "json/champions/champion_vesper_chirurgeon.json",
                        "json/champions/champion_vesper_beastmaster.json",
                        "json/cards/card_magic_missile.json",
                        "json/cards/card_shield_wall.json",
                        "json/units/unit_battle_chaplain.json",
                        "json/units/unit_berserker.json",
                        "json/units/unit_bounty_hunter.json",
                        "json/units/unit_camp_brawler.json",
                        "json/units/unit_chapel_acolyte.json",
                        "json/units/unit_cutpurse.json",
                        "json/units/unit_fieldmedic.json",
                        "json/units/unit_free_lance.json",
                        "json/units/unit_hedge_conjurer.json",
                        "json/units/unit_hedgemage.json",
                        "json/units/unit_ironbound_champion.json",
                        "json/units/unit_longbowman.json",
                        "json/units/unit_master_of_coin.json",
                        "json/units/unit_sellsword_scout.json",
                        "json/units/unit_shieldbearer.json",
                        "json/units/unit_the_new_guy.json",
                        "json/units/unit_troll.json",
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
                        "json/spells/spell_mercenarys_contract.json",
                        "json/spells/spell_paid_in_full.json",
                        "json/spells/spell_resurrection.json",
                        "json/spells/spell_shield.json",
                        "json/spells/spell_silence.json",
                        "json/spells/spell_spoils_of_war.json",
                        "json/spells/spell_web.json",
                        "json/rooms/room_caltrops.json",
                        "json/rooms/room_paymasters_tent.json",
                        "json/rooms/room_rope_and_grapple.json",
                        "json/rooms/room_the_pit.json",
                        "json/rooms/room_quartermasters_store.json",
                        "json/rooms/room_watchtower.json",
                        "json/kits/kit_barbarian.json",
                        "json/kits/kit_cleric.json",
                        "json/kits/kit_ranger.json",
                        "json/kits/kit_rogue.json",
                        "json/kits/kit_warrior.json",
                        "json/kits/kit_wizard.json",
                        "json/equipment/equip_widows_pension.json",
                        "json/relics/relic_butchers_bill.json",
                        "json/relics/relic_expanded_billet.json",
                        "json/relics/relic_field_forge.json",
                        "json/relics/relic_hazard_pay.json",
                        "json/relics/relic_letter_of_marque.json",
                        "json/relics/relic_marching_orders.json",
                        "json/relics/relic_quartermasters_seal.json",
                        "json/relics/relic_sappers_charges.json",
                        "json/relics/relic_scouts_map.json",
                        "json/relics/relic_veterans_papers.json",
                        "json/relics/relic_war_chest.json"
                    );
                }
            );

            // El arreglo de la pantalla de mejoras del logbook VIVIA AQUI hasta el 15-sep.
            // Se saco a su propio mod, David-CustomClanUIFixes
            // (https://github.com/David-defrutos/mt2-custom-clan-ui-fixes), porque no tiene
            // nada que ver con este clan y debe funcionar sin el.

            Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        }
    }
}
// 2026-09-20-2308||claude-mt2-the-free-company2-roderic-quartermaster||src/Plugin.cs||fuera la ruta json/kits/spell_requisition.json
// 2026-09-21-2035||claude-mt2-the-free-company2-varios||src/Plugin.cs||alta de la ruta json/equipment/equip_letter_of_credit.json: 71 --> 72 rutas
// 2026-09-21-2210||claude-mt2-the-free-company2-varios||src/Plugin.cs||ruta equip_letter_of_credit.json --> equip_widows_pension.json; alta de json/units/unit_the_new_guy.json: 72 --> 73 rutas
