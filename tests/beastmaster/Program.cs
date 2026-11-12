
using mt2_freecompany.Plugin;
static class Program {
 static void Check(bool pass,string text){if(!pass)throw new Exception(text);Console.WriteLine("PASS "+text);}
 static void Run(System.Collections.IEnumerator e){while(e.MoveNext())if(e.Current is System.Collections.IEnumerator child)Run(child);}
 static void Main(){
 var m=new Managers();var c=new CardState("CardData_Shadowsiege");m.cards.hand.Add(c);
 Check(BeastmasterTraining.IsCreature(c),"vanilla asset name resolved without relying on GUID");
 Check(BeastmasterTraining.IsCreature(new("mt2_pathogens.Plugin-Card-AntigenMimic")),"optional Pathogens unit accepted");
 Check(!BeastmasterTraining.IsCreature(new("CardData_AlphaFiend")),"unlisted unit excluded");
 BeastmasterTraining.Freeze(c,m);Check(!c.frozen,"no Frozen without Vesper");
 m.monsters.characters.Add(new(1));BeastmasterTraining.Freeze(c,m);Check(c.frozen,"Frozen applied when Vesper is deployed");
 var d=new CardState("CardData_Steelsinger");m.cards.deck.Add(d);var dead=new CardState("CardData_EelGorgon");m.cards.cemetery.Add(dead);
 BeastmasterTraining.TrainHand(m);Check(c.permanent.upgrades.Single().attack==0&&c.permanent.upgrades.Single().hp==0&&c.temp.upgrades.Single().cost==1,"level I tames without stats and discounts one Ember");
 Check(d.permanent.upgrades.Count==0&&dead.permanent.upgrades.Count==0,"draw pile and cemetery receive no training");
 m.monsters.characters.Add(new(2));m.monsters.characters.Add(new(3));c.frozen=false;
 BeastmasterTraining.TrainHand(m);Check(c.permanent.upgrades.Single().attack==0&&c.permanent.upgrades.Single().hp==0&&c.temp.upgrades.Single().cost==4,"three Vesper stack discounts even without Frozen");
 m.save.PreviewMode=true;BeastmasterTraining.TrainHand(m);Check(c.permanent.upgrades.Single().hp==0,"preview does not mutate permanent deck");m.save.PreviewMode=false;
 foreach(var v in m.monsters.characters)v.muted=true;BeastmasterTraining.TrainHand(m);Check(c.permanent.upgrades.Single().hp==0,"muted Vesper do not train");
 foreach(var v in m.monsters.characters){v.muted=false;v.IsDead=true;}BeastmasterTraining.TrainHand(m);Check(c.permanent.upgrades.Single().hp==0,"dead Vesper do not train");
 c.temp=new();Check(c.permanent.upgrades.Single().hp==0&&c.temp.upgrades.Count==0,"battle reset clears discount, preserves taming");
 m.monsters.characters.Clear();m.monsters.characters.Add(new(2));var troll=new CardState("mt2_freecompany.Plugin-Card-SpawnBogTroll");m.cards.deck.Add(troll);BeastmasterTraining.RecallTroll(m);Check(m.cards.hand.Contains(troll)&&!m.cards.deck.Contains(troll)&&troll.frozen,"recall moves same troll, freezes it, matches asset not GUID");
 int n=m.cards.hand.Count;BeastmasterTraining.RecallTroll(m);Check(m.cards.hand.Count==n,"recall never duplicates troll");m.cards.hand.Remove(troll);m.cards.cemetery.Add(troll);BeastmasterTraining.RecallTroll(m);Check(!m.cards.hand.Contains(troll),"recall excludes cemetery");
 var fullTroll=new CardState("mt2_freecompany.Plugin-Card-SpawnStoneTroll");m.cards.deck.Add(fullTroll);m.cards.full=true;BeastmasterTraining.RecallTroll(m);Check(m.cards.deck.Contains(fullTroll)&&!m.cards.hand.Contains(fullTroll),"full hand does not lose troll");
 AllGameManagers.Instance=new();AllGameManagers.Core=m;
 m.cards.hand.Clear();m.cards.hand.Add(c);m.data.upgrades.Add(new(){name="mt2_freecompany.Plugin-Upgrade-TamableMarker"});
 m.monsters.characters.Clear();
 Check(BeastmasterMarkers.HandMarker(c)==null,"H1 unrelated run has no Tamable icon or tooltip");
 var savedVesper=new CardState("mt2_freecompany.Plugin-Card-SpawnVesper");m.save.runDeck.Add(savedVesper);
 Check(BeastmasterMarkers.HandMarker(c)==null,"H1 Vesper without Beastmaster does not expose Tamable");
 for(int rank=1;rank<=3;rank++){
  savedVesper.paths.Clear();savedVesper.paths.Add("mt2_freecompany.Plugin-Upgrade-upg_Beastmaster"+rank);
  Check(BeastmasterMarkers.HandMarker(c)!=null,"H1 saved Beastmaster path shows marker before deployment "+rank);
 }
 m.save.runDeck.Clear();m.monsters.characters.Add(new(2));
 Check(BeastmasterMarkers.HandMarker(c)!=null,"H1 deployed Beastmaster supports marker without saved copy");
 var beforePermanent=c.permanent.upgrades.Count;var beforeTemporary=c.temp.upgrades.Count;
 Check(BeastmasterMarkers.HandMarker(c)!=null,"eligible card in hand has Tamable even without requiring Frozen");
 Check(c.permanent.upgrades.Count==beforePermanent&&c.temp.upgrades.Count==beforeTemporary,"UI marker never becomes a real upgrade");
 var container=new CardUpgradeDisplayContainer();container.ui.card=c;
 var originals=new List<CardUpgradeState>{new()};object[] args={container,originals,false};
 var patch=typeof(BeastmasterHandIconPatch).GetMethod("Prefix",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static)!;
 patch.Invoke(null,args);Check(originals.Count==1&&((List<CardUpgradeState>)args[1]).Count==2,"display copies list and adds just one icon");
 args=new object[]{container,originals,true};patch.Invoke(null,args);Check(ReferenceEquals(args[1],originals),"secondary region display is untouched");
 m.cards.hand.Clear();Check(BeastmasterMarkers.HandMarker(c)==null,"no hand marker outside hand");
 m.cards.hand.Add(new("CardData_AlphaFiend"));Check(BeastmasterMarkers.HandMarker(m.cards.hand[0])==null,"unlisted creature has no Tamable");
 var trained=new CharacterState(1){card=c};BeastmasterMarkers.MarkTrained(trained);Check(trained.statusAdds==1,"previously trained creature gains Tamed on deployment");
 BeastmasterMarkers.MarkTrained(trained);Check(trained.statusAdds==1,"repeated callbacks do not duplicate Tamed");
 var untrained=new CharacterState(1){card=new("CardData_Shadowsiege")};BeastmasterMarkers.MarkTrained(untrained);Check(untrained.statusAdds==0,"eligible but never trained creature is not Tamed");
 var preview=new CharacterState(1){card=c};m.save.PreviewMode=true;BeastmasterMarkers.MarkTrained(preview);Check(preview.statusAdds==0,"preview cannot add a marker");m.save.PreviewMode=false;
 Check(!new StatusEffectFreeCompanyTamedState().TestTrigger(new(),new(),m),"Tamed has no executable trigger");

 var game=AllGameManagers.Instance!;game.initialized=false;int coreCalls=game.coreCalls;
 Check(BeastmasterMarkers.HandMarker(c)==null&&game.coreCalls==coreCalls,"shop/deck rendering skips uninitialized combat managers");
 container.ui.card=c;args=new object[]{container,originals,false};patch.Invoke(null,args);
 Check(ReferenceEquals(args[1],originals),"shop upgrade preview passes through original UI list without exception");
 BeastmasterMarkers.MarkTrained(new CharacterState(1){card=c});Check(game.coreCalls==coreCalls,"unit marker also skips unavailable combat scene");
 game.initialized=true;game.missingCardManager=true;
 Check(BeastmasterMarkers.HandMarker(c)==null,"missing card manager is safe even with initialized scene");
 game.missingCardManager=false;m.cards.hand.Clear();m.cards.hand.Add(c);
 Check(BeastmasterMarkers.HandMarker(c)!=null,"combat hand marker still works after shop guards");
 AllGameManagers.Instance=null;Check(BeastmasterMarkers.HandMarker(c)==null,"UI without game manager is safe");

 m.data.upgrades.Add(new(){id=BeastmasterTraining.TrainingId,name="mt2_freecompany.Plugin-Upgrade-BeastmasterTraining"});
 m.data.upgrades.Add(new(){id=BeastmasterTraining.DiscountId,name="mt2_freecompany.Plugin-Upgrade-BeastmasterDiscount"});
 var legacy=new CardUpgradeState();legacy.Setup();legacy.SetCardUpgradeDataId("mt2_freecompany.BeastmasterTraining");legacy.SetAttackDamage(3);legacy.SetAdditionalHP(7);
 Check(!legacy.LoadFromFile(m.data,m.save),"reproduces original failure: unregistered training ID cannot load");
 BeastmasterSavedUpgradePatch.Prefix(legacy,m.data);
 Check(legacy.LoadFromFile(m.data,m.save),"legacy training resolves registered definition after migration");
 Check(legacy.GetAttackDamage()==3&&legacy.GetAdditionalHP()==7,"migration preserves permanent attack and HP");
 var legacyDiscount=new CardUpgradeState();legacyDiscount.SetCardUpgradeDataId("mt2_freecompany.BeastmasterDiscount");legacyDiscount.SetCostReduction(4);BeastmasterSavedUpgradePatch.Prefix(legacyDiscount,m.data);
 Check(legacyDiscount.LoadFromFile(m.data,m.save)&&legacyDiscount.GetCostReduction()==4,"old temporary discount migrates without resetting value");
 var other=new CardUpgradeState();other.SetCardUpgradeDataId("unrelated");BeastmasterSavedUpgradePatch.Prefix(other,m.data);Check(other.GetCardUpgradeDataId()=="unrelated","other mods and native upgrades are untouched");
 Check(c.permanent.upgrades[0].LoadFromFile(m.data,m.save),"new training uses a loadable registered upgrade ID");

 m.save.data=m.data;
 var nativeRules=Newtonsoft.Json.Linq.JObject.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"json/champions/champion_vesper_beastmaster.json")));
 var tooltipDef=nativeRules["additional_tooltips"]![0]!;
 Check(((Newtonsoft.Json.Linq.JObject)tooltipDef["descriptions"]!).Count==10,"native Beastmaster rules retain all translations");
 for(int rank=1;rank<=3;rank++){
  var upgrade=nativeRules["upgrades"]!.Single(u=>(string?)u["id"]=="upg_Beastmaster"+rank);
  Check((string?)upgrade["ability_upgrade"]=="@VesperCommand"+rank,"native tooltip carrier remains available at Beastmaster rank "+rank);
  Check(Newtonsoft.Json.Linq.JToken.DeepEquals(upgrade["descriptions"],tooltipDef["descriptions"]),"native rules equal passive description at rank "+rank);
 }
 var commandEffect=nativeRules["effects"]!.Single(e=>(string?)e["id"]=="BeastmasterCommand");
 Check((string?)commandEffect["additional_tooltips"]![0]=="@BeastmasterRules","Command exposes native passive tooltip without extra gameplay effects");
 Check(typeof(BeastmasterMarkers).Assembly.GetType("mt2_freecompany.Plugin.BeastmasterChampionTooltipPatch")==null,"global Beastmaster character tooltip patch removed");
 for(int rank=1;rank<=3;rank++){
  var battle=new Managers();AllGameManagers.Core=battle;AllGameManagers.Instance=new();
  var beastCard=new CardState("mt2_freecompany.Plugin-Card-SpawnBogTroll");var beast=new CharacterState(0){card=beastCard};var v=new CharacterState(rank);
  battle.monsters.characters.Add(beast);
  Run(BeastmasterTraining.TrainDeployed(battle));Check(beastCard.permanent.upgrades.Count==0,"no taming without Vesper "+rank);
  battle.monsters.characters.Add(v);Run(BeastmasterTraining.TrainDeployed(battle));
  Check(beastCard.permanent.upgrades.Single().attack==0&&beastCard.permanent.upgrades.Single().hp==0,"rank "+rank+" deployed taming gives no stats");
  Check(beast.damage==7&&beast.hp==5&&beast.maxHp==10,"rank "+rank+" current stats unchanged");
  Check(beastCard.temp.upgrades.Count==0&&beast.HasStatusEffect(BeastmasterMarkers.StatusId),"deployed paw without discount "+rank);
  Run(BeastmasterTraining.TrainDeployed(battle));Check(beastCard.permanent.upgrades.Count==1&&beast.statusAdds==1,"repeated taming deduplicates "+rank);
  beastCard.permanent.upgrades.Single().attack=3;beastCard.permanent.upgrades.Single().hp=7;
  battle.cards.hand.Add(beastCard);BeastmasterTraining.TrainHand(battle);
  Check(beastCard.permanent.upgrades.Single().attack==3&&beastCard.permanent.upgrades.Single().hp==7&&beastCard.temp.upgrades.Single().cost==1,"preserves legacy stats and applies discount "+rank);
  battle.save.PreviewMode=true;BeastmasterTraining.TrainHand(battle);Check(beastCard.temp.upgrades.Single().cost==1,"preview cannot discount "+rank);battle.save.PreviewMode=false;
  v.muted=true;var fresh=new CardState("CardData_Shadowsiege");BeastmasterTraining.Freeze(fresh,battle);Check(!fresh.frozen&&fresh.permanent.upgrades.Count==0,"silenced Vesper cannot freeze or tame "+rank);v.muted=false;
  fresh.frozen=true;BeastmasterTraining.Freeze(fresh,battle);Check(fresh.permanent.upgrades.Count==1,"already frozen card still tamed "+rank);
  var effect=new CardEffectBeastmasterCommand();var state=new CardEffectState();var parameters=new CardEffectParams();
  Check(effect.TestEffect(state,parameters,battle),"Command available with valid beast "+rank);
  parameters.targets=new(){beast};Check(effect.TestEffect(state,parameters,battle),"selected eligible Tamed beast accepted "+rank);
  beast.IsDead=true;Check(!effect.TestEffect(state,parameters,battle),"dead target rejected "+rank);beast.IsDead=false;
  beast.canAttack=false;Check(!effect.TestEffect(state,parameters,battle),"cannot attack target rejected "+rank);beast.canAttack=true;
  beast.room!.enemies=0;Check(!effect.TestEffect(state,parameters,battle),"empty own floor rejected "+rank);beast.room.enemies=1;
  beast.statuses.Clear();Check(!effect.TestEffect(state,parameters,battle),"untamed target rejected "+rank);BeastmasterMarkers.MarkTrained(beast);
  parameters.targets.Add(beast);Check(!effect.TestEffect(state,parameters,battle),"multiple targets rejected "+rank);parameters.targets.RemoveAt(1);
  var previous=beast.card;beast.card=new("CardData_AlphaFiend");Check(!effect.TestEffect(state,parameters,battle),"unlisted creature rejected even with paw "+rank);beast.card=previous;
  Check(effect.CanApplyInPreviewMode,"target preview retained "+rank);
 }
 var spawnManagers=new Managers();AllGameManagers.Instance=new();AllGameManagers.Core=spawnManagers;
 var savedBeastCard=new CardState("CardData_Shadowsiege");var savedMarker=new CardUpgradeState();savedMarker.SetCardUpgradeDataId(BeastmasterTraining.TrainingId);savedBeastCard.permanent.upgrades.Add(savedMarker);
 var savedBeast=new CharacterState(0){card=savedBeastCard};spawnManagers.monsters.characters.Add(savedBeast);
 var spawnPatch=typeof(BeastmasterTamedSpawnPatch).GetMethod("AfterSpawn",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static)!;
 Run((System.Collections.IEnumerator)spawnPatch.Invoke(null,new object[]{savedBeast,savedBeast.OnSpawn()})!);
 Check(savedBeast.HasStatusEffect(BeastmasterMarkers.StatusId),"spawn restores persisted Tamed without deployed Vesper");
 var newBeast=new CharacterState(0){card=new("CardData_EelGorgon")};spawnManagers.monsters.characters.Add(newBeast);spawnManagers.monsters.characters.Add(new(3));
 Run((System.Collections.IEnumerator)spawnPatch.Invoke(null,new object[]{newBeast,newBeast.OnSpawn()})!);
 Check(newBeast.HasStatusEffect(BeastmasterMarkers.StatusId)&&newBeast.card.permanent.upgrades.Single().hp==0,"spawn domesticates new beast with active Vesper without stats");
 var sprite=new TrainworksReloaded.Base.Prefab.CharacterUIMeshAnimatedSprite();sprite.transform.name="renderer";sprite.transform.parent=new(){name="Character_mt2_freecompany.Plugin_VesperCharacterArt [3]"};
 Check(VesperAnimationTiming.IsVesper(sprite),"Vesper scope resolved through parent hierarchy");
 var speedType=typeof(VesperAnimationTiming).GetNestedType("SpeedPatch",System.Reflection.BindingFlags.NonPublic)!;
 var speed=speedType.GetMethod("Prefix",System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic)!;
 var idleType=typeof(VesperAnimationTiming).GetNestedType("IdlePatch",System.Reflection.BindingFlags.NonPublic)!;
 var idle=idleType.GetMethod("Prefix",System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic)!;
 var finish=typeof(VesperAnimationTiming).GetNestedType("FinishPatch",System.Reflection.BindingFlags.NonPublic)!.GetMethod("Postfix",System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic)!;
 object[] speedArgs={sprite,8f};Check(!(bool)speed.Invoke(null,speedArgs)!&&(float)speedArgs[1]==1f,"Vesper animation unaffected by accelerated combat");
 foreach(var anim in new[]{CharacterUI.Anim.Attack,CharacterUI.Anim.Attack_Spell,CharacterUI.Anim.HitReact}){
  sprite._isPlaying=true;sprite._currentAnimType=anim;sprite.returnedToIdle=false;
  Check(!(bool)idle.Invoke(null,new object[]{sprite,CharacterUI.Anim.Idle})!,"Idle cannot interrupt "+anim);
  finish.Invoke(null,new object[]{sprite});Check(!sprite.returnedToIdle,"active "+anim+" not finished early");
  sprite._isPlaying=false;finish.Invoke(null,new object[]{sprite});Check(sprite.returnedToIdle,"completed "+anim+" returns to queued Idle");
 }
 sprite._isPlaying=true;sprite._currentAnimType=CharacterUI.Anim.Attack;sprite.returnedToIdle=false;
 idle.Invoke(null,new object[]{sprite,CharacterUI.Anim.Idle});Check((bool)idle.Invoke(null,new object[]{sprite,CharacterUI.Anim.Death})!,"Death can interrupt action");sprite._isPlaying=false;sprite._currentAnimType=CharacterUI.Anim.Death;finish.Invoke(null,new object[]{sprite});Check(!sprite.returnedToIdle,"queued Idle cannot override Death");
 sprite._isPlaying=true;sprite._currentAnimType=CharacterUI.Anim.Attack;idle.Invoke(null,new object[]{sprite,CharacterUI.Anim.Idle});Check((bool)idle.Invoke(null,new object[]{sprite,CharacterUI.Anim.Attack_Spell})!,"new action can interrupt previous action");
 var otherSprite=new TrainworksReloaded.Base.Prefab.CharacterUIMeshAnimatedSprite();otherSprite.transform.name="OtherClan";otherSprite._isPlaying=true;otherSprite._currentAnimType=CharacterUI.Anim.Attack;
 object[] otherSpeed={otherSprite,8f};Check((bool)speed.Invoke(null,otherSpeed)!&&(float)otherSpeed[1]==8f,"other clan retains combat speed");Check((bool)idle.Invoke(null,new object[]{otherSprite,CharacterUI.Anim.Idle})!,"other clan retains Idle transitions");
 var rules=Newtonsoft.Json.Linq.JObject.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"json/champions/champion_vesper_beastmaster.json")));
 var cards=(Newtonsoft.Json.Linq.JArray)rules["cards"]!;
 Check(cards.Count==3,"three Command ranks");
 for(int i=0;i<3;i++){
  Check((int)cards[i]["cooldown"]! ==4-i&&(int)cards[i]["initial_cooldown"]! ==0,"ready Command cooldown "+(i+1));
  Check((bool)cards[i]["can_ability_target_other_floors"]!,"cross-floor targeting "+(i+1));
  Check(((Newtonsoft.Json.Linq.JObject)cards[i]["descriptions"]!).Count==10,"ten localized descriptions "+(i+1));
 }
 var list=(Newtonsoft.Json.Linq.JArray)rules["beastmaster_creatures"]!;
 Check(list.Count==29,"all 29 creatures retained");
 foreach(Newtonsoft.Json.Linq.JObject entry in list){
  var mod=(string?)entry["mod"];var name=(string)entry["card"]!;
  Check(BeastmasterTraining.IsCreature(new(string.IsNullOrEmpty(mod)?"CardData_"+name:mod+"-Card-"+name)),"catalog creature "+name);
 }

 }
}

