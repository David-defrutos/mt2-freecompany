using System.Collections;
using mt2_freecompany.Plugin;
class Program {
 static int checks;
 static void Check(bool value,string text){if(!value)throw new Exception(text);checks++;}
 static void Run(IEnumerator e){try{while(e.MoveNext())if(e.Current is IEnumerator child)Run(child);}finally{(e as IDisposable)?.Dispose();}}
 static CharacterState Unit(int bad=0)=>new(){Negative=bad};
 static void Main(){
 var g=new Managers();var p=new CardEffectParams();var state=new CardEffectState();var clean=new CardEffectCleanseRandomDebuffed();
 var healthy=Unit();var tank=Unit(3);tank.Positive=7;var dead=Unit(2);dead.IsAlive=false;var hidden=Unit();hidden.Hidden=2;var immune=Unit(1);immune.Untouchable=true;
 var zero=Unit();zero.ZeroNegativeEntry=true;p.targets.AddRange(new[]{zero,healthy,tank,dead,hidden,immune});var original=p.targets;
 Run(clean.ApplyEffect(state,p,g,new Systems()));
 Check(tank.Negative==0&&tank.Cleansed==1,"cleanse picks debuffed tank not healthy first target");Check(zero.Cleansed==0,"zero-count negative entry is not a candidate");Check(tank.Positive==7&&dead.Negative==2&&hidden.Hidden==2&&immune.Negative==1,"cleanse preserves positive hidden dead untouchable");Check(ReferenceEquals(original,p.targets),"target list restored after native cleansing");
 var rng=RandomManager.calls;Run(clean.ApplyEffect(state,p,g,new Systems()));Check(RandomManager.calls==rng,"no RNG when nobody eligible");
 tank.Negative=2;g.save.PreviewMode=true;Run(clean.ApplyEffect(state,p,g,new Systems()));Check(RandomManager.calls==rng&&tank.Negative==2,"preview does not consume RNG or mutate status");g.save.PreviewMode=false;
 var other=Unit(1);var preview=Unit(1);preview.PreviewMode=true;g.monsters.units.AddRange(new[]{preview,other});state.global=true;Run(clean.ApplyEffect(state,p,g,new Systems()));Check(other.Negative==0&&preview.Negative==1&&tank.Negative==2,"global cleanse reaches another floor and ignores preview clones");
 other.Negative=1;var other2=Unit(3);g.monsters.units.Add(other2);Run(clean.ApplyEffect(state,p,g,new Systems()));Run(clean.ApplyEffect(state,p,g,new Systems()));Check(other.Negative==0&&other2.Negative==0,"repeated cleanse selects distinct remaining debuffed allies");
 Check(clean.TestEffect(state,p,g)&&clean.CanApplyInPreviewMode,"no-op cleansing does not block trigger/preview validation");
 var room=g.room;var on=new CardUpgradeData{name="mt2_freecompany.Plugin_WatchtowerRoomUpgrade",Damage=10,Sniper=1};var off=new CardUpgradeData{name="mt2_freecompany.Plugin_WatchtowerRoomOffUpgrade"};
 var onState=new CardUpgradeState();onState.Setup(on);var a=new TrainRoomAttachmentState();a.upgrades.Add((onState,on));a.card.effects.Add(new CardEffectState{upgrade=on,nativeAttach=true});room.attachment=a;
 var archer=Unit();archer.Attack=8;archer.Sniper=1;var ordinary=Unit();ordinary.Attack=5;room.units.AddRange(new[]{archer,ordinary});Run(room.UpdateTrainRoomAttachments());
 Check(archer.Attack==18&&ordinary.Attack==15&&archer.Sniper==1,"native active aura grants attack and additional Sniper source");
 var toggle=new CardEffectWatchtowerToggle();var action=new CardEffectState{upgrade=off};var args=new CardEffectParams{roomThatActivatedAbility=room};a.CurrentAbilityCooldown=1;
 Check(toggle.TestEffect(action,args,g),"room ability finds active Watchtower");Run(toggle.ApplyEffect(action,args,g,new Systems()));
 Check(archer.Attack==8&&ordinary.Attack==5&&archer.Sniper==1&&ordinary.Sniper==0,"off removes only aura attack and Sniper");Check(ReferenceEquals(room.attachment,a)&&a.CurrentAbilityCooldown==1,"toggle preserves room identity and cooldown");
 var newcomer=Unit();newcomer.Attack=3;room.units.Add(newcomer);Run(room.UpdateTrainRoomAttachments());Check(newcomer.Attack==3&&newcomer.Sniper==0,"units entering inactive room get no buffs");
 Run(toggle.ApplyEffect(action,args,g,new Systems()));Check(archer.Attack==18&&ordinary.Attack==15&&newcomer.Attack==13,"on grants exactly ten to all present units");
 for(int i=0;i<10;i++){Run(toggle.ApplyEffect(action,args,g,new Systems()));Check(ordinary.Attack==(i%2==0?5:15),"ten toggle cycles do not accumulate attack");}

 var gearData=new CardUpgradeData{name="SniperEquipment",Sniper=1};var gear=new CardUpgradeState();gear.Setup(gearData);Run(ordinary.ApplyCardUpgrade(gear,false,"equipment",false,false,null,null,null!));
 Run(toggle.ApplyEffect(action,args,g,new Systems()));Check(ordinary.Sniper==1&&ordinary.Attack==5,"equipment Sniper added while active survives toggling off");Run(toggle.ApplyEffect(action,args,g,new Systems()));
 Check(a.upgrades.Count==1&&a.CharactersImpacted.Count==3,"toggle leaves one aura and correct membership");
 g.save.PreviewMode=true;var attack=archer.Attack;Run(toggle.ApplyEffect(action,args,g,new Systems()));Check(archer.Attack==attack,"toggle preview is mutation-free");g.save.PreviewMode=false;
 a.AttachType=TrainRoomAttachmentType.Primary_Embedded;Check(toggle.TestEffect(action,args,g),"embedded Watchtower remains toggleable");
 var stranger=new TrainRoomAttachmentState();stranger.upgrades.Add((new(),new(){name="OtherRoom"}));room.attachment=stranger;Check(!toggle.TestEffect(action,args,g),"another room is not toggleable");

 var combinedRoom=new TrainRoomAttachmentState();var otherData=new CardUpgradeData{name="OtherRoom",Damage=2};var otherState=new CardUpgradeState();otherState.Setup(otherData);
 combinedRoom.upgrades.Add((otherState,otherData));combinedRoom.upgrades.Add((onState,on));combinedRoom.card.effects.Add(new CardEffectState{upgrade=otherData,nativeAttach=true});combinedRoom.card.effects.Add(new CardEffectState{upgrade=on,nativeAttach=true});room.attachment=combinedRoom;room.units.Clear();var mergedUnit=Unit();mergedUnit.Attack=5;room.units.Add(mergedUnit);Run(room.UpdateTrainRoomAttachments());
 Check(mergedUnit.Attack==17,"merged room starts with both auras");Run(toggle.ApplyEffect(action,args,g,new Systems()));
 Check(mergedUnit.Attack==7&&mergedUnit.Sniper==0,"merged toggle disables only Watchtower even if another attachment effect is first");
 Check(ReferenceEquals(combinedRoom.upgrades[0].Item2,otherData)&&combinedRoom.upgrades.Count==2,"merged room keeps other upgrade unchanged");Run(toggle.ApplyEffect(action,args,g,new Systems()));
 Check(mergedUnit.Attack==17&&mergedUnit.Sniper==1,"merged room restores Watchtower aura without duplicate other room");
 var relic=new RelicEffectFieldForgeCost();relic.Initialize(new(),new(),new(){Amount=-1});AllGameManagers.Instance=new(){relics=g.relics};g.relics.effects.Add(relic);
 var stats=new CardStatistics();
 foreach(int cost in new[]{0,1,2,5})foreach(int extra in new[]{0,-1,-2,2}){
  var card=new CardState{Cost=cost};card.mods.cost=extra-1;int before=card.mods.cost;
  int merged=FieldForgeMergeCost.ComponentCost(card,stats,null,null,null);
  Check(merged==Math.Clamp(cost+extra,0,99),"merge lifts only Field Forge before cost clamp");Check(card.mods.cost==before,"merge helper restores temporary modifier");
 }
 var failure=new CardState{Cost=3,ThrowCost=true};failure.mods.cost=-1;try{FieldForgeMergeCost.ComponentCost(failure,stats,null,null,null);throw new Exception("missing exception");}catch(InvalidOperationException){}Check(failure.mods.cost==-1,"temporary cost restored on exception");
 var spell=new CardState{Cost=3,Type=CardType.Spell};spell.mods.cost=-1;Check(FieldForgeMergeCost.ComponentCost(spell,stats,null,null,null)==2,"non-equipment unaffected");
 g.relics.effects.Clear();var noRelic=new CardState{Cost=4};noRelic.mods.cost=-2;Check(FieldForgeMergeCost.ComponentCost(noRelic,stats,null,null,null)==2,"no Field Forge means unchanged merge cost");
 Console.WriteLine($"PASS {checks} semantic checks (actual fixes, simulated game API)");
 }
}

