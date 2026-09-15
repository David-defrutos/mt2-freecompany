using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using HarmonyLib;
using UnityEngine;

namespace mt2_freecompany.Plugin
{
    /// <summary>
    /// Encaja los rombos de clan de la pagina de mejoras de campeon del logbook
    /// (CompendiumSectionChampUpgrades) cuando hay mas clanes de los que cabian.
    ///
    /// Como esta montada la pantalla, medido en partida el 15-sep-2026:
    ///   - Dos raices, y son las dos columnas de la hoja: classesOptionRoot (clanes
    ///     normales) y crewClassesOptionRoot (tripulacion). NO hay GridLayoutGroup: cada una
    ///     es una columna vertical con su layout.
    ///   - Cuelgan de "Clans layout root", y ese de "Clan selection", que mide **400 x 1000**:
    ///     esa es la zona visible de la hoja.
    ///   - Rombo de 136 x 136, 8 px entre filas, 96 px entre columnas.
    ///   - La seccion NO hereda de PaginatedCompendiumSection: no pagina, asi que la columna
    ///     se sale de la hoja por abajo.
    ///
    /// REGLA QUE COSTO UN INTENTO: **no se toca la jerarquia**. Mover rombos de una raiz a
    /// otra parece funcionar la primera vez y luego rompe la pantalla: el juego RECONSTRUYE
    /// los botones al abrirla, no encuentra los que le han quitado, crea otros nuevos, y los
    /// viejos se quedan ahi como rombos muertos (18 clanes -> 26 rombos, unos pulsables y
    /// otros no). Nada de SetParent, nada de clonar columnas.
    ///
    /// Lo que se hace en su lugar: apagar los layouts de las dos columnas y del contenedor,
    /// y **colocar cada rombo a mano dentro de su propia raiz**, en la rejilla de N columnas
    /// que mejor aproveche la hoja. Cada boton sigue donde el juego lo puso, asi que su
    /// reconstruccion sigue funcionando; solo cambia donde se dibuja.
    ///
    /// Para desactivarlo: [LogbookFit] Enabled = false en el config de BepInEx.
    /// </summary>
    [HarmonyPatch]
    public static class LogbookClanFit
    {
        // --- ajustes, los rellena Plugin.Awake desde el config de BepInEx ---
        public static bool Enabled = true;
        public static float MinScale = 0.45f;     // hasta donde se deja encoger
        // 2 (por defecto) = NO se toca nada: se deja la rejilla del juego y solo se escala.
        // 3 o mas = rejilla propia, que reaprovecha el ancho pero es terreno experimental.
        public static int MaxAutoColumns = 2;
        public static float ColumnSpacing = 16f;  // separacion al pasar de dos columnas
        public static float HeightBudget = 0f;    // alto util en px; 0 = detectarlo (1000)
        public static float WidthBudget = 0f;     // ancho util en px; 0 = detectarlo (400)
        public static bool Verbose = true;

        static readonly FieldInfo? FClasses =
            AccessTools.Field(typeof(CompendiumSectionChampUpgrades), "classesOptionRoot");
        static readonly FieldInfo? FCrew =
            AccessTools.Field(typeof(CompendiumSectionChampUpgrades), "crewClassesOptionRoot");
        static readonly FieldInfo? FClases =
            AccessTools.Field(typeof(CompendiumSectionChampUpgrades), "availableClasses");

        // Medidas naturales, tomadas UNA vez con el layout del juego todavia vivo.
        static float itemNatural, vgapNatural, hgapNatural;
        static Vector2 origen;
        static bool yaListado;

        [HarmonyPatch(typeof(CompendiumSectionChampUpgrades), "InitializeImpl")]
        [HarmonyPostfix]
        static void TrasInicializar(CompendiumSectionChampUpgrades __instance) => Lanzar(__instance);

        [HarmonyPatch(typeof(CompendiumSectionChampUpgrades), "Open")]
        [HarmonyPostfix]
        static void TrasAbrir(CompendiumSectionChampUpgrades __instance) => Lanzar(__instance);

        static void Lanzar(CompendiumSectionChampUpgrades seccion)
        {
            if (!Enabled || seccion == null) return;
            Encajar(seccion);
            // Al abrir, el layout puede no estar resuelto en el primer frame. Se repite al
            // siguiente, y solo si el objeto esta activo: en InitializeImpl aun no lo esta.
            if (seccion.isActiveAndEnabled)
            {
                try { seccion.StartCoroutine(EncajarAlSiguienteFrame(seccion)); }
                catch (Exception e) { Log("no se pudo encolar el reintento: " + e.Message, true); }
            }
        }

        static IEnumerator EncajarAlSiguienteFrame(CompendiumSectionChampUpgrades seccion)
        {
            yield return null;
            Encajar(seccion);
        }