// 2026-10-03-2249||codex-freecompany-fx||salidas/dll/freecompany-beastmaster-tests/Program.cs||arnés semántico del código real Beastmaster; persistencia, múltiples Vesper, pilas, preview y recuperación

// 2026-10-03-2352||codex-freecompany-fx||D:\Juegos\MT2_mod\salidas\dll\freecompany-beastmaster-tests\Program.cs||prueba elegibilidad, no alteración de upgrades, duplicados, entrenamiento y previews

// 2026-10-04-0016||codex-freecompany-fx||D:\Juegos\MT2_mod\salidas\dll\freecompany-beastmaster-tests\Program.cs||añade seis regresiones para tienda/mazo sin combate y retorno a combate

// 2026-10-04-0028||codex-freecompany-fx||D:\Juegos\MT2_mod\salidas\dll\freecompany-beastmaster-tests\Program.cs||reproduce fallo real de guardado y verifica migración, estadísticas y GUID válido

// 2026-10-04-0044||codex-freecompany-fx||D:\Juegos\MT2_mod\salidas\dll\freecompany-beastmaster-tests\Program.cs||verifica tres niveles, silencio, otros campeones y ausencia de duplicados en panel Vesper

// 2026-10-08-1936||codex-freecompany-fx||D:\Juegos\MT2_mod\salidas\dll\freecompany-beastmaster-tests\Program.cs||27 comprobaciones de entrenamiento desplegado: tres niveles, persistencia, vida, preview, silencio, muertos, carta compartida y regreso a mano

