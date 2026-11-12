// Documento generado el 2026-10-29-1955
using System.Collections;
using mt2_freecompany.Plugin;
static class Program {
 static int checks;
 static void Check(bool condition,string name){if(!condition)throw new Exception(name);checks++;Console.WriteLine("PASS "+name);}
 sealed class Setup {
  internal Managers managers=new();internal RoomState room=new();internal CharacterState owner,target,enemy;
  internal Setup(int tier=1){
   AllGameManagers.Instance=new(){managers=managers};
   for(int n=1;n<=3;n++)managers.data.upgrades.Add(new(){name=MyPluginInfo.PLUGIN_GUID+"-Upgrade-upg_Wall"+n});
   owner=new(){room=room,point=new(){index=0},card=new(){asset=MyPluginInfo.PLUGIN_GUID+"-Card-SpawnRoderic"}};owner.upgrades.Add(managers.data.upgrades[tier-1]);
   target=new(){hp=10,room=room,point=new(){index=1},card=new(){asset="Ally"}};enemy=new(){team=Team.Type.Heroes,room=room,hp=100};room.units.AddRange(new[]{owner,target});
  }
  internal void Hit(int damage=20,Damage.Type type=Damage.Type.DirectAttack,CharacterState? targetUnit=null)=>Bridge.Run((targetUnit??target).ApplyDamage(damage,new(){attacker=enemy,damageType=type},new(),new()));
 }
 static void Main(){
  // A broken native binding must not initialize when any card asks for a tooltip.
  Check(typeof(ShieldwallInterception).GetNestedType("CardRulesPatch",System.Reflection.BindingFlags.NonPublic)==null,"no global card tooltip patch remains");
  Check(!(bool)typeof(ShieldwallInterception).GetField("nativeBeforeAttackChecked",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static)!.GetValue(null)!,"unrelated card tooltip does not bind private combat API");
  var damageCase=new Setup();damageCase.owner.hp=5;damageCase.Hit(20);
  Check(damageCase.enemy.lastBeforeDamage==5,"new native Before Damage receives actual damage argument once");
  foreach(int tier in new[]{1,2,3}){var a=new Setup(tier);a.Hit();Check(a.owner.hp==80&&a.target.hp==10,"intercept full overkill damage at tier "+tier);Check(a.enemy.attacks==1&&a.target.debuffs==1&&a.owner.debuffs==0,"attack effects stay on original target at tier "+tier);Check(a.enemy.before==1&&a.enemy.healed==20,"one before-attack and actual lifesteal at tier "+tier);}
  var x=new Setup();x.target.stacks["armor"]=50;x.Hit();Check(x.owner.hp==80&&x.target.stacks["armor"]==50,"rear Armor is not spent or used to reduce transferred hit");
  x=new Setup();x.target.stacks["damage shield"]=1;x.owner.stacks["damage shield"]=2;x.Hit();Check(x.owner.hp==100&&x.target.hp==10&&x.target.stacks["damage shield"]==0&&x.owner.stacks["damage shield"]==2,"protected Damage Shield consumed first; interceptor shields untouched");x.Hit();Check(x.owner.stacks["damage shield"]==1&&x.target.hp==10,"next hit uses interceptor Damage Shield");
  x=new Setup();x.owner.stacks["armor"]=8;x.owner.stacks["titanskin"]=2;x.Hit();Check(x.owner.hp==90&&x.owner.stacks["armor"]==0&&x.target.hp==10,"native interceptor Armor and Titanskin resolve transferred hit");
  x=new Setup();x.target.point.index=0;x.owner.point.index=1;x.Hit();Check(x.owner.hp==100&&x.target.hp==0,"units in front are not protected");
  x=new Setup();x.target.room=new();x.target.room.units.Add(x.target);x.Hit();Check(x.owner.hp==100&&x.target.hp==0,"other floor not protected");
  x=new Setup();x.owner.hp=0;x.Hit();Check(x.target.hp==0,"dead interceptor offers no protection");
  x=new Setup();x.owner.stacks["silenced"]=1;x.Hit();Check(x.target.hp==0,"silenced interceptor offers no protection");
  x=new Setup();x.owner.stacks["muted"]=1;x.Hit();Check(x.target.hp==0,"muted interceptor offers no protection");
  x=new Setup();x.owner.card!.asset="OtherMod-Card-SpawnRoderic";x.Hit();Check(x.target.hp==0,"other clan identity not treated as Shieldwall");
  x=new Setup();x.owner.upgrades.Clear();x.Hit();Check(x.target.hp==0,"other Roderic paths do not intercept");
  foreach(var type in new[]{Damage.Type.Default,Damage.Type.Poison,Damage.Type.Decay,Damage.Type.Spikes}){var a=new Setup();a.Hit(5,type);Check(a.owner.hp==100&&a.target.hp==5,"non-attack damage unchanged: "+type);}
  foreach(var type in new[]{Damage.Type.Splash,Damage.Type.Trample}){var a=new Setup();a.Hit(5,type);Check(a.owner.hp==95&&a.target.hp==10,"native attack damage intercepted: "+type);}
  x=new Setup();x.enemy.team=Team.Type.Monsters;x.Hit(5);Check(x.owner.hp==100&&x.target.hp==5,"friendly damage not intercepted");
  x=new Setup();x.target.stacks["untouchable"]=1;x.Hit();Check(x.owner.hp==100&&x.target.hp==10,"untouchable rear unit does not send a hit to Roderic");
  x=new Setup();x.enemy.crit=2;x.room.bonus=3;x.Hit(10);Check(x.owner.hp==74&&x.enemy.bonusCalls==1,"attacker bonus and room modifier applied once, not doubled by transfer");
  x=new Setup();x.owner.hp=5;x.Hit();Check(x.target.hp==10&&x.owner.hp==0&&x.enemy.slays==1,"interceptor death credited once; protected ally survives overkill");Check(x.enemy.healed==5,"lifesteal limited to actual interceptor HP lost");x.Hit();Check(x.target.hp==0&&x.enemy.slays==2,"later attack after Roderic death resolves on normal target");
  x=new Setup();x.owner.hp=5;x.owner.stacks["undying"]=1;x.Hit();Check(x.owner.hp==1&&x.owner.stacks["undying"]==0&&x.target.hp==10,"native Undying revives interceptor");x.Hit();Check(x.owner.hp==0&&x.target.hp==10,"revived Roderic intercepts again until actually dead");
  x=new Setup();x.owner.stacks["damage shield"]=2;var third=new CharacterState{hp=10,room=x.room,point=new(){index=2}};x.room.units.Add(third);x.Hit(20,targetUnit:x.owner);x.Hit();x.Hit(20,targetUnit:third);Check(x.owner.hp==80&&x.owner.stacks["damage shield"]==0&&x.target.hp==10&&third.hp==10,"Sweep consumes own hit plus each protected ally hit");
  x=new Setup();x.Hit(5);x.Hit(5);x.Hit(5);Check(x.owner.hp==85&&x.target.hp==10&&x.target.debuffs==3,"Multistrike intercepts individual hits without stacking attack callbacks");
  x=new Setup();x.owner.stacks["spikes"]=3;x.target.stacks["spikes"]=2;x.Hit();Check(x.enemy.hp==98&&x.owner.spikesReturned==0&&x.target.spikesReturned==1,"OnAttacked effects stay on protected target, not interceptor");
  x=new Setup();x.owner.stacks["spikes"]=3;x.Hit(5,targetUnit:x.owner);Check(x.enemy.hp==97&&x.owner.spikesReturned==1,"Roderic normal front attack retains his own Spikes");
  x=new Setup();x.owner.stacks["fragile"]=1;x.Hit(1);Check(x.owner.hp==0&&x.target.hp==10,"native Fragile on interceptor is respected");
  x=new Setup();x.target.stacks["fragile"]=1;x.Hit(1);Check(x.owner.hp==99&&x.target.hp==10,"protected Fragile unit takes no transferred damage");
  x=new Setup();x.owner.nestedTarget=x.owner;x.Hit();Check(x.owner.hp==77&&x.target.hp==10&&x.enemy.bonusCalls==1,"nested non-attack damage is isolated from transfer context");
  x=new Setup();var secondOwner=new CharacterState{room=x.room,point=new(){index=1},card=new(){asset=MyPluginInfo.PLUGIN_GUID+"-Card-SpawnRoderic"}};secondOwner.upgrades.Add(x.managers.data.upgrades[0]);x.target.point.index=2;x.room.units.Add(secondOwner);x.Hit();Check(x.owner.hp==80&&secondOwner.hp==100,"multiple Shieldwalls select frontmost provider without redirect loops");
  x=new Setup();x.owner.PreviewMode=true;x.owner.previewHp=100;x.target.PreviewMode=true;x.target.previewHp=10;x.enemy.PreviewMode=true;x.Hit();Check(x.owner.previewHp==80&&x.target.previewHp==10,"preview predicts interceptor damage");Check(x.owner.hp==100&&x.target.hp==10,"preview leaves real HP unchanged");Check(Bridge.ContextEmpty(),"preview leaves no interception context active");
  x=new Setup();x.owner.throwInShield=true;try{x.Hit();throw new Exception("expected native failure");}catch(Exception e)when(e.Message.Contains("native stage error")||e.InnerException?.Message=="native stage error"){}Check(Bridge.ContextEmpty(),"native exception restores coroutine context");
  using(var doc=System.Text.Json.JsonDocument.Parse(System.IO.File.ReadAllText(System.IO.Path.GetFullPath(System.IO.Path.Combine(AppContext.BaseDirectory,"../../../../../json/champions/champion_roderic_wall.json"))))){
   var data=doc.RootElement;var tooltipData=data.GetProperty("additional_tooltips")[0];
   Check(tooltipData.GetProperty("titles").EnumerateObject().Count()==10&&tooltipData.GetProperty("descriptions").EnumerateObject().Count()==10,"native Interception tooltip retains ten translations");
   foreach(int tier in new[]{1,2,3}){
    var path=data.GetProperty("upgrades").EnumerateArray().Single(u=>u.GetProperty("id").GetString()=="upg_Wall"+tier);
    Check(path.GetProperty("trigger_upgrades").EnumerateArray().Any(t=>t.GetString()=="@WallTrigger"+tier),"Shieldwall path retains tooltip carrier "+tier);
    var effect=data.GetProperty("effects").EnumerateArray().Single(e=>e.GetProperty("id").GetString()=="WallArmor"+tier);
    Check(effect.GetProperty("additional_tooltips")[0].GetString()=="@ShieldwallInterceptionRules","same native tooltip ID attached at rank "+tier);
   }
  }
  Check(typeof(ShieldwallInterception).GetNestedType("UnitRulesPatch",System.Reflection.BindingFlags.NonPublic)==null,"no global character tooltip patch remains");
  x=new Setup();var ordered=x;x.enemy.beforeAction=target=>{Check(ReferenceEquals(target,ordered.target),"Before Damage keeps original target");Check(ordered.owner.hp==100,"Before Damage runs before interceptor HP loss");};x.Hit();Check(x.enemy.before==1&&x.owner.hp==80,"Before Damage runs once at correct stage");
  x=new Setup();x.enemy.crit=2;var suspended=x.target.ApplyDamage(20,new(){attacker=x.enemy,damageType=Damage.Type.DirectAttack},new(),new());bool yielded=suspended.MoveNext();Check(yielded&&Bridge.ContextEmpty(),"suspended attack leaves no global context active");var child=suspended.Current as IEnumerator;Bridge.Run(x.owner.ApplyDamage(3,new(){damageType=Damage.Type.Poison},new(),new()));if(child!=null)Bridge.Run(child);Bridge.Run(suspended);Check(x.owner.hp==57&&x.target.hp==10,"interleaved damage is not mistaken for transferred damage");
  HarmonyLib.AccessTools.FailBinding=true;
  typeof(ShieldwallInterception).GetField("nativeBeforeAttackChecked",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static)!.SetValue(null,false);
  typeof(ShieldwallInterception).GetField("nativeBeforeAttack",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static)!.SetValue(null,null);
  x=new Setup();x.Hit(5);Check(x.owner.hp==100&&x.target.hp==5,"binding failure leaves native damage intact");
  Check(typeof(ShieldwallInterception).GetNestedType("CardRulesPatch",System.Reflection.BindingFlags.NonPublic)==null,"no global card tooltip patch remains");
  Check(Bridge.ContextEmpty(),"binding failure cannot poison tooltip type or leak combat context");
  Console.WriteLine($"PASS {checks} Shieldwall checks (production source and simulated native pipeline; not Unity)");
 }
}

