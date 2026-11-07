# Documento generado el 2026-11-07-1204
"""Local FreeCompany workflow. Validation, installation and packaging are separate actions."""
from pathlib import Path
import argparse,datetime,hashlib,json,os,re,shutil,subprocess,sys,tempfile,zipfile

REPO=Path(__file__).resolve().parents[1]
WORKSPACE=Path(os.environ.get('MT2_WORKSPACE',r'D:\Juegos\MT2_mod'))
PLUGINS=Path(os.environ.get('APPDATA',r'C:\Users\david\AppData\Roaming'))/'Thunderstore Mod Manager/DataFolder/MonsterTrain2/profiles/Default/BepInEx/plugins'
INSTALLED=PLUGINS/'frutos-FreeCompany'
MANAGED=Path(r'C:\Juegos\Steam\steamapps\common\Monster Train 2\MonsterTrain2_Data\Managed')
DOTNET=Path(r'C:\Program Files\dotnet\dotnet.exe')
DLL='mt2_freecompany.Plugin.dll'
TRACKER='.freecompany-installed-files.json'

def run(args,**kw):
    return subprocess.run([str(x) for x in args],check=True,**kw)

def git(*args):
    return subprocess.check_output(['git','-c','safe.directory='+str(REPO),'-C',str(REPO),*args],text=True).strip()

def sha(data):return hashlib.sha256(data).hexdigest().upper()

def beneath(path,root):
    p=path.resolve();r=root.resolve()
    if p==r or r not in p.parents:raise ValueError('Unsafe path: '+str(p))
    return p

def manifest():
    m=json.loads((REPO/'manifest.json').read_text(encoding='utf-8-sig'))
    v=re.search(r'<Version>([^<]+)</Version>',(REPO/'src/mt2_freecompany.Plugin.csproj').read_text()).group(1)
    assert m['version_number']==v and re.fullmatch(r'\d+\.\d+\.\d+',v)
    return m

def required():
    paths=[]
    for folder in ['src','json','textures','scripts','tests']:
        paths += [p for p in (REPO/folder).rglob('*') if p.is_file() and not {'bin','obj','__pycache__'}.intersection(p.relative_to(REPO).parts)]
    paths += [REPO/n for n in ['manifest.json','README.md','CHANGELOG.md','NOTICE.md','LICENSE','icon.png']]
    return sorted(paths,key=lambda p:p.relative_to(REPO).as_posix())

def fingerprint():
    return sha(''.join(p.relative_to(REPO).as_posix()+'\t'+sha(p.read_bytes())+'\n' for p in required()).encode())

def game_closed():
    result=subprocess.run(['powershell','-NoProfile','-NonInteractive','-Command',
        "if (Get-Process -Name 'MonsterTrain2' -ErrorAction SilentlyContinue) { exit 2 }"],capture_output=True)
    if result.returncode:raise RuntimeError('Close Monster Train 2 before installing. No files were installed.')

def validate():
    before=fingerprint();head=git('rev-parse','HEAD');m=manifest()
    import struct
    icon=(REPO/'icon.png').read_bytes()
    assert icon.startswith(b'\x89PNG\r\n\x1a\n') and struct.unpack('>II',icon[16:24])==(256,256),'Icon must be 256x256 PNG'
    run([sys.executable,REPO/'scripts/validate-content.py'])
    run([sys.executable,REPO/'scripts/test-workflow.py'])
    for project in sorted((REPO/'tests').glob('*/tests.csproj')):
        # These harnesses have no online package restore requirement. Ignore failed
        # remote feeds; Newtonsoft is referenced from the installed dependency.
        run([DOTNET,'run','--project',project,'-p:RestoreIgnoreFailedSources=true'])
    stamp=datetime.datetime.now().strftime('%Y%m%d-%H%M%S-%f')
    out=WORKSPACE/'salidas/build/freecompany'/stamp
    out.mkdir(parents=True,exist_ok=False)
    sdk=subprocess.check_output([str(DOTNET),'--list-sdks'],text=True)
    candidates=[]
    for line in sdk.splitlines():
        match=re.match(r'(\S+) \[(.+)\]',line)
        if match:candidates.append(Path(match[2])/match[1]/'Roslyn/bincore/csc.dll')
    csc=next((p for p in reversed(candidates) if p.is_file()),None)
    if csc is None:raise RuntimeError('No installed Roslyn compiler')
    refs={}
    roots=[MANAGED,PLUGINS.parent/'core',PLUGINS/'MT2-Trainworks_Reloaded',PLUGINS/'Conductor-Conductor']
    for folder in roots:
        if not folder.is_dir():raise FileNotFoundError(folder)
        for p in sorted(folder.glob('*.dll')):
            # BepInEx ships compatibility copies of Harmony with duplicate types.
            if p.name.lower().startswith('0harmony') and p.name.lower()!='0harmony.dll':continue
            refs[p.name.lower()]=p
    dirty=bool(git('status','--porcelain'))
    info='global using System; global using System.Collections.Generic; global using System.Linq;\n'
    info+='[assembly:System.Reflection.AssemblyVersion("'+m['version_number']+'.0")]\n'
    info+='[assembly:System.Reflection.AssemblyFileVersion("'+m['version_number']+'.0")]\n'
    info+='[assembly:System.Reflection.AssemblyInformationalVersion("'+m['version_number']+'+'+head+('.local' if dirty else '')+'")]\n'
    info+='namespace mt2_freecompany.Plugin { internal static class MyPluginInfo { public const string PLUGIN_GUID="mt2_freecompany.Plugin"; public const string PLUGIN_NAME="mt2_freecompany.Plugin"; public const string PLUGIN_VERSION="'+m['version_number']+'"; } }\n'
    (out/'BuildInfo.cs').write_text(info,encoding='utf-8')
    sources=[p for p in required() if p.suffix=='.cs' and p.is_relative_to(REPO/'src')]+[out/'BuildInfo.cs']
    rsp=['/nostdlib+','/target:library','/langversion:latest','/nullable:enable','/optimize+','/out:"'+str(out/DLL)+'"']
    rsp+=['/reference:"'+str(p)+'"' for p in refs.values()]+['"'+str(p)+'"' for p in sources]
    (out/'compile.rsp').write_text('\n'.join(rsp),encoding='utf-8')
    run([DOTNET,csc,'@'+str(out/'compile.rsp')])
    raw=(out/DLL).read_bytes()
    declared=set(re.findall(r'"(json/[A-Za-z0-9_/]+\.json)"',(REPO/'src/Plugin.cs').read_text(encoding='utf-8-sig')))
    assert all(p.encode('utf-16le') in raw for p in declared),'Compiled DLL does not register all JSON'
    if fingerprint()!=before or git('rev-parse','HEAD')!=head:raise RuntimeError('Sources or revision changed during validation; repeat with the new content')
    record={'repository':str(REPO),'commit':head,'dirty':dirty,'sourceFingerprint':before,'version':m['version_number'],
        'dll':str(out/DLL),'dllSHA256':sha(raw),'references':{str(p):sha(p.read_bytes()) for p in refs.values()}}
    (out/'validation.json').write_text(json.dumps(record,indent=2),encoding='utf-8')
    print('VALIDATED:',out,flush=True)
    return record