// 2026-10-08-1949||codex-freecompany-fx||D:\Juegos\MT2_mod\salidas\dll\freecompany-beastmaster-tests\Program.cs||adapta pruebas semánticas a domesticación sin atributos y Command, valida 29 criaturas y cooldowns

// 2026-10-08-1952||codex-freecompany-fx||D:\Juegos\MT2_mod\salidas\dll\freecompany-beastmaster-tests\Program.cs||prueba OnSpawn para marcas persistentes sin Vesper y domesticación nueva con Vesper

// 2026-10-08-2032||codex-freecompany-fx||D:\Juegos\MT2_mod\salidas\dll\freecompany-beastmaster-tests\Program.cs||valida velocidad, scope Vesper, finalización de tres acciones y prioridad de muerte/acción nueva

// 2026-10-27-2205||codex-freecompany-fx||D:\Juegos\MT2_mod\salidas\dll\freecompany-beastmaster-tests\Program.cs||H1: seis escenarios de icono, ausencia de Vesper/senda y presencia en mazo o tablero

// 2026-11-07-1203||codex-freecompany-fx||tests/beastmaster/Program.cs||consolida arnés existente en repositorio; enlaza fuentes locales canónicas, conserva original de laboratorio

// 2026-11-12-0912||codex-freecompany-fx||tests/beastmaster/Program.cs||migra reglas Beastmaster a AdditionalTooltipData en Command y retira parche de personaje
