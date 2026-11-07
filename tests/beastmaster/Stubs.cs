
using System.Collections;
public enum CardType { Monster, Spell }
public class CardUpgradeData { public string name="";public string id=""; }
public class CardUpgradeState {
 public string id=""; public int attack,hp,cost; public void Setup(){}public bool LoadFromFile(AllGameData data,SaveManager save)=>data.FindCardUpgradeData(id)!=null; public string title="";public string body="";public void Setup(CardUpgradeData data){id=data.name;title=data.name;body=data.name+" rules";}public string GetUpgradeTitle()=>title;public string GetUpgradeDescriptionKey()=>body; public void SetCardUpgradeDataId(string x)=>id=x; public string GetCardUpgradeDataId()=>id; public void SetIsUnique(bool x){}
 public void SetAttackDamage(int x)=>attack=x; public int GetAttackDamage()=>attack; public void SetAdditionalHP(int x)=>hp=x; public int GetAdditionalHP()=>hp; public void SetCostReduction(int x)=>cost=x; public int GetCostReduction()=>cost;
}
public class Modifiers { public List<CardUpgradeState> upgrades=new(); public List<CardUpgradeState> GetCardUpgrades()=>upgrades; }
public class CardState {
 public string name; public CardType type=CardType.Monster; public bool frozen; public Modifiers permanent=new(),temp=new(); public HashSet<string> paths=new();
 public CardState(string n)=>name=n; public CardType GetCardType()=>type; public string GetAssetName()=>name; public string GetCardDataID()=>"GUID-does-not-match-name";
 public bool HasTrait(Type t)=>frozen; public bool HasUpgrade(CardUpgradeData u)=>paths.Contains(u.name); public Modifiers GetCardStateModifiers()=>permanent;public Modifiers GetTemporaryCardStateModifiers()=>temp;
 public void RemoveUpgrade(CardUpgradeState u,Modifiers m)=>m.upgrades.Remove(u);public void ApplyPermanentUpgrade(CardUpgradeState u,SaveManager s,bool ignoreUpgradeAnimation=false)=>permanent.upgrades.Add(u);public void ApplyTemporaryUpgrade(CardUpgradeState u,SaveManager s)=>temp.upgrades.Add(u);public void UpdateCardBodyText(){}
}
public class CardTraitFreeze {} public class CardTraitData {public void Setup(string x){}}
public class CharacterState {public bool canAttack=true;public RoomState? room=new();public bool CheckAttackConditions()=>!IsDead&&hp>0&&damage>0&&canAttack;public RoomState? GetCurrentRoom()=>room;public int damage=7,hp=5,maxHp=10;public int GetHP()=>hp;public void BuffDamage(int n)=>damage+=n;public IEnumerator BuffMaxHP(int n,bool triggerOnHeal=true){maxHp+=n;hp+=n;yield break;}public bool IsDead;public bool muted;public CardState card;public CharacterState(int level){card=new("mt2_freecompany.Plugin-Card-SpawnVesper");card.paths.Add("mt2_freecompany.Plugin-Upgrade-upg_Beastmaster"+level);}public CardState GetSpawnerCard()=>card;public HashSet<string> statuses=new();public int statusAdds;public bool HasStatusEffect(string x)=>statuses.Contains(x)||((x=="muted"||x=="silenced")&&muted);public static readonly object defaultAddStatusEffectParams=new();public void AddStatusEffect(string id,int stacks,object p,bool allowModification,bool allowDualism,bool fromPermanentUpgrade,bool skipDefaultStatusEffectNotifs){if(allowModification||allowDualism||!fromPermanentUpgrade||!skipDefaultStatusEffectNotifs)throw new Exception("marker flags");statuses.Add(id);statusAdds++;}public IEnumerator OnSpawn(){yield break;}public bool HasUpgrade(CardUpgradeData u)=>false;}
public class SaveManager {public List<CardState> runDeck=new();public List<CardState> GetDeckState()=>runDeck;public bool PreviewMode;public AllGameData? data;public AllGameData GetAllGameData()=>data??AllGameManagers.Core.data;}
public class MonsterManager {public List<CharacterState> characters=new();public void AddCharactersToList(List<CharacterState> list)=>list.AddRange(characters);}
public class AllGameData {public List<CardUpgradeData> upgrades=Enumerable.Range(1,3).Select(i=>new CardUpgradeData{name="mt2_freecompany.Plugin-Upgrade-upg_Beastmaster"+i}).ToList();public List<CardUpgradeData> GetAllCardUpgradeData()=>upgrades;public CardUpgradeData? FindCardUpgradeData(string id)=>upgrades.FirstOrDefault(u=>u.id==id);}
public class CardManager {public List<CardState> hand=new(),deck=new(),cemetery=new();public bool full;public List<CardState> GetHand()=>hand;public bool IsCardInHand(CardState c)=>hand.Contains(c);public List<CardState> GetDrawPile()=>deck;public void AddTemporaryTraitToCard(CardState c,CardTraitData t)=>c.frozen=true;public void RefreshCardInHand(CardState c,bool cleanupTweens=false){}public bool DrawSpecificCard(CardState c,HandUI.DrawSource drawSource=HandUI.DrawSource.Deck){if(full)return false;deck.Remove(c);hand.Add(c);return true;}}
public class HandUI {public enum DrawSource{Deck}}
public interface ICoreGameManagers {SaveManager GetSaveManager();CardManager GetCardManager();MonsterManager GetMonsterManager();AllGameData GetAllGameData();}
public class Managers:ICoreGameManagers {public SaveManager save=new();public CardManager cards=new();public MonsterManager monsters=new();public AllGameData data=new();public SaveManager GetSaveManager()=>save;public CardManager GetCardManager()=>cards;public MonsterManager GetMonsterManager()=>monsters;public AllGameData GetAllGameData()=>data;}
public class AllGameManagers {public static AllGameManagers? Instance;public static Managers Core=new();public bool initialized=true;public bool missingCardManager;public int coreCalls;public bool AreMainSceneOnlyManagersInitialized()=>initialized;public CardManager? GetCardManager()=>missingCardManager?null:Core.cards;public ICoreGameManagers GetCoreManagers(){coreCalls++;if(!initialized)throw new Exception("combat managers accessed outside combat");return Core;}}
public class CombatManager {} public class RelicManager {public void ApplyCardAddedToHandRelicEffects(CardState cardState){}}
public class CardEffectState {} public class CardEffectParams {public List<CharacterState>? targets;} public interface ISystemManagers {} public class PropDescriptions {}
public abstract class CardEffectBase {public virtual bool CanApplyInPreviewMode=>true;public abstract PropDescriptions CreateEditorInspectorDescriptions();public abstract bool TestEffect(CardEffectState s,CardEffectParams p,ICoreGameManagers m);public abstract IEnumerator ApplyEffect(CardEffectState s,CardEffectParams p,ICoreGameManagers m,ISystemManagers y);}
namespace HarmonyLib { [AttributeUsage(AttributeTargets.Class)]public class HarmonyPatch:Attribute {public HarmonyPatch(){}public HarmonyPatch(Type t,string n){}}[AttributeUsage(AttributeTargets.Method)]public class HarmonyPostfix:Attribute{} }
namespace mt2_freecompany.Plugin {public class Plugin {} public class MyPluginInfo {public const string PLUGIN_GUID="mt2_freecompany.Plugin";}}