def payload(record):
    if fingerprint()!=record['sourceFingerprint']:raise RuntimeError('Source changed after validation')
    result={DLL:Path(record['dll']).read_bytes()}
    for n in ['NOTICE.md','LICENSE','icon.png'] :result[n]=(REPO/n).read_bytes()
    # Public metadata must not contain internal trace comments or a UTF-8 BOM.
    for n in ['README.md','CHANGELOG.md']:
        s=(REPO/n).read_text(encoding='utf-8-sig');s=re.sub(r'<!--.*?-->','',s,flags=re.S)
        result[n]=s.encode('utf-8')
    result['manifest.json']=json.dumps(manifest(),ensure_ascii=False,indent=2).encode('utf-8')
    textures=set()
    for p in (REPO/'json').rglob('*.json'):
        rel=p.relative_to(REPO).as_posix();raw=p.read_bytes();result[rel]=raw
        textures.update(re.findall(r'"(textures/[^"\r\n]+)"',raw.decode('utf-8-sig')))
    for rel in textures:
        p=beneath(REPO/rel,REPO/'textures');result[rel]=p.read_bytes()
    assert sha(result[DLL])==record['dllSHA256']
    return result

def historical_owned():
    names=set(git('log','--all','--format=','--name-only','--','json','textures').splitlines())
    return names|{DLL,'NOTICE.md','LICENSE','README.md','CHANGELOG.md','manifest.json','icon.png'}