// 2026-10-26-2054||codex-freecompany-fx||D:\Juegos\MT2_mod\salidas\build\freecompany-fixes-2026-10-26\tests\Program.cs||prueba candidatos antes del azar, preview, Watchtower no acumulativo y costes fusionados con otros descuentos

// 2026-10-26-2103||codex-freecompany-fx||D:\Juegos\MT2_mod\salidas\build\freecompany-fixes-2026-10-26\tests\Program.cs||modela clamp Sniper 0/1 nativo y prueba conservación de Sniper inicial y de equipo posterior

// 2026-10-26-2105||codex-freecompany-fx||D:\Juegos\MT2_mod\salidas\build\freecompany-fixes-2026-10-26\tests\Program.cs||prueba no seleccionar entradas negativas residuales a cero junto a sano y afectado

// 2026-10-26-2211||codex-freecompany-fx||D:\Juegos\MT2_mod\salidas\build\freecompany-fixes-2026-10-26\tests\Program.cs||pruebas de sala fusionada con otra aura antes de Watchtower: desactivar, reactivar y conservar mejora ajena

// 2026-10-26-2211||codex-freecompany-fx||D:\Juegos\MT2_mod\salidas\build\freecompany-fixes-2026-10-26\tests\Program.cs||evita colisión de nombre de variable en pruebas de sala fusionada

// 2026-11-07-1203||codex-freecompany-fx||tests/cleanse-rooms/Program.cs||consolida arnés existente en repositorio; enlaza fuentes locales canónicas, conserva original de laboratorio
