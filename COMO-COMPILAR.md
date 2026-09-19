# Como compilar el DLL de The Free Company

> El DLL lleva dentro la lista de ficheros JSON que carga el clan. **Cada JSON nuevo
> obliga a recompilar**, aunque el JSON en si no necesite compilacion.
> Procedimiento general en `D:\Juegos\MT2_mod\docs\guias\compilar-e-instalar.md`
> (antes `docs\64-anadir-json-nuevos-y-recompilar.md`; la documentacion se reorganizo el 18-sep).

## 1. Anadir un JSON nuevo

1. Crear el fichero bajo `json\`.
2. Anadir su ruta al `AddMergedJsonFile` de `src\Plugin.cs`.
3. Commit y push a `main`. Actions compila solo (dispara con cualquier cambio en `src/`).

Hasta que el DLL nuevo este puesto, la comprobacion **[9]** de `validate-mt2-mods.ps1`
saldra en rojo: compara el disco contra las cadenas del **DLL instalado**, no contra
`src\Plugin.cs`. Es lo normal, no un fallo.

## 2. Bajar el DLL que compila Actions

El artefacto esta detras del login de GitHub: sin sesion el ZIP baja de 0 bytes. Usar
**GitHub CLI**.

```powershell
$mod = "C:\Users\david\AppData\Roaming\Thunderstore Mod Manager\DataFolder\MonsterTrain2\profiles\Default\BepInEx\plugins\David-FreeCompany"
cd $mod
gh run download --name mt2_freecompany.Plugin --dir "D:\Juegos\MT2_mod\ddls\dll-nuevo"
Copy-Item "D:\Juegos\MT2_mod\ddls\dll-nuevo\mt2_freecompany.Plugin.dll" $mod -Force
& D:\Juegos\MT2_mod\scripts\validate-mt2-mods.ps1
```

Copia de seguridad del DLL bueno **siempre antes**. Y ojo: el DLL nuevo puede pesar
exactamente lo mismo que el viejo por el alineado del PE. El tamano no sirve para saber si
se instalo; lo que lo dice es la comprobacion **[9]** del validador.

## 3. Compilar en local (alternativa)

```powershell
dotnet build .\src -c Release --output D:\Juegos\MT2_mod\_dll-build\out
```

**Nunca** compilar con la salida dentro de esta carpeta: BepInEx escanea `plugins\` en
profundidad y cargaria el plugin dos veces.

La fuente `monster-train-packages` esta en GitHub Packages y **pide credenciales aunque el
paquete sea publico**: hace falta un token personal con `read:packages`. En Actions las
pone el runner solo.

## 4. Desactivar el mod sin desinstalarlo

Renombrar el `.dll` a `.dll.off`. Renombrar la carpeta **no vale**: BepInEx rastrea
recursivo.
