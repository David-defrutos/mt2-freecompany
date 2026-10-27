# Changelog

[English changelog](#english) · [Español](#spanish) · [Français](#french) · [Deutsch](#german) · [Русский](#russian) · [Português (Brasil)](#portuguese-brazil) · [简体中文](#chinese) · [繁體中文](#chinese-traditional) · [한국어](#korean) · [日本語](#japanese)

Latest release in all ten game languages. Full history in English and Spanish.

## English

### 0.3.2 — 2026-10-27 — BETA

- **Battle Chaplain:** Last Rites now uses native healing and all-floor targeting, improving compatibility and following the game's standard targeting rules.
- **Fuse and Banished:** cleansing and reapplying these effects no longer carries over damage from the previous application.
- **Requisition:** random equipment for units without a matching role now follows the game's battle seed.
- **Goblin visuals:** slightly enlarged Battle Chaplain, Chapel Acolyte, Hedge Conjurer, Longbowman, Shieldbearer and The New Guy by 10–20%, preserving proportions and foot placement.

### 0.3.1 — 2026-10-26 — BETA

Thanks to **Rratvar, the Untitled** for reporting the cleansing, champion visual, Field Forge and Butcher's Bill issues, and for the Watchtower feedback. [Bug reports and feedback](https://discord.com/channels/336546996779483136/1563998046084268032).

- **Watchtower:** starts active. Its room ability toggles Sniper and +10 attack together, with cooldown 1. The attack bonus does not stack, and Sniper from other sources is preserved. Merged rooms keep their other bonuses.
- **Field Medic and Chirurgeon:** random cleansing chooses an ally that actually has a visible debuff. Chirurgeon II/III retain their cross-floor range; repeated cleanses skip already cleansed allies.
- **Field Forge:** its discount is applied once to newly merged equipment, preserving other cost modifiers.
- **Roderic and Vesper:** adjusted animated-unit bounds, VFX anchors and the static reference sprite to address oversized deployment flashes and misplaced status effects.
- **Longbowman:** Shift Aim cooldown increases from 1 to 2. Temporarily exchanges Sniper for Conductor's Pierce 1, which hits one additional enemy behind the front target. Pierce granted by equipment is preserved.
- **Butcher's Bill:** text now explicitly includes Pyre kills. Minimum Trainworks Reloaded: **0.7.33**.
- **Known issue:** mixing champion paths can replace the previous activated ability. Still pending redesign.

### 0.3.0 — 2026-10-10 — BETA

- First **BETA** release.
- **Vesper starter:** Field Dressing costs 1 Ember, removes one random debuff from a chosen ally and restores 2 health. New artwork. Magic Missile is now a Common reward spell.
- **Beastmaster:** expanded creature roster, Tamable/Tamed paw icons, Frozen and accumulating Ember discounts in hand. Command orders a chosen Tamed creature on any floor to attack, with cooldown 4/3/2. Replaces stat growth. Fixes shop upgrade crashes and keeps the chosen Troll in the run deck.
- **Roderic:** removed Rally, adjusted path stats and removed Titanskin from Quartermaster and Plunder. Plunder's Call to Arms lets you choose a unit from hand, draw or discard with base cost up to 1/2/3; falls back to fallen units with 1 health and Undying 1. Ready on summon, cooldown 2. Fixed preview targeting validation.
- **Marching Orders:** reduces the Ember cost of the first unit each turn.
- Smoother champion animations; improved clan/status icons and Troll positioning. Corrected Champion labels, Severance Pay localization and equipment/ability text.

### 0.2.6 — 2026-09-29

- **Vesper and Roderic**: animated card artwork and unit sprites. Added intermediate frames and adjusted Vesper’s size and proportions in combat.
- **Chirurgeon**: heals and cleanses at the start of the turn so random selection does not alter the combat preview. Can also remove **Dizzy**.
- Clan cards now have visual effects and sounds suited to their actions.
- **Delayed Blast Fireball** costs **2** Ember instead of 3 and changes from Rare to Uncommon. **Haste** applies **Burst 1** instead of 2.
- Revised card and upgrade text to better explain Roderic, Vesper and several units.
- Minimum dependencies: **Trainworks Reloaded 0.7.26** and **Conductor 0.5.14**.
- Cards, units, champions, abilities, statuses, equipment, rooms, relics and dialogue are translated into **all ten game languages**. Includes Severance Pay’s dynamic damage text.

### 0.2.5 — 2026-09-25

Rebalanced Vesper’s **Beastmaster** path and the Two-Headed Troll.

- **Beastmaster**: trolls no longer grow through Celebrate after each battle. With Beastmaster Vesper in play, they now enter with **+10** attack and health **for each Ring after the first**.
- **Beastmaster II and III**: trolls gain **Trample** only; removed Sweep.
- **Beastmaster III**: *Call of the Wild* is ready **one turn earlier** (cooldown 4, starting at 3).
- **Two-Headed Troll**: loses Multistrike and gains **Wrath** (attack immediately, cooldown 3), usable on the turn it enters play.
- **Double Shift** now applies **Dizzy**, a new clan status with its own icon. At the start of your next turn, it becomes the same amount of Dazed.
- **Kit Bash** costs **1** instead of 0.
- **Invisibility** applies **Stealth 2** instead of 3.
- **Field Medic** heals and cleanses at the **start of combat** on its floor.
- **Severance Pay** is now Roderic’s starter card.
- New artwork for 16 cards.

### 0.2.4 — 2026-09-24

- The package keeps `json/` and `textures/` beside the DLL when installed through Gale and other BepInEx mod managers. Fixes the plugin loading without the clan appearing.
- Severance Pay displays its current Pyre damage in the description, with a single final period.
- Shift Aim clarifies that the Longbowman changes its target when it attacks; the ability does not cause an immediate attack.
- Excluded two abandoned card JSON files that the DLL did not load.

### 0.2.3 — 2026-09-23

Fixes following the first run with the new cards.

Thanks to **lostone**, **Brandon**, **fohnjarmery**, **Chéradénine** and the other modders in the Discord channel for testing and reporting issues.

- **Reporting bugs**: Thunderstore does not support comments on the package page. The README now points to GitHub issues and the Monster Train Discord modding channel.
- **Kit Bash** and **Severance Pay** no longer report *“No valid target”*.
- **Bounty Hunter**: *Claim the Bounty* no longer freezes the run when targeting another floor. Its ability now refreshes if it kills, as described. It is also Deployable and appears in the deployment hand.
- **Hedge Conjurer** is now Deployable too.
- **Chirurgeon**: fixed the same freeze between floors at all three levels.
- **Quartermaster’s Store**: the equipment discount works.
- **Double Shift** no longer removes too much when it ends.
- **Rope & Grapple**: reworked; no longer uses Haste, which only moves enemies.
- **Two new clan statuses, with their own icons**, lose one stack **each time the enemy changes floor**, rather than each round like Doom or Timebomb:
  - **Banishment** applies **Banished 2**. When the final stack is lost, the enemy is banished. Cannot target bosses. No longer uses Stasis, which made the enemy permanently untargetable.
  - **Delayed Blast Fireball** applies **Fuse 2**. When the final stack is lost, it deals **100 Explosive** damage. Can target bosses. Damage is calculated when the card is played, including upgrades.
- **Widow’s Pension** no longer summons a Free Lance: the replacement inherited the pension, creating an endless chain. It now grants gold when its holder dies: the card’s attack + health, including permanent upgrades and excluding equipment, rounded to 5.
- **Hedge-Mage**: reworked. Each spell grants **1 Arcane Charge**. Its new ability, **Arcane Volley**, deals **2** damage per charge to **3** random enemies on its floor and consumes the charges. Random damage previously triggered when playing a spell, making the preview disagree with the outcome.
- **Field Medic** heals and cleanses at the **start** of the turn instead of the end, for the same reason.
- **Roderic**: the three Quartermaster abilities are now named Requisition I, II and III.
- **Sapper’s Charges**: reworked. Previously damaged each enemy changing floor. Now deals **10** damage to the front enemy on a friendly unit’s new floor when that unit ascends.
- **Marching Orders**: the room copies on other floors can now be replaced later.
- **Letter of Marque** gave all six tier III equipment cards at the start of each battle. It now gives one at random, as described.
- Clan card rewards no longer display a white square on the loot screen.

### 0.2.2 — 2026-09-23

- **Credits**: the README now names Trainworks Reloaded and Conductor, by the Monster Train 2 Modding Group, as the frameworks the clan uses.
- Corrected the authorship statement: the original work is the cards, units, champions, relics and custom DLL effects. Other effects come from the base game and Conductor.

### 0.2.0 — 2026-09-23 — first public release (ALPHA)

First upload. The clan loads, can be selected and can be played from start to finish. It is released **without balancing**, with much of the content tested only in my own runs.

#### Content

- **Roderic** (`Melee` + `Warrior`), with the **Shield Wall**, **Quartermaster** and **Plunder** paths.
- **Vesper** (`Ranged` + `Wizard`), with the **Grimoire**, **Chirurgeon** and **Beastmaster** paths. Beastmaster includes five tameable trolls that stay in your deck.
- Banner and draft units, spells, six rooms, equipment split into tiers and artifacts.

#### Still missing, and why this is alpha

- Balance has not been measured: some cards have never been drafted.
- The three card frames still use StewardClan’s frames.

#### Feedback welcome

Bug reports and balance feedback through GitHub issues or the package page comments.

## Spanish

### 0.3.2 — 2026-10-27 — BETA

- **Battle Chaplain:** Last Rites usa ahora curación y selección de todos los pisos nativas, mejorando la compatibilidad y respetando las reglas normales de objetivos.
- **Fuse y Banished:** limpiar y volver a aplicar estos efectos ya no arrastra daño de la aplicación anterior.
- **Requisition:** el equipo aleatorio de unidades sin un rol compatible respeta ahora la semilla de batalla del juego.
- **Goblins:** aumentados ligeramente Battle Chaplain, Chapel Acolyte, Hedge Conjurer, Longbowman, Shieldbearer y The New Guy entre un 10 y un 20 %, conservando proporciones y apoyo de pies.

### 0.3.1 — 2026-10-26 — BETA

Gracias a **Rratvar, the Untitled** por los reportes sobre limpieza, visuales de campeones, Field Forge y Butcher's Bill, y por sus comentarios sobre Watchtower. [Errores y comentarios](https://discord.com/channels/336546996779483136/1563998046084268032).

- **Watchtower:** empieza activa. Su habilidad de sala alterna Francotirador y +10 ataque juntos, con recarga 1. El bono no se acumula y conserva Francotirador de otras fuentes. En salas fusionadas conserva los demás bonos.
- **Field Medic y Chirurgeon:** la limpieza aleatoria elige un aliado que tenga aflicciones visibles. Chirurgeon II/III mantienen el alcance entre pisos; las limpiezas sucesivas excluyen aliados ya limpiados.
- **Field Forge:** su descuento se aplica una sola vez al equipo recién fusionado, conservando otros modificadores de coste.
- **Roderic y Vesper:** ajustados los límites del personaje animado, anclajes VFX y sprite estático para corregir el destello gigante al desplegar y los efectos desplazados.
- **Longbowman:** recarga de Shift Aim de 1 a 2. Cambia temporalmente Francotirador por Pierce 1 de Conductor, que golpea a un enemigo adicional detrás del delantero. Conserva el Pierce concedido por equipos.
- **Butcher’s Bill:** el texto incluye las bajas de la Pira. Mínimo Trainworks Reloaded: **0.7.33**.
- **Problema conocido:** mezclar sendas de campeón puede reemplazar la habilidad activada anterior. Pendiente de rediseño.

### 0.3.0 — 2026-10-10 — BETA

- Primera versión **BETA**.
- **Starter de Vesper:** Cura de campaña cuesta 1 Ember, elimina una aflicción aleatoria del aliado elegido y cura 2. Arte nuevo. Proyectil mágico pasa a recompensas comunes.
- **Beastmaster:** más criaturas, patas Tamable/Tamed, Frozen y descuentos acumulables de Ember en mano. Command ordena atacar a una criatura Tamed elegida de cualquier piso, con recarga 4/3/2; sustituye el crecimiento de atributos. Corregidos el fallo en tiendas y la permanencia del troll elegido en el mazo.
- **Roderic:** eliminado Rally, estadísticas ajustadas y Titanskin retirado de Quartermaster y Plunder. Call to Arms permite elegir una unidad de mano, robo o descarte con coste base máximo 1/2/3; si no hay, recupera una muerta con 1 de vida y Undying 1. Lista al invocar, recarga 2. Corregida la validación de objetivos en preview.
- **Marching Orders:** reduce el coste Ember de la primera unidad de cada turno.
- Animaciones más fluidas, iconos del clan/estados y colocación de trolls mejorados. Corregidos Champion, localización de Severance Pay y textos de equipo/habilidades.

### 0.2.6 — 2026-09-29

- **Vesper y Roderic**: ilustraciones de carta y sprites de unidad animados. Las
  transiciones incluyen fotogramas intermedios; se ajustaron el tamaño y la proporción
  de Vesper en combate.
- **Chirurgeon**: cura y limpia al inicio del turno para que la elección aleatoria no
  altere la vista previa de combate. También puede eliminar **Dizzy**.
- Las cartas del clan tienen efectos visuales y sonidos acordes con su acción.
- **Delayed Blast Fireball** cuesta **2** Ember en vez de 3 y pasa de rara a infrecuente.
  **Haste** aplica **Burst 1** en vez de 2.
- Textos de cartas y mejoras revisados para explicar mejor los efectos de Roderic,
  Vesper y varias unidades.
- Dependencias mínimas: **Trainworks Reloaded 0.7.26** y **Conductor 0.5.14**.
- Traducción a los **diez idiomas del juego** de cartas, unidades, campeones, habilidades, estados, equipo, salas, reliquias y diálogos. Incluye el daño dinámico de Severance Pay.

### 0.2.5 — 2026-09-25

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

### 0.2.4 — 2026-09-24

- El paquete conserva `json/` y `textures/` junto al DLL al instalarse con Gale y otros
  gestores de mods BepInEx. Corrige el caso en que el plugin cargaba, pero el clan no aparecia.
- Severance Pay muestra el dano actual de la Pyre en la descripcion, con un solo punto final.
- Shift Aim aclara que el Longbowman cambiara de objetivo cuando ataque; la habilidad no
  realiza un ataque inmediato.
- Se excluyen dos JSON de cartas descartadas que el DLL no cargaba.

### 0.2.3 — 2026-09-23

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

### 0.2.2 — 2026-09-23

- **Créditos**: el README nombra ahora a Trainworks Reloaded y Conductor, del Monster Train
  2 Modding Group, sobre los que funciona el clan.
- Corregida la frase de autoría: lo propio son las cartas, unidades, campeones y reliquias
  y los efectos a medida del DLL; el resto de efectos son del juego base y de Conductor.

### 0.2.0 — 2026-09-23 — primera versión pública (ALPHA)

Primera subida. El clan carga, se selecciona y se juega de principio a fin, pero se publica
**sin balancear** y con buena parte del contenido probado solo en mis propias partidas.

#### Contenido

- **Roderic** (`Melee`+`Warrior`), con las sendas **Shield Wall**, **Quartermaster** y
  **Plunder**.
- **Vesper** (`Ranged`+`Wizard`), con las sendas **Grimoire**, **Chirurgeon** y
  **Beastmaster**, esta última con cinco trolls domables que se quedan en el mazo.
- Unidades de estandarte y de draft, hechizos, seis salas, equipo repartido en escaleras y
  artefactos.

#### Lo que falta, y por eso es alpha

- El equilibrio está sin medir: hay cartas que no se han drafteado nunca.
- Los tres marcos de carta son todavía los de StewardClan.

#### Se agradece

Informes de fallos y opiniones de equilibrio, en las incidencias de GitHub o en los
comentarios de la ficha.

## French

### 0.3.2 — 2026-10-27 — BETA

- **Battle Chaplain :** Last Rites utilise désormais les soins et le ciblage de tous les étages natifs, pour une meilleure compatibilité et le respect des règles de ciblage du jeu.
- **Fuse et Banished :** purifier puis réappliquer ces effets ne conserve plus les dégâts de l'application précédente.
- **Requisition :** l'équipement aléatoire des unités sans rôle correspondant suit désormais la graine de combat du jeu.
- **Gobelins :** Battle Chaplain, Chapel Acolyte, Hedge Conjurer, Longbowman, Shieldbearer et The New Guy sont agrandis de 10 à 20 %, en conservant leurs proportions et la position de leurs pieds.

### 0.3.1 — 2026-10-26 — BETA

Merci à **Rratvar, the Untitled** pour les signalements concernant la purification, les visuels des champions, Field Forge et Butcher's Bill, ainsi que les retours sur Watchtower. [Bugs et retours](https://discord.com/channels/336546996779483136/1563998046084268032).

- Watchtower démarre active : capacité de salle, recharge 1, alternant Tireur d’élite et +10 attaque sans cumul. Préserve Tireur d’élite d’autres sources. Les salles fusionnées conservent leurs autres bonus.
- Field Medic et Chirurgeon choisissent uniquement des alliés avec un malus visible. Chirurgeon II/III conservent la portée entre étages et évitent les alliés déjà purifiés.
- Field Forge applique une seule réduction au nouvel équipement fusionné, en conservant les autres modificateurs.
- Ajustements visuels de Roderic/Vesper : apparition surdimensionnée et effets décalés. Longbowman : recharge 2, échange temporairement Sniper contre Pierce 1. Butcher’s Bill mentionne le Brasier. Trainworks minimum **0.7.33**. Le Pierce des équipements est conservé.
- Problème connu : mélanger les voies du champion peut remplacer la capacité précédente.

### 0.3.0 — 2026-10-10 — BETA

- Première version **BETA**.
- Nouveau sort initial de Vesper : Field Dressing coûte 1 Ember, retire un malus aléatoire à un allié choisi et rend 2 PV. Nouvelle illustration. Magic Missile devient une récompense commune.
- Beastmaster : plus de créatures, icônes Tamable/Tamed, Frozen et réductions cumulatives d'Ember en main. Command fait attaquer une créature Tamed choisie à n'importe quel étage ; recharge 4/3/2, remplace la croissance des statistiques. Corrections du magasin et du Troll permanent.
- Roderic : Rally supprimé, statistiques ajustées ; Titanskin retiré de Quartermaster et Plunder. Call to Arms choisit une unité de la main, pioche ou défausse (coût de base maximal 1/2/3), sinon une unité morte avec 1 PV et Undying 1. Disponible à l'invocation, recharge 2.
- Marching Orders réduit le coût de la première unité par tour. Animations, icônes, placement des Trolls et textes améliorés ; corrections de Champion et Severance Pay.

Français

### 0.2.6 — 2026-09-29

- Vesper et Roderic : illustrations de cartes et sprites d’unités animés. Ajout d’images intermédiaires et ajustement de la taille et des proportions de Vesper en combat.
- Chirurgeon soigne et purifie au début du tour afin que le choix aléatoire ne modifie pas l’aperçu du combat. Peut aussi retirer Dizzy.
- Les cartes du clan ont désormais des effets visuels et des sons adaptés à leurs actions.
- Delayed Blast Fireball coûte 2 Ember au lieu de 3 et passe de Rare à Peu commun. Haste applique Burst 1 au lieu de 2.
- Textes des cartes et améliorations révisés pour mieux expliquer Roderic, Vesper et plusieurs unités.
- Dépendances minimales : Trainworks Reloaded 0.7.26 et Conductor 0.5.14.
- Cartes, unités, champions, capacités, états, équipements, salles, reliques et dialogues traduits dans les dix langues du jeu. Inclut le texte dynamique des dégâts de Severance Pay.

## German

### 0.3.2 — 2026-10-27 — BETA

- **Battle Chaplain:** Last Rites verwendet jetzt die native Heilung und Zielauswahl über alle Stockwerke, für bessere Kompatibilität und die üblichen Zielregeln des Spiels.
- **Fuse und Banished:** Nach dem Entfernen und erneuten Anwenden wird kein Schaden der vorherigen Anwendung mehr übernommen.
- **Requisition:** Zufällige Ausrüstung für Einheiten ohne passende Rolle folgt jetzt dem Kampf-Zufallsseed des Spiels.
- **Goblins:** Battle Chaplain, Chapel Acolyte, Hedge Conjurer, Longbowman, Shieldbearer und The New Guy sind um 10–20 % größer; Proportionen und Fußposition bleiben erhalten.

### 0.3.1 — 2026-10-26 — BETA

Danke an **Rratvar, the Untitled** für die Fehlerberichte zu Reinigung, Champion-Darstellung, Field Forge und Butcher's Bill sowie das Feedback zu Watchtower. [Fehler und Feedback](https://discord.com/channels/336546996779483136/1563998046084268032).

- Watchtower startet aktiv: Raumfähigkeit mit Abklingzeit 1 schaltet Scharfschütze und +10 Angriff gemeinsam um. Kein Stapeln; andere Scharfschützen-Quellen bleiben erhalten. Verschmolzene Räume behalten ihre anderen Boni.
- Field Medic und Chirurgeon wählen nur Verbündete mit sichtbaren negativen Effekten. Chirurgeon II/III behalten die etagenübergreifende Reichweite und überspringen bereits gereinigte Einheiten.
- Field Forge verbilligt neu verschmolzene Ausrüstung nur einmal; andere Kostenmodifikatoren bleiben erhalten.
- Roderic/Vesper: Anpassungen gegen riesige Einsatzbilder und versetzte VFX. Longbowman: Abklingzeit 2, zeitweiser Tausch von Sniper gegen Pierce 1. Butcher’s Bill erwähnt Höllenglut-Tötungen. Trainworks mindestens **0.7.33**. Pierce aus Ausrüstung bleibt erhalten.
- Bekanntes Problem: Gemischte Championpfade können die vorherige Fähigkeit ersetzen.

### 0.3.0 — 2026-10-10 — BETA

- Erste **BETA**-Version.
- Vespers neue Startkarte Field Dressing kostet 1 Ember, entfernt einen zufälligen negativen Effekt vom gewählten Verbündeten und heilt 2 Leben. Neue Illustration. Magic Missile ist nun eine gewöhnliche Belohnung.
- Beastmaster: mehr Kreaturen, Tamable/Tamed-Symbole, Frozen und kumulative Ember-Rabatte auf der Hand. Command lässt eine gewählte Tamed-Kreatur auf beliebiger Etage angreifen; Abklingzeit 4/3/2, ersetzt Attributwachstum. Händlerfehler und dauerhafter Troll korrigiert.
- Roderic: Rally entfernt, Werte angepasst; kein Titanskin für Quartermaster und Plunder. Call to Arms wählt aus Hand, Zieh- oder Ablagestapel (Basiskosten bis 1/2/3), sonst eine gefallene Einheit mit 1 Leben und Undying 1. Sofort bereit, Abklingzeit 2.
- Marching Orders verbilligt die erste Einheit pro Zug. Animationen, Symbole, Trollpositionen und Texte verbessert; Champion und Severance Pay korrigiert.

Deutsch

### 0.2.6 — 2026-09-29

- Vesper und Roderic: animierte Kartenillustrationen und Einheitensprites. Zwischenbilder ergänzt sowie Vespers Größe und Proportionen im Kampf angepasst.
- Chirurgeon heilt und entfernt Schwächungen zu Zugbeginn, damit die zufällige Auswahl die Kampfvorschau nicht verändert. Kann auch Dizzy entfernen.
- Die Karten des Clans haben jetzt zu ihren Aktionen passende visuelle Effekte und Sounds.
- Delayed Blast Fireball kostet 2 statt 3 Ember und ist jetzt Ungewöhnlich statt Selten. Haste verleiht Burst 1 statt 2.
- Karten- und Verbesserungstexte überarbeitet, um Roderic, Vesper und mehrere Einheiten besser zu erklären.
- Mindestabhängigkeiten: Trainworks Reloaded 0.7.26 und Conductor 0.5.14.
- Karten, Einheiten, Champions, Fähigkeiten, Statuseffekte, Ausrüstung, Räume, Relikte und Dialoge in alle zehn Spielsprachen übersetzt. Einschließlich des dynamischen Schadenstexts von Severance Pay.

## Russian

### 0.3.2 — 2026-10-27 — BETA

- **Battle Chaplain:** Last Rites теперь использует встроенное лечение и выбор целей на всех этажах, улучшая совместимость и соблюдая обычные правила выбора целей.
- **Fuse и Banished:** после очищения и повторного применения больше не сохраняется урон от предыдущего эффекта.
- **Requisition:** случайное снаряжение для бойцов без подходящей роли теперь зависит от зерна боевой случайности игры.
- **Гоблины:** Battle Chaplain, Chapel Acolyte, Hedge Conjurer, Longbowman, Shieldbearer и The New Guy увеличены на 10–20 % с сохранением пропорций и положения ног.

### 0.3.1 — 2026-10-26 — BETA

Спасибо **Rratvar, the Untitled** за сообщения о проблемах очищения, визуальных эффектов чемпионов, Field Forge и Butcher's Bill, а также отзывы о Watchtower. [Ошибки и отзывы](https://discord.com/channels/336546996779483136/1563998046084268032).

- Watchtower изначально активна: способность комнаты с перезарядкой 1 одновременно переключает Снайпер и +10 атаки без накопления. Сохраняет Снайпер из других источников. Объединённые комнаты сохраняют остальные бонусы.
- Field Medic и Chirurgeon выбирают только союзников с видимым отрицательным эффектом. Chirurgeon II/III сохраняют дальность между этажами и пропускают уже очищенных союзников.
- Field Forge применяет скидку к новому объединённому снаряжению только один раз, сохраняя другие изменения цены.
- Визуальные исправления Roderic/Vesper: огромная фигура при размещении и смещение VFX. Longbowman: перезарядка 2, временная замена Sniper на Pierce 1. Butcher’s Bill упоминает убийства Пеклом. Trainworks минимум **0.7.33**. Pierce от снаряжения сохраняется.
- Известная проблема: смешение путей чемпиона может заменить прежнюю способность.

### 0.3.0 — 2026-10-10 — BETA

- Первая версия **BETA**.
- Новый стартовый Field Dressing Веспер: 1 Ember, снимает случайный отрицательный эффект с выбранного союзника и лечит 2 здоровья. Новая иллюстрация. Magic Missile доступен как обычная награда.
- Beastmaster: больше существ, значки Tamable/Tamed, Frozen и накопительные скидки Ember в руке. Command заставляет выбранное Tamed-существо атаковать на любом этаже; перезарядка 4/3/2 вместо роста характеристик. Исправлены магазин и сохранение тролля в колоде.
- Roderic: удалён Rally, изменены характеристики; Titanskin убран у Quartermaster и Plunder. Call to Arms выбирает из руки, колоды или сброса (базовая цена до 1/2/3), иначе возвращает погибшего с 1 здоровья и Undying 1. Сразу готово, перезарядка 2.
- Marching Orders удешевляет первую боевую единицу каждого хода. Улучшены анимации, значки, положение троллей и тексты; исправлены Champion и Severance Pay.

Русский

### 0.2.6 — 2026-09-29

- Vesper и Roderic: анимированные иллюстрации карт и спрайты боевых единиц. Добавлены промежуточные кадры; скорректированы размер и пропорции Vesper в бою.
- Chirurgeon лечит и снимает ослабления в начале хода, чтобы случайный выбор не изменял предпросмотр боя. Также может снимать Dizzy.
- Карты клана получили визуальные и звуковые эффекты, соответствующие их действиям.
- Delayed Blast Fireball стоит 2 Ember вместо 3 и меняет редкость с редкой на необычную. Haste накладывает Burst 1 вместо 2.
- Переработаны тексты карт и улучшений: они лучше объясняют действия Roderic, Vesper и нескольких боевых единиц.
- Минимальные зависимости: Trainworks Reloaded 0.7.26 и Conductor 0.5.14.
- Карты, боевые единицы, чемпионы, способности, состояния, снаряжение, помещения, реликвии и диалоги переведены на все десять языков игры. Включая динамический текст урона Severance Pay.

## Portuguese Brazil

### 0.3.2 — 2026-10-27 — BETA

- **Battle Chaplain:** Last Rites agora usa cura e seleção de alvos em todos os andares nativas, melhorando a compatibilidade e respeitando as regras normais do jogo.
- **Fuse e Banished:** remover e reaplicar esses efeitos não conserva mais o dano da aplicação anterior.
- **Requisition:** equipamentos aleatórios para unidades sem uma função correspondente agora seguem a semente de batalha do jogo.
- **Goblins:** Battle Chaplain, Chapel Acolyte, Hedge Conjurer, Longbowman, Shieldbearer e The New Guy aumentaram de 10 a 20 %, mantendo as proporções e a posição dos pés.

### 0.3.1 — 2026-10-26 — BETA

Obrigado a **Rratvar, the Untitled** pelos relatos sobre limpeza, visuais dos campeões, Field Forge e Butcher's Bill, e pelos comentários sobre Watchtower. [Erros e comentários](https://discord.com/channels/336546996779483136/1563998046084268032).

- Watchtower começa ativa: habilidade de sala com recarga 1 alterna Atirador de Elite e +10 de ataque juntos, sem acumular. Preserva o efeito de outras fontes. Salas fundidas preservam os outros bônus.
- Field Medic e Chirurgeon escolhem apenas aliados com efeitos negativos visíveis. Chirurgeon II/III mantêm alcance entre andares e ignoram aliados já purificados.
- Field Forge aplica o desconto uma vez ao equipamento recém-fundido, preservando outros modificadores.
- Ajustes visuais de Roderic/Vesper para aparição gigante e VFX deslocados. Longbowman: recarga 2, troca temporária de Sniper por Pierce 1. Butcher’s Bill menciona abates da Pira. Trainworks mínimo **0.7.33**. Preserva Pierce concedido por equipamentos.
- Problema conhecido: misturar caminhos de campeão pode substituir a habilidade anterior.

### 0.3.0 — 2026-10-10 — BETA

- Primeira versão **BETA**.
- Novo inicial de Vesper: Field Dressing custa 1 Ember, remove um efeito negativo aleatório do aliado escolhido e cura 2. Nova arte. Magic Missile vira recompensa comum.
- Beastmaster: mais criaturas, ícones Tamable/Tamed, Frozen e descontos cumulativos de Ember na mão. Command faz uma criatura Tamed escolhida atacar em qualquer andar; recarga 4/3/2, substituindo o crescimento de atributos. Corrigidos loja e Troll permanente.
- Roderic: Rally removido, atributos ajustados; Titanskin removido de Quartermaster e Plunder. Call to Arms escolhe da mão, compra ou descarte (custo base até 1/2/3), ou revive um morto com 1 de vida e Undying 1. Pronto ao invocar, recarga 2.
- Marching Orders reduz o custo da primeira unidade por turno. Animações, ícones, posição dos Trolls e textos melhorados; correções de Champion e Severance Pay.

Português (Brasil)

### 0.2.6 — 2026-09-29

- Vesper e Roderic: artes de cartas e sprites de unidades animados. Adicionados quadros intermediários e ajustados o tamanho e as proporções de Vesper em combate.
- Chirurgeon cura e purifica no início do turno para que a escolha aleatória não altere a prévia do combate. Também pode remover Dizzy.
- As cartas do clã agora têm efeitos visuais e sons adequados às suas ações.
- Delayed Blast Fireball custa 2 Ember em vez de 3 e passa de Rara para Incomum. Haste aplica Burst 1 em vez de 2.
- Textos de cartas e melhorias revisados para explicar melhor Roderic, Vesper e várias unidades.
- Dependências mínimas: Trainworks Reloaded 0.7.26 e Conductor 0.5.14.
- Cartas, unidades, campeões, habilidades, estados, equipamentos, salas, relíquias e diálogos traduzidos para os dez idiomas do jogo. Inclui o texto de dano dinâmico de Severance Pay.

## Chinese

### 0.3.2 — 2026-10-27 — BETA

- **Battle Chaplain：**Last Rites 现使用游戏原生治疗效果和全楼层目标选择，提高兼容性并遵循游戏的正常目标规则。
- **Fuse 和 Banished：**净化后重新施加这些效果，不再保留上一次施加时的待结算伤害。
- **Requisition：**为没有匹配职业的单位随机生成装备时，现在使用游戏的战斗随机种子。
- **哥布林外观：**Battle Chaplain、Chapel Acolyte、Hedge Conjurer、Longbowman、Shieldbearer 和 The New Guy 增大了 10–20%，保持原有比例和脚部位置。

### 0.3.1 — 2026-10-26 — BETA

感谢 **Rratvar, the Untitled** 报告净化、英雄视觉效果、Field Forge 和 Butcher's Bill 的问题，并对 Watchtower 提出反馈。[错误报告与反馈](https://discord.com/channels/336546996779483136/1563998046084268032)。

- Watchtower初始激活。房间技能冷却1，同时切换狙击与+10攻击；不叠加并保留其他来源的狙击。 融合房间保留其他加成。
- Field Medic和Chirurgeon只从带有可见减益的友军中随机选择。Chirurgeon II/III保留跨层范围，连续净化跳过已净化目标。
- Field Forge对新融合装备只应用一次减费，保留其他费用修正。
- 调整Roderic/Vesper部署时的巨大闪现及VFX位置。Longbowman冷却改为2，暂时以Pierce 1替换Sniper。Butcher’s Bill文本包含薪火击杀。Trainworks最低版本**0.7.33**。 保留装备赋予的Pierce。
- 已知问题：混合冠军路线可能覆盖原有主动技能。

### 0.3.0 — 2026-10-10 — BETA

- 首个 **BETA** 版本。
- Vesper的新初始法术Field Dressing：费用1 Ember，移除所选友军的一种随机减益并恢复2生命。新插画。Magic Missile成为普通奖励法术。
- Beastmaster：更多生物、Tamable/Tamed爪印、Frozen及手牌累积Ember减费。Command令任意楼层选定的Tamed生物立即攻击，冷却4/3/2，替代属性成长。修复商店错误及永久巨魔。
- Roderic：移除Rally，调整属性，Quartermaster和Plunder移除Titanskin。Call to Arms从手牌、抽牌或弃牌堆选择基础费用不超过1/2/3的单位；无目标时复活阵亡单位，获得1生命和Undying 1。登场可用，冷却2。
- Marching Orders降低每回合首个单位的费用。改善动画、图标、巨魔位置和文本，修复Champion及Severance Pay。

简体中文

### 0.2.6 — 2026-09-29

- Vesper与Roderic：卡牌插画和单位精灵现已具有动画。添加了过渡帧，并调整了Vesper在战斗中的尺寸与比例。
- Chirurgeon在回合开始时治疗和净化，避免随机选择影响战斗预览。也可以移除Dizzy。
- 佣兵团卡牌现已具有与其动作相符的视觉和音效。
- Delayed Blast Fireball的费用由3 Ember降为2，稀有度由稀有改为罕见。Haste施加Burst 1，不再是2。
- 修订卡牌与升级的说明，更清楚地解释Roderic、Vesper及多个单位的效果。
- 最低依赖版本：Trainworks Reloaded 0.7.26与Conductor 0.5.14。
- 卡牌、单位、勇者、技能、状态、装备、房间、遗物和对白已翻译为游戏的全部十种语言，包括Severance Pay的动态伤害文本。

## Chinese Traditional

### 0.3.2 — 2026-10-27 — BETA

- **Battle Chaplain：**Last Rites 現使用遊戲原生治療效果及全樓層目標選擇，提高相容性並遵循遊戲的正常目標規則。
- **Fuse 和 Banished：**淨化後重新施加這些效果，不再保留上一次施加時的待結算傷害。
- **Requisition：**為沒有匹配職業的單位隨機產生裝備時，現在使用遊戲的戰鬥隨機種子。
- **哥布林外觀：**Battle Chaplain、Chapel Acolyte、Hedge Conjurer、Longbowman、Shieldbearer 和 The New Guy 增大了 10–20%，保持原有比例及腳部位置。

### 0.3.1 — 2026-10-26 — BETA

感謝 **Rratvar, the Untitled** 回報淨化、英雄視覺效果、Field Forge 和 Butcher's Bill 的問題，並對 Watchtower 提出意見。[錯誤回報與意見](https://discord.com/channels/336546996779483136/1563998046084268032)。

- Watchtower初始啟用。房間技能冷卻1，同時切換狙擊與+10攻擊；不疊加並保留其他來源的狙擊。 融合房間保留其他加成。
- Field Medic和Chirurgeon只從帶有可見減益的友軍中隨機選擇。Chirurgeon II/III保留跨層範圍，連續淨化跳過已淨化目標。
- Field Forge對新融合裝備只套用一次減費，保留其他費用修正。
- 調整Roderic/Vesper部署時的巨大閃現及VFX位置。Longbowman冷卻改為2，暫時以Pierce 1取代Sniper。Butcher’s Bill文字包含薪火擊殺。Trainworks最低版本**0.7.33**。 保留裝備賦予的Pierce。
- 已知問題：混合冠軍路線可能覆蓋原有主動技能。

### 0.3.0 — 2026-10-10 — BETA

- 首個 **BETA** 版本。
- Vesper的新初始法術Field Dressing：費用1 Ember，移除所選友軍的一種隨機減益並恢復2生命。新插畫。Magic Missile成為普通獎勵法術。
- Beastmaster：更多生物、Tamable/Tamed爪印、Frozen及手牌累積Ember減費。Command令任意樓層選定的Tamed生物立即攻擊，冷卻4/3/2，取代屬性成長。修復商店錯誤及永久巨魔。
- Roderic：移除Rally，調整屬性，Quartermaster和Plunder移除Titanskin。Call to Arms從手牌、抽牌或棄牌堆選擇基礎費用不超過1/2/3的單位；無目標時復活陣亡單位，獲得1生命和Undying 1。登場可用，冷卻2。
- Marching Orders降低每回合首個單位的費用。改善動畫、圖示、巨魔位置和文字，修復Champion及Severance Pay。

繁體中文

### 0.2.6 — 2026-09-29

- Vesper與Roderic：卡牌插畫和單位精靈現已具有動畫。加入了過渡幀，並調整了Vesper在戰鬥中的尺寸與比例。
- Chirurgeon在回合開始時治療和淨化，避免隨機選擇影響戰鬥預覽。也可以移除Dizzy。
- 傭兵團卡牌現已具有與其動作相符的視覺和音效。
- Delayed Blast Fireball的費用由3 Ember降為2，稀有度由稀有改為罕見。Haste施加Burst 1，不再是2。
- 修訂卡牌與升級的說明，更清楚地解釋Roderic、Vesper及多個單位的效果。
- 最低依賴版本：Trainworks Reloaded 0.7.26與Conductor 0.5.14。
- 卡牌、單位、勇者、技能、狀態、裝備、房間、遺物和對白已翻譯為遊戲的全部十種語言，包括Severance Pay的動態傷害文字。

## Korean

### 0.3.2 — 2026-10-27 — BETA

- **Battle Chaplain:** Last Rites가 게임 기본 회복 효과와 모든 층 대상 선택을 사용하여 호환성을 높이고 기본 대상 지정 규칙을 따릅니다.
- **Fuse 및 Banished:** 정화 후 다시 적용할 때 이전 효과의 예정 피해가 남지 않도록 수정했습니다.
- **Requisition:** 일치하는 역할이 없는 유닛의 무작위 장비가 이제 게임의 전투 난수 시드를 따릅니다.
- **고블린 외형:** Battle Chaplain, Chapel Acolyte, Hedge Conjurer, Longbowman, Shieldbearer 및 The New Guy를 10–20% 확대하고 신체 비율과 발 위치를 유지했습니다.

### 0.3.1 — 2026-10-26 — BETA

정화, 챔피언 시각 효과, Field Forge 및 Butcher's Bill 문제를 제보하고 Watchtower에 의견을 주신 **Rratvar, the Untitled**께 감사드립니다. [버그 제보 및 의견](https://discord.com/channels/336546996779483136/1563998046084268032).

- Watchtower는 활성 상태로 시작. 방 능력 대기시간 1로 저격과 공격력 +10을 함께 전환. 중첩되지 않으며 다른 출처의 저격은 유지. 융합한 방의 다른 보너스 유지.
- Field Medic과 Chirurgeon은 보이는 약화 효과가 있는 아군만 무작위로 선택. Chirurgeon II/III는 층 간 범위를 유지하며 이미 정화한 아군 제외.
- Field Forge는 새 융합 장비에 할인 한 번만 적용하고 다른 비용 변경 유지.
- Roderic/Vesper의 배치 순간 거대한 모습과 VFX 위치 조정. Longbowman: 대기시간 2, Sniper를 일시적으로 Pierce 1로 교체. Butcher’s Bill 설명에 화로 처치 포함. Trainworks 최소 **0.7.33**. 장비가 부여한 Pierce는 유지.
- 알려진 문제: 챔피언 경로를 혼합하면 이전 활성 능력이 교체될 수 있음.

### 0.3.0 — 2026-10-10 — BETA

- 첫 **BETA** 버전.
- Vesper의 새 시작 주문 Field Dressing: 1 Ember, 선택한 아군의 무작위 약화 효과 하나를 제거하고 체력 2 회복. 새 그림. Magic Missile은 일반 보상 주문으로 변경.
- Beastmaster: 생물 목록 확대, Tamable/Tamed 아이콘, Frozen과 손패의 누적 Ember 할인. Command는 어느 층이든 선택한 Tamed 생물을 즉시 공격시킴. 재사용 대기시간 4/3/2이며 능력치 성장을 대체. 상점 오류와 영구 트롤 수정.
- Roderic: Rally 제거, 능력치 조정, Quartermaster와 Plunder의 Titanskin 제거. Call to Arms는 손패·뽑기·버린 더미에서 기본 비용 1/2/3 이하의 유닛 선택. 없으면 죽은 유닛을 체력 1과 Undying 1로 소환. 소환 즉시 준비, 대기시간 2.
- Marching Orders는 매 턴 첫 유닛 비용 감소. 애니메이션, 아이콘, 트롤 위치와 설명 개선. Champion과 Severance Pay 수정.

한국어

### 0.2.6 — 2026-09-29

- Vesper와 Roderic: 카드 일러스트와 유닛 스프라이트에 애니메이션 추가. 중간 프레임을 추가하고 전투 중 Vesper의 크기와 비율을 조정했습니다.
- Chirurgeon이 턴 시작 시 치유와 정화를 수행하여 무작위 선택이 전투 미리보기에 영향을 주지 않게 했습니다. Dizzy도 제거할 수 있습니다.
- 용병단 카드에 행동에 맞는 시각 효과와 소리를 추가했습니다.
- Delayed Blast Fireball의 비용이 3 Ember에서 2로 줄고 희귀도가 희귀에서 고급으로 변경되었습니다. Haste는 Burst 2 대신 Burst 1을 부여합니다.
- Roderic, Vesper 및 여러 유닛의 효과를 더 명확하게 설명하도록 카드와 업그레이드 문구를 수정했습니다.
- 최소 의존성 버전: Trainworks Reloaded 0.7.26 및 Conductor 0.5.14.
- 카드, 유닛, 챔피언, 능력, 상태, 장비, 방, 유물 및 대사를 게임의 10개 언어로 번역했습니다. Severance Pay의 동적 피해 문구도 포함됩니다.

## Japanese

### 0.3.2 — 2026-10-27 — BETA

- **Battle Chaplain：**Last Rites がゲーム標準の回復効果と全階層の対象選択を使用するようになり、互換性が向上し、通常の対象選択ルールに従います。
- **Fuse と Banished：**浄化後に再付与しても、前回の付与による保留ダメージが残らないよう修正しました。
- **Requisition：**対応する役割がないユニットへのランダム装備が、ゲームの戦闘用乱数シードに従うようになりました。
- **ゴブリンの表示：**Battle Chaplain、Chapel Acolyte、Hedge Conjurer、Longbowman、Shieldbearer、The New Guy を10～20%拡大し、体の比率と足の位置を維持しました。

### 0.3.1 — 2026-10-26 — BETA

浄化、チャンピオンの表示、Field Forge、Butcher's Bill の問題報告と、Watchtower へのご意見をくださった **Rratvar, the Untitled** に感謝します。[不具合報告・ご意見](https://discord.com/channels/336546996779483136/1563998046084268032)。

- Watchtowerは有効状態で開始。部屋能力のクールダウン1で狙撃と攻撃力+10を同時に切り替え。重複せず、他の効果による狙撃を保持。 融合した部屋の他のボーナスを保持。
- Field MedicとChirurgeonは可視の弱体効果を持つ味方のみから選択。Chirurgeon II/IIIは階をまたぐ範囲を維持し、連続浄化では浄化済みの味方を除外。
- Field Forgeは新しい融合装備に軽減を1回だけ適用し、他のコスト補正を保持。
- Roderic/Vesperの配置直後の巨大表示とVFX位置を調整。Longbowmanはクールダウン2、Sniperを一時的にPierce 1へ変更。Butcher’s Billに炉の撃破を明記。Trainworks最低**0.7.33**。 装備由来のPierceを保持。
- 既知の問題：チャンピオン経路の混合で以前の能力が置き換わる場合があります。

### 0.3.0 — 2026-10-10 — BETA

- 初の **BETA** 版。
- Vesperの新しい初期呪文Field Dressing：1 Emberで選んだ味方の弱体効果をランダムに1種類解除し、体力を2回復。新イラスト。Magic Missileはコモン報酬に変更。
- Beastmaster：対象生物を拡大、Tamable/Tamedアイコン、Frozenと手札の累積Ember軽減。Commandは任意の階の選んだTamed生物を即座に攻撃させる。クールダウン4/3/2で能力値成長を置換。ショップ不具合と永続トロールを修正。
- Roderic：Rallyを削除、能力値調整、QuartermasterとPlunderのTitanskinを削除。Call to Armsは手札・山札・捨て札から基本コスト1/2/3以下を選択。対象がなければ死亡ユニットを体力1とUndying 1で召喚。登場時使用可能、クールダウン2。
- Marching Ordersは各ターン最初のユニットを軽減。アニメーション、アイコン、トロール位置と説明を改善。ChampionとSeverance Payを修正。

日本語

### 0.2.6 — 2026-09-29

- VesperとRoderic：カードイラストとユニットスプライトにアニメーションを追加。中間フレームを追加し、戦闘中のVesperの大きさと縦横比を調整しました。
- Chirurgeonはターン開始時に回復と浄化を行い、ランダムな対象選択が戦闘プレビューに影響しないようになりました。Dizzyも除去できます。
- 傭兵団のカードに、それぞれの動作に合う視覚効果と効果音を追加しました。
- Delayed Blast Fireballのコストを3 Emberから2に変更し、レアからアンコモンになりました。HasteはBurst 2ではなくBurst 1を付与します。
- Roderic、Vesperと複数のユニットの効果をより明確に説明するよう、カードと強化の文章を修正しました。
- 必要な最低バージョン：Trainworks Reloaded 0.7.26とConductor 0.5.14。
- カード、ユニット、チャンピオン、アビリティ、状態、装備、部屋、遺物、台詞をゲームの全10言語に翻訳しました。Severance Payの動的ダメージ表示も含みます。

<!-- 2026-09-25-2019||claude-mt2-the-free-company2-vesper-beastmaster||CHANGELOG.md||entrada 0.2.5: Beastmaster por anillo, fuera Celebrate y Sweep, Call of the Wild III antes, Two-Headed Troll con Wrath, Double Shift con Dizzy, Kit Bash 1, Field Medic, Severance Pay inicial, 16 artes -->

<!-- 2026-09-25-2040||david||CHANGELOG.md||entrada 0.2.5: linea nueva de Invisibility (Stealth 2 en lugar de 3) -->

<!-- 2026-09-29-2349||codex-freecompany-fx||CHANGELOG.md||añade entrada 0.2.6 con animaciones, Chirurgeon, FX, balance y dependencias -->

<!-- 2026-09-30-0103||codex-freecompany-fx||CHANGELOG.md||añade enlace inglés al inicio, historial inglés/español y notas 0.2.6 en diez idiomas con localización -->

<!-- 2026-10-10-2256||codex-freecompany-fx||C:\Users\david\AppData\Roaming\Thunderstore Mod Manager\DataFolder\MonsterTrain2\profiles\Default\BepInEx\plugins\David-FreeCompany\CHANGELOG.md||añade changelog 0.3.0 BETA en los diez idiomas manteniendo enlace inglés superior -->

<!-- 2026-10-26-2156||codex-freecompany-fx||C:\Users\david\AppData\Roaming\Thunderstore Mod Manager\DataFolder\MonsterTrain2\profiles\Default\BepInEx\plugins\David-FreeCompany\CHANGELOG.md||changelog 0.3.1 en diez idiomas, pruebas no jugadas y mezcla de sendas pendiente explícitas -->

<!-- 2026-10-26-2208||codex-freecompany-fx||CHANGELOG.md||revisión previa 0.3.1: conservación Pierce de equipos y 118 comprobaciones en diez idiomas -->

<!-- 2026-10-26-2212||codex-freecompany-fx||CHANGELOG.md||revisión 0.3.1: conserva otras mejoras de salas fusionadas y total 122 comprobaciones en diez idiomas -->

<!-- 2026-10-26-2222||codex-freecompany-fx||C:\Users\david\AppData\Roaming\Thunderstore Mod Manager\DataFolder\MonsterTrain2\profiles\Default\BepInEx\plugins\David-FreeCompany\CHANGELOG.md||añade agradecimientos y enlace al subhilo de bugs en los diez idiomas de 0.3.1 -->

<!-- 2026-10-26-2223||codex-freecompany-fx||CHANGELOG.md||limita agradecimientos 0.3.1 a aportaciones concretas de Rratvar; agradecimiento general permanece en README -->

<!-- 2026-10-27-2053||codex-freecompany-fx||CHANGELOG.md||añade 0.3.2 BETA en diez idiomas: curación nativa, Countdown, RNG y seis goblins; retira líneas de pruebas del texto público -->