// 2026-10-03-2249||codex-freecompany-fx||salidas/dll/freecompany-beastmaster-tests/Stubs.cs||arnés semántico del código real Beastmaster; persistencia, múltiples Vesper, pilas, preview y recuperación

public class CardUI {public CardState? card;public CardState? GetCardState()=>card;}
public class CardUpgradeDisplayContainer {public CardUI ui=new();public T GetComponentInParent<T>() where T:class=>ui as T;public void DisplayUpgrades(List<CardUpgradeState> upgrades,List<CardUpgradeState> temporaryUpgrades,int minimumSlotCount=0){}}
public class CardTooltipContainer {public void AddTooltipsCardUpgrade(CardState c,CardUpgradeState u,SaveManager s){}}
public abstract class StatusEffectState {public class InputTriggerParams{}public class OutputTriggerParams{}public virtual bool TestTrigger(InputTriggerParams i,OutputTriggerParams o,ICoreGameManagers m)=>true;}

// 2026-10-03-2352||codex-freecompany-fx||D:\Juegos\MT2_mod\salidas\dll\freecompany-beastmaster-tests\Stubs.cs||amplía arnés para indicadores sin tocar mejoras de cartas

// 2026-10-04-0016||codex-freecompany-fx||D:\Juegos\MT2_mod\salidas\dll\freecompany-beastmaster-tests\Stubs.cs||simula tienda sin escena de combate y gestor de cartas ausente