        static void Encajar(CompendiumSectionChampUpgrades seccion)
        {
            try
            {
                ListarClanes(seccion);

                var c1 = ComoTransform(FClasses?.GetValue(seccion));
                var c2 = ComoTransform(FCrew?.GetValue(seccion));
                if (c1 == null || c1.parent is not RectTransform padre) return;
                bool hayCrew = c2 != null && c2.gameObject.activeInHierarchy;

                // Los rombos, cada uno donde el juego lo dejo. NO se mueven de raiz.
                var botones = new List<Transform>();
                botones.AddRange(Hijos(c1));
                if (hayCrew) botones.AddRange(Hijos(c2!));
                if (botones.Count == 0) return;

                float zonaAncho = 0f, zonaAlto = 0f;
                Zona(padre, ref zonaAncho, ref zonaAlto);
                if (HeightBudget > 1f) zonaAlto = HeightBudget;
                if (WidthBudget > 1f) zonaAncho = WidthBudget;
                if (zonaAlto <= 1f || zonaAncho <= 1f) return;

                // --- camino seguro: la rejilla la sigue haciendo el juego y aqui solo se
                // escala el contenedor hasta que quepa. Es lo unico que hay que hacer
                // mientras las dos columnas del juego basten.
                if (MaxAutoColumns <= 2)
                {
                    float contenido = c1 is RectTransform r1 ? r1.rect.height : 0f;
                    if (hayCrew && c2 is RectTransform r2) contenido = Mathf.Max(contenido, r2.rect.height);
                    float anchoContenido = padre.rect.width;
                    if (contenido <= 1f) return;

                    float ks = Mathf.Min(1f, zonaAlto / contenido);
                    if (anchoContenido > 1f) ks = Mathf.Min(ks, zonaAncho / anchoContenido);
                    ks = Mathf.Clamp(ks, MinScale, 1f);

                    c1.localScale = Vector3.one;
                    if (c2 != null) c2.localScale = Vector3.one;
                    padre.localScale = new Vector3(ks, ks, 1f);
                    Log($"{botones.Count} rombos, rejilla del juego, contenido " +
                        $"{anchoContenido:0}x{contenido:0}, zona {zonaAncho:0}x{zonaAlto:0}, factor {ks:0.00}");
                    return;
                }

                // --- camino experimental: rejilla propia de 3+ columnas.
                // Medidas naturales: hay que cogerlas antes de apagar nada, y solo valen si
                // el layout ya ha corrido (si no, las posiciones vienen a cero).
                if (!Medir(c1, hayCrew ? c2 : null)) return;

                // A partir de aqui la rejilla la llevamos nosotros: fuera los layouts, que si
                // no recolocan y reescalan los rombos por su cuenta (fue lo que los dejo en
                // 112 px al meter una tercera columna).
                ApagarLayouts(c1);
                if (hayCrew) ApagarLayouts(c2!);
                ApagarLayouts(padre);

                // --- cuantas columnas dejan los rombos mas grandes
                int total = botones.Count;
                int mejorN = 2;
                float mejorK = 0f;
                for (int n = 1; n <= Mathf.Max(1, MaxAutoColumns); n++)
                {
                    int filas = Mathf.CeilToInt((float)total / n);
                    float sep = n > 2 ? ColumnSpacing : hgapNatural;
                    float h = filas * itemNatural + (filas - 1) * vgapNatural;
                    float w = n * itemNatural + (n - 1) * sep;
                    float k = Mathf.Min(1f, Mathf.Min(zonaAlto / h, zonaAncho / w));
                    if (k > mejorK + 0.001f) { mejorK = k; mejorN = n; }
                }
                mejorK = Mathf.Clamp(mejorK, MinScale, 1f);

                // --- colocar, columna a columna, de arriba abajo
                int filasFinal = Mathf.CeilToInt((float)total / mejorN);
                float hgap = mejorN > 2 ? ColumnSpacing : hgapNatural;
                for (int i = 0; i < total; i++)
                {
                    int col = i / filasFinal;
                    int fila = i % filasFinal;
                    var boton = botones[i];
                    var raiz = boton.parent;
                    if (raiz == null) continue;

                    float x = origen.x + col * (itemNatural + hgap) - raiz.localPosition.x;
                    float y = origen.y - fila * (itemNatural + vgapNatural) - raiz.localPosition.y;
                    boton.localPosition = new Vector3(x, y, boton.localPosition.z);
                    boton.localScale = Vector3.one;
                    if (i == 0)
                        Log($"primer rombo: celda (0,0) en ({x:0}, {y:0}) dentro de {raiz.name} " +
                            $"(raiz en {raiz.localPosition.x:0}, {raiz.localPosition.y:0})");
                }

                // La escala va en el contenedor: encoge tambien los huecos entre columnas.
                c1.localScale = Vector3.one;
                if (c2 != null) c2.localScale = Vector3.one;
                padre.localScale = new Vector3(mejorK, mejorK, 1f);

                Log($"{total} rombos en {mejorN} columnas de {filasFinal}, factor {mejorK:0.00} " +
                    $"(zona {zonaAncho:0}x{zonaAlto:0}, rombo {itemNatural:0}, huecos {vgapNatural:0}/{hgap:0})");
            }
            catch (Exception e)
            {
                Log("fallo encajando la pagina: " + e, true);
            }
        }