// 2026-10-29-1955||codex-freecompany-fx||salidas/build/freecompany-shieldwall-2026-10-29-1500/tests/Program.cs||regresiones Intercepcion con fuente real y pipeline simulado en orden nativo, preview, escudos, muerte y efectos originales

// 2026-10-29-1957||codex-freecompany-fx||salidas/build/freecompany-shieldwall-2026-10-29-1500/tests/Program.cs||regresion del orden y objetivo original de Before Damage frente a transferencia

// 2026-10-29-1959||codex-freecompany-fx||salidas/build/freecompany-shieldwall-2026-10-29-1500/tests/Program.cs||regresion de golpe suspendido y dano intercalado sin fuga de contexto

// 2026-11-07-1203||codex-freecompany-fx||tests/shieldwall/Program.cs||consolida arnés existente en repositorio; enlaza fuentes locales canónicas, conserva original de laboratorio

// 2026-11-12-0901||codex-freecompany-fx||tests/shieldwall/Program.cs||añade regresiones de tooltip ajeno sin enlace nativo y propagación del nuevo parámetro de daño

// 2026-11-12-0901||codex-freecompany-fx||tests/shieldwall/Program.cs||añade prueba de enlace fallido sin daño global y elimina aviso nullable

// 2026-11-12-0907||codex-freecompany-fx||tests/shieldwall/Program.cs||sustituye tooltips Harmony de Intercepción por AdditionalTooltipData nativo y actualiza regresiones
