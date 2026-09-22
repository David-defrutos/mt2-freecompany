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

## 5. Empaquetar para Thunderstore

El paquete es un **zip plano**: los siete ficheros sueltos en la raiz, mas `json\` y
`textures\` como carpetas. **Sin `src\`, sin `.git`, sin `screenshots\` y sin el `.dll.bak`.**
Las capturas viven en el repo, no en el zip: el README las enlaza con URL absoluta de
`raw.githubusercontent.com` porque una ruta relativa sale rota en Thunderstore.

```powershell
$mod  = "$env:APPDATA\Thunderstore Mod Manager\DataFolder\MonsterTrain2\profiles\Default\BepInEx\plugins\David-FreeCompany"
$ver  = "0.2.0"
$dest = "D:\Juegos\MT2_mod\salidas\frutos-FreeCompany-$ver.zip"
$tmp  = "D:\Juegos\MT2_mod\tmp\pkg"
Remove-Item $tmp -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Path $tmp -Force | Out-Null
foreach ($f in 'manifest.json','icon.png','README.md','CHANGELOG.md','LICENSE','NOTICE.md','mt2_freecompany.Plugin.dll') {
    Copy-Item (Join-Path $mod $f) $tmp -Force
}
Copy-Item (Join-Path $mod 'json')     $tmp -Recurse -Force
Copy-Item (Join-Path $mod 'textures') $tmp -Recurse -Force
Remove-Item $dest -Force -ErrorAction SilentlyContinue
Compress-Archive -Path "$tmp\*" -DestinationPath $dest
```

Y **mirar dentro antes de subir**, que es lo unico que Thunderstore rechaza sin explicarse
(el `manifest.json` tiene que estar en la raiz, no dentro de una carpeta):

```powershell
Add-Type -AssemblyName System.IO.Compression.FileSystem
$zip = [System.IO.Compression.ZipFile]::OpenRead($dest)
$zip.Entries | Where-Object { $_.FullName -notmatch '/' } | Select-Object FullName, Length
$zip.Entries.Count
$zip.Dispose()
```

Tienen que salir los siete, y el total ronda las 230 entradas.

## 6. Subir una version nueva

**Una version publicada no se puede sobrescribir ni borrar.** Lo que salga torcido se
arregla subiendo la siguiente.

1. **Subir el numero en los dos sitios**: el `version_number` del `manifest.json` y el
   `<Version>` del `src\*.csproj`. Si se olvida uno, el paquete dice una version y el DLL
   otra. Paso el 21-sep: el csproj iba por 0.2.0 y el manifiesto seguia en 0.1.0.
2. **Recompilar**, aunque el cambio sea solo JSON, para que el DLL lleve la version nueva:
   `& D:\Juegos\MT2_mod\scripts\publicar-y-instalar-dll.ps1 -Clan FreeCompany -Mensaje "..."`.
   Si el cambio no anade clases de C#, hace falta `-Marca` con un literal nuevo o el script
   no distingue el binario nuevo del viejo.
3. **Revisar las dependencias del `manifest.json`** contra lo que hay instalado de verdad.
   Se quedan viejas solas, y con una version vieja declarada una instalacion limpia se baja
   algo anterior a lo que el DLL necesita:

   ```powershell
   $p = "$env:APPDATA\Thunderstore Mod Manager\DataFolder\MonsterTrain2\profiles\Default\BepInEx\plugins"
   foreach ($d in 'MT2-Trainworks_Reloaded','Conductor-Conductor') {
       $m = Get-Content (Join-Path $p "$d\manifest.json") -Raw | ConvertFrom-Json
       "{0} {1}" -f $m.name, $m.version_number
   }
   ```

4. **Anadir la entrada al `CHANGELOG.md`**, arriba del todo.
5. **Empaquetar** con la seccion 5 y el `$ver` nuevo.
6. **Subir** en thunderstore.io con el team **frutos**, *Upload package*. Al ser el mismo
   `namespace` y el mismo `name`, se cuelga como version nueva del paquete existente.
7. **Probar en un perfil limpio**, nunca en este: el gestor vacia la carpeta
   `plugins\<namespace>-<nombre>` antes de descomprimir, y aqui dentro vive el repo.

<!-- 2026-09-23-0140||claude-mt2-the-free-company2-vesper-grimoire||COMO-COMPILAR.md||secciones nuevas 5 (empaquetar para Thunderstore: bloque de zip y bloque de verificacion) y 6 (subir una version nueva: los dos sitios de la version, recompilar, revisar dependencias, changelog, empaquetar, subir y probar en perfil limpio). publicar-en-thunderstore.md ya remitia a esta seccion y no existia -->
