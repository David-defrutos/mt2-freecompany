using System;
using System.Collections;
using System.Collections.Generic;

namespace mt2_freecompany.Plugin
{
    /// <summary>
    /// Aplica UN efecto del juego base a TODAS las unidades amigas de TODOS los pisos.
    ///
    /// --------------------------------------------------------------------------------
    /// POR QUE HACE FALTA C# PARA ESTO
    ///
    /// El enum TargetMode del juego (Assembly-CSharp) NO tiene ningun valor que signifique
    /// "todas las unidades de todos los pisos". Los unicos modos multipiso son:
    ///
    ///   Tower, Pyre, FrontInAllRooms, FrontInRoomAndRoomAbove,
    ///   WeakestAllRooms, StrongestAllRooms, RandomFromAnyRoom
    ///
    /// es decir: el de delante de cada piso, el mas fuerte, el mas debil o uno al azar.
    /// Ninguno da "todos". Las cartas del juego base que dicen "on all floors" o son
    /// FrontInAllRooms (la habilidad del pyreheart de Wyngh) o son RELIQUIAS, que si
    /// recorren pisos porque son globales. Una carta, no.
    ///
    /// Comprobado el 19-sep-2026 leyendo Assembly-CSharp.dll: el enum TargetMode y el
    /// metodo TargetModeExtensions.GetIsMultiRoomTargeting.
    /// --------------------------------------------------------------------------------
    /// COMO FUNCIONA
    ///
    /// MonsterManager lleva la lista de TODAS las unidades del jugador, de todos los pisos,
    /// en un solo sitio: AddCharactersToList(List&lt;CharacterState&gt;). De ahi sale la lista.
    ///
    /// Y en vez de reimplementar curar / poner estados (cada uno con su VFX, sus
    /// notificaciones y sus multiplicadores), se INSTANCIA el efecto del juego base que
    /// diga param_str y se le pasa la lista ya ampliada. Asi el comportamiento es
    /// exactamente el del juego, sin copiarlo.
    ///
    /// Json:
    ///   { "id": "CuraTodosLosPisos", "name": "@CardEffectAllRooms",
    ///     "target_mode": "self", "target_team": "monsters",
    ///     "param_str": "CardEffectHeal", "param_int": 18 }
    ///
    ///   { "id": "ArmaduraTodosLosPisos", "name": "@CardEffectAllRooms",
    ///     "target_mode": "self", "target_team": "monsters",
    ///     "param_str": "CardEffectAddStatusEffect",
    ///     "param_status_effects": [ { "status": "armor", "count": 22 } ] }
    ///
    /// El target_mode propio da igual: la lista se sustituye antes de llamar al efecto
    /// interno. Se pone "self" porque siempre resuelve y no cuesta nada.
    ///
    /// OJO: param_str es el nombre EXACTO de la clase del juego base, sin espacio de
    /// nombres y sin la arroba de Trainworks.
    /// </summary>
    public sealed class CardEffectAllRooms : CardEffectBase
    {
        public override PropDescriptions CreateEditorInspectorDescriptions()
        {
            return new PropDescriptions();
        }

        // Nunca falla: un TestEffect en false cancelaria los efectos siguientes de la carta.
        public override bool TestEffect(CardEffectState cardEffectState, CardEffectParams cardEffectParams, ICoreGameManagers coreGameManagers)
        {
            return true;
        }

        public override IEnumerator ApplyEffect(CardEffectState cardEffectState, CardEffectParams cardEffectParams, ICoreGameManagers coreGameManagers, ISystemManagers sysManagers)
        {
            string nombre = cardEffectState.GetParamStr();
            if (string.IsNullOrEmpty(nombre))
            {
                Log("CardEffectAllRooms: falta param_str con el nombre del efecto interno.");
                yield break;
            }

            // El efecto interno vive en Assembly-CSharp, en el espacio de nombres global.
            var tipo = typeof(CardEffectBase).Assembly.GetType(nombre);
            if (tipo == null)
            {
                Log($"CardEffectAllRooms: no existe la clase '{nombre}' en Assembly-CSharp.");
                yield break;
            }

            CardEffectBase? interno = Activator.CreateInstance(tipo) as CardEffectBase;
            if (interno == null)
            {
                Log($"CardEffectAllRooms: '{nombre}' no es un CardEffectBase.");
                yield break;
            }
            interno.Setup(cardEffectState);

            // Todas las unidades del jugador, de todos los pisos.
            var todos = new List<CharacterState>();
            coreGameManagers.GetMonsterManager().AddCharactersToList(todos);

            var vivos = new List<CharacterState>();
            foreach (var c in todos)
            {
                if (c != null && !c.IsDead) vivos.Add(c);
            }
            if (vivos.Count == 0)
            {
                Log("CardEffectAllRooms: no hay unidades vivas a las que aplicarlo.");
                yield break;
            }

            // Se sustituye la lista de objetivos EN EL MISMO objeto de parametros (no se
            // construye otro) para no perder ningun campo que el efecto interno pueda
            // mirar: playedCard, characterThatActivatedAbility, roomThatActivatedAbility...
            // Se deja como estaba al terminar, por si el objeto se reutiliza.
            var lista = cardEffectParams.targets;
            if (lista == null)
            {
                Log("CardEffectAllRooms: cardEffectParams.targets es null.");
                yield break;
            }

            var previos = new List<CharacterState>(lista);
            lista.Clear();
            lista.AddRange(vivos);

            Log($"CardEffectAllRooms: {nombre} sobre {vivos.Count} unidades de todos los pisos.");
            // Corrutina: se recorre a mano en vez de con un "yield return" pelado, que
            // depende de que quien conduzca este efecto entienda enumeradores anidados.
            // 20-sep-2026: ApplyCardUpgrade/RemoveCardUpgrade devuelven IEnumerator y
            // Requisition no hacia nada justo por esto. Ver CardEffectRequisition.cs.
            var ejecutar = interno.ApplyEffect(cardEffectState, cardEffectParams, coreGameManagers, sysManagers);
            while (ejecutar.MoveNext()) yield return ejecutar.Current;

            lista.Clear();
            lista.AddRange(previos);
        }

        private static void Log(string mensaje)
        {
            global::mt2_freecompany.Plugin.Plugin.Logger.LogInfo(mensaje);
        }
    }
}
