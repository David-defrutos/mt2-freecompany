using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace mt2_freecompany.Plugin
{
    /// <summary>
    /// «Si mata con su habilidad, la habilidad vuelve a estar lista.» Bounty Hunter,
    /// Claim the Bounty.
    ///
    /// --------------------------------------------------------------------------------
    /// POR QUE NO VALE `CardEffectAdjustAbilityCooldown` (23-sep-2026, leido el IL)
    ///
    /// 1. `CardEffectAdjustAbilityCooldown` NO resta a la recarga en curso: cambia la
    ///    recarga BASE (`SetUnitAbilityCooldown`, que la deja en `Math.Max(1, x)`) y luego
    ///    recorta las acumulaciones del estado `cooldown` que haya EN ESE MOMENTO hasta el
    ///    valor nuevo sin redondear. Con recarga 1 y -1: base 1 (sin cambios) y recorte a 0.
    /// 2. La recarga que se pone al usar la habilidad NO la pone el codigo de la carta. La
    ///    pone un trigger oculto comun a todas las unidades con habilidad
    ///    (`CombatManager.GetUnitAbilityCommonData().GetCommonTriggers()`, que
    ///    `CharacterState.SetupTriggers` anade ANTES que los triggers propios), que dispara
    ///    en `OnOwnAbilityActivated` con `CardEffectResetCooldown`.
    /// 3. Orden en la cola: la muerte del objetivo encola `on_kill` durante los efectos;
    ///    `OnOwnAbilityActivated` se encola despues, en `FireUnitTriggersForCardPlayed`.
    ///    Asi que el recorte del paso 1 llega cuando todavia no hay recarga que recortar,
    ///    y acto seguido el trigger comun pone la recarga. Resultado: no hace nada.
    ///
    /// --------------------------------------------------------------------------------
    /// COMO LO RESUELVE
    ///
    /// Una clase, tres momentos, por `param_int`:
    ///   0 = `on_pre_own_ability_activated`: empieza una activacion; se olvida lo anterior.
    ///   1 = `on_kill`: si hay una activacion en marcha, se apunta la muerte; si no (muerte
    ///       en el combate normal), se limpia la recarga ya.
    ///   2 = `on_own_ability_activated`: corre DESPUES del trigger comun (va detras en la
    ///       lista de triggers de la unidad), asi que la recarga ya esta puesta y se puede
    ///       quitar. Solo si se apunto una muerte en esta activacion.
    ///
    /// Los triggers 0 y 2 van con `hide_visual_and_ignore_silence`; el texto de la unidad
    /// sigue en el `on_kill`.
    ///
    /// Json:
    ///   { "id": "...", "name": "@CardEffectRefreshAbilityOnKill",
    ///     "target_mode": "self", "target_team": "monsters", "param_int": 0 | 1 | 2 }
    /// --------------------------------------------------------------------------------
    /// </summary>
    public sealed class CardEffectRefreshAbilityOnKill : CardEffectBase
    {
        private sealed class Estado
        {
            public bool activacionEnCurso;
            public bool mato;
        }

        private static readonly ConditionalWeakTable<CharacterState, Estado> estados =
            new ConditionalWeakTable<CharacterState, Estado>();

        // `PrimaryStateInformation` es privada y `isUnitAbilityResolving` es un campo de su
        // clase anidada. Solo se usa como red: si el on_kill llegase ANTES que el
        // on_pre_own_ability_activated, esto dice igualmente que la habilidad esta en curso.
        private static readonly PropertyInfo? PropEstado =
            typeof(CharacterState).GetProperty("PrimaryStateInformation", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

        public override PropDescriptions CreateEditorInspectorDescriptions()
        {
            return new PropDescriptions();
        }

        // Cambia estado de verdad: nada en las pasadas de vista previa.
        public override bool CanApplyInPreviewMode => false;

        // Nunca falla: si fallase, cancelaria los efectos siguientes del trigger (el oro).
        public override bool TestEffect(CardEffectState cardEffectState, CardEffectParams cardEffectParams, ICoreGameManagers coreGameManagers)
        {
            return true;
        }

        public override IEnumerator ApplyEffect(CardEffectState cardEffectState, CardEffectParams cardEffectParams, ICoreGameManagers coreGameManagers, ISystemManagers sysManagers)
        {
            int momento = cardEffectState.GetParamInt();
            var objetivos = cardEffectParams.targets;
            if (objetivos == null || objetivos.Count == 0)
            {
                Log($"RefreshAbilityOnKill[{momento}]: sin objetivo (target_mode tiene que ser self).");
                yield break;
            }

            foreach (var unidad in objetivos)
            {
                if (unidad == null || !unidad.HasUnitAbility()) continue;
                var estado = estados.GetOrCreateValue(unidad);

                switch (momento)
                {
                    case 0:
                        estado.activacionEnCurso = true;
                        estado.mato = false;
                        break;

                    case 1:
                        if (estado.activacionEnCurso || HabilidadResolviendose(unidad))
                        {
                            estado.mato = true;
                            Log("RefreshAbilityOnKill: muerte durante la habilidad; la recarga se quita al terminar.");
                        }
                        else
                        {
                            int quitadas = QuitarRecarga(unidad);
                            Log($"RefreshAbilityOnKill: muerte fuera de la habilidad; quitadas {quitadas} de recarga.");
                        }
                        break;

                    case 2:
                        if (estado.mato)
                        {
                            int quitadas = QuitarRecarga(unidad);
                            Log($"RefreshAbilityOnKill: la habilidad mato; quitadas {quitadas} de recarga, lista otra vez.");
                        }
                        estado.activacionEnCurso = false;
                        estado.mato = false;
                        break;

                    default:
                        Log($"RefreshAbilityOnKill: param_int {momento} no es 0, 1 ni 2.");
                        break;
                }
            }
        }

        private static int QuitarRecarga(CharacterState unidad)
        {
            int pilas = unidad.GetStatusEffectStacks("cooldown");
            if (pilas > 0) unidad.RemoveStatusEffect("cooldown", pilas, true);
            return pilas;
        }

        private static bool HabilidadResolviendose(CharacterState unidad)
        {
            try
            {
                var info = PropEstado?.GetValue(unidad);
                var campo = info?.GetType().GetField("isUnitAbilityResolving", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                return campo != null && campo.GetValue(info) is bool resolviendo && resolviendo;
            }
            catch
            {
                return false;
            }
        }

        private static void Log(string mensaje)
        {
            global::mt2_freecompany.Plugin.Plugin.Logger.LogInfo(mensaje);
        }
    }
}
// 2026-09-23-1030||claude-mt2-the-free-company2-mazo-pruebas||src/code/CardEffectRefreshAbilityOnKill.cs||clase nueva: Claim the Bounty vuelve a estar lista si la habilidad mata (0 pre / 1 on_kill / 2 on_own_ability_activated)
