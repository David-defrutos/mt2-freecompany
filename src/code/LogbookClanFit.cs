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
    ///   - Dos raices, y son LAS DOS COLUMNAS de la hoja: classesOptionRoot (clanes
    ///     normales) y crewClassesOptionRoot (tripulacion). NO hay GridLayoutGroup: cada una
    ///     es una columna vertical que se autoexpande.
    ///   - Cuelgan de "Clans layout root", y ese de "Clan selection", que mide **400 x 1000**:
    ///     esa es la zona visible de la hoja.
    ///   - Rombo de 136 x 136, 8 px entre filas, 96 px entre columnas (368 = 2x136 + 96).
    ///   - La seccion NO hereda de PaginatedCompendiumSection: no pagina.
    ///
    /// Lo que se hace:
    ///   1. Se juntan todos los rombos y se reparten entre N columnas, creando columnas
    ///      extra si hacen falta (el ancho de la hoja da para tres).
    ///   2. Se escala **el contenedor**, no cada columna: asi encoge tambien la separacion
    ///      entre columnas y se aprovecha el ancho.
    ///   3. Se elige el N que deja los rombos mas grandes.
    ///
    /// Es un apano visual: no toca datos de partida ni guardado, y `clanOptionButtons` ya
    /// esta construida cuando corre esto y guarda referencias, no posiciones, asi que mover
    /// rombos de columna no descoloca el clan que abre cada uno.
    /// Para desactivarlo: [LogbookFit] Enabled = false en el config de BepInEx.
    /// </summary>
    [HarmonyPatch]
    public static class LogbookClanFit
    {
        // --- ajustes, los rellena Plugin.Awake desde el config de BepInEx ---
        public static bool Enabled = true;
        public static float MinScale = 0.45f;     // hasta donde se deja encoger
        public static int MaxAutoColumns = 3;     // 2 = como el juego, sin columnas extra
        public static float ColumnSpacing = 16f;  // separacion entre columnas al pasar de 2
        public static float HeightBudget = 0f;    // alto util en px; 0 = detectarlo
        public static float WidthBudget = 0f;     // ancho util en px; 0 = detectarlo
        public static bool Verbose = true;

        const string NOMBRE_EXTRA = "LogbookFitColumn";

        static readonly FieldInfo? FClasses =
            AccessTools.Field(typeof(CompendiumSectionChampUpgrades), "classesOptionRoot");
        static readonly FieldInfo? FCrew =
            AccessTools.Field(typeof(CompendiumSectionChampUpgrades), "crewClassesOptionRoot");
        static readonly FieldInfo? FClases =
            AccessTools.Field(typeof(CompendiumSectionChampUpgrades), "availableClasses");

        static bool yaListado;
        static float separacionOriginal = float.NaN;

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
                if (c1 == null) return;
                if (c1.parent is not RectTransform padre) return;

                // Columnas de las que se puede tirar: las del juego (la de tripulacion solo
                // si esta activa; si no lo esta, meter algo alli seria hacerlo desaparecer)
                // mas las que hayamos creado antes.
                var columnas = new List<Transform> { c1 };
                if (c2 != null && c2.gameObject.activeInHierarchy) columnas.Add(c2);
                columnas.AddRange(Extras(padre));

                var botones = new List<Transform>();
                foreach (var col in columnas)
                    foreach (Transform hijo in col)
                        if (hijo.gameObject.activeSelf) botones.Add(hijo);
                if (botones.Count == 0) return;

                // --- medidas, sacadas de los propios rombos, no de rects que se autoexpanden
                var primero = botones[0] as RectTransform;
                float alto = primero != null && primero.rect.height > 1f ? primero.rect.height : 136f;
                float ancho = primero != null && primero.rect.width > 1f ? primero.rect.width : 136f;
                float vgap = SeparacionVertical(columnas[0], alto);
                float hgap = columnas.Count > 1
                    ? Mathf.Abs(columnas[1].localPosition.x - columnas[0].localPosition.x) - ancho
                    : 96f;
                if (hgap < 0f || hgap > 400f) hgap = 96f;

                float zonaAlto = 0f, zonaAncho = 0f;
                Zona(padre, ref zonaAncho, ref zonaAlto);
                if (HeightBudget > 1f) zonaAlto = HeightBudget;
                if (WidthBudget > 1f) zonaAncho = WidthBudget;
                if (zonaAlto <= 1f || zonaAncho <= 1f) return;

                // --- elegir el numero de columnas que deja los rombos mas grandes
                int total = botones.Count;
                int mejorN = columnas.Count;
                float mejorK = 0f;
                for (int n = 1; n <= Mathf.Max(2, MaxAutoColumns); n++)
                {
                    int filas = Mathf.CeilToInt((float)total / n);
                    float sep = n > 2 ? ColumnSpacing : hgap;
                    float h = filas * alto + (filas - 1) * vgap;
                    float w = n * ancho + (n - 1) * sep;
                    float k = Mathf.Min(1f, Mathf.Min(zonaAlto / h, zonaAncho / w));
                    if (k > mejorK + 0.001f) { mejorK = k; mejorN = n; }
                }
                mejorK = Mathf.Clamp(mejorK, MinScale, 1f);

                // --- ajustar cuantas columnas hay y repartir
                AjustarColumnas(padre, columnas, c1, mejorN);
                Repartir(columnas, botones, mejorN);

                float sepFinal = mejorN > 2 ? ColumnSpacing : hgap;
                if (!Separacion(padre, mejorN > 2 ? ColumnSpacing : (float?)null))
                {
                    // Sin layout horizontal que las coloque, se colocan a mano: si no, la
                    // columna clonada se queda justo encima de la primera.
                    for (int c = 1; c < mejorN && c < columnas.Count; c++)
                    {
                        var pos = columnas[0].localPosition;
                        pos.x += c * (ancho + sepFinal);
                        columnas[c].localPosition = pos;
                    }
                }

                // --- escalar EL CONTENEDOR: asi encoge tambien el hueco entre columnas
                foreach (var col in columnas) col.localScale = Vector3.one;
                padre.localScale = new Vector3(mejorK, mejorK, 1f);

                int filasFinal = Mathf.CeilToInt((float)total / mejorN);
                Log($"{total} rombos en {mejorN} columnas de {filasFinal}, factor {mejorK:0.00} " +
                    $"(zona {zonaAncho:0}x{zonaAlto:0}, rombo {ancho:0} + {vgap:0}/{hgap:0} de hueco)");
            }
            catch (Exception e)
            {
                Log("fallo encajando la pagina: " + e, true);
            }
        }

        /// <summary>Paso entre dos rombos de la misma columna, menos el propio rombo.</summary>
        static float SeparacionVertical(Transform columna, float alto)
        {
            Transform? a = null;
            foreach (Transform hijo in columna)
            {
                if (!hijo.gameObject.activeSelf) continue;
                if (a == null) { a = hijo; continue; }
                float paso = Mathf.Abs(hijo.localPosition.y - a.localPosition.y);
                float gap = paso - alto;
                return gap >= 0f && gap < 200f ? gap : 8f;
            }
            return 8f;
        }

        static List<Transform> Extras(Transform padre)
        {
            var lista = new List<Transform>();
            foreach (Transform hijo in padre)
                if (hijo.name.StartsWith(NOMBRE_EXTRA, StringComparison.Ordinal)) lista.Add(hijo);
            return lista;
        }

        /// <summary>Crea o quita columnas nuestras hasta tener las que se han decidido.</summary>
        static void AjustarColumnas(RectTransform padre, List<Transform> columnas, Transform modelo, int objetivo)
        {
            while (columnas.Count < objetivo)
            {
                var clon = UnityEngine.Object.Instantiate(modelo.gameObject, padre);
                clon.name = NOMBRE_EXTRA + columnas.Count;
                // El clon viene con COPIAS de los rombos: fuera, y desenganchados ya, para
                // que no los cuente nadie mientras Destroy hace su trabajo a fin de frame.
                var sobran = new List<Transform>();
                foreach (Transform hijo in clon.transform) sobran.Add(hijo);
                foreach (var hijo in sobran)
                {
                    hijo.SetParent(null, false);
                    UnityEngine.Object.Destroy(hijo.gameObject);
                }
                clon.transform.localScale = Vector3.one;
                clon.SetActive(true);
                columnas.Add(clon.transform);
                Log($"columna extra creada: {clon.name}");
            }

            while (columnas.Count > objetivo)
            {
                var ultima = columnas[columnas.Count - 1];
                if (!ultima.name.StartsWith(NOMBRE_EXTRA, StringComparison.Ordinal)) break; // del juego, no se toca
                columnas.RemoveAt(columnas.Count - 1);
                var sueltos = new List<Transform>();
                foreach (Transform hijo in ultima) sueltos.Add(hijo);
                foreach (var hijo in sueltos) hijo.SetParent(columnas[0], false);
                UnityEngine.Object.Destroy(ultima.gameObject);
                Log($"columna extra retirada: {ultima.name}");
            }
        }

        /// <summary>Reparte los rombos en orden entre las columnas, a partes iguales.</summary>
        static void Repartir(List<Transform> columnas, List<Transform> botones, int cols)
        {
            if (cols <= 0) return;
            int porColumna = Mathf.CeilToInt((float)botones.Count / cols);
            int i = 0;
            for (int c = 0; c < cols && c < columnas.Count; c++)
            {
                for (int n = 0; n < porColumna && i < botones.Count; n++, i++)
                {
                    var boton = botones[i];
                    if (boton.parent != columnas[c]) boton.SetParent(columnas[c], false);
                    boton.SetSiblingIndex(n);
                }
            }
        }

        /// <summary>
        /// Ajusta la separacion del layout horizontal del contenedor (null = la original).
        /// Devuelve false si el contenedor no tiene layout horizontal, y entonces las
        /// columnas hay que colocarlas a mano.
        /// </summary>
        static bool Separacion(RectTransform padre, float? valor)
        {
            Component? layout = null;
            foreach (var c in padre.GetComponents<Component>())
            {
                if (c == null) continue;
                var n = c.GetType().Name;
                if (n.Contains("HorizontalLayoutGroup")) { layout = c; break; }
            }
            if (layout == null) return false;

            var p = layout.GetType().GetProperty("spacing");
            if (p == null || p.PropertyType != typeof(float)) return true;

            if (float.IsNaN(separacionOriginal)) separacionOriginal = (float)p.GetValue(layout, null);
            float nuevo = valor ?? separacionOriginal;
            if (!Mathf.Approximately((float)p.GetValue(layout, null), nuevo))
            {
                p.SetValue(layout, nuevo, null);
                Log($"separacion entre columnas: {nuevo:0}");
            }
            return true;
        }

        /// <summary>
        /// La zona visible: el primer ancestro con un rect razonable. Medido: "Clan selection",
        /// 400 x 1000. La raiz de columnas NO vale, se autoexpande con sus hijos.
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
                    Log($"zona: {p.name}{traza}");
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
