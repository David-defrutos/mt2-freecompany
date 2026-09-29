# Changelog

[English changelog](#english) · [Español](#spanish) · [Français](#french) · [Deutsch](#german) · [Русский](#russian) · [Português (Brasil)](#portuguese-brazil) · [简体中文](#chinese) · [繁體中文](#chinese-traditional) · [한국어](#korean) · [日本語](#japanese)

Latest release in all ten game languages. Full history in English and Spanish.

## English

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
