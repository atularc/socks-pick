"""Package the verified authored-Canvas release and record measured size changes."""
from pathlib import Path
import hashlib
import html
import json
import re
import shutil
import zipfile

root=Path(__file__).resolve().parents[1]
clone=root/'.gui-build-workspace'
qa=root/'Documentation/Scene-Canvas'
assert (qa/'EditorQA/report.txt').read_text().startswith('PASS:')
for name in ['BrowserProgression','BrowserNavigation']:
    report=qa/name/'report.json'
    assert json.loads(report.read_text())['result']=='PASS', name
    assert report.stat().st_mtime >= max(p.stat().st_mtime for p in (clone/'Builds/Web/Build').iterdir()), 'Browser checks must use the final build'
assert 'Exiting batchmode successfully' in (root/'scene-canvas-release-build.log').read_text(errors='replace')
folder_guid_differences=[]
for folder in ['Assets/Scripts/Runtime','Assets/Scripts/Core','Assets/Scenes','Assets/UI','Assets/Resources']:
    for path in (root/folder).rglob('*'):
        if not path.is_file():continue
        source_bytes=path.read_bytes();build_bytes=(clone/path.relative_to(root)).read_bytes()
        if source_bytes!=build_bytes and path.suffix=='.meta' and path.with_suffix('').is_dir():
            # Folder identities are not player assets. Their importer settings must still match.
            normalize=lambda data:re.sub(rb'guid: [0-9a-f]+',b'guid: <folder>',data.replace(b'\r\n',b'\n'))
            assert normalize(source_bytes)==normalize(build_bytes),str(path)
            folder_guid_differences.append(str(path.relative_to(root)));continue
        assert source_bytes==build_bytes,str(path)

artifacts=[]
backup=root/'Builds/Before-Scene-Canvas';backup.mkdir(exist_ok=True)
for target,filename in [('Web','Web.zip'),('YouTube','SockPairs-YouTube.zip')]:
    source=clone/'Builds'/target;archive=root/'Builds'/filename
    old=backup/filename
    if archive.exists() and not old.exists():shutil.copy2(archive,old)
    previous=old.stat().st_size if old.exists() else None
    temporary=archive.with_suffix('.zip.tmp')
    with zipfile.ZipFile(temporary,'w',compression=zipfile.ZIP_DEFLATED,compresslevel=9) as z:
        for path in sorted(source.rglob('*')):
            if path.is_file():z.write(path,path.relative_to(source))
    temporary.replace(archive);shutil.copytree(source,root/'Builds'/target,dirs_exist_ok=True)
    size=archive.stat().st_size
    artifacts.append({'target':target,'archive':str(archive.relative_to(root)),'archiveBytes':size,
                      'archiveMB':round(size/1_000_000,3),'previousArchiveBytes':previous,
                      'changeBytes':size-previous if previous else None,
                      'changePercent':round((size/previous-1)*100,2) if previous else None,
                      'exportedFilesBytes':sum(p.stat().st_size for p in source.rglob('*') if p.is_file()),
                      'sha256':hashlib.sha256(archive.read_bytes()).hexdigest()})
report={'result':'PASS','units':'MB = 1,000,000 bytes','screens':29,'initialWasmHeapBytes':67108864,
        'cloneFolderGuidDifferences':folder_guid_differences,
        'artifacts':artifacts,'note':'YouTube upload files are uncompressed; serving compression is controlled by the host.'}
(qa/'build-sizes.json').write_text(json.dumps(report,indent=2))
shots=[('BrowserProgression/level-1-start.png','Direct level 1 startup'),('BrowserNavigation/map.png','Level map'),
       ('BrowserNavigation/album.png','Sock album'),('BrowserNavigation/pause.png','Pause'),
       ('BrowserNavigation/settings.png','Settings'),('BrowserProgression/level-3-tutorial.png','Reveal tutorial'),
       ('BrowserProgression/level-4-tutorial.png','Basket tutorial'),('BrowserProgression/level-1-complete.png','Level complete')]
gallery='<!doctype html><meta charset="utf-8"><title>Saved Canvas review</title><style>body{background:#173d3b;color:#ffdfb5;font:16px system-ui;margin:28px}main{display:grid;grid-template-columns:repeat(auto-fit,minmax(220px,1fr));gap:20px}img{width:100%;border-radius:14px}figure{margin:0}figcaption{padding:10px}a{color:inherit}</style><h1>Saved Canvas screens</h1><p>Actual browser captures. Scene layouts are saved in Laundry.unity.</p><main>'
for image,title in shots:gallery+=f'<figure><a href="{image}"><img loading="lazy" src="{image}" alt="{html.escape(title)}"></a><figcaption>{html.escape(title)}</figcaption></figure>'
(qa/'Review.html').write_text(gallery+'</main>',encoding='utf-8')
print(json.dumps(report,indent=2))