        static List<Transform> Hijos(Transform raiz)
        {
            var lista = new List<Transform>();
            foreach (Transform hijo in raiz)
                if (hijo.gameObject.activeSelf) lista.Add(hijo);
            return lista;
        }

        /// <summary>
        /// Rombo, separacion entre filas, separacion entre columnas y esquina de salida, en
        /// el espacio del contenedor. Se toma UNA vez, con el layout del juego aun activo.
        /// </summary>
        static bool Medir(Transform c1, Transform? c2)
        {
            if (itemNatural > 0f) return true;

            var hijos = Hijos(c1);
            if (hijos.Count < 2) return false;
            if (hijos[0] is not RectTransform a || hijos[1] is not RectTransform b) return false;

            float alto = a.rect.height, ancho = a.rect.width;
            float paso = Mathf.Abs(b.localPosition.y - a.localPosition.y);
            if (alto < 10f || ancho < 10f || paso < 10f) return false;   // aun sin colocar

            itemNatural = alto;
            vgapNatural = Mathf.Clamp(paso - alto, 0f, 200f);
            hgapNatural = 96f;
            if (c2 != null)
            {
                float pasoX = Mathf.Abs(c2.localPosition.x - c1.localPosition.x);
                if (pasoX > ancho && pasoX - ancho < 400f) hgapNatural = pasoX - ancho;
            }
            origen = new Vector2(c1.localPosition.x + a.localPosition.x,
                                 c1.localPosition.y + a.localPosition.y);

            Log($"medidas naturales: rombo {itemNatural:0}x{ancho:0}, fila {vgapNatural:0}, " +
                $"columna {hgapNatural:0}, salida ({origen.x:0}, {origen.y:0})");
            return true;
        }

        /// <summary>
        /// Apaga los LayoutGroup y ContentSizeFitter del objeto. Behaviour.enabled es de
        /// UnityEngine, asi que no hace falta referenciar UnityEngine.UI para esto.
        /// </summary>
        static void ApagarLayouts(Transform t)
        {
            foreach (var c in t.GetComponents<Component>())
            {
                if (c == null) continue;
                var n = c.GetType().Name;
                if (!n.Contains("LayoutGroup") && !n.Contains("ContentSizeFitter")) continue;
                if (c is Behaviour b && b.enabled)
                {
                    b.enabled = false;
                    Log($"layout apagado en {t.name}: {n}");
                }
            }
        }

        /// <summary>
        /// La zona visible: el primer ancestro con rect. Medido: "Clan selection", 400 x 1000.
        /// La raiz de columnas no vale, se autoexpande con sus hijos.
        /// </summary>
        static void Zona(RectTransform desde, ref float ancho, ref float alto)
        {
            var traza = new StringBuilder();
            var p = desde.parent as RectTransform;
            int saltos = 0;
            while (p != null && saltos++ < 8)
            {
                traza.Append($" <- {p.name} {p.rect.width:0}x{p.rect.height:0}");
                if (p.rect.width > 1f && p.rect.height > 1f)
                {
                    ancho = p.rect.width;
                    alto = p.rect.height;
                    return;
                }
                p = p.parent as RectTransform;
            }
            Log($"zona: ningun ancestro mide;{traza}; se tira de la pantalla");
            ancho = Screen.width * 0.30f;
            alto = Screen.height * 0.70f;
        }

        /// <summary>
        /// Vuelca una vez por sesion los clanes que el juego ha registrado. El filtro de
        /// `availableClasses` coge TODAS las clases (solo aparta las de tripulacion si no
        /// esta desbloqueada), asi que si aqui falta un clan instalado, el problema esta en
        /// que su ClassData no se registra, no en esta pantalla.
        /// </summary>
        static void ListarClanes(CompendiumSectionChampUpgrades seccion)
        {
            if (yaListado || !Verbose || FClases == null) return;
            if (FClases.GetValue(seccion) is not IEnumerable lista) return;

            var nombres = new List<string>();
            foreach (var clase in lista)
            {
                if (clase == null) continue;
                var mTitulo = clase.GetType().GetMethod("GetTitle", Type.EmptyTypes);
                var mCrew = clase.GetType().GetMethod("IsCrew", Type.EmptyTypes);
                string titulo = mTitulo?.Invoke(clase, null) as string ?? clase.ToString();
                bool crew = mCrew?.Invoke(clase, null) is true;
                nombres.Add(crew ? titulo + " (crew)" : titulo);
            }
            yaListado = true;
            Log($"clases registradas ({nombres.Count}): " + string.Join(", ", nombres));
        }

        static Transform? ComoTransform(object? o) => o switch
        {
            null => null,
            GameObject go => go.transform,
            Component c => c.transform,
            _ => null,
        };

        static void Log(string mensaje, bool aviso = false)
        {
            if (aviso) Plugin.Logger.LogWarning("[LogbookFit] " + mensaje);
            else if (Verbose) Plugin.Logger.LogInfo("[LogbookFit] " + mensaje);
        }
    }
}
