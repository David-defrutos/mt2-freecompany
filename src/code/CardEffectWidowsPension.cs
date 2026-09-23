using System;
using System.Collections;

namespace mt2_freecompany.Plugin
{
    /// <summary>
    /// Widow's Pension: al morir la unidad que lo lleva, da oro segun LA CARTA de esa unidad.
    ///
    /// --------------------------------------------------------------------------------
    /// POR QUE YA NO INVOCA UN FREE LANCE (23-sep-2026)
    ///
    /// `CardEffectSpawnMonster` copia las mejoras de la carta del muerto (su `param_bool` sin
    /// poner), asi que el sustituto heredaba tambien la pension y al morir invocaba otro:
    /// cadena infinita. En JSON no hay forma de copiar el equipo SIN copiar la pension.
    /// David eligio convertirla en dinero (Q013/Q014).
    ///
    /// --------------------------------------------------------------------------------
    /// LA FORMULA (Q014, opcion 3)
    ///
    ///   oro = redondeo a 5 de (ataque + vida de la carta)
    ///
    /// con el ataque y la vida BASE del personaje mas las mejoras PERMANENTES de la carta
    /// (forja, sendas de campeon), y SIN el equipo ni lo temporal del combate:
    /// `CardStateModifiers.GetUpgradedStatValue(base, tipo, carta.GetCardStateModifiers())`,
    /// que es lo mismo que hace `CardState.GetHealth` pero sin `temporaryCardModifiers`.
    ///
    /// La carta de la unidad sale de `CharacterState.GetSpawnerCard()` (hallazgo 48).
    /// El oro va con `PlayerManager.AdjustGold(int, bool)`, que DEVUELVE IEnumerator: se
    /// recorre a mano (hallazgo 28).
    ///
    /// Json (en el trigger on_death de la mejora del equipo):
    ///   { "id": "...", "name": "@CardEffectWidowsPension",
    ///     "target_mode": "self", "target_team": "monsters", "param_int": 5 }
    /// `param_int` es el multiplo al que se redondea (5 si falta).
    /// --------------------------------------------------------------------------------
    /// </summary>
    public sealed class CardEffectWidowsPension : CardEffectBase
    {
        public override PropDescriptions CreateEditorInspectorDescriptions()
        {
            return new PropDescriptions();
        }

        // Da oro de verdad: nada en las pasadas de vista previa.
        public override bool CanApplyInPreviewMode => false;

        public override bool TestEffect(CardEffectState cardEffectState, CardEffectParams cardEffectParams, ICoreGameManagers coreGameManagers)
        {
            return true;
        }

        public override IEnumerator ApplyEffect(CardEffectState cardEffectState, CardEffectParams cardEffectParams, ICoreGameManagers coreGameManagers, ISystemManagers sysManagers)
        {
            int multiplo = cardEffectState.GetParamInt();
            if (multiplo <= 0) multiplo = 5;

            var jugador = coreGameManagers.GetPlayerManager();
            var objetivos = cardEffectParams.targets;
            if (jugador == null || objetivos == null) yield break;

            foreach (var unidad in objetivos)
            {
                if (unidad == null) continue;
                int oro = Calcular(unidad, multiplo, out string detalle);
                Log($"WidowsPension: {unidad} -> {oro} de oro ({detalle}).");
                if (oro <= 0) continue;

                var pago = jugador.AdjustGold(oro, true);
                while (pago.MoveNext()) yield return pago.Current;
            }
        }

        private static int Calcular(CharacterState unidad, int multiplo, out string detalle)
        {
            var carta = unidad.GetSpawnerCard();
            var datos = carta?.GetSpawnCharacterData() ?? unidad.GetSourceCharacterData();
            if (datos == null)
            {
                detalle = "sin carta ni datos de personaje";
                return 0;
            }

            int ataque = datos.GetAttackDamage();
            int vida = datos.GetHealth();
            var permanentes = carta?.GetCardStateModifiers();
            if (permanentes != null)
            {
                ataque = CardStateModifiers.GetUpgradedStatValue(ataque, CardStateModifiers.StatType.Damage, permanentes);
                vida = CardStateModifiers.GetUpgradedStatValue(vida, CardStateModifiers.StatType.HP, permanentes);
            }

            int suma = Math.Max(0, ataque) + Math.Max(0, vida);
            int oro = (int)Math.Round(suma / (double)multiplo, MidpointRounding.AwayFromZero) * multiplo;
            detalle = $"ataque {ataque} + vida {vida} = {suma}, redondeo a {multiplo}";
            return oro;
        }

        private static void Log(string mensaje)
        {
            global::mt2_freecompany.Plugin.Plugin.Logger.LogInfo(mensaje);
        }
    }
}
// 2026-09-23-2115||claude-mt2-the-free-company2-mazo-pruebas||src/code/CardEffectWidowsPension.cs||clase nueva: Widow's Pension da oro = (ataque + vida de la carta, con mejoras permanentes y sin equipo) redondeado a 5
