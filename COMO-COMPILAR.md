# Validar instalar y empaquetar The Free Company

## Repositorio único de trabajo

Desde el 07-11-2026 las fuentes, JSON, texturas, pruebas y scripts editables están aquí:

```text
D:\Juegos\MT2_mod\repos\MT2\repos-propios\mt2-freecompany
```

GitHub recibe los commits de este repositorio. Las carpetas bajo plugins de Default son copias de ejecución, no se editan como fuentes. La carpeta anterior David-FreeCompany se conserva íntegra en un respaldo externo. Los actualizadores de repositorios propios conservan cambios/commits y solo hacen avance rápido; no restaurar reset/clean.

## Validar

Compila contra las DLL del juego y de las dependencias instaladas, valida JSON/recursos, ejecuta las ocho suites existentes y las pruebas del instalador. No instala ni prepara una publicación.

```powershell
& 'D:\Juegos\MT2_mod\repos\MT2\repos-propios\mt2-freecompany\scripts\freecompany.ps1' -Accion Validar
```

Necesita Python `C:\Users\david\miniconda3\python.exe`, .NET SDK, ilspycmd, el juego y las dependencias instaladas. La compilación local no necesita credenciales de NuGet/GitHub: usa el compilador del SDK y DLL instaladas. Las suites usan .NET 10 y Newtonsoft de Trainworks. Las advertencias nullable de stubs no son errores del build de producción.

Resultados fuera del juego: `D:\Juegos\MT2_mod\salidas\build\freecompany\<fecha>/`. `validation.json` identifica commit, cambios locales, huella de recursos/fuentes, DLL y referencias. La versión del DLL y las constantes BepInEx proceden del manifest/proyecto, no de un BuildInfo histórico. No compilar dentro de plugins ni copiar resultados automáticamente.

## Instalar para probar en Default

Cerrar Monster Train 2. Este comando valida primero y después actualiza **la copia instalada que utiliza David**, sin activar alternativas ni modificar mods.yml:

```powershell
& 'D:\Juegos\MT2_mod\repos\MT2\repos-propios\mt2-freecompany\scripts\freecompany.ps1' -Accion Instalar
```

Destino fijo:

```text
C:\Users\david\AppData\Roaming\Thunderstore Mod Manager\DataFolder\MonsterTrain2\profiles\Default\BepInEx\plugins\frutos-FreeCompany
```

Copia DLL, JSON registrados, texturas referenciadas, NOTICE y metadatos. Antes guarda respaldo completo en `D:\Juegos\MT2_mod\backups\freecompany\install\`. Verifica bytes copiados. Retira solo rutas obsoletas identificadas como propias por el historial local y el inventario anterior `.freecompany-installed-files.json`. Conserva archivos ajenos y obsoletos que el usuario haya modificado desde una instalación gestionada. No sigue enlaces de directorio ni cambia activaciones.

Para comprobar el plan sin modificar Default (también valida y compila):

```powershell
& 'D:\Juegos\MT2_mod\repos\MT2\repos-propios\mt2-freecompany\scripts\freecompany.ps1' -Accion Instalar -Simular
```

Descargar de Thunderstore puede sustituir estos archivos. Para volver a desarrollo, repetir `-Accion Instalar` sobre la misma copia instalada. No crear perfiles ni activar David-FreeCompany. Compilar o hacer push no instala.

## Preparar publicación sin publicar

Guardar primero los cambios en Git desde este repositorio. Esta acción exige árbol limpio, valida/compila y genera un ZIP con commit y huella identificados. No instala ni sube a Thunderstore.

```powershell
& 'D:\Juegos\MT2_mod\repos\MT2\repos-propios\mt2-freecompany\scripts\freecompany.ps1' -Accion Empaquetar
```

Salida predeterminada: `D:\Juegos\MT2_mod\salidas\frutos-FreeCompany-<version>-<commit>.zip`, junto a `.identity.json`. Nunca sobrescribe un ZIP existente. Se puede indicar `-Destination` con una ruta nueva. El alias `scripts/crear-paquete-thunderstore.ps1` delega a esta misma acción.

Metadatos en raíz, manifest UTF-8 sin BOM, icono 256x256. Bajo plugins van la DLL, NOTICE, JSON y texturas; recursos JSON conservan BOM/CRLF. Excluye fuentes, Git, tests, screenshots, copias y recursos sin referencia. Elimina comentarios internos del README/changelog público. Comprueba CRC y coincidencia byte a byte con lo validado.

Una versión publicada no se puede sobrescribir: comprobar disponibilidad antes de subir. Cambiar versión en manifest y csproj obliga a generar nueva DLL. El antiguo ZIP `salidas/frutos-FreeCompany-0.3.2.zip` sigue obsoleto, no usarlo como nueva candidata.

## Git y fuentes nuevas

```powershell
$repo = 'D:\Juegos\MT2_mod\repos\MT2\repos-propios\mt2-freecompany'
Set-Location -LiteralPath $repo
git status --short
# Añadir solo los archivos revisados, después commit y push desde este repositorio.
```

Si se crea un JSON nuevo, añadir su ruta a `src/Plugin.cs` y validar/compilar. Las nuevas definiciones dentro de JSON ya registrados no necesitan una ruta nueva. El pipeline local siempre compila para identificar lo que se prueba.

GitHub Actions sigue disponible para compilar la DLL del código subido, pero el flujo local no descarga fuentes remotas ni depende de ese artefacto. No usar un DLL antiguo versionado como si fuese el build actual. La DLL raíz local es una semilla ignorada; Validar/Instalar/Empaquetar usan el nuevo resultado identificado.

<!-- 2026-11-07-1207||codex-freecompany-fx||COMO-COMPILAR.md||documenta repositorio único, comandos separados, Default instalado, respaldos, obsoletos, descarga Thunderstore y ZIP identificado -->
