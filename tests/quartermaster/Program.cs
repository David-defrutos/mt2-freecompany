// Documento generado el 2026-10-29-1405
using System.Reflection;
using mt2_freecompany.Plugin;
static class Program {
 static int checks;
 static void Check(bool ok,string name){if(!ok)throw new Exception(name);checks++;Console.WriteLine("PASS "+name);}
 static Managers Setup(int tier=1){
  RodericQuartermaster.Reset();RandomManager.Reset(42);var g=new Managers();
  foreach(var name in new[]{"Buckler","KiteShield","BulwarkPlate","NotchedAxe","TwinAxes","ReaversMaul","Lockpicks","PoisonedDagger","ThiefsMantle","ChalkCircle","Grimoire","ArchmagesStaff","Bandages","HolySymbol","Reliquary","Shortbow","Longbow","GreatBow"})g.data.cards.Add(new(){name=MyPluginInfo.PLUGIN_GUID+"-Card-"+name});
  for(int n=1;n<=3;n++)g.data.upgrades.Add(new(){name=MyPluginInfo.PLUGIN_GUID+"-Upgrade-upg_Quarter"+n,description="Rules "+n});
  var r=new CharacterState{card=new(){asset=MyPluginInfo.PLUGIN_GUID+"-Card-SpawnRoderic"}};r.upgrades.Add(g.data.upgrades[tier-1]);g.monsters.units.Add(r);return g;
 }
 static CharacterState Ally(string role="Warrior")=>new(){card=new(){asset="OtherUnit"},subtypes=[new(){Key=MyPluginInfo.PLUGIN_GUID+"-Subtype-Sub_"+role}]};
 static CharacterState Dead(params CardState[] gear)=>new(){IsDead=true,hp=0,equipment=gear.ToList()};
 static void Main(){
  foreach(int tier in new[]{1,2,3}){var g=Setup(tier);RodericQuartermaster.Supply(Ally(),g);Check(g.cards.hand.Single().asset.EndsWith(new[]{"Buckler","KiteShield","BulwarkPlate"}[tier-1]),"supply correct tier "+tier);Check(RandomManager.calls==0,"known role no RNG "+tier);}
  var a=Setup();var owner=a.monsters.units[0];RodericQuartermaster.Supply(owner,a);Check(a.cards.hand.Count==0,"owner summon does not consume supply");RodericQuartermaster.Supply(Ally("Cleric"),a);Check(a.cards.hand.Single().asset.EndsWith("Bandages"),"first other ally determines kit");RodericQuartermaster.Supply(Ally(),a);Check(a.cards.hand.Count==1,"second summon same turn gives no kit");a.combat.turn++;RodericQuartermaster.Supply(Ally("Ranger"),a);Check(a.cards.hand.Count==2&&a.cards.hand.Last().asset.EndsWith("Shortbow"),"supply resets next turn");
  var b=Setup();b.save.PreviewMode=true;RodericQuartermaster.Supply(Ally("Unknown"),b);Check(b.cards.added.Count==0&&RandomManager.calls==0,"preview no cards or RNG");b.save.PreviewMode=false;RodericQuartermaster.Supply(Ally(),b);Check(b.cards.hand.Count==1,"preview did not consume supply");
  b=Setup();var pu=Ally();pu.PreviewMode=true;RodericQuartermaster.Supply(pu,b);Check(b.cards.added.Count==0,"preview unit cannot grant card");pu.PreviewMode=false;pu.team=Team.Type.Heroes;RodericQuartermaster.Supply(pu,b);Check(b.cards.added.Count==0,"enemy spawn cannot grant card");
  b=Setup();b.monsters.units[0].statuses.Add("silenced");RodericQuartermaster.Supply(Ally(),b);Check(b.cards.added.Count==0,"silenced provider gives no supply");b.monsters.units[0].statuses.Clear();RodericQuartermaster.Supply(Ally(),b);Check(b.cards.added.Count==1,"silence did not consume supply");
  b=Setup();b.monsters.units[0].IsDead=true;RodericQuartermaster.Supply(Ally(),b);Check(b.cards.added.Count==0,"dead provider gives no supply");
  b=Setup();b.monsters.units[0].card!.asset="other.Plugin-Card-SpawnRoderic";RodericQuartermaster.Supply(Ally(),b);Check(b.cards.added.Count==0,"path identity restricted to our Roderic");
  b=Setup();b.monsters.units[0].card!.upgrades.Add(b.data.upgrades[2]);RodericQuartermaster.Supply(Ally(),b);Check(b.cards.hand.Single().asset.EndsWith("BulwarkPlate"),"highest path wins without duplicate tier bonuses");
  b=Setup();b.cards.max=0;RodericQuartermaster.Supply(Ally(),b);Check(b.cards.deck.Count==1&&b.cards.hand.Count==0,"full hand preserves kit in draw pile");
  b=Setup();b.cards.reject=true;RodericQuartermaster.Supply(Ally(),b);b.cards.reject=false;RodericQuartermaster.Supply(Ally(),b);Check(b.cards.added.Count==1,"failed card creation does not lose supply");
  b=Setup();b.cards.onAdd=()=>RodericQuartermaster.Supply(Ally(),b);RodericQuartermaster.Supply(Ally(),b);Check(b.cards.added.Count==1,"hand callback nested summon cannot duplicate supply");
  b=Setup(2);RodericQuartermaster.Supply(Ally("Unknown"),b);Check(RandomManager.calls==1,"unknown role uses one seeded choice");string seeded=b.cards.hand.Single().asset;b=Setup(2);RodericQuartermaster.Supply(Ally("Unknown"),b);Check(b.cards.hand.Single().asset==seeded,"seeded fallback reproducible");
  var g1=new CardState{asset="Merged",modifiers={{"cost",-1}}};var component=new CardState{asset="Component"};g1.merged.Add(component);var g2=new CardState{asset="Second"};b=Setup();var dead=Dead(g1,g2);
  Check(RodericQuartermaster.Salvage(dead,g1,CardPile.ExhaustedPile,b)==CardPile.HandPile,"first equipped death recovers equipment");Check(RodericQuartermaster.Salvage(dead,g2,CardPile.ExhaustedPile,b)==CardPile.HandPile,"all equipment on selected death recovered");Check(RodericQuartermaster.Salvage(dead,g1,CardPile.ExhaustedPile,b)==CardPile.HandPile,"repeated native checks do not spend again");Check(b.cards.added.Count==0&&g1.modifiers["cost"]==-1&&ReferenceEquals(g1.merged[0],component),"salvage creates no card and preserves upgrades/merged identities");
  var otherGear=new CardState();var second=Dead(otherGear);Check(RodericQuartermaster.Salvage(second,otherGear,CardPile.ExhaustedPile,b)==CardPile.ExhaustedPile,"second equipped death same turn not recovered");b.combat.turn++;Check(RodericQuartermaster.Salvage(second,otherGear,CardPile.ExhaustedPile,b)==CardPile.HandPile,"salvage resets next turn");
  b=Setup();b.cards.max=0;dead=Dead(g1,g2);Check(RodericQuartermaster.Salvage(dead,g1,CardPile.ExhaustedPile,b)==CardPile.DeckPile,"full hand salvages to draw pile");b.cards.max=1;Check(RodericQuartermaster.Salvage(dead,g2,CardPile.ExhaustedPile,b)==CardPile.HandPile,"destination reevaluates current hand capacity");
  b=Setup();dead=Dead(g1);b.save.PreviewMode=true;Check(RodericQuartermaster.Salvage(dead,g1,CardPile.ExhaustedPile,b)==CardPile.ExhaustedPile,"preview cannot change equipment destination");b.save.PreviewMode=false;Check(RodericQuartermaster.Salvage(dead,g1,CardPile.ExhaustedPile,b)==CardPile.HandPile,"preview does not consume recovery");
  b=Setup();dead=Dead(g1);Check(RodericQuartermaster.Salvage(dead,g1,CardPile.KeepInStandBy,b)==CardPile.KeepInStandBy,"undying or inherited graft stays in native standby");Check(RodericQuartermaster.Salvage(dead,g1,CardPile.ExhaustedPile,b)==CardPile.HandPile,"pending death did not consume recovery");
  b=Setup();dead=Dead(g1);var removed=new CardState();Check(RodericQuartermaster.Salvage(dead,removed,CardPile.ExhaustedPile,b)==CardPile.ExhaustedPile,"previously replaced equipment cannot be recovered");Check(RodericQuartermaster.Salvage(dead,g1,CardPile.ExhaustedPile,b)==CardPile.HandPile,"replaced equipment check does not spend recovery");
  b=Setup();dead=Dead(g1);dead.IsDead=false;dead.hp=10;Check(RodericQuartermaster.Salvage(dead,g1,CardPile.ExhaustedPile,b)==CardPile.ExhaustedPile,"bounce or living removal does not trigger salvage");
  b=Setup();dead=Dead(g1);dead.team=Team.Type.Heroes;Check(RodericQuartermaster.Salvage(dead,g1,CardPile.ExhaustedPile,b)==CardPile.ExhaustedPile,"enemy death cannot consume recovery");
  b=Setup();b.monsters.units[0].statuses.Add("muted");dead=Dead(g1);Check(RodericQuartermaster.Salvage(dead,g1,CardPile.ExhaustedPile,b)==CardPile.ExhaustedPile,"muted provider cannot salvage");
  b=Setup();var owner2=Ally();owner2.card=new(){asset=MyPluginInfo.PLUGIN_GUID+"-Card-SpawnRoderic"};owner2.upgrades.Add(b.data.upgrades[0]);b.monsters.units.Add(owner2);dead=Dead(g1,g2);RodericQuartermaster.Salvage(dead,g1,CardPile.ExhaustedPile,b);RodericQuartermaster.Salvage(dead,g2,CardPile.ExhaustedPile,b);Check(RodericQuartermaster.Salvage(Dead(otherGear),otherGear,CardPile.ExhaustedPile,b)==CardPile.HandPile,"two Quartermasters recover distinct deaths without duplicate first set");
  b=Setup();dead=Dead(g1);RodericQuartermaster.Salvage(dead,g1,CardPile.ExhaustedPile,b);RodericQuartermaster.Supply(Ally(),b);Check(b.cards.added.Count==1,"supply and salvage have independent limits");
  b=Setup();owner=b.monsters.units[0];RodericQuartermaster.Supply(Ally(),b);RodericQuartermaster.Reset();RodericQuartermaster.Supply(Ally(),b);Check(b.cards.hand.Count==2,"battle reset clears per-instance usage");
  var tooltip=typeof(RodericQuartermaster).GetNestedType("RulesTooltipPatch",BindingFlags.NonPublic)!;var append=tooltip.GetMethod("AppendRules",BindingFlags.Static|BindingFlags.NonPublic)!;
  var rendered=(IEnumerable<(TooltipContent content,bool allowDuplicates)>)append.Invoke(null,new object[]{Array.Empty<(TooltipContent,bool)>(),owner,b.save})!;var rule=rendered.Single();Check(rule.content.body=="Rules 1","unit tooltip uses localized registered path description");
  rendered=(IEnumerable<(TooltipContent content,bool allowDuplicates)>)append.Invoke(null,new object[]{new[]{rule},owner,b.save})!;Check(rendered.Count()==1,"unit tooltip not duplicated");
  b=Setup();AllGameManagers.Instance=new(){core=b};RodericQuartermaster.Supply(Ally(),b);b.save.PreviewMode=true;
  var reset=typeof(RodericQuartermaster).GetNestedType("BattleResetPatch",BindingFlags.NonPublic)!.GetMethod("Prefix",BindingFlags.Static|BindingFlags.NonPublic)!;reset.Invoke(null,null);b.save.PreviewMode=false;RodericQuartermaster.Supply(Ally(),b);Check(b.cards.hand.Count==1,"preview battle setup cannot reset real supply limit");
  reset.Invoke(null,null);RodericQuartermaster.Supply(Ally(),b);Check(b.cards.hand.Count==2,"real battle setup resets usage");
  b=Setup();AllGameManagers.Instance=new(){core=b};var before=typeof(RodericQuartermaster).GetNestedType("SupplyPatch",BindingFlags.NonPublic)!.GetMethod("BeforeSpawn",BindingFlags.Static|BindingFlags.NonPublic)!;
  System.Collections.IEnumerator NativeSpawn(){Check(b.cards.hand.Count==1,"supply precedes nested Summon triggers");yield break;}
  var wrapper=(System.Collections.IEnumerator)before.Invoke(null,new object[]{Ally(),NativeSpawn()})!;while(wrapper.MoveNext()){};AllGameManagers.Instance=null;
  Console.WriteLine($"PASS {checks} Quartermaster checks (production sources, simulated game API; not Unity)");
 }
}

// 2026-10-29-1405||codex-freecompany-fx||salidas/build/freecompany-quartermaster-2026-10-29-1402/tests/Program.cs||pruebas de Quartermaster: limites por turno, preview, identidad de equipo, fusiones, mano llena y coexistencia

// 2026-10-29-1407||codex-freecompany-fx||salidas/build/freecompany-quartermaster-2026-10-29-1402/tests/Program.cs||anade regresiones de inicializacion preview y orden anterior a Summon

// 2026-11-07-1203||codex-freecompany-fx||tests/quartermaster/Program.cs||consolida arnés existente en repositorio; enlaza fuentes locales canónicas, conserva original de laboratorio
