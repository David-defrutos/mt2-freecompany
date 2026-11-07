from pathlib import Path
import re,json,subprocess,collections
root=Path(__file__).resolve().parents[1]
ilspy=r"C:\Users\david\.dotnet\tools\ilspycmd.exe"
game=r"C:\Juegos\Steam\steamapps\common\Monster Train 2\MonsterTrain2_Data\Managed\Assembly-CSharp.dll"
classes=set(re.findall(r"^Class ([^\r\n]+)",subprocess.check_output([ilspy,'-l','c',game],text=True),re.M))
custom=set()
for p in (root/'src').rglob('*.cs'):custom.update(re.findall(r'\bclass\s+(\w+)',p.read_text(encoding='utf-8-sig')))
plugin=(root/'src/Plugin.cs').read_text(encoding='utf-8-sig')
registered=set(re.findall(r'"(json/[A-Za-z0-9_/]+\.json)"',plugin))
actual={p.relative_to(root).as_posix() for p in (root/'json').rglob('*.json')}
assert registered==actual and actual
allids=set(); files={}; sections=collections.defaultdict(set)
for filename in sorted(actual):
    raw=(root/filename).read_bytes();assert raw.startswith(b'\xef\xbb\xbf'),filename
    if filename=='json/units/unit_battle_chaplain.json':assert b'\n' not in raw.replace(b'\r\n',b''),filename
    data=json.loads(raw.decode('utf-8-sig')); files[filename]=data
    for category,items in data.items():
        if isinstance(items,list):
            for item in items:
                if isinstance(item,dict) and 'id' in item:
                    key=item['id'];assert key not in sections[category],(category,key,filename)
                    sections[category].add(key);allids.add(key)
refs=[]; effects=0;sprites=0
for filename,data in files.items():
    for category in ['effects','relic_effects','status_effects']:
        for item in data.get(category,[]):
            value=item.get('class_name' if category=='status_effects' else 'name')
            if isinstance(value,str):
                if value.startswith('@'):assert value[1:] in custom,(filename,value)
                else:assert value in classes,(filename,value)
            effects+=1
    for sprite in data.get('sprites',[]):
        if 'path' in sprite:assert (root/sprite['path']).is_file(),(filename,sprite['path']);sprites+=1
    def visit(v):
        if isinstance(v,dict):
            if v.get('mod_reference'):return
            for value in v.values():visit(value)
        elif isinstance(v,list):
            for value in v:visit(value)
        elif isinstance(v,str) and v.startswith('@'):
            name=v[1:];assert name in allids or name in custom,(filename,v)
            refs.append(v)
    visit(data)
chaplain=files['json/units/unit_battle_chaplain.json'];heal=next(e for e in chaplain['effects'] if e['id']=='BattleChaplainAbilityHeal')
assert heal['name']=='CardEffectHeal' and heal['target_mode']=='tower' and heal['target_team']=='monsters' and heal['param_int']==18
assert 'param_str' not in heal
assert not any(n in custom for n in ['CardEffectAllRooms','CardEffectIfAllyHasUpgrade','CardEffectIfSubtypeInDrawPile'])
print(f'PASS: {len(actual)} registered JSON, {effects} effect/status/relic definitions, {len(refs)} local references, {sprites} sprite paths; native Last Rites and no retired classes')
# 2026-10-27-1535||codex-freecompany-fx||D:\Juegos\MT2_mod\salidas\build\freecompany-native-audit-2026-10-27\validate-json.py||audita todos los JSON registrados, referencias locales, clases nativas y recursos

# 2026-10-27-1535||codex-freecompany-fx||D:\Juegos\MT2_mod\salidas\build\freecompany-native-audit-2026-10-27\validate-json.py||comprueba CRLF en JSON editado; no rechaza estilos preexistentes de ficheros ajenos

# 2026-11-07-1203||codex-freecompany-fx||scripts/validate-content.py||valida contenido desde el repositorio del script, sin ruta de fuentes en plugins ni cuenta fija de JSON
