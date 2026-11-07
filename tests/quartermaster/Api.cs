// Documento generado el 2026-10-29-1405
using System.Collections;
public class PropDescriptions {}
public class CardEffectBase {
 public virtual PropDescriptions CreateEditorInspectorDescriptions()=>new();
 public virtual bool CanPlayWhenHandFull=>true;
 public virtual bool TestEffect(CardEffectState s,CardEffectParams p,ICoreGameManagers g)=>true;
 public virtual IEnumerator ApplyEffect(CardEffectState s,CardEffectParams p,ICoreGameManagers g,ISystemManagers systems){yield break;}
}
public class CardEffectState {public int tier=1;public int GetParamInt()=>tier;}
public class CardEffectParams {public List<CharacterState> targets=[];}
public interface ISystemManagers {}
public enum CardPile {None,HandPile,DeckPile,ExhaustedPile,KeepInStandBy,DiscardPile}
public static class Team {public enum Type {Monsters,Heroes}}
public class SubtypeData {public string Key="";}
public class CardData {public string name="";}
public class CardUpgradeData {public string name="";public string description="";}
public class CardUpgradeState {
 CardUpgradeData data=new();public void Setup(CardUpgradeData d)=>data=d;
 public string GetUpgradeDescriptionKey()=>data.description;public string GetUpgradeTitle()=>data.name;
}
public class CardState {
 public string asset="";public HashSet<CardUpgradeData> upgrades=[];public Dictionary<string,int> modifiers=[];public List<CardState> merged=[];
 public string GetAssetName()=>asset;public bool HasUpgrade(CardUpgradeData u)=>upgrades.Contains(u);
}
public class CharacterState {
 public bool PreviewMode,IsDestroyed,IsDead;public int hp=10;public Team.Type team=Team.Type.Monsters;
 public CardState? card;public List<SubtypeData> subtypes=[];public HashSet<CardUpgradeData> upgrades=[];public List<CardState> equipment=[];public HashSet<string> statuses=[];public List<string> notifications=[];
 public int GetHP()=>hp;public Team.Type GetTeamType()=>team;public CardState? GetSpawnerCard()=>card;
 public bool HasUpgrade(CardUpgradeData u)=>upgrades.Contains(u);public bool HasStatusEffect(string s)=>statuses.Contains(s);
 public List<SubtypeData> GetSubtypes()=>subtypes;public List<CardState> GetEquipment()=>equipment;
 public void ShowNotification(string s,PopupNotificationUI.Source source)=>notifications.Add(s);
 public IEnumerator OnSpawn(CardState? fromCard=null){yield break;}
}
public class MonsterManager {public List<CharacterState> units=[];public void AddCharactersToList(List<CharacterState> l)=>l.AddRange(units);}
public class CardManager {
 public List<CardState> hand=[],deck=[];public List<CardData> added=[];public int max=10;public Action? onAdd;public bool reject;
 public bool GetBelowHandSize()=>hand.Count<max;
 public CardState? AddNewCard(CardData d,CardPile pile,bool relic,bool permanent,object? upgrade,bool animate) {
  if(reject)return null;var c=new CardState{asset=d.name};added.Add(d);if(pile==CardPile.HandPile)hand.Add(c);else deck.Add(c);onAdd?.Invoke();return c;
 }
 public CardPile CheckEquipmentRemoveFromStandByCondition(CharacterState character,CardState equipmentCard)=>CardPile.ExhaustedPile;
 public void SetupCards(){}
}
public class AllGameData {public List<CardUpgradeData> upgrades=[];public List<CardData> cards=[];public List<CardUpgradeData> GetAllCardUpgradeData()=>upgrades;public List<CardData> GetAllCardData()=>cards;}
public class SaveManager {public bool PreviewMode;public AllGameData data=new();public AllGameData GetAllGameData()=>data;}
public class CombatManager {public int turn=1;public int GetTurnCount()=>turn;}
public interface ICoreGameManagers {SaveManager GetSaveManager();AllGameData GetAllGameData();CardManager GetCardManager();MonsterManager GetMonsterManager();CombatManager GetCombatManager();}
public class Managers:ICoreGameManagers {
 public SaveManager save=new();public AllGameData data=new();public CardManager cards=new();public MonsterManager monsters=new();public CombatManager combat=new();
 public Managers(){save.data=data;}
 public SaveManager GetSaveManager()=>save;public AllGameData GetAllGameData()=>data;public CardManager GetCardManager()=>cards;public MonsterManager GetMonsterManager()=>monsters;public CombatManager GetCombatManager()=>combat;
}
public class AllGameManagers {public static AllGameManagers? Instance;public Managers core=new();public bool AreMainSceneOnlyManagersInitialized()=>true;public ICoreGameManagers GetCoreManagers()=>core;}
public static class PopupNotificationUI {public enum Source{Trigger}}
public static class CharacterTooltipHelper {public static IEnumerable<(TooltipContent content,bool allowDuplicates)> GetAdditionalTooltipContents(CharacterState characterState,SaveManager saveManager)=>[];}
public class TooltipContent {public string tooltipId,body;public TooltipContent(string title,string body,TooltipDesigner.TooltipDesignType type,string id){this.body=body;tooltipId=id;}}
public static class TooltipDesigner {public enum TooltipDesignType{Keyword}}
public class CardEffectLocalizationContext {public CardEffectLocalizationContext(CardUpgradeData u,object? a,CardState? c){}}
public static class Loc {public static string Localize(this string text,object? context=null)=>text;}
public enum RngId{Battle,BattleTest}
public static class RandomManager {public static int calls;public static Random random=new(1);public static void Reset(int seed){calls=0;random=new(seed);}public static int Range(int min,int max,RngId id){calls++;return random.Next(min,max);}}
namespace HarmonyLib {[AttributeUsage(AttributeTargets.Class|AttributeTargets.Method,AllowMultiple=true)]public class HarmonyPatch:Attribute {public HarmonyPatch(Type type,string method){}}}
namespace mt2_freecompany.Plugin {public static class MyPluginInfo {public const string PLUGIN_GUID="mt2_freecompany.Plugin";}public class Logger {public void LogInfo(string s){}}public class Plugin {public static Logger Logger=new();}}

// 2026-10-29-1405||codex-freecompany-fx||salidas/build/freecompany-quartermaster-2026-10-29-1402/tests/Api.cs||pruebas de Quartermaster: limites por turno, preview, identidad de equipo, fusiones, mano llena y coexistencia

// 2026-11-07-1203||codex-freecompany-fx||tests/quartermaster/Api.cs||consolida arnés existente en repositorio; enlaza fuentes locales canónicas, conserva original de laboratorio
