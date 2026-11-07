# Documento generado el 2026-11-07-1206
"""Exercise deployment and packaging in a temporary workspace, never Default."""
from pathlib import Path
import importlib.util,json,tempfile,zipfile,unittest
spec=importlib.util.spec_from_file_location('workflow',Path(__file__).with_name('workflow.py'))
w=importlib.util.module_from_spec(spec);spec.loader.exec_module(w)
class DeploymentTests(unittest.TestCase):
    def setUp(self):
        self.temp=tempfile.TemporaryDirectory(dir=w.WORKSPACE/'tmp');self.base=Path(self.temp.name)
        self.root=self.base/'plugins';self.root.mkdir();self.target=self.root/'frutos-FreeCompany';self.target.mkdir()
        self.backups=self.base/'backups';self.files={w.DLL:b'new-dll','json/new.json':b'new-json'}
    def tearDown(self):self.temp.cleanup()
    def put(self,p,data):
        f=self.target/p;f.parent.mkdir(parents=True,exist_ok=True);f.write_bytes(data)
    def apply(self,owned,**kwargs):return w.apply_install(self.files,self.target,self.root,self.backups,owned,**kwargs)
    def test_foreign_files_preserved_and_owned_removed(self):
        self.put('json/foreign.json',b'foreign');self.put('json/old.json',b'old');self.put(w.DLL,b'old-dll')
        self.apply({'json/old.json',w.DLL});self.assertEqual((self.target/'json/foreign.json').read_bytes(),b'foreign')
        self.assertFalse((self.target/'json/old.json').exists());self.assertEqual((self.target/w.DLL).read_bytes(),b'new-dll')
    def test_complete_backup_before_install(self):
        self.put('foreign.txt',b'keep');self.apply(set())
        self.assertEqual(next(self.backups.glob('*/installed-before/foreign.txt')).read_bytes(),b'keep')
    def test_dry_run_does_not_install(self):
        self.put('old.txt',b'old');self.apply({'old.txt'},dry_run=True)
        self.assertEqual((self.target/'old.txt').read_bytes(),b'old');self.assertFalse((self.target/w.DLL).exists())
        self.assertFalse(self.backups.exists())
    def test_modified_obsolete_file_is_preserved(self):
        self.put('old.txt',b'user edit');self.put(w.TRACKER,json.dumps({'mod':'frutos-FreeCompany','files':{'old.txt':w.sha(b'original')}}).encode())
        self.apply({'old.txt'});self.assertEqual((self.target/'old.txt').read_bytes(),b'user edit')
        self.apply({'old.txt'});self.assertEqual((self.target/'old.txt').read_bytes(),b'user edit')
    def test_reject_path_escape(self):
        with self.assertRaises(ValueError):w.apply_install({'../escape':b'bad'},self.target,self.root,self.backups,set())
        self.assertFalse((self.root/'escape').exists())
    def test_reject_target_outside_plugins(self):
        with self.assertRaises(ValueError):w.apply_install(self.files,self.base/'other',self.root,self.backups,set())
    def test_tracker_records_exact_payload(self):
        self.apply(set());d=json.loads((self.target/w.TRACKER).read_text());self.assertEqual(d['files'],{k:w.sha(v) for k,v in self.files.items()})
    def test_publication_rejects_dirty_revision(self):
        with self.assertRaises(RuntimeError):w.package({'dirty':True},self.base/'bad.zip')
        self.assertFalse((self.base/'bad.zip').exists())
    def test_package_structure_and_identity(self):
        old=w.payload;files={'manifest.json':b'{}','README.md':b'text','CHANGELOG.md':b'changes','LICENSE':b'license','icon.png':b'icon',w.DLL:b'dll','json/plugin.json':b'json','textures/test.png':b'image','NOTICE.md':b'notice'}
        try:
            w.payload=lambda record:files;out=self.base/'fixture.zip'
            w.package({'dirty':False,'commit':'abc','sourceFingerprint':'fp','dllSHA256':w.sha(b'dll')},out)
            with zipfile.ZipFile(out) as z:self.assertIn('plugins/json/plugin.json',z.namelist());self.assertNotIn('json/plugin.json',z.namelist())
            self.assertTrue(out.with_suffix('.identity.json').exists())
            with self.assertRaises(FileExistsError):w.package({'dirty':False},out)
        finally:w.payload=old
if __name__=='__main__':unittest.main()

# 2026-11-07-1206||codex-freecompany-fx||scripts/test-workflow.py||prueba instalación en carpetas temporales, backup, contenido ajeno, obsoletos, límites de rutas, simulación e identidad del ZIP

# 2026-11-07-1207||codex-freecompany-fx||scripts/test-workflow.py||prueba preservación de contenido modificado en dos reinstalaciones sucesivas
