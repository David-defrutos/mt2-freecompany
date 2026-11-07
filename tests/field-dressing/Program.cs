using System.Collections;
using mt2_freecompany.Plugin;
class Program {
 static int checks;static void Check(bool c,string m){if(!c)throw new Exception(m);checks++;}
 static void Run(IEnumerator e){while(e.MoveNext())if(e.Current is IEnumerator a)Run(a);}
 static void Main(){var g=new Managers();var effect=new CardEffectRemoveRandomDebuff();var p=new CardEffectParams();var state=new CardEffectState();var target=new CharacterState();p.targets.Add(target);
 CharacterState.StatusEffectStack S(string id,int count,StatusEffectData.DisplayCategory category=StatusEffectData.DisplayCategory.Negative,bool hidden=false)=>new(){Count=count,State=new(){id=id,category=category,hidden=hidden}};
 var poison=S("poison",8);var dizzy=S("dizzy",3);var armor=S("armor",5,StatusEffectData.DisplayCategory.Positive);var hidden=S("internal",2,hidden:true);var zero=S("empty",0);target.statuses.AddRange(new[]{poison,dizzy,armor,hidden,zero});
 Check(effect.TestEffect(state,p,g)&&effect.CanApplyInPreviewMode,"preview targeting allowed");g.save.PreviewMode=true;Run(effect.ApplyEffect(state,p,g,new Systems()));Check(RandomManager.calls==0&&poison.Count==8&&target.ui.calls==0,"preview has no RNG/status/VFX mutation");g.save.PreviewMode=false;
 RandomManager.index=1;Run(effect.ApplyEffect(state,p,g,new Systems()));Check(dizzy.Count==0&&poison.Count==8,"one selected complete debuff removed");Check(armor.Count==5&&hidden.Count==2&&zero.Count==0,"positive hidden and empty excluded");Check(!target.modified&&target.notified,"stack modifiers disabled and removal notified");Check(target.ui.calls==1,"cleanse VFX on actual cleansed target");
 RandomManager.index=0;Run(effect.ApplyEffect(state,p,g,new Systems()));Check(poison.Count==0,"all stacks removed");var calls=RandomManager.calls;Run(effect.ApplyEffect(state,p,g,new Systems()));Check(RandomManager.calls==calls&&target.ui.calls==2,"no debuff means no random draw or cleanse visual");
 Check(effect.TestEffect(state,p,g),"healthy unit remains valid for following heal");p.targets.Clear();Check(!effect.TestEffect(state,p,g),"no unit target invalid");Console.WriteLine($"PASS {checks} semantic checks (real cleansing source, simulated game API)");}
}
public class PropDescriptions{}
public abstract class CardEffectBase{public virtual bool CanApplyInPreviewMode=>true;public abstract PropDescriptions CreateEditorInspectorDescriptions();public abstract bool TestEffect(CardEffectState e,CardEffectParams p,ICoreGameManagers m);public abstract IEnumerator ApplyEffect(CardEffectState e,CardEffectParams p,ICoreGameManagers m,ISystemManagers s);}
public class CardEffectState{public object GetAppliedVFX()=>new();}
public class CardEffectParams{public List<CharacterState> targets=new();public object? playedCard,sourceRelic;public int appliedVfxId;}
public class StatusEffectData{public enum DisplayCategory{Positive,Negative,Persistent,Ability}}
public class StatusState{public string id="";public bool hidden;public StatusEffectData.DisplayCategory category;public bool IsHidden()=>hidden;public StatusEffectData.DisplayCategory GetDisplayCategory()=>category;public string GetStatusId()=>id;}
public class CharacterUI{public int calls;public void ShowEffectVFX(CharacterState t,object v,int id){calls++;}}
public class CharacterState{public class StatusEffectStack{public int Count;public StatusState State=new();}public class RemoveStatusEffectParams{public bool showNotification;public object? sourceCardState,sourceRelicState;}public List<StatusEffectStack> statuses=new();public CharacterUI ui=new();public bool modified,notified;public void GetStatusEffects(ref List<StatusEffectStack> s)=>s.AddRange(statuses);public void RemoveStatusEffect(string id,int count,RemoveStatusEffectParams p,bool modifiers){statuses.First(s=>s.State.id==id).Count-=count;modified=modifiers;notified=p.showNotification;}public CharacterUI GetCharacterUI()=>ui;}
public interface ICoreGameManagers{SaveManager GetSaveManager();}public class SaveManager{public bool PreviewMode;}public class Managers:ICoreGameManagers{public SaveManager save=new();public SaveManager GetSaveManager()=>save;}public interface ISystemManagers{}public class Systems:ISystemManagers{}public enum RngId{Battle}public static class RandomManager{public static int calls,index;public static int Range(int a,int b,RngId rng){if(rng!=RngId.Battle||index>=b)throw new Exception("RNG invalid");calls++;return index;}}

// 2026-10-09-1829||codex-freecompany-fx||D:\Juegos\MT2_mod\salidas\dll\freecompany-field-dressing-tests\Program.cs||prueba aflicción completa aleatoria, estados excluidos, vista previa y VFX solo al limpiar

// 2026-11-07-1203||codex-freecompany-fx||tests/field-dressing/Program.cs||consolida arnés existente en repositorio; enlaza fuentes locales canónicas, conserva original de laboratorio
