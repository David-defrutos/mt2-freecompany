// Documento generado el 2026-10-29-1955
using System.Collections;
using System.Reflection;
using mt2_freecompany.Plugin;
public static class Bridge {
 public static object? Call(string cls,string method,params object?[] args)=>typeof(ShieldwallInterception).GetNestedType(cls,BindingFlags.NonPublic)!.GetMethod(method,BindingFlags.NonPublic|BindingFlags.Static)!.Invoke(null,args);
 public static IEnumerator Empty(){yield break;}
 public static void Run(IEnumerator e){try{while(e.MoveNext()){if(e.Current is IEnumerator child)Run(child);}}finally{(e as IDisposable)?.Dispose();}}
 public static bool ContextEmpty()=>typeof(ShieldwallInterception).GetField("current",BindingFlags.NonPublic|BindingFlags.Static)!.GetValue(null)==null;
}
public static class Team {public enum Type{Monsters,Heroes}}
public static class Damage {public enum Type{Default,DirectAttack,Trample,Splash,Spikes,Poison,Decay}public static bool CheckTypeCompatibleWithOnAttackingTrigger(Type t)=>t is Type.DirectAttack or Type.Trample or Type.Splash;}
public class CardUpgradeData {public string name="";}
public class CardState {public string asset="";public HashSet<CardUpgradeData> upgrades=[];public string GetAssetName()=>asset;public bool HasUpgrade(CardUpgradeData u)=>upgrades.Contains(u);}
public class PlayerManager {}
public class CardStatistics {}
public class SpawnPoint {public int index;public int GetIndexInRoom()=>index;}
public class CharacterUI {public enum AnimNote {TakeDamage}public void OnAnimEvent(AnimNote n){}}
public class RoomState {public List<CharacterState> units=[];public int bonus;public void AddCharactersToList(List<CharacterState> list,Team.Type team)=>list.AddRange(units.Where(u=>u.team==team));public int GetRoomStateModifiedDamage(CharacterState attacker,Damage.Type type,bool request,ICoreGameManagers managers)=>bonus;}
public class CharacterState {
 public struct ApplyDamageParams {public CharacterState? attacker;public Damage.Type damageType;public bool fromAttractDamageTrigger;}
 public bool PreviewMode,IsDestroyed;public int hp=100,previewHp=100;public Dictionary<string,int> stacks=[],previewStacks=[];
 public int HP {get=>PreviewMode?previewHp:hp;set{if(PreviewMode)previewHp=value;else hp=value;}}
 public Dictionary<string,int> Status=>PreviewMode?previewStacks:stacks;
 public bool IsDead=>HP<=0;public int GetHP()=>HP;public bool HasStatusEffect(string id)=>Status.GetValueOrDefault(id)>0;
 public Team.Type team=Team.Type.Monsters;public RoomState room=new();public SpawnPoint point=new();public CardState? card;
 public HashSet<CardUpgradeData> upgrades=[];public int lastBeforeDamage;public int before,attacks,debuffs,slays,healed,revenge,spikesReturned,bonusCalls,crit=1;public bool shieldPierced,throwInShield;
 public CharacterState? nestedTarget;public bool nestedOnce;
 public Team.Type GetTeamType()=>team;public RoomState GetCurrentRoom()=>room;public SpawnPoint GetSpawnPoint()=>point;
 public CardState? GetSpawnerCard()=>card;public bool HasUpgrade(CardUpgradeData u)=>upgrades.Contains(u);
 public CharacterState GetCharacterStateThatHasMesh()=>this;public CharacterUI GetCharacterUI()=>new();
 public Action<CharacterState>? beforeAction;
 public IEnumerator AttackingBeforeDamageForTests(CharacterState target,Damage.Type type,int damage)=>AttackingBeforeDamage(target,type,damage);
 private IEnumerator AttackingBeforeDamage(CharacterState target,Damage.Type type,int attackDamageDealt){
  IEnumerator Native(){lastBeforeDamage=attackDamageDealt;before++;beforeAction?.Invoke(target);yield break;}
  object?[] a={this,target,type,new object[]{target,type,attackDamageDealt},Native()};return (bool)Bridge.Call("BeforeAttackPatch","Prefix",a)!?Native():(IEnumerator)a[4]!;
 }
 public IEnumerator ApplyDamage(int damage,ApplyDamageParams damageParams,PlayerManager playerManager,CardStatistics cardStatistics){
  IEnumerator original=NativeDamage(damage,damageParams,playerManager,cardStatistics);object?[] a={this,damageParams,playerManager,cardStatistics,original};Bridge.Call("DamageScopePatch","Postfix",a);return (IEnumerator)a[4]!;
 }
 // Simulated native pipeline in installed DLL order: bonus, shield stage,
 // defenses, HP, attack callbacks, OnAttacked, OnHit, then Undying/death.
 private IEnumerator NativeDamage(int damage,ApplyDamageParams p,PlayerManager player,CardStatistics statistics){
  if(IsDead||IsDestroyed)yield break;
  damage+=p.attacker==null?0:room.bonus;
  object?[] calc={this,0};bool calculate=(bool)Bridge.Call("SingleCalculationPatch","Prefix",calc)!;
  int raw;
  if(calculate){if(p.attacker!=null)p.attacker.bonusCalls++;raw=damage*(p.attacker?.crit??1);}else raw=(int)calc[1]!;
  var input=new StatusEffectState.InputTriggerParams{attacker=p.attacker,damageType=p.damageType};var output=new StatusEffectState.OutputTriggerParams{damage=Math.Max(0,raw)};
  IEnumerator Shield(){
   if(throwInShield)throw new Exception("native stage error");
   if(Status.GetValueOrDefault("damage shield")>0&&output.damage>0&&!(p.attacker?.shieldPierced??false)){Status["damage shield"]--;output.damageBlocked+=output.damage;output.damage=0;}
   yield break;
  }
  object?[] stage={StatusEffectData.TriggerStage.OnPreAttackedDamageShield,this,input,output,Shield()};Bridge.Call("ShieldStagePatch","Postfix",stage);yield return (IEnumerator)stage[4]!;
  if(nestedTarget!=null&&!nestedOnce){nestedOnce=true;yield return nestedTarget.ApplyDamage(3,new(){damageType=Damage.Type.Poison},player,statistics);}
  if(output.damage>0&&!(p.attacker?.shieldPierced??false)){
   int titan=Math.Min(output.damage,Status.GetValueOrDefault("titanskin"));output.damage-=titan;output.damageBlocked+=titan;
   int armor=Math.Min(output.damage,Status.GetValueOrDefault("armor"));output.damage-=armor;output.damageBlocked+=armor;if(armor>0)Status["armor"]-=armor;
  }
  if(output.damage>0&&HasStatusEffect("fragile"))output.damage=HP;
  if(HasStatusEffect("untouchable")){output.damage=0;output.damageBlocked=0;}
  if(p.attacker!=null)yield return p.attacker.AttackingBeforeDamageForTests(this,p.damageType,Math.Min(HP,output.damage));
  int sustained=Math.Min(HP,output.damage);HP=Math.Max(0,HP-output.damage);
  if(p.attacker!=null){object?[] a={p.attacker,this,output.damage,sustained,output.damageBlocked,raw,0,Bridge.Empty()};if((bool)Bridge.Call("AttackEffectsPatch","Prefix",a)!){p.attacker.attacks++;debuffs++;p.attacker.healed+=(int)a[3]!;if(IsDead)p.attacker.slays++;}else yield return (IEnumerator)a[7]!;}
  object?[] reactions={this,Bridge.Empty()};if((bool)Bridge.Call("OriginalTargetReactionsPatch","Prefix",reactions)!){if(p.attacker!=null&&Status.GetValueOrDefault("spikes")>0){p.attacker.HP-=Status["spikes"];spikesReturned++;}}else yield return (IEnumerator)reactions[1]!;
  if(output.damage>0||output.damageBlocked>0)revenge++;
  if(IsDead&&Status.GetValueOrDefault("undying")>0){Status["undying"]--;HP=1;}
 }
}
public class AllGameData {public List<CardUpgradeData> upgrades=[];public List<CardUpgradeData> GetAllCardUpgradeData()=>upgrades;}
public class SaveManager {public AllGameData data=new();public AllGameData GetAllGameData()=>data;}
public class CombatManager {public IEnumerator QueueAndRunTrigger(CharacterState owner,CharacterTriggerData.Trigger trigger,CharacterState killed){owner.slays++;yield break;}}
public static class CharacterTriggerData {public enum Trigger {OnKill}}
public interface ICoreGameManagers {AllGameData GetAllGameData();CombatManager GetCombatManager();}
public class Managers:ICoreGameManagers {public AllGameData data=new();public CombatManager combat=new();public AllGameData GetAllGameData()=>data;public CombatManager GetCombatManager()=>combat;}
public class AllGameManagers {public static AllGameManagers? Instance;public Managers managers=new();public bool AreMainSceneOnlyManagersInitialized()=>true;public ICoreGameManagers GetCoreManagers()=>managers;}
public class StatusEffectState {public class InputTriggerParams {public CharacterState? attacker;public Damage.Type damageType;}public class OutputTriggerParams {public int damage,damageBlocked,visualDamage;}}
public static class StatusEffectData {public enum TriggerStage {OnPreAttackedDamageShield,OnPreAttackedArmor}}
public class StatusEffectManager {public IEnumerator TriggerStatusEffectOnUnit(StatusEffectData.TriggerStage stage,CharacterState unit,StatusEffectState.InputTriggerParams input,StatusEffectState.OutputTriggerParams output,bool runTriggerQueue=true){yield break;}}
public class TooltipContent {public string tooltipId;public TooltipContent(string title,string body,TooltipDesigner.TooltipDesignType type,string id){tooltipId=id;}}
public static class TooltipDesigner {public enum TooltipDesignType {Keyword}}
public class CardTooltipContainer {public List<TooltipContent> shown=[];public bool HasTooltip(string id)=>shown.Any(t=>t.tooltipId==id);public void ShowTooltip(TooltipContent t)=>shown.Add(t);}
public static class CharacterTooltipHelper {public static IEnumerable<(TooltipContent content,bool allowDuplicates)> GetAdditionalTooltipContents(CharacterState unit,SaveManager save)=>[];}
public static class Localization {public static string Localize(this string s)=>s;}
namespace HarmonyLib {[AttributeUsage(AttributeTargets.Class|AttributeTargets.Method,AllowMultiple=true)]public class HarmonyPatch:Attribute {public HarmonyPatch(Type t,string method){}}public static class AccessTools {public static bool FailBinding;public static MethodInfo Method(Type t,string name)=>t.GetMethod(name,BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic)!;public static T MethodDelegate<T>(MethodInfo m) where T:Delegate=>FailBinding?throw new ArgumentException("simulated incompatible API"):m.CreateDelegate<T>();}}
namespace mt2_freecompany.Plugin {public static class MyPluginInfo {public const string PLUGIN_GUID="mt2_freecompany.Plugin";}}

// 2026-10-29-1955||codex-freecompany-fx||salidas/build/freecompany-shieldwall-2026-10-29-1500/tests/Api.cs||regresiones Intercepcion con fuente real y pipeline simulado en orden nativo, preview, escudos, muerte y efectos originales

// 2026-10-29-1957||codex-freecompany-fx||salidas/build/freecompany-shieldwall-2026-10-29-1500/tests/Api.cs||simula llamada nativa Before Damage y enlace delegado para verificar orden real sin duplicacion

// 2026-11-07-1203||codex-freecompany-fx||tests/shieldwall/Api.cs||consolida arnés existente en repositorio; enlaza fuentes locales canónicas, conserva original de laboratorio

namespace mt2_freecompany.Plugin { public static class Plugin { public static TestLogger Logger=new(); } public sealed class TestLogger { public void LogError(object message) {} } }

// 2026-11-12-0901||codex-freecompany-fx||tests/shieldwall/Api.cs||actualiza API simulada a firma de juego con attackDamageDealt para reproducir fallo reportado

// 2026-11-12-0901||codex-freecompany-fx||tests/shieldwall/Api.cs||añade prueba de enlace fallido sin daño global y elimina aviso nullable
