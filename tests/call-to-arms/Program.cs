using System.Collections;
using mt2_freecompany.Plugin;
public static class Program {
 static int count;
 static void Check(bool ok,string name){if(!ok)throw new Exception(name);count++;}
 static void Run(IEnumerator e){while(e.MoveNext())if(e.Current is IEnumerator child)Run(child);}
 public static void Main(){
 var g=new Managers();var systems=new Systems();var effect=new CardEffectRodericReinforcements();var state=new CardEffectState();var owner=new CharacterState();g.room=owner.room;
 var args=new CardEffectParams{characterThatActivatedAbility=owner};
 CardState Card(int cost)=>new(){cost=cost};
 var one=Card(1);var two=Card(2);var three=Card(3);var zero=Card(0);var x=Card(-1);var champ=Card(1);champ.champion=true;var spell=Card(0);spell.monster=false;
 g.cards.draw.AddRange(new[]{one,two,three,zero,x,champ,spell});
 var dead=Card(1);g.cards.exhausted.Add(dead);RodericReinforcements.RecordReturn(dead,new(){hp=0,IsDead=true});
 var list=RodericReinforcements.Candidates(g.cards,1,out var revive);
 Check(!revive&&list.SequenceEqual(new[]{one,zero}),"draw priority, base cost, zero, exclusions");
 Check(RodericReinforcements.Candidates(g.cards,2,out _).Count==3,"rank two");
 Check(RodericReinforcements.Candidates(g.cards,3,out _).Count==4,"rank three");
 Check(effect.TestEffect(state,args,g),"valid availability");
 bool NativePreviewValid()=>effect.TestEffect(state,args,g)&&effect.CanApplyInPreviewMode;
 Check(NativePreviewValid(),"native GameEffectHelper preview gate accepts cost-one draw unit");
 g.save.PreviewMode=true;Run(effect.ApplyEffect(state,args,g,systems));
 Check(g.monsters.spawned==null&&!ScreenManager.showing&&g.cards.draw.Contains(one),"preview gate opens no screen, spawns nothing and preserves draw pile");
 Check(NativePreviewValid(),"preview evaluation keeps ability available");g.save.PreviewMode=false;
Check(RandomManager.calls==0,"test never rolls RNG");
 owner.room.count=7;Check(!effect.TestEffect(state,args,g),"seven blocks");owner.room.count=6;
 Check(effect.TestEffect(state,args,g),"six allows capacity overflow");
 owner.room.remaining=0;Check(!effect.TestEffect(state,args,g),"physical slots block");owner.room.remaining=1;
 owner.point.index=6;Check(!effect.TestEffect(state,args,g),"behind slot required");owner.point.index=2;
 owner.IsDead=true;Check(!effect.TestEffect(state,args,g),"dead owner blocks");owner.IsDead=false;
 Check(effect.TestEffect(state,new(){selfTarget=owner},g),"self target fallback");
 g.save.PreviewMode=true;Run(effect.ApplyEffect(state,args,g,systems));Check(g.monsters.spawned==null&&RandomManager.calls==0,"preview no mutation");g.save.PreviewMode=false;
 g.cards.draw.Clear();g.cards.draw.Add(one);one.upgrade=9;
 Run(effect.ApplyEffect(state,args,g,systems));
 Check(g.monsters.card==one&&one.upgrade==9,"same card preserves upgrades");
 Check(g.monsters.location==3&&g.monsters.mode==SpawnMode.SelectedSlot,"immediately behind owner");
 Check(g.monsters.spawned!.hp==28&&g.monsters.spawned.undying==0,"draw unit full health");
 Check(!g.cards.draw.Contains(one)&&g.cards.standby.Contains(one),"draw moved once to deployed");
 Check(g.cards.drawable&&g.combat.queued==1,"native playable lifecycle and other unit trigger");
 var consumed=Card(1);g.cards.exhausted.Add(consumed);RodericReinforcements.RecordReturn(consumed,new(){hp=20});
 var eaten=Card(1);g.cards.eaten.Add(eaten);RodericReinforcements.RecordReturn(eaten,new(){hp=0});
 list=RodericReinforcements.Candidates(g.cards,1,out revive);
 Check(revive&&list.Count==2&&list.Contains(dead)&&list.Contains(eaten)&&!list.Contains(consumed),"only actual deaths incl eaten");
 g.cards.eaten.Clear();g.cards.exhausted.Remove(consumed);
 Run(effect.ApplyEffect(state,args,g,systems));
 Check(g.monsters.card==dead&&g.monsters.spawned!.hp==1&&g.monsters.spawned.max==28,"revival current one max retained");
 Check(g.monsters.spawned!.undying==1&&!g.monsters.spawned.modified&&!g.monsters.spawned.dualism,"exact undying unaffected");
 Check(!g.cards.exhausted.Contains(dead)&&!RodericReinforcements.DeadCards.Contains(dead),"revival removes dead eligibility");
 Check(!effect.TestEffect(state,args,g),"empty candidates unusable");
 var purged=Card(0);RodericReinforcements.DeadCards.Add(purged);Check(RodericReinforcements.Candidates(g.cards,3,out _).Count==0,"purged cannot revive");
 g.cards.draw.Add(two);Check(!effect.TestEffect(state,args,g),"cost two excluded rank one");state.cost=2;Check(effect.TestEffect(state,args,g),"cost two allowed rank two");
 g.monsters.fail=true;Run(effect.ApplyEffect(state,args,g,systems));Check(g.cards.draw.Contains(two),"failed spawn leaves card intact");
 g.monsters.fail=false;var choice=Card(2);g.cards.draw.Add(choice);ScreenManager.pickLast=true;
 Run(effect.ApplyEffect(state,args,g,systems));
 Check(g.monsters.card==choice&&g.cards.draw.Contains(two),"player chooses second draw candidate");
 Check(ScreenManager.visible.SequenceEqual(new[]{two,choice}),"screen exposes exact eligible draw cards");
 Check(!ScreenManager.showing&&RodericReinforcements.SelectionCards.Count==0,"screen closes and scoped override cleaned");
 var deathA=Card(1);var deathB=Card(2);g.cards.draw.Clear();g.cards.exhausted.Add(deathA);g.cards.eaten.Add(deathB);RodericReinforcements.DeadCards.Add(deathA);RodericReinforcements.DeadCards.Add(deathB);
 Run(effect.ApplyEffect(state,args,g,systems));
 Check(g.monsters.card==deathB&&g.monsters.spawned!.hp==1,"player chooses eaten dead unit instead of first");
 Check(ScreenManager.visible.SequenceEqual(new[]{deathA,deathB}),"screen includes both actual dead piles");

 var fromHand=Card(1);var fromDiscard=Card(1);var fromDraw=Card(1);var expensive=Card(3);
 g.cards.hand.AddRange(new[]{fromHand,expensive});g.cards.draw.Add(fromDraw);g.cards.discard.Add(fromDiscard);
 list=RodericReinforcements.Candidates(g.cards,1,out revive);
 Check(!revive&&list.SequenceEqual(new[]{fromHand,fromDraw,fromDiscard}),"all three zones combined, base cost respected and no dead fallback");
 ScreenManager.desired=fromHand;Run(effect.ApplyEffect(state,args,g,systems));
 Check(g.monsters.card==fromHand&&g.monsters.spawned!.hp==28&&g.monsters.spawned.undying==0,"hand selection summoned normally");
 Check(!g.cards.hand.Contains(fromHand)&&g.cards.standby.Contains(fromHand),"native lifecycle removes hand card and deploys same instance");
 ScreenManager.desired=fromDiscard;Run(effect.ApplyEffect(state,args,g,systems));
 Check(g.monsters.card==fromDiscard&&!g.cards.discard.Contains(fromDiscard)&&g.cards.standby.Contains(fromDiscard),"discard selection removed without duplication");
 g.monsters.fail=true;ScreenManager.desired=fromDraw;Run(effect.ApplyEffect(state,args,g,systems));
 Check(g.cards.draw.Contains(fromDraw),"three-zone failed spawn preserves source");g.monsters.fail=false;
 g.cards.hand.Clear();g.cards.draw.Clear();g.cards.discard.Clear();ScreenManager.desired=null;
 list=RodericReinforcements.Candidates(g.cards,2,out revive);
 Check(revive&&list.Contains(deathA),"Not Yet only after all three living zones have no valid candidates");
 Check(RandomManager.calls==0,"all selection avoids RNG");
 Check(effect.RequiredScreenName==ScreenName.Deck,"native replay dialog interface");
 Check(effect.CanApplyInPreviewMode,"native preview targeting is allowed");
 Console.WriteLine($"PASS {count} semantic checks (actual effect source, simulated game API)");
 }
}
namespace HarmonyLib{[AttributeUsage(AttributeTargets.Class)]public class HarmonyPatch:Attribute{public HarmonyPatch(Type t,string n){}}}
public class CardState{public string GetTitleKey()=>"Title";public int cost,upgrade;public bool monster=true,champion;public bool IsMonsterCard()=>monster;public bool IsChampionCard()=>champion;public CharacterData GetSpawnCharacterData()=>new();public int GetCostWithoutAnyModifications()=>cost;}
public class CharacterData{}
public class CardPile{}
public static class Team{[Flags]public enum Type{Monsters=1,Heroes=2}}
public class SpawnPoint{public int index;public int GetIndexInRoom()=>index;}
public class RoomState{public int count=1,remaining=6;public int GetNumCharacters(Team.Type t)=>count;public int GetRemainingSpawnPointCount(Team.Type t)=>remaining;public SpawnPoint? GetMonsterPoint(int n)=>n<7?new(){index=n}:null;public void AddCharactersToList(List<CharacterState> list,Team.Type t){list.Add(new());}}
public class CharacterState{public bool IsDead;public int hp=28,max=28,undying;public bool modified,dualism;public RoomState room=new();public SpawnPoint point=new();public static object defaultAddStatusEffectParams=new();public int GetHP()=>hp;public int GetMaxHP()=>max;public RoomState GetCurrentRoom()=>room;public int GetCurrentRoomIndex()=>0;public SpawnPoint GetSpawnPoint()=>point;public void SetHealth(int h,int m){hp=h;max=m;}public void AddStatusEffect(string s,int n,object p,bool allowModification=true,bool allowDualism=true){undying=n;modified=allowModification;dualism=allowDualism;}}
public class CardEffectState{public int cost=1;public int GetParamInt()=>cost;public CardState? GetParentCardState()=>null;}
public class CardEffectParams{public CharacterState? characterThatActivatedAbility,selfTarget;public CardState? playedCard;}
public class PropDescriptions{}
public abstract class CardEffectBase{public virtual bool CanApplyInPreviewMode=>true;public virtual bool CanPlayAfterBossDead=>true;public abstract PropDescriptions CreateEditorInspectorDescriptions();public abstract bool TestEffect(CardEffectState s,CardEffectParams p,ICoreGameManagers m);public abstract IEnumerator ApplyEffect(CardEffectState s,CardEffectParams p,ICoreGameManagers m,ISystemManagers y);}
public interface ISystemManagers{ScreenManager GetScreenManager();}
public class Systems:ISystemManagers{public ScreenManager screen=new();public ScreenManager GetScreenManager()=>screen;}
public interface ICardEffectUiDialog{}
public enum ScreenName{Deck}
public enum TargetMode{Exhaust,DrawPile}
public enum CardType{Monster}
public interface IScreen{}
public class ScreenManager{public static List<CardState> visible=new();public static CardState? desired;public static bool showing;public static bool pickLast;public void SetScreenActive(ScreenName n,bool active,Action<IScreen>? setup=null){showing=active;if(active)setup!(new DeckScreen());}}
public class DeckScreen:IScreen{public enum Mode{CardEffectSelection}public class Params{public Mode mode;public TargetMode targetMode;public CardType cardTypeFilter;public bool showCancel,ignoreDefaultFilters,excludeFilteredOutCards;public string? titleKey;}public void Setup(Params p){var patch=typeof(RodericReinforcements).GetNestedType("SelectionCardsPatch",System.Reflection.BindingFlags.NonPublic)!;var method=patch.GetMethod("Prefix",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static)!;object?[] args={this,null};if((bool)method.Invoke(null,args)!)throw new Exception("candidate screen not scoped");ScreenManager.visible=(List<CardState>)args[1]!;}public void AddDeckScreenCardStateChosenDelegate(Action<CardState> done){done(ScreenManager.desired??(ScreenManager.pickLast?ScreenManager.visible.Last():ScreenManager.visible.First()));}}
public interface ICoreGameManagers{CardManager GetCardManager();SaveManager GetSaveManager();MonsterManager GetMonsterManager();CombatManager GetCombatManager();}
public class Managers:ICoreGameManagers{public CardManager cards=new();public SaveManager save=new();public MonsterManager monsters=new();public CombatManager combat=new();public RoomState room=new();public CardManager GetCardManager()=>cards;public SaveManager GetSaveManager()=>save;public MonsterManager GetMonsterManager()=>monsters;public CombatManager GetCombatManager()=>combat;}
public class SaveManager{public bool PreviewMode;}
public class AllGameManagers{public static AllGameManagers? Instance;public bool AreMainSceneOnlyManagersInitialized()=>true;public ICoreGameManagers GetCoreManagers()=>new Managers();}
public class CardManager{public List<CardState> hand=new(),discard=new(),draw=new(),exhausted=new(),eaten=new(),standby=new();public bool drawable;public List<CardState> GetHand()=>hand;public List<CardState> GetDiscardPile()=>discard;public List<CardState> GetDrawPile()=>draw;public List<CardState> GetExhaustedPile()=>exhausted;public List<CardState> GetEatenPile()=>eaten;public bool IsCardInStandByPile(CardState c)=>standby.Contains(c);public void SetupCards(){}public void OnMonsterCardReturned(){}public class DiscardCardParams{public CardState discardCard=null!;public bool wasPlayed;public CharacterState characterSummoned=null!;}public IEnumerator DiscardCard(DiscardCardParams p){hand.Remove(p.discardCard);discard.Remove(p.discardCard);standby.Add(p.discardCard);yield break;}public void SetCardIsDrawable(CardState c,bool drawable){this.drawable=drawable;}}
public enum SpawnMode{SelectedSlot}
public class MonsterManager{public CardState? card;public CharacterState? spawned;public int location;public SpawnMode mode;public bool fail;public IEnumerator CreateMonsterState(CharacterData data,CardState card,int room,Action<CharacterState> callback,SpawnMode spawnMode,SpawnPoint? spawnLocation,Action<CharacterState>? afterCharacterSetup=null,CardState? fromPlayedCard=null){if(fail)yield break;this.card=card;location=spawnLocation!.index;mode=spawnMode;spawned=new();afterCharacterSetup?.Invoke(spawned);callback(spawned);yield break;}}
public static class CharacterTriggerData{public enum Trigger{CardMonsterPlayed}}
public class CombatManager{public int queued;public void QueueTrigger(CharacterState c,CharacterTriggerData.Trigger t){queued++;}}
public enum RngId{CardDraw}
public static class RandomManager{public static int calls;public static int Range(int a,int b,RngId id){calls++;return a;}}

// 2026-10-04-0558||codex-freecompany-fx||salidas/dll/freecompany-roderic-tests/Program.cs||arnés semántico del efecto real: prioridad, coste, muerte real, siete unidades, preview y movimiento de carta

// 2026-10-04-0613||codex-freecompany-fx||salidas/dll/freecompany-roderic-tests/Program.cs||pruebas de selector nativo: elección segunda carta, muertos devorados, ámbito del parche y sin RNG

// 2026-10-08-2258||codex-freecompany-fx||D:\Juegos\MT2_mod\salidas\dll\freecompany-roderic-tests\Program.cs||prueba la guarda nativa TestEffect+CanApplyInPreviewMode y ausencia de efectos durante simulación

// 2026-10-09-1435||codex-freecompany-fx||D:\Juegos\MT2_mod\salidas\dll\freecompany-roderic-tests\Program.cs||prueba selección combinada mano/robo/descarte, movimiento nativo, coste y fallback

// 2026-11-07-1203||codex-freecompany-fx||tests/call-to-arms/Program.cs||consolida arnés existente en repositorio; enlaza fuentes locales canónicas, conserva original de laboratorio