// 2026-10-04-0028||codex-freecompany-fx||D:\Juegos\MT2_mod\salidas\dll\freecompany-beastmaster-tests\Stubs.cs||simula resolución de upgrades al cargar partidas

public struct TooltipContent {public string title,body,tooltipId;public TooltipContent(string t,string b,TooltipDesigner.TooltipDesignType d,string id){title=t;body=b;tooltipId=id;}}
public class TooltipDesigner {public enum TooltipDesignType{Keyword}}
public class CharacterTooltipHelper {public static void GetAdditionalTooltipContents(){}}
public class CardEffectLocalizationContext {public CardEffectLocalizationContext(CardUpgradeData d,object? effect,CardState? c){}}
namespace ShinyShoe {public static class LocalizationStub {public static string Localize(this string value,CardEffectLocalizationContext context)=>value;}}

// 2026-10-04-0044||codex-freecompany-fx||D:\Juegos\MT2_mod\salidas\dll\freecompany-beastmaster-tests\Stubs.cs||simula tooltip nativo localizado de senda del campeón

// 2026-10-08-1936||codex-freecompany-fx||D:\Juegos\MT2_mod\salidas\dll\freecompany-beastmaster-tests\Stubs.cs||API simulada de bono directo de ataque y vida a unidades desplegadas

public class RoomState {public int enemies=1;public int GetNumCharacters(Team.Type t)=>enemies;}
public class Team {public enum Type{Heroes,Monsters}}
public class CardEffectAttackWithUnit:CardEffectBase {
 public override PropDescriptions CreateEditorInspectorDescriptions()=>new();
 public override bool TestEffect(CardEffectState s,CardEffectParams p,ICoreGameManagers m)=>true;
 public override IEnumerator ApplyEffect(CardEffectState s,CardEffectParams p,ICoreGameManagers m,ISystemManagers y){yield break;}
}

// 2026-10-08-1948||codex-freecompany-fx||D:\Juegos\MT2_mod\salidas\dll\freecompany-beastmaster-tests\Stubs.cs||añade API mínima de selección de unidad y piso para pruebas de Command

public class CharacterUI {public enum Anim{None,Idle,Attack,HitReact,Idle_Relentless,Attack_Spell,Death,Talk,Hover}}
namespace UnityEngine {
 public class Component {public Transform transform=new();}
 public class Transform {public string name="";public Transform? parent;}
}
namespace ShinyShoe {public class CharacterUIMeshBase:UnityEngine.Component {public bool returnedToIdle;public void PlayAnimLoop(CharacterUI.Anim animation,float start){returnedToIdle=animation==CharacterUI.Anim.Idle;}}}
namespace HarmonyLib {public static class AccessTools {
 public static Type? TypeByName(string n)=>typeof(AccessTools).Assembly.GetType(n);
 public static System.Reflection.FieldInfo? Field(Type t,string n)=>t.GetField(n,System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.Instance);
 public static System.Reflection.MethodInfo? Method(Type t,string n)=>t.GetMethod(n,System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.Instance);
}}
namespace TrainworksReloaded.Base.Prefab {public class CharacterUIMeshAnimatedSprite:ShinyShoe.CharacterUIMeshBase {
 public bool _isPlaying;public CharacterUI.Anim _currentAnimType;public float GetAnimSpeedMultiplier()=>8f;public void PlayAnimInternal(){}public void Update(){}
}}

// 2026-10-08-2032||codex-freecompany-fx||D:\Juegos\MT2_mod\salidas\dll\freecompany-beastmaster-tests\Stubs.cs||API simulada de renderer opcional y estado animado para validar VesperAnimationTiming

// 2026-10-27-2205||codex-freecompany-fx||D:\Juegos\MT2_mod\salidas\dll\freecompany-beastmaster-tests\Stubs.cs||H1: simula mazo permanente nativo separado del tablero

// 2026-10-27-2205||codex-freecompany-fx||D:\Juegos\MT2_mod\salidas\dll\freecompany-beastmaster-tests\Stubs.cs||H1: modelo usa identidad registrada real de la carta Vesper

// 2026-11-07-1203||codex-freecompany-fx||tests/beastmaster/Stubs.cs||consolida arnés existente en repositorio; enlaza fuentes locales canónicas, conserva original de laboratorio
