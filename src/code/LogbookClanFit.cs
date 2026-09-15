using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace mt2_freecompany.Plugin
{
    /// <summary>
    /// Encaja los rombos de clan de la pagina de mejoras de campeon del logbook
    /// (CompendiumSectionChampUpgrades) cuando hay mas clanes de los que cabian.
    ///
    /// El juego coloca los botones en dos raices, classesOptionRoot (clanes normales) y
    /// crewClassesOptionRoot (clanes de tripulacion), y esa seccion NO hereda de
    /// PaginatedCompendiumSection: no pagina, asi que con muchos mods los rombos se salen
    /// de la pagina. Aqui se reduce la celda del layout hasta que la columna entera cabe.
    ///
    /// Es un apano visual: no toca datos de partida ni guardado. Si algo no cuadra,
    /// FreeCompany.LogbookFit.Enabled a false en el config de BepInEx y todo queda como estaba.
    ///
    /// Trabaja por reflexion sobre GridLayoutGroup para no tener que referenciar
    /// UnityEngine.UI en el csproj.
    /// </summary>
    [HarmonyPatch]
    public static class LogbookClanFit
    {
        // --- ajustes, los rellena Plugin.Awake desde el config de BepInEx ---
        public static bool Enabled = true;
        public static float MinScale = 0.45f;   // hasta donde se deja encoger un rombo
        public static int MaxColumns = 0;       // 0 = respetar las columnas del juego
        public static bool Verbose = true;      // deja la traza en LogOutput.log

        static readonly FieldInfo? FClasses =
            AccessTools.Field(typeof(CompendiumSectionChampUpgrades), "classesOptionRoot");
        static readonly FieldInfo? FCrew =
            AccessTools.Field(typeof(CompendiumSectionChampUpgrades), "crewClassesOptionRoot");

        // medidas originales de cada grid, para que reaplicar no vaya encogiendo sin fin
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
            // al abrir, el rect puede medir 0 en el primer frame: se repite al siguiente
            try { seccion.StartCoroutine(EncajarAlSiguienteFrame(seccion)); }
            catch (Exception e) { Log("no se pudo encolar el reintento: " + e.Message, true); }
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
                EncajarRaiz(ComoTransform(FClasses?.GetValue(seccion)), "clanes");
                EncajarRaiz(ComoTransform(FCrew?.GetValue(seccion)), "tripulacion");
            }
            catch (Exception e)
            {
                Log("fallo encajando la pagina: " + e, true);
            }
        }

        /// <summary>
        /// El hueco disponible: el rect de la propia raiz si mide algo, y si no el del
        /// primer padre que mida. Con layouts de Unity es normal que la raiz venga a 0.
        /// </summary>
        static void BuscarHueco(RectTransform? rt, ref float ancho, ref float alto)
        {
            int saltos = 0;
            while (rt != null && saltos++ < 6)
            {
                if (rt.rect.width > 1f && rt.rect.height > 1f)
                {
                    ancho = rt.rect.width;
                    alto = rt.rect.height;
                    return;
                }
                rt = rt.parent as RectTransform;
            }
        }

        static Transform? ComoTransform(object? o) => o switch
        {
            null => null,
            GameObject go => go.transform,
            Component c => c.transform,
            _ => null,
        };

        static void EncajarRaiz(Transform? raiz, string etiqueta)
        {
            if (raiz == null || !raiz.gameObject.activeInHierarchy) return;

            int botones = 0;
            foreach (Transform hijo in raiz)
                if (hijo.gameObject.activeSelf) botones++;
            if (botones == 0) return;

            var rt = raiz as RectTransform;
            float ancho = 0f, alto = 0f;
            BuscarHueco(rt, ref ancho, ref alto);

            var grid = raiz.GetComponent("GridLayoutGroup");
            Log($"{etiqueta}: {botones} botones, hueco {ancho:0}x{alto:0}, grid={(grid != null)}");

            if (grid == null) { EncajarPorEscala(raiz, botones, alto, etiqueta); return; }
            if (alto <= 1f && ancho <= 1f) return;   // todavia sin medir

            var t = grid.GetType();
            var pCelda = t.GetProperty("cellSize");
            var pHueco = t.GetProperty("spacing");
            var pRestriccion = t.GetProperty("constraint");
            var pCuantas = t.GetProperty("constraintCount");
            if (pCelda == null || pHueco == null) return;

            int id = ((Component)grid).GetInstanceID();
            Vector2 celda = (Vector2)pCelda.GetValue(grid, null);
            Vector2 hueco = (Vector2)pHueco.GetValue(grid, null);
            if (!Originales.TryGetValue(id, out var orig))
            {
                orig = [celda.x, celda.y, hueco.x, hueco.y];
                Originales[id] = orig;
            }
            celda = new Vector2(orig[0], orig[1]);
            hueco = new Vector2(orig[2], orig[3]);

            // columnas: las que fije el grid, o las que quepan de ancho
            int columnas = 0;
            bool columnaFija = false;
            if (pRestriccion != null && pCuantas != null)
            {
                // GridLayoutGroup.Constraint: 0 Flexible, 1 FixedColumnCount, 2 FixedRowCount
                int restriccion = Convert.ToInt32(pRestriccion.GetValue(grid, null));
                if (restriccion == 1)
                {
                    columnas = Convert.ToInt32(pCuantas.GetValue(grid, null));
                    columnaFija = true;
                }
            }
            if (columnas <= 0 && ancho > 1f && celda.x > 0f)
                columnas = Mathf.Max(1, Mathf.FloorToInt((ancho + hueco.x) / (celda.x + hueco.x)));
            if (columnas <= 0) columnas = 2;

            if (MaxColumns > 0 && MaxColumns > columnas && pRestriccion != null && pCuantas != null)
            {
                columnas = MaxColumns;
                pRestriccion.SetValue(grid, 1, null);      // FixedColumnCount
                pCuantas.SetValue(grid, columnas, null);
                columnaFija = true;
            }
            else if (columnaFija && pCuantas != null)
            {
                pCuantas.SetValue(grid, columnas, null);
            }

            int filas = Mathf.CeilToInt((float)botones / columnas);

            float k = 1f;
            if (alto > 1f)
            {
                float necesario = filas * celda.y + (filas - 1) * hueco.y;
                if (necesario > alto) k = Mathf.Min(k, alto / necesario);
            }
            if (ancho > 1f)
            {
                float necesario = columnas * celda.x + (columnas - 1) * hueco.x;
                if (necesario > ancho) k = Mathf.Min(k, ancho / necesario);
            }
            k = Mathf.Clamp(k, MinScale, 1f);

            pCelda.SetValue(grid, celda * k, null);
            pHueco.SetValue(grid, hueco * k, null);
            Log($"{etiqueta}: {columnas} col x {filas} filas, factor {k:0.00} " +
                $"(celda {celda.x:0}x{celda.y:0} -> {celda.x * k:0}x{celda.y * k:0})");
        }

        /// <summary>Sin grid no hay celda que tocar: se encoge la raiz entera.</summary>
        static void EncajarPorEscala(Transform raiz, int botones, float alto, string etiqueta)
        {
            if (alto <= 1f) return;
            float altoHijo = 0f;
            foreach (Transform hijo in raiz)
            {
                if (hijo is RectTransform hrt && hrt.rect.height > altoHijo) altoHijo = hrt.rect.height;
            }
            if (altoHijo <= 1f) return;

            int filas = Mathf.CeilToInt(botones / 2f);   // la pagina va a dos columnas
            float necesario = filas * altoHijo;
            float k = necesario > alto ? Mathf.Clamp(alto / necesario, MinScale, 1f) : 1f;
            raiz.localScale = new Vector3(k, k, 1f);
            Log($"{etiqueta}: sin grid, escala {k:0.00} ({filas} filas de {altoHijo:0})");
        }

        static void Log(string mensaje, bool aviso = false)
        {
            if (aviso) Plugin.Logger.LogWarning("[LogbookFit] " + mensaje);
            else if (Verbose) Plugin.Logger.LogInfo("[LogbookFit] " + mensaje);
        }
    }
}
