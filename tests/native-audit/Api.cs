using System.Collections;
public class PropDescriptions {}
public class CardEffectBase {
 public virtual PropDescriptions CreateEditorInspectorDescriptions()=>new();
 public virtual bool CanPlayWhenHandFull=>true;
 public virtual bool TestEffect(CardEffectState e,CardEffectParams p,ICoreGameManagers g)=>true;
 public virtual IEnumerator ApplyEffect(CardEffectState e,CardEffectParams p,ICoreGameManagers g,ISystemManagers s){yield break;}
 public virtual bool CanApplyInPreviewMode=>true;
}
public class CardEffectDamage:CardEffectBase {}
public class StatusEffectState {
 public class InputTriggerParams{} public class OutputTriggerParams{}
 public CharacterState? associated;public int fallback;
 public virtual bool TestTrigger(InputTriggerParams i,OutputTriggerParams o,ICoreGameManagers g)=>false;
 public virtual int GetEffectMagnitude(int stacks=1)=>0;
 protected CharacterState? GetAssociatedCharacter()=>associated;
 protected string GetStatusId()=>"fuse";
 protected int GetParamInt()=>fallback;
}
public class StatusEffectStackData{public string statusId="fuse";public int count=2;}
public class CardEffectState {
 public int value;public StatusEffectStackData[] stacks=[new()];
 public int GetParamInt()=>value;public StatusEffectStackData[] GetParamStatusEffectStackData()=>stacks;
}
public class CardEffectParams{public List<CharacterState> targets=[];public CardState? playedCard;}
public class CardState{}
public class CardData{public string name="";public string GetID()=>name;}
public class SubtypeData{public string Key="";}
public class CharacterState {
 public bool IsDestroyed,PreviewMode,RejectStatus;public bool IsAlive=true;public int floor;
 public bool IsDead=>!IsAlive;public Dictionary<string,int> statuses=[];public List<SubtypeData> subtypes=[];
 public int GetStatusEffectStacks(string id)=>statuses.GetValueOrDefault(id);
 public void AddStatusEffect(string id,int n){if(!RejectStatus)statuses[id]=GetStatusEffectStacks(id)+n;}
 public void RemoveStatusEffect(string id,int n,bool notification)=>statuses[id]=Math.Max(0,GetStatusEffectStacks(id)-n);
 public RoomState GetCurrentRoom()=>new(){index=floor};public List<SubtypeData> GetSubtypes()=>subtypes;
}
public class RoomState{public int index;public int GetRoomIndex()=>index;}
public enum CardPile{HandPile}
public class CardManager{public List<CardData> added=[];public CardState AddNewCard(CardData data,CardPile pile,bool relic,bool permanent,object? upgrades,bool animate){added.Add(data);return new();}}
public class SaveManager{public bool PreviewMode;}
public class AllGameData{public List<CardData> cards=[];public List<CardData> GetAllCardData()=>cards;}
public class StatusEffectData{public int GetParamInt()=>60;}
public class StatusEffectManager{public StatusEffectData GetStatusEffectDataById(string id)=>new();}
public class CombatManager {
 public class ApplyDamageToTargetParameters{public CardState? playedCard;public Damage.Type damageType;}
 public List<(int amount,CharacterState unit,CardState? source)> hits=[];
 public IEnumerator ApplyDamageToTarget(int damage,CharacterState unit,ApplyDamageToTargetParameters p){hits.Add((damage,unit,p.playedCard));yield break;}
}
public static class Damage{public enum Type{Default}}
public interface ISystemManagers{}
public class Systems:ISystemManagers{}
public interface ICoreGameManagers {SaveManager GetSaveManager();CardManager GetCardManager();AllGameData GetAllGameData();CombatManager GetCombatManager();StatusEffectManager GetStatusEffectManager();}
public class Managers:ICoreGameManagers {
 public SaveManager save=new();public CardManager cards=new();public AllGameData data=new();public CombatManager combat=new();
 public SaveManager GetSaveManager()=>save;public CardManager GetCardManager()=>cards;public AllGameData GetAllGameData()=>data;
 public CombatManager GetCombatManager()=>combat;public StatusEffectManager GetStatusEffectManager()=>new();
}
public enum RngId{Battle,BattleTest}
public static class RandomManager {
 public static int calls;public static RngId last;public static Random random=new(1);
 public static void Reset(int seed){calls=0;random=new(seed);}
 public static int Range(int min,int max,RngId id){calls++;last=id;return random.Next(min,max);}
}
namespace mt2_freecompany.Plugin{public class Logger{public void LogInfo(string s){}}public class Plugin{public static Logger Logger=new();}}

// 2026-10-27-1532||codex-freecompany-fx||D:\Juegos\MT2_mod\salidas\build\freecompany-native-audit-2026-10-27\tests\Api.cs||pruebas de regresión de Countdown/Requisition y compilación de revisión nativa

// 2026-11-07-1203||codex-freecompany-fx||tests/native-audit/Api.cs||consolida arnés existente en repositorio; enlaza fuentes locales canónicas, conserva original de laboratorio
