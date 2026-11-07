from pathlib import Path
import json, shutil, zipfile

root=Path(__file__).resolve().parent.parent
review=root/'Documentation/Editor-Screen-Audit'
assert 'PASS:' in (review/'report.txt').read_text()
assert json.loads((review/'Browser/report.json').read_text())['result']=='PASS'
assert json.loads((review/'Browser/blur-check.json').read_text())['result']=='PASS'
sizes={}
for target in ('Web','YouTube'):
    source=root/'.gui-build-workspace/Builds'/target
    destination=root/'Builds/Screen-Audit'/target
    shutil.copytree(source,destination,dirs_exist_ok=True)
    archive=destination.parent/f'SockPairs-{target}.zip'
    with zipfile.ZipFile(archive,'w',zipfile.ZIP_DEFLATED,compresslevel=9) as output:
        for file in sorted(source.rglob('*')):
            if file.is_file():output.write(file,file.relative_to(source))
    with zipfile.ZipFile(archive) as output:
        assert output.testzip() is None
        assert 'index.html' in output.namelist()
    sizes[target]={'files_bytes':sum(p.stat().st_size for p in source.rglob('*') if p.is_file()),'zip_bytes':archive.stat().st_size}
    # Keep the standard delivery paths current; old review folders are not the release entry point.
    shutil.copytree(source,root/'Builds'/target,dirs_exist_ok=True)
    shutil.copy2(archive,root/'Builds'/('Web.zip' if target=='Web' else 'SockPairs-YouTube.zip'))
(review/'build-sizes.json').write_text(json.dumps(sizes,indent=2))
index=(review/'index.html').read_text()
index=index.replace('<h1>Unity editor screen audit</h1>','<h1>Unity editor screen audit</h1><p><a style="color:#ffd886" href="README.md">Fixes and verification</a> · <a style="color:#ffd886" href="Browser/report.json">Browser checks</a></p>')
(review/'index.html').write_text(index)
print(json.dumps(sizes,indent=2))
