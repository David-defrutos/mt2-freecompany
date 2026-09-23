using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace mt2_freecompany.Plugin
{
    /// <summary>
    /// Cuentas atras por CAMBIO DE PISO: Banished (Banishment) y Fuse (Delayed Blast Fireball).
    ///
    /// --------------------------------------------------------------------------------
    /// POR QUE NO VALEN DOOM NI TIMEBOMB (23-sep-2026, probado en partida por David)
    ///
    /// Los dos del juego bajan una carga por RONDA. En Endless eso es una carga por cada
    /// ronda de ataque, y ademas Timebomb no hizo nada al llegar a 0. Lo que se quiere es
    /// que baje cuando el enemigo CAMBIA DE PISO, y que al acabarse haga dano.
    ///
    /// --------------------------------------------------------------------------------
    /// LAS TRES PIEZAS
    ///
    /// 1. `CardEffectPlantCountdown` HEREDA de `CardEffectDamage`. No hace dano al lanzarse:
    ///    lo que se aprovecha de la herencia es que `CardEffectState.GetModifiedParam` solo
    ///    aplica las mejoras de la carta (piedras de la forja, `bonus_damage` temporales como el
    ///    de Runespeak o Volatile Reagents) cuando `cardEffect is CardEffectDamage`. Asi el dano
    ///    se CALCULA AL LANZAR con todo lo que la carta tenga encima, y `[effect0.power]` en el
    ///    texto ensena el valor modificado. Pone el estado de `param_status_effects` y apunta
    ///    el dano y la carta en una tabla por unidad.
    /// 2. `CardEffectCountdownTick` va en dos triggers ocultos del enemigo (`post_ascension` y
    ///    `post_descension`, que el juego dispara en `HeroManager.DoAscension`, en los
    ///    movimientos de jefe y en `CardEffectBump`). Quita una carga de cada estado de su
    ///    `param_status_effects` que el enemigo tenga y, al llegar a 0, aplica el dano.
    /// 3. `StatusEffectFreeCompanyCountdownState` es el estado en si: no dispara nada, solo
    ///    se ve (icono, cargas y tooltip).
    ///
    /// Los triggers llegan al enemigo con una mejora temporal (`CardEffectAddTempCardUpgradeToUnits`,
    /// el mismo patron que Double Shift). Si se lanza dos veces, la mejora puede quedar dos
    /// veces y el trigger saltar dos veces por movimiento: por eso el tick recuerda en que
    /// piso conto por ultima vez y no vuelve a contar en el mismo.
    ///
    /// --------------------------------------------------------------------------------
    /// EL DANO DE LA EXPLOSION
    ///
    /// Se aplica con `CombatManager.ApplyDamageToTarget` y `playedCard` = la carta original.
    /// Con eso `CharacterState.ApplyDamage` pasa por los traits de ESA carta
    /// (`CalcBonusDamage` y `DoNotifyDamageWasApplied`), y si la carta lleva
    /// `CardTraitDamageOverflow` el sobrante se reparte como en cualquier dano Explosive.
    /// Tambien pasan los modificadores del objetivo (escudos, debilidades).
    ///
    /// Lo que NO se guarda al recargar la partida a mitad de combate es la tabla: si falta,
    /// se usa el `param_int` del estado (100 o 9999) sin mejoras.
    /// --------------------------------------------------------------------------------
    /// </summary>
    internal static class Countdowns
    {
        internal sealed class Pendiente
        {
            public int dano;
            public CardState? carta;
        }

        internal sealed class PorUnidad
        {
            public readonly Dictionary<string, Pendiente> pendientes = new Dictionary<string, Pendiente>();
            public readonly Dictionary<string, int> ultimoPiso = new Dictionary<string, int>();
        }

        internal static readonly ConditionalWeakTable<CharacterState, PorUnidad> tabla =
            new ConditionalWeakTable<CharacterState, PorUnidad>();

        internal static void Log(string mensaje)
        {
            global::mt2_freecompany.Plugin.Plugin.Logger.LogInfo(mensaje);
        }
    }

    /// <summary>
    /// Json:
    ///   { "id": "...", "name": "@CardEffectPlantCountdown",
    ///     "target_mode": "drop_target_character", "target_team": "heroes",
    ///     "param_int": 100,
    ///     "param_status_effects": [ { "status": "@fuse", "count": 2 } ] }
    /// </summary>
    public class CardEffectPlantCountdown : CardEffectDamage
    {
        // Solo para escribir una vez en el log que la vista previa se salta la tabla.
        private static bool avisoPrevia;

        public override IEnumerator ApplyEffect(CardEffectState cardEffectState, CardEffectParams cardEffectParams, ICoreGameManagers coreGameManagers, ISystemManagers sysManagers)
        {
            // Modificado por las mejoras de la carta: es lo que se hereda de CardEffectDamage.
            int dano = cardEffectState.GetParamInt();
            var estados = cardEffectState.GetParamStatusEffectStackData();
            if (estados == null || estados.Length == 0)
            {
                Countdowns.Log("PlantCountdown: falta param_status_effects.");
                yield break;
            }

            var objetivos = cardEffectParams.targets;
            if (objetivos == null) yield break;

            foreach (var unidad in objetivos)
            {
                if (unidad == null || unidad.IsDestroyed || !unidad.IsAlive) continue;

                // 23-sep-2026: la vista previa del hechizo pasa por aqui con la MISMA
                // CharacterState (PreviewMode activo), una vez por cada vez que el juego
                // recalcula la previa. Apuntar el dano en esas pasadas lo acumulaba: el log dio
                // "dano pendiente 100, 200, 300" para un solo lanzamiento y la explosion hizo 300.
                // En previa solo se pone el estado, para que se vea; la tabla no se toca.
                if (unidad.PreviewMode)
                {
                    if (!avisoPrevia)
                    {
                        avisoPrevia = true;
                        Countdowns.Log("PlantCountdown: [previa sin acumular] la vista previa pone el estado pero no apunta dano.");
                    }
                    foreach (var e in estados)
                    {
                        if (e != null && !string.IsNullOrEmpty(e.statusId)) unidad.AddStatusEffect(e.statusId, e.count);
                    }
                    continue;
                }

                var info = Countdowns.tabla.GetOrCreateValue(unidad);

                foreach (var e in estados)
                {
                    if (e == null || string.IsNullOrEmpty(e.statusId)) continue;
                    unidad.AddStatusEffect(e.statusId, e.count);

                    if (!info.pendientes.TryGetValue(e.statusId, out var p))
                    {
                        p = new Countdowns.Pendiente();
                        info.pendientes[e.statusId] = p;
                    }
                    // Dos lanzamientos sobre el mismo enemigo: se suman el dano y las cargas.
                    p.dano += dano;
                    p.carta = cardEffectParams.playedCard;
                    info.ultimoPiso[e.statusId] = PisoDe(unidad);

                    Countdowns.Log($"PlantCountdown: {e.statusId} x{e.count} en {unidad}, dano pendiente {p.dano}.");
                }
            }
        }

        internal static int PisoDe(CharacterState unidad)
        {
            var sala = unidad.GetCurrentRoom();
            return sala != null ? sala.GetRoomIndex() : -1;
        }
    }

    /// <summary>
    /// Json (en los triggers post_ascension y post_descension de la mejora temporal):
    ///   { "id": "...", "name": "@CardEffectCountdownTick",
    ///     "target_mode": "self", "target_team": "heroes",
    ///     "param_status_effects": [ { "status": "@fuse", "count": 1 }, { "status": "@banished", "count": 1 } ] }
    /// </summary>
    public sealed class CardEffectCountdownTick : CardEffectBase
    {
        public override PropDescriptions CreateEditorInspectorDescriptions()
        {
            return new PropDescriptions();
        }

        // Nada en las pasadas de vista previa: quita cargas y hace dano de verdad.
        public override bool CanApplyInPreviewMode => false;

        public override bool TestEffect(CardEffectState cardEffectState, CardEffectParams cardEffectParams, ICoreGameManagers coreGameManagers)
        {
            return true;
        }

        public override IEnumerator ApplyEffect(CardEffectState cardEffectState, CardEffectParams cardEffectParams, ICoreGameManagers coreGameManagers, ISystemManagers sysManagers)
        {
            var estados = cardEffectState.GetParamStatusEffectStackData();
            var objetivos = cardEffectParams.targets;
            if (estados == null || objetivos == null) yield break;

            foreach (var unidad in objetivos)
            {
                if (unidad == null || unidad.IsDestroyed || !unidad.IsAlive) continue;
                var info = Countdowns.tabla.GetOrCreateValue(unidad);
                int piso = CardEffectPlantCountdown.PisoDe(unidad);

                foreach (var e in estados)
                {
                    if (e == null || string.IsNullOrEmpty(e.statusId)) continue;
                    int cargas = unidad.GetStatusEffectStacks(e.statusId);
                    if (cargas <= 0) continue;

                    // Mismo piso que la ultima cuenta: es una copia repetida del trigger, no un
                    // movimiento nuevo.
                    if (info.ultimoPiso.TryGetValue(e.statusId, out int ultimo) && ultimo == piso) continue;
                    info.ultimoPiso[e.statusId] = piso;

                    unidad.RemoveStatusEffect(e.statusId, 1, true);
                    Countdowns.Log($"CountdownTick: {e.statusId} {cargas} -> {cargas - 1} en {unidad} (piso {piso}).");
                    if (cargas - 1 > 0) continue;

                    int dano;
                    CardState? carta = null;
                    if (info.pendientes.TryGetValue(e.statusId, out var p))
                    {
                        dano = p.dano;
                        carta = p.carta;
                        info.pendientes.Remove(e.statusId);
                    }
                    else
                    {
                        dano = ValorDelEstado(coreGameManagers, e.statusId);
                        Countdowns.Log($"CountdownTick: sin dano apuntado para {e.statusId}; uso el param_int del estado, {dano}.");
                    }

                    if (dano <= 0) continue;
                    Countdowns.Log($"CountdownTick: {e.statusId} a 0, {dano} de dano a {unidad}.");

                    var parametros = new CombatManager.ApplyDamageToTargetParameters
                    {
                        playedCard = carta,
                        damageType = Damage.Type.Default,
                    };
                    var golpe = coreGameManagers.GetCombatManager().ApplyDamageToTarget(dano, unidad, parametros);
                    while (golpe.MoveNext()) yield return golpe.Current;

                    if (unidad == null || unidad.IsDestroyed || !unidad.IsAlive) break;
                }
            }
        }

        private static int ValorDelEstado(ICoreGameManagers coreGameManagers, string statusId)
        {
            var datos = coreGameManagers.GetStatusEffectManager()?.GetStatusEffectDataById(statusId);
            return datos != null ? datos.GetParamInt() : 0;
        }
    }

    /// <summary>
    /// El estado visible. No dispara por si mismo: lo mueve `CardEffectCountdownTick`.
    /// `[codeint0]` en su tooltip es el dano que hara al acabarse.
    /// </summary>
    public sealed class StatusEffectFreeCompanyCountdownState : StatusEffectState
    {
        public override bool TestTrigger(InputTriggerParams inputTriggerParams, OutputTriggerParams outputTriggerParams, ICoreGameManagers coreGameManagers)
        {
            return false;
        }

        public override int GetEffectMagnitude(int stacks = 1)
        {
            var unidad = GetAssociatedCharacter();
            if (unidad != null
                && Countdowns.tabla.TryGetValue(unidad, out var info)
                && info.pendientes.TryGetValue(GetStatusId(), out var p))
            {
                return p.dano;
            }
            return GetParamInt();
        }
    }
}
// 2026-09-23-2130||claude-mt2-the-free-company2-mazo-pruebas||src/code/CountdownOnFloorChange.cs||fichero nuevo: CardEffectPlantCountdown (hereda de CardEffectDamage), CardEffectCountdownTick y StatusEffectFreeCompanyCountdownState, para Banished y Fuse
// 2026-09-23-2155||claude-mt2-the-free-company2-mazo-pruebas||src/code/CountdownOnFloorChange.cs||PlantCountdown: en PreviewMode no se apunta el dano (la previa lo acumulaba: 100 -> 300)
// 2026-09-23-2210||claude-mt2-the-free-company2-mazo-pruebas||src/code/CountdownOnFloorChange.cs||literal de log "[previa sin acumular]" (una vez por sesion) como marca de compilacion: un comentario no llega al DLL
