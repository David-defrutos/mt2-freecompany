# NOTICE

Este mod parte del **andamiaje** de `StewardClan`, el mod de ejemplo del Monster Train 2
Modding Group, publicado bajo licencia **MIT**.

- Origen: https://github.com/Monster-Train-2-Modding-Group/StewardClan
- Copyright (c) 2025 Monster Train 2 Modding Group
- Licencia MIT completa en `LICENSE`

## Que se ha heredado

| que | estado |
|---|---|
| Estructura del proyecto (`src/`, `json/`, `textures/`, csproj, nuget.config) | heredado y renombrado |
| `Plugin.cs` y su `AddMergedJsonFile` | heredado y vaciado |
| Forma de `json/plugin.json` (clase, banner, pools, recompensas, estilos de carta) | heredada, contenido propio |
| **3 texturas de marco de carta** | **son de StewardClan, siguen en uso** |

## Las tres texturas que siguen siendo suyas

Los tres marcos que rodean cada carta del clan son arte de StewardClan. Se copiaron con el
andamiaje y no se han tocado desde entonces: el dibujo es el mismo que el del repo original
(504x588, marco hexagonal), solo cambia la compresion del PNG.

- `textures/BorderUnit.png`
- `textures/BorderSpell.png`
- `textures/BorderEquipmentRoom.png`

La MIT permite redistribuirlas mientras se conserven el aviso de copyright y la licencia,
que van en `LICENSE` y en este fichero, y la atribucion se repite en el `README.md` publico.
Aun asi, **conviene sustituirlas por arte propio**: son lo unico que hace que este clan se
parezca a otro en la interfaz.

## Lo que ya NO es suyo (corregido el 23-09-2026)

Hasta esta fecha este fichero decia que eran **ocho** las texturas heredadas e incluia el
estandarte, los tres iconos de clan y la silueta. **Ya no**: esas cinco se regeneraron el
15-09-2026 con arte propio (el goblin del estandarte y los iconos). Este aviso se habia
quedado viejo y bloqueaba la publicacion sin motivo.

- `textures/FreeCompanyBanner.png` — propio
- `textures/FreeCompanyIconSmall.png` — propio
- `textures/FreeCompanyIconRegular.png` — propio
- `textures/FreeCompanyIconLarge.png` — propio
- `textures/FreeCompanySilhouette.png` — propio

Los tres ficheros que este aviso marcaba como "ya no se usan, hay que borrarlos a mano"
—`FreeCompanyClanIcon.png`, `FreeCompanyClanLockedIcon.png` y `FreeCompanyPortrait.png`—
tampoco estan ya en `textures/`.

## Lo que NO se ha heredado

Ni una carta, ni una unidad, ni un campeon, ni una reliquia, ni un efecto a medida de
StewardClan. Todo el contenido de este clan es propio.
