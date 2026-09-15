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
    ///   - Dos raices, y son LAS DOS COLUMNAS que se ven en la hoja:
    ///     classesOptionRoot (clanes normales) y crewClassesOptionRoot (tripulacion).
    ///   - NO hay GridLayoutGroup. Cada raiz es una columna que se autoexpande: con 12
    ///     clanes medía 136 x 1720 (12 x 136 + 11 x 8 de separacion).
    ///   - Esa seccion NO hereda de PaginatedCompendiumSection: no pagina, asi que la
    ///     columna se sale de la hoja por abajo.
    ///
    /// Lo que se hace: medir el contenido de cada columna, buscar el hueco real (el primer
    /// ancestro que acote), y reescalar LAS DOS columnas con el mismo factor, para que no
    /// queden de tamanos distintos.
    ///
    /// Es un apano visual: no toca datos de partida ni guardado. Si algo no cuadra,
    /// [LogbookFit] Enabled = false en el config de BepInEx y todo queda como estaba.
    /// </summary>
    [HarmonyPatch]
    public static class LogbookClanFit
    {
        // --- ajustes, los rellena Plugin.Awake desde el config de BepInEx ---
        public static bool Enabled = true;
        public static float MinScale = 0.45f;   // hasta donde se deja encoger una columna
        public static int MaxColumns = 0;       // solo aplica si algun dia hubiera grid
        public static bool Verbose = true;      // deja la traza en LogOutput.log

        static readonly FieldInfo? FClasses =
            AccessTools.Field(typeof(CompendiumSectionChampUpgrades), "classesOptionRoot");
        static readonly FieldInfo? FCrew =
            AccessTools.Field(typeof(CompendiumSectionChampUpgrades), "crewClassesOptionRoot");

        // medidas originales de cada grid, por si alguna version del juego trae uno
        static readonly Dictionary<int, float[]> Originales = new();

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
            // siguiente, pero solo si el objeto esta activo: en InitializeImpl todavia no lo
            // esta y StartCoroutine soltaria un error rojo en el log.
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
                var clanes = ComoTransform(FClasses?.GetValue(seccion));
                var crew = ComoTransform(FCrew?.GetValue(seccion));

                // Mismo factor para las dos columnas: manda la que peor lo tiene.
                float k = Mathf.Min(Factor(clanes, "clanes"), Factor(crew, "tripulacion"));
                k = Mathf.Clamp(k, MinScale, 1f);

                Aplicar(clanes, k, "clanes");
                Aplicar(crew, k, "tripulacion");
            }
            catch (Exception e)
            {
                Log("fallo encajando la pagina: " + e, true);
            }
        }

        /// <summary>Cuanto hay que encoger esta columna para que quepa. 1 = cabe tal cual.</summary>
        static float Factor(Transform? raiz, string etiqueta)
        {
            if (raiz == null || !raiz.gameObject.activeInHierarchy) return 1f;

            int botones = 0;
            foreach (Transform hijo in raiz)
                if (hijo.gameObject.activeSelf) botones++;
            if (botones == 0) return 1f;

            var rt = raiz as RectTransform;
            if (rt == null) return 1f;

            float contenido = rt.rect.height;
            if (contenido <= 1f) return 1f;             // todavia sin resolver el layout

            float hueco = Hueco(rt, contenido, etiqueta);
            if (hueco <= 1f || hueco >= contenido) return 1f;

            float k = hueco / contenido;
            Log($"{etiqueta}: {botones} botones, contenido {rt.rect.width:0}x{contenido:0}, " +
                $"hueco {hueco:0}, factor {k:0.00}");
            return k;
        }

        /// <summary>
        /// El alto disponible de verdad: el primer ancestro que ACOTE, es decir el primero
        /// que mida menos que el contenido. La raiz no vale: se autoexpande con sus hijos.
        /// </summary>
        static float Hueco(RectTransform raiz, float contenido, string etiqueta)
        {
            var traza = new StringBuilder();
            var p = raiz.parent as RectTransform;
            int saltos = 0;
            while (p != null && saltos++ < 8)
            {
                traza.Append($" <- {p.name} {p.rect.width:0}x{p.rect.height:0}");
                if (p.rect.height > 1f && p.rect.height < contenido - 1f)
                {
                    Log($"{etiqueta}: acota {p.name}{traza}");
                    return p.rect.height;
                }
                p = p.parent as RectTransform;
            }
            Log($"{etiqueta}: ningun padre acota;{traza}; se tira del 70% de la pantalla");
            return Screen.height * 0.70f;
        }

        static void Aplicar(Transform? raiz, float k, string etiqueta)
        {
            if (raiz == null) return;

            // Si algun dia la pantalla trae GridLayoutGroup, mejor tocar la celda que la escala.
            var grid = raiz.GetComponent("GridLayoutGroup");
            if (grid != null) { AplicarEnGrid(grid, k, etiqueta); return; }

            raiz.localScale = new Vector3(k, k, 1f);
            Log($"{etiqueta}: escala {k:0.00}");
        }

        static void AplicarEnGrid(Component grid, float k, string etiqueta)
        {
            var t = grid.GetType();
            var pCelda = t.GetProperty("cellSize");
            var pHueco = t.GetProperty("spacing");
            if (pCelda == null || pHueco == null) return;

            int id = grid.GetInstanceID();
            Vector2 celda = (Vector2)pCelda.GetValue(grid, null);
            Vector2 hueco = (Vector2)pHueco.GetValue(grid, null);
            if (!Originales.TryGetValue(id, out var orig))
            {
                orig = [celda.x, celda.y, hueco.x, hueco.y];
                Originales[id] = orig;
            }
            celda = new Vector2(orig[0], orig[1]);
            hueco = new Vector2(orig[2], orig[3]);

            if (MaxColumns > 0)
            {
                var pRestriccion = t.GetProperty("constraint");
                var pCuantas = t.GetProperty("constraintCount");
                if (pRestriccion != null && pCuantas != null)
                {
                    pRestriccion.SetValue(grid, 1, null);   // FixedColumnCount
                    pCuantas.SetValue(grid, MaxColumns, null);
                }
            }

            pCelda.SetValue(grid, celda * k, null);
            pHueco.SetValue(grid, hueco * k, null);
            Log($"{etiqueta}: grid, celda {celda.x:0}x{celda.y:0} -> {celda.x * k:0}x{celda.y * k:0}");
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
