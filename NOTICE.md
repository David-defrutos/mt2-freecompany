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
| **8 texturas de `textures/`** | **son de StewardClan, puestas como placeholder** |

## AVISO sobre las texturas

Ocho texturas de clan (iconos, silueta, estandarte y los tres bordes de carta) son **arte
original de StewardClan** copiado tal cual para que el clan cargue y se vea coherente desde
el primer arranque. El resto del arte (campeones, unidades, cartas) son placeholders
generados, no arte ajeno.

**Hay que sustituirlas por arte propio antes de publicar nada.** Mientras sigan ahi, este
mod no debe subirse a Thunderstore ni distribuirse.

Lista de ficheros afectados:

- `textures/FreeCompanyBanner.png`
- `textures/FreeCompanyIconSmall.png`
- `textures/FreeCompanyIconRegular.png`
- `textures/FreeCompanyIconLarge.png`
- `textures/FreeCompanySilhouette.png`
- `textures/BorderEquipmentRoom.png`
- `textures/BorderUnit.png`
- `textures/BorderSpell.png`

## Lo que NO se ha heredado

Ni una carta, ni una unidad, ni un campeon, ni una reliquia, ni un efecto a medida de
StewardClan. Todo el contenido de este clan es propio.

## Ya no se usan

Estos tres salieron del JSON al dar a cada campeón su propio icono y retrato. Los ficheros
siguen en `textures/` y **hay que borrarlos a mano**:

- `textures/FreeCompanyClanIcon.png`
- `textures/FreeCompanyClanLockedIcon.png`
- `textures/FreeCompanyPortrait.png`
