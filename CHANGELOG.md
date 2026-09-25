# Changelog

## 0.2.5 — 2026-09-25

Reequilibrio de la senda **Beastmaster** de Vesper y del Two-Headed Troll.

- **Beastmaster**: los trolls ya no crecen con Celebrate al acabar cada combate. Ahora,
  con Vesper Beastmaster en juego, entran con **+10** de ataque y vida **por cada anillo
  después del primero**.
- **Beastmaster II y III**: los trolls ganan solo **Trample**; se quita Sweep.
- **Beastmaster III**: *Call of the Wild* está lista **un turno antes** (recarga 4, empieza
  en 3).
- **Two-Headed Troll**: pierde Multistrike y gana la habilidad **Wrath** (atacar de
  inmediato, recarga 3), que se puede usar el mismo turno en que sale.
- **Double Shift**: ahora aplica **Dizzy**, un estado nuevo del clan con icono propio, que
  al empezar tu siguiente turno se convierte en la misma cantidad de Dazed.
- **Kit Bash**: cuesta **1** en lugar de 0.
- **Invisibility**: aplica **Stealth 2** en lugar de 3.
- **Field Medic**: cura y limpia al **empezar el combate** del piso.
- **Severance Pay** pasa a ser carta inicial de Roderic.
- Arte nuevo en 16 cartas.

## 0.2.4 — 2026-09-24

- El paquete conserva `json/` y `textures/` junto al DLL al instalarse con Gale y otros
  gestores de mods BepInEx. Corrige el caso en que el plugin cargaba, pero el clan no aparecia.
- Severance Pay muestra el dano actual de la Pyre en la descripcion, con un solo punto final.
- Shift Aim aclara que el Longbowman cambiara de objetivo cuando ataque; la habilidad no
  realiza un ataque inmediato.
- Se excluyen dos JSON de cartas descartadas que el DLL no cargaba.

## 0.2.3 — 2026-09-23

Correcciones tras la primera partida con las cartas nuevas.

Gracias a **lostone**, **Brandon**, **fohnjarmery**, **Chéradénine** y al resto de modders del
canal de Discord por las pruebas y los informes.

- **Dónde avisar de fallos**: Thunderstore no permite comentarios en la ficha. Ahora el README
  remite a las incidencias de GitHub y al canal de modding del Discord de Monster Train.

- **Kit Bash** y **Severance Pay** ya no dicen *"No valid target"*.
- **Bounty Hunter**: *Claim the Bounty* ya no cuelga la partida al apuntar a otro piso, y
  ahora cumple lo que dice: si mata, la habilidad vuelve a estar lista. Además es
  desplegable: sale en la mano de la fase de despliegue.
- **Hedge Conjurer**: ahora también es desplegable.
- **Chirurgeon**: corregido el mismo cuelgue entre pisos en sus tres escalones.
- **Quartermaster's Store**: el descuento de equipo funciona.
- **Double Shift**: ya no quita de más al acabar.
- **Rope & Grapple**: rehecha; ya no usa Haste, que solo mueve enemigos.
- **Dos estados nuevos del clan, con icono propio**, que bajan una carga **cada vez que el
  enemigo cambia de piso** (no por ronda, como Doom o Timebomb):
  - **Banishment** aplica **Banished 2**: al perder la última, el enemigo es desterrado. No
    se puede lanzar a jefes. Ya no usa Stasis, que lo dejaba intocable para siempre.
  - **Delayed Blast Fireball** aplica **Fuse 2**: al perder la última, **100** de daño
    **Explosive**. Se puede lanzar a jefes. El daño se calcula al lanzar la carta, con sus
    mejoras.
- **Widow's Pension**: ya no invoca un Free Lance (el sustituto heredaba la pensión y se
  encadenaba sin fin). Ahora, al morir quien la lleva, da oro: ataque + vida de su carta,
  con las mejoras permanentes y sin el equipo, redondeado a 5.
- **Hedge-Mage**: rehecho. Cada hechizo le da **1 Arcane Charge**; su habilidad nueva,
  **Arcane Volley**, hace **2** de daño por carga a **3** enemigos al azar de su piso y gasta
  las cargas. Antes el daño al azar saltaba al lanzar un hechizo y la vista previa no
  coincidía con lo que pasaba.
- **Field Medic**: cura y limpia al **empezar** el turno en lugar de al acabarlo, por el mismo
  motivo.
- **Roderic**: las tres habilidades de Quartermaster se llaman ahora Requisition I, II y III.
- **Sapper's Charges**: rehecho. Hacía otra cosa (dañaba a cada enemigo que cambiaba de
  piso); ahora, cuando una unidad tuya sube de piso, hace **10** de daño al primer enemigo de
  su piso nuevo.
- **Marching Orders**: las copias de la sala en los otros pisos se pueden sustituir después.
- **Letter of Marque**: daba las seis piezas de equipo III al empezar cada combate; ahora da
  una al azar, como dice su texto.
- La pantalla de botín ya no enseña un cuadrado blanco en las recompensas de carta del clan.

## 0.2.2 — 2026-09-23

- **Créditos**: el README nombra ahora a Trainworks Reloaded y Conductor, del Monster Train
  2 Modding Group, sobre los que funciona el clan.
- Corregida la frase de autoría: lo propio son las cartas, unidades, campeones y reliquias
  y los efectos a medida del DLL; el resto de efectos son del juego base y de Conductor.

## 0.2.0 — 2026-09-23 — primera versión pública (ALPHA)

Primera subida. El clan carga, se selecciona y se juega de principio a fin, pero se publica
**sin balancear** y con buena parte del contenido probado solo en mis propias partidas.

### Contenido

- **Roderic** (`Melee`+`Warrior`), con las sendas **Shield Wall**, **Quartermaster** y
  **Plunder**.
- **Vesper** (`Ranged`+`Wizard`), con las sendas **Grimoire**, **Chirurgeon** y
  **Beastmaster**, esta última con cinco trolls domables que se quedan en el mazo.
- Unidades de estandarte y de draft, hechizos, seis salas, equipo repartido en escaleras y
  artefactos.

### Lo que falta, y por eso es alpha

- El equilibrio está sin medir: hay cartas que no se han drafteado nunca.
- Los tres marcos de carta son todavía los de StewardClan.

### Se agradece

Informes de fallos y opiniones de equilibrio, en las incidencias de GitHub o en los
comentarios de la ficha.

<!-- 2026-09-25-2019||claude-mt2-the-free-company2-vesper-beastmaster||CHANGELOG.md||entrada 0.2.5: Beastmaster por anillo, fuera Celebrate y Sweep, Call of the Wild III antes, Two-Headed Troll con Wrath, Double Shift con Dizzy, Kit Bash 1, Field Medic, Severance Pay inicial, 16 artes -->

<!-- 2026-09-25-2040||david||CHANGELOG.md||entrada 0.2.5: linea nueva de Invisibility (Stealth 2 en lugar de 3) -->
