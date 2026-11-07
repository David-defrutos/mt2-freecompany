using System.Collections;
using mt2_freecompany.Plugin;
class Program{
 static int checks;
 static void Check(bool condition,string message){if(!condition)throw new Exception(message);checks++;}
 static void Run(IEnumerator e){try{while(e.MoveNext())if(e.Current is IEnumerator nested)Run(nested);}finally{(e as IDisposable)?.Dispose();}}
 static void Main(){
 var g=new Managers();var sys=new Systems();var unit=new CharacterState();var args=new CardEffectParams{targets=[unit],playedCard=new()};
 var effect=new CardEffectState{value=20};var plant=new CardEffectPlantCountdown();var tick=new CardEffectCountdownTick();
 Run(plant.ApplyEffect(effect,args,g,sys));Check(unit.GetStatusEffectStacks("fuse")==2,"initial charges");
 var magnitude=new StatusEffectFreeCompanyCountdownState{associated=unit};Check(magnitude.GetEffectMagnitude()==20,"initial pending damage");
 Run(tick.ApplyEffect(effect,args,g,sys));Check(unit.GetStatusEffectStacks("fuse")==2,"no tick on same floor");
 unit.floor=1;Run(tick.ApplyEffect(effect,args,g,sys));Check(unit.GetStatusEffectStacks("fuse")==1&&g.combat.hits.Count==0,"first floor consumes one");
 effect.value=30;Run(plant.ApplyEffect(effect,args,g,sys));Check(magnitude.GetEffectMagnitude()==50&&unit.GetStatusEffectStacks("fuse")==3,"live effects accumulate");
 unit.statuses.Clear();effect.value=7;Run(plant.ApplyEffect(effect,args,g,sys));Check(magnitude.GetEffectMagnitude()==7,"cleanse then replant drops old damage without intermediate tick");
 unit.floor=2;Run(tick.ApplyEffect(effect,args,g,sys));unit.floor=3;Run(tick.ApplyEffect(effect,args,g,sys));
 Check(g.combat.hits.Count==1&&g.combat.hits[0].amount==7,"explosion only new damage");Check(ReferenceEquals(g.combat.hits[0].source,args.playedCard),"native damage source retained");
 Run(tick.ApplyEffect(effect,args,g,sys));Check(g.combat.hits.Count==1,"explosion not duplicated");
 effect.value=40;Run(plant.ApplyEffect(effect,args,g,sys));unit.statuses.Clear();unit.floor=2;Run(tick.ApplyEffect(effect,args,g,sys));
 var pending=Countdowns.tabla.GetOrCreateValue(unit);Check(!pending.pendientes.ContainsKey("fuse")&&!pending.ultimoPiso.ContainsKey("fuse"),"cleared state removes pending data");
 unit.RejectStatus=true;Run(plant.ApplyEffect(effect,args,g,sys));Check(!pending.pendientes.ContainsKey("fuse"),"immune target does not record phantom damage");unit.RejectStatus=false;
 unit.statuses.Clear();effect.value=13;Run(plant.ApplyEffect(effect,args,g,sys));
 var rememberedFloor=pending.ultimoPiso["fuse"];unit.RejectStatus=true;effect.value=99;unit.floor=6;
 Run(plant.ApplyEffect(effect,args,g,sys));Check(magnitude.GetEffectMagnitude()==13,"H4 rejected reapplication cannot add damage to existing stacks");
 Check(pending.ultimoPiso["fuse"]==rememberedFloor,"H4 rejected reapplication cannot reset floor tracking");unit.RejectStatus=false;
 var preview=new CharacterState{PreviewMode=true};args.targets=[preview];Run(plant.ApplyEffect(effect,args,g,sys));Check(!Countdowns.tabla.TryGetValue(preview,out _),"preview stores no real pending damage");
 args.targets=[unit];effect.value=10;Run(plant.ApplyEffect(effect,args,g,sys));var before=unit.GetStatusEffectStacks("fuse");g.save.PreviewMode=true;Check(!tick.CanApplyInPreviewMode,"native preview execution skips ticks");Check(unit.GetStatusEffectStacks("fuse")==before,"preview setup does not mutate real stacks");g.save.PreviewMode=false;
 var roles=new[]{"Buckler","NotchedAxe","Lockpicks","ChalkCircle","Bandages","Shortbow"};var roles2=new[]{"KiteShield","TwinAxes","PoisonedDagger","Grimoire","HolySymbol","Longbow"};var roles3=new[]{"BulwarkPlate","ReaversMaul","ThiefsMantle","ArchmagesStaff","Reliquary","GreatBow"};
 foreach(var id in roles.Concat(roles2).Concat(roles3))g.data.cards.Add(new(){name="mt2_freecompany.Plugin-Card-"+id});
 var req=new CardEffectRequisition();args.targets=[new()];effect.value=2;
 RandomManager.Reset(42);Run(req.ApplyEffect(effect,args,g,sys));var first=g.cards.added.Last().name;
 Check(RandomManager.calls==1&&RandomManager.last==RngId.Battle,"Requisition uses battle RNG once for unknown role");
 RandomManager.Reset(42);Run(req.ApplyEffect(effect,args,g,sys));Check(first==g.cards.added.Last().name,"same seed repeats equipment selection");
 Check(roles2.Any(id=>first.EndsWith("-"+id)),"random equipment retains path tier");
 args.targets[0].subtypes.Add(new(){Key="SubtytpesData_nameKey-mt2_freecompany.Plugin-Subtype-Sub_Warrior"});effect.value=3;int rng=RandomManager.calls;
 Run(req.ApplyEffect(effect,args,g,sys));Check(g.cards.added.Last().name.EndsWith("-BulwarkPlate")&&RandomManager.calls==rng,"known role deterministic and tier preserved");
 args.targets[0].subtypes[0].Key="SubtypesData_nameKey-mt2_freecompany.Plugin-Subtype-Sub_Warrior";
 Run(req.ApplyEffect(effect,args,g,sys));Check(g.cards.added.Last().name.EndsWith("-BulwarkPlate")&&RandomManager.calls==rng,"H5 corrected Trainworks prefix retains intended role");
 args.targets[0].subtypes[0].Key="SubtypesData_nameKey-other.Plugin-Subtype-Sub_Warrior";
 Run(req.ApplyEffect(effect,args,g,sys));Check(RandomManager.calls==rng+1,"H5 role identity is scoped to our namespace");rng=RandomManager.calls;
 g.save.PreviewMode=true;int count=g.cards.added.Count;Run(req.ApplyEffect(effect,args,g,sys));Check(g.cards.added.Count==count&&RandomManager.calls==rng,"preview grants no equipment and consumes no RNG");
 Console.WriteLine($"PASS {checks} native-audit regression checks (actual production sources, simulated game API)");
 }
}

// 2026-10-27-1532||codex-freecompany-fx||D:\Juegos\MT2_mod\salidas\build\freecompany-native-audit-2026-10-27\tests\Program.cs||pruebas de regresión de Countdown/Requisition y compilación de revisión nativa

// 2026-10-27-2205||codex-freecompany-fx||D:\Juegos\MT2_mod\salidas\build\freecompany-native-audit-2026-10-27\tests\Program.cs||H4/H5: cuatro regresiones para reaplicación rechazada y prefijo de rol corregido sin confundir otros mods

// 2026-11-07-1203||codex-freecompany-fx||tests/native-audit/Program.cs||consolida arnés existente en repositorio; enlaza fuentes locales canónicas, conserva original de laboratorio