def apply_install(files,target,allowed_root,backup_root,owned,dry_run=False):
    if target.is_symlink() or (target.exists() and target.is_junction()):raise ValueError('Refuse linked install folder')
    target=beneath(target,allowed_root)
    if target.is_symlink() or (target.exists() and target.is_junction()):raise ValueError('Refuse linked install folder')
    # Never traverse links/reparse points in a user directory.
    if target.exists():
        for p in target.rglob('*'):
            if p.is_symlink() or p.is_junction():raise ValueError('Refuse linked path: '+str(p))
    previous={}
    preserved=set()
    tracker=target/TRACKER
    if tracker.exists():
        state=json.loads(tracker.read_text(encoding='utf-8'));assert state['mod']=='frutos-FreeCompany'
        previous=state['files'];preserved=set(state.get('preserved',[]));owned=(set(owned)|set(previous))-preserved
    stale=[]
    for rel in sorted(set(owned)-set(files)):
        p=beneath(target/rel,target)
        if p.is_file():
            if rel in previous and sha(p.read_bytes())!=previous[rel]:
                print('PRESERVED modified obsolete file:',rel);preserved.add(rel);continue
            stale.append(rel)
    print('INSTALL PLAN:',target,'write',len(files),'remove obsolete owned',len(stale),flush=True)
    if dry_run:return {'write':sorted(files),'remove':stale}
    snapshots={}
    if target.exists():
        backup=backup_root/datetime.datetime.now().strftime('%Y%m%d-%H%M%S-%f')
        backup.mkdir(parents=True,exist_ok=False)
        shutil.copytree(target,backup/'installed-before')
        for p in target.rglob('*'):
            if p.is_file():
                rel=p.relative_to(target).as_posix();raw=p.read_bytes()
                if (backup/'installed-before'/rel).read_bytes()!=raw:raise RuntimeError('Backup mismatch: '+rel)
                snapshots[rel]=(raw,p.stat().st_mtime_ns)
    target.mkdir(parents=True,exist_ok=True)
    def unchanged(rel):
        p=beneath(target/rel,target)
        if rel in snapshots:
            raw,mt=snapshots[rel]
            if not p.is_file() or p.read_bytes()!=raw or p.stat().st_mtime_ns!=mt:raise RuntimeError('Concurrent installed change: '+rel)
        elif p.exists():raise RuntimeError('New installed file appeared: '+rel)
        return p
    if target==INSTALLED.resolve():game_closed()
    # Check the complete mutation set before the first write.
    for rel in set(files)|set(stale)|{TRACKER}:unchanged(rel)
    for rel,raw in files.items():
        p=unchanged(rel);p.parent.mkdir(parents=True,exist_ok=True)
        fd,tmp=tempfile.mkstemp(prefix='.freecompany-',suffix='.tmp',dir=p.parent)
        with os.fdopen(fd,'wb') as f:f.write(raw)
        os.replace(tmp,p)
        if p.read_bytes()!=raw:raise RuntimeError('Copy verification failed: '+rel)
    for rel in stale:unchanged(rel).unlink()
    state={'mod':'frutos-FreeCompany','files':{k:sha(v) for k,v in files.items()},'preserved':sorted(preserved-set(files))}
    unchanged(TRACKER).write_text(json.dumps(state,indent=2),encoding='utf-8')
    assert all((target/rel).read_bytes()==raw for rel,raw in files.items())
    print('INSTALLED and verified:',target,flush=True)
    return state

def package(record,destination):
    if record['dirty']:raise RuntimeError('Commit local changes before preparing a publication package')
    dest=Path(destination) if destination else WORKSPACE/'salidas'/('frutos-FreeCompany-'+record['version']+'-'+record['commit'][:7]+'.zip')
    if dest.exists():raise FileExistsError(dest)
    files=payload(record);dest.parent.mkdir(parents=True,exist_ok=True)
    with zipfile.ZipFile(dest,'x',zipfile.ZIP_DEFLATED) as z:
        for rel,raw in files.items():
            entry=rel if rel in ['manifest.json','README.md','CHANGELOG.md','LICENSE','icon.png'] else 'plugins/'+rel
            z.writestr(entry,raw)
    with zipfile.ZipFile(dest) as z:
        assert z.testzip() is None
        assert not z.read('manifest.json').startswith(b'\xef\xbb\xbf')
        for rel,raw in files.items():
            entry=rel if rel in ['manifest.json','README.md','CHANGELOG.md','LICENSE','icon.png'] else 'plugins/'+rel
            assert z.read(entry)==raw
    identity={'commit':record['commit'],'sourceFingerprint':record['sourceFingerprint'],'dllSHA256':record['dllSHA256'],'zipSHA256':sha(dest.read_bytes())}
    dest.with_suffix('.identity.json').write_text(json.dumps(identity,indent=2),encoding='utf-8')
    print('PACKAGE PREPARED (not published):',dest,flush=True)

def main():
    p=argparse.ArgumentParser();p.add_argument('action',choices=['validar','instalar','empaquetar']);p.add_argument('--destination');p.add_argument('--dry-run',action='store_true');a=p.parse_args()
    if a.action=='instalar':game_closed()
    if a.action=='empaquetar' and git('status','--porcelain'):raise RuntimeError('Commit local changes before preparing publication')
    record=validate()
    if a.action=='instalar':
        game_closed()
        apply_install(payload(record),INSTALLED,PLUGINS,WORKSPACE/'backups/freecompany/install',historical_owned(),a.dry_run)
    elif a.action=='empaquetar':package(record,a.destination)
    else:print('Actions: validated YES; installed NO; publication package NO.',flush=True)

if __name__=='__main__':main()

# 2026-11-07-1204||codex-freecompany-fx||scripts/workflow.py||añade validación/compilación local, instalación completa con respaldo/limpieza selectiva en frutos-FreeCompany Default y ZIP identificado sin publicar

# 2026-11-07-1206||codex-freecompany-fx||scripts/workflow.py||excluye Harmony de compatibilidad duplicado del compilador y añade pruebas aisladas del instalador/ZIP

# 2026-11-07-1207||codex-freecompany-fx||scripts/workflow.py||conserva obsoletos modificados también entre instalaciones sucesivas y revalida juego cerrado tras backup

# 2026-11-07-1208||codex-freecompany-fx||scripts/workflow.py||comprueba icono de publicación y configura restore de suites sin exigir feeds remotos

# 2026-11-07-1321||codex-freecompany-fx||scripts/workflow.py||incluye scripts y pruebas en huella validada y rechaza cambio concurrente de HEAD
