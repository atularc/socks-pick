"""Package the validated gameplay update without rebuilding GUI assets."""
from pathlib import Path
import hashlib
import json
import shutil
import subprocess
import sys
import zipfile

root = Path(__file__).resolve().parents[1]
subprocess.run([sys.executable, str(root/'Tools/verify_opening_scope.py')], cwd=root, check=True)
qa = root/'Documentation/Opening-Progression'
for report in [qa/'Browser/report.json', qa/'Regression/report.json']:
    assert json.loads(report.read_text())['result'] == 'PASS', str(report)
clone = root/'.gui-build-workspace'
for relative in ['Assets/Scripts/Core/Puzzle.cs', 'Assets/Scripts/Runtime/SockGame.cs',
                 'Assets/Scripts/Runtime/SaveData.cs', 'Assets/Resources/Data/levels.json']:
    assert (root/relative).read_bytes() == (clone/relative).read_bytes(), relative
assert 'Exiting batchmode successfully' in (root/'opening-progression-release-build.log').read_text(errors='replace')
artifacts=[]
for target, filename in [('Web','Web.zip'),('YouTube','SockPairs-YouTube.zip')]:
    source=clone/'Builds'/target
    assert (source/'index.html').exists()
    archive=root/'Builds'/filename
    if archive.exists() and not (qa/'Before'/filename).exists():
        shutil.copy2(archive,qa/'Before'/filename)
    temporary=archive.with_suffix('.zip.tmp')
    with zipfile.ZipFile(temporary,'w',compression=zipfile.ZIP_DEFLATED,compresslevel=9) as z:
        for path in sorted(source.rglob('*')):
            if path.is_file(): z.write(path,path.relative_to(source))
    temporary.replace(archive)
    shutil.copytree(source,root/'Builds'/target,dirs_exist_ok=True)
    artifacts.append({'file':str(archive.relative_to(root)),'bytes':archive.stat().st_size,
                      'sha256':hashlib.sha256(archive.read_bytes()).hexdigest()})
(qa/'delivery.json').write_text(json.dumps({'result':'PASS','artifacts':artifacts},indent=2))
print(json.dumps(artifacts,indent=2))
