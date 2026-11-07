using mt2_freecompany.Plugin;
class GoodBefore{} class Bad{} class GoodAfter{}
class Program {
 static int checks;static void Check(bool value,string text){if(!value)throw new Exception(text);checks++;}
 static void Main(){
  HarmonyLib.Harmony.bad=typeof(Bad);var h=new HarmonyLib.Harmony("test");
  var failures=Plugin.ApplyPatches(h,new[]{typeof(GoodBefore),typeof(Bad),typeof(GoodAfter)});
  Check(failures==1,"returns failed class count");Check(HarmonyLib.Harmony.visited.SequenceEqual(new[]{typeof(GoodBefore),typeof(Bad),typeof(GoodAfter)}),"later class processed after failing patch");
  Check(Plugin.Logger.errors.Any(s=>s.Contains(typeof(Bad).FullName!)&&s.Contains("synthetic contract failure")),"error names class and exception");
  new Plugin().Awake();Check(Plugin.Logger.errors.Any(s=>s.Contains("Some features are unavailable")),"partial startup has visible summary");
  HarmonyLib.Harmony.bad=null;Plugin.Logger.infos.Clear();new Plugin().Awake();Check(Plugin.Logger.infos.Any(s=>s.Contains("is loaded")),"successful startup logged only after processing");
  Check(TrainworksReloaded.Core.Builder.count==79,"JSON registration preserved");
  Console.WriteLine($"PASS {checks} patch-loader checks (production Plugin.cs with simulated Harmony)");
 }
}
namespace BepInEx.Logging {public class ManualLogSource{public List<string> errors=new(),infos=new();public ManualLogSource(string name){}public void LogError(object s)=>errors.Add(s.ToString()!);public void LogInfo(object s)=>infos.Add(s.ToString()!);}}
namespace BepInEx {public class BepInPlugin:Attribute{public BepInPlugin(string a,string b,string c){}}public class BaseUnityPlugin{protected BepInEx.Logging.ManualLogSource Logger=new("base");}}
namespace HarmonyLib {public class Harmony {public static Type? bad;public static List<Type> visited=new();public Harmony(string id){}public Processor CreateClassProcessor(Type t)=>new(t);}public class Processor(Type t){public void Patch(){Harmony.visited.Add(t);if(t==Harmony.bad)throw new Exception("synthetic contract failure");}}}
namespace TrainworksReloaded.Core {public class Config{public void AddMergedJsonFile(params string[] paths)=>Builder.count=paths.Length;}public class Builder{public static int count;public void Configure(string id,Action<Config> setup)=>setup(new());}public class Railhead{public static Builder GetBuilder()=>new();}}
namespace TrainworksReloaded.Core.Extensions {public static class Placeholder{}}
namespace mt2_freecompany.Plugin {public class MyPluginInfo{public const string PLUGIN_GUID="mt2_freecompany.Plugin",PLUGIN_NAME="FreeCompany",PLUGIN_VERSION="0.3.2";}}

// 2026-10-27-2205||codex-freecompany-fx||D:\Juegos\MT2_mod\salidas\build\freecompany-external-audit-2026-10-27\loader-tests\Program.cs||prueba código real Plugin: fallo Harmony aislado, diagnóstico y continuación

// 2026-11-07-1203||codex-freecompany-fx||tests/loader/Program.cs||consolida arnés existente en repositorio; enlaza fuentes locales canónicas, conserva original de laboratorio
